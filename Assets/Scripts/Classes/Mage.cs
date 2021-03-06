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


    [Header("Blast Wave - Spell2:")]
    public float blastWaveCD = 3f;
    private bool isBlastWaveOnCD = false;
    public float blastWaveRadius = 5f;
    public float blastForce = 500f;


    [Header("Ice Block - Spell3:")]
    public float iceBlockCD = 10f;
    private bool isIceBlockOnCD = false;
    public float iceBlockDuration = 10f;
    public bool isIceBlocked = false;
   


    [Header("Special Attack - Blink")]
    public float blinkDistance = 20f;
    public float blinkCD = 2f;
    public bool isBlinkOnCD = false;
    public new bool IsSpellcastAvailable => !(IsDead || isStunned ||isIceBlocked);

    public override void BasicAttack(Vector3 viewDirection, Vector3 shootPosition, Transform shootOrigin)
    {
        if (!IsSpellcastAvailable) return;
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
        if (!IsSpellcastAvailable) return;
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


    public void ResetBlinkCD()
    {
        isBlinkOnCD = false;
    }
    public override void SpecialAttack(Vector3 viewDirection, Vector3 shootPosition) //Blink
    {
        if (!IsSpellcastAvailable) return;
        if (!isBlinkOnCD)
        {
            var teleportVector = viewDirection.normalized * blinkDistance;
            var rb = GetComponent<Rigidbody>();
            var testposition = rb.position + teleportVector;
            var isNewPosValid = false;
            while(!isNewPosValid)
            {
                Collider[] _colliders = Physics.OverlapSphere(testposition, 0.49f);
                if (_colliders.Length != 0 || testposition.y <= -1f)
                {
                    testposition -= viewDirection.normalized;
                }
                else
                    isNewPosValid = true;
                   

            }
            rb.position += teleportVector;





        }
    }

    public override void Spell2(Vector3 viewDirection, Transform shootPosition) //Blast Wave
    {


        if (!IsSpellcastAvailable) return;
        if (!isBlastWaveOnCD)
        {
            NetworkManager.instance.BlastWaveCasted(transform.position);

            Collider[] _colliders = Physics.OverlapSphere(transform.position, blastWaveRadius);
            foreach (Collider _collider in _colliders)
            {
                if (_collider.CompareTag("Player"))
                {
                    if (PlayerID != _collider.GetComponent<RigidbodyPlayer>().id)
                    {
                        _collider.GetComponent<RigidbodyPlayer>().TakeDamage(magicalDamage * magicalDamageMultiplier, (int)DamageType.Magical, resistancePenetration);
                        var rb = _collider.GetComponent<Rigidbody>();
                        if (rb != null)
                        {
                            rb.AddExplosionForce(blastForce, transform.position, blastWaveRadius, 3f);
                        }
                    }

                }
                /*else if (_collider.CompareTag("Enemy"))
                {
                    _collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
                }*/


            }
            isBlastWaveOnCD = true;
            Invoke(nameof(ResetBlastWaveCD), blastWaveCD * CooldownReduction);
        }
        
    }

    private void ResetBlastWaveCD()
    {
        isBlastWaveOnCD = false;
    }

    public override void Spell3(Vector3 viewDirection, Transform shootPosition) //Ice Block
    {
        if (IsDead) return;

        if (!isIceBlockOnCD)
        {
            NetworkManager.instance.IceBlockCasted(PlayerID);
            isIceBlockOnCD = true;
            isIceBlocked = true;
            isImmuneToDamage = true;
            isMovementLocked = true;
            GetComponent<Rigidbody>().isKinematic = true;
            Invoke(nameof(ResetIceBlockCD), iceBlockCD * CooldownReduction);
            Invoke(nameof(RemoveIceBlockEffect), iceBlockDuration);
        }
        else RemoveIceBlockEffect();
    }

    private void ResetIceBlockCD()
    {
        isIceBlockOnCD = false;
    }

    private void RemoveIceBlockEffect()
    {
        isImmuneToDamage = false;
        isMovementLocked = false;
        isIceBlocked = false;
        isStunned = false;
        GetComponent<Rigidbody>().isKinematic = false;
        NetworkManager.instance.IceBlockEnded(PlayerID);


    }
}

