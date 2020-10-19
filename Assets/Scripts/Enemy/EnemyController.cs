using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour, IDamageable
{
    #region Variables

    [Header("Physics")]
    [SerializeField]
    private float _gravity;

    [Header("Enemy")]
    [SerializeField]
    private CharacterStats _stats;
    [SerializeField]
    private float _speed;
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

    // Objects
    private Image _healthBar;
    private Canvas _healthBarCanvas;
    private Camera _mainCamera;
    private Transform _target;
    private List<Vector3> _path;

    private float _currentHealth;
    private Coroutine _coroutine;
    private Vector3 _groundRayPosition, _velocity, _destination;
    private bool _grounded;
    private float _timeSinceGrounded, _currentGravity;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _healthBarCanvas = _healthBarGO.GetComponent<Canvas>();
        _healthBar = _healthBarCanvas.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();

        _currentHealth = _stats.health;
        _healthBarCanvas.enabled = false;

        _groundRayPosition = new Vector3(0, -_enemyHeight + _groundOffset, 0);

        // TODO change
        _target = GameObject.FindGameObjectWithTag("Player").transform;
        ChangeState(WaitForPath());
    }

    void Update()
    {
        if (_healthBarCanvas.enabled)
        {
            LookAtCamera();
        }
    }

    private void ChangeState(IEnumerator nextState)
    {
        if (_coroutine != null)
            StopCoroutine(_coroutine);

        _coroutine = StartCoroutine(nextState);
    }

    public void ReceivePath(List<Vector3> path)
    {
        _path = path;
        ChangeState(FollowTarget());
    }

    #region State Methods

    private IEnumerator WaitForPath()
    {
        // TODO remove
        yield return new WaitForSeconds(3f);

        // TODO change
        GameObject.FindGameObjectWithTag("Pathfinding").GetComponent<Pathfinding>().GetPath(transform.position, _target.position, ReceivePath);
        yield return null;
    }

    private IEnumerator FollowTarget()
    {
        int index = 0;
        SetDestination(_path[index]);
        index++;

        while (true)
        {
            // TODO change?
            if(Vector2.Distance(new Vector2(transform.position.x - _destination.x, transform.position.z - _destination.z), Vector2.zero) <= _moveDistanceTolerance)
            {
                if(index >= _path.Count)
                {
                    break;
                }

                SetDestination(_path[index]);
                index++;
            }

            CalculatePosition();
            transform.position += _velocity;
            _grounded = true; // IsGrounded();

            yield return new WaitForFixedUpdate();
        }

        ChangeState(AttackTarget());
    }

    private IEnumerator LookForTarget()
    {
        while (!CheckForTarget())
        {
            yield return new WaitForFixedUpdate();
        }

        ChangeState(FollowTarget());
    }

    private IEnumerator AttackTarget()
    {
        yield return new WaitForSeconds(_delayBeforeAttack);
        Attack();
        yield return new WaitForSeconds(_delayAfterAttack);

        //ChangeState(FollowTarget());
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
                _target.GetComponent<IDamageable>().TakeDamage(_damage);
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

    public void CalculatePosition()
    {
        if (_grounded)
        {
            //timeSinceGrounded = Time.fixedDeltaTime; // If gravity doesnt work
            _timeSinceGrounded = 0;
        }
        else
        {
            _timeSinceGrounded += Time.fixedDeltaTime;
        }

        //currentGravity = jumpForce * timeSinceGrounded - 0.5f * gravity * Mathf.Pow(timeSinceGrounded, 2);
        _currentGravity = (-_gravity) * Mathf.Pow(_timeSinceGrounded, 2);

        //velocity = new Vector3(direction.x, 0, direction.z);
        _velocity.y = _currentGravity;

        //velocity = transform.TransformDirection(velocity);
    }

    public bool IsGrounded()
    {
        Ray ray = new Ray(transform.TransformPoint(_groundRayPosition), Vector3.down);

        RaycastHit tempHit = new RaycastHit();
        if (Physics.SphereCast(ray, _sphereRadius, out tempHit, _groundOffset + _rayOverhead, _excludeCharacters))
        {
            Debug.Log("hit");
            transform.position = new Vector3(transform.position.x, transform.position.y + _groundOffset - _sphereRadius - tempHit.distance, transform.position.z);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SetDestination(Vector3 location)
    {
        //goingToDestination = true;
        // face destination TODO - coroutine?
        _destination = location;
        Vector2 temp = new Vector2(location.x - transform.position.x, location.z - transform.position.z).normalized * _speed;
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

    // TODO die and rop items
    public void TakeDamage(float damageTaken)
    {
        _currentHealth -= damageTaken;
        _healthBar.fillAmount = _currentHealth / _stats.health;

        if (_currentHealth <= 0)
        {
            _healthBarCanvas.enabled = false;
            Debug.Log(name + " destroyed");
        }
        else
        {
            _healthBarCanvas.enabled = true;
        }
    }
}
