﻿using System;
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

    public SkinnedMeshRenderer matLeft;
    public SkinnedMeshRenderer matRight;

    public float wheelRadius;
    public float springLenght;
    public float springStiffness;
    public float damperStiffness;
    public float springHeight;

    [SerializeField] public Wheel[] wheelsLeft;
    [SerializeField] public Wheel[] wheelsRight;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        foreach (var w in wheelsLeft) w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();
        foreach (var w in wheelsRight) w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();
    }

    void FixedUpdate()
    {
        foreach (var w in wheelsLeft) UseWheelCollider(w);
        foreach (var w in wheelsRight) UseWheelCollider(w);

        DisableSuspensionRenderer();
    }

    void UseWheelCollider(Wheel w)
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");

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
            forwardDirection *= forwardAcceleration * (1 - (Mathf.Abs(forwardVel) / maxMoveSpeed));
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
