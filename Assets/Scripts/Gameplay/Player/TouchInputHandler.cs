using UnityEngine;
using UnityEngine.Events;

public enum SwipeDirection
{
    None,
    Left,
    Right,
    Up,
    Down
}

/// <summary>
/// Optimized touch/swipe input handler with minimal lag
/// Detects swipes early based on velocity, not just on finger lift
/// </summary>
public class TouchInputHandler : MonoBehaviour
{
    [Header("Tap Events")]
    public UnityEvent<Vector2> OnTap;

    [Header("Swipe Events")]
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeDown;
    public UnityEvent<SwipeDirection> OnSwipe;

    [Header("Drag Events")]
    public UnityEvent<Vector2> OnDragStart;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent<Vector2> OnDragEnd;

    [Header("Tap Settings")]
    [SerializeField] private float tapTimeThreshold = 0.2f;
    [SerializeField] private float tapDistanceThreshold = 30f;

    [Header("Swipe Settings")]
    [Tooltip("Minimum distance to trigger swipe (pixels)")]
    [SerializeField] private float swipeDistanceThreshold = 50f;

    [Tooltip("Minimum velocity to trigger early swipe (pixels/second)")]
    [SerializeField] private float swipeVelocityThreshold = 300f;

    [Tooltip("Max time for a swipe gesture (seconds)")]
    [SerializeField] private float swipeTimeThreshold = 0.5f;

    [Tooltip("Time window where taps are prioritized over swipes (seconds)")]
    [SerializeField] private float tapPriorityWindow = 0.1f;

    [Header("Drag Settings")]
    [SerializeField] private float dragThreshold = 15f;

    [Tooltip("Minimum distance moved to fire OnDrag event (reduces event spam)")]
    [SerializeField] private float dragUpdateThreshold = 5f;

    // State
    private Vector2 touchStartPos;
    private Vector2 lastTouchPos;
    private Vector2 lastDragEventPos; // Last position where OnDrag was fired
    private float touchStartTime;
    private float lastTouchTime;
    private bool isDragging;
    private bool swipeDetected;
    private bool isTracking;
    private bool isMouseInput; // Track if input is from mouse vs touch

    // Velocity tracking
    private Vector2 velocity;
    private const int VELOCITY_SAMPLES = 3;
    private Vector2[] velocitySamples = new Vector2[VELOCITY_SAMPLES];
    private int velocitySampleIndex;

    [Header("Debug (Editor Only)")]
    [SerializeField] private bool enableKeyboardSimulation = true;
    [SerializeField] private bool showDebugLogs = true;

    private void Update()
    {
        HandleTouchInput();

#if UNITY_EDITOR
        HandleMouseInput();
        if (enableKeyboardSimulation)
        {
            HandleKeyboardSimulation();
        }
#endif
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            // Only end touch if we were tracking touch (not mouse)
            if (isTracking && !isMouseInput)
            {
                EndTouch(lastTouchPos);
            }
            return;
        }

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                isMouseInput = false;
                StartTouch(touch.position);
                break;

            case TouchPhase.Moved:
            case TouchPhase.Stationary:
                UpdateTouch(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndTouch(touch.position);
                break;
        }
    }

