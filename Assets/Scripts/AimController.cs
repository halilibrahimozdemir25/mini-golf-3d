using UnityEngine;

public class AimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GolfBall ball;
    [SerializeField] private Transform holeTarget;
    [SerializeField] private LineRenderer aimLine;

    [Header("Aiming Settings")]
    [SerializeField] private float rotationSpeed = 0.4f;
    [SerializeField] private float aimLineLength = 3.5f;

    public Vector3 AimDirection { get; private set; }
    public bool IsAimingEnabled { get; set; } = true;

    private float currentAngle = 0f;
    private Vector2 lastTouchPosition;
    private bool isDragging = false;

    private void Awake()
    {
        if (aimLine == null)
        {
            aimLine = gameObject.AddComponent<LineRenderer>();
            aimLine.startWidth = 0.08f;
            aimLine.endWidth = 0.01f;
            aimLine.positionCount = 2;
            aimLine.useWorldSpace = true;
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.startColor = new Color(1f, 0.92f, 0.2f, 0.9f);
            aimLine.endColor = new Color(1f, 0.3f, 0.1f, 0f);
        }
    }

    private void Start()
    {
        AimAtTarget();
    }

    public void SetTarget(Transform newHole)
    {
        holeTarget = newHole;
        AimAtTarget();
    }

    public void AimAtTarget()
    {
        if (ball != null && holeTarget != null)
        {
            Vector3 dir = (holeTarget.position - ball.transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                currentAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
                UpdateDirection();
            }
        }
    }

    public void SetAimLinePower(float powerRatio)
    {
        aimLineLength = Mathf.Lerp(1.8f, 6.5f, Mathf.Clamp01(powerRatio));
    }

    private void Update()
    {
        if (!IsAimingEnabled || ball == null || ball.IsMoving || ball.IsInHole)
        {
            if (aimLine != null) aimLine.enabled = false;
            return;
        }

        HandleInput();
        UpdateDirection();
        UpdateAimVisual();
    }

    private void HandleInput()
    {
        // Touch Input for Mobile
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                bool isOverUI = UnityEngine.EventSystems.EventSystem.current != null && 
                                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId);

                if (!isOverUI && touch.position.y > Screen.height * 0.22f)
                {
                    isDragging = true;
                    lastTouchPosition = touch.position;
                }
                else
                {
                    isDragging = false;
                }
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                float deltaX = touch.position.x - lastTouchPosition.x;
                currentAngle += deltaX * rotationSpeed;
                lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        // Mouse / Standalone fallback
        else if (Input.GetMouseButtonDown(0))
        {
            bool isOverUI = UnityEngine.EventSystems.EventSystem.current != null && 
                            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (!isOverUI && Input.mousePosition.y > Screen.height * 0.22f)
            {
                isDragging = true;
                lastTouchPosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            float deltaX = (float)Input.mousePosition.x - lastTouchPosition.x;
            currentAngle += deltaX * rotationSpeed;
            lastTouchPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }

    private void UpdateDirection()
    {
        float rad = currentAngle * Mathf.Deg2Rad;
        AimDirection = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)).normalized;
    }

    private void UpdateAimVisual()
    {
        if (aimLine == null || ball == null) return;

        aimLine.enabled = true;
        Vector3 startPos = ball.transform.position + Vector3.up * 0.05f;
        Vector3 endPos = startPos + AimDirection * aimLineLength;

        aimLine.SetPosition(0, startPos);
        aimLine.SetPosition(1, endPos);
    }
}
