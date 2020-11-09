using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum ActionType
{
    None,
    Attack,
    Roll,
    Jump
}

public class PlayerController : MonoBehaviour, IDamageable
{
    #region Basic Variables

    [Header("General")]
    public float controllerGroundHeightOffset;
    [Range(0.0f, 89.0f)]
    public float walkAngle; // not working rn
    public float rollDuration;

    [Header("Speeds")]
    public float moveSpeed;
    [Range(0.0f, 1.0f)]
    public float inAirSpeed;
    [Range(0.0f, 2.0f)]
    public float rollSpeedMultiplier;
    public float playerRotationSpeed;

    [Header("Physics")]
    public float gravity;
    public float jumpForce;

    [Header("Rays")]
    public float groundRayOffset;
    public float groundRayOverhead;
    public float groundRayJumpDecrease;
    public float sphereOffset;
    public float sphereRadius;
    public LayerMask excludePlayer;
    public float groundOffset;
    //public float groundSphereRadius; why?

    [Header("Components")]
    public SphereCollider sphereCollider;
    public SphereCollider sphereFeetCollider;
    public Transform cameraTransform;
    public Joystick joystick;

    [Header("Camera")]
    public float cameraSensitivityX;
    public float cameraSensitivityY;
    public float distanceFromTarget;
    public Vector2 pitchMinMax;
    public float rotationSmoothTime;
    public Vector3 cameraOffset;
    public float cameraClippingOffset;
    public float automaticCameraRotationSpeed;

    [Header("Target Lock")]
    [Range(0f, 1f)]
    public float targetLockTimeWindow;
    public float targetLockMaxFingerDistance;
    [Range(1, 10)]
    public int recordedTouchesLimit;
    public float targetLockRayDistance;
    public LayerMask excludeUILayer;

    [Header("Inventory and UI")]
    [SerializeField]
    private InventoryMonoBehaviour _inventory;

    [Header("Stats temporary")] // TODO remove, prob not
    [SerializeField]
    private CharacterStatsSO baseStats;
    private CharacterStats _currentStats;
    [SerializeField]
    private float currentHealth;

    [Header("Animatons")]
    [SerializeField]
    private Animator animator; // TODO change w weapon
    [SerializeField]
    private AnimatorOverrideController fistsOverrideController;
    [SerializeField]
    private AnimatorOverrideController onehandedOverrideController, twohandedOverrideController, bothhandedOverrideController; 

    private InventorySlotContainer _inventoryContainer;

    private Vector3 rotationSmoothVelocity;
    private Vector3 currentRotation;
    private float yaw;
    private float pitch;

    private float currentGravity;
    private RaycastHit groundHit;

    // const
    private Vector3 groundRayPosition;
    private Vector3 spherePos;
    private Vector3 groundPos;
    private float maxStep;

    // Actions
    private ActionType nextAction;
    private bool canDoAction, acceptingInput;

    // FixedUpdate
    private Vector3 velocity, collisionCorectionVector;
    private Vector3 joystickInput;
    private bool grounded;
    private bool jumpNow;
    private bool jumping;
    private float timeSinceGrounded;
    private bool rollPressed;
    private bool rolling;
    private float rollTimer;
    private Vector3 rollVector;
    private Vector3 velocityRotation;

    // Camera
    private int rightFingerId;
    private Vector2 lookInput;
    private Dictionary<int, float> fingerTouchTimeDictionary;
    private Transform cameraLockedTarget;
    private bool lockedOnTarget;

    #endregion

    // TODO public -> private + serializeField
    #region Attack Variables 

    [Header("Attack General")]
    [SerializeField]
    private Transform _rightHandTransform;
    [SerializeField]
    private Transform _leftHandTransform;
    public bool findEnemy;
    public LayerMask enemies;
    public Vector3 offsetPosition, offsetRotation; // TODO

    [Header("Attack Timings")]
    public float attackDuration;
    public float attackTime;

    [Header("Attack Collisions")]
    public float weaponRange;
    public float angleInDegrees;
    public int maxAttackCollisions;

    private bool attackPressed;
    private bool attacking;
    private bool attacked;
    private float attackingForSeconds;
    private Vector3 attackDirection;
    private Collider[] attackOverlaps;
    private int attackCollisions;

    #endregion

    #region Unity methods

