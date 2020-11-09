using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : EnemyStateMachineMonoBehaviour, IDamageable
{
    //public static Action OnBossDeath;

    [Header("Physics")]
    [SerializeField]
    private float _gravity; // TODO physics script

    [Header("Stats")]
    [SerializeField]
    private CharacterStats _bossStats;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private GameObject _healthBarGO; // TODO health bar fixed on screen?

    private Transform _target;
    private Coroutine _coroutine;

    private float _currentHealth;

    #region Unity Methods

    // Start is called before the first frame update
    void Awake()
    {
        _currentHealth = _bossStats.Health;
    }

    void Start()
    {
        ChangeState(WaitForActivation());
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

    private void PlayIntro() // TODO rename?
    {
        ChangeState(Intro());
    }

    #region States

    private IEnumerator WaitForActivation()
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
        }
    }

    private IEnumerator Intro()
    {
        Debug.Log("Entered boss fight");
        yield return null;
        
        // TODO enter next state
    }

    #endregion

    public void TakeDamage(float damage, float armourPenetration)
    {
        // TODO get healthbar
        /*_currentHealth -= damage; 
        _healthBar.fillAmount = _currentHealth / _bossStats.Health;

        if (_currentHealth <= 0)
        {
            _healthBarCanvas.enabled = false;
            GetDestroyed();
            Debug.Log(name + " destroyed");
        }
        else
        {
            _healthBarCanvas.enabled = true;
        }*/
    }
}
