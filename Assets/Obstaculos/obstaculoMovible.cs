using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class obstaculoMovible : MonoBehaviour
{
	public float distancia = 5f;
	public bool horizontal = true;
	public float speed = 3f;
	public float offset = 0f;

	private bool haciaAdelante = true; 
	private Vector3 startPos;
   
    void Awake()
    {
		startPos = transform.position;
		if (horizontal)
			transform.position += Vector3.right * offset;
		else
			transform.position += Vector3.forward * offset;
	}

    void Update()
    {
		if (horizontal)
		{
			if (haciaAdelante)
			{
				if (transform.position.x < startPos.x + distancia)
				{
					transform.position += Vector3.right * Time.deltaTime * speed;
				}
				else
					haciaAdelante = false;
			}
			else
			{
				if (transform.position.x > startPos.x)
				{
					transform.position -= Vector3.right * Time.deltaTime * speed;
				}
				else
					haciaAdelante = true;
			}
		}
		else
		{
			if (haciaAdelante)
			{
				if (transform.position.z < startPos.z + distancia)
				{
					transform.position += Vector3.forward * Time.deltaTime * speed;
				}
				else
					haciaAdelante = false;
			}
			else
			{
				if (transform.position.z > startPos.z)
				{
					transform.position -= Vector3.forward * Time.deltaTime * speed;
				}
				else
					haciaAdelante = true;
			}
		}
    }
}
