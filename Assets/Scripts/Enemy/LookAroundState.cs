using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAroundState : AliveState
{
    public LookAroundState(EnemyStateController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();

        // TODO rotate or move?
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (Controller.CheckForTarget())
        {
            Controller.ChangeState(new FollowState(Controller));
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
