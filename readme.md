# VR Tower Defense Game

This project is a VR-based tower defense game that combines immersive gesture recognition with gameplay. Players can interact with the game world using VR controllers to draw gestures, spawn guards, and defend their base from waves of enemies.

## Features

- **Gesture Recognition**: Use VR controllers to draw gestures that trigger in-game actions.
- **Tower Defense Mechanics**: Defend your base by spawning guards and managing resources.
- **Dynamic Drawing**: Create 3D drawings in real-time using tube-like visualizations.
- **Wave-Based Enemies**: Face increasingly challenging waves of enemies.

## Key Components

- **Gesture Recognition**:

  - `VRGestureRecorder`: Records gestures and renders them onto a texture for classification.
  - `GestureSynthGrayClassifier`: Classifies gestures using a pre-trained ONNX model.

- **Game Framework**:

  - `GameManager`: Manages game state, including game over logic and cooldowns.
  - `EnemySpawner` & `GuardSpawner`: Handle spawning and removal of enemies and guards.
  - `BaseHealth` & `HealthBar`: Manage health systems for the base and UI.

- **Drawing Mechanics**:
  - `XRControllerDraw`: Enables drawing in 3D space using a VR controller.
  - `TubeRenderer`: Generates tube-like meshes for visualizing drawings.

## Training Data

Gesture training data is located in the `Python_scripts/data/` directory, organized by gesture type (e.g., `circle`, `loop`).
