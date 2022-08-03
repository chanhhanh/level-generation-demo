using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Entity : MonoBehaviour
{
    [SerializeField]
    protected float health, maxHealth;
    [SerializeField]
    protected float mana, maxMana;
    [SerializeField]
    protected float strength, dexterity, intelligence, constitution, perception, speed;
    protected float bonusStr, bonusCon, bonusDex, bonusInt, bonusPer, bonusSpd;
    [SerializeField]
    protected float armor, magicResist;
    public virtual void Start()
    {
        InitStats();
    }

    void TakeDamage(float damage)
    {
        health -= Mathf.Clamp(damage, 0, Mathf.Infinity);
        if (health <= 0)
        {
            Die();
        }
    }
    void PhysicalAttack(Entity entity)
    {
        float damage = strength - entity.armor;
        float critChance = perception / entity.perception * 0.5f;
        float critRoll = Random.Range(0, 1f);
        if (critRoll > 1 - critChance)
        {
            damage *= 1.75f;
        }
        entity.TakeDamage(damage);
    }
    void MagicalAttack(Entity entity)
    {
        float damage = intelligence - entity.magicResist;
        entity.TakeDamage(damage);
    }
    void Die()
    {
        Destroy(gameObject);
    }
    public void InitStats()
    {
        health = maxHealth;
        mana = maxMana;
    }
}
