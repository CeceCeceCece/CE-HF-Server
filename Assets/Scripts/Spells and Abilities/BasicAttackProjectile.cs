using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAttackProjectile : MonoBehaviour
{
    public static Dictionary<int, BasicAttackProjectile> basicAttacks = new Dictionary<int, BasicAttackProjectile>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigidBody;
    public int thrownByPlayer;
    public Vector3 initialForce;
    public float damage;
    public float penetration;
    public int damageType;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        basicAttacks.Add(id, this);

        ServerSend.SpawnBasicAttack(this, thrownByPlayer);

        rigidBody.AddForce(initialForce);
        StartCoroutine(FadeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.BasicAttackPosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var playerHit = collision.other.GetComponent<RigidbodyPlayer>();
        if (playerHit != null)
        {
            if (playerHit.id == thrownByPlayer) return;
            playerHit.TakeDamage(damage, damageType, penetration);
        }
        
        Explode();
    }


    
    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer, float _damage, float _penetrationAmount, int _damageType)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
        damage = _damage;
        damageType = _damageType;
        penetration =_penetrationAmount;

    }

    private void Explode()
    {
        ServerSend.BasicAttackHit(this);
        basicAttacks.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator FadeAfterTime()
    {
        yield return new WaitForSeconds(0.5f);

        Explode();
    }
}

