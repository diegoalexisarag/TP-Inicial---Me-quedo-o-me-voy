using UnityEngine;

public class RotacionContinua : MonoBehaviour
{
    [Tooltip("Velocidad a la que gira la daga (grados por segundo). Usa números negativos para girar al revés.")]
    public float speed = 250f;
    
    [Tooltip("El eje sobre el cual girará. Por defecto es el eje Y (0, 1, 0).")]
    public Vector3 ejeRotacion = Vector3.up;
    

    void Update()
    {
        // Rota el objeto continuamente sumando grados a su rotación actual
        transform.Rotate(ejeRotacion * speed * Time.deltaTime);
    }
}