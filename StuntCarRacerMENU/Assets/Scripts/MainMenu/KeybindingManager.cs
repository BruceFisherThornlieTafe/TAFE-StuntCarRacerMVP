// KeybindingManager.cs
// Scene: MainMenu
//
// Handles saving, loading and resetting player keybinding overrides.
//
// JSON Filename: keybindings.json
//
// Usage:
//   Save   - call KeybindingManager.SaveBindings()
//   Load   - happens automatically on Awake
//   Reset  - call KeybindingManager.ResetAllBindings()
//
// Inspector setup:
//  - inputActionAsset : Input Action Asset from the project
//
// Unity / System API References used:
//
// Application.persistentDataPath   — Returns the folder path where the game can save data.
//
// Path.Combine(string, string)     — Joins folder path and file name into a full path.
//
// File.WriteAllText(string, string)— Writes text to a file.
//                                    Overwrites the file if it already exists.
//
// File.ReadAllText(string)         — Reads all text from a file and returns it as a string.
//
// File.Exists(string)              — Checks if a file exists.
//                                    Returns true or false.
//
// File.Delete(string)              — Deletes a file from disk.
//
// InputActionAsset.SaveBindingOverridesAsJson()
//                                  — Converts all keybinding overrides into a JSON string.
//
// InputActionAsset.LoadBindingOverridesFromJson(string)
//                                  — Loads keybinding overrides from a JSON string.
//
// InputActionAsset.RemoveAllBindingOverrides()
//                                  — Removes all custom keybindings and restores defaults.
//
// MonoBehaviour.Awake()            — Called when the object is first created.
//
// Debug.Log(string)                — Prints a message to the Unity console.
//
// Coding Ref:  https://www.youtube.com/watch?v=OMVMqFZV03M&t=319s
//              See Unity Sample script RebindActionUI.cs
//              https://discussions.unity.com/t/rebinding-excluding-certain-keys/877703


using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class KeybindingManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------
    [Header("<mark=#00008BFF>Input Actions - Player controller")]
    public InputActionAsset inputActionAsset;

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    // Resolves the save path at runtime —> Please Note: persistentDataPath varies per platform
    private string SavePath => Path.Combine(Application.persistentDataPath, "keybindings.json");

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Applies saved binding overrides -> before any scene input runs.
    /// </summary>
    private void Awake()
    {
        LoadBindings();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Serialises all current binding overrides to JSON and writes them to file.
    /// Call this whenever the player confirms a rebind.
    /// </summary>
    public void SaveBindings()
    {
        string json = inputActionAsset.SaveBindingOverridesAsJson();
        File.WriteAllText(SavePath, json);

        Debug.Log($"Keybindings saved to: {SavePath}");
    }

    /// <summary>
    /// Reads binding overrides from file and applies them to the action asset.
    /// Called automatically on Awake —> can also be called manually if needed.
    /// Does nothing if no save file exists.
    /// </summary>
    public void LoadBindings()
    {
        if (!File.Exists(SavePath)) return;

        string json = File.ReadAllText(SavePath);
        inputActionAsset.LoadBindingOverridesFromJson(json);

        Debug.Log("Keybindings loaded.");
    }

    /// <summary>
    /// Removes all binding overrides and deletes the save file,
    /// returning every action to its default binding.
    /// </summary>
    public void ResetAllBindings()
    {
        inputActionAsset.RemoveAllBindingOverrides();

        // Remove the save file so defaults are used on next load
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
        }

        Debug.Log("All keybindings reset to default.");
    }
}