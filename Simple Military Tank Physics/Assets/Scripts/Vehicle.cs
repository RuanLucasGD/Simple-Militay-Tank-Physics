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
    public float springHeight;

    [SerializeField] public Wheel[] wheels;

    private Rigidbody rb;

    private float minLenght;
    private float maxLenght;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.AddForce(Vector3.up * force);
        }

        foreach (var w in wheels)
        {
            if (Physics.Raycast(w.collider.position + (transform.up * springHeight), -transform.up, out RaycastHit hit, springLenght + wheelRadius + springHeight))
            {
                Vector3 wheelVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));

                float distance = hit.distance - wheelRadius - springHeight;

                w.lastSpringLenght = w.currentSpringLenght;
                w.currentSpringLenght = distance;

                w.currentSpringLenght = Mathf.Clamp(w.currentSpringLenght, 0, springLenght);

                float springVelocity = (w.lastSpringLenght - w.currentSpringLenght) / Time.fixedDeltaTime;
                float springForce = springStiffness * (springLenght - w.currentSpringLenght);

                float damperForce = damperStiffness * springVelocity;
                float suspensionForce = springForce + damperForce;

                suspensionForce = Mathf.Clamp(suspensionForce, 0, suspensionForce);

                float up = suspensionForce;
                float side = springForce * wheelVelocity.x;
                float forward = springForce * wheelVelocity.z;

                Vector3 upForce = Vector3.up * up;
                Vector3 sideForce = -transform.right * side;
                Vector3 forwardForce = -transform.forward * forward;
                Vector3 totalForce = upForce + sideForce + forwardForce;

                rb.AddForceAtPosition(totalForce, w.collider.position);

                Vector3 wheelPos = w.collider.position + (-transform.up * distance);
                w.bone.position = wheelPos;
                w.mesh.position = wheelPos;

                Debug.DrawLine(w.collider.position, hit.point);
                Debug.Log(suspensionForce);
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
