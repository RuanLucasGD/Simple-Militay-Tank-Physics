using System;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public float force;

    [Serializable]
    public class Wheel
    {
        public Transform collider;
        public Transform bone;
        public Transform mesh;

        [NonSerialized] public float lastSpringLenght;
        [NonSerialized] public float currentSpringLenght;
    }

    public float wheelRadius;
    public float springLenght;
    public float springStiffness;
    public float damperStiffness;

    [SerializeField] public Wheel[] wheels;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Time.timeScale = 0.4f;

        if (Input.GetKeyDown(KeyCode.Space))
        {

            rb.AddForce(Vector3.up * force);
        }

        foreach (var w in wheels)
        {
            if (Physics.Raycast(w.collider.position, -transform.up, out RaycastHit hit, springLenght + wheelRadius))
            {
                float distance = hit.distance - wheelRadius;

                w.lastSpringLenght = w.currentSpringLenght;
                w.currentSpringLenght = distance;

                w.currentSpringLenght = Mathf.Clamp(w.currentSpringLenght, 0, springLenght);

                float springVelocity = (w.lastSpringLenght - w.currentSpringLenght) / Time.fixedDeltaTime;
                float springForce = springStiffness * (springLenght - w.currentSpringLenght);

                float damperForce = damperStiffness * springVelocity;
                float suspensionForce = springForce + damperForce;

                Vector3 upForce = Vector3.up * suspensionForce;
                Vector3 totalForce = upForce;

                rb.AddForceAtPosition(totalForce, w.collider.position);

                Vector3 wheelPos = w.collider.position + (-transform.up * distance);
                w.bone.position = wheelPos;
                w.mesh.position = wheelPos;

                Debug.DrawLine(w.collider.position, hit.point);
            }

            else
            {
                Vector3 wheelPos = w.collider.position + (-transform.up * springLenght);
                w.bone.position = wheelPos;
                w.mesh.position = wheelPos;
            }
        }
    }
}
