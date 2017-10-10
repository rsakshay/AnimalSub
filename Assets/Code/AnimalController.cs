using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour {

    public float MAX_SPEED = 10;
    public float MAX_FORCE = 7;

    enum MovemetMode
    {
        Wander = 0,
        Seek
    }

    Rigidbody2D rgb;
    Vector2 acceleration = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    MovemetMode moveMode = MovemetMode.Wander;
    Vector2 seekTarget = Vector2.zero;


	// Use this for initialization
	void Start () {
        rgb = GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        switch(moveMode)
        {
            case MovemetMode.Wander:
                Wander();
                break;

            case MovemetMode.Seek:
                Arrival();
                break;

            default:
                break;
        }
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

    void Arrival()
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

    private void LateUpdate()
    {
        transform.up = rgb.velocity.normalized;
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
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.ToLower().Contains("food"))
        {
            seekTarget = collision.transform.position;
            moveMode = MovemetMode.Seek;
        }
    }
}
