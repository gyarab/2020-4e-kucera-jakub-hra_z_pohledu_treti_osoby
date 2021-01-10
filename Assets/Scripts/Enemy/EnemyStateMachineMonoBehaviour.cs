using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachineMonoBehaviour : MonoBehaviour
{
    private Coroutine _coroutine;
    private bool _beingDestroyed;

    // Změní Coroutine, která běží
    protected void ChangeState(IEnumerator nextState)
    {
        if (_beingDestroyed)
        {
            return;
        }

        if (_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }

        _coroutine = StartCoroutine(nextState);
    }

    // Při zničení objektu zastaví běžící Coroutine
    protected void OnDestroy()
    {
        if(_coroutine != null)
        {
            StopCoroutine(_coroutine);
        }
        _beingDestroyed = true;
    }
}