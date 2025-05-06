using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;  
// Estado base para todos los comportamientos de movimiento
public abstract class FishMovementState
{
    protected AquaticCreatureMovement movement;
    
    public FishMovementState(AquaticCreatureMovement movement)
    {
        this.movement = movement;
    }
    
    public abstract void Enter();       // Se llama al entrar al estado
    public abstract Vector2 UpdateMove(); // Calcula la nueva dirección/movimiento
    public abstract void Exit();        // Se llama al salir del estado
    
    public virtual bool ShouldTransition() // Determina si debería cambiar de estado
    {
        return false;
    }
}