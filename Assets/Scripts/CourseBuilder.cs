using System.Collections.Generic;
using UnityEngine;

public class CourseBuilder : MonoBehaviour
{
    public static CourseBuilder Instance { get; private set; }

    [Header("Course Materials")]
    private Material grassMat;
    private Material wallMat;
    private Material holeMat;
    private Material flagMat;
    private Material poleMat;
    private Material sandMat;
    private Material boosterMat;
    private Material bumperMat;
    private Material woodMat;
    private Material treeMat;
    private Material trunkMat;

    private GameObject currentCourseRoot;

    public struct HoleData
    {
        public int holeNumber;
        public string holeName;
        public int par;
        public Vector3 ballSpawn;
        public Vector3 cupPosition;
    }

    private readonly List<HoleData> holeDefinitions = new List<HoleData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        CreateMaterials();
        InitHoleDefinitions();
    }

    private void CreateMaterials()
    {
        Shader litShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Diffuse");

        grassMat = new Material(litShader) { color = new Color(0.16f, 0.72f, 0.28f) };
        grassMat.SetFloat("_Glossiness", 0.1f);

        wallMat = new Material(litShader) { color = new Color(0.96f, 0.96f, 0.98f) };
        wallMat.SetFloat("_Glossiness", 0.45f);

        holeMat = new Material(litShader) { color = new Color(0.06f, 0.06f, 0.06f) };
        flagMat = new Material(litShader) { color = new Color(0.98f, 0.18f, 0.22f) };
        poleMat = new Material(litShader) { color = new Color(0.9f, 0.9f, 0.9f) };

        // Golf Battle Theme Materials
        sandMat = new Material(litShader) { color = new Color(0.92f, 0.82f, 0.52f) };
        boosterMat = new Material(litShader) { color = new Color(0.1f, 0.95f, 1f) };
        bumperMat = new Material(litShader) { color = new Color(1f, 0.25f, 0.7f) };
        woodMat = new Material(litShader) { color = new Color(0.55f, 0.35f, 0.18f) };
        treeMat = new Material(litShader) { color = new Color(0.12f, 0.5f, 0.2f) };
        trunkMat = new Material(litShader) { color = new Color(0.42f, 0.26f, 0.14f) };
    }

    private void InitHoleDefinitions()
    {
        holeDefinitions.Clear();

        // Hole 1: Pine Valley - Turbo Strip Jump
        holeDefinitions.Add(new HoleData
        {
            holeNumber = 1,
            holeName = "1. Çam Vadisi (Turbo Şeridi)",
            par = 2,
            ballSpawn = new Vector3(0f, 0.15f, -6f),
            cupPosition = new Vector3(0f, 0f, 6.5f)
        });

        // Hole 2: Windmill Dunes - Rotating Rotor & Sand
        holeDefinitions.Add(new HoleData
        {
            holeNumber = 2,
            holeName = "2. Değirmen & Kum Tuzağı",
            par = 3,
            ballSpawn = new Vector3(-3.5f, 0.15f, -6f),
            cupPosition = new Vector3(3.5f, 0f, 7.5f)
        });

        // Hole 3: Skyline Bounce - Jump & Pinball Bumpers
        holeDefinitions.Add(new HoleData
        {
            holeNumber = 3,
            holeName = "3. Zirve & Pinball Tamponları",
            par = 3,
            ballSpawn = new Vector3(0f, 0.15f, -6f),
            cupPosition = new Vector3(0f, 0.55f, 8.5f)
        });
    }

    public int TotalHoles => holeDefinitions.Count;

    public HoleData GetHoleData(int index)
    {
        return holeDefinitions[Mathf.Clamp(index, 0, holeDefinitions.Count - 1)];
    }

    public GameObject BuildHole(int index, out Vector3 ballSpawnPos, out GolfHole holeComponent)
    {
        if (currentCourseRoot != null)
        {
            Destroy(currentCourseRoot);
        }

        currentCourseRoot = new GameObject($"Course_Hole_{index + 1}");
        HoleData data = GetHoleData(index);
        ballSpawnPos = data.ballSpawn;

        switch (index)
        {
            case 0:
                BuildHole1(currentCourseRoot, data.cupPosition, out holeComponent);
                break;
            case 1:
                BuildHole2(currentCourseRoot, data.cupPosition, out holeComponent);
                break;
            case 2:
            default:
                BuildHole3(currentCourseRoot, data.cupPosition, out holeComponent);
                break;
        }

        // Add scenic environment props around each hole (Pine trees & rocks)
        SpawnEnvironmentProps(currentCourseRoot, index);

        return currentCourseRoot;
    }

    private void BuildHole1(GameObject root, Vector3 cupPos, out GolfHole holeComp)
    {
        // Straight fairway: 3.6m wide, 16m long
        CreateFloor(root, new Vector3(0f, 0f, 0f), new Vector3(3.6f, 0.2f, 16f));

        // Side Walls
        CreateWall(root, new Vector3(-1.9f, 0.3f, 0f), new Vector3(0.2f, 0.5f, 16.2f));
        CreateWall(root, new Vector3(1.9f, 0.3f, 0f), new Vector3(0.2f, 0.5f, 16.2f));
        CreateWall(root, new Vector3(0f, 0.3f, -8.1f), new Vector3(3.8f, 0.5f, 0.2f));
        CreateWall(root, new Vector3(0f, 0.3f, 8.1f), new Vector3(3.8f, 0.5f, 0.2f));

        // Speed Booster pad right in the center!
        CreateSpeedBooster(root, new Vector3(0f, 0.11f, -1.0f), new Vector3(1.2f, 0.04f, 2.5f), Vector3.forward, 20f);

        // Sand Trap hazard on the right side
        CreateSandTrap(root, new Vector3(1.0f, 0.11f, 2.5f), new Vector3(1.4f, 0.05f, 3.0f));

        // Gentle chicane bumper on the left
        CreateBumper(root, new Vector3(-1.0f, 0.35f, 2.5f), 0.7f);

        holeComp = CreateCupAndFlag(root, cupPos);
    }

    private void BuildHole2(GameObject root, Vector3 cupPos, out GolfHole holeComp)
    {
        // 1. Left Fairway (Spawn segment)
        CreateFloor(root, new Vector3(-3.5f, 0f, -2.5f), new Vector3(3.4f, 0.2f, 9.0f));
        CreateWall(root, new Vector3(-5.2f, 0.3f, -1.5f), new Vector3(0.2f, 0.5f, 11.2f));
        CreateWall(root, new Vector3(-3.5f, 0.3f, -7.1f), new Vector3(3.6f, 0.5f, 0.2f));
        CreateWall(root, new Vector3(-1.8f, 0.3f, -3.0f), new Vector3(0.2f, 0.5f, 8.0f));

        // 2. Middle Connector (Corner turn)
        CreateFloor(root, new Vector3(0f, 0f, 2.5f), new Vector3(10.4f, 0.2f, 3.4f));
        CreateWall(root, new Vector3(-1.7f, 0.3f, 4.2f), new Vector3(7.0f, 0.5f, 0.2f)); // Stops at x=1.8 to leave green wide open!
        CreateWall(root, new Vector3(0f, 0.3f, 0.9f), new Vector3(3.8f, 0.5f, 0.2f));

        // Sand Trap inside the turn corner
        CreateSandTrap(root, new Vector3(-1.0f, 0.11f, 2.5f), new Vector3(1.6f, 0.05f, 1.6f));

        // Rotating Windmill Obstacle in the corner!
        CreateRotatingBarrier(root, new Vector3(0.5f, 0.4f, 2.5f), 55f);

        // 3. Right Fairway (Green segment leading to Hole)
        CreateFloor(root, new Vector3(3.5f, 0f, 6.0f), new Vector3(3.4f, 0.2f, 7.0f));
        CreateWall(root, new Vector3(5.2f, 0.3f, 5.25f), new Vector3(0.2f, 0.5f, 8.9f));
        CreateWall(root, new Vector3(1.8f, 0.3f, 6.85f), new Vector3(0.2f, 0.5f, 5.5f));
        CreateWall(root, new Vector3(3.5f, 0.3f, 9.6f), new Vector3(3.6f, 0.5f, 0.2f));

        // Speed Booster guiding into the green
        CreateSpeedBooster(root, new Vector3(3.5f, 0.11f, 4.2f), new Vector3(1.2f, 0.04f, 1.8f), Vector3.forward, 16f);

        holeComp = CreateCupAndFlag(root, cupPos);
    }

    private void BuildHole3(GameObject root, Vector3 cupPos, out GolfHole holeComp)
    {
        // 1. Lower fairway
        CreateFloor(root, new Vector3(0f, 0f, -4f), new Vector3(3.8f, 0.2f, 7.0f));
        CreateWall(root, new Vector3(-2.0f, 0.35f, -4f), new Vector3(0.2f, 0.6f, 7.2f));
        CreateWall(root, new Vector3(2.0f, 0.35f, -4f), new Vector3(0.2f, 0.6f, 7.2f));
        CreateWall(root, new Vector3(0f, 0.35f, -7.6f), new Vector3(4.2f, 0.6f, 0.2f));

        // Controlled Speed booster - provides smooth glide without launching off course
        CreateSpeedBooster(root, new Vector3(0f, 0.11f, -2.2f), new Vector3(1.4f, 0.04f, 2.0f), Vector3.forward, 13.5f);

        // 2. Gentle Incline Ramp (Transitions smoothly from y=0.1 to y=0.65 with zero lip or step)
        GameObject ramp = CreateFloor(root, new Vector3(0f, 0.275f, 2.0f), new Vector3(3.8f, 0.2f, 5.1f));
        ramp.transform.localRotation = Quaternion.Euler(-6.28f, 0f, 0f);

        GameObject rWallLeft = CreateWall(root, new Vector3(-2.0f, 0.62f, 2.0f), new Vector3(0.2f, 0.7f, 5.1f));
        rWallLeft.transform.localRotation = Quaternion.Euler(-6.28f, 0f, 0f);

        GameObject rWallRight = CreateWall(root, new Vector3(2.0f, 0.62f, 2.0f), new Vector3(0.2f, 0.7f, 5.1f));
        rWallRight.transform.localRotation = Quaternion.Euler(-6.28f, 0f, 0f);

        // 3. Upper green plateau (Seamlessly flush at y=0.65)
        CreateFloor(root, new Vector3(0f, 0.55f, 7.45f), new Vector3(4.6f, 0.2f, 6.2f));
        CreateWall(root, new Vector3(-2.4f, 0.95f, 7.45f), new Vector3(0.2f, 0.8f, 6.4f));
        CreateWall(root, new Vector3(2.4f, 0.95f, 7.45f), new Vector3(0.2f, 0.8f, 6.4f));
        CreateWall(root, new Vector3(0f, 1.05f, 10.6f), new Vector3(5.0f, 1.0f, 0.2f)); // Extra tall back wall to stop wild bounces!

        // Corner connector walls
        CreateWall(root, new Vector3(-2.2f, 0.95f, 4.35f), new Vector3(0.5f, 0.8f, 0.2f));
        CreateWall(root, new Vector3(2.2f, 0.95f, 4.35f), new Vector3(0.5f, 0.8f, 0.2f));

        // Two Pinball Bumper pillars flanking the green for bank shots
        CreateBumper(root, new Vector3(-1.4f, 0.85f, 6.5f), 0.7f);
        CreateBumper(root, new Vector3(1.4f, 0.85f, 6.5f), 0.7f);

        holeComp = CreateCupAndFlag(root, cupPos);
    }

    private GameObject CreateFloor(GameObject parent, Vector3 pos, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Floor";
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().material = grassMat;

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.material = new PhysicsMaterial("CourseGrass")
        {
            dynamicFriction = 0.16f,
            staticFriction = 0.20f,
            bounciness = 0.25f
        };

        return go;
    }

    private GameObject CreateWall(GameObject parent, Vector3 pos, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Wall";
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().material = wallMat;

        BoxCollider col = go.GetComponent<BoxCollider>();
        col.material = new PhysicsMaterial("CourseWall")
        {
            dynamicFriction = 0.05f,
            staticFriction = 0.05f,
            bounciness = 0.80f,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        return go;
    }

    private void CreateSpeedBooster(GameObject parent, Vector3 pos, Vector3 size, Vector3 direction, float speed)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "SpeedBooster";
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().material = boosterMat;

        SpeedBooster booster = go.AddComponent<SpeedBooster>();
    }

    private void CreateSandTrap(GameObject parent, Vector3 pos, Vector3 size)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "SandTrap";
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().material = sandMat;

        go.AddComponent<SandTrap>();
    }

    private void CreateBumper(GameObject parent, Vector3 pos, float diameter)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        go.name = "PinballBumper";
        go.transform.SetParent(parent.transform);
        go.transform.position = pos;
        go.transform.localScale = new Vector3(diameter, 0.45f, diameter);
        go.GetComponent<Renderer>().material = bumperMat;

        go.AddComponent<BumperPillar>();
    }

    private void CreateRotatingBarrier(GameObject parent, Vector3 pos, float speed)
    {
        GameObject rotorObj = new GameObject("RotatingBarrier");
        rotorObj.transform.SetParent(parent.transform);
        rotorObj.transform.position = pos;

        // Base post
        GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        post.transform.SetParent(rotorObj.transform);
        post.transform.localPosition = Vector3.zero;
        post.transform.localScale = new Vector3(0.35f, 0.4f, 0.35f);
        post.GetComponent<Renderer>().material = woodMat;

        // Rotating arm
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arm.transform.SetParent(rotorObj.transform);
        arm.transform.localPosition = new Vector3(0f, 0.25f, 0f);
        arm.transform.localScale = new Vector3(2.4f, 0.35f, 0.25f);
        arm.GetComponent<Renderer>().material = woodMat;

        RotatingObstacle rot = rotorObj.AddComponent<RotatingObstacle>();
    }

    private GolfHole CreateCupAndFlag(GameObject parent, Vector3 cupPos)
    {
        GameObject cupObj = new GameObject("GolfCup");
        cupObj.transform.SetParent(parent.transform);
        cupObj.transform.position = cupPos;

        // Visual black disc for hole
        GameObject cupVisual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cupVisual.name = "CupVisual";
        cupVisual.transform.SetParent(cupObj.transform);
        cupVisual.transform.position = cupPos + Vector3.up * 0.11f;
        cupVisual.transform.localScale = new Vector3(0.38f, 0.02f, 0.38f);
        cupVisual.GetComponent<Renderer>().material = holeMat;
        Destroy(cupVisual.GetComponent<Collider>());

        // Flag Pole
        GameObject pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "FlagPole";
        pole.transform.SetParent(cupObj.transform);
        pole.transform.position = cupPos + new Vector3(0f, 1.1f, 0f);
        pole.transform.localScale = new Vector3(0.04f, 1f, 0.04f);
        pole.GetComponent<Renderer>().material = poleMat;
        Destroy(pole.GetComponent<Collider>());

        // Red Flag pennant
        GameObject flag = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flag.name = "FlagPennant";
        flag.transform.SetParent(cupObj.transform);
        flag.transform.position = cupPos + new Vector3(0.24f, 1.9f, 0f);
        flag.transform.localScale = new Vector3(0.44f, 0.28f, 0.02f);
        flag.GetComponent<Renderer>().material = flagMat;
        Destroy(flag.GetComponent<Collider>());

        GolfHole golfHole = cupObj.AddComponent<GolfHole>();
        return golfHole;
    }

    private void SpawnEnvironmentProps(GameObject parent, int holeIdx)
    {
        // Stylized low-poly pine trees and rock props along course periphery
        Vector3[] treePositions = new Vector3[]
        {
            new Vector3(-4.5f, 0f, -4f),
            new Vector3(-4.2f, 0f, 4f),
            new Vector3(4.5f, 0f, -2f),
            new Vector3(4.8f, 0f, 5f),
            new Vector3(-3.0f, 0f, 8f),
            new Vector3(3.2f, 0f, 11f)
        };

        foreach (var pos in treePositions)
        {
            SpawnPineTree(parent, pos);
        }
    }

    private void SpawnPineTree(GameObject parent, Vector3 basePos)
    {
        GameObject tree = new GameObject("PineTree");
        tree.transform.SetParent(parent.transform);
        tree.transform.position = basePos;

        // Trunk
        GameObject trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.SetParent(tree.transform);
        trunk.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        trunk.transform.localScale = new Vector3(0.25f, 0.5f, 0.25f);
        trunk.GetComponent<Renderer>().material = trunkMat;
        Destroy(trunk.GetComponent<Collider>());

        // Cone Foliage 1
        GameObject cone1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone1.transform.SetParent(tree.transform);
        cone1.transform.localPosition = new Vector3(0f, 1.3f, 0f);
        cone1.transform.localScale = new Vector3(1.1f, 0.5f, 1.1f);
        cone1.GetComponent<Renderer>().material = treeMat;
        Destroy(cone1.GetComponent<Collider>());

        // Cone Foliage 2
        GameObject cone2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cone2.transform.SetParent(tree.transform);
        cone2.transform.localPosition = new Vector3(0f, 1.9f, 0f);
        cone2.transform.localScale = new Vector3(0.75f, 0.45f, 0.75f);
        cone2.GetComponent<Renderer>().material = treeMat;
        Destroy(cone2.GetComponent<Collider>());
    }
}
