using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class SceneSetup
{
    [MenuItem("MiniGolf/Build Game Scene")]
    public static void BuildGameScene()
    {
        Debug.Log("[SceneSetup] Starting MiniGolf scene construction...");

        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 1. Lighting & Environment
        CreateLightingAndEnvironment();

        // 2. Sound Manager
        GameObject soundGo = new GameObject("SoundManager");
        SoundManager soundMgr = soundGo.AddComponent<SoundManager>();

        // 3. Course Builder
        GameObject builderGo = new GameObject("CourseBuilder");
        CourseBuilder courseBuilder = builderGo.AddComponent<CourseBuilder>();

        // 4. Golf Ball with Trail
        GameObject ballGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        ballGo.name = "GolfBall";
        ballGo.transform.position = new Vector3(0f, 0.15f, -6f);
        ballGo.transform.localScale = Vector3.one * 0.24f; // 24cm diameter

        // Material for Golf Ball
        Material ballMat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"))
        {
            color = Color.white
        };
        ballGo.GetComponent<Renderer>().material = ballMat;
        GolfBall golfBall = ballGo.AddComponent<GolfBall>();
        ballGo.AddComponent<BallTrail>();

        // 5. Camera
        GameObject camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        Camera cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.42f, 0.70f, 0.88f); // Soft horizon blue
        cam.fieldOfView = 55;
        camGo.AddComponent<AudioListener>();

        CameraController camCtrl = camGo.AddComponent<CameraController>();
        SetPrivateField(camCtrl, "targetBall", golfBall);

        // 6. UI Canvas (HUD + Slingshot Power Badge)
        CreateUI(out UIManager uiManager, out Text powerBadgeText, out Image powerBadgeBg, out GameObject powerBadgeObj);

        // 7. Slingshot Controller
        GameObject slingGo = new GameObject("SlingshotController");
        SlingshotController slingCtrl = slingGo.AddComponent<SlingshotController>();
        SetPrivateField(slingCtrl, "ball", golfBall);
        SetPrivateField(slingCtrl, "mainCamera", cam);
        SetPrivateField(slingCtrl, "powerBadgeText", powerBadgeText);
        SetPrivateField(slingCtrl, "powerBadgeBg", powerBadgeBg);
        SetPrivateField(slingCtrl, "powerBadgeObj", powerBadgeObj);

        SetPrivateField(camCtrl, "slingshotController", slingCtrl);

        // 8. GameManager
        GameObject gmGo = new GameObject("GameManager");
        GameManager gm = gmGo.AddComponent<GameManager>();
        SetPrivateField(gm, "ball", golfBall);
        SetPrivateField(gm, "slingshotController", slingCtrl);
        SetPrivateField(gm, "cameraController", camCtrl);
        SetPrivateField(gm, "courseBuilder", courseBuilder);
        SetPrivateField(gm, "uiManager", uiManager);

        // Save Scene
        string scenePath = "Assets/Scenes/MainScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        Debug.Log($"[SceneSetup] Scene saved successfully to {scenePath}!");

        // Add to Build Settings
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = scenes;

        // Configure PlayerSettings for iOS Mobile
        PlayerSettings.companyName = "HalilStudio";
        PlayerSettings.productName = "Mini Golf 3D";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.halil.minigolf3d");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
        PlayerSettings.iOS.targetOSVersionString = "15.0";

        Debug.Log("[SceneSetup] PlayerSettings configured for iOS.");
    }

    private static void CreateLightingAndEnvironment()
    {
        // Directional Sun
        GameObject lightGo = new GameObject("Directional Light");
        Light light = lightGo.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.98f, 0.94f);
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.48f, 0.52f, 0.58f);

        // Backdrop Infinite Ground (surrounding grass plane)
        GameObject backdrop = GameObject.CreatePrimitive(PrimitiveType.Plane);
        backdrop.name = "SurroundingGround";
        backdrop.transform.position = new Vector3(0f, -0.15f, 0f);
        backdrop.transform.localScale = Vector3.one * 15f;
        Material bgMat = new Material(Shader.Find("Standard") ?? Shader.Find("Diffuse"))
        {
            color = new Color(0.12f, 0.44f, 0.20f)
        };
        backdrop.GetComponent<Renderer>().material = bgMat;
        GameObject.DestroyImmediate(backdrop.GetComponent<Collider>());
    }

    private static void CreateUI(out UIManager uiManager, out Text powerBadgeText, out Image powerBadgeBg, out GameObject powerBadgeObj)
    {
        // Canvas
        GameObject canvasGo = new GameObject("Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGo.AddComponent<CanvasScaler>();
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        // EventSystem
        GameObject esGo = new GameObject("EventSystem");
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // UIManager Root
        GameObject uiMgrGo = new GameObject("UIManager");
        uiMgrGo.transform.SetParent(canvasGo.transform, false);
        uiManager = uiMgrGo.AddComponent<UIManager>();

        // 1. Top HUD Panel (Golf Battle Style sleek dark pill)
        GameObject topHud = CreatePanel(canvasGo, "TopHUD", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -90), new Vector2(980, 140));
        topHud.GetComponent<Image>().color = new Color(0.04f, 0.10f, 0.06f, 0.82f);

        Text holeText = CreateText(topHud, "HoleText", "1. Çam Vadisi (Turbo)", 38, TextAnchor.MiddleLeft, new Vector2(0.06f, 0.5f), new Vector2(0.06f, 0.5f), new Vector2(250, 0), new Vector2(480, 80));
        holeText.color = new Color(1f, 0.92f, 0.35f);

        Text parText = CreateText(topHud, "ParText", "PAR: 2", 36, TextAnchor.MiddleCenter, new Vector2(0.62f, 0.5f), new Vector2(0.62f, 0.5f), Vector2.zero, new Vector2(180, 70));
        parText.color = Color.white;

        Text strokeText = CreateText(topHud, "StrokeText", "VURUŞ: 0", 38, TextAnchor.MiddleRight, new Vector2(0.94f, 0.5f), new Vector2(0.94f, 0.5f), new Vector2(-120, 0), new Vector2(220, 70));
        strokeText.color = new Color(0.35f, 0.95f, 0.45f);

        // Notification Banner
        Text notifText = CreateText(canvasGo, "NotificationText", "⚠️ SAHA DIŞI! (+1 Ceza)", 44, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.75f), Vector2.zero, new Vector2(800, 100));
        notifText.color = new Color(1f, 0.3f, 0.2f);
        notifText.gameObject.SetActive(false);

        // 2. Floating Slingshot Power Badge
        powerBadgeObj = CreatePanel(canvasGo, "PowerBadge", new Vector2(0.5f, 0.22f), new Vector2(0.5f, 0.22f), Vector2.zero, new Vector2(380, 75));
        powerBadgeBg = powerBadgeObj.GetComponent<Image>();
        powerBadgeBg.color = new Color(0.06f, 0.08f, 0.10f, 0.88f);

        powerBadgeText = CreateText(powerBadgeObj, "PowerText", "%0 GÜÇ", 38, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        powerBadgeText.color = Color.white;

        // Bottom Instruction hint
        GameObject hintGo = CreatePanel(canvasGo, "ControlHint", new Vector2(0.5f, 0.06f), new Vector2(0.5f, 0.06f), Vector2.zero, new Vector2(750, 60));
        hintGo.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.06f, 0.75f);
        Text hintText = CreateText(hintGo, "Text", "👆 Topu geriye çekip bırakarak vuruş yap!", 32, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        hintText.color = new Color(0.85f, 0.95f, 0.9f);

        // 3. Result Modal Panel
        GameObject resultPanel = CreatePanel(canvasGo, "ResultPanel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(850, 750));
        resultPanel.GetComponent<Image>().color = new Color(0.06f, 0.14f, 0.1f, 0.95f);

        Text resTitle = CreateText(resultPanel, "Title", "TEBRİKLER! ⛳", 54, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.82f), new Vector2(0.5f, 0.82f), Vector2.zero, new Vector2(700, 90));
        resTitle.color = new Color(1f, 0.9f, 0.2f);

        Text resRating = CreateText(resultPanel, "Rating", "⭐⭐⭐", 62, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.64f), new Vector2(0.5f, 0.64f), Vector2.zero, new Vector2(600, 80));
        resRating.color = new Color(1f, 0.85f, 0.1f);

        Text resScore = CreateText(resultPanel, "Score", "2 Vuruşta Tamamlandı\n(Par: 2)", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.44f), new Vector2(0.5f, 0.44f), Vector2.zero, new Vector2(700, 120));
        resScore.color = Color.white;

        GameObject nextBtnGo = CreatePanel(resultPanel, "NextButton", new Vector2(0.5f, 0.18f), new Vector2(0.5f, 0.18f), Vector2.zero, new Vector2(600, 90));
        nextBtnGo.GetComponent<Image>().color = new Color(0.2f, 0.75f, 0.35f);
        Button nextBtn = nextBtnGo.AddComponent<Button>();

        Text nextBtnText = CreateText(nextBtnGo, "Text", "SONRAKİ DELİK ➡️", 40, TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(550, 70));
        nextBtnText.color = Color.white;

        // Connect UIManager fields
        SetPrivateField(uiManager, "holeNameText", holeText);
        SetPrivateField(uiManager, "strokeText", strokeText);
        SetPrivateField(uiManager, "parText", parText);
        SetPrivateField(uiManager, "notificationText", notifText);
        SetPrivateField(uiManager, "resultPanel", resultPanel);
        SetPrivateField(uiManager, "resultTitleText", resTitle);
        SetPrivateField(uiManager, "resultScoreText", resScore);
        SetPrivateField(uiManager, "resultRatingText", resRating);
        SetPrivateField(uiManager, "nextHoleButton", nextBtn);
        SetPrivateField(uiManager, "nextButtonText", nextBtnText);
    }

    private static GameObject CreatePanel(GameObject parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        Image img = go.AddComponent<Image>();
        img.color = Color.white;
        return go;
    }

    private static Text CreateText(GameObject parent, string name, string content, int fontSize, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.fontSize = fontSize;
        txt.alignment = alignment;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.raycastTarget = false;
        return txt;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
        if (field != null)
        {
            field.SetValue(target, value);
        }
    }
}
