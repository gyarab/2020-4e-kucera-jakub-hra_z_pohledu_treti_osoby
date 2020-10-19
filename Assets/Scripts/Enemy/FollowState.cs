using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowState : AliveState
{
    public FollowState(EnemyStateController controller) : base(controller)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // TODO
        //GameManager.Instance.Pathfinding.GetPath();
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (Controller.VisionClearenceToTarget())
        {
            // TODO follow player?
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
