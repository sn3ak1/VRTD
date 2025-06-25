using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.UI;

public class DrawingSelector : MonoBehaviour
{
    private RawImage imageDisplay; // UI element to show the next drawing
    public TextMeshProUGUI resultText; // UI element to show the result of the drawing selection
    private string currentDrawing;
    // Start is called before the first frame update
    void Start()
    {
        imageDisplay = GetComponent<RawImage>();
        if (imageDisplay == null)
        {
            Debug.LogError("NextDrawing script requires a RawImage component on the same GameObject.");
            return;
        }
        LoadNext(); // Load the first drawing when the script starts
    }

    public string CurrentDrawing
    {
        get { return currentDrawing; }
    }

    public void NextDrawing(bool prev)
    {
        if (prev)
        {
            resultText.text = "Nice :)";
        }
        else
        {
            resultText.text = "Try again!";
        }
        LoadNext();
    }

    private void LoadNext()
    {
        // Load all PNG files from the "Drawing" folder in the Resources directory
        Object[] drawings = Resources.LoadAll("Drawings", typeof(Texture2D));
        Debug.Log("Loaded " + drawings.Length + " drawings from the Resources directory.");
        if (drawings.Length == 0)
        {
            Debug.LogWarning("No drawings found in the Drawing folder within the Resources directory.");
            return;
        }

        // Filter out the current drawing
        List<Object> filteredDrawings = new List<Object>(drawings);
        filteredDrawings.RemoveAll(d => d.name == currentDrawing);

        if (filteredDrawings.Count == 0)
        {
            Debug.LogWarning("No new drawings available to select.");
            return;
        }

        // Select a random drawing from the filtered list
        int randomIndex = Random.Range(0, filteredDrawings.Count);
        Texture2D texture = filteredDrawings[randomIndex] as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Failed to load texture from the Drawing folder in Resources.");
            return;
        }
        Debug.Log("Selected drawing: " + texture.name);

        // Set the texture to the RawImage component
        currentDrawing = texture.name; // Store the name of the drawing
        imageDisplay.texture = texture;
    }


}
