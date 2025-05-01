using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class pregunta : MonoBehaviour
{
    [SerializeField] public GameObject canvasPregunta;
    [SerializeField] public TextMeshProUGUI textoPregunta;
    [SerializeField] public TextMeshProUGUI[] textosRespuestas;
    [SerializeField] public string preguntaT;
    [SerializeField] public string re1;
    [SerializeField] public string re2;
    [SerializeField] public string re3;
    [SerializeField] public string re4;
    
    [SerializeField] public GameObject puerta;
    [SerializeField] public int repuestaC = 1; 
    [SerializeField] private float rangoDeteccion = 7f;
    private GameObject jugador;
    [SerializeField] public AudioSource rCorrecta;
    
    [SerializeField] public GameObject mensajeCorrecto;
    [SerializeField] public GameObject mensajeInCorrecto;  
    
    public FatherMoment1 fatherMoment1Script;

    
}