#if UNITY_EDITOR
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isMouseInput = true;
            StartTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isTracking && isMouseInput)
        {
            UpdateTouch(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && isTracking && isMouseInput)
        {
            EndTouch(Input.mousePosition);
        }
    }

    private void HandleKeyboardSimulation()
    {
        // Arrow keys or WASD to simulate swipes
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            FireSwipeEvent(SwipeDirection.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            FireSwipeEvent(SwipeDirection.Down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            FireSwipeEvent(SwipeDirection.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            FireSwipeEvent(SwipeDirection.Right);
        }
    }
#endif

    private void StartTouch(Vector2 position)
    {
        touchStartPos = position;
        lastTouchPos = position;
        touchStartTime = Time.unscaledTime;
        lastTouchTime = Time.unscaledTime;
        isDragging = false;
        swipeDetected = false;
        isTracking = true;

        // Reset velocity samples
        velocitySampleIndex = 0;
        for (int i = 0; i < VELOCITY_SAMPLES; i++)
        {
            velocitySamples[i] = Vector2.zero;
        }
    }

    private void UpdateTouch(Vector2 position)
    {
        if (!isTracking) return;

        float currentTime = Time.unscaledTime;
        float deltaTime = currentTime - lastTouchTime;

        if (deltaTime > 0.001f)
        {
            // Calculate instantaneous velocity
            Vector2 delta = position - lastTouchPos;
            Vector2 instantVelocity = delta / deltaTime;

            // Store velocity sample
            velocitySamples[velocitySampleIndex] = instantVelocity;
            velocitySampleIndex = (velocitySampleIndex + 1) % VELOCITY_SAMPLES;

            // Calculate smoothed velocity
            velocity = Vector2.zero;
            for (int i = 0; i < VELOCITY_SAMPLES; i++)
            {
                velocity += velocitySamples[i];
            }
            velocity /= VELOCITY_SAMPLES;
        }

        lastTouchPos = position;
        lastTouchTime = currentTime;

        // Check for early swipe detection (based on velocity)
        if (!swipeDetected)
        {
            TryDetectSwipe(position, false);
        }

        // Handle drag
        if (!isDragging && !swipeDetected)
        {
            float distance = Vector2.Distance(position, touchStartPos);
            if (distance > dragThreshold)
            {
                isDragging = true;
                lastDragEventPos = touchStartPos;
                OnDragStart?.Invoke(touchStartPos);
            }
        }

        // Only fire OnDrag if position changed beyond threshold (reduces event spam)
        if (isDragging && !swipeDetected)
        {
            float dragDelta = Vector2.Distance(position, lastDragEventPos);
            if (dragDelta >= dragUpdateThreshold)
            {
                lastDragEventPos = position;
                OnDrag?.Invoke(position);
            }
        }
    }

    private void EndTouch(Vector2 position)
    {
        if (!isTracking) return;

        float touchDuration = Time.unscaledTime - touchStartTime;
        float distance = Vector2.Distance(position, touchStartPos);

#if UNITY_EDITOR
        if (showDebugLogs)
        {
            Debug.Log($"[Touch] EndTouch - Duration: {touchDuration:F3}s, Distance: {distance:F1}px, Velocity: {velocity.magnitude:F1}px/s");
        }
#endif

        // Try final swipe detection if not already detected
        if (!swipeDetected)
        {
            TryDetectSwipe(position, true);
        }

        // Check for tap (small distance = tap, regardless of duration)
        // This allows both quick taps and longer "hold and release" to register as taps
        if (!swipeDetected && !isDragging)
        {
            if (distance < tapDistanceThreshold)
            {
#if UNITY_EDITOR
                if (showDebugLogs) Debug.Log($"[Touch] TAP detected (duration: {touchDuration:F2}s)");
#endif
                OnTap?.Invoke(position);
            }
#if UNITY_EDITOR
            else if (showDebugLogs)
            {
                Debug.Log($"[Touch] No tap - distance: {distance:F1}px (threshold: {tapDistanceThreshold}px)");
            }
#endif
        }

        // End drag
        if (isDragging)
        {
            OnDragEnd?.Invoke(position);
        }

        // Reset state
        isTracking = false;
        isDragging = false;
        swipeDetected = false;
        isMouseInput = false;
    }

    private void TryDetectSwipe(Vector2 currentPos, bool isFinalCheck)
    {
        if (swipeDetected) return;

        float touchDuration = Time.unscaledTime - touchStartTime;

        // Don't detect swipe if too much time has passed
        if (touchDuration > swipeTimeThreshold) return;

        // During tap priority window, don't detect swipes (let quick taps complete)
        if (!isFinalCheck && touchDuration < tapPriorityWindow) return;

        Vector2 delta = currentPos - touchStartPos;
        float distance = delta.magnitude;

        // On final check, if within tap thresholds, don't treat as swipe (will be detected as tap)
        if (isFinalCheck && touchDuration < tapTimeThreshold && distance < tapDistanceThreshold)
        {
            return;
        }

        // Check velocity-based early detection
        float speed = velocity.magnitude;
        bool velocityTriggered = speed > swipeVelocityThreshold && distance > swipeDistanceThreshold * 0.5f;

        // Check distance-based detection (on final check or with high velocity)
        bool distanceTriggered = distance > swipeDistanceThreshold;

        if (velocityTriggered || (isFinalCheck && distanceTriggered))
        {
            SwipeDirection direction = GetSwipeDirection(delta);

            if (direction != SwipeDirection.None)
            {
                swipeDetected = true;
                FireSwipeEvent(direction);
            }
        }
    }

    private SwipeDirection GetSwipeDirection(Vector2 delta)
    {
        // Determine if swipe is more horizontal or vertical
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            // Horizontal swipe
            return delta.x > 0 ? SwipeDirection.Right : SwipeDirection.Left;
        }
        else
        {
            // Vertical swipe
            return delta.y > 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
    }

    private void FireSwipeEvent(SwipeDirection direction)
    {
#if UNITY_EDITOR
        if (showDebugLogs) Debug.Log($"[Touch] SWIPE detected: {direction}");
#endif

        OnSwipe?.Invoke(direction);

        switch (direction)
        {
            case SwipeDirection.Left:
                OnSwipeLeft?.Invoke();
                break;
            case SwipeDirection.Right:
                OnSwipeRight?.Invoke();
                break;
            case SwipeDirection.Up:
                OnSwipeUp?.Invoke();
                break;
            case SwipeDirection.Down:
                OnSwipeDown?.Invoke();
                break;
        }
    }
}
