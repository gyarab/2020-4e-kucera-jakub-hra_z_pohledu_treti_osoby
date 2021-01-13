using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class State
{
    // Metoda je zavolána po změnění stavu na danou třídu
    public virtual void OnEntered()
    {

    }

    // Je volána při každém snímku
    public virtual void FrameUpdate()
    {

    }

    // Volá se v pravidelných intervalech danách enginem
    public virtual void PhysicsUpdate()
    {

    }

    // Metoda je zavolána po změnění stavu na jiný
    public virtual void OnExit()
    {

    }
}
