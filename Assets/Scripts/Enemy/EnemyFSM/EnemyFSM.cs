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

    // Inicializace a nastaví počáteční stav
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

    // Změní stav a zavolá metodu ukončující starý stav a začínající nový stav
    public void ChangeState(EnemyStateType nextState)
    {
        _currentState.OnExit();
        SetState(nextState);
        _currentState.OnEntered();
    }

    // Změní stav podle parametru
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

    // Zavolá metodu, která vykoná instrukce pro aktuální stav
    void Update()
    {
        _currentState.FrameUpdate();
    }

    // Zavolá metodu, která vykoná instrukce pro aktuální stav
    void FixedUpdate()
    {
        _currentState.PhysicsUpdate();
    }
}
