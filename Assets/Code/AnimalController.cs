using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour {

    public float MAX_SPEED = 10;
    public float MAX_FORCE = 7;
    public float MAX_HP = 10;
    public float FleeTime = 5;
    public Color fatalColor;

    public enum ActionMode
    {
        Wander = 0,
        Seek,
        Attack,
        Flee
    }

    enum HealthCondition
    {
        Healthy = 0,
        Fatal
    }

    Rigidbody2D rgb;
    SpriteRenderer sprite;
    Color startColor;
    Vector2 acceleration = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    ActionMode actionMode = ActionMode.Wander;
    GameObject seekTarget = null;
    GameObject attackTarget = null;
    public float hp;
    float fleeStartTime = 0;
    HealthCondition healthCond = HealthCondition.Healthy;

    public ActionMode CurrentActionMode { get { return actionMode; } }
    
	// Use this for initialization
	void Start () {
        rgb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        hp = MAX_HP;
        startColor = sprite.color;
	}
	
	// Update is called once per frame
	void Update () {
        SelectActionMode();

        switch (actionMode)
        {
            case ActionMode.Wander:
                Wander();
                break;

            case ActionMode.Seek:
                if (seekTarget)
                    Arrival(seekTarget.transform.position);
                break;

            case ActionMode.Attack:
                //if (CheckForFight())
                //    Arrival(attackTarget.transform.position);
                //else
                //    moveMode = MovemetMode.Seek;
                if (attackTarget)
                    Arrival(attackTarget.transform.position);
                break;

            case ActionMode.Flee:
                //if (seekTarget)
                //    Flee(seekTarget.transform.position);
                Flee();
                break;

            default:
                break;
        }

        transform.up = rgb.velocity.normalized;

        sprite.color = Color.Lerp(fatalColor, startColor, hp / MAX_HP);
    }

    void SelectActionMode()
    {
        if (healthCond == HealthCondition.Fatal)
        {
            if (seekTarget && attackTarget)
            {
                if (actionMode == ActionMode.Flee)
                    return;

                Vector2 awayVec = (transform.position - seekTarget.transform.position);
                transform.up = awayVec.normalized;

                actionMode = ActionMode.Flee;
                return;
            }
        }

        if (attackTarget)
        {
            if (attackTarget.GetComponent<AnimalController>().CurrentActionMode != ActionMode.Flee)
            {
                actionMode = ActionMode.Attack;
                return;
            }
        }

        if (seekTarget && attackTarget == null)
        {
            actionMode = ActionMode.Seek;
            return;
        }
        
        if (seekTarget == null && attackTarget == null)
        {
            actionMode = ActionMode.Wander;
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

    void Flee()
    {
        Vector2 desired_velocity = transform.up * MAX_SPEED;

        Vector2 steering_force = desired_velocity - velocity;

        if (steering_force.magnitude > MAX_FORCE)
            steering_force = steering_force.normalized * MAX_FORCE;

        acceleration = steering_force / rgb.mass;

        velocity += acceleration;

        if (velocity.magnitude > MAX_SPEED)
            velocity = velocity.normalized * MAX_SPEED;

        rgb.velocity = velocity;
    }

    //bool CheckForFight()
    //{
    //    if (attackTarget == null)
    //        return false;

    //    AnimalController targetController = attackTarget.GetComponent<AnimalController>();

    //    if (targetController.CurrentActionMode == ActionMode.Flee)
    //        return false;

    //    return true;
    //}

    void TakeDamage(AnimalController enemy)
    {
        attackTarget = enemy.gameObject;
        float val = Random.Range(1, 3);
        hp -= val;

        if (hp < 0.2f * MAX_HP)
        {
            healthCond = HealthCondition.Fatal;
        }

        if (hp < 0)
        {
            hp = 0;
            Destroy(gameObject);
        }
    }

    void EatFood(float val)
    {
        hp += val;

        if (hp > 0.7f * MAX_HP)
        {
            healthCond = HealthCondition.Healthy;
        }

        if (hp > MAX_HP)
            hp = MAX_HP;
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

        if (collision.gameObject.name.ToLower().Contains("animal") && attackTarget != null)
        {
            if (collision.gameObject == attackTarget && actionMode == ActionMode.Attack)
            {
                attackTarget.GetComponent<AnimalController>().TakeDamage(this);
            }
        }

        if (collision.gameObject.name.ToLower().Contains("food") && seekTarget != null)
        {
            EatFood(collision.gameObject.GetComponent<Food>().TrasferHPToAnimal());
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.name.ToLower().Contains("food") && seekTarget == null)
        {
            seekTarget = collider.gameObject;
        }

        if (collider.gameObject.name.ToLower().Contains("animal") && seekTarget != null)
        {
            if (!collider.isTrigger)
            {
                Vector2 distanceVec = transform.position - collider.gameObject.transform.position;

                if (hp <= 0.2f * MAX_HP)
                    attackTarget = collider.gameObject;
                else if (distanceVec.magnitude < 2)
                    attackTarget = collider.gameObject;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.name.ToLower().Contains("food") && seekTarget == null)
        {
            seekTarget = collider.gameObject;
        }

        if (collider.gameObject.name.ToLower().Contains("animal") && seekTarget != null)
        {
            if (!collider.isTrigger)
            {
                Vector2 distanceVec = transform.position - collider.gameObject.transform.position;

                if (distanceVec.magnitude < 2)
                    attackTarget = collider.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.name.ToLower().Contains("food"))
        {
            if (collider.gameObject == seekTarget)
            {
                seekTarget = null;
                if (attackTarget)
                    attackTarget = null;
            }
        }

        if (collider.gameObject.name.ToLower().Contains("animal"))
        {
            if (collider.gameObject == attackTarget && !collider.isTrigger)
            {
                attackTarget = null;
            }
        }
    }
}
