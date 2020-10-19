using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [Header("Attack General")]
    public float damage;
    public bool findEnemy;
    public LayerMask enemies;
    public Transform target;
    public Vector3 offsetPosition;
    
    [Header("Attack Timings")]
    public float attackDuration;
    public float attackTime;

    [Header("Attack Collisions")]
    public float weaponRange;
    public float angleInDegrees;
    public int maxCollisions;

    private bool attackPressed;
    private bool attacking;
    private bool attacked;
    private float attackingForSeconds;
    private Vector3 direction;
    private Collider[] overlaps;
    private int collisions;

    // Start is called before the first frame update
    void Start()
    {
        direction = new Vector3(0, 0, 1.0f);
        overlaps = new Collider[maxCollisions];
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = target.position + offsetPosition;

        if (attackPressed)
        {
            attackingForSeconds = 0;
            attacked = false;
            attacking = true;
        } else if (attacking)
        {
            attackingForSeconds += Time.fixedDeltaTime;
            if(attackTime < attackingForSeconds && !attacked)
            {
                CalculateAttack();
                attacked = true;
            }
        }

        attackPressed = false;
    }

    private void CalculateAttack()
    {
        collisions = Physics.OverlapSphereNonAlloc(target.position, weaponRange, overlaps, enemies);
        for (int i = 0; i < collisions; i++)
        {
            if(overlaps[i].GetType() == typeof(MeshCollider)){
                continue;
            }

            Vector3 attackDirection = overlaps[i].ClosestPoint(target.position) - target.position;

            if (angleInDegrees > Vector3.Angle(target.TransformVector(direction), attackDirection))
            {
                overlaps[i].GetComponent<IDamageable>().TakeDamage(damage);
                Debug.DrawRay(target.position, attackDirection, Color.green);
            } else
            {
                Debug.DrawRay(transform.position, attackDirection, Color.grey);
            }
        }
    }

    public void Attack()
    {
        attackPressed = true;
    }
}
