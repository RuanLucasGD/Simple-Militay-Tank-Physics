using UnityEngine;

public class Restart : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick1Button0))
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
            transform.position = transform.position + (Vector3.up * 5);
        }
    }
}
