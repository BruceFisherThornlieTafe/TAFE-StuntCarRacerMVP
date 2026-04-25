// AudioManager.cs
// Scene: MainMenu
//
// Manages the volume of background music and sound effects separately.
// Uses an AudioMixer to control volume levels via sliders in the options panel.
// Saves and loads volume settings via PlayerPrefs.
//
// Audio Mixer setup:
//  - Expose the music volume parameter and name it "MusicVolume"
//  - Expose the SFX volume parameter and name it "SFXVolume"
//
// Inspector setup:
//  - audioMixer    : Audio Mixer from your project
//  - musicSlider   : Music volume Slider UI element
//  - sfxSlider     : the SFX volume Slider UI element
//
// Unity API References used:
//
// PlayerPrefs.GetFloat(string, float)  — Reads a saved float from local storage
//                                        returns the default value if the key does not exist.
// Slider.value                         — Gets or sets the current numeric value of the slider.
//
// Mathf.Max(float, float)              — Returns the larger of two float values.
//
// Mathf.Log10(float)                   — Returns the base-10 logarithm of a float.
//
// AudioMixer.SetFloat(string, float)   — Sets a given name volume float parameter on the
//                                        Audio Mixer.
//
// PlayerPrefs.SetFloat(string, float)  — Writes a float value to local storage under a named key.
//
// PlayerPrefs.Save()                   — Forces an immediate write of all pending
//                                        PlayerPrefs data to disk.
//
// Coding Ref: https://www.youtube.com/watch?v=pbuJUaO-wpY
//             https://discussions.unity.com/t/changing-audio-mixer-group-volume-with-ui-slider/567394/7
//             https://stackoverflow.com/questions/46529147/how-to-set-a-mixers-volume-to-a-sliders-volume-in-unity#:~:text=I%20think%2C%20your%20problem%20is,puts%20the%20volume%20up%20again.

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Audio Mixer References")]
    // Reference to the Audio Mixer
    public AudioMixer audioMixer;

    // Reference to the music volume slider
    public Slider musicSlider;

    // Reference to the SFX volume slider
    public Slider sfxSlider;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    // Called once when the scene loads
    private void Start()
    {
        // Load saved volume settings if they exist, otherwise default to full volume
        float savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Apply the loaded values to the sliders —> triggers OnValueChanged
        // which in turn calls SetMusicVolume and SetSFXVolume automatically
        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    // Called by the music slider OnValueChanged event
    // Volume parameter is linear (0.0 to 1.0) and converted to decibels for the mixer
    public void SetMusicVolume(float volume)
    {
        // AudioMixer works in decibels —> convert linear slider value to dB
        // Mathf.Log10 of a very small number approaches infinity so we clamp to 0.0001
        audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);

        // Save the linear slider value
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    // Called by the SFX slider OnValueChanged event
    // Volume parameter is linear (0.0 to 1.0) and converted to decibels for the mixer
    public void SetSFXVolume(float volume)
    {
        // AudioMixer works in decibels —> convert linear slider value to dB
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20);

        // Save the linear slider value
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }
}