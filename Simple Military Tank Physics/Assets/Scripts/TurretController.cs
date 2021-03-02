using UnityEngine;

/*
Controla a torre e canhão co veículo
*/
public class TurretController : MonoBehaviour
{
    // usado para pegar a posição do alvo para a mira
    public CameraController cameraController;

    // --- componentes --- //
    public Transform vehicle;
    public Transform turret;
    public Transform gun;

    // --- velocidade de rotação --- //
    public float rotSpeedHorizontal;
    public float rotSpeedVertical;


    public float maxGunAngle;   // angulo maximo de rotação do canhão 
    public float minGunAngle;   // angulo minimo de rotação do canhão

    private Quaternion turretRelativeRotation;  // rotação relativa da torre para o veículo
    private Quaternion gunRelativeRotation;     // rotação relativa do canhão para a torre

    void FixedUpdate()
    {
        /*
        Usar apenas o lookAt para setar a rotação dos componentes causa grandes problemas na mira quando
        o veículo está em lugares inclinados.

        Para contornar o problema é usado a seguinte formula para achar o angulo de rotação real:

            relative = parentRot - lookAt
            finalRot = parentRot - relative

            a formula é aplicada para os dois componentes, canhão e torre. Lemre-se que ao aplicar a rotação, o canhçao
            vem sempre primeiro.
        */

        // posição do alvo
        Vector3 targetPos = cameraController.GetTargetPosition();

        // acha a rotação bruta para o alvo
        Quaternion lookAtTurret = Quaternion.LookRotation(targetPos - turret.position, vehicle.up);
        Quaternion lookAtGun = Quaternion.LookRotation(targetPos - gun.position, gun.up);

        // acha a rotação relativa e faz uma transição suavel da rotação
        turretRelativeRotation = Quaternion.Slerp(turretRelativeRotation, Quaternion.Euler(vehicle.eulerAngles - lookAtTurret.eulerAngles), rotSpeedHorizontal);
        gunRelativeRotation = Quaternion.Slerp(gunRelativeRotation, Quaternion.Euler(turret.eulerAngles - lookAtGun.eulerAngles), rotSpeedVertical);

        // rotação final dos componentes 
        Quaternion turretRot = Quaternion.Euler(vehicle.eulerAngles - turretRelativeRotation.eulerAngles);
        Quaternion gunRot = Quaternion.Euler(turret.eulerAngles - gunRelativeRotation.eulerAngles);

        // aplica a rotação 
        // **CANHÃO DEVE SER SEMPRE ANTES DA TORRE**
        gun.rotation = gunRot;          // - 1
        turret.rotation = turretRot;    // - 2

        // --- angulo maximo e angulo minimo de rotação do canhão --- //
        float max = 360 - maxGunAngle;
        float min = minGunAngle;
        float currentAngle = gun.localEulerAngles.x;

        if (currentAngle > 180)
        {
            if (currentAngle < max) gun.localEulerAngles = new Vector3(max, 0, 0);
        }
        else
        {
            if (currentAngle > min) gun.localEulerAngles = new Vector3(min, 0, 0);
        }

        // bloqueia a rotação em certos eixos
        turret.localRotation = Quaternion.Euler(0, turret.localEulerAngles.y, 0);
        gun.localRotation = Quaternion.Euler(gun.localEulerAngles.x, 0, 0);
    }
}
