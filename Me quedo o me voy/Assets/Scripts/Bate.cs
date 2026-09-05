using UnityEngine;
using System.Collections;

// Este script se encarga únicamente de mover el objeto que el jugador tiene
// equipado en la mano (por ejemplo el bate) cuando se ejecuta el golpe.
// Colocarlo en el mismo GameObject que "controlar" (el jugador).
public class Bate : MonoBehaviour
{
    [Header("Referencia a la mano")]
    public Transform mano; // Arrastrar acá el empty "Mano", hijo del hueso de la mano del avatar

    [Header("Parámetros del swing")]
    public Vector3 anguloSwing = new Vector3(0f, 0f, -90f); // Rotación local que gira el objeto al golpear
    public float duracionIda = 0.12f;
    public float duracionVuelta = 0.18f;

    private Coroutine swingCoroutine;

    // Llamar a este método (desde controlar.cs) en el momento del golpe.
    public void Golpear()
    {
        Transform objeto = ObtenerObjetoEquipado();
        if (objeto == null) return; // No tiene nada en la mano, no hay nada que mover

        if (swingCoroutine != null) StopCoroutine(swingCoroutine);
        swingCoroutine = StartCoroutine(SwingObjeto(objeto));
    }

    // Devuelve el objeto que está actualmente enganchado a la mano (el bate), o null si no tiene nada.
    private Transform ObtenerObjetoEquipado()
    {
        if (mano == null || mano.childCount == 0) return null;
        return mano.GetChild(0);
    }

    // Gira el objeto en la mano describiendo un golpe (ida y vuelta) sin depender de la animación del hueso.
    private IEnumerator SwingObjeto(Transform objeto)
    {
        Quaternion rotInicial = objeto.localRotation;
        Quaternion rotSwing = rotInicial * Quaternion.Euler(anguloSwing);

        float t = 0f;
        while (t < duracionIda)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracionIda);
            objeto.localRotation = Quaternion.Slerp(rotInicial, rotSwing, p);
            yield return null;
        }

        t = 0f;
        while (t < duracionVuelta)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duracionVuelta);
            objeto.localRotation = Quaternion.Slerp(rotSwing, rotInicial, p);
            yield return null;
        }

        objeto.localRotation = rotInicial;
    }
}