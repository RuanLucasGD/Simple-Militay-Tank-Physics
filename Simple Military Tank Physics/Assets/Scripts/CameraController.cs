using UnityEngine;

/*
A unica ultilidade real desse script é pegar a posição do alvo, o resto pode ser descartado que não irá afetar o script do veículo
*/
public class CameraController : MonoBehaviour
{
    public float height;
    public float distance;

    public Transform target;

    public float rotSpeed;
    public float camHeight;
    public float camZoomSpeed;
    public float maxSightDistance;  // distancia maxima que um alvo pode ser ser identificado

    // --- distancia do zoom --- //
    public float maxCamDistance;
    public float minCamDistance;

    // --- rotação vertical --- //
    public float minAngle;
    public float maxAngle;

    private float currentRotX;
    private float currentRotY;

    private float currentDistance;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        transform.position = target.position;
        transform.eulerAngles = Vector3.up * target.eulerAngles.y;

        currentDistance = distance;
    }

    void Update()
    {
        float m_mouseX = Input.GetAxis("Mouse X");
        float m_mouseY = Input.GetAxis("Mouse Y");

        // se clicar o botão direito mouse aplica zoom
        // se não a camera rotaciona em torno do veículo
        if (Input.GetKey(KeyCode.Mouse1) || Input.GetKey(KeyCode.JoystickButton4))
        {
            currentDistance += m_mouseY * camZoomSpeed * Time.deltaTime;
            currentDistance = Mathf.Clamp(currentDistance, minCamDistance, maxCamDistance);
        }
        else
        {
            currentRotX += m_mouseX * rotSpeed;
            currentRotY -= m_mouseY * rotSpeed;

            currentRotY = Mathf.Clamp(currentRotY, minAngle, maxAngle);
            transform.eulerAngles = new Vector3(currentRotY, currentRotX);
        }

        // segue o jogador
        transform.position = (target.position + (Vector3.up * height)) - (transform.forward * currentDistance);
    }

    public Vector3 GetTargetPosition()
    {
        // caso a camera não encontre nenhum alvo até a distancia maxima da mira, ele retorna um vetor para frente da camera,
        //fazendo com que a torre do veículo mire para onde está apontandp
        Vector3 m_pos = transform.position + (transform.forward * 5000);

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, maxSightDistance)) m_pos = hit.point;

        return m_pos;
    }
}
