using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

public class Boss : EnemyStateMachineMonoBehaviour, IDamageable
{
    //public static Action OnBossDeath; TODO do smth?
    private Vector3 drawCenter; // TODO remove
    private float drawRadius; // TODO remove

    #region Variables

    [Header("Physics"), SerializeField]
    private float _gravity; // TODO physics script
    [SerializeField]
    private float _rotationSpeed, _movementSpeed;
    [SerializeField, Range(0f, 180f)]
    private float _maxAngleDifference;
    [SerializeField]
    private float _maxTargetDistance;

    [Header("Rays"), SerializeField]
    private LayerMask _ground;
    [SerializeField]
    private float _groundOffset, _rayOverhead, _yRayPositionOffset;

    [Header("Stats"), SerializeField]
    private CharacterStatsSO _bossStats;
    [SerializeField]
    private GameObject _healthBarGO; // TODO health bar fixed on screen?

    [Header("Miscellaneous")] // TODO rename
    [SerializeField]
    private float _delayAfterEntering;
    [SerializeField]
    private bool _grounded;

    [Header("Attacks"), SerializeField]
    private ComboSO[] _combos;
    [SerializeField]
    private Transform _rightHand, _leftHand;
    [SerializeField]
    private float _groundSmashRadius, _jumpSmashRadius, _swipeRadius, _landDistanceOffset, _minLandDistanceProgress;

    [Header("Animations"), SerializeField]
    private Animator _animator;

    private Transform _target;
    private Coroutine _coroutine;
    private HealthBar _healthBar;

    private float _currentHealth, _timeSinceGrounded;
    private Vector3 _rayPosition;

    // Combos
    private int[] _comboBias;
    private ComboSO _currentCombo;
    private int _currentActionIndex, _currentWaitTimeIndex;

    #endregion

    #region Unity Methods

    // Start is called before the first frame update
    void Awake()
    {
        _currentHealth = _bossStats.health;
        _comboBias = new int[_combos.Length];

        _timeSinceGrounded = 0;
        _rayPosition = new Vector3(0, _yRayPositionOffset, 0);
    }

    void Start()
    {
        _target = GameManager.Instance.Player.transform;
    }

    void FixedUpdate()
    {
        IsGrounded();
        ApplyGravity();
    }

    private void OnEnable()
    {
        BossRoomDoor.OnDoorsOpened += PlayIntro;
    }

    private void OnDisable()
    {
        BossRoomDoor.OnDoorsOpened -= PlayIntro;
    }

    #endregion

    #region Physics

    private void IsGrounded()
    {
        _grounded = GamePhysics.IsGroundedRayCast(transform.TransformPoint(_rayPosition), _groundOffset, _rayOverhead, _ground, out float yCorrection);
        transform.position = new Vector3(transform.position.x, transform.position.y + yCorrection, transform.position.z);
    }

    private void ApplyGravity()
    {
        if (_grounded)
        {
            _timeSinceGrounded = 0;
        }
        else
        {
            _timeSinceGrounded += Time.fixedDeltaTime;
        }

        float gravitationalForce = GamePhysics.GetGravitationalForce(_timeSinceGrounded);
        transform.position = new Vector3(transform.position.x, transform.position.y + gravitationalForce, transform.position.z);
    }

    #endregion

    private void PlayIntro() // TODO rename?
    {
        _healthBar = GameManager.Instance.Player.GetComponent<PlayerController>().GetPlayerInventory().BossHealthBar;
        _healthBar.SetVisibility(true);
        ChangeState(Intro());
    }

    #region Combo

