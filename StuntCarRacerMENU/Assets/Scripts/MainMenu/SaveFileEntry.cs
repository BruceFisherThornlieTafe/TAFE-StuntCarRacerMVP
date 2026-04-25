// SaveFileEntry.cs
// Scene: MainMenu -> Prefab SaveFileEntry
//
// Attached to the save file row prefab used inside the continue panel.
// Each instance represents one row for each found saved filename
// to display in the scroll content list.
//
// When clicked, the entry fires EventBus.SaveFileSelected() with its save filename.
// MenuManager listens on that event to handle deselection, highlighting
// and enabling the Load/Delete button.
//
// Inspector / Prefab setup:
//  - entryButton       : Button on this prefab
//  - entryLabel        : Text that displays the save file name
//  - backgroundImage   : Image component of "SaveFileEntry" Button which is
//                        used for row selection tinting
//  - normalColor       : colour used when the entry is not selected
//  - selectedColor     : colour used when the entry is selected
//
//   Note: The Button's OnClick should NOT be wired in the Inspector as the method
//   Initialise() adds the listener at runtime so each entry knows its own name.
//
// Unity / UI / Event System API References used:
//
// Button.onClick.AddListener(...)  — Adds a method to be called when the button is clicked.
//
// Image.color                      — Sets the UI image colour.
//
// TMP_Text.text                    — Sets the displayed text in a TextMeshPro element.
//
// GameObject.OnEnable()            — Called when the object becomes active.
//
// GameObject.OnDisable()           — Called when the object becomes inactive.
//
// MonoBehaviour.GetComponent<T>()  — Gets a component attached to the same GameObject
//
// UnityEvent (Button.onClick)      — Event system used for UI button clicks.
//
// Coding Ref:  https://docs.unity3d.com/6000.3/Documentation/Manual/UIE-uxml-element-ScrollView.html
//              https://docs.unity3d.com/2018.2/Documentation/ScriptReference/UI.Selectable-interactable.html      
//              https://community.gamedev.tv/t/nice-way-to-let-the-quit-button-to-work-in-the-editor/13302
//              https://www.youtube.com/watch?v=P0wv-3NgF6c
//              https://discussions.unity.com/t/how-can-i-change-button-width/787606
//              https://discussions.unity.com/t/placing-a-button-on-a-scrollview-programmatically-3d/842127

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveFileEntry : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Entry UI")]
    // Button the player clicks to select this save file.
    public Button entryButton;

    // Label that shows the save filename.
    public TMP_Text entryLabel;

    // Background image of the entry row —> its colour changes on selection.
    // The "SaveFileEntry" Button used for each filename listed for Player to select.
    public Image backgroundImage;

    [Header("<mark=#00008BFF>Selection Colours")]
    // Colour when this entry is not selected (backgroundImage).
    public Color normalColor;

    // Colour when this entry has been selected by the player (backgroundImage).
    public Color selectedColor;

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    // The save filename this entry represents.
    private string saveFileName;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Subscribes to the SaveFileSelected event so this entry can
    /// update its own highlight whenever any row is clicked.
    /// </summary>
    private void OnEnable()
    {
        EventBus.OnSaveFileSelected += OnAnyEntrySelected;
    }

    /// <summary>
    /// Unsubscribes from the SaveFileSelected event when this entry
    /// is disabled or destroyed to prevent calls to a dead object.
    /// </summary>
    private void OnDisable()
    {
        EventBus.OnSaveFileSelected -= OnAnyEntrySelected;
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Configures this Entry row with the saved filename it represents.
    /// Called by MenuManager immediately after Instantiate().
    /// Wires the button click in code -> so each entry captures its own saveFileName.
    /// Basically sets the row -> Set the name text and reset the selected look.
    /// Then it "wires up" the button so it knows to run the OnClicked method.
    /// </summary>
    public void ConfigureEntryRow(string name)
    {
        saveFileName = name;

        // Display the file name in the label.
        entryLabel.text = name;

        // Start in the unselected visual state.
        SetSelected(false);

        // Wire the click in code.
        // Adding a listener to the Button.onClick UnityEvent.
        entryButton.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// Updates the background colour to reflect the selected or unselected state.
    /// </summary>
    public void SetSelected(bool isSelected)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = isSelected ? selectedColor : normalColor;
        }
    }

    // -------------------------------------------------------
    // Private Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Fired when the player clicks the SaveFileEntry row.
    /// Broadcasts the save filename through the EventBus 
    /// —> every instance of an entry in the scroll list 
    /// will receive this event also and update its own highlighted state.
    /// </summary>
    private void OnButtonClicked()
    {
        EventBus.SaveFileSelected(saveFileName);
    }

    /// <summary>
    /// Activated by SaveFileEntry when a row is clicked.
    /// Highlights this entry (chnages background colour) if the selected name matches
    /// otherwise deselects it.
    /// Only one row is ever highlighted at one time.
    /// </summary>
    private void OnAnyEntrySelected(string selectedName)
    {
        // Change the background colour to highlight the SaveFileEntry row.
        SetSelected(saveFileName == selectedName);
    }
}
