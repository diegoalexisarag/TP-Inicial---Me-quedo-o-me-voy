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
    
    [Header("UI de Victoria")]
    [SerializeField] bool pantallaVictoriaMostrada = false;
    [SerializeField] private GameObject panelGameOver; 
    [SerializeField] private TextMeshProUGUI textoGanador; 
    [SerializeField] private TextMeshProUGUI textoTiempoFinal; 

    [Header("UI de Tiempo acabado")]
    [SerializeField] bool pantallaTiempoAcabadoMostrada = false;
    [SerializeField] private GameObject panelTiempoAcabado; 

    public NetworkVariable<bool> carreraFinalizada = new NetworkVariable<bool>(false);
    public NetworkVariable<ulong> idGanador = new NetworkVariable<ulong>(0);

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
            NetworkManager.Singleton.OnClientConnectedCallback += ActualizarContador;
            NetworkManager.Singleton.OnClientDisconnectCallback += ActualizarContador;
            jugadoresConectados.Value = 1;
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


        if (IsServer && !enLobby.Value && !carreraIniciada.Value && tiempoActual >= tiempoInicioCarrera.Value)
        {
            carreraIniciada.Value = true;
        }

        double tiempoRestante = tiempoFinCarrera.Value - tiempoActual;

        if (IsServer && carreraIniciada.Value && !carreraFinalizada.Value && tiempoRestante <= 0)
        {
            carreraFinalizada.Value = true;
            enLobby.Value = false; 
        }


        ActualizarUI(tiempoActual);
    }

    private void ActualizarUI(double tiempoActual)
    {

        if (carreraFinalizada.Value)
        {   
            if (tiempoFinCarrera.Value - tiempoActual <= 0)
            {
                MostrarPantallaTiempoAcabado();
            }
            else
            {
                MostrarPantallaVictoria();
            }
            return;
        }
        else
        {
            if (panelGameOver != null) panelGameOver.SetActive(false);
        }

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
        else if (!pantallaVictoriaMostrada || !pantallaTiempoAcabadoMostrada)
        {
            textoCentroPantalla.gameObject.SetActive(false);
            textoCronometro.gameObject.SetActive(true);
        }

        // 3. Lógica del Cronómetro Superior
        if (carreraIniciada.Value)
        {
            textoCronometro.text = calcularTiempoRestante(tiempoActual);
        }
    }
    
    public void DeclararGanador(ulong clientId)
    {
        if (IsServer)
        {
            idGanador.Value = clientId;
            carreraFinalizada.Value = true;
            enLobby.Value = false; 
        }
    }

    private void MostrarPantallaVictoria()
    {
        double tiempoActual = NetworkManager.Singleton.ServerTime.Time;
        panelGameOver.SetActive(true);
        textoCronometro.gameObject.SetActive(false);
        textoGanador.text = $"Ganador: Jugador {idGanador.Value}";
        textoCronometro.gameObject.SetActive(false);

        if (!pantallaVictoriaMostrada)
        {
            textoTiempoFinal.text = $"Tiempo restante: {calcularTiempoRestante(tiempoActual)}";
        }

        pantallaVictoriaMostrada = true;
    }

    private void MostrarPantallaTiempoAcabado()
    {
        panelTiempoAcabado.SetActive(true);
        textoCronometro.gameObject.SetActive(false);
        pantallaTiempoAcabadoMostrada = true;
    }

    private string calcularTiempoRestante(double tiempoActual){
        double tiempoRestante = tiempoFinCarrera.Value - tiempoActual;
        if (tiempoRestante <= 0) tiempoRestante = 0;

        int minutos = Mathf.FloorToInt((float)tiempoRestante / 60);
        int segundos = Mathf.FloorToInt((float)tiempoRestante % 60);
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }
}