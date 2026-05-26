using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public enum CameraMode { FOLLOWING, LOCKED, FREE }

    [Header("Target & Mode")]
    [SerializeField] private HeroController hero_ctrl;
    [SerializeField] private CameraMode mode = CameraMode.FOLLOWING;

    [Header("Smooth Damp Settings")]
    [SerializeField] private float dampTimeX = 0.15f;
    [SerializeField] private float dampTimeY = 0.15f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Look Offsets")]
    [SerializeField] private float lookUpOffset = 6f;
    [SerializeField] private float lookDownOffset = -6f;
    [SerializeField] private float lookSmoothing = 5.5f;

    [Header("Map Bounds")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private float xLockMin = -100f;
    [SerializeField] private float xLockMax = 100f;
    [SerializeField] private float yLockMin = -100f;
    [SerializeField] private float yLockMax = 100f;

    private Camera cam;
    private Vector2 velocity;
    private float currentLookOffset;
    private Vector3 position;
    private Vector3 destination;

    private void Start()
    {
        cam = GetComponent<Camera>();

        if (hero_ctrl == null && HeroController.instance != null)
        {
            hero_ctrl = HeroController.instance;
        }
        else if (hero_ctrl == null)
        {
            hero_ctrl = FindObjectOfType<HeroController>();
        }

        if (hero_ctrl != null)
        {
            position = transform.position;
        }
    }

    private void LateUpdate()
    {
        if (hero_ctrl == null || Time.timeScale <= Mathf.Epsilon) return;

        position = transform.position;

        // Base destination is player position + offset
        destination = hero_ctrl.transform.position + offset;

        // Look up/down offset logic relying directly on HeroController cState
        float targetLookOffset = 0f;
        if (hero_ctrl.cState.lookingUp)
        {
            targetLookOffset = lookUpOffset;
        }
        else if (hero_ctrl.cState.lookingDown)
        {
            targetLookOffset = lookDownOffset;
        }

        // Smoothly interpolate the look offset
        currentLookOffset = Mathf.Lerp(currentLookOffset, targetLookOffset, Time.deltaTime * lookSmoothing);
        destination.y += currentLookOffset;

        // Keep within scene bounds (No child object lock area necessary)
        if (useBounds && (mode == CameraMode.FOLLOWING || mode == CameraMode.LOCKED))
        {
            destination = KeepWithinSceneBounds(destination);
        }

        // Apply SmoothDamp to X and Y
        float smoothX = Mathf.SmoothDamp(position.x, destination.x, ref velocity.x, dampTimeX);
        float smoothY = Mathf.SmoothDamp(position.y, destination.y, ref velocity.y, dampTimeY);

        transform.position = new Vector3(smoothX, smoothY, destination.z);
    }

    private Vector3 KeepWithinSceneBounds(Vector3 targetPos)
    {
        if (cam == null) return targetPos;

        float vertExtent = cam.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        float clampedX = Mathf.Clamp(targetPos.x, xLockMin + horzExtent, xLockMax - horzExtent);
        float clampedY = Mathf.Clamp(targetPos.y, yLockMin + vertExtent, yLockMax - vertExtent);

        return new Vector3(clampedX, clampedY, targetPos.z);
    }

    public void SetBounds(float xMin, float xMax, float yMin, float yMax)
    {
        xLockMin = xMin;
        xLockMax = xMax;
        yLockMin = yMin;
        yLockMax = yMax;
        useBounds = true;
    }

    public void SetMode(CameraMode newMode)
    {
        mode = newMode;
    }
}
