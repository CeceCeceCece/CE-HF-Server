using UnityEngine;
using System.Collections;
using System;

public class RigidbodyPlayer : MonoBehaviour
{

    //Other
    Rigidbody rb;
    ClassBase characterType;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 1f;
    public int itemAmount = 0;
    public int maxItemAmount = 3;

    //Movement
    //public float moveSpeed = 4500;
    public bool grounded;
    public LayerMask whatIsGround;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    public float maxSlopeAngle = 35f;

    //Crouch & Slide
    /*private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    public float slideForce = 400;
    public float slideCounterMovement = 0.2f;*/

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;
    public float jumpForce = 550f;

    private Vector3 normalVector = Vector3.up;

    //Input
    float x, y;
    bool jumping;/*, sprinting, crouching;*/
    public int id;
    public string username;
    public Transform shootOrigin;
   // public float throwForce = 600f;
    //public float health;
   // public float maxHealth;

    private bool[] inputs;
    private float yVelocity = 0;
    private bool isDead = false;



    private void Start()
    {
        
    }
    public void Initialize(int _id, string _username)
    {
        id = _id;
        username = _username;
       
        rb = GetComponent<Rigidbody>();
        CharacterType = GetComponent<ClassBase>();
        CharacterType.PlayerID = id;
        CharacterType.Initialize();
        inputs = new bool[5];
    }

    public void FixedUpdate()
    {
        if (isDead)
        {
            return;
        }
        x = 0;
        y = 0;
        jumping = false;
        if (inputs[0])
        {
            y = 1;
        }
        if (inputs[1])
        {
            y = -1;
        }
        if (inputs[2])
        {
           x = -1;
        }
        if (inputs[3])
        {
           x = 1;
        }
        if (inputs[4])
            jumping = true;

        Move();
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
        //Move(_inputDirection);
    }

    public void Spell3()
    {
        CharacterType.Spell3(transform.position, shootOrigin);
    }

    private void Move()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If holding jump && ready to jump, then jump
        if (readyToJump && jumping) Jump();

        //Set max speed
        float maxSpeed = CharacterType.MaximumMovementSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
       /* if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }*/

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed) x = 0;
        if (x < 0 && xMag < -maxSpeed) x = 0;
        if (y > 0 && yMag > maxSpeed) y = 0;
        if (y < 0 && yMag < -maxSpeed) y = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
       // if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        rb.AddForce(transform.forward * y * CharacterType.MoveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(transform.right * x * CharacterType.MoveSpeed * Time.deltaTime * multiplier);

       
    }

    private void Jump()
    {
        if (grounded && readyToJump)
        {
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            //rb.AddForce(normalVector * jumpForce * 0.5f);

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) return;

        //Slow down sliding
        /*if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }
        */
        //Counter movement
        if (Math.Abs(mag.x) > threshold && Math.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(CharacterType.MoveSpeed * transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Math.Abs(mag.y) > threshold && Math.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(CharacterType.MoveSpeed * transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > CharacterType.MaximumMovementSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * CharacterType.MaximumMovementSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }



    private float desiredX;

    /*private void Move(Vector2 _inputDirection)
    {

        Vector3 _moveDirection = transform.right * _inputDirection.x + transform.forward * _inputDirection.y;
        _moveDirection *= moveSpeed;



        if (controller.isGrounded)
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
    }*/

    public void SetInput(bool[] _inputs, Quaternion _rotation)
    {
        inputs = _inputs;
        transform.rotation = _rotation;
    }

    public void Spell2()
    {
       
        CharacterType.Spell2(Vector3.zero, shootOrigin);

    }

    public void Spell1(Vector3 _viewDirection)
    {
        Debug.Log("Spell1");
        CharacterType.Spell1(_viewDirection, shootOrigin);

    }
    public void BasicAttack(Vector3 viewDirection)
    {
        Debug.Log("Basic");
        CharacterType.BasicAttack(viewDirection, shootOrigin.position, shootOrigin);
       
    }

    public void SpecialAttack(Vector3 direction)
    {
        CharacterType.SpecialAttack(direction, shootOrigin.position);
    }
    public void TakeDamage(float amount, int damageType, float penetration)
    {
        if (isDead) return;

        CharacterType.TakeDamage(amount, damageType, penetration);
        if (CharacterType.health <= 0)
        {
            isDead = true;
            CharacterType.health = 0f;
            transform.position = new Vector3(0f, 25f, 0f);
            ServerSend.PlayerPosition(this);
            StartCoroutine(Respawn());
        }

        ServerSend.PlayerHealth(this);
    }

    /*public float DamageTaken(float amount, int damageType, float penetration)
    {
        if (damageType == (int)DamageType.Physical)
            return amount * characterType.armor/100 *(100-penetration)/100;
        if (damageType == (int)DamageType.Magical)
            return amount * characterType.resistance / 100 * (100 - penetration) / 100;
        else
            return amount;
    }*/
    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    public ClassBase CharacterType { get => characterType; set => characterType = value; }

    /// <summary>
    /// Handle ground detection
    /// </summary>
    private void OnCollisionStay(Collision other)
    { 
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;
 
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
           
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

       
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);
        isDead = false;
        CharacterType.Initialize();
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


