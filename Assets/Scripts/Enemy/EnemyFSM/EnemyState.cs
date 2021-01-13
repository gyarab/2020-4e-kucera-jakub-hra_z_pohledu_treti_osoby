using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyState : State
{
    protected EnemyController _enemyController;
    protected EnemyFSM _FSM;

    // Konstruktor
    public EnemyState(EnemyController enemyController, EnemyFSM FSM)
    {
        _enemyController = enemyController;
        _FSM = FSM;
    }

    // Nasměruje ukazatel životů směrem na kameru
    public override void FrameUpdate()
    {
        _enemyController.LookAtCameraIfCanvasEnabled();
    }

    // Řeší kolize a grvitační sílu
    public override void PhysicsUpdate()
    {
        _enemyController.PassivePhysics();
    }
}
