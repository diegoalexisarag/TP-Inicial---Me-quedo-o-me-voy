using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovimientoHorizontalTests
{
    private GameObject playerGameObject;
    private Controlar scriptControlar;
    private Rigidbody rb;

    [SetUp]
    public void Setup()
    {
        playerGameObject = new GameObject("PlayerTest");

        playerGameObject.AddComponent<CapsuleCollider>();
        rb = playerGameObject.AddComponent<Rigidbody>();

        // Crear Animator y asignar controlador temporal para evitar warnings
        Animator animator = playerGameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = new UnityEditor.Animations.AnimatorController();

        scriptControlar = playerGameObject.AddComponent<Controlar>();

        scriptControlar.walkSpeed = 3f;
        scriptControlar.runSpeed = 6f;
        scriptControlar.rotateSpeed = 15f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerGameObject);
    }

    [UnityTest]
    public IEnumerator CP01_DesplazamientoSimple_CambiaPosicionEnEjeHorizontal()
    {
        Vector3 posicionInicial = playerGameObject.transform.position;
        Vector3 direccionMovimiento = Vector3.forward;

        float velocidadUsada = scriptControlar.walkSpeed;
        rb.linearVelocity = direccionMovimiento * velocidadUsada;

        yield return new WaitForFixedUpdate();

        Assert.AreNotEqual(posicionInicial, playerGameObject.transform.position, "El personaje no cambió su posición al recibir input de movimiento.");
    }

    [UnityTest]
    public IEnumerator CP02_VariacionVelocidad_IncrementaVelocidadAlCorrer()
    {
        float velocidadCaminar = scriptControlar.walkSpeed;
        float velocidadCorrer = scriptControlar.runSpeed;

        Assert.Greater(velocidadCorrer, velocidadCaminar, "La velocidad al correr debe ser mayor que la velocidad al caminar.");

        rb.linearVelocity = Vector3.forward * velocidadCorrer;
        yield return new WaitForFixedUpdate();

        Assert.AreEqual(velocidadCorrer, rb.linearVelocity.magnitude, 0.1f, "La velocidad alcanzada no coincide con la velocidad de carrera.");
    }
}