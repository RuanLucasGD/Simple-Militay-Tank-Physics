using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float rotSpeed;
    public float camZoomSpeed;
    public float camHeight;

    private Transform cam;

    public float maxCamDistance;
    public float minCamDistance;

    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void FixedUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        transform.Rotate(((Vector3.up * mouseX) + (Vector3.right * mouseY)) * rotSpeed);

        Vector3 cameraLookAt = target.position + (Vector3.up * camHeight);

        cam.LookAt(cameraLookAt);
        transform.position = target.position;
    }
}
