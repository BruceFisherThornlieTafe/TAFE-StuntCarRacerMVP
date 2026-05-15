// PlayerController.cs
// Scene: NewGame
//
// Car-like movement BASIC RigidBody controller using Unity's Input System.
// (Sound State Modified from very early version of my game "Rhizome")
// Copyright © Bruce Fisher 2026
//
// Default Controls:
//   W / S              - Accelerate forwards / reverse
//   A / D              - Steer left / right (only effective while moving)
//   Turb key           - Hold to exceed normal max speed
//   Brake key          - Slow down quickly (only triggers brake sound if moving)
//
// Fires EventBus events to drive the SoundManager:
//   EngineIdle     - no input or fully stopped
//   EngineRunning  - moving at normal speed
//   EngineTurbo    - moving with turbo held
//   CarBraking     - brake held fires only while the car has some speed
//
// Unity / Input System / Physics API References used:
//
// Rigidbody                        — Physics component used for movement and collisions.
//
// Rigidbody.constraints            — Locks position or rotation axes (used to keep car flat).
//
// Rigidbody.MoveRotation(...)      — Rotates a Rigidbody safely using physics.
//
// Rigidbody.AddForce(...)          — Applies force to a Rigidbody.
//
// Rigidbody.linearVelocity         — Current velocity of the Rigidbody.
//
// Rigidbody.angularVelocity        — Current rotation speed of the Rigidbody.
//
// RigidbodyConstraints             — Enum used to freeze position or rotation axes.
//
// ForceMode.VelocityChange         — Instantly changes velocity ignoring mass.
//
// InputActionReference.action      — Gets the InputAction from a reference.
//
// InputAction.action.Enable()      — Enables input detection.
//
// InputAction.action.Disable()     — Disables input detection.
//
// InputAction.ReadValue<Vector2>() — Reads 2D movement input (e.g. WASD / stick).
//
// InputAction.IsPressed()          — Checks if a button input is currently held.
//
// MonoBehaviour.Awake()            — Called when script instance is loaded.
//
// MonoBehaviour.Start()            — Called before first frame update.
//
// MonoBehaviour.OnEnable()         — Called when object becomes active.
//
// MonoBehaviour.OnDisable()        — Called when object becomes inactive.
//
// MonoBehaviour.FixedUpdate()      — Called every physics step.
//
// MonoBehaviour.OnCollisionEnter() — Called when collision starts.
//
// Mathf.Abs(float)                 — Returns absolute value (removes negative sign).
//
// Mathf.MoveTowards(...)           — Moves a value toward a target smoothly.
//
// Mathf.Clamp01(float)             — Clamps value between 0 and 1.
//
// Mathf.Euler(...) / Quaternion.Euler
//                                  — Creates rotation from angles.
//
// Vector3                          — Represents 3D position / direction.
//
// Vector2                          — Represents 2D input values.
//
// Time.fixedDeltaTime              — Fixed physics timestep value.
//
// Note:    For DEMO purpose only to save game using player cordinates
//          Just a quick and dirty controller and scene for demo

