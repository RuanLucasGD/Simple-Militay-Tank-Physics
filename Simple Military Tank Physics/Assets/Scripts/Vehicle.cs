using System;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Serializable]
    public class Wheel
    {
        public Transform collider;
        public Transform bone;
        public Transform mesh;

        [NonSerialized] public float lastSpringLenght;
        [NonSerialized] public float currentSpringLenght;
        [NonSerialized] public MeshRenderer meshRenderer;
    }

    public float forwardAcceleration;
    public float rotationAcceleration;
    public float maxSpeed;

    public Transform centerOfMass;

    public SkinnedMeshRenderer mat;

    public float wheelRadius;
    public float springLenght;
    public float springStiffness;
    public float damperStiffness;
    public float springHeight;

    [SerializeField] public Wheel[] wheels;

    private Rigidbody rb;
    private MeshRenderer[] wheelsRenderers;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        foreach (var w in wheels)
        {
            w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();
        }
    }

    void FixedUpdate()
    {
        rb.centerOfMass = centerOfMass.localPosition;

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");

        Vector3 localVeloity = transform.InverseTransformDirection(rb.velocity);

        foreach (var w in wheels)
        {
            Vector3 wheelPosition = w.collider.position;
            Vector3 springPosition = wheelPosition + (transform.up * springHeight);

            Debug.DrawRay(springPosition, -transform.up * springHeight, Color.red);

            if (Physics.Raycast(springPosition, -transform.up, out RaycastHit hit, springLenght + wheelRadius + springHeight))
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

                float forwardDirection = vertical;
                float sideDirection = horizontal;

                float forwardVel = Mathf.Clamp(localVeloity.z, -maxSpeed, maxSpeed);

                forward *= 1 - Mathf.Abs(vertical);
                forwardDirection *= forwardAcceleration * (1 - (Mathf.Abs(forwardVel) / maxSpeed));
                sideDirection *= w.collider.localPosition.z * rotationAcceleration;

                if (verticalRaw == -1) sideDirection *= -1;

                Vector3 upForce = Vector3.up * up;
                Vector3 sideForce = -transform.right * side;
                Vector3 forwardForce = -transform.forward * forward;
                Vector3 directionForce = (transform.forward * forwardDirection) + (transform.right * sideDirection);

                Vector3 totalForce = upForce + sideForce + forwardForce + directionForce;

                rb.AddForceAtPosition(totalForce, w.collider.position);

                Vector3 wheelPos = w.collider.position + (-transform.up * distance);
                w.bone.position = wheelPos;
                w.mesh.position = wheelPos;

                Debug.DrawRay(wheelPosition, -transform.up * w.currentSpringLenght);
                Debug.DrawRay(hit.point, transform.up * wheelRadius, Color.red);
            }

            else
            {
                Vector3 wheelPos = wheelPosition + (-transform.up * springLenght);
                w.bone.position = wheelPos;
                w.mesh.position = wheelPos;

                Debug.DrawRay(wheelPosition, -transform.up * springLenght);
                Debug.DrawLine(wheelPosition + (-transform.up * springLenght), wheelPosition + (-transform.up * (springLenght + wheelRadius)), Color.red);
            }
        }

        DisableSuspensionRenderer();
    }

    void DisableSuspensionRenderer()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            mat.enabled = mat.enabled ? false : true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var w in wheels)
            {
                w.meshRenderer.enabled = w.meshRenderer.enabled ? false : true;
            }
        }
    }
}
