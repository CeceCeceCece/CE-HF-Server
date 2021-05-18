using UnityEngine;

public class Mage : ClassBase
{

    [Header("Fireball - Spell1:")]
    [Header("Class Specific:")] 
    public float fireballCD = 1f;
   
    public float fireballThrowForce = 600f;
    public float fireballExplosionForce = 10f;
    public float fireballExplosionRadius = 10f;


    private bool isFireballOnCD = false;
    public  void BasicAttackOriginal(Vector3 viewDirection, Vector3 shootPosition) //NEEDS REWRITE
    {
        if (health <= 0) return;

        if (Physics.Raycast(shootPosition, viewDirection, out RaycastHit hit, 25f))
        {
            if (hit.collider.CompareTag("Player"))
            {

                var penetration = 0f;
                var multiplier = 1f;
                if (basicAttackDamageType == (int)DamageType.Physical)
                {
                    penetration = armorPenetration;
                    multiplier = physicalDamageMultiplier;
                }
                else if (basicAttackDamageType == (int)DamageType.Magical)
                {
                    penetration = resistancePenetration;
                    multiplier = magicalDamageMultiplier;
                }
                else if (basicAttackDamageType == (int)DamageType.Unmitigateable)
                {
                    penetration = 100;
                    multiplier = physicalDamageMultiplier * magicalDamageMultiplier;
                }


                    
                hit.collider.GetComponent<RigidbodyPlayer>()
                    .TakeDamage(CalculateDamage(basicAttackDamage, multiplier, criticalStrikeChance), basicAttackDamageType, penetration);
            }
        }
    }

    public override void BasicAttack(Vector3 viewDirection, Vector3 shootPosition, Transform shootOrigin)
    {
        if (health <= 0f)
        {
            return;
        }
        if (!isBasicAttackOnCD)
        {

            var metrics = CalculateBasicAttackMetrics();
            NetworkManager.instance.BasicAttackInit(shootOrigin, basicAttackPrefab).Initialize(viewDirection, basicAttackProjectileForce, PlayerID, CalculateDamage(basicAttackDamage, metrics.Item2, criticalStrikeChance), metrics.Item1, basicAttackDamageType);
            isBasicAttackOnCD = true;
            Invoke(nameof(ResetBasicAttackCD), basicAttackCD * CooldownReduction);
        }
    }

    public override void Spell1(Vector3 viewDirection, Transform shootOrigin) //Fireball
    {
        if (health <= 0f)
        {
            return;
        }
        if (!isFireballOnCD)
        {
            NetworkManager.instance.FireballInit(shootOrigin).Initialize(viewDirection, fireballThrowForce, PlayerID, CalculateDamage(magicalDamage, magicalDamageMultiplier, criticalStrikeChance), fireballExplosionForce, fireballExplosionRadius, resistancePenetration);
            isFireballOnCD = true;
            Invoke(nameof(ResetFireballCD), fireballCD * CooldownReduction);
        }
    }
    public override void Initialize()
    {
        health = maxHealth;
    }



    public void ResetFireballCD()
    {
        isFireballOnCD = false;
    }
    
    public override void SpecialAttack(Vector3 viewDirection, Vector3 shootPosition) //Blink
    {
        throw new System.NotImplementedException();
    }

    public override void Spell2(Vector3 viewDirection, Transform shootPosition) //Blast Wave
    {
        throw new System.NotImplementedException();
    }

    public override void Spell3(Vector3 viewDirection, Transform shootPosition) //Ice Block
    {
        throw new System.NotImplementedException();
    }
}

