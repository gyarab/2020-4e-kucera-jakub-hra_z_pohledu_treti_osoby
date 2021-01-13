using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackTargetState : EnemyState
{
    private float _delayBeforeAttack, _delayAfterAttack, _timer;
    private bool _attacked;

    // Konstruktor
    public AttackTargetState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        _enemyController.GetATInitValues(out _delayBeforeAttack, out _delayAfterAttack);
    }

    // Spustí útočnou animaci
    public override void OnEntered()
    {
        _enemyController.GetAnimator().SetTrigger("Attack");
        _attacked = false;
        _timer = 0;
    }

    // Zavolá zděděnou metodu
    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    // Udělí poškození po uplynutí prodlevy
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
