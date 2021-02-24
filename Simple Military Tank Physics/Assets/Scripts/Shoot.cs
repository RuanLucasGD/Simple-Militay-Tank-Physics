using UnityEngine;

public class Shoot : MonoBehaviour
{
    public GameObject bullet;

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Instantiate(bullet, transform.position, transform.rotation);
        }
    }
}
