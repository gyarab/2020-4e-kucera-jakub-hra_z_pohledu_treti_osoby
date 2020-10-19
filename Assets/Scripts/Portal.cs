using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField]
    private float _range = 2f;
    [SerializeField]
    private MazeSettingsSO _mazeSettings;

    private Transform _target;
    private Vector3 _secondPoint;
    private int _side;

    // Start is called before the first frame update
    void Start()
    {
        _secondPoint = transform.position + transform.right;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(_side != GetSide(_target.position))
        {
            _side = GetSide(_target.position);
            if (Vector3.Distance(_target.position, transform.position) < _range)
            {
                Debug.Log("Went through");
                // TODO load specific level; change
                GameManager.Instance.LoadMaze(_mazeSettings);
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
