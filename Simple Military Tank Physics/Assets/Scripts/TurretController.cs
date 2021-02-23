using UnityEngine;

public class TurretController : MonoBehaviour
{
    public CameraController cameraController;

    public Transform vehicle;
    public Transform turret;
    public Transform gun;

    void Start()
    {

    }

    void Update()
    {
        Vector3 targetPos = cameraController.GetTargetPosition();

        Quaternion lookAtTurret = Quaternion.LookRotation(targetPos - turret.position, vehicle.up);
        Quaternion lookAtGun = Quaternion.LookRotation(targetPos - gun.position, gun.up);

        Quaternion turretRotDelta = Quaternion.Euler(vehicle.eulerAngles - lookAtTurret.eulerAngles);
        Quaternion gunRotDelta = Quaternion.Euler(turret.eulerAngles - lookAtGun.eulerAngles);
        Quaternion turretRot = Quaternion.Euler(vehicle.eulerAngles - turretRotDelta.eulerAngles);
        Quaternion gunRot = Quaternion.Euler(turret.eulerAngles - turretRotDelta.eulerAngles);

        turret.rotation = turretRot;
        gun.rotation = gunRot;

        turret.localRotation = Quaternion.Euler(0, turret.localEulerAngles.y, 0);
        gun.localRotation = Quaternion.Euler(gun.localEulerAngles.x, 0, 0);
    }
}
