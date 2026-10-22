using UnityEngine;

public class SandTrap : MonoBehaviour
{
    [SerializeField] private float sandDrag = 0.82f;
    [SerializeField] private float maxSandSpeed = 3.5f;

    private void Awake()
    {
        BoxCollider col = GetComponent<BoxCollider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        GolfBall ball = other.GetComponent<GolfBall>();
        if (ball != null && ball.IsMoving)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySand();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        GolfBall ball = other.GetComponent<GolfBall>();
        if (ball != null && ball.IsMoving)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Rapidly slow the ball down in the sand
                rb.linearVelocity *= sandDrag;

                if (rb.linearVelocity.magnitude > maxSandSpeed)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * maxSandSpeed;
                }
            }
        }
    }
}
