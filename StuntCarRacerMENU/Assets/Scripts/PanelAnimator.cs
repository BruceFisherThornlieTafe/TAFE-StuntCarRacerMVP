// PanelAnimator.cs
//
// Animates a UI panel scaling in from zero when activated and scaling out
// to zero before deactivating. Attach to any panel GameObject.
// The target scale is read automatically from the panels Inspector scale
// so no manual size assignment is needed.
// Call Close() instead of SetActive(false) to play the shrink animation
// before the panel deactivates.
//
// Inspector values:
//  duration    - how long the open and close aninations take in seconds
// Animation curves -> Edit each independently for effects 
//  openCurve   — controls the feel of the scale-in (open) animation
//  closeCurve  — controls the feel of the scale-out (close) animation
//
// Unity API References used:
//
// MonoBehaviour.Awake()            — Called when the object is first created.
//
// MonoBehaviour.OnEnable()         — Called when the GameObject becomes active.
//
// MonoBehaviour.StartCoroutine(...)— Starts a coroutine.
//
// MonoBehaviour.StopCoroutine(...) — Stops a running coroutine.
//
// Coroutine                        — Used to run code over multiple frames.
//
// IEnumerator                      — Used for coroutine methods.
//
// yield return null                — Waits until the next frame before continuing.
//
// Transform.localScale             — Gets or sets the scale of the object.
//
// Vector3.zero                     — Represents (0, 0, 0).
//
// Vector3.LerpUnclamped(...)       — Interpolates between two values.
//                                    Can go beyond 0–1 for overshoot effects.
//
// AnimationCurve.Evaluate(float)   — Returns a value from the curve at a given time.
//
// Time.unscaledDeltaTime           — Time between frames ignoring timeScale.
//
// GameObject.SetActive(bool)       — Enables or disables a GameObject.
//
// Coding ref: https://www.youtube.com/watch?v=RNccTrsgO9g
//
// Copyright © Bruce Fisher 2026 (made for "Rhizome" game)

using UnityEngine;
using System.Collections;

public class PanelAnimator : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Animation Settings")]
    // How long both the open and close scale animations take in seconds.
    public float duration = 0.25f;

    // Controls the feel of the opening animation
    // —> edit in the Inspector for effects.
    public AnimationCurve openCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // Controls the feel of the closing animation —> edit in the Inspector
    // independently from the open curve for a different closing feel.
    public AnimationCurve closeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    // The scale the panel animates toward on open and shrinks from on close.
    // Captured from the panels localScale in the Inspector during Awake.
    private Vector3 targetScale;

    // Reference to the currently running animation coroutine.
    // Stored so it can be stopped before starting a new one.
    private Coroutine currentAnimation;

    // -------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------

    /// <summary>
    /// Captures the panels Inspector scale as the animation target.
    /// This means whatever size the panel is set to in the Inspector
    /// is automatically used as the fully open size —> no manual setup needed.
    /// </summary>
    private void Awake()
    {
        // Set targetscale to panels localScale full size
        targetScale = transform.localScale;
    }

    /// <summary>
    /// Called automatically by Unity when the GameObject is activated via SetActive(true).
    /// Stops any in-progress animation and plays the scale-in animation from zero
    /// to the target scale using the openCurve.
    /// </summary>
    private void OnEnable()
    {
        // Stop any in-progress animation before starting the open animation
        // prevents conflicts if the panel is toggled rapidly.
        if (currentAnimation != null) StopCoroutine(currentAnimation);

        // Reset scale to zero before playing the open animation.
        transform.localScale = Vector3.zero;

        // Animate from zero to the target scale using the open curve.
        currentAnimation = StartCoroutine(ScaleTo(Vector3.zero, targetScale, openCurve));
    }

    // -------------------------------------------------------
    // Public Methods
    // -------------------------------------------------------

    /// <summary>
    /// Call this instead of SetActive(false) to play the shrink animation
    /// before the panel deactivates. Stops any in-progress animation first
    /// so the open and close are never run simultaneously.
    /// Uses the closeCurve to control the feel of the shrink animation.
    /// </summary>
    public void Close()
    {
        // Stop any in-progress animation before starting the close animation.
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        // Play shrink animation (using the close curve) then deactivate.
        currentAnimation = StartCoroutine(ScaleOutThenHide());
    }

    // -------------------------------------------------------
    // Private Coroutines
    // -------------------------------------------------------

    /// <summary>
    /// Animates the panel shrinking to zero using the closeCurve,
    /// then deactivates the GameObject.
    /// Resets the scale to zero after deactivation so the panel is ready
    /// to animate back in the next time it is activated.
    /// </summary>
    IEnumerator ScaleOutThenHide()
    {
        // Wait and let the shrink animation finish before deactivating.
        // Uses the closeCurve so the closing feel is independent of the open.
        yield return StartCoroutine(ScaleTo(targetScale, Vector3.zero, closeCurve));

        // Deactivate the panel now that it has fully shrunk.
        gameObject.SetActive(false);

        // Reset scale to zero so OnEnable starts the open animation from zero.
        transform.localScale = Vector3.zero;
    }

    /// <summary>
    /// Animates localScale from one value to another over the set duration.
    /// Accepts a curve parameter so opening and closing can each pass their
    /// own curve without needing separate coroutine implementations.
    /// LerpUnclamped allows the curve to go beyond 0-1 for bounce or overshoot effects.
    /// </summary>
    /// <param name="from">The scale to start from.</param>
    /// <param name="to">The scale to animate toward.</param>
    /// <param name="curve">The AnimationCurve used to evaluate the interpolation over time.</param>
    IEnumerator ScaleTo(Vector3 scaleFrom, Vector3 scaleTo, AnimationCurve curve)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // Accumulate time each frame
            // Any UI animation during pause must use unscaledDeltaTime instead of deltaTime as
            // "Time.timeScale = 0f" used by Pause Menus stops the timeScale
            elapsed += Time.unscaledDeltaTime;

            // Evaluate the provided animation curve at the normalised time (0 to 1)
            // https://docs.unity3d.com/ScriptReference/AnimationCurve.html
            // https://discussions.unity.com/t/scaling-an-animationcurve-variable/738755
            float curveTime = curve.Evaluate(elapsed / duration);

            // Apply the scale —> LerpUnclamped allows overshoot if the curve goes above 1
            transform.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, curveTime);

            // Wait for the next frame before continuing
            yield return null;
        }

        // Snap back to the exact target scale to correct any floating point drift
        // This is a common for reseting floats when using Lerp.
        transform.localScale = scaleTo;
    }
}