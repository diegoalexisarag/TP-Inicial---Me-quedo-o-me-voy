using UnityEngine;
using System.Collections;

[RequireComponent (typeof (Rigidbody))]
[RequireComponent (typeof (CapsuleCollider))]

public class controlarAlJugador : MonoBehaviour {
	
	public float speed = 10.0f;
	public float airVelocity = 8f;
	public float gravity = 10.0f;
	public float maxVelocityChange = 10.0f;
	public float jumpHeight = 2.0f;
	public float maxFallSpeed = 20.0f;
	public float rotateSpeed = 25f;
	private Vector3 moveDir;
	private Rigidbody rb;

	private float distanciaAlSuelo;

	private bool canMove = true;
	private bool estaAturdido = false;
	private bool estuvoAturdido = false;
	private float pushForce;
	private Vector3 pushDir;

	public Vector3 checkPoint;
	private bool slide = false;

	void  Start (){
		distanciaAlSuelo = GetComponent<Collider>().bounds.extents.y;
	}
	
	bool EstaEnElSuelo (){
		return Physics.Raycast(transform.position, -Vector3.up, distanciaAlSuelo + 0.1f);
	}
	
	void Awake () {
		rb = GetComponent<Rigidbody>();
		rb.freezeRotation = true;
		rb.useGravity = false;

		checkPoint = transform.position;
		Cursor.visible = false;
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
				Quaternion tr = Quaternion.LookRotation(targetDir);
				Quaternion targetRotation = Quaternion.Slerp(transform.rotation, tr, Time.deltaTime * rotateSpeed);
				transform.rotation = targetRotation;
			}

			if (EstaEnElSuelo())
			{
				Vector3 targetVelocity = moveDir;
				targetVelocity *= speed;

				Vector3 velocity = rb.linearVelocity;
				if (targetVelocity.magnitude < velocity.magnitude)
				{
					targetVelocity = velocity;
					rb.linearVelocity /= 1.1f;
				}
				Vector3 velocityChange = (targetVelocity - velocity);
				velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
				velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
				velocityChange.y = 0;
				if (!slide)
				{
					if (Mathf.Abs(rb.linearVelocity.magnitude) < speed * 1.0f)
						rb.AddForce(velocityChange, ForceMode.VelocityChange);
				}
				else if (Mathf.Abs(rb.linearVelocity.magnitude) < speed * 1.0f)
				{
					rb.AddForce(moveDir * 0.15f, ForceMode.VelocityChange);
				}

				// Jump
				if (EstaEnElSuelo() && Input.GetButton("Jump"))
				{
					rb.linearVelocity = new Vector3(velocity.x, CalcularVelocidadVerticalSalto(), velocity.z);
				}
			}
			else
			{
				if (!slide)
				{
					Vector3 targetVelocity = new Vector3(moveDir.x * airVelocity, rb.linearVelocity.y, moveDir.z * airVelocity);
					Vector3 velocity = rb.linearVelocity;
					Vector3 velocityChange = (targetVelocity - velocity);
					velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
					velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
					rb.AddForce(velocityChange, ForceMode.VelocityChange);
					if (velocity.y < -maxFallSpeed)
						rb.linearVelocity = new Vector3(velocity.x, -maxFallSpeed, velocity.z);
				}
				else if (Mathf.Abs(rb.linearVelocity.magnitude) < speed * 1.0f)
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
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");

		Vector3 v2 = v * Vector3.forward; 
		Vector3 h2 = h * Vector3.right; 
		moveDir = (v2 + h2).normalized;
		
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
}
