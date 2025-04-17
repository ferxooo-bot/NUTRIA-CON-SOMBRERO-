using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player"; // Player tag to identify the player
    [SerializeField] private Transform plataform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object colliding has the "Player" tag
        if (other.CompareTag(playerTag))
        {
            other.transform.SetParent(plataform); // Make the player a child of the platform
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object colliding has the "Player" tag
        if (other.CompareTag(playerTag))
        {
            other.transform.SetParent(null); // Remove the player from the platform
        }
    }
    
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
