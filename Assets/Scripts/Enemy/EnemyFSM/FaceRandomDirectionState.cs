using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceRandomDirectionState : EnemyState
{
    private float _alreadyRotatedDegrees, _angularSpeed, _degreesToRotate;
    int _sign;

    public FaceRandomDirectionState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        _angularSpeed = _enemyController.GetFRDInitValues();
    }

    public override void OnEntered()
    {
        _alreadyRotatedDegrees = 0;
        _degreesToRotate = Random.Range(90f, 180f);
        
        if (Random.Range(0, 2) == 0)
        {
            _sign = 1;
        } else
        {
            _sign = -1;
        }
        Debug.Log("FRD");
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (_enemyController.TryToDetectTarget())
        {
            _FSM.ChangeState(EnemyStateType.FollowPathToTarget);
            return;
        }

        if (_alreadyRotatedDegrees + _angularSpeed >= _degreesToRotate)
        {
            _enemyController.RotateYDegrees((_degreesToRotate - _alreadyRotatedDegrees) * _sign);

            _FSM.ChangeState(EnemyStateType.WaitForNextAction);
            return;
        } else
        {
            _enemyController.RotateYDegrees(_angularSpeed * _sign);
            _alreadyRotatedDegrees += _angularSpeed;
        }
    }

    public override void OnExit()
    {

    }
}
