using UnityEngine;

// Clase auxiliar para almacenar datos de reciclaje por tipo
[System.Serializable]
public class ReciclajeData
{
    public int botellaPlastico;
    public int botellaVidrio;
    public int lata;
}

public class ControladorCheckpoint : MonoBehaviour
{
    public static ControladorCheckpoint Instance;

    [Header("Estado del último checkpoint")]
    [Tooltip("Número de checkpoint alcanzado (1 a 4)")]
    public int CheckpointAlcanzado = 0;

    [Header("Datos por Checkpoint")]
    [Tooltip("Índice 0→Checkpoint1, 1→Checkpoint2, ...")]
    public ReciclajeData[] reciclajePorCheckpoint = new ReciclajeData[4];

    [Header("Total acumulado")]
    public ReciclajeData reciclajeTotal = new ReciclajeData();

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            // Inicializar arrays
            for (int i = 0; i < reciclajePorCheckpoint.Length; i++)
                reciclajePorCheckpoint[i] = new ReciclajeData();
        }
        else Destroy(gameObject);
    }

    /// <summary>
    /// Llamar cuando se alcanza un checkpoint
    /// </summary>
    /// <param name="numeroCheckpoint">Del 1 al 4</param>
    public void GuardarCheckpoint(int numeroCheckpoint)
    {
        if (numeroCheckpoint < 1 || numeroCheckpoint > 4)
        {
            Debug.LogError("Número de checkpoint inválido: " + numeroCheckpoint);
            return;
        }

        // Actualizar cuál fue el último alcanzado
        CheckpointAlcanzado = numeroCheckpoint;

        // Obtener instacia del contador de objetos
        var cont = ContadorObjetos.Instance;
        int idx = numeroCheckpoint - 1;

        // Guardar los datos actuales (solo 3 tipos)
        reciclajePorCheckpoint[idx].botellaPlastico = cont.cantidadBotellaPlastico;
        reciclajePorCheckpoint[idx].botellaVidrio = cont.cantidadBotellaVidrio;
        reciclajePorCheckpoint[idx].lata = cont.cantidadLata;

        // Recalcular total acumulado
        reciclajeTotal.botellaPlastico = 0;
        reciclajeTotal.botellaVidrio = 0;
        reciclajeTotal.lata = 0;
        for (int i = 0; i < reciclajePorCheckpoint.Length; i++)
        {
            reciclajeTotal.botellaPlastico += reciclajePorCheckpoint[i].botellaPlastico;
            reciclajeTotal.botellaVidrio += reciclajePorCheckpoint[i].botellaVidrio;
            reciclajeTotal.lata += reciclajePorCheckpoint[i].lata;
        }

        Debug.Log($"Checkpoint {numeroCheckpoint} Guardado → " +
                  $"Plástico: {reciclajePorCheckpoint[idx].botellaPlastico}, " +
                  $"Vidrio: {reciclajePorCheckpoint[idx].botellaVidrio}, " +
                  $"Latas: {reciclajePorCheckpoint[idx].lata}");

        Debug.Log($"Total acumulado → Plástico: {reciclajeTotal.botellaPlastico}, " +
                  $"Vidrio: {reciclajeTotal.botellaVidrio}, " +
                  $"Latas: {reciclajeTotal.lata}");

        // Reiniciar contadores (solo los tres relevantes)
        cont.cantidadBotellaPlastico = 0;
        cont.cantidadBotellaVidrio = 0;
        cont.cantidadLata = 0;
    }
}
