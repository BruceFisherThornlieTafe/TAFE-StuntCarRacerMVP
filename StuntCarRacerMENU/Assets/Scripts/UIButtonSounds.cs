// UIButtonSounds.cs
//
// Plays a hover sound when the player moves mouse over a button and a click
// sound when the button is pressed. Attach this script to a button that
// needs audio feedback. Both sounds are routed through the SFX Audio Source
// so they are controlled by the SFX slider in the options panel.
//
// Inspector setup:
//   - sfxSource  : SFX AudioSource with its Output set to the SFX mixer group
//   - hoverSound : clip to play when the pointer enters the button
//   - clickSound : clip to play when the button is clicked
//
// Unity / UI / EventSystem / Audio API References used:
//
// AudioSource                  — Component used to play audio in Unity.
//
// AudioSource.PlayOneShot(...) — Plays a single audio clip without interrupting the current one.
//
// AudioClip                    — Stores audio data (sound files).
//
// MonoBehaviour.OnPointerEnter — Interface callback triggered when the mouse enters a UI element.
//
// MonoBehaviour.OnPointerClick — Interface callback triggered when the UI element is clicked.
//
// IPointerEnterHandler         — Unity interface for detecting pointer hover events.
//
// IPointerClickHandler         — Unity interface for detecting pointer click events.
//
// PointerEventData             — Contains data about the pointer event (mouse/touch input).
//
// Copyright © Bruce Fisher 2026 (made for "Rhizome" game)

using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    // SFX Audio Source must have its Output set to the SFX mixer group
    // so hover and click sounds are controlled by the SFX volume slider
    [Header("<mark=#00008BFF>Audio Source")]
    public AudioSource sfxSource;

    [Header("<mark=#00008BFF>Audio Clips")]
    // Sound that plays when the player hovers over the button
    public AudioClip hoverSound;

    // Sound that plays when the player clicks the button
    public AudioClip clickSound;

    // -------------------------------------------------------
    // Unity Interface Implementations
    // IPointerEnterHandler, IPointerClickHandler
    // - called automatically
    // -------------------------------------------------------

    /// <summary>
    /// Called automatically by Unity when the pointer enters this button.
    /// PlayOneShot is used so hover sounds can overlap without interrupting each other.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only play if both the source and clip have been assigned in the Inspector.
        if (sfxSource != null && hoverSound != null)
        {
            sfxSource.PlayOneShot(hoverSound);
        }
    }

    /// <summary>
    /// Called automatically by Unity when the pointer clicks this button.
    /// PlayOneShot is used so a click does not cut off an overlapping hover sound.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        // Only play if both the source and clip have been assigned in the Inspector.
        if (sfxSource != null && clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}