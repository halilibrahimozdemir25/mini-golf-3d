using System.Collections;
using UnityEngine;

public class BumperPillar : MonoBehaviour
{
    [SerializeField] private float bounceMultiplier = 1.4f;
    [SerializeField] private float minBounceVelocity = 10f;

    private Vector3 originalScale;
    private Coroutine bounceRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        GolfBall ball = collision.gameObject.GetComponent<GolfBall>();
        if (ball != null)
        {
            Rigidbody rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 normal = collision.contacts.Length > 0 ? collision.contacts[0].normal : -collision.relativeVelocity.normalized;
                normal.y = 0f;
                normal.Normalize();

                float inSpeed = Mathf.Max(collision.relativeVelocity.magnitude, minBounceVelocity);
                rb.linearVelocity = -normal * inSpeed * bounceMultiplier;

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlayBumper();
                }

                if (bounceRoutine != null) StopCoroutine(bounceRoutine);
                bounceRoutine = StartCoroutine(BounceAnimation());
            }
        }
    }

    private IEnumerator BounceAnimation()
    {
        transform.localScale = originalScale * 1.25f;
        float elapsed = 0f;
        while (elapsed < 0.2f)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(originalScale * 1.25f, originalScale, elapsed / 0.2f);
            yield return null;
        }
        transform.localScale = originalScale;
    }
}
