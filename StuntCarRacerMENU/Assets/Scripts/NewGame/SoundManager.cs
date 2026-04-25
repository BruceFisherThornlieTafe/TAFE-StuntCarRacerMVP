// SoundManager.cs
// Scene: NewGame
//
// Listens to EventBus events and plays the appropriate audio clip for each state.
//
// Looping engine clips (idle, running, turbo) are managed through a shared helper
// that avoids restarting a clip that is already playing.
//
// The brake clip plays as a one-shot since it is a discrete sound effect
// rather than a continuous engine tone.
//
// Assign all clips and the AudioSources in the Inspector.
//
// Unity / Audio / System API References used:
//
// AudioSource              — Component used to play audio in Unity.
//
// AudioSource.clip         — The AudioClip currently assigned to the source.
//
// AudioSource.loop         — Determines if the audio should repeat.
//
// AudioSource.Play()       — Starts playing the assigned audio clip.
//
// AudioSource.Stop()       — Stops audio playback immediately.
//
// AudioSource.isPlaying    — Returns true if the audio source is currently playing.
//
// AudioClip                — Stores audio data (sound files).
//
// MonoBehaviour.OnEnable() - Called when the object becomes active.
//
// MonoBehaviour.OnDisable()— Called when the object becomes inactive.

using UnityEngine;

public class SoundManager : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    // Audio Sources must have its Output set to the SFX mixer group
    // so sounds are controlled by the SFX volume slider.
    [Header("<mark=#00008BFF>Audio Source")]
    public AudioSource engineAudioSource;   // looping engine tone.

    // To explain further...
    // Need a seperate sound source as the braking sound as triggered by a different
    // type of behaviour than the engine sounds.
    // Simply put:
    //  -> Engine sounds loop continuously (engine is always on)
    //  -> Braking sound is a short one-shot sound effect (a short to long brake squeal on demand)
    public AudioSource brakingAudioSource;  // dedicated source for braking.

    [Header("<mark=#00008BFF>Engine Sounds")]
    public AudioClip engineIdleClip;        // Looped while the car is stationary.
    public AudioClip engineRunningClip;     // Looped during normal driving.
    public AudioClip engineTurboClip;       // Looped while turbo is held.
    public AudioClip carBrakingClip;        // One-shot played when braking begins.

    // -------------------------------------------------------
    // Event Subscription
    // -------------------------------------------------------

    // Subscribe to all EventBus events when this object becomes active.
    private void OnEnable()
    {
        EventBus.OnEngineIdle += PlayEngineIdle;
        EventBus.OnEngineRunning += PlayEngineRunning;
        EventBus.OnEngineTurbo += PlayEngineTurbo;
        EventBus.OnCarBraking += PlayCarBraking;
        EventBus.OnCarStoppedBraking += StopPlayCarBraking;
    }

    // Unsubscribe when deactivated to prevent calls to a destroyed object.
    private void OnDisable()
    {
        EventBus.OnEngineIdle -= PlayEngineIdle;
        EventBus.OnEngineRunning -= PlayEngineRunning;
        EventBus.OnEngineTurbo -= PlayEngineTurbo;
        EventBus.OnCarBraking -= PlayCarBraking;
        EventBus.OnCarStoppedBraking -= StopPlayCarBraking;
    }

    // -------------------------------------------------------
    // Event Handlers
    // -------------------------------------------------------

    // Fired when the car is stationary and no input is given.
    private void PlayEngineIdle()
    {
        PlayLooping(engineIdleClip);
    }

    // Fired when the car is moving at normal speed.
    private void PlayEngineRunning()
    {
        PlayLooping(engineRunningClip);
    }

    // Fired when the car is moving with turbo held.
    private void PlayEngineTurbo()
    {
        PlayLooping(engineTurboClip);
    }

    // Fired once when braking begins.
    private void PlayCarBraking()
    {
        brakingAudioSource.clip = carBrakingClip;
        brakingAudioSource.loop = false;
        brakingAudioSource.Play();
    }

    // Fired when braking ends.
    // When the car is no longer moving.
    // Creates real life effect (as you should not hear brakes when car stops)
    // -> short braking sound to longer braking sound.
    private void StopPlayCarBraking()
    {
        brakingAudioSource.Stop();
    }

    // -------------------------------------------------------
    // Helpers
    // -------------------------------------------------------

    /// <summary>
    /// Stops the current clip and starts the new one looping.
    /// Does nothing if the requested clip is already playing,
    /// preventing audible restarts on repeated event calls.
    /// </summary>
    private void PlayLooping(AudioClip clip)
    {
        if (clip == null) return;

        // Exit if this clip is already running —> no need to restart it.
        if (engineAudioSource.clip == clip && engineAudioSource.isPlaying) return;

        // Stop whatever is currently playing before switching clips.
        engineAudioSource.Stop();
        engineAudioSource.clip = clip;
        engineAudioSource.loop = true;
        engineAudioSource.Play();
    }
}