    private void Awake()
    {
        groundRayPosition = new Vector3(0, -controllerGroundHeightOffset + groundRayOffset, 0);
        spherePos = new Vector3(0, -sphereOffset, 0);
        groundPos = new Vector3(0, -groundOffset, 0);
        maxStep = sphereRadius / Mathf.Cos(walkAngle * Mathf.Deg2Rad);

        rightFingerId = -1;
        fingerTouchTimeDictionary = new Dictionary<int, float>(recordedTouchesLimit);
        canDoAction = true;
        acceptingInput = true;

        AttackSettings();

        // TODO set stats and rigth animation controller
        SwitchAnimationController(AnimationType.Onehanded);
    }

    // Staaaaaaaaaaart
    private void Start()
    {
        GameManager.Instance.Player = gameObject;
        currentHealth = baseStats.health;
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

        if (rightFingerId != -1 && lookInput != Vector2.zero)
        {
            // Ony look around if the right finger is being tracked
            //Debug.Log("Rotating");
            LookAround();
            lockedOnTarget = false; // TODO
        }
        else
        {
            if (lockedOnTarget)
            {
                currentRotation = new Vector3(currentRotation.x, Mathf.LerpAngle(currentRotation.y, Quaternion.LookRotation(cameraLockedTarget.position - transform.position).eulerAngles.y, automaticCameraRotationSpeed * Time.deltaTime));
                cameraTransform.eulerAngles = currentRotation;
            }
            else if (velocityRotation != Vector3.zero) // CHANGE x and z != 0
            {
                // TODO hopefully works; Rotates camera so it faces player movement direction  
                /*Debug.Log("Current rot: " + currentRotation.y);
                Debug.Log("Quaternion look rot: " + Quaternion.LookRotation(velocityRotation).eulerAngles.y);
                Debug.Log("Result: " + Quaternion.LookRotation(velocityRotation).eulerAngles.y);*/
                currentRotation = new Vector3(currentRotation.x, Mathf.LerpAngle(currentRotation.y, Quaternion.LookRotation(velocityRotation).eulerAngles.y, automaticCameraRotationSpeed * Time.deltaTime));
                cameraTransform.eulerAngles = currentRotation;
            }
        }

        newCamPos = transform.position - cameraTransform.forward * distanceFromTarget + cameraOffset;

        // Camera Collision Check
        Ray ray = new Ray(transform.position + cameraOffset, newCamPos - (transform.position + cameraOffset));
        if (Physics.Raycast(ray, out hit, distanceFromTarget, excludePlayer))
        {
            cameraTransform.position = hit.point + cameraTransform.forward * cameraClippingOffset;
        }
        else
        {
            cameraTransform.position = newCamPos;
        }
    }

    //Everything else
    void FixedUpdate()
    {
        // FixedUpdate or Update?
        if (canDoAction && grounded)
        {
            switch (nextAction)
            {
                case ActionType.None:
                    break;
                case ActionType.Attack:
                    acceptingInput = false;
                    canDoAction = false;
                    nextAction = ActionType.None;

                    attackingForSeconds = 0;
                    attacked = false;
                    attacking = true;
                    animator.SetBool("Attack", true);
                    break;
                case ActionType.Roll:
                    acceptingInput = false;
                    canDoAction = false;
                    nextAction = ActionType.None;

                    rolling = true;
                    animator.SetTrigger("Roll");

                    if (joystickInput.x == 0 && joystickInput.z == 0)
                    {
                        rollVector = transform.forward * moveSpeed * rollSpeedMultiplier;
                    }
                    else
                    {
                        rollVector = new Vector3(joystickInput.x, currentGravity, joystickInput.z) * moveSpeed * rollSpeedMultiplier;
                        rollVector = cameraTransform.TransformDirection(rollVector);
                    }

                    rollTimer = 0;
                    break;
                case ActionType.Jump:
                    acceptingInput = false;
                    canDoAction = false;
                    nextAction = ActionType.None;

                    jumping = true;
                    jumpNow = true;
                    animator.SetTrigger("Jump");
                    break;
                default:
                    break;
            }
        }

        if (attacking)
        {
            Attack();
        }

        CalculatePosition();
        transform.position += velocity;
        //Debug.Log("1: " + velocity.x + ", " + velocity.y + ", " + velocity.z);

        Rotate();

        CheckForCollisions();
        transform.position += collisionCorectionVector;
        //Debug.Log("2: " + collisionCorectionVector.x + ", " + collisionCorectionVector.y + ", " + collisionCorectionVector.z);

        grounded = IsGrounded();
        animator.SetBool("Grounded", grounded);
        //Debug.Log("g " + grounded);
        //Debug.Log("r " + rolling);
    }

