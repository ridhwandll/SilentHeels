using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("0 = Moves exactly with the camera (UI/Foreground). 1 = Static (Distant Sky).")]
    public Vector2 ParallaxMultiplier = new Vector2(0.5f, 0.5f);

    private Transform _cameraTransform;
    private Vector3 _lastCameraPosition;

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lastCameraPosition = _cameraTransform.position;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = _cameraTransform.position - _lastCameraPosition;
        transform.position += new Vector3(deltaMovement.x * ParallaxMultiplier.x, deltaMovement.y * ParallaxMultiplier.y, 0);
        _lastCameraPosition = _cameraTransform.position;
    }
}