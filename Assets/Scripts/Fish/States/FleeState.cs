using UnityEngine;
using System.Collections;

public class FleeState : FishMovementState
{
    private Transform threat;
    private Vector2 dangerPosition;
    private float fleeRadius;
    private float maxFleeSpeed;
    private float originalMaxSpeed;
    private float panicTime;
    private float panicTimer;
    private bool usePositionInsteadOfTransform;
    private Vector2 fleeDirection;
    private float directionUpdateInterval = 0.3f;
    private float directionTimer;
    private float recoverySpeed = 0.8f; // Recovery speed modifier when swimming away

    public FleeState(AquaticCreatureMovement movement, Transform threat, float fleeRadius, float maxFleeSpeed)
        : base(movement)
    {
        this.threat = threat;
        this.fleeRadius = fleeRadius;
        this.maxFleeSpeed = maxFleeSpeed;
        this.panicTime = Random.Range(1.0f, 2.5f); // Random panic time after losing sight of threat
        
        // Initialize with a valid danger position even if threat is null
        if (threat != null)
        {
            dangerPosition = threat.position;
            usePositionInsteadOfTransform = false;
        }
        else
        {
            dangerPosition = movement.transform.position; // Will be updated later
            usePositionInsteadOfTransform = true;
        }
    }

    public override void Enter()
    {
        // Store original speed and apply flee speed
        originalMaxSpeed = movement.maxSpeed;
        movement.maxSpeed = maxFleeSpeed;
        
        // Reset timers
        panicTimer = panicTime;
        directionTimer = 0;
        
        // Initialize flee direction
        UpdateFleeDirection();
        
        // If we're dealing with a SchoolingFish, notify nearby fish about the danger
        SchoolingFish schoolingFish = movement as SchoolingFish;
        if (schoolingFish != null)
        {
            schoolingFish.AlertNearbyFish(usePositionInsteadOfTransform ? dangerPosition : threat.position);
        }
    }

    public override Vector2 UpdateMove()
    {
        // Update the danger position if we're tracking a transform
        if (threat != null && !usePositionInsteadOfTransform)
        {
            dangerPosition = threat.position;
        }
        
        // Update flee direction periodically
        directionTimer += Time.deltaTime;
        if (directionTimer > directionUpdateInterval)
        {
            directionTimer = 0;
            UpdateFleeDirection();
        }
        
        // Decrease panic timer
        if (ShouldDecreasePanic())
        {
            panicTimer -= Time.deltaTime;
            
            // Gradually slow down as panic decreases
            float speedFactor = Mathf.Lerp(recoverySpeed, 1f, panicTimer / panicTime);
            movement.maxSpeed = Mathf.Lerp(originalMaxSpeed, maxFleeSpeed, speedFactor);
        }
        
        return fleeDirection;
    }

    private void UpdateFleeDirection()
    {
        // Get direction away from threat
        Vector2 awayFromThreat = ((Vector2)movement.transform.position - dangerPosition).normalized;
        
        // Add slight randomness to the flee direction
        float angleVariation = Random.Range(-30f, 30f);
        fleeDirection = Quaternion.Euler(0, 0, angleVariation) * awayFromThreat;
    }

    private bool ShouldDecreasePanic()
    {
        // Only decrease panic when the threat is gone or out of range
        if (usePositionInsteadOfTransform)
        {
            return true; // Always decrease when using static position
        }
        else if (threat == null)
        {
            usePositionInsteadOfTransform = true; // Switch to using last known position
            return true;
        }
        else
        {
            float distanceToThreat = Vector2.Distance(movement.transform.position, threat.position);
            return distanceToThreat > fleeRadius;
        }
    }

    public override void Exit()
    {
        // Restore original speed
        movement.maxSpeed = originalMaxSpeed;
    }

    public override bool ShouldTransition()
    {
        // Transition if the panic time is over and we're out of danger
        if (panicTimer <= 0)
            return true;
            
        // Otherwise, check if we're still in danger
        if (threat != null && !usePositionInsteadOfTransform)
        {
            float distanceToThreat = Vector2.Distance(movement.transform.position, threat.position);
            // If we're still within the flee radius, don't transition
            if (distanceToThreat <= fleeRadius)
                return false;
        }
        
        // Continue fleeing based on panic timer
        return false;
    }
    
    // Method to set a danger position when no transform is available
    public void SetDangerPosition(Vector2 position)
    {
        dangerPosition = position;
        usePositionInsteadOfTransform = true;
        
        // Reset the panic timer when a new danger is detected
        panicTimer = panicTime;
        
        // Update flee direction immediately
        UpdateFleeDirection();
    }
}