using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    // -------------------------------------------------------
    // Inspector Fields
    // -------------------------------------------------------

    [Header("<mark=#00008BFF>Input Actions - Player controller")]
    public InputActionReference moveAction;
    public InputActionReference brakeAction;
    public InputActionReference turboAction;

    [Header("<mark=#00008BFF>Car Movement")]
    public float acceleration = 50f;        // Units per second added to speed while input held
    public float maxSpeed = 100f;           // Top speed during normal driving
    public float turboSpeed = 150f;         // Top speed while turbo is held
    public float coastDecelerate = 20f;     // Deceleration rate when no input is given

    [Header("<mark=#00008BFF>Steering")]
    public float steerSpeed = 80f;          // Turn degrees per second at full speed

    [Header("<mark=#00008BFF>Braking")]
    public float brakeDecelerate = 100f;    // Deceleration rate while brake is held

    [Header("<mark=#00008BFF>Boost UI")]
    public GameObject boostActivatedUI;     // Boost pad activation UI in the inspector

    // -------------------------------------------------------
    // Private State
    // -------------------------------------------------------

    private Rigidbody rb;
    private float currentSpeed;

    // Tracks the current engine/brakes sound state
    private enum SoundState { Idle, Running, Turbo, Braking }
    private SoundState soundState = SoundState.Idle;

    // Boost pad stuff
    private bool boostPadActive = false;
    private Coroutine boostPadCoroutine;

    // -------------------------------------------------------
    // Unity Lifecycle
    // -------------------------------------------------------

    private void Awake()
    {
        // Get cars RigidBody for movement
        rb = GetComponent<Rigidbody>();

        // Lock Y position and all rotation so the demo car (capsule with Car UI)
        // moves only on a flat plane.
        // This would be changed for the actual game (real car body) with ramps etc.
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotation;

        // To explain further...
        // To allow ramps, jumps, and normal physics movement.
        // Freeze only the unwanted rotations so the car stays upright.
        // Something like this...
        // rb.constraints = RigidbodyConstraints.FreezeRotationX
        //                | RigidbodyConstraints.FreezeRotationZ;

        // No passive drag —> deceleration is handled manually via MoveTowards
        rb.linearDamping = 0f;
    }

    private void Start()
    {
        // Begin in idle state
        EventBus.EngineIdle();
        soundState = SoundState.Idle;
    }

    // Enable input actions when this object becomes active
    private void OnEnable()
    {
        moveAction.action.Enable();
        brakeAction.action.Enable();
        turboAction.action.Enable();
    }

    // Disable input actions when this object is deactivated to avoid ghost input
    private void OnDisable()
    {
        moveAction.action.Disable();
        brakeAction.action.Disable();
        turboAction.action.Disable();
    }

    // -------------------------------------------------------
    // Physics Update
    // -------------------------------------------------------

    /// <summary>
    /// Runs every fixed timestep. Handles sound state transitions,
    /// speed calculation, steering and speed application.
    /// </summary>
    private void FixedUpdate()
    {
        // Read Input
        Vector2 input = moveAction.action.ReadValue<Vector2>();
        bool braking = brakeAction.action.IsPressed();
        bool turbo = turboAction.action.IsPressed() || boostPadActive;

        // Dead zone on the forward/back axis to avoid drift from near zero stick input
        // I could just use "bool hasInput = input.y != 0f;" as keyboard only
        // But good practice to leave this to keep it gamepad safe without any extra
        // thought later on. Math.Abs -> no negatives
        //
        // To explain further why -> a thumbstick can return tiny numbers
        // as its a spring loaded analogue mechanism, not a perfect digital switch
        // like the old C64 joysticks that clicked as the micro switches were pressed.
        bool hasInput = Mathf.Abs(input.y) > 0.1f;

        // Small threshold to determine if the car is physically moving
        // used to prevent brake sound from firing on a very slow/stationary car
        bool isMoving = Mathf.Abs(currentSpeed) > 0.5f;

        // --- Sound State Machine ---
        // Priority order Braking > Turbo > Running > Idle
        // Each branch fires its EventBus event only once on the frame the state changes

        SoundState newState;

        // Car Braking
        if (braking && isMoving)
        {
            newState = SoundState.Braking;
        }
        // Engine Turbo
        else if (hasInput && turbo)
        {
            newState = SoundState.Turbo;
        }
        // Engine Running
        else if (hasInput)
        {
            newState = SoundState.Running;
        }
        // Engine Idle
        else
        {
            newState = SoundState.Idle;
        }

        // To explain the point of using a state machine:
        //      If you had the events below firing in above if statements
        //      then the sound would fire every frame -> stop sound then start
        //      this would be jittery. It also simplifies the logic.
        //
        // Also in SoundManager there is a second check to prevent this also
        // if (engineAudioSource.clip == clip && engineAudioSource.isPlaying) return;
        //
        // Exit if this audio clip is already running —> no need to restart it.
        // Only act on a state change!
        if (newState != soundState)
        {
            // Fire stop event when leaving braking state
            if (soundState == SoundState.Braking)
            {
                EventBus.CarStoppedBraking();
            }

            // Fire event for the new state
            switch (newState)
            {
                case SoundState.Idle: EventBus.EngineIdle(); break;
                case SoundState.Running: EventBus.EngineRunning(); break;
                case SoundState.Turbo: EventBus.EngineTurbo(); break;
                case SoundState.Braking: EventBus.CarBraking(); break;
            }

            soundState = newState;
        }

        // Speed Calculation
        // Choose target top speed based on whether turbo is active
        //
        // To explain further...
        // Work out the maximum speed we should aim for right now.
        //
        // If turbo is held -> use turboSpeed.
        // Otherwise -> use normal maxSpeed.
        //
        // input.y controls direction:
        //  +1 = forward
        //  -1 = reverse
        //
        // I used ternary operator here to save on further if statements
        // condition ? value if true : value if false
        float targetSpeed = hasInput && turbo
            ? input.y * turboSpeed
            : input.y * maxSpeed;

        // Set calculated speed
        //
        // To explain further added extra comments...
        // If brake is held -> slowly reduce speed down to zero (no matter which direction)
        if (braking)
        {
            // Brake -> decelerate toward zero regardless of input direction
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeDecelerate * Time.fixedDeltaTime);
        }
        // If player is pressing forward or back input.
        else if (hasInput)
        {
            // Accelerate -> toward target speed (handles both forward and reverse)
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
        }
        // If no input is pressed.
        else
        {
            // Coast -> gradually slow to a stop when no input is given
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, coastDecelerate * Time.fixedDeltaTime);
        }

        // Steering
        // Normalise speed against the current effective top speed so steer
        // sensitivity scales consistently whether turbo on or not
        //
        // To explain further...
        // Work out what the max possible speed is right now with
        // the ternary operator (short if/else statement)
        // condition ? value if true : value if false
        float effectiveMax = hasInput && turbo ? turboSpeed : maxSpeed;

        // To explain further...
        // Convert current speed into a 0–1 range compared to max speed.
        // This gives us a normalized value where:
        //      0 = not moving, 1 = at max speed
        // Clamp01 forces the value to stay between 0 and 1 so it never
        // goes negative or exceeds 1 if the speed goes over the limit.
        //
        // This is used to scale steering so:
        //      low speed = sharp and responsive turning
        //      high speed = reduced steering to prevent oversteering
        // Gives an arcade feel to the steering (used in my very early "Rhizome" game)
        float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / effectiveMax);

        // Set steering amount
        // Scale steer amount by how fast we're going —> no steering while stationary
        //
        // To explain further...
        // This calculates how much the car should rotate this frame.
        // input.x = left/right input (-1 to 1)
        // steerSpeed = turning strength (defined in inspector)
        // speedFactor = reduces steering at low or high speeds for better control
        // fixedDeltaTime = keeps physics movement consistent per frame
        float steer = input.x * steerSpeed * speedFactor * Time.fixedDeltaTime;

        // Reverse steering direction when travelling in reverse
        if (currentSpeed < 0f) steer = -steer;

        // Apply Calculated Steering
        //
        // To explain further...
        // This rotates the car around the Y axis (turning left/right)
        // A Quaternion represents an objects orientation in 3D space
        // —> meaning which direction it is facing
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, steer, 0f));

        // Calculate the velocity you WANT (Target)
        //
        // To explain further...
        // transform.forward is the direction the car is currently facing in world space.
        // Multiplying it by currentSpeed gives a velocity in that direction
        // -> so the car always moves forward/backward based on where it is pointing.
        Vector3 targetVelocity = transform.forward * currentSpeed;

        // Keep the current Y velocity so gravity still works
        //
        // To explain further...
        // Keep current up/down movement (gravity, falling, slopes)
        // -> Only control left/right/forward movement.
        targetVelocity.y = rb.linearVelocity.y;

        // Calculate the difference between where we are and where we want to be
        //
        // To explain further...
        // targetVelocity = how fast the car should be moving in that direction
        // rb.linearVelocity = currently how fast is the car going
        // in the 3D world (direction + speed)
        // For example you want the speed to be 10 but you are currently
        // going 6 therfore 10 - 6 = 4 so you neeed to add +4 more to speed
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;

        // Apply a force to match the target velocity immediately
        //
        // To explain further...
        // Apply an instant velocity correction
        // velocityChange = difference between current and desired movement speed (above)
        // ForceMode.VelocityChange ignores mass -> the adjustment is instant
        // This makes movement feel responsive and arcade-like
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    // -------------------------------------------------------
    // Collision Handling
    // -------------------------------------------------------
    // Added Collisions here as BASIC controller
   
    /* Disabed to avoid collision with barrels
    private void OnCollisionEnter(Collision collision)
    {
        // When car hits something set speed to 0
        currentSpeed = 0f;

        // Kill any leftover momentum to prevent jittering against any walls or objects
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
    */

    // Checks if boost pad is still on, if so it stops the current boost pad then it starts it again so the timer runs fully

    public void TriggerBoostPad(float duration)
    {
        if (boostPadCoroutine != null)
        {
            StopCoroutine(boostPadCoroutine);
        }

        boostPadCoroutine = StartCoroutine(BoostPadTimer(duration));
    }

    // Timer for the boost pad, activates by setting the boost to true then is told to wait for the duration to finish then sets boost to false
    private IEnumerator BoostPadTimer(float duration)
    {
        boostPadActive = true;
        boostActivatedUI.SetActive(true); // Activates the UI element when the boost pad is active
        yield return new WaitForSeconds(duration);
        boostPadActive = false;
        boostPadCoroutine = null;
        boostActivatedUI.SetActive(false); // Deactivates the UI element once boost pad period ends
    }

}