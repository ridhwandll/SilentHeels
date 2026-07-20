using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform target;
    private bool hasPlayer = false;

    public Vector3 offset = new Vector3(0f, 2f, -10f);
    [Range(0.01f, 1f)]
    public float smoothTime = 0.15f;

    private Vector3 _currentVelocity = Vector3.zero;

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
            hasPlayer = true;
        }
        else
            hasPlayer = false;
    }

    private void LateUpdate()
    {
        if (!hasPlayer)
            return;

        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _currentVelocity, smoothTime);
    }
}
