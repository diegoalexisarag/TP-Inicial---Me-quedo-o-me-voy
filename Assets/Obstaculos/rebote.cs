using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rebote : MonoBehaviour
{
	public float fuerza = 10f;
	public float tiempoAturdimiento = 0.5f;
	private Vector3 direccionGolpe;

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			Debug.DrawRay(contact.point, contact.normal, Color.white);
			if (collision.gameObject.tag == "Player")
			{
				direccionGolpe = contact.normal;
				collision.gameObject.GetComponent<CharacterControls>().HitPlayer(-direccionGolpe * fuerza, tiempoAturdimiento);
				return;
			}
		}
	}
}
