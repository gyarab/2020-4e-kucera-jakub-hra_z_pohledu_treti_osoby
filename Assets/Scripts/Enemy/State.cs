using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State
{
    public State(EnemyStateController controller)
    {
        Controller = controller;
    }

    protected EnemyStateController Controller { get; set; }

    public virtual void Enter()
    {    
    }

    public virtual void LogicUpdate()
    {
    }

    public virtual void PhysicsUpdate()
    {
    }

    public virtual void Exit()
    {
    }
}
