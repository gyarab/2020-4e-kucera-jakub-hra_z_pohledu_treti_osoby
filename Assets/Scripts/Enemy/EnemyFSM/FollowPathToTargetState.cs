using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPathToTargetState : EnemyState
{
    private List<Vector3> _path;
    private int _index;
    private float _distance, _timePassed, _pathfindingRefreshInterval, _moveDistanceTolerance;

    // Konstruktor
    public FollowPathToTargetState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        enemyController.GetFPTTInitValues(out _pathfindingRefreshInterval, out _moveDistanceTolerance);
    }

    // Získá cestu k hráči a začne přehrávat animaci chůze
    public override void OnEntered()
    {
        Debug.Log("1: FPTT");
        _enemyController.GetAnimator().SetBool("Walk", true);
        _path = _enemyController.GetPathToTarget();

        if (_path == null)
        {
            _FSM.ChangeState(EnemyStateType.FollowTarget);
            return;
        }
        else if (_path.Count < 1)
        {
            _FSM.ChangeState(EnemyStateType.FollowTarget);
            return;
        }

        _index = 0;
        _timePassed = 0;

        if (_path.Count > 1)
        {
            if (_enemyController.IsPathToNextWaypointClear(_path[1]))
            {
                _index = 1;
            }
        }
        Debug.Log("2: FPTT");
    }

    // Zavolá zdědenou metodu
    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    // Zařizuje to, aby nepřítel prošel vyhledanou cestou k hráči
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (_enemyController.CanAttack())
        {
            _FSM.ChangeState(EnemyStateType.AttackTarget);
            return;
        }

        if (_timePassed > _pathfindingRefreshInterval)
        {
            _FSM.ChangeState(EnemyStateType.FollowPathToTarget);
            return;
        }

        _timePassed += Time.fixedDeltaTime;

        _distance = _enemyController.MoveToPositionAndRotate(_path[_index]);

        if (_distance <= _moveDistanceTolerance)
        {
            _index++;

            if (_index >= _path.Count || _path[_index] == null) // how can it be null
            {
                if (_enemyController.IsEnemyVisibleAndInAttackRange())
                {
                    _FSM.ChangeState(EnemyStateType.FollowTarget);
                    return;
                } else
                {
                    _FSM.ChangeState(EnemyStateType.FollowPathToTarget);
                    return;
                }
            }
        }
    }

    // Přestane přehrávat animaci chůze
    public override void OnExit()
    {
        _enemyController.GetAnimator().SetBool("Walk", false);
    }
}
