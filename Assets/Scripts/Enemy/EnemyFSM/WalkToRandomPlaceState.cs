using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkToRandomPlaceState : EnemyState
{
    private List<Vector3> _path;
    private int _index;
    private float _distance, _moveDistanceTolerance;

    // Konstruktor
    public WalkToRandomPlaceState(EnemyController enemyController, EnemyFSM FSM) : base(enemyController, FSM)
    {
        _moveDistanceTolerance = _enemyController.GetWTRPInitValues();
    }

    // Víská vygenerovanou náhodnou cestu z Pathfinding, začne přehrávat animaci chůze
    public override void OnEntered()
    {
        _enemyController.GetAnimator().SetBool("Walk", true);
        _path = _enemyController.GetRandomPath();
        _index = 0;
        Debug.Log("WTRP");
    }

    // Zavolá zděděnou metodu
    public override void FrameUpdate()
    {
        base.FrameUpdate();
    }

    // Prochází body a rozhlíží se po hráči
    public override void PhysicsUpdate()
    {
        base.PhysicsUpdate();

        if (_enemyController.TryToDetectTarget())
        {
            _FSM.ChangeState(EnemyStateType.FollowPathToTarget);
            return;
        }

        _distance = _enemyController.MoveToPositionAndRotate(_path[_index]);

        if (_distance <= _moveDistanceTolerance)
        {
            _index++;

            if (_index >= _path.Count)
            {
                _FSM.ChangeState(EnemyStateType.WaitForNextAction);
            }
        }
    }

    // Skončí přehrávat animaci běhu
    public override void OnExit()
    {
        _enemyController.GetAnimator().SetBool("Walk", false);
    }
}
