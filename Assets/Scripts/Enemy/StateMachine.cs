﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySM : MonoBehaviour
{
    protected State State;

    public void InitState(State state)
    {
        State = state;
        State.Enter();
    }

    public void ChangeState(State state)
    {
        State.Exit();

        State = state;
        State.Enter();
    }
}