    private ComboSO ChooseNextCombo()
    {
        float targetBossAngle = Vector2.Angle(transform.forward, _target.position - transform.position);
        float targetBossDistance = Vector3.Distance(transform.position, _target.position);
        int currentHighestBias = -1;
        int indexWithHighestBias = 0;

        int index;
        int randomOffset = UnityEngine.Random.Range(0, _combos.Length);

        for (int j = 0; j < _combos.Length; j++)
        {
            index = (j + randomOffset) % _combos.Length;

            _comboBias[index] = 0;
            if (_combos[index] == _currentCombo)
            {
                _comboBias[index]--;
            }

            if(targetBossAngle <= _combos[index].optimalAngle)
            {
                _comboBias[index]++;
            }

            if(targetBossDistance >= _combos[index].optimalMinDistance && targetBossDistance <= _combos[index].optimalMaxDistance)
            {
                _comboBias[index]++;
            }

            if(_comboBias[index] > currentHighestBias)
            {
                indexWithHighestBias = index;
            }
        }

        return _combos[indexWithHighestBias];
    }

    private void StartNextAction()
    {
        _currentActionIndex++;

        if(_currentCombo == null || (_currentActionIndex >= _currentCombo.actions.Length))
        {
            _currentCombo = ChooseNextCombo();
            _currentActionIndex = _currentWaitTimeIndex = 0;
        }

        BossActionType nextAction = _currentCombo.actions[_currentActionIndex];
        Debug.Log(nextAction);

        switch (nextAction)
        {
            case BossActionType.Wait:
                ChangeState(Wait(_currentCombo.waitTime[_currentWaitTimeIndex]));
                _currentWaitTimeIndex++;
                break;
            case BossActionType.Swipe:
                ChangeState(StartCoroutine(Swipe(nextAction)));
                break;
            default:
                string enumeratorName = Enum.GetName(typeof(BossActionType), nextAction);
                ChangeState(StartCoroutine(enumeratorName));
                break;
        }
    }

    #endregion

    #region Actions

    private IEnumerator Intro()
    {
        Debug.Log("Entered boss fight");
        yield return new WaitForSeconds(_delayAfterEntering);

        StartNextAction();
    }

