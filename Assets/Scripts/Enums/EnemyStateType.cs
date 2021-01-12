using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyStateType
{
    FollowPathToTarget,
    WaitForNextAction,
    AttackTarget,
    FollowTarget,
    WalkToRandomPlace,
    FaceRandomDirection
}
