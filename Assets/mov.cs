using System;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
 // Reference to the platform that the player will follow
    public float speed = 2f; // Speed at which the platform moves
    public float minX = 110f; // Min X position for platform movement
    public float maxX = 130f; // Max X position for platform movement

    private int direction = 1; // Movement direction (1 = right, -1 = left)
    

    void Update()
    {
        // Move the platform horizontally
        transform.Translate(Vector2.right * speed * direction * Time.deltaTime);

        // Change direction when platform reaches the limits
        if (transform.position.x >= maxX)
        {
            direction = -1; // Move left
        }
        else if (transform.position.x <= minX)
        {
            direction = 1; // Move right
        }
    }
}