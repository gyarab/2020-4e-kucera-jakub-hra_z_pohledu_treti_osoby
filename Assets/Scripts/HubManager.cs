using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HubManager : MonoBehaviour
{
    [SerializeField]
    private Portal[] _portals;
    // TODO shopkeeper

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.CurrentHubManager = this;
    }

    public void EnablePlayerDependantObjects(Transform target)
    {
        for (int i = 0; i < _portals.Length; i++)
        {
            _portals[i].enabled = true;
        }
    }

    public void ChangeLevel()
    {
        // TODO
    }
}
