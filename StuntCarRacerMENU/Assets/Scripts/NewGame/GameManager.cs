// GameManager.cs
// Scene: NewGame
//
// Central game manager. Handles pause state and communicates
// with the PauseMenuController via EventBus.
//
// Inspector setup:
//   - pauseAction : UI/Cancel InputActionReference from the Input Action Asset
//
// Unity API References used:
//
// InputActionReference.action      — Gets the InputAction linked to this reference.
//
// InputAction.Enable()             — Enables the input so it can detect player input.
//
// InputAction.Disable()            — Disables the input so it stops detecting input.
//
// InputAction.performed            — Event that is called when the input is triggered.
//
// InputAction.CallbackContext      — Provides information about the input event.
//
// MonoBehaviour.OnEnable()         — Called when the object becomes active.
//
// MonoBehaviour.OnDisable()        — Called when the object is disabled or destroyed.
//
// MonoBehaviour.Start()            — Called once when the game starts.
//
// MonoBehaviour.OnApplicationFocus(bool)
//                                  — Called when the game window gains or loses focus.
//
// Time.timeScale                   — Controls game time.
//                                    0 = paused, 1 = normal speed.
//
// Cursor.lockState                 — Controls how the cursor is locked to the screen.
//
// Cursor.visible                   — Shows or hides the mouse cursor.
//
// CursorLockMode.Locked            — Locks the cursor to the centre of the screen.
//
// CursorLockMode.None              — Frees the cursor so it can move normally.
//
// NOTE TO MYSELF: in future use a UI State machine much better than this implementation!

using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Input Actions - UI/Cancel for ESC")]
    public InputActionReference pauseAction;

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    private bool isPaused = false;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Locks and hides the cursor when the game begins.
    /// </summary>
    private void Start()
    {
        HideCursor();
    }

    /// <summary>
    /// Subscribes to pause input when this object becomes active.
    /// </summary>
    private void OnEnable()
    {
        pauseAction.action.Enable();
        pauseAction.action.performed += OnPausePerformed;
    }

    /// <summary>
    /// Unsubscribes from pause input when deactivated to prevent calls to a destroyed object.
    /// </summary>
    private void OnDisable()
    {
        pauseAction.action.performed -= OnPausePerformed;
        pauseAction.action.Disable();
    }

    /// <summary>
    /// Restores cursor lock when the game window regains focus, unless the game is paused.
    /// Unity automativally calls this when the game window gains or loses focus.
    /// Fixes alt-tabbing, clicking another app, even the Windows taskbar.
    /// </summary>
    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !isPaused)
        {
            HideCursor();
        }
    }

    // -------------------------------------------------------
    // Input Handlers
    // -------------------------------------------------------

    /// <summary>
    /// Called when the player presses ESC.
    /// </summary>
    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        // Always give SavePanel first chance to handle ESC
        // So player can quickly save and return to gameplay.
        EventBus.SavePanelEscape();

        // Close the pause menu (Save menu has control).
        TogglePause();
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Toggles between paused and unpaused states.
    /// </summary>
    public void TogglePause()
    {
        if (isPaused)
        {
            Unpause();
        }
        else
        {
            Pause();
        }
    }

    /// <summary>
    /// Freezes time, shows the cursor and fires the Pause event.
    /// Does nothing if the game is already paused.
    /// </summary>
    public void Pause()
    {
        if (isPaused) return;

        isPaused = true;
        Time.timeScale = 0f;
        ShowCursor();
        EventBus.Pause();
    }

    /// <summary>
    /// Restores time, hides the cursor and fires the Resume event.
    /// Does nothing if the game is not currently paused.
    /// </summary>
    public void Unpause()
    {
        if (!isPaused) return;

        isPaused = false;
        Time.timeScale = 1f;
        HideCursor();
        EventBus.Resume();
    }

    /// <summary>
    /// Returns true if the game is currently paused.
    /// </summary>
    public bool IsPaused() => isPaused;

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Locks the cursor to the centre of the screen and makes it invisible.
    /// </summary>
    void HideCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Releases the cursor and makes it visible for UI interaction.
    /// </summary>
    void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}