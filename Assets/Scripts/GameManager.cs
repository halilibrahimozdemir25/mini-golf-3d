using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Core References")]
    [SerializeField] private GolfBall ball;
    [SerializeField] private SlingshotController slingshotController;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private CourseBuilder courseBuilder;
    [SerializeField] private UIManager uiManager;

    private int currentHoleIndex = 0;
    private int currentStrokes = 0;
    private int totalStrokes = 0;
    private CourseBuilder.HoleData currentHoleData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    private void Start()
    {
        if (ball != null)
        {
            ball.OnBallInHole += HandleBallInHole;
            ball.OnOutOfBounds += HandleOutOfBounds;
        }

        LoadHole(0);
    }

    private void OnDestroy()
    {
        if (ball != null)
        {
            ball.OnBallInHole -= HandleBallInHole;
            ball.OnOutOfBounds -= HandleOutOfBounds;
        }
    }

    public void LoadHole(int holeIdx)
    {
        currentHoleIndex = holeIdx % courseBuilder.TotalHoles;
        currentStrokes = 0;
        currentHoleData = courseBuilder.GetHoleData(currentHoleIndex);

        // Build the physical 3D hole
        courseBuilder.BuildHole(currentHoleIndex, out Vector3 spawnPos, out GolfHole holeComponent);

        // Reset ball position
        if (ball != null)
        {
            ball.ResetBall(spawnPos);
        }

        // Configure Slingshot and Camera
        if (cameraController != null && holeComponent != null)
        {
            cameraController.SetHoleTarget(holeComponent.transform);
        }

        if (slingshotController != null)
        {
            slingshotController.IsAimingEnabled = true;
        }

        if (uiManager != null)
        {
            uiManager.UpdateHUD(currentHoleData.holeName, currentStrokes, currentHoleData.par);
            uiManager.HideResultModal();
        }
    }

    public void RegisterStroke()
    {
        currentStrokes++;
        totalStrokes++;

        if (uiManager != null)
        {
            uiManager.UpdateHUD(currentHoleData.holeName, currentStrokes, currentHoleData.par);
        }
    }

    private void HandleOutOfBounds()
    {
        currentStrokes++; // Penalty stroke
        totalStrokes++;

        if (uiManager != null)
        {
            uiManager.ShowNotification("⚠️ SAHA DIŞI! (+1 Ceza)", new Color(1f, 0.3f, 0.2f));
            uiManager.UpdateHUD(currentHoleData.holeName, currentStrokes, currentHoleData.par);
        }
    }

    private void HandleBallInHole()
    {
        StartCoroutine(HoleCompletedRoutine());
    }

    private IEnumerator HoleCompletedRoutine()
    {
        if (slingshotController != null)
        {
            slingshotController.IsAimingEnabled = false;
        }

        yield return new WaitForSeconds(1.2f);

        bool isFinal = (currentHoleIndex == courseBuilder.TotalHoles - 1);
        if (uiManager != null)
        {
            uiManager.ShowHoleCompleted(currentStrokes, currentHoleData.par, isFinal);
        }
    }

    public void NextHole()
    {
        int nextIdx = (currentHoleIndex + 1) % courseBuilder.TotalHoles;
        LoadHole(nextIdx);
    }
}
