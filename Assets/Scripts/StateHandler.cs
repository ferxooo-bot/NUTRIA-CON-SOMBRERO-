using UnityEngine;

public class StateHandler : MonoBehaviour
{
    public FatherMoment1 fatherMoment; 
    public MovementSwin movementSwin; 
    //---------------------
    public GameObject bones;  
    public GameObject sprite;
    //---------------------
    
    // Add a small buffer to prevent rapid state switching
    private float stateChangeBufferTime = 0.2f;
    private float lastStateChangeTime;
    private bool isInWater = false;
    
    void Start()
    {
        lastStateChangeTime = -stateChangeBufferTime; // Allow immediate state change on start
        CheckInitialState();
    }

    void CheckInitialState()
    {
        // Check if character starts touching a water collider
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.1f);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                isInWater = true;
                ActivateWaterMode();
                return;
            }
        }

        isInWater = false;
        ActivateLandMode();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            if (!isInWater && Time.time >= lastStateChangeTime + stateChangeBufferTime)
            {
                isInWater = true;
                lastStateChangeTime = Time.time;
                ActivateWaterMode();
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
        {
            // Only exit water mode if we're not inside any other water colliders
            bool stillInWater = false;
            Collider2D[] currentColliders = Physics2D.OverlapCircleAll(transform.position, 0.1f);
            
            foreach (Collider2D col in currentColliders)
            {
                if (col.gameObject.layer == LayerMask.NameToLayer("Water") && col != other)
                {
                    stillInWater = true;
                    break;
                }
            }
            
            if (!stillInWater && isInWater && Time.time >= lastStateChangeTime + stateChangeBufferTime)
            {
                isInWater = false;
                lastStateChangeTime = Time.time;
                ActivateLandMode();
            }
        }
    }

    public void ActivateWaterMode()
{
    ShowBones();
    if (fatherMoment != null) {
        // Sync the lookRight state before disabling
        movementSwin.lookRight = fatherMoment.lookRight;
        
        // If the parent scale is flipped, reset it and adjust the child scale accordingly
        if (transform.localScale.x < 0) {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), 
                                             transform.localScale.y, 
                                             transform.localScale.z);
            Vector3 bonesScale = bones.transform.localScale;
            bonesScale.x *= -1;
            bones.transform.localScale = bonesScale;
        }
        
        fatherMoment.enabled = false;
    }
    if (movementSwin != null) movementSwin.enabled = true;
}

public void ActivateLandMode()
{
    ShowSprite();
    if (movementSwin != null) {
        // Sync the lookRight state before disabling
        fatherMoment.lookRight = movementSwin.lookRight;
        
        // Reset the child transform scale and apply direction to parent instead
        Vector3 bonesScale = bones.transform.localScale;
        bonesScale.x = Mathf.Abs(bonesScale.x);
        bones.transform.localScale = bonesScale;
        
        // If needed, flip the parent transform based on lookRight
        if (!movementSwin.lookRight && transform.localScale.x > 0) {
            transform.localScale = new Vector3(-transform.localScale.x, 
                                             transform.localScale.y, 
                                             transform.localScale.z);
        } else if (movementSwin.lookRight && transform.localScale.x < 0) {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), 
                                             transform.localScale.y, 
                                             transform.localScale.z);
        }
        
        movementSwin.enabled = false;
    }
    if (fatherMoment != null) fatherMoment.enabled = true;
}
    //-----------------------------------------
    public void ShowBones()
    {
        if (bones != null) SetBonesChildrenVisibility(true);
        if (sprite != null) SetSpriteVisibility(false);
    }

    public void ShowSprite()
    {
        if (bones != null) SetBonesChildrenVisibility(false);
        if (sprite != null) SetSpriteVisibility(true);
    }

    private void SetBonesChildrenVisibility(bool visible)
    {
        SpriteRenderer[] spriteRenderers = bones.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = visible;
        }
    }

    private void SetSpriteVisibility(bool visible)
    {
        SpriteRenderer sr = sprite.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = visible;
        }
    }
    
    // Optional: Add this method to help debug the issue
    void OnDrawGizmos()
    {
        // Draw a sphere to visualize the overlap check area
        Gizmos.color = isInWater ? Color.blue : Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.1f);
    }
}