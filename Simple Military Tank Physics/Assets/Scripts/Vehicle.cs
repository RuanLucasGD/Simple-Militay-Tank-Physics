using System;
using UnityEngine;

/*
Sistema de suspensao customizada usando raycasts para veículos militares (tanques de guerra, mas pode ser ajustado para outros
tipos de veículos que usam esteiras).

Sinta-se livre para modificações e usar em qualquer projeto. Caso use, considere dar os creditos a Ruan Lucas, ruanlucaspgbr@outlook.com,
criador do asset.

=========================================================================================================================================================================

O veículo possui seu proprio sistema de suspensao, os calculos de fisica são baseados em raycasts em cada roda, a movimentação das esteiras é feita por uma mesh 
com rig, na posição de cada roda existe um bone, a mesh da esteira é influenciada pelos bones. Via script, os bones acompanham a posição da roda, criando então a simulação
da esteira.

Para configurar um novo modelo 3D, lembresse que o modelo deve cumprir alguns requisitos:

    1 - o modelo deve ter as esteiras de cada lado separadas para que o sistema consiga pegar seus materiais corretamente

    2 - as esteiras precisam ter rig para criar o efeito de suspençã. Crie uma armature e coloque um bone na posição de cada roda (os bones não devem estar parenteados na
    roda, apenas na mesma posição)

    Caso tenha alguma dúvida, de uma olhada no modelo de exemplo que está na pasta ./models
*/
public class Vehicle : MonoBehaviour
{
    [Serializable]
    public class Wheel
    {
        public Transform collider;  // posicao da roda
        public Transform bone;      // bone que trata da movimentacao da esteira
        public Transform mesh;      // modelo 3D da roda

        public float springLenght;  // tamanho da suspensao

        // --- usado para calcular a velocidade em que a suspensao é comprimida: (last - current) / deltaTime --- //
        [NonSerialized] public float lastSpringLenght;      // tamanho da suspensao (comprimida) do ultimo frame
        [NonSerialized] public float currentSpringLenght;   // tamanho da suspensao (comprimida) do frame atual

        // ---
        [NonSerialized] public MeshRenderer meshRenderer;   // meshRenderer da roda (usado apenas para debug)
        [NonSerialized] public RaycastHit wheelHit;         // guarda informações sobre a colisão dos raycasts da suspensão

        // --- força aplicadas na suspensao para sustentar o veículo e controle de direção ---//

        /*
        Quando o raycast da roda toca o chão é feito varios calculos que geram uma força para alguma direção.

        **LEMBRANDO QUE TODOS OS CALCULOS SÃO FEITOS EM CADA RODA**

        Primeiro calculamos a força da suspensao usando a distancia que o veículo está do chão e tambem a macies da suspensão.
        Isso faz com que o veículo flutue, porem ele fica descontrolado deslizando.

        Para fazer com que ele pare de deslizar é calculado uma força contraria a velocidade que ele se move.
        Com isso, o veículo consegue ficar suspenso no ar sem deslizar.

        Para controlar o veículo, é calculado mais uma direção usando a velocidade de aceleração e os inputs.

        Depois que todas essas forças são calculadas, basta soma-las e aplicalas na posição da roda usando o rigidbody

        Lembre-se que é possivel visualizar a suspensão ativando os gizmos de debug
        */

        [NonSerialized] public Vector3 upForce;         // força aplicada pela suspensao para suspender o veículo
        [NonSerialized] public Vector3 sideForce;       // força lateral para evitar que o veículo deslize para os lado
        [NonSerialized] public Vector3 forwardForce;    // força frontal para evitar que o veículo deslize para frente ou para tras
        [NonSerialized] public Vector3 directionForce;  // direção que o veículo vai se mover

        [NonSerialized] public bool onGronded;          // se a roda está no chão
    }

    [Header("controll")]
    public float forwardAcceleration;   // forca de aceleracao para movimentar o veiculo (para frente e para tras)
    public float rotationAcceleration;  // forca de aceleracao para rotacionar o veiculo
    public float maxMoveSpeed;          // velocidade maxima que o veiculo pode atingir

    public Transform centerOfMass;      // ponto de partida para que o rigdbody calcule a fisica (ajuda a manter a estabilidade quando bem posicionado)


    // --- as configs abaixo se aplicam para todas as rodas ---//

