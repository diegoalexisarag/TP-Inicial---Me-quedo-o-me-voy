using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class controlar : MonoBehaviour {
	
	//public float speed = 10.0f;
	public float currentSpeed;
	public float airVelocity = 8f;
	public float gravity = 10.0f;
	//public float maxVelocityChange = 10.0f;
	public float jumpHeight = 2.0f;
	//public float maxFallSpeed = 20.0f;
	public float rotateSpeed = 15f;
	public float fuerzaDeEmpuje = 55.0f;
	public bool tienePoderDeEmpuje = false;
	private Vector3 moveDir;
	private Rigidbody rb;
	private Animator animator;

	private bool isRunning;
	private bool isMoving;
	public float walkSpeed = 3f;
	public float runSpeed = 6f;
	private float VerticalVelocity;

	private float distanciaAlSuelo;

	private bool canMove = true;
	private bool estaAturdido = false;
	private bool estuvoAturdido = false;
	private float pushForce;
	private Vector3 pushDir;

	public Vector3 checkPoint;
	private bool slide = false;

	//[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;

	//[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 70.0f;

	//[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -30.0f;

	//[Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
	//public float CameraAngleOverride = 0.0f;

	//[Tooltip("For locking the camera position on all axis")]
	//public bool LockCameraPosition = false;

	//[Tooltip("Mouse look sensitivity")]
	public float MouseSensitivity = 1.0f;

	// cinemachine
	private float _cinemachineTargetYaw;
	private float _cinemachineTargetPitch;

	private const float _threshold = 0.01f;

	void  Start (){
		animator = GetComponent<Animator>();
		distanciaAlSuelo = GetComponent<Collider>().bounds.extents.y;

		if (CinemachineCameraTarget != null)
			_cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
	}
	
	bool EstaEnElSuelo (){
    bool resultado = Physics.Raycast(transform.position, -Vector3.up, distanciaAlSuelo + 0.1f);
    Debug.Log("EstaEnElSuelo: " + resultado + " | distancia usada: " + (distanciaAlSuelo + 0.1f));
    return resultado;
}
	
	//bool EstaEnElSuelo (){
	//	return Physics.Raycast(transform.position, -Vector3.up, distanciaAlSuelo + 0.1f);
	//}
	
	void Awake () {
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		rb.useGravity = false;

		checkPoint = transform.position;
		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}
	
	void FixedUpdate () {
		if (canMove)
		{
			if (moveDir.x != 0 || moveDir.z != 0)
			{
				Vector3 targetDir = moveDir;

				targetDir.y = 0;
				if (targetDir == Vector3.zero)
					targetDir = transform.forward;
				    Quaternion targetRotation = Quaternion.LookRotation(targetDir);
    				transform.rotation = Quaternion.RotateTowards(
        			transform.rotation,
        			targetRotation,
        			rotateSpeed * 10f * Time.fixedDeltaTime
    				);
			}

			if (EstaEnElSuelo())
			{
				Vector3 targetVelocity = moveDir;
				targetVelocity *= currentSpeed;

				Vector3 velocity = rb.linearVelocity;
				if (targetVelocity.magnitude < velocity.magnitude)
				{
					targetVelocity = velocity;
					rb.linearVelocity /= 1.1f;
				}
				Vector3 velocityChange = (targetVelocity - velocity);
				//velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				//velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;
				if (!slide)
				{
					if (Mathf.Abs(rb.linearVelocity.magnitude) < currentSpeed * 1.0f)
						rb.AddForce(velocityChange, ForceMode.VelocityChange);
				}
				else if (Mathf.Abs(rb.linearVelocity.magnitude) < currentSpeed * 1.0f)
				{
					rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
				}

				// Jump
				if (EstaEnElSuelo() && Input.GetButton("Jump"))
				{
					rb.linearVelocity = new Vector3(velocity.x, CalcularVelocidadVerticalSalto(), velocity.z);
					animator.SetTrigger("Jump");
				}
			}
			else
			{
				if (!slide)
				{
					Vector3 targetVelocity = new Vector3(moveDir.x * airVelocity, rb.linearVelocity.y, moveDir.z * airVelocity);
					Vector3 velocity = rb.linearVelocity;
					Vector3 velocityChange = (targetVelocity - velocity);
					//velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
					//velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
					rb.AddForce(velocityChange, ForceMode.VelocityChange);
					//if (velocity.y < -maxFallSpeed)
					//	rb.linearVelocity = new Vector3(velocity.x, -maxFallSpeed, velocity.z);
				}
				else if (Mathf.Abs(rb.linearVelocity.magnitude) < currentSpeed * 1.0f)
				{
					rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
				}
			}
		}
		else
		{
			rb.linearVelocity = pushDir * pushForce;
		}
		rb.AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
	}

	private void Update()
	{
		isRunning = Input.GetKey(KeyCode.LeftShift);
		currentSpeed = (isRunning&&isMoving) ? runSpeed: walkSpeed;

		VerticalVelocity = rb.linearVelocity.y;

		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		Vector3 inputDir = new Vector3(h, 0, v);
		if (inputDir.sqrMagnitude > 0.0001f)
		{
			// Rotamos el input según hacia dónde mira la cámara (yaw), así "adelante" es "adelante de la cámara"
			float camYaw = CinemachineCameraTarget != null ? CinemachineCameraTarget.transform.eulerAngles.y : transform.eulerAngles.y;
			Quaternion camYawRotation = Quaternion.Euler(0f, camYaw, 0f);
			moveDir = (camYawRotation * inputDir).normalized;
		}
		else
		{
			moveDir = Vector3.zero;
		}
		if (Input.GetKeyDown(KeyCode.P) && EstaEnElSuelo() && !isMoving)
    	{
        animator.SetTrigger("Dance");
    	}
		if (Input.GetKeyDown(KeyCode.I) && EstaEnElSuelo() && !isMoving)
    	{
        animator.SetTrigger("Dance01");
    	}
		if (Input.GetKeyDown(KeyCode.O) && EstaEnElSuelo() && !isMoving)
    	{
        animator.SetTrigger("Dance02");
    	}
		
		RaycastHit hit;
		if (Physics.Raycast(transform.position, -Vector3.up, out hit, distanciaAlSuelo + 0.1f))
		{
			if (hit.transform.tag == "Slide")
			{
				slide = true;
			}
			else
			{
				slide = false;
			}
		}
		UpdateAnimations();
	}
	public void UpdateAnimations()
	{
			isMoving = moveDir.magnitude > 0.1f;

			animator.SetBool("IsMoving", isMoving);
			animator.SetBool("IsRunning", isRunning && isMoving);
			animator.SetBool("Grounded", EstaEnElSuelo());
			animator.SetFloat("VerticalVelocity", VerticalVelocity);
	}



	private void LateUpdate()
	{
		CameraRotation();
	}

	private void CameraRotation()
	{
		if (CinemachineCameraTarget == null)
			return;

		// Input del mouse (reemplaza a _input.look del nuevo Input System)
		Vector2 look = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * MouseSensitivity;

		// si hay input y la posición de cámara no está fija
		if (look.sqrMagnitude >= _threshold)
		{
			// el input de mouse no se multiplica por Time.deltaTime
			_cinemachineTargetYaw += look.x;
			_cinemachineTargetPitch -= look.y;
		}

		// clamp para que los valores queden limitados a 360 grados
		_cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
		_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

		// Cinemachine seguirá a este target
		CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch,
			_cinemachineTargetYaw, 0.0f);
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f) lfAngle += 360f;
		if (lfAngle > 360f) lfAngle -= 360f;
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	float CalcularVelocidadVerticalSalto () {
		return Mathf.Sqrt(2 * jumpHeight * gravity);
	}

	public void HitPlayer(Vector3 velocityF, float time)
	{
		rb.linearVelocity = velocityF;

		pushForce = velocityF.magnitude;
		pushDir = Vector3.Normalize(velocityF);
		StartCoroutine(Disminuir(velocityF.magnitude, time));
	}

	public void LoadCheckPoint()
	{
		transform.position = checkPoint;
	}

	private IEnumerator Disminuir(float value, float duration)
	{
		if (estaAturdido)
			estuvoAturdido = true;
		estaAturdido = true;
		canMove = false;

		float delta = 0;
		delta = value / duration;

		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			yield return null;
			if (!slide)
			{
				pushForce = pushForce - Time.deltaTime * delta;
				pushForce = pushForce < 0 ? 0 : pushForce;
			}
			rb.AddForce(new Vector3(0, -gravity * GetComponent<Rigidbody>().mass, 0));
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
		// Aumentamos la velocidad
		currentSpeed *= multiplicador;

		// Esperamos los segundos
		yield return new WaitForSeconds(tiempo);

		// Devolvemos la velocidad a la normalidad
		currentSpeed /= multiplicador;
	}
	public void ActivarSuperSalto(float multiplicador, float tiempo)
	{
		StartCoroutine(RutinaSalto(multiplicador, tiempo));
	}

	private IEnumerator RutinaSalto(float multiplicador, float tiempo)
	{
		jumpHeight *= multiplicador;

		yield return new WaitForSeconds(tiempo);

		jumpHeight /= multiplicador;
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

        if (otroRb != null && !otroRb.isKinematic && collision.gameObject.CompareTag("Player"))
        {
            Vector3 direccionEmpuje = collision.transform.position - transform.position;
            
            direccionEmpuje.y = 0; 
            
            direccionEmpuje = direccionEmpuje.normalized;

            otroRb.AddForce(direccionEmpuje * fuerzaDeEmpuje, ForceMode.Impulse);
        }
    }
	}
}