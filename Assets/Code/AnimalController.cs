using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour {

    public float MAX_SPEED = 10;
    public float MAX_FORCE = 7;
    public float MAX_HP = 10;
    public Color fatalColor;

    public enum MovemetMode
    {
        Wander = 0,
        Seek,
        Attack,
        Flee
    }

    Rigidbody2D rgb;
    SpriteRenderer sprite;
    Color startColor;
    Vector2 acceleration = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    MovemetMode moveMode = MovemetMode.Wander;
    Vector2 seekTarget = Vector2.zero;
    GameObject attackTarget = null;
    public float hp;

    public MovemetMode CurrentMoveMode { get { return moveMode; } }
    
	// Use this for initialization
	void Start () {
        rgb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        hp = MAX_HP;
        startColor = sprite.color;
	}
	
	// Update is called once per frame
	void Update () {
        switch(moveMode)
        {
            case MovemetMode.Wander:
                Wander();
                break;

            case MovemetMode.Seek:
                Arrival(seekTarget);
                break;

            case MovemetMode.Attack:
                if (CheckForFight())
                    Arrival(attackTarget.transform.position);
                else
                    moveMode = MovemetMode.Seek;
                break;

            default:
                break;
        }

        transform.up = rgb.velocity.normalized;

        sprite.color = Color.Lerp(fatalColor, startColor, hp / MAX_HP);
    }

    void Wander()
    {
        Vector2 sway = transform.right * Random.Range(-2.0f, 2.0f);
        Vector2 steering_force = (Vector2)transform.up * Random.Range(4.0f, 6.0f) + sway;

        if (steering_force.magnitude > MAX_FORCE)
            steering_force = steering_force.normalized * MAX_FORCE;

        acceleration = steering_force / rgb.mass;

        velocity += acceleration;

        if (velocity.magnitude > MAX_SPEED)
            velocity = velocity.normalized * MAX_SPEED;

        rgb.velocity = velocity;
    }

    void Arrival(Vector2 seekTarget)
    {
        Vector2 target_offset = seekTarget - (Vector2)transform.position;
        float distance = target_offset.magnitude;
        float ramped_speed = MAX_SPEED * (distance / 1.5f);
        float clipped_speed = Mathf.Min(ramped_speed, MAX_SPEED);

        Vector2 desired_velocity = (clipped_speed / distance) * target_offset;

        Vector2 steering_force = desired_velocity - velocity;

        if (steering_force.magnitude > MAX_FORCE)
            steering_force = steering_force.normalized * MAX_FORCE;

        acceleration = steering_force / rgb.mass;

        velocity += acceleration;

        if (velocity.magnitude > MAX_SPEED)
            velocity = velocity.normalized * MAX_SPEED;

        rgb.velocity = velocity;
    }

    bool CheckForFight()
    {
        if (attackTarget == null)
            return false;

        AnimalController targetController = attackTarget.GetComponent<AnimalController>();

        if (targetController.CurrentMoveMode == MovemetMode.Flee)
            return false;

        return true;
    }

    void TakeDamage(float val)
    {
        hp -= val;

        if (hp < 0)
        {
            hp = 0;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name.ToLower().Contains("wall"))
        {
            Vector2 collisionVector = collision.contacts[0].point - (Vector2)transform.position;

            transform.up = -collisionVector.normalized; 
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name.ToLower().Contains("wall"))
        {
            Vector2 collisionVector = collision.contacts[0].point - (Vector2)transform.position;

            transform.up = -collisionVector.normalized;
        }

        if (collision.gameObject.name.ToLower().Contains("animal") && moveMode != MovemetMode.Wander)
        {
            //if (moveMode != MovemetMode.Attack)
            //{
            //    attackTarget = collision.gameObject;
            //    moveMode = MovemetMode.Attack;
            //}

            TakeDamage(Random.Range(0.5f, 1.5f));
        }

        if (collision.gameObject.name.ToLower().Contains("animal") && moveMode != MovemetMode.Wander)
        {
            TakeDamage(Random.Range(0.5f, 1.5f));
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name.ToLower().Contains("food") && moveMode != MovemetMode.Seek && attackTarget == null)
        {
            seekTarget = collider.transform.position;
            moveMode = MovemetMode.Seek;
        }

        if (collider.gameObject.name.ToLower().Contains("animal") && moveMode != MovemetMode.Wander)
        {
            if (!collider.isTrigger)
            {
                attackTarget = collider.gameObject;
                moveMode = MovemetMode.Attack;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.name.ToLower().Contains("food") && moveMode == MovemetMode.Seek && attackTarget == null)
        {
            seekTarget = Vector2.zero;
            moveMode = MovemetMode.Wander;
        }
    }
}
