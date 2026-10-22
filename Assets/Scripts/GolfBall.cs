using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class GolfBall : MonoBehaviour
{
    [Header("Physics Settings")]
    [SerializeField] private float minLaunchSpeed = 0.5f;
    [SerializeField] private float maxLaunchSpeed = 34.5f;
    [SerializeField] private float stopVelocityThreshold = 0.12f;
    [SerializeField] private float outOfBoundsY = -2.5f;

    public bool IsMoving { get; private set; }
    public bool IsInHole { get; private set; }
    public event Action OnBallStopped;
    public event Action OnBallInHole;
    public event Action OnOutOfBounds;

    private Rigidbody rb;
    private Vector3 lastShotPosition;
    private float stopTimer = 0f;
    private const float RequiredStopTime = 0.25f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 0.046f;
        rb.linearDamping = 0.08f; // Low air drag so high-speed shots travel far
        rb.angularDamping = 0.15f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        SphereCollider col = GetComponent<SphereCollider>();
        col.radius = 0.12f;

        // Realistic golf ball physics material
        PhysicsMaterial mat = new PhysicsMaterial("GolfBallMat")
        {
            dynamicFriction = 0.12f,
            staticFriction = 0.18f,
            bounciness = 0.55f,
            frictionCombine = PhysicsMaterialCombine.Multiply,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };
        col.material = mat;
    }

    private void Start()
    {
        lastShotPosition = transform.position;
    }

    private void FixedUpdate()
    {
        if (IsInHole) return;

        // Check Out of bounds
        if (transform.position.y < outOfBoundsY)
        {
            HandleOutOfBounds();
            return;
        }

        float speed = rb.linearVelocity.magnitude;

        if (IsMoving)
        {
            if (speed < stopVelocityThreshold)
            {
                stopTimer += Time.fixedDeltaTime;
                if (stopTimer >= RequiredStopTime)
                {
                    StopBallCompletely();
                }
            }
            else
            {
                stopTimer = 0f;
            }
        }
    }

    public void Shoot(Vector3 direction, float powerRatio)
    {
        if (IsMoving || IsInHole) return;

        lastShotPosition = transform.position;
        direction.y = 0f;
        direction.Normalize();

        float clampedRatio = Mathf.Clamp01(powerRatio);
        // Smooth exponential power curve:
        // Low power (5%-20%) gives soft, precise putts
        // High power (80%-100%) gives fast, explosive full-course drives
        float curvedRatio = Mathf.Pow(clampedRatio, 1.8f);
        float launchSpeed = Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, curvedRatio);

        rb.linearVelocity = direction * launchSpeed;
        rb.angularVelocity = Vector3.Cross(Vector3.up, direction) * (launchSpeed / 0.12f);

        IsMoving = true;
        stopTimer = 0f;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayPutt(clampedRatio);
        }
    }

    private void StopBallCompletely()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        IsMoving = false;
        stopTimer = 0f;
        OnBallStopped?.Invoke();
    }

    private void HandleOutOfBounds()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = lastShotPosition + Vector3.up * 0.1f;
        IsMoving = false;
        stopTimer = 0f;

        OnOutOfBounds?.Invoke();
        OnBallStopped?.Invoke();
    }

    public void EnterHole(Vector3 holePosition)
    {
        if (IsInHole) return;

        IsInHole = true;
        IsMoving = false;

        // Pull into hole smoothly
        rb.linearVelocity = Vector3.down * 1.5f;
        rb.angularVelocity = Vector3.zero;

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayHoleCup();
            SoundManager.Instance.PlayCheer();
        }

        OnBallInHole?.Invoke();
    }

    public void ResetBall(Vector3 spawnPosition)
    {
        IsInHole = false;
        IsMoving = false;
        stopTimer = 0f;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = spawnPosition;
        lastShotPosition = spawnPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsMoving && collision.relativeVelocity.magnitude > 0.6f)
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayBounce(collision.relativeVelocity.magnitude / maxLaunchSpeed);
            }
        }
    }
}
