// PlayerSaveHandler.cs
// Scene: NewGame
//
// Handles saving and loading player position and rotation.
// The static loadSavedGame flag is set by MenuManager before
// the scene loads so Start() knows which path to take.
// The static pendingSaveName holds the name of the file to load
// and is set alongside loadSavedGame by MenuManager.
//
// Inspector setup:
//  - No references needed —> Rigidbody is fetched automatically
//  - Assign this script to the player GameObject
//
// Unity / Physics / System API References used:
//
// Rigidbody                — Physics component that controls movement and rotation.
//
// Rigidbody.position       — Current physics position of the object.
//
// Rigidbody.rotation       — Current physics rotation of the object.
//
// Rigidbody.linearVelocity — Current movement velocity.
//
// Rigidbody.angularVelocity— Current rotational velocity.
//
// RigidbodyConstraints     — Used to lock movement or rotation axes.
//
// Transform.position       — Scene position (can be out of sync with physics).
//
// Quaternion.Euler(...)    — Creates a rotation from X/Y/Z angles.
//
// Vector3                  — Stores 3D position values (x, y, z).
//
// MonoBehaviour.Awake()    — Called when script is loaded.
//
// MonoBehaviour.Start()    — Called before first frame update.
//
// Debug.Log(string)        — Prints a message to console.
//
// Debug.LogWarning(string) — Prints a warning message to console.
//
// static variables         — Shared values across scenes (used here for save/load flags).
//
// FileManager.SaveFile(...)— Saves data object to disk as JSON.
//
// FileManager.Load(...)    — Loads saved JSON data from disk.
//
// string.IsNullOrEmpty(...)— Checks if a string is null or empty.

using UnityEngine;

public class PlayerSaveHandler : MonoBehaviour
{
    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    private Rigidbody rb;

    // -------------------------------------------------------
    // Static - Load Saved Game Flag and Saved File Name
    // -------------------------------------------------------

    // Static flag set by MenuManager before the scene loads.
    // Not initialised here so scene reloads do not reset the value.
    public static bool loadSavedGame;

    // Static name of the save file to load, set by MenuManager
    // alongside loadSavedGame so LoadPosition() knows which file to read.
    public static string loadSaveFileName;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Caches the Rigidbody on the same GameObject.
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    /// <summary>
    /// Loads the saved position if the flag was set by MenuManager.
    /// Resets both the flag and the pending save name after reading
    /// so subsequent scene loads default to new game behaviour.
    /// </summary>
    private void Start()
    {
        // Load a game or else its a New game.
        if (loadSavedGame)
        {
            LoadPosition(loadSaveFileName);
        }

        // Reset static state so a new game scene reload starts fresh
        loadSavedGame = false;
        loadSaveFileName = null;
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Writes the current position and rotation to a filename via SaveManager.
    /// Reads from rb.position rather than transform.position —>
    /// the Rigidbody owns position when driven by linearVelocity,
    /// transform.position can be stale and return the spawn point.
    /// Code Ref: https://discussions.unity.com/t/how-to-properly-set-initial-position-of-rigidbody/924264/22?page=2
    /// </summary>
    public void Save(string saveName)
    {
        SaveData data = new SaveData();
     
        // Get player current position
        data.positionX = rb.position.x;
        data.positionY = rb.position.y;
        data.positionZ = rb.position.z;
        data.rotationY = rb.rotation.eulerAngles.y;

        FileManager.SaveFile(data, saveName);
        Debug.Log($"Player saved to '{saveName}' at position: {rb.position} rotation: {rb.rotation.eulerAngles.y}");
    }

    // -------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Reads from the saved filename and repositions the car to the saved
    /// coordinates and rotation. Sets position through both the
    /// Rigidbody and transform to ensure physics and scene view
    /// both reflect the correct position immediately.
    /// Does nothing if no save file exists.
    /// </summary>
    private void LoadPosition(string saveName)
    {
        // Guard against a missing or empty save name
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogWarning("LoadPosition: no save name provided");
            return;
        }

        // get saved file data
        SaveData data = FileManager.Load(saveName);

        if (data == null)
        {
            Debug.Log($"LoadPosition: no save data found for '{saveName}'");
            return;
        }

        // Set player current position data from saved position data
        Vector3 savedPosition = new Vector3(data.positionX, data.positionY, data.positionZ);
        Quaternion savedRotation = Quaternion.Euler(0f, data.rotationY, 0f);

        // Set Rigidbody position and rotation.
        // The Transform will be updated by the physics system on the next FixedUpdate.
        rb.position = savedPosition;
        rb.rotation = savedRotation;

        // Load car with no velocity
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Debug.Log($"Player loaded from '{saveName}' at position: {savedPosition} rotation: {data.rotationY}");
    }
}
