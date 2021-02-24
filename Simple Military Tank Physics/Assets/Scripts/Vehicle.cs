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
    public float maxMoveSpeed;

    public Transform centerOfMass;

    public float wheelRadius;
    public float springLenght;
    public float springStiffness;
    public float damperStiffness;
    public float springHeight;

    public Transform[] additionalWheelMeshs;
    public SkinnedMeshRenderer matLeft;
    public SkinnedMeshRenderer matRight;
    public Vector3 wheelMeshsAxisRight;

    [SerializeField] public Wheel[] wheelsLeft;
    [SerializeField] public Wheel[] wheelsRight;

    private Material leftMatMaterial;
    private Material rightMatMaterial;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        leftMatMaterial = matLeft.GetComponent<SkinnedMeshRenderer>().material;
        rightMatMaterial = matRight.GetComponent<SkinnedMeshRenderer>().material;

        foreach (var w in wheelsLeft) w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();
        foreach (var w in wheelsRight) w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();
    }

    void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        float leftAccForward = 0;
        float rightAccForward = 0;

        float leftAccRotation = 0;
        float rightAccRotation = 0;

        foreach (var w in wheelsLeft)
        {
            UseWheelCollider(w, out float wheelSpeedForward, out float wheelSpeedRotation);
            leftAccForward += wheelSpeedForward;
            leftAccRotation += wheelSpeedRotation;
        }

        foreach (var w in wheelsRight)
        {
            UseWheelCollider(w, out float wheelSpeed, out float wheelSpeedRotation);
            rightAccForward += wheelSpeed;
            rightAccRotation += wheelSpeedRotation;
        }

        int rightLenght = wheelsRight.Length;
        int leftLenght = wheelsLeft.Length;

        leftAccForward /= leftLenght;
        rightAccForward /= rightLenght;
        leftAccRotation /= leftLenght;
        rightAccRotation /= rightLenght;

        float accLeft = Mathf.Abs(leftAccForward) + Mathf.Abs(rightAccRotation);
        float accRight = Mathf.Abs(rightAccForward) + Mathf.Abs(leftAccRotation);

        //if (horizontal > 0) leftAccRotation *= -1;
        //else if (horizontal < 0) rightAccRotation *= -1;

        Vector2 matDir = Vector2.down;

        if (vertical == 0)
        {
            if (horizontal < 0) accRight *= -1;
            else if (horizontal > 0) accLeft *= -1;
            matDir = Vector2.down;
        }
        else
        {
            if (horizontal == 0)
            {
                if (vertical > 0)
                {
                    accLeft *= -1;
                    accRight *= -1;

                }

                matDir = Vector2.up;
            }
            else
            {
                if (vertical > 0)
                {
                    accLeft *= -1;
                    accRight *= -1;
                    matDir = Vector2.up;
                }



                if (horizontal > 0) accRight *= 0.5f;
                else if (horizontal < 0) accLeft *= 0.5f;
            }

        }

        foreach (var w in wheelsLeft)
        {
            w.mesh.Rotate(wheelMeshsAxisRight * accRight * Time.fixedDeltaTime, Space.Self);
        }

        foreach (var w in wheelsRight)
        {
            w.mesh.Rotate(wheelMeshsAxisRight * accLeft * Time.fixedDeltaTime, Space.Self);
        }

        foreach (var w in additionalWheelMeshs)
        {
            w.Rotate(wheelMeshsAxisRight * accLeft * Time.fixedDeltaTime, Space.Self);
        }

        leftMatMaterial.mainTextureOffset += matDir * accLeft * Time.fixedDeltaTime / 100;
        rightMatMaterial.mainTextureOffset += matDir * accRight * Time.fixedDeltaTime / 100;

        DisableSuspensionRenderer();
    }

    void UseWheelCollider(Wheel w, out float forwardAccelerationResult, out float rotationAccelerationResult)
    {

        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");

        forwardAccelerationResult = vertical * forwardAcceleration;
        rotationAccelerationResult = horizontal * rotationAcceleration;

        Vector3 localVeloity = transform.InverseTransformDirection(rb.velocity);
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

            float forwardVel = Mathf.Clamp(localVeloity.z, -maxMoveSpeed, maxMoveSpeed);
            forward *= 1 - Mathf.Abs(vertical);

            sideDirection *= w.collider.localPosition.z * rotationAcceleration;

            if (verticalRaw == -1) sideDirection *= -1;


            forwardDirection *= forwardAcceleration * (1 - (Mathf.Abs(forwardVel) / maxMoveSpeed));

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
            Debug.DrawLine(hit.point, hit.point + Vector3.Normalize(directionForce));
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

    void DisableSuspensionRenderer()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            matLeft.enabled = matLeft.enabled ? false : true;
            matRight.enabled = matRight.enabled ? false : true;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var w in wheelsLeft) w.meshRenderer.enabled = w.meshRenderer.enabled ? false : true;
            foreach (var w in wheelsRight) w.meshRenderer.enabled = w.meshRenderer.enabled ? false : true;
        }
    }
}
