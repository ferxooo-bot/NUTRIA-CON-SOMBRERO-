using UnityEngine;
using System.Collections;

public class RespawnZone : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
    
            FatherMoment1 fatherMoment1 = other.GetComponent<FatherMoment1>();
            PlayerDataSynchronizer dataSynchronizer = other.GetComponent<PlayerDataSynchronizer>();
            if (fatherMoment1 != null)
            {
                // Actualizar respawn
                fatherMoment1.SetRespawnPoint(this.name); 

                // Guardar datos
                if (dataSynchronizer != null)
                {
                    dataSynchronizer.UpdateSaveWithPlayerData();
                    Debug.Log("sincronizando con la base de datos todo sobre el jugador");
                }
            }
        }
    }
}
