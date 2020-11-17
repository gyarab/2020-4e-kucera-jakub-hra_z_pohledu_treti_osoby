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

    [Header("Rays")]
    [SerializeField]
    private LayerMask _excludePlayer;
    [SerializeField]
    private float _groundRayOffset, _groundRayOverhead, _groundRayJumpDecrease, _sphereOffset, _sphereRadius, _groundOffset; // TODO remove _sphereRadius?


    [Header("Components"), SerializeField]
    private SphereCollider _sphereCollider;
    [SerializeField]
    private SphereCollider _sphereFeetCollider;
    [SerializeField]
    private Transform _cameraTransform;
    [SerializeField]
    private Joystick joystick;

    [Header("Camera"), SerializeField]
    private Vector3 _cameraOffset;
    [SerializeField]
    private Vector2 _pitchMinMax;
    [SerializeField]
    private float _cameraSensitivityX, _cameraSensitivityY, _distanceFromTarget, _rotationSmoothTime, _cameraClippingOffset, _automaticCameraRotationSpeed;

    [Header("Target Lock"), Range(0f, 1f), SerializeField]
    private float _targetLockTimeWindow;
    [SerializeField]
    public float _targetLockMaxFingerDistance, _targetLockRayDistance;
    [Range(1, 10), SerializeField]
    private int _recordedTouchesLimit;
    [SerializeField]
    private LayerMask _excludeUILayer;

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

    [Header("Animatons")]
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private AnimatorOverrideController _fistsOverrideController, _onehandedOverrideController, _twohandedOverrideController, _bothhandedOverrideController; 

    private InventorySlotContainer _inventoryContainer;
    private CharacterStats _currentStats;

    private Vector3 _rotationSmoothVelocity, _currentRotation;
    private float _yaw, _pitch;

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
    private Vector3 _rollVector, _velocityRotation;

    // Camera
    private int _rightFingerId;
    private Vector2 _lookInput;
    private Dictionary<int, float> _fingerTouchTimeDictionary;
    private Transform _cameraLockedTarget;
    private bool _lockedOnTarget;

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

        _rightFingerId = -1;
        _fingerTouchTimeDictionary = new Dictionary<int, float>(_recordedTouchesLimit);
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

    // Directional Input
    void Update()
    {
        GetInput();
    }

    // Camera stuff
    void LateUpdate()
    {
        Vector3 newCamPos;
        RaycastHit hit;

        if (_rightFingerId != -1 && _lookInput != Vector2.zero)
        {
            // Ony look around if the right finger is being tracked
            //Debug.Log("Rotating");
            LookAround();
            _lockedOnTarget = false; // TODO
        }
        else
        {
            if (_lockedOnTarget)
            {
                _currentRotation = new Vector3(_currentRotation.x, Mathf.LerpAngle(_currentRotation.y, Quaternion.LookRotation(_cameraLockedTarget.position - transform.position).eulerAngles.y, _automaticCameraRotationSpeed * Time.deltaTime));
                _cameraTransform.eulerAngles = _currentRotation;
            }
            else if (_velocityRotation != Vector3.zero) // CHANGE x and z != 0
            {
                // TODO hopefully works; Rotates camera so it faces player movement direction  
                /*Debug.Log("Current rot: " + currentRotation.y);
                Debug.Log("Quaternion look rot: " + Quaternion.LookRotation(velocityRotation).eulerAngles.y);
                Debug.Log("Result: " + Quaternion.LookRotation(velocityRotation).eulerAngles.y);*/
                _currentRotation = new Vector3(_currentRotation.x, Mathf.LerpAngle(_currentRotation.y, Quaternion.LookRotation(_velocityRotation).eulerAngles.y, _automaticCameraRotationSpeed * Time.deltaTime));
                _cameraTransform.eulerAngles = _currentRotation;
            }
        }

        newCamPos = transform.position - _cameraTransform.forward * _distanceFromTarget + _cameraOffset;

        // Camera Collision Check
        Ray ray = new Ray(transform.position + _cameraOffset, newCamPos - (transform.position + _cameraOffset));
        if (Physics.Raycast(ray, out hit, _distanceFromTarget, _excludePlayer))
        {
            _cameraTransform.position = hit.point + _cameraTransform.forward * _cameraClippingOffset;
        }
        else
        {
            _cameraTransform.position = newCamPos;
        }
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
                        _rollVector = _cameraTransform.TransformDirection(_rollVector);
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
        _velocityRotation = new Vector3(_velocity.x, 0, _velocity.z); // TODO try to get rid of it
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
            _velocity = _cameraTransform.TransformDirection(_velocity);

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
        transform.position += GamePhysics.ResolveCollisions(transform.position, transform.rotation, transform.TransformPoint(_sphereCollider.center), _sphereCollider, _excludePlayer);
    }

    private bool IsGrounded()
    {
        if(_velocity.y > 0 || _jumpNow)
        {
            _jumpNow = false;
            return false;
        }

        bool grounded = GamePhysics.IsGroundedWithMaxStepDistanceAndCollisions(transform.TransformPoint(_groundRayPosition), _groundRayOffset, _groundRayOverhead, _maxStep, _sphereFeetCollider, transform.TransformPoint(_sphereFeetCollider.center), _excludePlayer, out Vector3 correction);
        transform.position += correction;
        return grounded;
    }

    #endregion

    #region Button Activated Methods

    public void AttackInput()
    {
        if (_acceptingInput)
        {
            _nextAction = PlayerActionType.Attack;
        } else if (_attacking && _attacked)
        {
            _nextAction = PlayerActionType.Attack;
            animator.SetBool("Attack", true);
        }
    }

    public void JumpInput()
    {
        if (_acceptingInput)
        {
            _nextAction = PlayerActionType.Jump;
        }
    }

    public void RollInput()
    {
        if (_acceptingInput)
        {
            _nextAction = PlayerActionType.Roll;
        }
    }

    public void PauseGame()
    {
        _inventory.ShowInventory(false);
    }

    #endregion

    #region Camera

    private void GetInput()
    {
        // Tracking the finger that controlls the camera
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch t = Input.GetTouch(i);

            switch (t.phase)
            {
                case TouchPhase.Began:

                    // Didn¨t touch UI
                    if (!EventSystem.current.IsPointerOverGameObject(t.fingerId))
                    {
                        if (_rightFingerId == -1)
                        {
                            // Start tracking the rightfinger if it was not previously being tracked
                            _rightFingerId = t.fingerId;
                            //Debug.Log("Started tracking right finger");
                        }

                        if (_fingerTouchTimeDictionary.Count < _recordedTouchesLimit)
                        {
                            // and if it hits enemy; maybe not
                            _fingerTouchTimeDictionary.Add(t.fingerId, 0);
                        }
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:

                    if (t.fingerId == _rightFingerId)
                    {
                        // Stop tracking the right finger
                        _rightFingerId = -1;
                        //Debug.Log("Stopped tracking right finger");
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary.Remove(t.fingerId);

                        Ray ray = _cameraTransform.GetComponent<Camera>().ScreenPointToRay(t.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, _targetLockRayDistance, _excludeUILayer))
                        {
                            if(hit.transform.tag == "Damageable")
                            {
                                Debug.Log("Enemy Lock; dst: " + hit.distance);
                                // TODO camera movement
                                _lockedOnTarget = true;
                                _cameraLockedTarget = hit.transform;
                            }
                        }
                    }

                    break;
                case TouchPhase.Moved:

                    // Get input for looking around
                    if (t.fingerId == _rightFingerId)
                    {
                        _lookInput = t.deltaPosition * Time.deltaTime;
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary[t.fingerId] += t.deltaTime;
                        if (Vector2.SqrMagnitude(t.deltaPosition) > _targetLockMaxFingerDistance || _fingerTouchTimeDictionary[t.fingerId] > _targetLockTimeWindow)
                        {
                            _fingerTouchTimeDictionary.Remove(t.fingerId);
                        }
                    }

                    break;
                case TouchPhase.Stationary:
                    // Set the look input to zero if the finger is still
                    if (t.fingerId == _rightFingerId)
                    {
                        _lookInput = Vector2.zero;
                    }

                    if (_fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        _fingerTouchTimeDictionary[t.fingerId] += t.deltaTime;
                        if (_fingerTouchTimeDictionary[t.fingerId] > _targetLockTimeWindow)
                        {
                            _fingerTouchTimeDictionary.Remove(t.fingerId);
                        }
                    }
                    break;
            }
        }

        _joystickInput = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
    }

    private void LookAround()
    {
        // Moving camera
        _yaw += _lookInput.x * _cameraSensitivityX;
        _pitch -= _lookInput.y * _cameraSensitivityY;
        _pitch = Mathf.Clamp(_pitch, _pitchMinMax.x, _pitchMinMax.y);

        _currentRotation = Vector3.SmoothDamp(_currentRotation, new Vector3(_pitch, _yaw), ref _rotationSmoothVelocity, _rotationSmoothTime);
        _cameraTransform.eulerAngles = _currentRotation;

        // change rotation based on movement; prob useless
        /*Vector3 e = cameraTransform.eulerAngles;
        e.x = 0;

        transform.eulerAngles = e;*/
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
                _attackOverlaps[i].GetComponent<IDamageable>().TakeDamage(_currentStats.Damage, _currentStats.ArmourPenetration);
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
        Debug.Log("Switched controller");
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

            return true;
        }
        else
        {
            return true;
        }
    }

    #endregion

    public InventoryMonoBehaviour GetPlayerInventory()
    {
        return _inventory;
    }

    public Transform GetPlayerCameraTransform()
    {
        return _cameraTransform;
    }

    public void SetWeapons(GameObject prefab, bool twoHanded)
    {
        if(_rightHandTransform.childCount > 0)
        {
            Destroy(_rightHandTransform.GetChild(0).gameObject);
        }
        if (_leftHandTransform.childCount > 0)
        {
            Destroy(_leftHandTransform.GetChild(0).gameObject);
        }

        Instantiate(prefab, _rightHandTransform);
        if (twoHanded)
        {
            Instantiate(prefab, _leftHandTransform);
        }
    }

    public void TakeDamage(float damageTaken, float armourPenentration)
    {
        if (_rolling)
        {
            if(_rollInvincibilityTiming.x <= _rollTimer && _rollTimer <= _rollInvincibilityTiming.y)
            {
                return;
            }
        }

        float armourLeft = Mathf.Max(_currentStats.Armour - armourPenentration, 0);

        _currentHealth -= damageTaken + armourLeft;
        _healthBar.SetValue(_currentHealth / _currentStats.Health);

        if(_currentHealth <= 0)
        {
            Debug.Log("dead"); // TODO respawn
        }
    }
}