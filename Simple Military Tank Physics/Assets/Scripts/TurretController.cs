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

    private Quaternion turretRelativeRotation;
    private Quaternion gunRelativeRotation;

    void Start()
    {

    }

    void FixedUpdate()
    {
        Vector3 targetPos = cameraController.GetTargetPosition();

        Quaternion lookAtTurret = Quaternion.LookRotation(targetPos - turret.position, vehicle.up);
        Quaternion lookAtGun = Quaternion.LookRotation(targetPos - gun.position, gun.up);

        turretRelativeRotation = Quaternion.Slerp(turretRelativeRotation, Quaternion.Euler(vehicle.eulerAngles - lookAtTurret.eulerAngles), rotSpeedHorizontal);
        gunRelativeRotation = Quaternion.Slerp(gunRelativeRotation, Quaternion.Euler(turret.eulerAngles - lookAtGun.eulerAngles), rotSpeedVertical);

        Quaternion turretRot = Quaternion.Euler(vehicle.eulerAngles - turretRelativeRotation.eulerAngles);
        Quaternion gunRot = Quaternion.Euler(turret.eulerAngles - gunRelativeRotation.eulerAngles);

        Quaternion finalTurretRot = turretRot;
        Quaternion finalGunRot = gunRot;

        gun.rotation = finalGunRot; // FIRST!
        turret.rotation = finalTurretRot; // LATER!

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
