using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : State
{
    protected EnemyController _enemyController;
    protected EnemyFSM _FSM;

    public EnemyState(EnemyController enemyController, EnemyFSM FSM)
    {
        _enemyController = enemyController;
        _FSM = FSM;
    }

    public override void FrameUpdate()
    {
        _enemyController.LookAtCameraIfCanvasEnabled();
    }

    public override void PhysicsUpdate()
    {
        _enemyController.PassivePhysics();
    }
}
