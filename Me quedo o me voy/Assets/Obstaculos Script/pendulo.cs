using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pendulo : MonoBehaviour
{
    public float speed = 1.5f;
    public float limit = 75f;
    public bool randomStart = false;
    
    private float phaseOffset = 0f;
    
    // Variable para guardar la rotación de reposo inicial
    private float startZRotation; 
    private float startYRotation;
    private float startXRotation;

    void Awake()
    {
        // 1. Guardamos la rotación actual en Z que tiene el objeto en el Inspector (-63.193)
        startZRotation = transform.localEulerAngles.z;
        startYRotation = transform.localEulerAngles.y;
        startXRotation = transform.localEulerAngles.x;

        if (randomStart)
        {
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        // 2. Calculamos el ángulo de oscilación (-75 a +75)
        float anguloOscilacion = limit * Mathf.Cos((Time.time * speed) + phaseOffset);
        
        // 3. Sumamos esa oscilación a la rotación base para que pivote desde ahí
        transform.localRotation = Quaternion.Euler(startXRotation, startYRotation, startZRotation + anguloOscilacion);
    }
}
