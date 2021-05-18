using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public CharacterController controller;
    public Transform shootOrigin;

    public float health;
    public float maxHealth;
    public float gravity = -9.81f;

    private float moveSpeed = 5f;

    public float jumpSpeed = 9f;
    private bool[] inputs;
    private float yVelocity = 0;
    public int itemAmount = 0;
    public int maxItemAmount = 3;


    private void Start()
    {
        gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed *= Time.fixedDeltaTime;
        jumpSpeed *= Time.fixedDeltaTime;
    }
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
        health = maxHealth;

        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if(health <= 0)
        {
            return;
        }
        Vector2 _inputDirection = Vector2.zero;
        if (inputs[0])
        {
            _inputDirection.y += 1;
        }
        if (inputs[1])
        {
            _inputDirection.y -= 1;
        }
        if (inputs[2])
        {
            _inputDirection.x -= 1;
        }
        if (inputs[3])
        {
            _inputDirection.x += 1;
        }

        Move(_inputDirection);
    }

    private void Move(Vector2 _inputDirection)
    {
       
        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;

       

        if(controller.isGrounded)
        {
            yVelocity = 0f;
            if (inputs[4])
                yVelocity = jumpSpeed;
        }
        yVelocity += gravity;
        _moveDirection.y = yVelocity;
        
        controller.Move(_moveDirection);
        
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }
    public void Shoot(Vector3 viewDirection)
    {
        if(Physics.Raycast(shootOrigin.position, viewDirection, out RaycastHit hit, 25f))
        {
            if(hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<Player>().TakeDamage(50f);
            }
        }
    }
    public void TakeDamage(float amount)
    {
        if (health <= 0) return;
        health -= amount;
        if(health <= 0)
        {
            health = 0f;
            controller.enabled = false;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);
        health = maxHealth;
        controller.enabled = true;

        ServerSend.PlayerRespawned(this);

    }

    public bool AttemptPickupItem()
    {
        if (itemAmount >= maxItemAmount)
        {
            return false;
        }

        itemAmount++;
        return true;
    }
}

