using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AliveState : State
{
    public AliveState(EnemyStateController controller) : base(controller)
    {
    }

    // TODO remove empty?
    public override void Enter()
    {
            
    }

    public override void LogicUpdate()
    {
        Controller.UpdateCanvas();
    }

    public override void PhysicsUpdate()
    {
            
    }

    public override void Exit()
    {
            
    }
}
