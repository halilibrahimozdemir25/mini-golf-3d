using UnityEngine;

[RequireComponent(typeof(GolfBall), typeof(Rigidbody))]
public class BallTrail : MonoBehaviour
{
    private TrailRenderer trail;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        trail = gameObject.AddComponent<TrailRenderer>();
        trail.time = 0.35f;
        trail.startWidth = 0.16f;
        trail.endWidth = 0.01f;
        trail.material = new Material(Shader.Find("Sprites/Default"));

        // Gradient from bright cyan/yellow to transparent
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 0.95f, 1f), 0.0f), 
                new GradientColorKey(new Color(1f, 0.9f, 0.2f), 0.5f),
                new GradientColorKey(new Color(1f, 0.3f, 0.1f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.85f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        trail.colorGradient = gradient;
        trail.autodestruct = false;
        trail.emitting = false;
    }

    private void Update()
    {
        if (rb != null && trail != null)
        {
            float speed = rb.linearVelocity.magnitude;
            trail.emitting = (speed > 0.4f);
        }
    }
}
