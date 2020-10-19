using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO might not be monobehavour; prob better to use some kind of hub manager
public class Teleport : MonoBehaviour
{
    [Range(0,10)]
    public float circularRange;
    public float x, y;

    private Transform target; // TODO asign
    private float startingHalf;

    // Start is called before the first frame update
    void Start()
    {
        // TODO use the same method as with waypoints?
        startingHalf = 0;
    }

    void FixedUpdate()
    {
        if(Vector3.Distance(target.transform.position, transform.position) < circularRange)
        {
            // TODO check if target is no longer in starting half
            
        }  
    }
}
