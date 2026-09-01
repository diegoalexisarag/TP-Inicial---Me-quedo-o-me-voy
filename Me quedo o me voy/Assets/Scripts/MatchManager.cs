using UnityEngine;
using Unity.Netcode;
using TMPro;
using UnityEngine.UI;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instancia { get; private set; }

    [Header("UI del Juego")]
    [SerializeField] private TextMeshProUGUI textoCronometro;
    [SerializeField] private TextMeshProUGUI textoCentroPantalla;

    [Header("UI del Lobby (Overlay)")]
    [SerializeField] private GameObject panelLobby;
    [SerializeField] private TextMeshProUGUI textoContadorJugadores;
    [SerializeField] private Button botonEmpezar; // Solo el Host debe poder usarlo

    // Variables de estado sincronizadas
    public NetworkVariable<int> jugadoresConectados = new NetworkVariable<int>(0);
    public NetworkVariable<bool> enLobby = new NetworkVariable<bool>(true);
    public NetworkVariable<double> tiempoInicioCarrera = new NetworkVariable<double>(0);
    public NetworkVariable<double> tiempoFinCarrera = new NetworkVariable<double>(0);
    public NetworkVariable<bool> carreraIniciada = new NetworkVariable<bool>(false);

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // El servidor escucha quién entra y quién sale
            NetworkManager.Singleton.OnClientConnectedCallback += ActualizarContador;
            NetworkManager.Singleton.OnClientDisconnectCallback += ActualizarContador;
            jugadoresConectados.Value = 1; // Contamos al Host inmediatamente
        }

        // Seguridad: Solo activamos el botón de empezar en la pantalla del Host
        if (botonEmpezar != null)
        {
            botonEmpezar.gameObject.SetActive(IsServer);
            botonEmpezar.onClick.AddListener(IniciarPartidaDesdeLobby);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= ActualizarContador;
            NetworkManager.Singleton.OnClientDisconnectCallback -= ActualizarContador;
        }
    }

    private void ActualizarContador(ulong clientId)
    {
        if (IsServer)
        {
            jugadoresConectados.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
        }
    }

    private void IniciarPartidaDesdeLobby()
    {
        // El Host presiona el botón y arranca la secuencia
        if (IsServer)
        {
            enLobby.Value = false;
            tiempoInicioCarrera.Value = NetworkManager.Singleton.ServerTime.Time + 3.0;
            tiempoFinCarrera.Value = tiempoInicioCarrera.Value + 300.0;
        }
    }

    void Update()
    {
        if (!IsSpawned) return;
        double tiempoActual = NetworkManager.Singleton.ServerTime.Time;

        // Disparador del inicio de carrera
        if (IsServer && !enLobby.Value && !carreraIniciada.Value && tiempoActual >= tiempoInicioCarrera.Value)
        {
            carreraIniciada.Value = true;
        }

        ActualizarUI(tiempoActual);
    }

    private void ActualizarUI(double tiempoActual)
    {
        // 1. Lógica del Panel de Espera
        if (panelLobby != null)
        {
            panelLobby.SetActive(enLobby.Value);
            textoContadorJugadores.text = $"Jugadores: {jugadoresConectados.Value}/4";
        }

        // Si seguimos en el lobby, no procesamos la UI de la carrera
        if (enLobby.Value) return;

        // 2. Lógica de la Cuenta Regresiva
        double tiempoParaInicio = tiempoInicioCarrera.Value - tiempoActual;

        if (tiempoParaInicio > 0)
        {   
            textoCronometro.gameObject.SetActive(false);
            textoCentroPantalla.gameObject.SetActive(true);
            textoCentroPantalla.text = Mathf.CeilToInt((float)tiempoParaInicio).ToString();
        }
        else if (tiempoParaInicio > -1.0)
        {
            textoCentroPantalla.text = "¡YA!";
        }
        else
        {
            textoCentroPantalla.gameObject.SetActive(false);
            textoCronometro.gameObject.SetActive(true);
        }

        // 3. Lógica del Cronómetro Superior
        if (carreraIniciada.Value)
        {
            double tiempoRestante = tiempoFinCarrera.Value - tiempoActual;
            if (tiempoRestante <= 0) tiempoRestante = 0;

            int minutos = Mathf.FloorToInt((float)tiempoRestante / 60);
            int segundos = Mathf.FloorToInt((float)tiempoRestante % 60);
            textoCronometro.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }
}