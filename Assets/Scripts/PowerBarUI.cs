using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerBarUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("UI Elements")]
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image fillImage;
    [SerializeField] private Text powerText;
    [SerializeField] private Text instructionText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform barRect;

    [Header("References")]
    [SerializeField] private GolfBall ball;
    [SerializeField] private AimController aimController;

    private float currentPower = 0f;
    private bool isDragging = false;

    public float CurrentPowerRatio => currentPower;

    private void Awake()
    {
        if (barRect == null)
        {
            barRect = GetComponent<RectTransform>();
        }

        if (powerSlider != null)
        {
            powerSlider.minValue = 0f;
            powerSlider.maxValue = 1f;
            powerSlider.value = 0f;
        }
    }

    private void Start()
    {
        if (ball != null)
        {
            ball.OnBallStopped += OnBallStopped;
        }

        currentPower = 0f;
        UpdateUI(false);
    }

    private void OnDestroy()
    {
        if (ball != null)
        {
            ball.OnBallStopped -= OnBallStopped;
        }
    }

    private void Update()
    {
        if (ball == null) return;

        bool canShoot = !ball.IsMoving && !ball.IsInHole;
        SetVisible(canShoot);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ball == null || ball.IsMoving || ball.IsInHole) return;

        isDragging = true;
        UpdatePowerFromPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || ball == null || ball.IsMoving || ball.IsInHole) return;

        UpdatePowerFromPointer(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        // If dragged enough power, release and shoot automatically!
        if (currentPower >= 0.05f && ball != null && !ball.IsMoving && !ball.IsInHole)
        {
            ShootNow();
        }
        else
        {
            // Cancelled or too low
            currentPower = 0f;
            UpdateUI(false);
        }
    }

    private void UpdatePowerFromPointer(PointerEventData eventData)
    {
        RectTransform targetRect = powerSlider != null ? powerSlider.GetComponent<RectTransform>() : barRect;
        if (targetRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(targetRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            float width = targetRect.rect.width;
            // localPoint.x ranges from -width/2 to +width/2
            float normalized = Mathf.Clamp01((localPoint.x + (width * 0.5f)) / width);

            // Minimum practical power when touched is 5%
            currentPower = normalized > 0.02f ? Mathf.Max(0.05f, normalized) : 0f;
            UpdateUI(true);
        }
    }

    private void UpdateUI(bool isCharging)
    {
        if (powerSlider != null)
        {
            powerSlider.value = currentPower;
        }

        if (fillImage != null)
        {
            // Color gradient from Green (soft) to Yellow (medium) to Deep Red (max)
            if (currentPower < 0.5f)
            {
                fillImage.color = Color.Lerp(new Color(0.2f, 0.85f, 0.35f), new Color(1f, 0.85f, 0.1f), currentPower * 2f);
            }
            else
            {
                fillImage.color = Color.Lerp(new Color(1f, 0.85f, 0.1f), new Color(0.95f, 0.2f, 0.15f), (currentPower - 0.5f) * 2f);
            }
        }

        if (powerText != null)
        {
            if (isCharging && currentPower >= 0.05f)
            {
                powerText.text = $"HIZ: %{(int)(currentPower * 100f)}  (BIRAKINCA VURUR)";
            }
            else
            {
                powerText.text = "HIZ: %0  (SÜRÜKLE VE BIRAK)";
            }
        }

        if (instructionText != null)
        {
            instructionText.text = isCharging 
                ? "🎯 İstediğin hıza çek ve parmağını bırak!" 
                : "👆 Hızı ayarlamak için çubuğu sağa çek ve bırak";
        }

        if (aimController != null)
        {
            aimController.SetAimLinePower(Mathf.Max(0.1f, currentPower));
        }
    }

    private void ShootNow()
    {
        Vector3 shootDir = aimController != null ? aimController.AimDirection : ball.transform.forward;
        ball.Shoot(shootDir, currentPower);

        // Notify GameManager to increment stroke
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterStroke();
        }

        // Reset power slider back to 0
        currentPower = 0f;
        UpdateUI(false);
    }

    private void OnBallStopped()
    {
        currentPower = 0f;
        UpdateUI(false);

        if (aimController != null)
        {
            aimController.AimAtTarget();
            aimController.SetAimLinePower(0.2f);
        }
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
        }
    }
}
