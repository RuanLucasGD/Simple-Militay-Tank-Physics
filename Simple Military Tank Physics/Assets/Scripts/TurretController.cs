using UnityEngine;

public class TurretController : MonoBehaviour
{
    public CameraController cameraController;

    public Transform vehicle;
    public Transform turret;
    public Transform gun;

    public float rotSpeedHorizontal;
    public float rotSpeedVertical;

    public float maxGunAngle;
    public float minGunAngle;

    void Start()
    {

    }

    void FixedUpdate()
    {
        Vector3 targetPos = cameraController.GetTargetPosition();

        Quaternion lookAtTurret = Quaternion.LookRotation(targetPos - turret.position, vehicle.up);
        Quaternion lookAtGun = Quaternion.LookRotation(targetPos - gun.position, gun.up);

        Quaternion turretRotDelta = Quaternion.Euler(vehicle.eulerAngles - lookAtTurret.eulerAngles);
        Quaternion gunRotDelta = Quaternion.Euler(turret.eulerAngles - lookAtGun.eulerAngles);
        Quaternion turretRot = Quaternion.Euler(vehicle.eulerAngles - turretRotDelta.eulerAngles);
        Quaternion gunRot = Quaternion.Euler(turret.eulerAngles - turretRotDelta.eulerAngles);

        turretRot = Quaternion.Slerp(turret.rotation, turretRot, rotSpeedHorizontal);
        gunRot = Quaternion.Slerp(gun.rotation, gunRot, rotSpeedVertical);

        turret.rotation = turretRot;
        gun.rotation = gunRot;

        float max = 360 - maxGunAngle;
        float min = minGunAngle;
        float currentAngle = gun.localEulerAngles.x;

        if (currentAngle > 180)
        {
            if (currentAngle < max)
            {
                gun.transform.localEulerAngles = new Vector3(max, 0, 0);
            }
        }
        else
        {
            if (currentAngle > min)
            {
                gun.transform.localEulerAngles = new Vector3(min, 0, 0);
            }
        }

        turret.localRotation = Quaternion.Euler(0, turret.localEulerAngles.y, 0);
        gun.localRotation = Quaternion.Euler(gun.localEulerAngles.x, 0, 0);
    }
}
