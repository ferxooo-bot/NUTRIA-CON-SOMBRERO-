using UnityEngine;

/// <summary>
/// Inicializador para escenas de cinemática.
/// Este script debe añadirse a la escena de cinemática para configurar 
/// el retorno automático al nivel correcto.
/// </summary>
public class CinematicInitializer : MonoBehaviour
{
    private void Start()
    {
        // Si existe una instancia del CinematicSystem, configuramos la escena
        if (CinematicSystem.Instance != null)
        {
            CinematicSystem.Instance.SetupCinematicScene();
        }
        else
        {
            Debug.LogWarning("CinematicSystem no encontrado. La cinemática no volverá automáticamente al nivel anterior.");
        }
    }
}