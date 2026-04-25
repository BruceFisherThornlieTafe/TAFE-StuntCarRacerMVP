// ResolutionSettings.cs
// Scene: MainMenu
//
// Handles resolution switching via UI Buttons.
// Each button calls a public method for a specific resolution.
//
// Inspector setup:
//  - backgroundVideoRT     : Render Texture used by the background video
//  - backgroundVideoPlayer : VideoPlayer component for the background video
//
// Coding Ref:  https://www.youtube.com/watch?v=EH9XAt1XGOE

using UnityEngine;
using UnityEngine.Video;

public class ResolutionSettings : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    public RenderTexture backgroundVideoRT;
    public VideoPlayer backgroundVideoPlayer;

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    // Tracks which resolution index is currently active
    private int currentResolutionIndex = 2; // Default Full HD

    // Preset resolution widths and heights
    private int[] widths = { 1280, 1600, 1920 };
    private int[] heights = { 720, 900, 1080 };

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    private void Start()
    {
        // Load saved resolution
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            currentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex");
        }

        ApplyResolution(currentResolutionIndex);
    }

    // -------------------------------------------------------
    // Public API
    // -------------------------------------------------------

    /// <summary>
    /// Sets resolution to 1280x720 (HD)
    /// </summary>
    public void SetHD()
    {
        SetResolution(0);
    }

    /// <summary>
    /// Sets resolution to 1600x900 (HD+)
    /// </summary>
    public void SetHDPlus()
    {
        SetResolution(1);
    }

    /// <summary>
    /// Sets resolution to 1920x1080 (Full HD)
    /// </summary>
    public void SetFullHD()
    {
        SetResolution(2);
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Updates the active index and applies the resolution
    /// and saves the choice to PlayerPrefs.
    /// </summary>
    private void SetResolution(int index)
    {
        currentResolutionIndex = index;

        ApplyResolution(index);

        PlayerPrefs.SetInt("ResolutionIndex", index);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Applies the screen resolution and resizes the Render Texture for the given index.
    /// Preserves the current fullscreen state when changing resolution.
    /// </summary>
    private void ApplyResolution(int index)
    {
        Screen.SetResolution(widths[index], heights[index], Screen.fullScreen);

        // Resize the Render Texture for the background video to match the new resolution
        UpdateRenderTexture(widths[index], heights[index]);
    }

    /// <summary>
    /// Releases and recreates the Render Texture at the new size, then restarts
    /// the video player so it plays on the freshly resized Render Texture.
    /// Stops the video before modifying the Render Texture to avoid GPU conflicts.
    /// Code Ref: https://www.reddit.com/r/unity/comments/1pna752/problem_with_video_player_playback_size/
    /// Code Ref: https://docs.unity3d.com/ScriptReference/RenderTexture.Release.html
    /// </summary>
    private void UpdateRenderTexture(int width, int height)
    {
        if (backgroundVideoRT == null) return;

        if (backgroundVideoPlayer != null)
        {
            backgroundVideoPlayer.Stop();
        }

        backgroundVideoRT.Release(); // Free the existing Render Texture from the GPU
        backgroundVideoRT.width = width;
        backgroundVideoRT.height = height;
        backgroundVideoRT.Create(); // Reallocate the Render Texture on the GPU at the new size

        if (backgroundVideoPlayer != null)
        {
            backgroundVideoPlayer.Play();
        }
    }
}