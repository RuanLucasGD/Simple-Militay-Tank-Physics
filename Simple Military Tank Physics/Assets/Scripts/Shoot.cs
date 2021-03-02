using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bullet;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.JoystickButton5))
        {
            Instantiate(bullet, transform.position, transform.rotation);
        }
    }
}