    private IEnumerator Wait(float timeToWait)
    {
        float timePlaying = 0;

        while (timePlaying <= timeToWait)
        {
            timePlaying += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        StartNextAction();
    }

    private IEnumerator MoveTowardsTarget()
    {
        _animator.SetBool("Walk", true);

        float distance;
        Vector3 positionDelta;

        while (true)
        {
            yield return new WaitForFixedUpdate();
            positionDelta = GamePhysics.MoveTowardsPositionNonYClamped(transform.position, _target.position, _movementSpeed, out distance);
            transform.rotation = GamePhysics.RotateTowardsTarget(transform.rotation, transform.position, _target.position, _rotationSpeed);

            if (distance <= _maxTargetDistance)
            {
                break;
            }

            transform.position += positionDelta;
            yield return new WaitForFixedUpdate();
        }

        _animator.SetBool("Walk", false);
        StartNextAction();
    }

    private IEnumerator RotateTowardsTarget()
    {
        Vector2 transformForwardDirection;
        Vector2 transformTargetDirection;
        do
        {
            yield return new WaitForFixedUpdate();
            transform.rotation = GamePhysics.RotateTowardsTarget(transform.rotation, transform.position, _target.position, _rotationSpeed);

            transformForwardDirection.x = transform.forward.x;
            transformForwardDirection.y = transform.forward.z;
            transformTargetDirection.x = _target.position.x - transform.position.x;
            transformTargetDirection.y = _target.position.z - transform.position.z;
        } while (Vector2.Angle(transformForwardDirection, transformTargetDirection) > _maxAngleDifference);

        StartNextAction();
    }

    // TODO Y movement; speed throughout the animation
    private IEnumerator JumpSmash()
    {
        const float TIME_TO_HIT = 1.3f;
        const float TIME_TO_END = 1.83f;

        Vector3 initialPosition = transform.position;
        Vector3 location = _target.position;
        float maxProgress = Mathf.Max(((_target.position - transform.position).magnitude - _landDistanceOffset) / (_target.position - transform.position).magnitude, _minLandDistanceProgress);

        transform.rotation = GamePhysics.RotateTowardsTarget(transform.rotation, transform.position, _target.position, 1);
        float timePlaying = 0, progress;
        _animator.SetTrigger("JumpSmash");

        while(timePlaying <= TIME_TO_HIT)
        {
            timePlaying += Time.fixedDeltaTime;
            progress = (timePlaying / TIME_TO_HIT) * maxProgress;

            transform.position = GamePhysics.MoveTowardsPositionProgressivelyNonYClamped(initialPosition, location, progress);
            yield return new WaitForFixedUpdate();
        }

        SphericalHitDetection(_jumpSmashRadius, GetBossCenterPosition());

        while (timePlaying <= TIME_TO_END)
        {
            timePlaying += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        StartNextAction();
    }

    private IEnumerator GroundSmash()
    {
        const float TIME_TO_HIT = 1f;
        const float TIME_TO_END = 1.58f;

        transform.rotation = GamePhysics.RotateTowardsTarget(transform.rotation, transform.position, _target.position, 1);
        float timePlaying = 0;
        _animator.SetTrigger("GroundSmash");

        while (timePlaying <= TIME_TO_HIT)
        {
            timePlaying += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        SphericalHitDetection(_groundSmashRadius, GetHandsCenterPosition());

        while (timePlaying <= TIME_TO_END)
        {
            timePlaying += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        StartNextAction();
    }

    private IEnumerator Swipe(BossActionType action)
    {
        const float TIME_TO_HIT = 0.67f;
        const float TIME_TO_END = 1.5f;
        const float TIME_WINDOW = 0.42f;

        int handIndex = UnityEngine.Random.Range(0, 2); // TODO decided in another way?
        Transform handTransform;

        if(handIndex == 0)
        {
            _animator.SetTrigger("SwipeRight");
            handTransform = _rightHand;
        } else
        {
            _animator.SetTrigger("SwipeLeft");
            handTransform = _leftHand;
        }

        transform.rotation = GamePhysics.RotateTowardsTarget(transform.rotation, transform.position, _target.position, 1);
        float timePlaying = 0;

        while (timePlaying <= TIME_TO_HIT)
        {
            timePlaying += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        while (timePlaying <= TIME_TO_HIT + TIME_WINDOW)
        {
            timePlaying += Time.fixedDeltaTime;
            SphericalHitDetection(_swipeRadius, handTransform.position);

            yield return new WaitForFixedUpdate();
        }

        while (timePlaying <= TIME_TO_END)
        {
            timePlaying += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        StartNextAction();
    }

    #endregion

    #region Attacks
    
    private void SphericalHitDetection(float radius, Vector3 position)
    {
        if (Vector3.Distance(position, _target.transform.position) <= radius)
        {
            _target.GetComponent<IDamageable>().TakeDamage(_bossStats.damage, _bossStats.armourPenetration);
        }

        // TODO remove code below
        drawCenter = position;
        drawRadius = radius;
    }

    private Vector3 GetHandsCenterPosition()
    {
        return (_rightHand.position + _leftHand.position) / 2;
    }

    private Vector3 GetBossCenterPosition()
    {
        Vector3 handsPosition = GetHandsCenterPosition();
        return (new Vector3(handsPosition.x + transform.position.x, handsPosition.y * 2, handsPosition.z + transform.position.z)) / 2;
    }

    #endregion

    private void GetDestroyed()
    {
        _healthBar.SetVisibility(false);
        Destroy(gameObject);
        Debug.Log(name + " destroyed");
    }

    public void TakeDamage(float damage, float armourPenetration)
    {
        _currentHealth -= damage; 
        _healthBar.SetValue(_currentHealth / _bossStats.health);

        if (_currentHealth <= 0)
        {
            GetDestroyed();
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(drawCenter, drawRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.TransformPoint(_rayPosition), new Vector3(transform.position.x, transform.position.y - _groundOffset, transform.position.z));
    }
}
