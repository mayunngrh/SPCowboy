using UnityEngine;

public class CameraZoom : MonoBehaviour
{
    [Header("Z Positions")]
    public float startZ = -950f;
    public float showOffZ = -1050f;
    public float zoomOutZ = -1300f;

    [Header("Y Positions")]
    public float startY = 0f;
    public float showOffY = 0f;
    public float zoomOutY = 0f;

    [Header("Timing")]
    public float moveToShowOffDuration = 1.2f;
    public float lingerDuration = 1.5f;
    public float zoomOutDuration = 1.8f;

    private float elapsed = 0f;
    private enum Phase { MoveIn, Linger, ZoomOut, Done }
    private Phase phase = Phase.MoveIn;

    void Start()
    {
        Vector3 pos = transform.position;
        pos.z = startZ;
        pos.y = startY;
        transform.position = pos;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        Vector3 pos = transform.position;

        switch (phase)
        {
            case Phase.MoveIn:
                float t1 = Mathf.Clamp01(elapsed / moveToShowOffDuration);
                float eased1 = 1f - Mathf.Pow(1f - t1, 3f);
                pos.z = Mathf.Lerp(startZ, showOffZ, eased1);
                pos.y = Mathf.Lerp(startY, showOffY, eased1);
                if (t1 >= 1f) { phase = Phase.Linger; elapsed = 0f; }
                break;

            case Phase.Linger:
                pos.z = showOffZ;
                pos.y = showOffY;
                if (elapsed >= lingerDuration) { phase = Phase.ZoomOut; elapsed = 0f; }
                break;

            case Phase.ZoomOut:
                float t3 = Mathf.Clamp01(elapsed / zoomOutDuration);
                float eased3 = t3 < 0.5f ? 4f * t3 * t3 * t3 : 1f - Mathf.Pow(-2f * t3 + 2f, 3f) / 2f;
                pos.z = Mathf.Lerp(showOffZ, zoomOutZ, eased3);
                pos.y = Mathf.Lerp(showOffY, zoomOutY, eased3);
                if (t3 >= 1f) phase = Phase.Done;
                break;
        }

        transform.position = pos;
    }
}
