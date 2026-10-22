using UnityEngine;
using UnityEngine.UI;

public class SlingshotController : MonoBehaviour
{
    private enum TouchMode { None, Slingshot, Orbit }

    [Header("Core References")]
    [SerializeField] private GolfBall ball;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private LineRenderer aimLine;
    [SerializeField] private LineRenderer pullBand;

    [Header("UI Feedback")]
    [SerializeField] private Text powerBadgeText;
    [SerializeField] private Image powerBadgeBg;
    [SerializeField] private GameObject powerBadgeObj;

    [Header("Slingshot Settings")]
    [SerializeField] private float ballTouchRadiusPx = 75f;
    [SerializeField] private float maxDragDistance = 220f;
    [SerializeField] private float minAimLength = 1.2f;
    [SerializeField] private float maxAimLength = 7.5f;

    public bool IsAimingEnabled { get; set; } = true;
    public bool IsPulling => currentMode == TouchMode.Slingshot;
    public Vector3 CurrentAimDirection { get; private set; } = Vector3.forward;
    public float CurrentPowerRatio { get; private set; } = 0f;

    private TouchMode currentMode = TouchMode.None;
    private Vector2 touchStartPos;
    private Vector2 lastPointerPos;
    private Vector3 lockedCamForward;
    private Vector3 lockedCamRight;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (cameraController == null && mainCamera != null)
        {
            cameraController = mainCamera.GetComponent<CameraController>();
        }

        SetupAimLine();
        SetupPullBand();

