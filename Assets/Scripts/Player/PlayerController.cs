using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IDamageable
{
    #region Basic Variables

    [Header("General")]
    [SerializeField]
    private float _controllerGroundHeightOffset;
    [Range(0.0f, 89.0f)]
    [SerializeField]
    private float _walkAngle; // not working rn
    [SerializeField]
    private float _rollDuration;
    [SerializeField]
    private Vector2 _rollInvincibilityTiming;

    [Header("Speeds")]
    [SerializeField]
    private float _moveSpeed;
    [SerializeField]
    private float _playerRotationSpeed;
    [Range(0.0f, 1.0f), SerializeField]
    private float _inAirSpeed;
    [Range(0.0f, 2.0f), SerializeField]
    private float _rollSpeedMultiplier;

    [Header("Physics"), SerializeField]
    private float gravity;
    [SerializeField]
    private float _jumpForce, _maxStep;
    [SerializeField]
    private LayerMask _collisionLayer;

    [Header("Rays")]
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private float _groundRayOffset, _groundRayOverhead, _groundRayJumpDecrease, _sphereOffset, _sphereRadius, _groundOffset; // TODO remove _sphereRadius?

    [Header("Components"), SerializeField]
    private SphereCollider _sphereCollider;
    [SerializeField]
    private SphereCollider _sphereFeetCollider;

    [Header("Inventory and UI")]
    [SerializeField]
    private InventoryMonoBehaviour _inventory;
    [SerializeField]
    private HealthBar _healthBar;

    [Header("Stats")]
    [SerializeField]
    private CharacterStatsSO _baseStats;
    [SerializeField]
    private float _currentHealth;

    [Header("Animaton")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AnimatorOverrideController _fistsOverrideController, _onehandedOverrideController, _twohandedOverrideController, _bothhandedOverrideController; 

    private InventorySlotContainer _inventoryContainer;
    private CharacterStats _currentStats;

    private float _currentGravity;
    private RaycastHit _groundHit;

    // const
    private Vector3 _groundRayPosition, _spherePos, _groundPos;

    // Actions
    private PlayerActionType _nextAction;
    private bool _canDoAction, _acceptingInput;

    // FixedUpdate
    private Vector3 _velocity, _collisionCorectionVector;
    private Vector3 _joystickInput;
    private bool _grounded, _jumpNow, _jumping, _rollPressed, _rolling;
    private float _timeSinceGrounded, _rollTimer;
    private Vector3 _rollVector;
    private float _yCameraRotation;

    #endregion

    #region Attack Variables 

    [Header("Attack General")]
    [SerializeField]
    private Transform _rightHandTransform;
    [SerializeField]
    private Transform _leftHandTransform;
    [SerializeField]
    private LayerMask _enemies;
    [SerializeField]
    private Vector3 _offsetPosition, _offsetRotation;

    [Header("Attack Timings"), SerializeField]
    public float _attackDuration;
    [SerializeField]
    private float _attackTime;

    [Header("Attack Collisions"), SerializeField]
    private float _weaponRange;
    [SerializeField]
    private float _angleInDegrees;
    [SerializeField]
    private int _maxAttackCollisions;

    private bool _attackPressed, _findEnemy , _attacking, _attacked;
    private float _attackingForSeconds;
    private Vector3 _attackDirection;
    private Collider[] _attackOverlaps;
    private int _attackCollisions;

    #endregion

    #region Unity methods

    private void Awake()
    {
        _groundRayPosition = new Vector3(0, -_controllerGroundHeightOffset + _groundRayOffset, 0);
        _spherePos = new Vector3(0, -_sphereOffset, 0);
        _groundPos = new Vector3(0, -_groundOffset, 0);

        _canDoAction = true;
        _acceptingInput = true;

        AttackSettings();

        // TODO set stats and rigth animation controller
        SwitchAnimationController(AnimationType.Onehanded);
    }

    // Staaaaaaaaaaart
    private void Start()
    {
        GameManager.Instance.Player = gameObject;
        _currentHealth = _baseStats.health;
    }

    //Everything else
    void FixedUpdate()
    {
        // FixedUpdate or Update?
        if (_canDoAction && _grounded)
        {
            switch (_nextAction)
            {
                case PlayerActionType.None:
                    break;
                case PlayerActionType.Attack:
                    _acceptingInput = false;
                    _canDoAction = false;
                    _nextAction = PlayerActionType.None;

                    _attackingForSeconds = 0;
                    _attacked = false;
                    _attacking = true;
                    animator.SetBool("Attack", true);
                    break;
                case PlayerActionType.Roll:
                    _acceptingInput = false;
                    _canDoAction = false;
                    _nextAction = PlayerActionType.None;

                    _rolling = true;
                    animator.SetTrigger("Roll");

                    if (_joystickInput.x == 0 && _joystickInput.z == 0)
                    {
                        _rollVector = transform.forward * _moveSpeed * _rollSpeedMultiplier;
                    }
                    else
                    {
                        _rollVector = new Vector3(_joystickInput.x, _currentGravity, _joystickInput.z) * _moveSpeed * _rollSpeedMultiplier;
                        _rollVector = Quaternion.Euler(0, _yCameraRotation, 0) * _rollVector;
                    }

                    _rollTimer = 0;
                    break;
                case PlayerActionType.Jump:
                    _acceptingInput = false;
                    _canDoAction = false;
                    _nextAction = PlayerActionType.None;

                    _jumping = true;
                    _jumpNow = true;
                    animator.SetTrigger("Jump");
                    break;
                default:
                    break;
            }
        }

        if (_attacking)
        {
            Attack();
        }

        CalculatePosition();
        //Debug.Log("1: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
        transform.position += _velocity;
        //Debug.Log("2: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
        //Debug.Log("1: " + _velocity.x + ", " + _velocity.y + ", " + _velocity.z);

        Rotate();
        //Debug.Log("3: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
        CheckForCollisions();
        //Debug.Log("4: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);

        _grounded = IsGrounded();
        //Debug.Log("5: " + transform.position.x + ", " + transform.position.y + ", " + transform.position.z);
        animator.SetBool("Grounded", _grounded);
    }

    #endregion

    #region FixedUpdate methods

    private void Attack()
    {
        _attackingForSeconds += Time.fixedDeltaTime;
        if (!_attacked && _attackTime < _attackingForSeconds)
        {
            CalculateAttack();
            _attacked = true;
            animator.SetBool("Attack", false);
        } else if(_attackDuration < _attackingForSeconds)
        {
            if(_nextAction != PlayerActionType.Attack)
            {
                _attacking = false;
                _canDoAction = true;
                _acceptingInput = true;
            } else
            {
                _nextAction = PlayerActionType.None;
                _attackingForSeconds = 0;
                _attacked = false;
                _attacking = true;
            }
        }
    }

    private void Rotate()
    {
        transform.rotation = GamePhysics.RotateTowardsMovementDirection(transform.rotation, _velocity, _playerRotationSpeed);
    }

    private void CalculatePosition()
    {
        if(_grounded)
        {
            if (_jumping)
            {
                if(_jumpNow == false)
                {
                    _jumping = false;
                    _canDoAction = true;
                    _acceptingInput = true;
                }
            }
            _timeSinceGrounded = 0;
        } else
        {
            _timeSinceGrounded += Time.fixedDeltaTime;
        }

        if (_jumping)
        {
            _currentGravity = _jumpForce * _timeSinceGrounded - 0.5f * gravity * Mathf.Pow(_timeSinceGrounded, 2);
        } else
        {
            _currentGravity = (-gravity) * Mathf.Pow(_timeSinceGrounded, 2);
        }

        if (_attacking)
        {
            _velocity = new Vector3(0, _currentGravity, 0) * _moveSpeed;
        }
        else if (_rolling)
        {
            _velocity = _rollVector;
            if(_rollTimer < _rollDuration)
            {
                _rollTimer += Time.fixedDeltaTime;
            } else
            {
                _rolling = false;
                _canDoAction = true;
                _acceptingInput = true;
            }
        } else
        {
            _velocity = new Vector3(_joystickInput.x, _currentGravity, _joystickInput.z) * _moveSpeed;
            _velocity = Quaternion.Euler(0, _yCameraRotation, 0) * _velocity;

            if (!_jumping)
            {
                if (_joystickInput.x == 0 && _joystickInput.z == 0)
                {
                    animator.SetBool("Run", false);
                }
                else
                {
                    animator.SetBool("Run", true);
                }
            }
        }
    }

    private void CheckForCollisions()
    {
        transform.position += GamePhysics.ResolveCollisionsNonY(transform.position, transform.rotation, transform.TransformPoint(_sphereCollider.center), _sphereCollider, _collisionLayer);
    }

    private bool IsGrounded()
    {
        if(_velocity.y > 0 || _jumpNow)
        {
            _jumpNow = false;
            return false;
        }

        bool grounded = GamePhysics.IsGroundedWithMaxStepDistanceAndCollisions(transform.TransformPoint(_groundRayPosition), _groundRayOffset, _groundRayOverhead, _maxStep, _sphereFeetCollider, transform.TransformPoint(_sphereFeetCollider.center), _groundLayer, out Vector3 correction);
        transform.position += correction;
        return grounded;
    }

    #endregion

    #region Input

    public void SendInput(PlayerActionType action)
    {
        if (_acceptingInput)
        {
            _nextAction = action;
        }
        else if (_attacking && _attacked && action == PlayerActionType.Attack)
        {
            _nextAction = PlayerActionType.Attack;
            animator.SetBool("Attack", true);
        }
    }

    public void SetJoystickInput(float horizontalInput, float verticalInput, float cameraYRotation)
    {
        _joystickInput = new Vector3(horizontalInput, 0, verticalInput);
        _yCameraRotation = cameraYRotation;
    }

    #endregion

    #region Camera

    public Vector3 GetRotationVelocity()
    {
        return new Vector3(_velocity.x, 0, _velocity.z);
    }

    #endregion

    #region Attack

    private void AttackSettings()
    {
        _attackDirection = new Vector3(0, 0, 1.0f);
        _attackOverlaps = new Collider[_maxAttackCollisions];
    }

    private void CalculateAttack()
    {
        _attackCollisions = Physics.OverlapSphereNonAlloc(transform.position, _weaponRange, _attackOverlaps, _enemies);

        for (int i = 0; i < _attackCollisions; i++)
        {
            if (_attackOverlaps[i].GetType() == typeof(MeshCollider))
            {
                continue;
            }

            Vector3 attackDirection = _attackOverlaps[i].ClosestPoint(transform.position) - transform.position;

            if (_angleInDegrees > Vector3.Angle(transform.TransformVector(this._attackDirection), attackDirection))
            {
                _attackOverlaps[i].transform.root.GetComponent<IDamageable>().TakeDamage(_currentStats.Damage, _currentStats.ArmourPenetration);
                Debug.DrawRay(transform.position, attackDirection, Color.green);
            }
            else
            {
                Debug.DrawRay(transform.position, attackDirection, Color.grey);
            }
        }
    }

    #endregion

    #region Animations
    
    public void SwitchAnimationController(AnimationType type)
    {
        switch (type)
        {
            case AnimationType.Fists:
                SetAnimationsController(_fistsOverrideController);
                break;
            case AnimationType.Onehanded:
                SetAnimationsController(_onehandedOverrideController);
                break;
            case AnimationType.Twohanded:
                SetAnimationsController(_twohandedOverrideController);
                break;
            case AnimationType.Bothhanded:
                SetAnimationsController(_bothhandedOverrideController);
                break;
            default:
                throw new System.NotImplementedException("Unimplmented player animation");
        }
    }

    private void SetAnimationsController(AnimatorOverrideController overrideController)
    {
        animator.runtimeAnimatorController = overrideController;
    }

    #endregion

    #region Stats

    public void SetStats(CharacterStats equipmentStats)
    {
        // TODO add equipment stats
        _currentStats = new CharacterStats(_baseStats.health, _baseStats.armour, _baseStats.damage, _baseStats.armourPenetration);
        _currentStats.AddStats(equipmentStats);

        if(_currentHealth > _currentStats.Health)
        {
            _currentHealth = _currentStats.Health;
            _healthBar.SetValue(_currentHealth / _currentStats.Health);
        }
    }

    public CharacterStats GetStats()
    {
        return _currentStats;
    }

    public bool RestoreHealth(float healAmount)
    {
        if (_currentHealth < _currentStats.Health)
        {
            _currentHealth += healAmount;

            if(_currentHealth > _currentStats.Health)
            {
                _currentHealth = _currentStats.Health;
            }

            _healthBar.SetValue(_currentHealth / _currentStats.Health);
            return true;
        }
        else
        {
            return true;
        }
    }

    public void Reset()
    {
        _currentHealth = _currentStats.Health;
        _healthBar.SetValue(_currentHealth / _currentStats.Health);

        _inventory.RemoveTemporaryItems();
    }

    #endregion

    public InventoryMonoBehaviour GetPlayerInventory()
    {
        return _inventory;
    }

    public void SetWeapons(GameObject prefab, Vector3 offset, bool twoHanded)
    {
        if(_rightHandTransform.childCount > 0)
        {
            Destroy(_rightHandTransform.GetChild(0).gameObject);
        }
        if (_leftHandTransform.childCount > 0)
        {
            Destroy(_leftHandTransform.GetChild(0).gameObject);
        }

        GameObject weapon;
        weapon = Instantiate(prefab, _rightHandTransform);
        weapon.transform.localPosition = offset;

        if (twoHanded)
        {
            weapon = Instantiate(prefab, _leftHandTransform);
            weapon.transform.localPosition = offset;
        }
    }

    public void TakeDamage(float damageTaken, float armourPenetration)
    {
        Debug.Log("hit");

        if (_rolling)
        {
            if(_rollInvincibilityTiming.x <= _rollTimer && _rollTimer <= _rollInvincibilityTiming.y)
            {
                return;
            }
        }

        _currentHealth -= DamageCalculator.CalculateDamage(damageTaken, armourPenetration, _currentStats.Armour);
        _healthBar.SetValue(_currentHealth / _currentStats.Health);

        if(_currentHealth <= 0)
        {
            GameManager.Instance.LoadHub(false, 0);
        }
    }
}