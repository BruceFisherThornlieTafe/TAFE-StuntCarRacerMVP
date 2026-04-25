// EventBus.cs
//
// Central event bus. Better way as Decouples systems.
//
// Why have I used a Static?
//  Static so it persists in all scenes
//  Does not inherit from MonoBehaviour, as it isn't attached to a GameObject.
//  Static is better and safer than using a singleton because isn't depended on
//  Unity LifeCycle (it can cause timing bugs) therefore triggering issues.
//
// Uses Wrapper Methods     eg EngineIdle() => OnEngineIdle?.Invoke();
//                          you call EnginIdle(); instead of OnEngineIdle?.Invoke();
// Much easier to write everywhere once your project expands.

using System;

public static class EventBus
{
    // -------------------------------------------------------
    // Car Sound Events
    // -------------------------------------------------------
    public static void EngineIdle() => OnEngineIdle?.Invoke();
    public static event Action OnEngineIdle;
    public static void EngineRunning() => OnEngineRunning?.Invoke();
    public static event Action OnEngineRunning;
    public static void EngineTurbo() => OnEngineTurbo?.Invoke();
    public static event Action OnEngineTurbo;
    public static void CarBraking() => OnCarBraking?.Invoke();
    public static event Action OnCarBraking;
    public static void CarStoppedBraking() => OnCarStoppedBraking?.Invoke();
    public static event Action OnCarStoppedBraking;

    // -------------------------------------------------------
    // Pause Events
    // -------------------------------------------------------
    public static void Pause() => OnPause?.Invoke();
    public static event Action OnPause;
    public static void Resume() => OnResume?.Invoke();
    public static event Action OnResume;
    public static void SavePanelEscape() => OnSavePanelEscape?.Invoke();
    public static event Action OnSavePanelEscape;

    // -------------------------------------------------------
    // Save File Selection Events
    // -------------------------------------------------------

    // Fired by SaveFileEntry (prefab script) when the player clicks a row
    // in the continue panel list.
    // Passses the fileName of the selected save file so MenuManager can:
    //   1. Deselect the previously highlighted row
    //   2. Highlight the newly selected row
    //   3. Enable the Load and Delete button and store the chosen name
    public static void SaveFileSelected(string saveFileName) => OnSaveFileSelected?.Invoke(saveFileName);
    public static event Action<string> OnSaveFileSelected;
}