    #endregion

    #region FixedUpdate methods

    private void Attack()
    {
        attackingForSeconds += Time.fixedDeltaTime;
        if (!attacked && attackTime < attackingForSeconds)
        {
            CalculateAttack();
            attacked = true;
            animator.SetBool("Attack", false);
        } else if(attackDuration < attackingForSeconds)
        {
            if(nextAction != ActionType.Attack)
            {
                attacking = false;
                canDoAction = true;
                acceptingInput = true;
            } else
            {
                nextAction = ActionType.None;
                attackingForSeconds = 0;
                attacked = false;
                attacking = true;
            }
        }
    }

    private void Rotate()
    {
        velocityRotation = new Vector3(velocity.x, 0, velocity.z);
        if (velocityRotation != Vector3.zero) // CHANGE x and z != 0
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(velocityRotation), playerRotationSpeed * Time.deltaTime);
        }
    }

    private void CalculatePosition()
    {
        if(grounded)
        {
            if (jumping)
            {
                if(jumpNow == true)
                {
                    jumpNow = false;
                } else
                {
                    jumping = false;
                    canDoAction = true;
                    acceptingInput = true;
                }
            }
            //timeSinceGrounded = Time.fixedDeltaTime; // If gravity doesnt work
            timeSinceGrounded = 0;
        } else
        {
            timeSinceGrounded += Time.fixedDeltaTime;
        }

        if (jumping)
        {
            currentGravity = jumpForce * timeSinceGrounded - 0.5f * gravity * Mathf.Pow(timeSinceGrounded, 2);
        } else
        {
            currentGravity = (-gravity) * Mathf.Pow(timeSinceGrounded, 2);
        }

        if (attacking)
        {
            velocity = new Vector3(0, currentGravity, 0) * moveSpeed;
        }
        else if (rolling)
        {
            velocity = rollVector;
            if(rollTimer < rollDuration)
            {
                rollTimer += Time.fixedDeltaTime;
            } else
            {
                rolling = false;
                canDoAction = true;
                acceptingInput = true;
            }
        } else
        {
            velocity = new Vector3(joystickInput.x, currentGravity, joystickInput.z) * moveSpeed;
            velocity = cameraTransform.TransformDirection(velocity);

            if (!jumping)
            {
                if (joystickInput.x == 0 && joystickInput.z == 0)
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
        Collider[] overlaps = new Collider[4];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(sphereCollider.center), sphereCollider.radius, overlaps, excludePlayer);

        collisionCorectionVector = Vector3.zero;
        for (int i = 0; i < num; i++)
        {
            Transform t = overlaps[i].transform;
            Vector3 dir;
            float dist;

            if (Physics.ComputePenetration(sphereCollider, transform.position, transform.rotation, overlaps[i], t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                collisionCorectionVector += penetrationVector;
            }
        }
        collisionCorectionVector = GetXZVector(collisionCorectionVector); // Remove if collisions dont work
    }

    private bool IsGrounded()
    {
        Ray ray = new Ray(transform.TransformPoint(groundRayPosition), Vector3.down);

        //Debug.DrawRay(transform.TransformPoint(groundRayPosition), Vector3.down, Color.red);
        float rayDistance;
        if (jumping)
        {
            rayDistance = groundRayOffset - sphereRadius - groundRayJumpDecrease;
        } else
        {
            rayDistance = groundRayOffset - sphereRadius + groundRayOverhead;
        }

        RaycastHit[] hits = new RaycastHit[4];
        int num = Physics.SphereCastNonAlloc(ray, sphereRadius, hits, rayDistance, excludePlayer);
        float moveY = (rayDistance + sphereRadius) * 2;
        float actualYDistance;

        if(num < 1)
        {
            return false;
        }

        for (int i = 0; i < num; i++)
        {
            actualYDistance = hits[i].distance + sphereRadius;

            if (!grounded || actualYDistance >= (groundRayOffset - maxStep))
            {
                //Debug.Log("hit " + actualYDistance + ", hit " + i);
                if (actualYDistance < moveY)
                {
                    moveY = actualYDistance;
                }
            }
        }

        //Debug.Log("ray dist " + (rayDistance + sphereRadius));
        if(moveY <= rayDistance + sphereRadius)
        {
            //Debug.Log("moveY " + moveY);
            transform.position = new Vector3(transform.position.x, transform.position.y + groundRayOffset - moveY, transform.position.z);
        }

        if (grounded)
        {
            for (int i = 0; i < num; i++)
            {
                actualYDistance = hits[i].distance + sphereRadius;

                if (actualYDistance < (groundRayOffset - maxStep))
                {
                    //Debug.Log("too high " + actualYDistance + ", hit " + i);
                    Transform t = hits[i].collider.transform;
                    Vector3 dir;
                    float dist;

                    if (Physics.ComputePenetration(sphereFeetCollider, transform.position, transform.rotation, hits[i].collider, t.position, t.rotation, out dir, out dist))
                    {
                        //Debug.Log("Collision");
                        Vector3 penetrationVector = dir * dist;
                        transform.position += GetXZVector(penetrationVector);
                    }
                }
            }
        }

        return true;
        /*
        RaycastHit tempHit = new RaycastHit();
        if (Physics.SphereCast(ray, sphereRadius, out tempHit, rayDistance, excludePlayer)) // OLD script
        {
            //ConfirmGround(tempHit); from tutorial
            transform.position = new Vector3(transform.position.x, transform.position.y + groundRayOffset - sphereRadius - tempHit.distance, transform.position.z);
            return true;
        }
        else
        {
            return false;
        }*/
    }

    // TODO Rework? 
    public Vector3 GetXZVector(Vector3 input)
    {
        //Debug.Log("Input" + input.x + ", " + input.y + ", " + input.z);
        if(input.y == 0)
        {
            //Debug.Log("Unchanged");
            return input;
        } else if (input.x == 0 && input.z == 0)
        {
            return Vector3.zero;
        }

        float k;
        Vector3 result;

        k = Mathf.Pow(input.y, 2) / (Mathf.Pow(input.x, 2) + Mathf.Pow(input.z, 2));
        result = new Vector3(input.x * (k + 1), 0, input.z * (k + 1));

        //Debug.Log("Result" + result.x + ", " + result.y + ", " + result.z);
        return input + result;
    }

    #endregion

    #region Button Activated Methods

    public void AttackInput()
    {
        if (acceptingInput)
        {
            nextAction = ActionType.Attack;
        } else if (attacking && attacked)
        {
            nextAction = ActionType.Attack;
            animator.SetBool("Attack", true);
        }
    }

    public void JumpInput()
    {
        if (acceptingInput)
        {
            nextAction = ActionType.Jump;
        }
    }

    public void RollInput()
    {
        if (acceptingInput)
        {
            nextAction = ActionType.Roll;
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
                        if (rightFingerId == -1)
                        {
                            // Start tracking the rightfinger if it was not previously being tracked
                            rightFingerId = t.fingerId;
                            //Debug.Log("Started tracking right finger");
                        }

                        if (fingerTouchTimeDictionary.Count < recordedTouchesLimit)
                        {
                            // and if it hits enemy; maybe not
                            fingerTouchTimeDictionary.Add(t.fingerId, 0);
                        }
                    }

                    break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:

                    if (t.fingerId == rightFingerId)
                    {
                        // Stop tracking the right finger
                        rightFingerId = -1;
                        //Debug.Log("Stopped tracking right finger");
                    }

                    if (fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        fingerTouchTimeDictionary.Remove(t.fingerId);

                        Ray ray = cameraTransform.GetComponent<Camera>().ScreenPointToRay(t.position);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, targetLockRayDistance, excludeUILayer))
                        {
                            if(hit.transform.tag == "Damageable")
                            {
                                Debug.Log("Enemy Lock; dst: " + hit.distance);
                                // TODO camera movement
                                lockedOnTarget = true;
                                cameraLockedTarget = hit.transform;
                            }
                        }
                    }

                    break;
                case TouchPhase.Moved:

                    // Get input for looking around
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = t.deltaPosition * Time.deltaTime;
                    }

                    if (fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        fingerTouchTimeDictionary[t.fingerId] += t.deltaTime;
                        if (Vector2.SqrMagnitude(t.deltaPosition) > targetLockMaxFingerDistance || fingerTouchTimeDictionary[t.fingerId] > targetLockTimeWindow)
                        {
                            fingerTouchTimeDictionary.Remove(t.fingerId);
                        }
                    }

                    break;
                case TouchPhase.Stationary:
                    // Set the look input to zero if the finger is still
                    if (t.fingerId == rightFingerId)
                    {
                        lookInput = Vector2.zero;
                    }

                    if (fingerTouchTimeDictionary.ContainsKey(t.fingerId))
                    {
                        fingerTouchTimeDictionary[t.fingerId] += t.deltaTime;
                        if (fingerTouchTimeDictionary[t.fingerId] > targetLockTimeWindow)
                        {
                            fingerTouchTimeDictionary.Remove(t.fingerId);
                        }
                    }
                    break;
            }
        }

        joystickInput = new Vector3(joystick.Horizontal, 0, joystick.Vertical);
    }

    private void LookAround()
    {
        // Moving camera
        yaw += lookInput.x * cameraSensitivityX;
        pitch -= lookInput.y * cameraSensitivityY;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);

        currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, rotationSmoothTime);
        cameraTransform.eulerAngles = currentRotation;

        // change rotation based on movement; prob useless
        /*Vector3 e = cameraTransform.eulerAngles;
        e.x = 0;

        transform.eulerAngles = e;*/
    }

    #endregion

    #region Attack

    private void AttackSettings()
    {
        attackDirection = new Vector3(0, 0, 1.0f);
        attackOverlaps = new Collider[maxAttackCollisions];
    }

    private void CalculateAttack()
    {
        attackCollisions = Physics.OverlapSphereNonAlloc(transform.position, weaponRange, attackOverlaps, enemies);

        for (int i = 0; i < attackCollisions; i++)
        {
            if (attackOverlaps[i].GetType() == typeof(MeshCollider))
            {
                continue;
            }

            Vector3 attackDirection = attackOverlaps[i].ClosestPoint(transform.position) - transform.position;

            if (angleInDegrees > Vector3.Angle(transform.TransformVector(this.attackDirection), attackDirection))
            {
                attackOverlaps[i].GetComponent<IDamageable>().TakeDamage(_currentStats.Damage, _currentStats.ArmourPenetration);
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
                SetAnimationsController(fistsOverrideController);
                break;
            case AnimationType.Onehanded:
                SetAnimationsController(onehandedOverrideController);
                break;
            case AnimationType.Twohanded:
                SetAnimationsController(twohandedOverrideController);
                break;
            case AnimationType.Bothhanded:
                SetAnimationsController(bothhandedOverrideController);
                break;
            default:
                Debug.Log("Really?");
                break;
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
        _currentStats = new CharacterStats(baseStats.health, baseStats.armour, baseStats.damage, baseStats.armourPenetration);
        _currentStats.AddStats(equipmentStats);
    }

    public CharacterStats GetStats()
    {
        return _currentStats;
    }

    #endregion

    // Useless
    #region Tutorial

    /*private void ConfirmGround(RaycastHit hit)
    {
        Collider[] colliders = new Collider[3];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(groundPos), groundSphereRadius, colliders, excludePlayer);
        grounded = false;

        foreach (Collider c in colliders)
        {
            if(c.transform == hit.transform)
            {
                groundHit = hit;
                grounded = true;
                break;
            }
        }
        
        if(num <= 1 && hit.distance <= 3f)
        {
            if(colliders[0] != null)
            {
                Ray ray = new Ray(transform.TransformPoint(spherePos), Vector3.down);
                RaycastHit tempHit;

                if(Physics.Raycast(ray, out tempHit, excludePlayer))
                {
                    if(hit.transform != colliders[0].transform)
                    {
                        return;
                    }
                }
            }
        }

        grounded = true;
    }

    private void CollisionCheck()
    {
        Collider[] overlaps = new Collider[4];
        int num = Physics.OverlapSphereNonAlloc(transform.TransformPoint(sphereCollider.center), sphereCollider.radius, overlaps, excludePlayer);

        foreach (Collider c in overlaps)
        {
            Transform t = c.transform;
            Vector3 dir;
            float dist;

            if(Physics.ComputePenetration(sphereCollider, transform.position, transform.rotation, c, t.position, t.rotation, out dir, out dist))
            {
                Vector3 penetrationVector = dir * dist;
                velocity -= penetrationVector;
            }
        }
    }*/

    #endregion

    public InventoryMonoBehaviour GetPlayerInventory()
    {
        return _inventory;
    }

    public Transform GetPlayerCameraTransform()
    {
        return cameraTransform;
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
        float armourLeft = Mathf.Max(_currentStats.Armour - armourPenentration, 0);

        currentHealth -= damageTaken + armourLeft;
        if(currentHealth <= 0)
        {
            Debug.Log("dead"); // TODO respawn
        }
    }
}