// ResolutionDisplay.cs
//
// Displays the current screen resolution in a TMP_Text element.
// Updates the display only when the resolution changes rather than every frame.
//
// Inspector setup:
//  - resolutionText : TMP_Text element to display the resolution

using UnityEngine;
using TMPro;

public class ResolutionDisplay : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>UI Output")]
    public TMP_Text resolutionText;

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    // Cached resolution values —> used to detect changes without updating every frame
    private int lastWidth;
    private int lastHeight;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Checks each frame whether the resolution has changed and
    /// updates the display text only when it has.
    /// </summary>
    private void Update()
    {
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            resolutionText.text = $"{Screen.width} x {Screen.height}";
        }
    }
}