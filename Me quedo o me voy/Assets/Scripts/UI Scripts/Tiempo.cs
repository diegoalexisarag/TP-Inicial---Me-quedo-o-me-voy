using UnityEngine;
using TMPro;

public class Tiempo : MonoBehaviour
{

    public static UIPauseManager pauseManager;

    public TextMeshProUGUI tiempoText;
    private float tiempoSegundos;
    private int tiempoMinutos;
    


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tiempoSegundos += Time.deltaTime; 
        if (tiempoSegundos >= 60f)
        {
            tiempoMinutos++;
            tiempoSegundos = 0f;
        }

        tiempoText.text = string.Format("{0:00}:{1:00}", tiempoMinutos, (int)tiempoSegundos); 
    }
}
