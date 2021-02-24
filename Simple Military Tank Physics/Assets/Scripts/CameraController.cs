using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public float rotSpeed;
    public float camHeight;
    public float camZoomSpeed;
    public float maxSightDistance;

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

        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.Joystick1Button4))
        {
            Vector3 moveVel = cam.forward * camZoomSpeed * mouseY;

            float distance = Vector3.Distance(cam.position, target.position + (Vector3.up * camHeight));

            if (distance <= maxCamDistance && distance >= minCamDistance) cam.Translate(moveVel, Space.World);
            else if (distance > maxCamDistance)
            {
                if (mouseY > 0) cam.Translate(moveVel, Space.World);
            }
            else if (distance < minCamDistance)
            {
                if (mouseY < 0) cam.Translate(moveVel, Space.World);
            }
        }
        else
        {
            Vector3 cameraLookAt = target.position + (Vector3.up * camHeight);
            cam.LookAt(cameraLookAt);
            transform.Rotate(((Vector3.up * mouseX) + (Vector3.right * mouseY)) * rotSpeed);
        }

        transform.position = target.position;
    }

    public Vector3 GetTargetPosition()
    {
        Vector3 pos = cam.position + cam.forward * 100;

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxSightDistance))
        {
            return hit.point;
        }

        return pos;
    }
}
