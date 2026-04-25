// MenuManager.cs
// Scene: MainMenu
//
// Handles main menu button actions and the continue panel savefile list.
//
// Inspector setup:
//  - mainMenuPanel         : main menu panel GameObject
//  - optionsPanel          : options panel GameObject
//  - resolutionPanel       : resolution panel GameObject
//  - controlPanel          : controls panel GameObject
//  - continuePanel         : continue panel GameObject
//  - saveFileEntryPrefab   : prefab with a SaveFileEntry script
//  - saveFileListContainer : Content transform of the ScrollView
//  - loadGameButton        : Load Game button inside the continue panel
//  - deleteGameButton      : Delte Game button inside the continue panel
//  - backgroundVideoPlayer : VideoPlayer
//  - backgroundMusic       : AudioSource
//
//  - Wire buttons:
//      Back button (some)  -> ShowMainMenu()
//      Options button      -> ShowOptions()
//      Resolution button   -> ShowResolutionPanel()
//      Controls button     -> ShowControlPanel()
//      Continue buton      -> ShowContinuePanel()
//      Load Game button    -> ContinueSavedGame()
//      New Game button     -> LoadNewGame()
//      Delete Game button  -> DeleteSelectedSave()
//      Quit button         -> QuitGame()
//
// Unity / System API References used:
//
// GameObject.SetActive(bool)       — Enables or disables a GameObject.
//
// GameObject.activeSelf            — Returns true if the GameObject is active.
//
// Component.GetComponent<T>()      — Gets a component attached to the GameObject.
//
// Object.Instantiate(...)          — Creates a copy of a prefab in the scene.
//
// Object.Destroy(Object)           — Removes a GameObject from the scene.
//
// Transform                        — Represents position in the scene hierarchy.
//                                    Used here to hold UI list items.
//
// SceneManager.LoadScene(string)   — Loads a scene by name.
//
// Application.Quit()               — Closes the game application.
//
// VideoPlayer.Play()               — Starts playing the video.
//
// AudioSource.Play()               — Starts playing audio.
//
// Button.interactable              — Enables or disables a UI button.
//
// MonoBehaviour.Start()            — Called once when the scene starts.
//
// MonoBehaviour.OnEnable()         — Called when the object becomes active.
//
// MonoBehaviour.OnDisable()        — Called when the object is disabled.
//
// Debug.Log(string)                — Prints a message to the Unity console.
//
// Debug.LogWarning(string)         — Prints a warning message to the console. (yello triangle icon)
//
// Coding Ref:  https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-uxml-element-ScrollView.html
//              https://docs.unity3d.com/2018.2/Documentation/ScriptReference/UI.Selectable-interactable.html      
//              https://community.gamedev.tv/t/nice-way-to-let-the-quit-button-to-work-in-the-editor/13302
//              https://www.youtube.com/watch?v=P0wv-3NgF6c
//              https://discussions.unity.com/t/how-can-i-change-button-width/787606
//              https://discussions.unity.com/t/placing-a-button-on-a-scrollview-programmatically-3d/842127
//              https://www.youtube.com/watch?v=40nrRWex_nI
//
// NOTE TO MYSELF: in future use a UI State machine much better than this implementation!

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Menu Panels")]
    public GameObject mainMenuPanel;
    public GameObject optionsPanel;
    public GameObject resolutionPanel;
    public GameObject controlPanel;

    // Panel that lists the savefiles
    public GameObject continuePanel;

    [Header("<mark=#00008BFF>Continue Panel - Save File List")]
    // Prefab
    public SaveFileEntry saveFileEntryPrefab;

    // ScrollView > Viewport > Content
    // holds the instantiated savefile rows
    public Transform saveFileListContainer;

    // The Load Game & Delete Game button inside the continue panel —> disabled until
    // the player selects a save file from the list
    public Button loadGameButton;
    public Button deleteGameButton;

    // Orignal Video I modified using Filmora
    // Ref: https://discussions.unity.com/t/stunt-car-racer-type-racing-game/490082
    [Header("<mark=#00008BFF>Background Video/Audio References")]
    public VideoPlayer backgroundVideoPlayer;

    // Audio Source must have its Output set to the Music mixer group
    // so the music is controlled by the Music volume slider
    public AudioSource backgroundMusic;

    // -------------------------------------------------------
    // Private
    // -------------------------------------------------------

    // Tracks the saved filename the player has clicked in the list.
    // Null when nothing was selected.
    private string selectedSaveFileName;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    private void Start()
    {
        if (backgroundVideoPlayer != null) backgroundVideoPlayer.Play();
        if (backgroundMusic != null) backgroundMusic.Play();

        ShowMainMenu();
    }

    /// <summary>
    /// MenuManager uses it to store the chosen filename and enable the Continue button.
    /// The highlight/deselect logic lives in SaveFileEntry itself via the same event.
    /// </summary>
    private void OnEnable()
    {
        EventBus.OnSaveFileSelected += OnSaveFileSelected;
    }

    /// <summary>
    /// Unsubscribes from the SaveFileSelected event when deactivated
    /// to prevent calls to a destroyed object.
    /// </summary>
    private void OnDisable()
    {
        EventBus.OnSaveFileSelected -= OnSaveFileSelected;
    }

    // -------------------------------------------------------
    // Private Methods
    // -------------------------------------------------------

    /// <summary>
    /// Closes all panels except the panelToShow which it opens.
    /// Note to myself: Duplicated method change logic pattern to accept an array
    /// of panels and iterate over to close -> add to static utils later
    /// </summary>
    private void SwitchTo(GameObject panelToShow)
    {
        CloseIfActive(mainMenuPanel, panelToShow);
        CloseIfActive(optionsPanel, panelToShow);
        CloseIfActive(resolutionPanel, panelToShow);
        CloseIfActive(controlPanel, panelToShow);
        CloseIfActive(continuePanel, panelToShow);

        if (!panelToShow.activeSelf)
        {
            panelToShow.SetActive(true);
        }
    }

    /// <summary>
    /// Closes a panel with animation unless it's the one we want to keep open.
    /// Note to myself: Duplicated Method -> add this to staic utilties class later
    /// </summary>
    private void CloseIfActive(GameObject panel, GameObject panelToKeepActive)
    {
        if (panel == panelToKeepActive) return;
        if (!panel.activeSelf) return;

        // Get panel coroutine animator
        PanelAnimator animator = panel.GetComponent<PanelAnimator>();

        if (animator != null)
        {
            // trigger PanelAnimator.Close() 
            // Stops coroutine (open) is running
            // and starts panel close animation coroutine
            // At the end of the animation de-activates the panel
            animator.Close();
        }
        else
        {
            // fallback if panel has no animation
            panel.SetActive(false);
        }
    }

    /// <summary>
    /// Clears all child objects from the saved files list container.
    /// Called before repopulating the list so stale entries are removed.
    /// </summary>
    private void ClearSaveFileList()
    {
        foreach (Transform child in saveFileListContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Populates the saved files list with one entry per save found on disk.
    /// Resets selection state and disables the load & delete buttons each time
    /// the panel opens so the player must always make a fresh selection.
    /// </summary>
    private void PopulateSaveFileList()
    {
        ClearSaveFileList();

        // Reset selection and disable load & delete buttons until the player picks a file
        selectedSaveFileName = null;
        loadGameButton.interactable = false;
        deleteGameButton.interactable = false;

        string[] savedNames = FileManager.GetAllSavedNames();

        if (savedNames.Length == 0)
        {
            Debug.Log("MenuManager: no save files found");
            return;
        }

        // Instantiate one row prefab per savefile and configure it.
        // Initialise() adds the Listener (meaning button has no method in inspector)
        // —> the entry fires EventBus.SaveFileSelected() internally when clicked.
        foreach (string filename in savedNames)
        {
            SaveFileEntry entryRow = Instantiate(saveFileEntryPrefab, saveFileListContainer);
            entryRow.ConfigureEntryRow(filename);
        }
    }

    // -------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------

    /// <summary>
    /// Received from EventBus when the player clicks any SaveFileEntry row.
    /// Stores the selected name and enables the Continue button.
    /// Row highlight/deselect is handled inside SaveFileEntry itself via the same event.
    /// </summary>
    private void OnSaveFileSelected(string saveName)
    {
        selectedSaveFileName = saveName;
        loadGameButton.interactable = true;
        deleteGameButton.interactable = true;

        Debug.Log($"MenuManager: selected save file '{saveName}'");
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Shows the main menu panel.
    /// </summary>
    public void ShowMainMenu()
    {
        SwitchTo(mainMenuPanel);
    }

    /// <summary>
    /// Shows the options panel.
    /// </summary>
    public void ShowOptions()
    {
        SwitchTo(optionsPanel);
    }

    /// <summary>
    /// Shows the resolution panel.
    /// </summary>
    public void ShowResolutionPanel()
    {
        SwitchTo(resolutionPanel);
    }

    /// <summary>
    /// Shows the controls panel.
    /// </summary>
    public void ShowControlPanel()
    {
        SwitchTo(controlPanel);
    }

    /// <summary>
    /// Opens the continue panel and refreshes the list of saved files.
    /// Called by the Continue button on the main menu.
    /// </summary>
    public void ShowContinuePanel()
    {
        SwitchTo(continuePanel);
        PopulateSaveFileList();
    }

    /// <summary>
    /// Sets the load flag and save name so PlayerSaveHandler loads
    /// the selected file, then transitions to the game scene.
    /// Called by the load button inside the continue panel.
    /// </summary>
    public void ContinueSavedGame()
    {
        // Guard: a file must be selected before this can be called
        if (string.IsNullOrEmpty(selectedSaveFileName))
        {
            Debug.LogWarning("MenuManager: ContinueSavedGame called with no save selected");
            return;
        }

        // Pass the chosen filename to PlayerSaveHandler via static state
        PlayerSaveHandler.loadSavedGame = true;
        PlayerSaveHandler.loadSaveFileName = selectedSaveFileName;

        // Load the game scene -> PlayerSaveHandler script loads saved game settings at start
        SceneManager.LoadScene("NewGame");
    }

    /// <summary>
    /// Sets the load flag to false so PlayerSaveHandler
    /// skips loading and spawns at the default scene position.
    /// </summary>
    public void LoadNewGame()
    {
        // New game -> no saved file passed
        PlayerSaveHandler.loadSavedGame = false;
        SceneManager.LoadScene("NewGame");
    }

    /// <summary>
    /// Deletes the currently selected saved filename using FileManager,
    /// then refreshes the saved file list UI so the deleted entry disappears.
    /// Called by the Delete button in the Continue panel.
    /// </summary>
    public void DeleteSelectedSave()
    {
        // Safety check -> ensure a file is actually selected
        if (string.IsNullOrEmpty(selectedSaveFileName))
        {
            Debug.LogWarning("MenuManager: No save file selected to delete");
            return;
        }

        // Attempt to delete the save file from disk
        bool deleted = FileManager.DeleteFile(selectedSaveFileName);

        if (deleted)
        {
            Debug.Log($"MenuManager: Deleted save file '{selectedSaveFileName}'");

            // Clear current selection since it no longer exists
            selectedSaveFileName = null;
            loadGameButton.interactable = false;

            // Refresh UI list so the deleted file is removed visually
            PopulateSaveFileList();
        }
        else
        {
            Debug.LogWarning($"MenuManager: Failed to delete save file '{selectedSaveFileName}'");
        }
    }

    /// <summary>
    /// Quits the application. In the editor, stops play mode.
    /// Coding Ref: https://community.gamedev.tv/t/nice-way-to-let-the-quit-button-to-work-in-the-editor/13302
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
