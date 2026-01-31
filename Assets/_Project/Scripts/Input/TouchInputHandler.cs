using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manejador básico de input táctil para mobile
/// </summary>
public class TouchInputHandler : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent<Vector2> OnTap;
    public UnityEvent<Vector2> OnDragStart;
    public UnityEvent<Vector2> OnDrag;
    public UnityEvent<Vector2> OnDragEnd;

    [Header("Settings")]
    [SerializeField] private float tapThreshold = 0.2f;  // Tiempo máximo para tap
    [SerializeField] private float dragThreshold = 10f;  // Pixels mínimos para drag

    private Vector2 touchStartPos;
    private float touchStartTime;
    private bool isDragging;

    private void Update()
    {
        HandleTouchInput();

        // También soporta mouse para testing en editor
        #if UNITY_EDITOR
        HandleMouseInput();
        #endif
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        switch (touch.phase)
        {
            case TouchPhase.Began:
                touchStartPos = touch.position;
                touchStartTime = Time.time;
                isDragging = false;
                break;

            case TouchPhase.Moved:
                if (!isDragging && Vector2.Distance(touch.position, touchStartPos) > dragThreshold)
                {
                    isDragging = true;
                    OnDragStart?.Invoke(touchStartPos);
                }
                if (isDragging)
                {
                    OnDrag?.Invoke(touch.position);
                }
                break;

            case TouchPhase.Ended:
                if (isDragging)
                {
                    OnDragEnd?.Invoke(touch.position);
                }
                else if (Time.time - touchStartTime < tapThreshold)
                {
                    OnTap?.Invoke(touch.position);
                }
                isDragging = false;
                break;
        }
    }

    #if UNITY_EDITOR
    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            touchStartTime = Time.time;
            isDragging = false;
        }
        else if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            if (!isDragging && Vector2.Distance(mousePos, touchStartPos) > dragThreshold)
            {
                isDragging = true;
                OnDragStart?.Invoke(touchStartPos);
            }
            if (isDragging)
            {
                OnDrag?.Invoke(mousePos);
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
            {
                OnDragEnd?.Invoke(Input.mousePosition);
            }
            else if (Time.time - touchStartTime < tapThreshold)
            {
                OnTap?.Invoke(Input.mousePosition);
            }
            isDragging = false;
        }
    }
    #endif
}