    [Header("Suspension")]
    public float wheelRadius;           // tamanho das rodas
    public float springStiffness;       // rigidez / força da suspensao
    public float damperStiffness;       // macies da suspensao

    /*
    A suspensão deve ser calculada um pouco acima da roda, para evitar q em momentos extremos a suspensão não seja comprimida ao maximo e fazendo com que
    a força da suspensão inverta fazendo com que o veículo force a ir para baixo gerando um grande problema.

    Aumente esse valor para que a posição da suspensão fique por dentro do colisor do veículo, nunda deve ficar para o lado de fora, 
    para que tambem evite o problema sitado acima

    recomenda-se deixar entre 0.1 e 0.5

    */
    public float springHeight;

    /*
    Para criar a sensação de aceleração nas esteiras do veículo é preciso mudar o offset da UV
    O fato da escala da UV ser diferente para cada modelo 3D de veículo impede que ache uma formula que se encaixe
    perfeitamente em cada veículo, já que a velocidade que a esteira acelera está ligada a escala da UV.

    para contornar isso usamos uma variavel que guarde a velocidade que a esteira acelera e assim fica facil de chegar a uma velocidade aproximada
    que a esteira deva acelerar
    */
    [Header("Mats & wheels")]
    public float matMoveSpeed;
    public SkinnedMeshRenderer matLeft;     // modelo da esteira esquerda
    public SkinnedMeshRenderer matRight;    // modelo da esteira direita
    public Vector3 wheelMeshsAxisRight;     // eixo que as rodas vao rotacionar ao acelerar o veiculo
                                            // (vamos deixar essa opcao pois o modelo 3D pode estar com o eixo right na direção errada)

    [SerializeField] public Wheel[] wheelsLeft;     // rodas do lado direito
    [SerializeField] public Wheel[] wheelsRight;    // rodas do lado esquerdo


    // --- os materiais sao usados para poder dar o efeito de aceleracao nas esteiras ---//
    private Material leftMatMaterial;
    private Material rightMatMaterial;

    // ---
    private float maxAngularVelocity; // velocidade maxima de rotação que o veículo consegue alcançar
    private float wheelCircunference; // usado para saber a velocidade que as rodas giram


    // ---
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition; // ajustando estabilidade do veículo

        leftMatMaterial = matLeft.GetComponent<SkinnedMeshRenderer>().material;
        rightMatMaterial = matRight.GetComponent<SkinnedMeshRenderer>().material;

        // --- pega os components de renderizacao de todas as rodas --- //
        foreach (var w in wheelsLeft) w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();
        foreach (var w in wheelsRight) w.meshRenderer = w.mesh.GetComponent<MeshRenderer>();

