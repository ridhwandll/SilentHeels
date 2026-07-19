using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraSizer : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float TargetZoom = 7f;
    public float ZoomSpeed = 3f;

    private Camera _mainCam;

    private static Coroutine _activeZoomCoroutine;
    private static CameraSizer _activeTrigger;

    private void Start()
    {
        _mainCam = Camera.main;
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (_activeZoomCoroutine != null && _activeTrigger != null)
                _activeTrigger.StopCoroutine(_activeZoomCoroutine);

            _activeTrigger = this;
            _activeZoomCoroutine = StartCoroutine(SmoothZoom());
        }
    }

    private IEnumerator SmoothZoom()
    {
        while (Mathf.Abs(_mainCam.orthographicSize - TargetZoom) > 0.01f)
        {
            _mainCam.orthographicSize = Mathf.Lerp(_mainCam.orthographicSize, TargetZoom, Time.deltaTime * ZoomSpeed);
            yield return null;
        }

        _mainCam.orthographicSize = TargetZoom;
    }
}