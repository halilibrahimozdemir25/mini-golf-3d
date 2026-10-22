using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Top HUD Elements")]
    [SerializeField] private Text holeNameText;
    [SerializeField] private Text strokeText;
    [SerializeField] private Text parText;
    [SerializeField] private Text notificationText;

    [Header("Result Modal")]
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private Text resultTitleText;
    [SerializeField] private Text resultScoreText;
    [SerializeField] private Text resultRatingText;
    [SerializeField] private Button nextHoleButton;
    [SerializeField] private Text nextButtonText;

    private float notificationTimer = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (nextHoleButton != null)
        {
            nextHoleButton.onClick.AddListener(OnNextButtonClicked);
        }

        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f && notificationText != null)
            {
                notificationText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateHUD(string holeName, int strokeCount, int par)
    {
        if (holeNameText != null) holeNameText.text = holeName;
        if (strokeText != null) strokeText.text = $"VURUŞ: {strokeCount}";
        if (parText != null) parText.text = $"PAR: {par}";
    }

    public void ShowNotification(string message, Color color)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = color;
            notificationText.gameObject.SetActive(true);
            notificationTimer = 2.0f;
        }
    }

    public void ShowHoleCompleted(int strokes, int par, bool isFinalHole)
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        int diff = strokes - par;
        string title = "TEBRİKLER! ⛳";
        string rating = "⭐⭐⭐";

        if (strokes == 1)
        {
            title = "HOLE IN ONE! 🌟";
            rating = "⭐⭐⭐ (Kusursuz!)";
        }
        else if (diff <= -1)
        {
            title = "BIRDIE! 🦅";
            rating = "⭐⭐⭐";
        }
        else if (diff == 0)
        {
            title = "PAR! 🎯";
            rating = "⭐⭐☆";
        }
        else if (diff == 1)
        {
            title = "BOGEY! 👍";
            rating = "⭐☆☆";
        }
        else
        {
            title = "BİTTİ! ⛳";
            rating = "⭐☆☆";
        }

        if (resultTitleText != null) resultTitleText.text = title;
        if (resultScoreText != null) resultScoreText.text = $"{strokes} Vuruşta Deliğe Sokuldu\n(Par: {par})";
        if (resultRatingText != null) resultRatingText.text = rating;

        if (nextButtonText != null)
        {
            nextButtonText.text = isFinalHole ? "ŞAMPİYONSUN! TEKRAR OYNA 🏆" : "SONRAKİ DELİK ➡️";
        }
    }

    public void HideResultModal()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }

    private void OnNextButtonClicked()
    {
        HideResultModal();
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextHole();
        }
    }
}
