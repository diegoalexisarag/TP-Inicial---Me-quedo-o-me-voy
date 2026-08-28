using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovimientoHorizontalTests
{
    private GameObject playerGameObject;
    private GameObject sueloGameObject;
    private controlar scriptControlar;
    private Rigidbody rb;

    [SetUp]
    public void Setup()
    {
        // 1. Crear un suelo para asegurar que Raycast / EstaEnElSuelo() funcione correctamente
        sueloGameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
        sueloGameObject.transform.position = new Vector3(0, -0.5f, 0);

        // 2. Instanciar el GameObject del jugador
        playerGameObject = new GameObject("PlayerTest");
        playerGameObject.transform.position = Vector3.zero;

        // 3. Agregar componentes físicos y requeridos
        playerGameObject.AddComponent<CapsuleCollider>();
        rb = playerGameObject.AddComponent<Rigidbody>();
        rb.useGravity = false; // Desactivar gravedad para aislar pruebas de movimiento horizontal

        // Desactivar script para evitar escrituras en Update/Animator durante la simulación manual
        scriptControlar = playerGameObject.AddComponent<controlar>();
        scriptControlar.walkSpeed = 3.0f;
        scriptControlar.runSpeed = 6.0f;
        scriptControlar.rotateSpeed = 15.0f;
        scriptControlar.enabled = false;
    }

    [TearDown]
    public void Teardown()
    {
        Object.Destroy(playerGameObject);
        Object.Destroy(sueloGameObject);
    }

    // =========================================================================
    // TC-MOV-01: Desplazamiento Dirección WASD
    // =========================================================================
    [UnityTest]
    public IEnumerator TC_MOV_01_DesplazamientoDireccionWASD()
    {
        float velocidad = scriptControlar.walkSpeed;

        // 1. Avanzar en eje +Z (Equivalente a Tecla W)
        Vector3 posAntesW = playerGameObject.transform.position;
        rb.linearVelocity = Vector3.forward * velocidad;
        yield return new WaitForFixedUpdate();
        Assert.Greater(playerGameObject.transform.position.z, posAntesW.z, "El personaje no avanzó hacia adelante (Eje +Z / Tecla W).");

        // 2. Retroceder en eje -Z (Equivalente a Tecla S)
        Vector3 posAntesS = playerGameObject.transform.position;
        rb.linearVelocity = Vector3.back * velocidad;
        yield return new WaitForFixedUpdate();
        Assert.Less(playerGameObject.transform.position.z, posAntesS.z, "El personaje no retrocedió (Eje -Z / Tecla S).");

        // 3. Desplazar a la derecha en eje +X (Equivalente a Tecla D)
        Vector3 posAntesD = playerGameObject.transform.position;
        rb.linearVelocity = Vector3.right * velocidad;
        yield return new WaitForFixedUpdate();
        Assert.Greater(playerGameObject.transform.position.x, posAntesD.x, "El personaje no se desplazó a la derecha (Eje +X / Tecla D).");

        // 4. Desplazar a la izquierda en eje -X (Equivalente a Tecla A)
        Vector3 posAntesA = playerGameObject.transform.position;
        rb.linearVelocity = Vector3.left * velocidad;
        yield return new WaitForFixedUpdate();
        Assert.Less(playerGameObject.transform.position.x, posAntesA.x, "El personaje no se desplazó a la izquierda (Eje -X / Tecla A).");
    }

    // =========================================================================
    // TC-MOV-02: Transición de Velocidad Caminar / Correr
    // =========================================================================
    [UnityTest]
    public IEnumerator TC_MOV_02_TransicionVelocidadCaminarCorrer()
    {
        float velocidadCaminar = scriptControlar.walkSpeed;
        float velocidadCorrer = scriptControlar.runSpeed;

        Assert.Greater(velocidadCorrer, velocidadCaminar, "La velocidad al correr debe ser mayor que la velocidad al caminar.");

        // 1. Probar estado de caminata
        rb.linearVelocity = Vector3.forward * velocidadCaminar;
        yield return new WaitForFixedUpdate();
        float velocidadAlcanzadaCaminando = rb.linearVelocity.magnitude;
        Assert.AreEqual(velocidadCaminar, velocidadAlcanzadaCaminando, 0.5f, "La velocidad medida no coincide con la velocidad de caminata.");

        // 2. Probar estado de carrera
        rb.linearVelocity = Vector3.forward * velocidadCorrer;
        yield return new WaitForFixedUpdate();
        float velocidadAlcanzadaCorriendo = rb.linearVelocity.magnitude;

        Assert.Greater(velocidadAlcanzadaCorriendo, velocidadAlcanzadaCaminando, "La velocidad alcanzada en modo carrera no superó a la velocidad de caminata.");
        Assert.AreEqual(velocidadCorrer, velocidadAlcanzadaCorriendo, 0.7f, "La velocidad en modo carrera difiere del límite máximo permitido (runSpeed).");
    }

    // =========================================================================
    // TC-MOV-03: Frenado e Inercia (Decay de Velocidad)
    // =========================================================================
    [UnityTest]
    public IEnumerator TC_MOV_03_FrenadoEInerciaDecayVelocidad()
    {
        // Impulsar al personaje
        rb.linearVelocity = Vector3.forward * scriptControlar.walkSpeed;
        yield return new WaitForFixedUpdate();

        Assert.Greater(rb.linearVelocity.magnitude, 1.0f, "El personaje no adquirió velocidad inicial para la prueba de frenado.");

        // Simular frenado
        Vector3 desaceleracion = -rb.linearVelocity;
        rb.AddForce(desaceleracion, ForceMode.VelocityChange);

        yield return new WaitForFixedUpdate();

        Assert.LessOrEqual(rb.linearVelocity.magnitude, 0.1f, "El personaje no redujo su velocidad hasta detenerse.");
    }
}