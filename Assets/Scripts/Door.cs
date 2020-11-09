using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private float _range = 2f;

    private IDoor doorImplementation;
    private Transform _target;
    private Vector3 _secondPoint;
    private int _side;

    private void Awake()
    {
        _secondPoint = transform.position + transform.right;
    }

    void FixedUpdate()
    {
        if (_side != GetSide(_target.position))
        {
            _side = GetSide(_target.position);
            if (Vector3.Distance(_target.position, transform.position) < _range)
            {
                // TODO load specific level; boss or puzzle etc...
                doorImplementation.Entered();
            }
        }
    }

    public void OnEnable()
    {
        _target = GameManager.Instance.Player.transform;
        _side = GetSide(_target.position);
    }

    public int GetSide(Vector3 position)
    {
        // d = (x−x1)(y2−y1)−(y−y1)(x2−x1); 2x2 matrix determinant
        float temp = (position.x - transform.position.x) * (_secondPoint.z - transform.position.z) - (position.z - transform.position.z) * (_secondPoint.x - transform.position.x);

        if (temp < 0)
        {
            return -1;
        }
        else if (temp == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}
