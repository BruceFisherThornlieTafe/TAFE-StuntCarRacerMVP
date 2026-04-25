// FileManager.cs
//
// Handles all file read/write/delete operations for the file system.
//
// Why have I used a Static?
//  Static so it persists in all scenes and accessible from all scripts
//  Does not inherit from MonoBehaviour, as it isn't attached to a GameObject.
//
// Save file location:
//  Application.persistentDataPath + "/<saveName>.json"
//  This resolves to a platform appropriate writable directory:
//  Windows  : %APPDATA%\..\LocalLow\<Company>\<Product>
//  Mac      : ~/Library/Application Support/<Company>/<Product>
//  Linux    : ~/.config/unity3d/<Company>/<Product>
//
// Usage:
//  FileManager.SaveFile(data, "filename");
//  -> write data for given filename to disk
//
//  SaveData data = FileManager.Load("filename");
//  -> reads filename data, returns null if missing
//
//  bool exists   = FileManager.SaveExists("filename");
//  -> check if a filename file exists
//
//  string[] names = FileManager.GetAllSaveNames();
//  -> returns all saved filenames on disk
//
//  FileManager.DeleteFile("filename")
//  -> deletes given filename if it exists
//
// File / System API References used:
//
// Application.persistentDataPath       — Returns the folder path where the game can save data.
//                                        This is different depending on the platform
//                                        (Windows, Mac, etc).
//
// Path.GetFileNameWithoutExtension     — Removes the file extension (like ".json") from a filename.
//
// Path.Combine(string, string)         — Joins folder path and file name into a full valid path.
//
// File.WriteAllText(string, string)    — Writes text to a file.
//                                        Overwrites the file if it already exists.
//
// File.ReadAllText(string)             — Reads all text from a file and returns it as a string.
//
// File.Exists(string)                  — Checks if a file exists at the given path.
//                                        Returns true or false.
//
// File.Delete(string)                  — Deletes a file from disk.
//
// Directory.Exists(string)             — Checks if a folder exists.
//                                        Returns true or false.
//
// Directory.GetFiles(string, string)   — Gets all files in a folder that match a pattern
//                                        (e.g. "*.json").
//
// JsonUtility.ToJson(object, bool)     — Converts an object into a JSON string.
//                                        prettyPrint makes it easier to read.
//
// JsonUtility.FromJson<T>(string)      — Converts a JSON string back into an object.
//
// Enumerable.Select(...)               — Transforms each item in a collection into something else.
//
// Enumerable.ToArray()                 — Converts a collection into an array.
//
// Debug.Log(string)                    — Prints a message to the Unity console.
//
// Debug.LogWarning(string)             — Prints a warning message to the console. (yello triangle icon)
//
// Debug.LogError(string)               — Prints an error message to the console.
//
// Code Ref:    https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
//              https://docs.unity3d.com/ScriptReference/JsonUtility.html
//              https://discussions.unity.com/t/writing-json-files-to-users-system-during-play/928237/2
//              https://www.youtube.com/watch?v=XOjd_qU2Ido
//
// NOTE TO MYSELF:  I would use BinaryFormatter in future as secure see video above.
//                  Even better also encrypt the data.

using UnityEngine;
using System.IO;
using System.Linq;

public static class FileManager
{
    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Builds the full path for a given save name.
    /// Strips any existing .json extension from the input so callers
    /// can pass either "MySave" or "MySave.json" safely.
    /// </summary>
    private static string GetSavePath(string fileName)
    {
        // Strip extension if the caller included it, then re-append
        string baseName = Path.GetFileNameWithoutExtension(fileName);
        return Path.Combine(Application.persistentDataPath, baseName + ".json");
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Serializes the SaveData object to JSON and writes it to the file.
    /// Overwrites any existing file with the same filename.
    /// </summary>
    public static void SaveFile(SaveData data, string fileName)
    {
        // Convert the SaveData object to a formatted JSON string
        string json = JsonUtility.ToJson(data, prettyPrint: true);

        // Write the JSON string to the named save file path
        string path = GetSavePath(fileName);
        File.WriteAllText(path, json);

        Debug.Log($"Game saved to {path}");
    }

    /// <summary>
    /// Reads the saved file and deserializes it into a SaveData object.
    /// Returns null if the file does not exist (caller should handle this case).
    /// </summary>
    public static SaveData Load(string fileName)
    {
        string path = GetSavePath(fileName);

        // Return null if no saved file exists -> caller should check for this null!
        if (!File.Exists(path))
        {
            Debug.Log($"No save file found at {path}");
            return null;
        }

        // Reads the raw JSON string from the file
        string json = File.ReadAllText(path);

        // Deserialize the JSON back into a SaveData object and returns it
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log($"Game loaded from {path}");
        return data;
    }

    /// <summary>
    /// Returns true if a saved file with the given filename exists.
    /// Used by the save panel to warn the player before overwriting.
    /// </summary>
    public static bool SaveExists(string fileName) => File.Exists(GetSavePath(fileName));

    /// <summary>
    /// Scans the data directory and returns the filenames
    /// (without extension) of every .json file found.
    /// Used by the continue panel in the main menu to populate the save list.
    /// </summary>
    public static string[] GetAllSavedNames()
    {
        string dir = Application.persistentDataPath;

        // Return an empty array if the directory doesn't exist yet
        if (!Directory.Exists(dir)) return new string[0];

        // Find all .json files and return their names without the extension
        return Directory.GetFiles(dir, "*.json")
                        .Select(Path.GetFileNameWithoutExtension)
                        .ToArray();
    }

    /// <summary>
    /// Deletes a saved filename.
    /// </summary>
    public static bool DeleteFile(string fileName)
    {
        // Build the full file path for the requested save file
        string path = GetSavePath(fileName);

        // Check if the file actually exists before attempting deletion
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Cannot delete save. File not found at {path}");
            return false;
        }

        try
        {
            // Attempt to delete the file from disk
            File.Delete(path);

            // Confirm successful deletion for debugging purposes
            Debug.Log($"Deleted save file at {path}");

            return true;
        }
        catch (System.Exception e)
        {
            // Catch any system-level errors (permissions, file locks, IO issues)
            // and log them without crashing the game
            Debug.LogError($"Failed to delete save file at {path}: {e.Message}");

            return false;
        }
    }
}
