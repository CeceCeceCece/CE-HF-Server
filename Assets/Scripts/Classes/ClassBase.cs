using UnityEngine;

public abstract class ClassBase : MonoBehaviour
{
    [Header("Health:")]
    public float health;
    public float maxHealth;


    [Header("Mitigation:")]
    [Range(0f, 100f)]
    public float armor;

    [Range(0f, 100f)]
    public float resistance;

    public float shieldedAmount = 0f;
    public float maxShieldedAmount = 200f;

    [Header("Penetration:")]
    [Range(0f, 100f)]
    public float armorPenetration;

    [Range(0f, 100f)]
    public float resistancePenetration;

    [Header("Base damage:")]
    public float physicalDamage;
    public float magicalDamage;
    public float basicAttackDamage;


    [Space]

    public int basicAttackDamageType;

    public float physicalDamageMultiplier = 1f;
    public float magicalDamageMultiplier = 1f;
    public float movementSpeedMultiplier = 1f;

    private float maximumMovementSpeed = 20f;
    private float moveSpeed = 4500;
    private float cooldownReduction = 1f;


    [Range(0f, 1f)]
    public float criticalStrikeChance = 0.1f;

    int playerID;
    public int PlayerID { get => playerID; set => playerID = value; }
    public float MaximumMovementSpeed { get => maximumMovementSpeed * movementSpeedMultiplier; set => maximumMovementSpeed = value; }
    public float MoveSpeed { get => moveSpeed * movementSpeedMultiplier; set => moveSpeed = value; }
    public float CooldownReduction { get => cooldownReduction; set => cooldownReduction = 1-value; }

    public abstract void BasicAttack(Vector3 viewDirection, Vector3 shootPosition);
    public abstract void SpecialAttack(Vector3 viewDirection, Vector3 shootPosition);
    public abstract void Spell1(Vector3 viewDirection, Transform shootPosition);
    public abstract void Spell2(Vector3 viewDirection, Transform shootPosition);
    public abstract void Spell3(Vector3 viewDirection, Transform shootPosition);

    public abstract void Initialize();

    public float DamageTaken(float amount, int damageType, float penetration)
    {
        if (damageType == (int)DamageType.Physical)
            return amount * armor / 100 * (100 - penetration) / 100;
        if (damageType == (int)DamageType.Magical)
            return amount * resistance / 100 * (100 - penetration) / 100;
        if (damageType == (int)DamageType.PhysicalPercentageBased)
            return amount * maxHealth * armor / 100 * (100 - penetration) / 100;
        if (damageType == (int)DamageType.MagicalPercentageBased)
            return amount * maxHealth * resistance / 100 * (100 - penetration) / 100;
        else
            return amount;
    }

    public void TakeDamage(float amount, int damageType, float penetration)
    { 
        shieldedAmount -= DamageTaken(amount, damageType, penetration); 
        if(shieldedAmount < 0f)
        {
            health += shieldedAmount;
            shieldedAmount = 0f;
        }
        
    }

    public void HealDamage(float amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }

    public void AddShield(float amount)
    {
        shieldedAmount += amount;
        if (shieldedAmount > maxShieldedAmount) shieldedAmount = maxShieldedAmount;
    }

    public float CalculateDamage(float damageAmount, float multiplier, float criticalStrikeChance)
    {
        var rolledRandom = Random.Range(0f, 1f);
        if (rolledRandom <= criticalStrikeChance)
        {
            return damageAmount * multiplier * 2;
            Debug.Log("Critical Hit!");
        }
        else
            return damageAmount * multiplier;
    }





}