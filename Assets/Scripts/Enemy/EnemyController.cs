using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class EnemyController : EnemyStateMachineMonoBehaviour, IDamageable // TODO combine visual navigation w pathfinding / rework nav; attack as class with interface / get attack component
{
    public static Action<Vector3> OnEnemyDeath;
    public static Pathfinding<PathfindingNode> Pathfinder { get; set; } // TODO do differently

    #region Variables

    [Header("Physics")]
    [SerializeField]
    private float _gravity;

    [Header("Enemy")]
    [SerializeField]
    private CharacterStats _stats;
    [SerializeField]
    private float _movementSpeed, _rotationSpeed;
    [SerializeField]
    private GameObject _healthBarGO;

    [Header("Target detection")]
    [SerializeField]
    private float _detectionRange;
    [SerializeField]
    private float _detectionAngle;
    [SerializeField]
    private Vector3 _firstRaycastOffset, _secondRaycastOffset;

    [Header("RayCast")]
    [SerializeField]
    private LayerMask _excludeCharacters;
    [SerializeField]
    private float _enemyHeight, _sphereRadius, _groundOffset, _rayOverhead;

    [Header("Combat")]
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private float _damage, _delayBeforeAttack, _delayAfterAttack, _attackAngleInDegrees;

    [Header("Pathfinding")]
    [SerializeField]
    private float _moveDistanceTolerance;

    [Header("Loot")]
    [SerializeField, Range(0f, 1f)]
    private float _itemDropChance;
    [SerializeField]
    private int _itemDropID;
    [SerializeField, Range(0f, 1f)]
    private float _coinDropChance;

    // Objects
    private Image _healthBar;
    private Canvas _healthBarCanvas;
    private Camera _mainCamera;
    private Transform _target;
    private List<Vector3> _path;

    private float _currentHealth;
    private Vector3 _velocity, _destination;
    private bool _grounded;
    private float _timeSinceGrounded, _currentGravity;

    #endregion

    void Start()
    {
        _mainCamera = Camera.main;
        _healthBarCanvas = _healthBarGO.GetComponent<Canvas>();
        _healthBar = _healthBarCanvas.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();

        _currentHealth = _stats.Health;
        _healthBarCanvas.enabled = false;

        _grounded = true;

        _target = GameManager.Instance.Player.transform;
        ChangeState(LookForTarget());
    }

    void Update()
    {
        if (_healthBarCanvas.enabled)
        {
            LookAtCamera();
        }
    }

    void FixedUpdate()
    {
        _grounded = IsGrounded();
        CalculateYSpeed();
        transform.position += _velocity;
    }

    public void ReceivePath(List<Vector3> path)
    {
        if(path == null)
        {
            ChangeState(FollowTarget());
        }

        _path = path;
        ChangeState(FollowPath());
    }

    #region State Methods

    private IEnumerator WaitForPath()
    {
        // TODO change
        Pathfinder.GetPath(transform.position, _target.position, ReceivePath);
        yield return null;
    }

    private IEnumerator FollowPath()
    {
        //Debug.Log("Following path");
        int index = 0;

        if(_path == null)
        {
            ChangeState(FollowTarget());
        }

        SetDestination(_path[index]);
        index++;

        while (true)
        {
            // TODO change?
            
            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_destination.x, _destination.z)) <= _moveDistanceTolerance)
            {
                if(index >= _path.Count)
                {
                    break;
                }

                SetDestination(_path[index]);
                index++;
            }

            transform.rotation = GamePhysics.RotateTowardsMovementDirection(transform.rotation, _velocity, _rotationSpeed);
            yield return new WaitForFixedUpdate();
        }

        _velocity = Vector3.zero;

        ChangeState(FollowTarget());
    }

    private IEnumerator LookForTarget()
    {
        //Debug.Log("Looking for target");
        while (!CheckForTarget())
        {
            yield return new WaitForFixedUpdate();
        }

        ChangeState(WaitForPath());
    }

    private IEnumerator AttackTarget()
    {
        //Debug.Log("Attacking target");
        yield return new WaitForSeconds(_delayBeforeAttack);
        Attack();
        yield return new WaitForSeconds(_delayAfterAttack);

        //Debug.Log("Attack");
        ChangeState(WaitForPath());
    }

    public IEnumerator FollowTarget()
    {
        //Debug.Log("Following target");
        while (true) // while target is visible TODO
        {
            SetDestination(_target.position);

            if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_destination.x, _destination.z)) <= _attackRange / 2)
            {
                break;
            }

            transform.rotation = GamePhysics.RotateTowardsTarget(transform.rotation, transform.position, _target.position, _rotationSpeed);
            yield return new WaitForFixedUpdate();
        }

        _velocity = Vector3.zero;

        ChangeState(AttackTarget());
    }

    #endregion

    #region Attack Methods

    private void Attack()
    {
        if (Vector3.Distance(transform.position, _target.transform.position) <= _attackRange)
        {
            Vector3 attackDirection = _target.transform.position - transform.position;

            if (_attackAngleInDegrees > Vector3.Angle(transform.forward, attackDirection))
            {
                // TODO rework damage method
                _target.GetComponent<IDamageable>().TakeDamage(_damage, 0f); // TODO add character stats
                Debug.Log("hit");
            }
        }
    }

    public bool CanAttack()
    {
        if (Vector3.Distance(transform.position, _target.transform.position) < _attackRange) // Is enemy in range to attack?
        {
            if (Physics.Raycast(transform.position, _target.transform.position - transform.position, _attackRange, ~transform.gameObject.layer))
            {
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Physics Methods

    public void CalculateYSpeed()
    {
        if (_grounded)
        {
            _timeSinceGrounded = 0;
        }
        else
        {
            _timeSinceGrounded += Time.fixedDeltaTime;
        }

        _velocity.y = GamePhysics.GetGravitationalForce(_timeSinceGrounded);

        //currentGravity = jumpForce * timeSinceGrounded - 0.5f * gravity * Mathf.Pow(timeSinceGrounded, 2);
    }

    public bool IsGrounded()
    {
        bool grounded = GamePhysics.IsGroundedRayCast(transform.position, _groundOffset, _rayOverhead, _excludeCharacters, out float yCorrection);
        transform.position = new Vector3(transform.position.x, transform.position.y + yCorrection, transform.position.z);
        return grounded;
    }

    public void SetDestination(Vector3 location)
    {
        _destination = location;
        Vector2 temp = new Vector2(_destination.x - transform.position.x, _destination.z - transform.position.z).normalized * _movementSpeed;
        _velocity = new Vector3(temp.x, 0, temp.y);
    }

    #endregion

    #region Update Methods

    private void LookAtCamera()
    {
        _healthBarGO.transform.LookAt(_mainCamera.transform);
        _healthBarGO.transform.Rotate(0, 180, 0);
    }

    public bool CheckForTarget()
    {
        if (Vector3.Distance(transform.position, _target.transform.position) < _detectionRange)
        {
            if (Vector3.Angle(transform.forward, _target.transform.position - transform.position) < _detectionAngle)
            {
                return true;
            }
        }

        return false;
    }

    #endregion
 
    private void GetDestroyed()
    {
        // TODO drop drop coin / items
        OnEnemyDeath?.Invoke(transform.position);

        Destroy(gameObject);
    }

    public void TakeDamage(float damage, float armourPenetration)
    {
        _currentHealth -= damage; // TODO add character stats
        _healthBar.fillAmount = _currentHealth / _stats.Health;

        if (_currentHealth <= 0)
        {
            _healthBarCanvas.enabled = false;
            GetDestroyed();
            Debug.Log(name + " destroyed");
        }
        else
        {
            _healthBarCanvas.enabled = true;
        }
    }
}
