using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFSM : MonoBehaviour
{
    private EnemyState _currentState;
    private FollowPathToTargetState _followPathToTargetState;
    private WaitForNextActionState _waitForNextActionState;
    private AttackTargetState _attackTargetState;
    private FollowTargetState _followTargetState;
    private WalkToRandomPlaceState _walkToRandomPlaceState;
    private FaceRandomDirectionState _faceRandomDirectionState;

    // Start is called before the first frame update
    void Start()
    {
        EnemyController controller = GetComponent<EnemyController>();

        _followPathToTargetState = new FollowPathToTargetState(controller, this);
        _waitForNextActionState = new WaitForNextActionState(controller, this);
        _attackTargetState = new AttackTargetState(controller, this);
        _followTargetState = new FollowTargetState(controller, this);
        _walkToRandomPlaceState = new WalkToRandomPlaceState(controller, this);
        _faceRandomDirectionState = new FaceRandomDirectionState(controller, this);

        SetState(EnemyStateType.WaitForNextAction);
        _currentState.OnEntered();
    }

    public void ChangeState(EnemyStateType nextState)
    {
        _currentState.OnExit();
        SetState(nextState);
        _currentState.OnEntered();
    }

    private void SetState(EnemyStateType nextState)
    {
        switch (nextState)
        {
            case EnemyStateType.FollowPathToTarget:
                _currentState = _followPathToTargetState;
                break;
            case EnemyStateType.WaitForNextAction:
                _currentState = _waitForNextActionState;
                break;
            case EnemyStateType.AttackTarget:
                _currentState = _attackTargetState;
                break;
            case EnemyStateType.FollowTarget:
                _currentState = _followTargetState;
                break;
            case EnemyStateType.WalkToRandomPlace:
                _currentState = _walkToRandomPlaceState;
                break;
            case EnemyStateType.FaceRandomDirection:
                _currentState = _faceRandomDirectionState;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        _currentState.FrameUpdate();
    }

    void FixedUpdate()
    {
        _currentState.PhysicsUpdate();
    }
}
