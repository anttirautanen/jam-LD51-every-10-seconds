using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float cameraMoveSpeed = 1f;
    public float cameraZoomSpeed = 1f;
    public float minCameraSize = 5;
    public float maxCameraSize = 50;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            var t = transform;
            var localPosition = t.localPosition;
            localPosition = new Vector3(
                localPosition.x,
                localPosition.y + Time.deltaTime * cameraMoveSpeed,
                localPosition.z
            );
            t.localPosition = localPosition;
        }

        if (Input.GetKey(KeyCode.A))
        {
            var t = transform;
            var localPosition = t.localPosition;
            localPosition = new Vector3(
                localPosition.x - Time.deltaTime * cameraMoveSpeed,
                localPosition.y,
                localPosition.z
            );
            t.localPosition = localPosition;
        }

        if (Input.GetKey(KeyCode.S))
        {
            var t = transform;
            var localPosition = t.localPosition;
            localPosition = new Vector3(
                localPosition.x,
                localPosition.y - Time.deltaTime * cameraMoveSpeed,
                localPosition.z
            );
            t.localPosition = localPosition;
        }

        if (Input.GetKey(KeyCode.D))
        {
            var t = transform;
            var localPosition = t.localPosition;
            localPosition = new Vector3(
                localPosition.x + Time.deltaTime * cameraMoveSpeed,
                localPosition.y,
                localPosition.z
            );
            t.localPosition = localPosition;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            var mainCamera = Camera.main;
            mainCamera!.orthographicSize = Mathf.Clamp(
                mainCamera.orthographicSize + Time.deltaTime * cameraZoomSpeed,
                minCameraSize,
                maxCameraSize
            );
        }

        if (Input.GetKey(KeyCode.E))
        {
            var mainCamera = Camera.main;
            mainCamera!.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - Time.deltaTime * cameraZoomSpeed,
                minCameraSize,
                maxCameraSize
            );
        }
    }
}