using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTargetState : EnemyState
{
    private float _delayBeforeAttack, _delayAfterAttack, _timer;
    private bool _attacked;

    public AttackTargetState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        _enemyController.GetATInitValues(out _delayBeforeAttack, out _delayAfterAttack);
    }

    public override void OnEntered()
    {
        _enemyController.GetAnimator().SetTrigger("Attack");
        _attacked = false;
        _timer = 0;
        Debug.Log("AT");
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        _timer += Time.fixedDeltaTime;

        if (!_attacked)
        {
            if (_timer > _delayBeforeAttack)
            {
                _enemyController.Attack();
                _attacked = true;
            }
        } else if (_timer > (_delayBeforeAttack + _delayAfterAttack))
        {
            _FSM.ChangeState(EnemyStateType.FollowTarget);
        }
    }
}
