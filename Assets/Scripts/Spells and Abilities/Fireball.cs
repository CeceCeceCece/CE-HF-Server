using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public static Dictionary<int, Fireball> fireballs = new Dictionary<int, Fireball>();
    private static int nextProjectileId = 1;

    public int id;
    public Rigidbody rigidBody;
    public int thrownByPlayer;
    public Vector3 initialForce;
    public float explosionRadius;
    public float damage;
    public float explosionForce;
    public float resistancePenetration;
    public int damageType = (int)DamageType.Magical;

    private void Start()
    {
        id = nextProjectileId;
        nextProjectileId++;
        fireballs.Add(id, this);

        ServerSend.SpawnFireball(this, thrownByPlayer);

        rigidBody.AddForce(initialForce);
        StartCoroutine(ExplodeAfterTime());
    }

    private void FixedUpdate()
    {
        ServerSend.FireballPosition(this);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var playerHit = collision.other.GetComponent<RigidbodyPlayer>();
        if(playerHit!= null)
        {
            if (playerHit.id == thrownByPlayer) return;
        }
        Explode();
    }

    public void Initialize(Vector3 _initialMovementDirection, float _initialForceStrength, int _thrownByPlayer, float _damage, float _explosionForce, float _explosionRadius, float _resistancePenetration)
    {
        initialForce = _initialMovementDirection * _initialForceStrength;
        thrownByPlayer = _thrownByPlayer;
        damage = _damage;
        explosionForce = _explosionForce;
        explosionRadius = _explosionRadius;
        resistancePenetration = _resistancePenetration;

    }

    private void Explode()
    {
        ServerSend.FireballExploded(this);

        Collider[] _colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider _collider in _colliders)
        {
            if (_collider.CompareTag("Player"))
            {
                if (thrownByPlayer != _collider.GetComponent<RigidbodyPlayer>().id)
                {
                    _collider.GetComponent<RigidbodyPlayer>().TakeDamage(damage, damageType, resistancePenetration);
                    var rb = _collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 3f);
                    }
                }
                
            }
            /*else if (_collider.CompareTag("Enemy"))
            {
                _collider.GetComponent<Enemy>().TakeDamage(explosionDamage);
            }*/

            
        }

        fireballs.Remove(id);
        Destroy(gameObject);
    }

    private IEnumerator ExplodeAfterTime()
    {
        yield return new WaitForSeconds(10f);

        Explode();
    }
}