        // ---
        maxAngularVelocity = rotationAcceleration / 1000;
        wheelCircunference = wheelRadius * Mathf.PI * 2;
    }

    private void FixedUpdate()
    {
        // --- aplicando efeito de aceleração --- //
        GetAccelerationOfVehicle(out float accLeft, out float accRight);

        Vector3 m_leftRotation = wheelMeshsAxisRight * accLeft;
        Vector3 m_rightRotation = wheelMeshsAxisRight * accRight;

        leftMatMaterial.mainTextureOffset += Vector2.up * accLeft * matMoveSpeed * Time.deltaTime;
        rightMatMaterial.mainTextureOffset += Vector2.up * accRight * matMoveSpeed * Time.deltaTime;

        // --- aplica a física das rodas

        foreach (var w in wheelsLeft)
        {
            UseWheelCollider(w);            // aplica a suspensao e movimentação
            w.mesh.Rotate(m_leftRotation);  // aplica rotação na roda
            DrawGizmos(w);                  // desenha as linhas de debug do veículo
        }

        foreach (var w in wheelsRight)
        {
            UseWheelCollider(w);            // aplica a suspensao e movimentação
            w.mesh.Rotate(m_rightRotation); // aplica rotação na roda   
            DrawGizmos(w);                  // desenha as linhas de debug do veículo
        }

        // desativa e ativa rodas e esteiras (para debug)
        DisableSuspensionRenderer();
    }

    /*
    Faz todos os calculos de suspensão e controle do jogador
    */
    void UseWheelCollider(Wheel w)
    {
        // --- inputs --- //
        float m_vertical = Input.GetAxis("Vertical");
        float m_horizontal = Input.GetAxis("Horizontal");
        float m_verticalRaw = Input.GetAxisRaw("Vertical");

        // ---
        Vector3 m_localVelocity = transform.InverseTransformDirection(rb.velocity); // pega a velocidade de movimento
        Vector3 m_springPosition = w.collider.position + (transform.up * springHeight); // pega a posição da suspensão

        // verifica se a roda está colidindo no chão
        if (Physics.Raycast(m_springPosition, -transform.up, out w.wheelHit, w.springLenght + wheelRadius + springHeight))
        {
            w.onGronded = true;

            // velocidade que a roda está se movendo
            Vector3 m_wheelVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(w.wheelHit.point));

            // distancia que a suspensão está do chão
            float m_distance = w.wheelHit.distance - wheelRadius - springHeight;
            w.lastSpringLenght = w.currentSpringLenght;
            w.currentSpringLenght = m_distance;
            // evita que a distancia da suspensão passe de limites que podem comprometer a força aplicada
            w.currentSpringLenght = Mathf.Clamp(w.currentSpringLenght, 0, w.springLenght);

            // velocidade que a mola da suspensão está sendo comprimida
            float m_springVelocity = (w.lastSpringLenght - w.currentSpringLenght) / Time.fixedDeltaTime;
            float m_springForce = springStiffness * (w.springLenght - w.currentSpringLenght); // força bruta da mola


            //Força da suspensão:
            //    suavização = velocidade de compressão da mola
            //    força da suspensao = distancia * rigidez da suspensao
            //    força final da suspensao = força da suspensao + suavização
            //
            //    sem a suavização o veículo pula muito

            float m_damperForce = damperStiffness * m_springVelocity;
            float m_suspensionForce = m_springForce + m_damperForce;

            // --- forças de controle do veículo --- //
            float m_up = m_suspensionForce;                                     // suspende o veículo no ar
            float m_sideStiffness = m_springForce * m_wheelVelocity.x;            // evita que escorregue para os lados
            float m_longitudinalStiffness = m_springForce * m_wheelVelocity.z;    // evita que escorregue para frente ou para trás

            float m_forwardVel = Mathf.Clamp(m_localVelocity.z, -maxMoveSpeed, maxMoveSpeed);
            m_longitudinalStiffness *= 1 - Mathf.Abs(m_vertical);               // não deve ter resitencia ao movimento se estiver acelerando / parar de freiar            

            // quando mais rapido estiver, menos deve acelerar / controle de velocidade longitudinal 
            float m_forwardDirection = m_vertical * forwardAcceleration * (1 - (Mathf.Abs(m_forwardVel) / maxMoveSpeed));

            //a rotação do veículo se da a forças aplicada para o lado.
            //quanto mais longe do centro a roda estiver, mais força para o lado ele exerce, fazedo com que o veículo gire
            float m_sideDirection = m_horizontal * w.collider.localPosition.z * rotationAcceleration;
            if (m_verticalRaw == -1) m_sideDirection *= -1; // caso o veículo esteja dando ré e virando ao mesmo tempo, deve inverter o controle

            // --- aplicando a força da suspensão e controle do veículo
            w.upForce = Vector3.up * m_up;
            w.sideForce = -transform.right * m_sideStiffness;
            w.forwardForce = -transform.forward * m_longitudinalStiffness;
            w.directionForce = (transform.forward * m_forwardDirection) + (transform.right * m_sideDirection);

            Vector3 m_totalForce = w.upForce + w.sideForce + w.forwardForce + w.directionForce;

            rb.AddForceAtPosition(m_totalForce, w.wheelHit.point);

            // atribuindo a posição da roda nas meshs de roda
            Vector3 m_wheelPos = w.collider.position + (-transform.up * m_distance);
            w.bone.position = m_wheelPos;
            w.mesh.position = m_wheelPos;
        }

        else
        {
            w.onGronded = false;

            // --- quando o veículo não está no chão, as rodas devem voltar ao seu local original --- //

            Vector3 m_wheelPos = w.collider.position + (-transform.up * w.springLenght);
            w.bone.position = m_wheelPos;
            w.mesh.position = m_wheelPos;
        }
    }

    /*
    Debug de gizmos da suspensão
    */
    void DrawGizmos(Wheel w)
    {
        Vector3 m_springPosition = w.collider.position + (transform.up * springHeight); // posicao da suspensão

        // desenha linha que vai da posicao da roda até a posicão da suspensao
        Debug.DrawLine(w.collider.position, m_springPosition, Color.red);

        if (w.onGronded)
        {
            Debug.DrawLine(w.collider.position, w.wheelHit.point);
            Debug.DrawRay(w.wheelHit.point, transform.up * wheelRadius, Color.red);
            Debug.DrawLine(w.wheelHit.point, w.wheelHit.point + Vector3.Normalize(w.directionForce));
        }
        else
        {
            // desenha uma linha da posicao da roda até o final da suspensão
            Debug.DrawLine(w.collider.position, w.collider.position + (-transform.up * w.springLenght), Color.blue);
            // desenha uma linha da posicao da roda (mais o seu raio de tamanho) até a posição final da mola da suspensão
            Debug.DrawRay(w.collider.position, -transform.up * w.springLenght);
            // desenha uma linha que vai do ponto de baixo da roda até o final da suspensão
            Debug.DrawLine(w.collider.position + (-transform.up * w.springLenght), w.collider.position + (-transform.up * (w.springLenght + wheelRadius)), Color.red);
        }
    }

    /*
    Pega a velocidade que deve ser usada para dar efeito de aceleração nas rodas e na esteira
    */
    void GetAccelerationOfVehicle(out float accLeft, out float accRight)
    {
        accLeft = 0;
        accRight = 0;

        // --- pegando velocidade de movimentacao e rotacao
        float m_longitudinalSpeed = Mathf.Abs(transform.InverseTransformDirection(rb.velocity).z);
        float m_angularvelocity = Mathf.Abs(rb.angularVelocity.y);

        // --- pegando inputs / tambem compativeis com gamepad, de uma olhada nos inputs da Unity e veja como estão configurados
        float m_vertical = Input.GetAxis("Vertical");
        float m_horizontal = Input.GetAxis("Horizontal");

        /*
        A aceleração das rodas é feita baseada no input e na velocidade que o veículo está.
        Se o veículo está em baixa velocidade ele derrapa se estiver acelerando, quando em alta velocidade ele derrapa menos e 
        a velocidade de rotação é baseada na sua velocidade.

        accInput =      input * (1 - (velocidade / maxVelocidade)) * maxVelocidade  --  quanto mais devagar estiver mais ele acelera
        accVelocidade = (velocidade / maxVelocidade) * velocidade                   --  a velocidade de rotacao tente ser a mesma de movimento (desde que esteja rapido)

        accInput = accInput + accVelocidade                                         -- quanto mais devagar e mais acelerar, mais ele derrapa

        */
        float input = Mathf.Clamp01(Mathf.Abs(m_vertical) + Mathf.Abs(m_horizontal));
        float inputAcc = input * (1 - (Mathf.Abs(m_longitudinalSpeed) / maxMoveSpeed)) * maxMoveSpeed;
        float velocityAcc = (Mathf.Abs(m_longitudinalSpeed) / maxMoveSpeed) * Mathf.Abs(m_longitudinalSpeed);

        float acc = inputAcc + velocityAcc;

        accLeft = acc;
        accRight = acc;

        /*
        Para criar o efeito de aceleração nas rodas e nas esteiras é preciso saber a direção que está se movendo e a força da aceleração,
        depois que eu tenho esses valores (para cada lado do veículo) aplico a rotação nas rodas e mudo o offset da textura da esteira.

        A forma mais simples que achei foi usar os inputs (vertical e horizontal) com valores sempre positivos (Mathf.Abs), depois que tenho
        esses valores eu inverto-os dependendo da ocasião para que a aceleração das esteiras e rodas condiza com a aceleração do veículo.

        Para isso usei duas acelerações (esquerda e direita pois cada lado do veículo corresponde difernete)
        */

        // --- ajustes da aceleração --- //
        if (m_vertical == 0)
        {
            if (m_horizontal < 0) accRight *= -1;
            else if (m_horizontal > 0) accLeft *= -1;
        }
        else
        {
            if (m_horizontal == 0)
            {
                if (m_vertical > 0)
                {
                    accLeft *= -1;
                    accRight *= -1;
                }

            }
            else
            {
                if (m_vertical > 0)
                {
                    accLeft *= -1;
                    accRight *= -1;
                }

                if (m_horizontal > 0) accRight *= 0.5f;
                else if (m_horizontal < 0) accLeft *= 0.5f;
            }
        }
    }

    /*
    Ajuda a debugar a suspensão desabilitando os elementos visuais, para ajudar a ver os gizmos
    */
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