        if (powerBadgeObj != null)
        {
            powerBadgeObj.SetActive(false);
        }
    }

    private void SetupAimLine()
    {
        if (aimLine == null)
        {
            GameObject lineObj = new GameObject("SlingshotAimLine");
            lineObj.transform.SetParent(transform);
            aimLine = lineObj.AddComponent<LineRenderer>();
            aimLine.startWidth = 0.12f;
            aimLine.endWidth = 0.03f;
            aimLine.positionCount = 2;
            aimLine.useWorldSpace = true;
            aimLine.material = new Material(Shader.Find("Sprites/Default"));
            aimLine.enabled = false;
        }
    }

    private void SetupPullBand()
    {
        if (pullBand == null)
        {
            GameObject bandObj = new GameObject("SlingshotPullBand");
            bandObj.transform.SetParent(transform);
            pullBand = bandObj.AddComponent<LineRenderer>();
            pullBand.startWidth = 0.04f;
            pullBand.endWidth = 0.04f;
            pullBand.positionCount = 3;
            pullBand.useWorldSpace = true;
            pullBand.material = new Material(Shader.Find("Sprites/Default"));
            pullBand.startColor = new Color(1f, 1f, 1f, 0.6f);
            pullBand.endColor = new Color(1f, 1f, 1f, 0.6f);
            pullBand.enabled = false;
        }
    }

    private void Update()
    {
        if (ball == null || ball.IsMoving || ball.IsInHole || !IsAimingEnabled)
        {
            HideAimVisuals();
            currentMode = TouchMode.None;
            return;
        }

        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        // 1. Mobile Touch
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                OnPointerDown(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                OnPointerMove(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                OnPointerUp();
            }
        }
        // 2. Mouse Input (for Editor / Mac)
        else if (Input.GetMouseButtonDown(0))
        {
            OnPointerDown(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && currentMode != TouchMode.None)
        {
            OnPointerMove(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0) && currentMode != TouchMode.None)
        {
            OnPointerUp();
        }
    }

    private void OnPointerDown(Vector2 screenPos)
    {
        lastPointerPos = screenPos;

        if (ball == null || mainCamera == null) return;

        Vector3 ballScreenPos = mainCamera.WorldToScreenPoint(ball.transform.position);
        float distToBall = Vector2.Distance(screenPos, new Vector2(ballScreenPos.x, ballScreenPos.y));

        // Touching directly on the ball activates the Slingshot
        if (distToBall <= ballTouchRadiusPx)
        {
            currentMode = TouchMode.Slingshot;
            touchStartPos = screenPos;
            CurrentPowerRatio = 0f;

            // Lock camera forward and right directions at the moment aiming begins
            lockedCamForward = mainCamera.transform.forward;
            lockedCamForward.y = 0f;
            lockedCamForward.Normalize();

            lockedCamRight = mainCamera.transform.right;
            lockedCamRight.y = 0f;
            lockedCamRight.Normalize();
        }
        else
        {
            // Touching anywhere else orbits the camera view
            currentMode = TouchMode.Orbit;
        }
    }

    private void OnPointerMove(Vector2 currentScreenPos)
    {
        if (currentMode == TouchMode.Orbit)
        {
            Vector2 delta = currentScreenPos - lastPointerPos;
            lastPointerPos = currentScreenPos;

            if (cameraController != null)
            {
                cameraController.OnOrbitDrag(delta);
            }
            return;
        }

        if (currentMode == TouchMode.Slingshot)
        {
            // In Unity screen coords, (0,0) is bottom-left
            // Dragging down: touchStartPos.y - currentScreenPos.y > 0
            // Dragging left: touchStartPos.x - currentScreenPos.x > 0
            Vector2 dragDelta = touchStartPos - currentScreenPos;
            float dragDistance = dragDelta.magnitude;

            if (dragDistance > 10f)
            {
                CurrentPowerRatio = Mathf.Clamp01(dragDistance / maxDragDistance);

                Vector2 normDrag = dragDelta.normalized;
                // Pulling back (down on screen) shoots forward (+lockedCamForward)
                // Pulling left shoots right (+lockedCamRight)
                Vector3 worldDir = (lockedCamForward * normDrag.y) + (lockedCamRight * normDrag.x);
                worldDir.y = 0f;

                if (worldDir.sqrMagnitude > 0.01f)
                {
                    CurrentAimDirection = worldDir.normalized;
                }

                UpdateAimVisuals();
            }
            else
            {
                CurrentPowerRatio = 0f;
                HideAimVisuals();
            }
        }
    }

    private void OnPointerUp()
    {
        if (currentMode == TouchMode.Slingshot)
        {
            HideAimVisuals();

            if (CurrentPowerRatio >= 0.05f && ball != null && !ball.IsMoving && !ball.IsInHole)
            {
                ball.Shoot(CurrentAimDirection, CurrentPowerRatio);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RegisterStroke();
                }
            }
        }

        currentMode = TouchMode.None;
        CurrentPowerRatio = 0f;
    }

    private void UpdateAimVisuals()
    {
        if (ball == null) return;

        Vector3 ballPos = ball.transform.position + Vector3.up * 0.04f;
        float aimLen = Mathf.Lerp(minAimLength, maxAimLength, CurrentPowerRatio);
        Vector3 targetPos = ballPos + CurrentAimDirection * aimLen;

        // Dynamic Color: Lime Green -> Yellow -> Bright Red
        Color aimColor;
        if (CurrentPowerRatio < 0.4f)
        {
            aimColor = Color.Lerp(new Color(0.2f, 0.95f, 0.35f), new Color(1f, 0.9f, 0.15f), CurrentPowerRatio / 0.4f);
        }
        else
        {
            aimColor = Color.Lerp(new Color(1f, 0.9f, 0.15f), new Color(1f, 0.18f, 0.2f), (CurrentPowerRatio - 0.4f) / 0.6f);
        }

        // 1. Aim Line
        if (aimLine != null)
        {
            aimLine.enabled = true;
            aimLine.startColor = aimColor;
            aimLine.endColor = new Color(aimColor.r, aimColor.g, aimColor.b, 0.1f);
            aimLine.SetPosition(0, ballPos);
            aimLine.SetPosition(1, targetPos);
        }

        // 2. Slingshot Elastic Band behind ball
        if (pullBand != null)
        {
            pullBand.enabled = true;
            Vector3 bandLeft = ballPos - CurrentAimDirection * 0.2f + Vector3.Cross(Vector3.up, CurrentAimDirection) * 0.25f;
            Vector3 bandRight = ballPos - CurrentAimDirection * 0.2f - Vector3.Cross(Vector3.up, CurrentAimDirection) * 0.25f;
            Vector3 pullPoint = ballPos - CurrentAimDirection * (0.3f + CurrentPowerRatio * 0.8f);

            pullBand.SetPosition(0, bandLeft);
            pullBand.SetPosition(1, pullPoint);
            pullBand.SetPosition(2, bandRight);
            pullBand.startColor = aimColor;
            pullBand.endColor = aimColor;
        }

        // 3. Power Badge UI
        if (powerBadgeObj != null)
        {
            powerBadgeObj.SetActive(true);
            if (powerBadgeText != null)
            {
                powerBadgeText.text = $"%{(int)(CurrentPowerRatio * 100f)} GÜÇ";
                powerBadgeText.color = aimColor;
            }
        }
    }

    private void HideAimVisuals()
    {
        if (aimLine != null) aimLine.enabled = false;
        if (pullBand != null) pullBand.enabled = false;
        if (powerBadgeObj != null) powerBadgeObj.SetActive(false);
    }
}
