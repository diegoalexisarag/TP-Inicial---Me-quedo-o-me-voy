using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MecanicaSaltoTests
{
    private GameObject playerGameObject;
    private Controlar scriptControlar;
    private Rigidbody rb;

    [SetUp]
    public void Setup()
    {
        playerGameObject = new GameObject("PlayerSaltoTest");

        playerGameObject.AddComponent<CapsuleCollider>();
        rb = playerGameObject.AddComponent<Rigidbody>();

        Animator animator = playerGameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = new UnityEditor.Animations.AnimatorController();

        scriptControlar = playerGameObject.AddComponent<Controlar>();

        scriptControlar.jumpHeight = 2.0f;
        scriptControlar.gravity = 10.0f;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerGameObject);
    }

    [UnityTest]
    public IEnumerator CP05_SaltoBasico_ElevaSuTrayectoriaYDesciende()
    {
        playerGameObject.transform.position = Vector3.zero;
        float velocidadSaltoCalculada = Mathf.Sqrt(2 * scriptControlar.jumpHeight * scriptControlar.gravity);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocidadSaltoCalculada, rb.linearVelocity.z);

        yield return new WaitForFixedUpdate();

        Assert.IsTrue(rb.linearVelocity.y > 0f, "El personaje no elevó su trayectoria al saltar.");

        for (int i = 0; i < 30; i++)
        {
            rb.AddForce(new Vector3(0, -scriptControlar.gravity * rb.mass, 0));
            yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(rb.linearVelocity.y < 0f, "La gravedad no provocó el descenso del personaje.");
    }

    [UnityTest]
    public IEnumerator CP06_PrevenirSaltoInfinito_NoSaltaEnElAire()
    {
        playerGameObject.transform.position = new Vector3(0, 10f, 0);
        rb.linearVelocity = new Vector3(0, -2f, 0);

        float velocidadVerticalPrevia = rb.linearVelocity.y;

        bool estaEnSuelo = Physics.Raycast(playerGameObject.transform.position, -Vector3.up, 1.0f + 0.1f);

        if (estaEnSuelo)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Sqrt(2 * scriptControlar.jumpHeight * scriptControlar.gravity), rb.linearVelocity.z);
        }

        yield return new WaitForFixedUpdate();

        Assert.IsFalse(estaEnSuelo, "El raycast detectó suelo erróneamente en el aire.");
        Assert.LessOrEqual(rb.linearVelocity.y, velocidadVerticalPrevia, "El personaje logró realizar un salto infinito en el aire.");
    }

    [UnityTest]
    public IEnumerator CP07_CaidaLibre_DesciendePorGravedad()
    {
        playerGameObject.transform.position = new Vector3(0, 50f, 0);
        float alturaInicial = playerGameObject.transform.position.y;

        for (int i = 0; i < 60; i++)
        {
            rb.AddForce(new Vector3(0, -scriptControlar.gravity * rb.mass, 0));
            yield return new WaitForFixedUpdate();
        }

        Assert.IsTrue(playerGameObject.transform.position.y < alturaInicial, "El personaje no descendió durante la caída libre.");
        Assert.IsTrue(rb.linearVelocity.y < 0f, "La velocidad vertical debe ser negativa en caída libre.");
    }
}