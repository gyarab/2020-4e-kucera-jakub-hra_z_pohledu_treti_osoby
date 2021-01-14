using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTargetState : EnemyState
{
    private float _distance, _attackRange;

    // Konstruktor
    public FollowTargetState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        _attackRange = _enemyController.GetFTInitValues();
    }

    // Nastaví animace chůze
    public override void OnEntered()
    {
        _enemyController.GetAnimator().SetBool("Walk", true);
        Debug.Log("FT");
    }

    // Zavolá zděděnou metodu
    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    // Následuje hráče, dokud ho neztratí z dohledu nebo nemůže zaútočit
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (_enemyController.IsEnemyVisibleAndInDetectionRange())
        {
            _distance = _enemyController.MoveToTargetAndRotate();

            if (_distance <= _attackRange)
            {
                _FSM.ChangeState(EnemyStateType.AttackTarget);
                return;
            }
        } else
        {
            _FSM.ChangeState(EnemyStateType.FollowPathToTarget);
            return;
        }
    }

    // Ukončí animace chůze
    public override void OnExit()
    {
        _enemyController.GetAnimator().SetBool("Walk", false);
    }
}
