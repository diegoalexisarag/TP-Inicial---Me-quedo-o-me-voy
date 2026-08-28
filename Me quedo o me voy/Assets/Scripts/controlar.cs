using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class Controlar : MonoBehaviour
{
    [Header("Configuración de Velocidad")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float airVelocity = 8f;
    public float gravity = 10.0f;
    public float jumpHeight = 2.0f;
    public float rotateSpeed = 15f;
    
    [Header("Configuración de Empuje")]
    public float fuerzaDeEmpuje = 55.0f;
    public bool tienePoderDeEmpuje = false;

    [Header("Cámara Cinemachine")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 70.0f;
    public float BottomClamp = -30.0f;
    public float MouseSensitivity = 1.0f;

    // Variables internas optimizadas
    private float currentSpeed;
    private float VerticalVelocity;
    private float pushForce;
    private Vector3 pushDir;
    private Vector3 moveDir;
    private Vector3 checkPoint;
    private float distanciaAlSuelo;
    private float masaRigidbody; // Caché de la masa

    // Estados
    private bool isRunning;
    private bool isMoving;
    private bool canMove = true;
    private bool estaAturdido = false;
    private bool estuvoAturdido = false;
    private bool slide = false;

    // Componentes cacheados
    private Rigidbody rb;
    private Animator animator;

    // Hashes de Animación (Optimización masiva)
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int groundedHash = Animator.StringToHash("Grounded");
    private readonly int verticalVelocityHash = Animator.StringToHash("VerticalVelocity");
    private readonly int jumpHash = Animator.StringToHash("Jump");
    private readonly int danceHash = Animator.StringToHash("Dance");
    private readonly int dance01Hash = Animator.StringToHash("Dance01");
    private readonly int dance02Hash = Animator.StringToHash("Dance02");

    // Cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        
        rb.freezeRotation = true;
        rb.useGravity = false;
        masaRigidbody = rb.mass; // Guardamos la masa una sola vez al inicio

        distanciaAlSuelo = GetComponent<Collider>().bounds.extents.y;
        checkPoint = transform.position;
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Start()
    {
        if (CinemachineCameraTarget != null)
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    void Update()
    {
        // 1. Recolección de Inputs
        isRunning = Input.GetKey(KeyCode.LeftShift);
        currentSpeed = (isRunning && isMoving) ? runSpeed : walkSpeed;
        VerticalVelocity = rb.linearVelocity.y;

        float h = Input.GetAxisRaw("Horizontal"); // GetAxisRaw elimina el input smoothing nativo (mejor respuesta)
        float v = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = new Vector3(h, 0, v).normalized;

        if (inputDir.sqrMagnitude > 0.1f)
        {
            float camYaw = CinemachineCameraTarget != null ? CinemachineCameraTarget.transform.eulerAngles.y : transform.eulerAngles.y;
            Quaternion camYawRotation = Quaternion.Euler(0f, camYaw, 0f);
            moveDir = camYawRotation * inputDir;
            isMoving = true;
        }
        else
        {
            moveDir = Vector3.zero;
            isMoving = false;
        }

        // 2. Detección de Suelo y Superficies Deslizantes
        ManejarDeteccionDeSuelo();

        // 3. Inputs de Animación y Salto
        ManejarInputsAcciones();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (canMove)
        {
            MoverJugadorFisicas();
            RotarJugadorFisicas();
        }
        else
        {
            rb.linearVelocity = pushDir * pushForce;
        }

        // Gravedad extra optimizada (sin GetComponent)
        rb.AddForce(new Vector3(0, -gravity * masaRigidbody, 0));
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    #region Lógica de Movimiento y Físicas

    private void MoverJugadorFisicas()
    {
        bool enSuelo = EstaEnElSuelo();
        Vector3 targetVelocity = moveDir * (enSuelo ? currentSpeed : airVelocity);
        Vector3 velocity = rb.linearVelocity;
        
        // Frenado preciso y responsivo
        if (moveDir == Vector3.zero && enSuelo && !slide)
        {
            // Forzamos la velocidad horizontal a cero inmediatamente para evitar deslizamientos
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        Vector3 velocityChange = (targetVelocity - velocity);
        velocityChange.y = 0; // Mantenemos intacta la velocidad vertical (gravedad/salto)

        if (!slide || (slide && velocity.magnitude < currentSpeed))
        {
            // Usamos un multiplicador para que la aceleración en el aire sea más suave que en el suelo
            float aceleracion = enSuelo ? 1f : 0.5f; 
            rb.AddForce(velocityChange * aceleracion, ForceMode.VelocityChange);
        }
        else if (slide)
        {
            rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
        }
    }

    private void RotarJugadorFisicas()
    {
        if (moveDir != Vector3.zero)
        {
            Vector3 targetDir = moveDir;
            targetDir.y = 0;
            
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            // IMPORTANTE: Usar MoveRotation en lugar de transform.rotation previene los temblores de físicas
            Quaternion nuevaRotacion = Quaternion.RotateTowards(rb.rotation, targetRotation, rotateSpeed * 10f * Time.fixedDeltaTime);
            rb.MoveRotation(nuevaRotacion); 
        }
    }

    private void ManejarInputsAcciones()
    {
        if (EstaEnElSuelo())
        {
            if (Input.GetButton("Jump"))
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, CalcularVelocidadVerticalSalto(), rb.linearVelocity.z);
                animator.SetTrigger(jumpHash);
            }
            
            if (!isMoving)
            {
                if (Input.GetKeyDown(KeyCode.P)) animator.SetTrigger(danceHash);
                if (Input.GetKeyDown(KeyCode.I)) animator.SetTrigger(dance01Hash);
                if (Input.GetKeyDown(KeyCode.O)) animator.SetTrigger(dance02Hash);
            }
        }
    }

    private void ManejarDeteccionDeSuelo()
    {
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit, distanciaAlSuelo + 0.1f))
        {
            // Optimización: CompareTag es mucho más rápido y no genera basura en memoria
            slide = hit.collider.CompareTag("Slide");
        }
        else
        {
            slide = false;
        }
    }

    private bool EstaEnElSuelo()
    {
        return Physics.Raycast(transform.position, -Vector3.up, distanciaAlSuelo + 0.1f);
    }

    #endregion

    #region Cámara y Animaciones

    private void CameraRotation()
    {
        if (CinemachineCameraTarget == null) return;

        Vector2 look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * MouseSensitivity;

        if (look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetYaw += look.x;
            _cinemachineTargetPitch -= look.y;
        }

        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void UpdateAnimations()
    {
        // Optimización: Usamos Hashes enteros en lugar de buscar strings
        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isRunningHash, isRunning && isMoving);
        animator.SetBool(groundedHash, EstaEnElSuelo());
        animator.SetFloat(verticalVelocityHash, VerticalVelocity);
    }

    #endregion

    #region Utilidades y Corrutinas

    float CalcularVelocidadVerticalSalto()
    {
        return Mathf.Sqrt(2 * jumpHeight * gravity);
    }

    public void HitPlayer(Vector3 velocityF, float time)
    {
        rb.linearVelocity = velocityF;
        pushForce = velocityF.magnitude;
        pushDir = velocityF.normalized;
        StartCoroutine(Disminuir(velocityF.magnitude, time));
    }

    public void LoadCheckPoint()
    {
        // Desactivar físicas temporalmente al teletransportar previene bugs visuales
        rb.isKinematic = true; 
        transform.position = checkPoint;
        rb.isKinematic = false;
    }

    private IEnumerator Disminuir(float value, float duration)
    {
        if (estaAturdido) estuvoAturdido = true;
        estaAturdido = true;
        canMove = false;

        float delta = value / duration;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            yield return null;
            if (!slide)
            {
                pushForce -= Time.deltaTime * delta;
                pushForce = Mathf.Max(pushForce, 0); // Más limpio que el ternario
            }
            rb.AddForce(new Vector3(0, -gravity * masaRigidbody, 0));
        }

        if (estuvoAturdido)
        {
            estuvoAturdido = false;
        }
        else
        {
            estaAturdido = false;
            canMove = true;
        }
    }

    public void ActivarSpeedBoost(float multiplicador, float tiempo)
    {
        StartCoroutine(RutinaVelocidad(multiplicador, tiempo));
    }

    private IEnumerator RutinaVelocidad(float multiplicador, float tiempo)
    {
        float baseWalk = walkSpeed;
        float baseRun = runSpeed;
        
        walkSpeed *= multiplicador;
        runSpeed *= multiplicador;

        yield return new WaitForSeconds(tiempo);

        walkSpeed = baseWalk;
        runSpeed = baseRun;
    }

    public void ActivarSuperSalto(float multiplicador, float tiempo)
    {
        StartCoroutine(RutinaSalto(multiplicador, tiempo));
    }

    private IEnumerator RutinaSalto(float multiplicador, float tiempo)
    {
        float baseJump = jumpHeight;
        jumpHeight *= multiplicador;
        yield return new WaitForSeconds(tiempo);
        jumpHeight = baseJump;
    }

    public void ActivarPoderEmpuje(float tiempo)
    {
        StartCoroutine(RutinaPoderEmpuje(tiempo));
    }

    private IEnumerator RutinaPoderEmpuje(float tiempo)
    {
        tienePoderDeEmpuje = true;
        yield return new WaitForSeconds(tiempo);
        tienePoderDeEmpuje = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (tienePoderDeEmpuje)
        {
            Rigidbody otroRb = collision.collider.GetComponent<Rigidbody>();

            // Optimización: CompareTag en lugar de == "Player"
            if (otroRb != null && !otroRb.isKinematic && collision.gameObject.CompareTag("Player"))
            {
                Vector3 direccionEmpuje = collision.transform.position - transform.position;
                direccionEmpuje.y = 0;
                direccionEmpuje = direccionEmpuje.normalized;

                otroRb.AddForce(direccionEmpuje * fuerzaDeEmpuje, ForceMode.Impulse);
            }
        }
    }
    #endregion
}