#!/usr/bin/env python3
# -*- coding: utf-8 -*-

"""
This script trains a convolutional neural network (CNN) for gesture classification using TensorFlow and Keras.

Modules:
- os, cv2, numpy: For file handling, image processing, and numerical computations.
- tensorflow: For building and training the CNN model.
- sklearn: For splitting the dataset and computing class weights.

Constants:
- TARGET: Target image size (216x216).
- DATA_DIR: Directory containing the dataset.
- CLASS_NAMES: List of gesture class names.
- THRESH: Threshold value (unused in this script).

Functions:
- preprocess(path):
    Preprocesses an image by resizing, normalizing, and converting it to grayscale.
    Args:
        path (str): Path to the image file.
    Returns:
        np.ndarray: Preprocessed image or None if the image is invalid.

- load_all():
    Loads and preprocesses all images from the dataset.
    Returns:
        tuple: Arrays of images (X) and their corresponding labels (y).

Workflow:
1. Load and preprocess the dataset.
2. Split the dataset into training and validation sets.
3. Compute class weights to handle class imbalance.
4. Define a data augmentation pipeline.
5. Build a CNN model using MobileNetV2 as the base.
6. Train the model's head with frozen base layers.
7. Fine-tune the last 20% of the base layers.
8. Save the trained model in Keras and ONNX formats.

Outputs:
- best_cnn.keras: Trained Keras model.
- gesture_cnn.onnx: Exported ONNX model.

Dependencies:
- TensorFlow
- OpenCV
- NumPy
- scikit-learn
- tf2onnx

Usage:
Run the script to train the model and save the outputs. Ensure the dataset is organized in subdirectories under DATA_DIR, with each subdirectory named after a class in CLASS_NAMES.
"""

import os, cv2, numpy as np, tensorflow as tf
from glob import glob
from sklearn.model_selection import train_test_split
from sklearn.utils.class_weight import compute_class_weight
from tensorflow.keras import layers, models, callbacks, optimizers

# -------- STAŁE --------
TARGET      = 216
DATA_DIR    = "./data"
CLASS_NAMES = ["circle","loop","s", "spiral", "w"]
THRESH      = 25

def preprocess(path):
    """
    Preprocesses an image by resizing, normalizing, and converting it to grayscale.

    Args:
        path (str): Path to the image file.

    Returns:
        np.ndarray: Preprocessed image or None if the image is invalid.
    """
    bgr = cv2.imread(path, cv2.IMREAD_COLOR)
    if bgr is None or bgr.shape[0] != TARGET or bgr.shape[1] != TARGET:
        print(f"[WARN] Skipping {path}: shape {None if bgr is None else bgr.shape}, expected ({TARGET},{TARGET},3)")
        return None
    g   = bgr[:,:,1]
    crop = g 
    rgb = np.stack([crop,crop,crop],-1).astype("float32")/255.0
    return rgb

def load_all():
    """
    Loads and preprocesses all images from the dataset.

    Returns:
        tuple: Arrays of images (X) and their corresponding labels (y).
    """
    X,y = [],[]
    for i,cls in enumerate(CLASS_NAMES):
        for fp in glob(os.path.join(DATA_DIR,cls,"*.png")):
            img = preprocess(fp)
            if img is not None:
                X.append(img); y.append(i)

    X = np.array(X); y = np.array(y)
    return X,y

print("Loading data…")
X,y = load_all()
print(f"✓ Loaded {len(X)} samples:", {cls:int((y==i).sum()) for i,cls in enumerate(CLASS_NAMES)})

# -------- TRAIN/VAL SPLIT --------
X_tr, X_val, y_tr, y_val = train_test_split(
    X,y, test_size=0.2, stratify=y, random_state=42)

# -------- CLASS WEIGHTS --------
cw = compute_class_weight("balanced", classes=np.arange(len(CLASS_NAMES)), y=y)
class_weights = {i:w for i,w in enumerate(cw)}
print("class_weights:", class_weights)

# -------- DATA AUGMENTATION --------
augment = models.Sequential([
    layers.RandomRotation(0.1)
])

# -------- MODEL --------
base = tf.keras.applications.MobileNetV2(
    input_shape=(TARGET,TARGET,3),
    include_top=False, weights="imagenet", alpha=0.5)
base.trainable = False

inp = layers.Input((TARGET,TARGET,3))
x   = augment(inp)
x   = tf.keras.applications.mobilenet_v2.preprocess_input(x*255.)
x   = base(x, training=False)
x   = layers.GlobalAveragePooling2D()(x)
x   = layers.Dropout(0.4)(x)
out = layers.Dense(len(CLASS_NAMES), activation="softmax")(x)

model = models.Model(inp,out)
model.compile(optimizers.Adam(1e-3),
              loss="sparse_categorical_crossentropy",
              metrics=["accuracy"])

model.summary()

cbs = [
    callbacks.EarlyStopping(patience=5, restore_best_weights=True),
    callbacks.ReduceLROnPlateau(factor=0.3, patience=3, verbose=1)
]

# -------- TRAIN HEAD --------
print("\n Training head…")
model.fit(X_tr, y_tr,
          validation_data=(X_val,y_val),
          epochs=10, batch_size=8,
          class_weight=class_weights,
          callbacks=cbs, verbose=2)

# -------- FINE-TUNE --------
print("\n Fine-tuning last 20% layers…")
n = len(base.layers)
for layer in base.layers[int(0.8*n):]:
    layer.trainable = True

model.compile(optimizers.Adam(1e-4),
              loss="sparse_categorical_crossentropy",
              metrics=["accuracy"])
model.fit(X_tr, y_tr,
          validation_data=(X_val,y_val),
          epochs=30, batch_size=8,
          class_weight=class_weights,
          callbacks=cbs, verbose=2)

# -------- SAVE & EXPORT --------
model.save("best_cnn.keras")
print("Saved best_cnn.keras")

import tf2onnx
spec = (tf.TensorSpec((None,TARGET,TARGET,3),tf.float32,name="input"),)
tf2onnx.convert.from_keras(model,input_signature=spec,
                           output_path="gesture_cnn.onnx",opset=13)
print("Exported gesture_cnn.onnx")
