// PauseMenuController.cs
// Scene: NewGame
//
// Controls the pause menu UI panel and the save panel.
//
// Inspector setup:
//  - gameManager           : GameManager GameObject
//  - playerSaveHandler     : PlayerSaveHandler from the player GameObject
//  - pauseMenuPanel        : pause menu root panel GameObject
//  - saveGamePanel         : save menu sub panel GameObject
//  - savePanelController   : SavePanelController component
//
//  - Wire buttons:
//      Resume button    -> OnResumePressed()
//      Save button      -> OnSavePressed()
//      Main Menu button -> OnMainMenuPressed()
//
// Unity API References used:
//
// GameObject.SetActive(bool)       — Enables or disables a GameObject.
//
// GameObject.activeSelf            — Returns true if the GameObject is active.
//
// Component.GetComponent<T>()      — Gets a component attached to the GameObject.
//
// SceneManager.LoadScene(string)   — Loads a scene by name.
//
// Time.timeScale                   — Controls game time.
//                                    0 = paused, 1 = normal speed.
//
// Cursor.lockState                 — Controls how the cursor is locked.
//
// Cursor.visible                   — Shows or hides the mouse cursor.
//
// CursorLockMode.None              — Frees the cursor for normal use.
//
// MonoBehaviour.Start()            — Called once when the scene starts.
//
// MonoBehaviour.OnEnable()         — Called when the object becomes active.
//
// MonoBehaviour.OnDisable()        — Called when the object is disabled.
//
// Debug.Log(string)                — Prints a message to the console.
//
// NOTE TO MYSELF: in future use a UI State machine much better than this implementation!

using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Class References")]
    public GameManager gameManager;
    public PlayerSaveHandler playerSaveHandler;

    [Header("<mark=#00008BFF>Menu Panels")]
    public GameObject pauseMenuPanel;
    public GameObject saveGamePanel;

    // Reference to the save panel controller that handles the
    // save file name input flow
    [Header("<mark=#00008BFF>Save Panel")]
    public SavePanelController savePanelController;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Hides pause menu panel start
    /// </summary>
    private void Start()
    {
        pauseMenuPanel.SetActive(false);
    }

    /// <summary>
    /// Subscribes to pause and resume events when this object becomes active.
    /// </summary>
    private void OnEnable()
    {
        EventBus.OnPause += ShowMenu;
        EventBus.OnResume += HideMenu;
    }

    /// <summary>
    /// Unsubscribes from pause and resume events when deactivated
    /// to prevent calls to a destroyed object.
    /// </summary>
    private void OnDisable()
    {
        EventBus.OnPause -= ShowMenu;
        EventBus.OnResume -= HideMenu;
    }

    // -------------------------------------------------------
    // Panel Switching
    // -------------------------------------------------------

    /// <summary>
    /// Closes all panels except the target and opens it.
    /// Note to myself: Duplicated method change logic pattern to accept an array
    /// of panels and iterate over to close -> add to static utils later
    /// </summary>
    private void SwitchTo(GameObject panelToShow)
    {
        CloseIfActive(pauseMenuPanel, panelToShow);
        CloseIfActive(saveGamePanel, panelToShow);

        if (!panelToShow.activeSelf)
        {
            panelToShow.SetActive(true);
        }
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Closes a panel with animation unless it's the one we want to keep open.
    /// Note to myself: Duplicated Method -> add this to static utilties class later
    /// </summary>
    private void CloseIfActive(GameObject panel, GameObject panelToKeepActive)
    {
        if (panel == panelToKeepActive) return;
        if (!panel.activeSelf) return;

        // Get panel coroutine animator
        PanelAnimator animator = panel.GetComponent<PanelAnimator>();

        if (animator != null)
        {
            // Play the close animation then deactivate the panel
            animator.Close();
        }
        else
        {
            // fallback if panel has no animation.
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Force focus to input field so player doesn't need to click into it.
    /// </summary>
    private void FocusOnTextInputField()
    {
        savePanelController.fileNameInput.ActivateInputField();
        savePanelController.fileNameInput.Select();
    }

    // -------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------

    /// <summary>
    /// Shows the pause menu panel when the pause event is fired.
    /// </summary>
    private void ShowMenu() => SwitchTo(pauseMenuPanel);

    /// <summary>
    /// Close the pause menu panel with its close animation.
    /// </summary>
    private void HideMenu() => CloseIfActive(pauseMenuPanel, saveGamePanel);

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Resumes the game when the resume button is pressed.
    /// </summary>
    public void OnResumePressed()
    {
        gameManager.Unpause();
    }

    /// <summary>
    /// Switches to the save panel so the player can enter a file name before saving.
    /// Uses SwitchTo so PanelAnimator transitions play on both panels.
    /// </summary>
    public void OnSavePressed()
    {
        SwitchTo(saveGamePanel);

        // Tell the save panel to return here when the player presses Back.
        savePanelController.Open(playerSaveHandler, OnSavePanelBack);

        FocusOnTextInputField();
    }

    /// <summary>
    /// Called by SavePanelController when the player presses Back.
    /// Switches back to the pause menu so PanelAnimator transitions play on both panels.
    /// </summary>
    public void OnSavePanelBack()
    {
        SwitchTo(pauseMenuPanel);
    }

    /// <summary>
    /// Restores time and cursor state then loads the main menu scene.
    /// </summary>
    public void OnMainMenuPressed()
    {
        // Restore time and cursor directly
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Load game MainMenu
        SceneManager.LoadScene("MainMenu");
    }
}