using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachineMonoBehaviour : MonoBehaviour
{
    private Coroutine _coroutine;

    protected void ChangeState(IEnumerator nextState)
    {
        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _coroutine = StartCoroutine(nextState);
    }

    protected void OnDestroy()
    {
        if(_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
    }
}