using UnityEngine;

public class SpeedBooster : MonoBehaviour
{
    [SerializeField] private float boostSpeed = 13.5f;
    [SerializeField] private Vector3 boostDirection = Vector3.forward;

    private Material padMaterial;
    private float pulseTimer = 0f;

    private void Awake()
    {
        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            padMaterial = rend.material;
        }

        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void Update()
    {
        // Animated pulse effect on the booster pad
        pulseTimer += Time.deltaTime * 5f;
        float brightness = 0.7f + Mathf.PingPong(pulseTimer, 0.4f);
        if (padMaterial != null)
        {
            padMaterial.color = new Color(0.1f * brightness, 0.95f * brightness, 1f * brightness);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GolfBall ball = other.GetComponent<GolfBall>();
        if (ball != null && ball.IsMoving)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 worldDir = transform.TransformDirection(boostDirection.normalized);
                worldDir.y = 0f;
                worldDir.Normalize();

                // Apply controlled turbo boost (smoothly bounded so it glides up ramps without flying off map)
                float currentSpeed = rb.linearVelocity.magnitude;
                float boostedSpeed = Mathf.Clamp(Mathf.Max(currentSpeed + 3.5f, boostSpeed), 11.0f, 13.5f);
                rb.linearVelocity = worldDir * boostedSpeed;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBooster();
                }
            }
        }
    }
}
