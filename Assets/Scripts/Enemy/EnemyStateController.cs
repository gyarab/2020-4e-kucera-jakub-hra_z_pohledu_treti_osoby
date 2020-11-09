using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyStateController : EntitySM, IDamageable // TODO remove?
{
    [Header("Enemy")]
    [SerializeField]
    private CharacterStats _stats;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private GameObject _healthBarGO;

    [Header("Target detection")]
    [SerializeField]
    private float _detectionRange, _detectionAngle;
    [SerializeField]
    private Vector3 _firstRaycastOffset, _secondRaycastOffset;

    // Objects
    private Image _healthBar;
    private Canvas _healthBarCanvas;
    private Camera _mainCamera;
    private Transform _target;

    private float _currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        _mainCamera = Camera.main;
        _healthBarCanvas = _healthBarGO.GetComponent<Canvas>();
        _healthBar = _healthBarCanvas.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>();

        _currentHealth = _stats.Health;
        _healthBarCanvas.enabled = false;

        // TODO set target

        InitState(new LookAroundState(this)); // dont create new
    }

    private void Update()
    {
        State.LogicUpdate();
    }

    private void FixedUpdate()
    {
        State.PhysicsUpdate();
    }

    public void UpdateCanvas()
    {
        if (_healthBarCanvas.enabled)
        {
            LookAtCamera();
        }
    }

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

    public bool VisionClearenceToTarget()
    {
        RaycastHit hit;

        Debug.DrawRay(transform.position + _firstRaycastOffset, _target.transform.position - _firstRaycastOffset - transform.position, Color.yellow);
        Debug.DrawRay(transform.position + _secondRaycastOffset, _target.transform.position - _secondRaycastOffset - transform.position, Color.blue);
        if (Physics.Raycast(transform.position + _firstRaycastOffset, _target.transform.position - _firstRaycastOffset - transform.position, out hit, Mathf.Infinity, ~transform.gameObject.layer))
        {
            if (hit.transform.gameObject.tag == _target.transform.tag)
            {
                if (Physics.Raycast(transform.position + _secondRaycastOffset, _target.transform.position - _secondRaycastOffset - transform.position, Mathf.Infinity, ~transform.gameObject.layer))
                {
                    if (hit.transform.gameObject.tag == _target.transform.tag)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    // TODO die and rop items
    public void TakeDamage(float damage, float armourPenetration)
    {
        _currentHealth -= damage; // TODO add character stats
        _healthBar.fillAmount = _currentHealth / _stats.Health;

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
