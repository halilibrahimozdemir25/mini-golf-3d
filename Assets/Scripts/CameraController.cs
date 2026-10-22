using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GolfBall targetBall;
    [SerializeField] private SlingshotController slingshotController;
    [SerializeField] private Transform currentHoleTarget;

    [Header("Distance & Height")]
    [SerializeField] private float distance = 5.4f;
    [SerializeField] private float heightOffset = 0.35f;
    [SerializeField] private float followSpeed = 6.5f;

    private Vector3 currentVelocity;
    private float camYaw = 0f;
    private float camPitch = 0.42f;

    public bool IsSlingshotLocked => slingshotController != null && slingshotController.IsPulling;

    public void SetTarget(GolfBall newBall)
    {
        targetBall = newBall;
    }

    public void SetHoleTarget(Transform hole)
    {
        currentHoleTarget = hole;
        ResetToHoleOrientation();
    }

    public void ResetToHoleOrientation()
    {
        if (targetBall != null && currentHoleTarget != null)
        {
            Vector3 toHole = currentHoleTarget.position - targetBall.transform.position;
            toHole.y = 0f;
            if (toHole.sqrMagnitude > 0.1f)
            {
                camYaw = Mathf.Atan2(toHole.x, toHole.z);
                camPitch = 0.42f;
            }
        }
    }

    public void OnOrbitDrag(Vector2 screenDelta)
    {
        camYaw -= screenDelta.x * 0.0055f;
        camPitch = Mathf.Clamp(camPitch - screenDelta.y * 0.004f, 0.12f, 1.22f);
    }

    private void LateUpdate()
    {
        if (targetBall == null) return;

        // When slingshot is pulled, the camera is COMPLETELY FROZEN (zero movement/shake!)
        if (IsSlingshotLocked)
        {
            return;
        }

        Vector3 ballPos = targetBall.transform.position;
        Vector3 lookTarget = ballPos + Vector3.up * heightOffset;

        Vector3 desiredPosition = ballPos + new Vector3(
            -Mathf.Sin(camYaw) * Mathf.Cos(camPitch) * distance,
            Mathf.Sin(camPitch) * distance,
            -Mathf.Cos(camYaw) * Mathf.Cos(camPitch) * distance
        );

        if (targetBall.IsMoving)
        {
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, 1f / followSpeed);
            Quaternion targetRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 8.0f);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10f);
            Quaternion targetRot = Quaternion.LookRotation(lookTarget - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }
}
