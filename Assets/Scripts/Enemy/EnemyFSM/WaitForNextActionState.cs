using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForNextActionState : EnemyState
{
    private Vector2 _waitTime;
    private float _timer, _timeToWait;

    // Konstruktor
    public WaitForNextActionState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        _waitTime = _enemyController.GetWFNAInitValues();
    }

    // Nastaví náhodně časový interval
    public override void OnEntered()
    {
        _timer = 0;
        _timeToWait = Random.Range(_waitTime.x, _waitTime.y);
    }

    // Zavolá zdědenou metodu
    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    // Čeká dokud neuplyne určený čas a také vyhlíží hráče
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (_enemyController.TryToDetectTarget())
        {
            _FSM.ChangeState(EnemyStateType.FollowPathToTarget);
            return;
        }

        _timer += Time.fixedDeltaTime;

        if (_timer > _timeToWait)
        {
            int randomNumber = Random.Range(0, 2);
            if (randomNumber == 0)
            {
                _FSM.ChangeState(EnemyStateType.WaitForNextAction);
                return;
            }
            else
            {
                _FSM.ChangeState(EnemyStateType.FaceRandomDirection);
                return;
            }
        }
    }
}
