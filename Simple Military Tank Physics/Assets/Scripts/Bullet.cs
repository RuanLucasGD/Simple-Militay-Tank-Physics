using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float moveSpeed;

    void Start()
    {
        Destroy(gameObject, 10);
    }

    void Update()
    {
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}
