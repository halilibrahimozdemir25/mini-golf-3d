using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class GolfHole : MonoBehaviour
{
    [SerializeField] private float pullRadius = 0.5f;
    [SerializeField] private float pullStrength = 8f;
    [SerializeField] private float maxCaptureSpeed = 7f;

    private SphereCollider triggerCol;

    private void Awake()
    {
        triggerCol = GetComponent<SphereCollider>();
        triggerCol.isTrigger = true;
        triggerCol.radius = pullRadius;
    }

    private void OnTriggerStay(Collider other)
    {
        GolfBall ball = other.GetComponent<GolfBall>();
        if (ball != null && !ball.IsInHole)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 toHole = transform.position - other.transform.position;
                toHole.y = 0f;
                float dist = toHole.magnitude;

                // Gravitational suction toward center of hole
                if (dist > 0.02f)
                {
                    rb.AddForce(toHole.normalized * pullStrength * (1f - dist / pullRadius), ForceMode.Acceleration);
                }

                // If within cup radius and speed is below capture threshold
                if (dist < 0.18f)
                {
                    if (rb.linearVelocity.magnitude <= maxCaptureSpeed)
                    {
                        ball.EnterHole(transform.position);
                    }
                }
            }
        }
    }
}
