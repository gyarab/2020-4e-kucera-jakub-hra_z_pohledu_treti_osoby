using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour, IDamageable
{
    // Základní proměnné
    #region Basic Variables

    [Header("General")]
    [SerializeField]
    private float _controllerGroundHeightOffset;
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
    private float _jumpForce;
    [SerializeField]
    private float _maxStep;
    [SerializeField]
    private LayerMask _collisionLayer;

    [Header("Rays")]
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private float _groundRayOffset, _groundRayOverhead, _groundRayJumpDecrease, _sphereOffset, _groundOffset;

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

    // Proměnné spojené s útočením
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

    // Inicializace proměnných
    private void Awake()
    {
        _groundRayPosition = new Vector3(0, -_controllerGroundHeightOffset + _groundRayOffset, 0);
        _spherePos = new Vector3(0, -_sphereOffset, 0);
        _groundPos = new Vector3(0, -_groundOffset, 0);

        _canDoAction = true;
        _acceptingInput = true;

        AttackSettings();

        SwitchAnimationController(AnimationType.Onehanded);
    }

    // Inicializace proměnných 2
    private void Start()
    {
        GameManager.Instance.Player = gameObject;
        _currentHealth = _baseStats.health;
    }

    // Metoda je volána jednou za stálý interval času
    void FixedUpdate()
    {
        // Pokud postava může provést akci a je na zemi
        if (_canDoAction && _grounded)
        {
            switch (_nextAction)
            {
                case PlayerActionType.None:
                    // Nic k porvedení
                    break;
                case PlayerActionType.Attack:
                    // Hráč se zastaví na místě a spustí animaci útočení
                    _acceptingInput = false;
                    _canDoAction = false;
                    _nextAction = PlayerActionType.None;

                    _attackingForSeconds = 0;
                    _attacked = false;
                    _attacking = true;
                    animator.SetBool("Attack", true);
                    break;
                case PlayerActionType.Roll:
                    // Hráč se začne kutálet dopředu, stane se nezasažitelným a spustí animaci útočení
                    _acceptingInput = false;
                    _canDoAction = false;
                    _nextAction = PlayerActionType.None;

                    _rolling = true;
                    animator.SetTrigger("Roll");

                    // Určení směru kotoulu, buď směrem, kterým se postava pohybuje, nebo směrem, kterým se dívá; spustí animaci uhýbání
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
                    // Postava vyskočí do vzduchu a začne se přehrávat animace skoku
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

        // Když postava útočí, je zavolána tato metoda
        if (_attacking)
        {
            Attack();
        }

        // Spočítá pozici postavy
        CalculatePosition();
        
        // Nasměruje postavu správným směrem
        Rotate();
        CheckForCollisions();

        // Zjistí, jestli je postava na zemi
        _grounded = IsGrounded();
        animator.SetBool("Grounded", _grounded);
    }

    #endregion

    #region FixedUpdate methods

    // Měří čas, kdy by postava měla zaútočit, a po uplynutí prodlevy postava zaútočí
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

    // Otáčí postavou, aby mířil směrem, kterým se pohybuje
    private void Rotate()
    {
        transform.rotation = GamePhysics.RotateTowardsMovementDirection(transform.rotation, _velocity, _playerRotationSpeed);
    }

    // Spočítá novou pozici postavy; bere v potaz stav postavy a vstup hráče
    private void CalculatePosition()
    {
        // Když je postava na zemi, nastaví časovač stráveného času ve vzduchu na 0
        if (_grounded)
        {
            if (_jumping)
            {
                // Po stisknutí tlačítka ke skoku nechá postavu se "odrazit" od země
                if(_jumpNow == false)
                {
                    _jumping = false;
                    _canDoAction = true;
                    _acceptingInput = true;
                }
            }
            _timeSinceGrounded = 0;
        } else // Jinak přičte k času strávenému ve vzduchu, čas strávený ve vzduchu před opětovným zavoláním této metody
        {
            _timeSinceGrounded += Time.fixedDeltaTime;
        }

        // Když hráč vyskočil do vzduchu je u výpočtu gravitace brán ohled na původní rychlost
        if (_jumping)
        {
            _currentGravity = GamePhysics.GetGravitationalForceWithInitialVelocity(_timeSinceGrounded, _jumpForce);
        } else // Jinak pouze počítá gravitační sílu
        {
            _currentGravity = GamePhysics.GetGravitationalForce(_timeSinceGrounded);
        }

        // Když postava útočí, tak na ni působí pouze gravitace
        if (_attacking)
        {
            _velocity = new Vector3(0, _currentGravity, 0);
        }
        else if (_rolling) // Postava vykonává kotoul stále stejným směrem
        {
            _velocity = _rollVector;
            _velocity.y = _currentGravity;
            if(_rollTimer < _rollDuration)
            {
                _rollTimer += Time.fixedDeltaTime;
            } else
            {
                _rolling = false;
                _canDoAction = true;
                _acceptingInput = true;
            }
        }
        else // Když pohyb postavy není omezen, pak je se pohybuje podle vstupu hráče
        {
            _velocity = new Vector3(_joystickInput.x, 0, _joystickInput.z) * _moveSpeed;
            _velocity.y = _currentGravity;
            _velocity = Quaternion.Euler(0, _yCameraRotation, 0) * _velocity;

            if (!_jumping)
            {
                // Nastaví animaci postavy podle toho, jestli se pohybuje
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

        transform.position += _velocity;
    }

    // Zkontroluje, jestli postava s něčím nekoliduje a případně upraví její pozici
    private void CheckForCollisions()
    {
        transform.position += GamePhysics.ResolveCollisionsNonY(transform.position, transform.rotation, transform.TransformPoint(_sphereCollider.center), _sphereCollider, _collisionLayer);
    }

    // Zjistí jestli postava se dotýká země
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

    // Obdrří vstup a zpracuje ho
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

    // Zpracovává vstup z joysticku
    public void SetJoystickInput(float horizontalInput, float verticalInput, float cameraYRotation)
    {
        _joystickInput = new Vector3(horizontalInput, 0, verticalInput);
        _yCameraRotation = cameraYRotation;
    }

    #endregion

    #region Camera

    // Metoda je volána z Input Manageru, který vektor předá kameře
    public Vector3 GetRotationVelocity()
    {
        return new Vector3(_velocity.x, 0, _velocity.z);
    }

    #endregion

    #region Attack

    // Inicializace prměnných spojených s útočením
    private void AttackSettings()
    {
        _attackDirection = new Vector3(0, 0, 1.0f);
        _attackOverlaps = new Collider[_maxAttackCollisions];
    }

    // Vypočítá, jestli útok postavy někoho trafil a udělí příslušný počet poškození
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

            if (_angleInDegrees > Vector3.Angle(transform.TransformVector(_attackDirection), attackDirection))
            {
                _attackOverlaps[i].transform.root.GetComponent<IDamageable>().TakeDamage(_currentStats.Damage, _currentStats.ArmourPenetration);
            }
        }
    }

    #endregion

    #region Animations
    
    // Nastaví animaci podle parametru AnimationType
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

    // Změní ovladač animací
    private void SetAnimationsController(AnimatorOverrideController overrideController)
    {
        animator.runtimeAnimatorController = overrideController;
    }

    #endregion

    #region Stats

    // Přepočítá staty postavy, jako parametr jsou předány staty z vybavení
    public void SetStats(CharacterStats equipmentStats)
    {
        _currentStats = new CharacterStats(_baseStats.health, _baseStats.armour, _baseStats.damage, _baseStats.armourPenetration);
        _currentStats.AddStats(equipmentStats);

        if(_currentHealth > _currentStats.Health)
        {
            _currentHealth = _currentStats.Health;
            _healthBar.SetValue(_currentHealth / _currentStats.Health);
        }
    }

    // Vrátí aktuální staty
    public CharacterStats GetStats()
    {
        return _currentStats;
    }

    // Obnoví životy na 100%
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
            return false;
        }
    }

    // Obnoví životy a odstraní limitované objekty z inventáře
    public void Reset()
    {
        _currentHealth = _currentStats.Health;
        _healthBar.SetValue(_currentHealth / _currentStats.Health);
        _inventory.BossHealthBar.SetVisibility(false);

        _inventory.RemoveTemporaryItems();
    }

    #endregion

    // Vrátí inventář spojený s hráčem
    public InventoryMonoBehaviour GetPlayerInventory()
    {
        return _inventory;
    }

    // Zasadí model zbraně do ruky postavy
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

    // Odstraní instance zbraně z rukou
    public void RemoveWeapons()
    {
        if (_rightHandTransform.childCount > 0)
        {
            Destroy(_rightHandTransform.GetChild(0).gameObject);
        }
        if (_leftHandTransform.childCount > 0)
        {
            Destroy(_leftHandTransform.GetChild(0).gameObject);
        }
    }

    // Metoda je volána, když hráč má obdržet poškození
    public void TakeDamage(float damageTaken, float armourPenetration)
    {
        if (_currentHealth <= 0)
        {
            return;
        }

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
            _inventory.ShowDeathScreen();
        }
    }
}