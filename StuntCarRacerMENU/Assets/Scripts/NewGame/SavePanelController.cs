// SavePanelController.cs
// Scene: NewGame
//
// Controls the save panel UI —> lets the player type a file name,
// warns them if the file already exists and either saves or returns
// to the pause menu.
//
// Inspector setup:
//  - savePanelRoot  : Save panel root GameObject
//  - fileNameInput  : InputField where the player types a name
//  - warningText    : Text used for the "file exists" etc warnings
//
//  - Wire buttons:
//      Back button         -> OnBackPressed()
//      Options button      -> ShowOptions()
//      Save Game button    -> OnSavePressed()
//
// Unity / UI / System API References used:
//
// GameObject.SetActive(bool)       — Enables or disables a GameObject.
//
// TMP_InputField.text              — Gets or sets the text inside the input field.
//
// TMP_InputField.ActivateInputField()
//                                  — Focuses the input field so the player can type.
//
// TMP_InputField.Select()          — Selects the input field for keyboard input.
//
// TMP_Text.text                    — Sets the displayed UI text.
//
// MonoBehaviour.Start()            — Called when the scene starts.
//
// MonoBehaviour.OnEnable()         — Called when the object becomes active.
//
// MonoBehaviour.OnDisable()        — Called when the object becomes inactive.
//
// string.IsNullOrWhiteSpace(...)   — Checks if a string is empty, null, or only spaces.
//
// string.Trim()                    — Removes leading and trailing whitespace.
//
// Debug.Log(string)                — Prints a message to the Unity console.
//
// Action (System)                  — Delegate used for callback methods.
//
// Null-conditional operator (?.Invoke)
//                                  — Safely calls a method only if it is not null.
//
// FileManager.SaveExists(...)      — Checks if a save filename already exists.
//
// PlayerSaveHandler.Save(...)      — Saves player data to save filename.

using UnityEngine;
using TMPro;
using System;

public class SavePanelController : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Save Panel UI")]
    public GameObject savePanelRoot;

    public TMP_InputField fileNameInput;

    // Text that displays file warnings.
    public TMP_Text warningText;

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    // Set by Open() —> used to trigger the actual save.
    private PlayerSaveHandler playerSaveHandler;

    // Set by Open() —> called when Back is pressed to return to the pause menu.
    private Action onBackCallback;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Ensures the save panel is hidden when the scene first loads.
    /// </summary>
    private void Start()
    {
        savePanelRoot.SetActive(false);
    }

    // Subscribe to all EventBus events when this object becomes active.
    private void OnEnable()
    {
        EventBus.OnSavePanelEscape += OnBackPressed;
    }

    // Unsubscribe when deactivated to prevent calls to a destroyed object
    private void OnDisable()
    {
        EventBus.OnSavePanelEscape -= OnBackPressed;
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>
    /// 
    /// </summary>
    private void FocusOnTextInputField()
    {
        // Force focus to input field so player doesn't need to click into it.
        fileNameInput.ActivateInputField();
        fileNameInput.Select();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Opens the save panel ready for the player to type a filename.
    /// Called by PauseMenuController when the player presses Save.
    /// </summary>
    public void Open(PlayerSaveHandler handler, Action backCallback)
    {
        playerSaveHandler = handler;
        onBackCallback = backCallback;

        // Clear any leftover input and warning from a previous open.
        fileNameInput.text = string.Empty;
        warningText.text = string.Empty;

        savePanelRoot.SetActive(true);
    }

    /// <summary>
    /// Fires every time the player types in the filename input field.
    /// Shows a warning if a saved filename already exists on disk.
    /// </summary>
    public void OnFileNameChanged(string input)
    {
        // Only show the warning when there is something to check
        if (string.IsNullOrWhiteSpace(input))
        {
            warningText.text = string.Empty;
            return;
        }

        // Warn the player if a file with this name is already on disk
        if (FileManager.SaveExists(input.Trim()))
        {
            warningText.text = "A save with that name already exists.\n Saving will overwrite it.";
        }
        else
        {
            warningText.text = string.Empty;
        }
    }

    /// <summary>
    /// Called by the Save button. Saves the game using the typed filename.
    /// Does nothing if the input field is empty.
    /// </summary>
    public void OnSavePressed()
    {
        string saveFileName = fileNameInput.text.Trim();

        FocusOnTextInputField();

        // Guard against an empty filename.
        if (string.IsNullOrEmpty(saveFileName))
        {
            warningText.text = "Please enter a save file name.";
            return;
        }

        // Call the PlayerSaveHandler to do the actual save.
        playerSaveHandler.Save(saveFileName);

        // Clear any leftover input.
        fileNameInput.text = string.Empty;

        // Confirm saving success in the warning text field.
        warningText.text = $"Saved as '{saveFileName}'.";

        Debug.Log($"SavePanelController: saved game as '{saveFileName}'");
    }

    /// <summary>
    /// Called by the Back button. Closes the save panel and returns to the pause menu.
    /// </summary>
    public void OnBackPressed()
    {
        savePanelRoot.SetActive(false);

        // Tell PauseMenuController to re-show the pause menu.
        onBackCallback?.Invoke();
    }
}
