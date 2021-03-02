using UnityEngine;

/*
A unica ultilidade real desse script é pegar a posição do alvo, o resto pode ser descartado que não irá afetar o script do veículo
*/
public class CameraController : MonoBehaviour
{
    public Transform target;

    public float rotSpeed;
    public float camHeight;
    public float camZoomSpeed;
    public float maxSightDistance;  // distancia maxima que um alvo pode ser ser identificado

    private Transform cam;          // camera do jogo

    // --- distancia do zoom --- //
    public float maxCamDistance;
    public float minCamDistance;

    void Start()
    {
        cam = GetComponentInChildren<Camera>().transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        transform.position = target.position;
        transform.eulerAngles = Vector3.up * target.eulerAngles.y;
    }

    void FixedUpdate()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // aplicando zoom
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
        // aplica rotação da camera
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
        // caso a camera não encontre nenhum alvo até a distancia maxima da mira, ele retorna um vetor para frente da camera,
        //fazendo com que a torre do veículo mire para onde está apontandp
        Vector3 pos = cam.position + (cam.forward * 5000);

        if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit, maxSightDistance)) pos = hit.point;

        return pos;
    }
}
