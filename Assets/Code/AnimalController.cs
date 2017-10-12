using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalController : MonoBehaviour {

    public float MAX_SPEED = 10;
    public float MAX_FORCE = 7;
    public float MAX_HP = 10;
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
    Vector2 steering_force = Vector2.zero;
    ActionMode actionMode = ActionMode.Wander;
    GameObject seekTarget = null;
    GameObject attackTarget = null;
    public float hp;
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
    /// <summary>
    /// Update also acts as the controller for the animal. It chooses the action to take based on the information it has 
    /// and always chooses the highest priority action first
    /// </summary>
	void Update () {
        // Choose the action to take
        SelectActionMode();

        // Perform movement based on the action taken
        // Set steering force
        SetSteeringForceBasedOnAction();

        // Move the animal based on the steer dir
        MoveAnimal();

        sprite.color = Color.Lerp(fatalColor, startColor, hp / MAX_HP);
    }

    /// <summary>
    /// Selects the current action mode for the animal based on priority. Returns highest priority first.
    /// </summary>
    void SelectActionMode()
    {
        // Check if health is in fatal condition
        if (healthCond == HealthCondition.Fatal)
        {
            if (seekTarget && attackTarget)
            {
                //if (actionMode == ActionMode.Flee)
                //    return;

                //Vector2 awayVec = (transform.position - seekTarget.transform.position);
                //transform.up = awayVec.normalized;

                actionMode = ActionMode.Flee;
                return;
            }
        }

        // Check if an attack target exists. (Remember: An attack target will only exist if there is also food around OR if someone is attacking this animal)
        if (attackTarget)
        {
            if (seekTarget)
            {
                Vector2 distanceFromFood = seekTarget.transform.position - transform.position;

                if (distanceFromFood.magnitude < 1.5f)
                {
                    actionMode = ActionMode.Attack;
                    return;
                }
                else
                {
                    attackTarget = null;
                }
            }
            else
            {
                actionMode = ActionMode.Attack;
                return;
            }
        }

        // Check if food is around but there is no enemy around.
        if (seekTarget && attackTarget == null)
        {
            actionMode = ActionMode.Seek;
            return;
        }
        
        // Check if no food is around or any enemy is around
        if (seekTarget == null && attackTarget == null)
        {
            actionMode = ActionMode.Wander;
        }
    }

    /// <summary>
    /// Sets the steer direction based on the action selected.
    /// </summary>
    void SetSteeringForceBasedOnAction()
    {
        Vector2 steer = Vector2.zero;

        switch (actionMode)
        {
            case ActionMode.Wander:
                steer = Wander();
                break;

            case ActionMode.Seek:
                if (seekTarget)
                    steer = Arrival(seekTarget.transform.position);
                break;

            case ActionMode.Attack:
                if (attackTarget)
                    steer = Arrival(attackTarget.transform.position);
                break;

            case ActionMode.Flee:
                steer = Flee();
                break;

            default:
                break;
        }

        steering_force = steer;
    }

    /// <summary>
    /// Moves the actual rigidbody using the steering_force
    /// </summary>
    void MoveAnimal()
    {
        if (steering_force.magnitude > MAX_FORCE)
            steering_force = steering_force.normalized * MAX_FORCE;

        acceleration = steering_force / rgb.mass;

        velocity += acceleration;

        if (velocity.magnitude > MAX_SPEED)
            velocity = velocity.normalized * MAX_SPEED;

        rgb.velocity = velocity;
        transform.up = rgb.velocity.normalized;
    }

    /// <summary>
    /// Uses the current transform.up and adds a tiny bit of sway to it.
    /// </summary>
    /// <returns>Steering force as a vector2</returns>
    Vector2 Wander()
    {
        Vector2 sway = transform.right * Random.Range(-2.0f, 2.0f);
        Vector2 steer = (Vector2)transform.up * Random.Range(4.0f, 6.0f) + sway;
        return steer;
        //if (steering_force.magnitude > MAX_FORCE)
        //    steering_force = steering_force.normalized * MAX_FORCE;

        //acceleration = steering_force / rgb.mass;

        //velocity += acceleration;

        //if (velocity.magnitude > MAX_SPEED)
        //    velocity = velocity.normalized * MAX_SPEED;

        //rgb.velocity = velocity;
    }

    /// <summary>
    /// Uses the Arrival behaviour to move to a target location and slow down as it reaches the location.
    /// </summary>
    /// <returns>Steering force as a vector2</returns>
    Vector2 Arrival(Vector2 seekTarget)
    {
        Vector2 target_offset = seekTarget - (Vector2)transform.position;
        float distance = target_offset.magnitude;
        float ramped_speed = MAX_SPEED * (distance / 1.5f);
        float clipped_speed = Mathf.Min(ramped_speed, MAX_SPEED);

        Vector2 desired_velocity = (clipped_speed / distance) * target_offset;

        Vector2 steer = desired_velocity - velocity;
        return steer;
        //if (steering_force.magnitude > MAX_FORCE)
        //    steering_force = steering_force.normalized * MAX_FORCE;

        //acceleration = steering_force / rgb.mass;

        //velocity += acceleration;

        //if (velocity.magnitude > MAX_SPEED)
        //    velocity = velocity.normalized * MAX_SPEED;

        //rgb.velocity = velocity;
    }

    /// <summary>
    /// Moves away from the food target.
    /// </summary>
    /// <returns>Steering force as a vector2</returns>
    Vector2 Flee()
    {
        Vector2 awayVec = (transform.position - seekTarget.transform.position);
        transform.up = awayVec.normalized;

        Vector2 desired_velocity = transform.up * MAX_SPEED;

        Vector2 steer = desired_velocity - velocity;
        return steer;
        //if (steering_force.magnitude > MAX_FORCE)
        //    steering_force = steering_force.normalized * MAX_FORCE;

        //acceleration = steering_force / rgb.mass;

        //velocity += acceleration;

        //if (velocity.magnitude > MAX_SPEED)
        //    velocity = velocity.normalized * MAX_SPEED;

        //rgb.velocity = velocity;
    }
    
    /// <summary>
    /// Takes random amounts of damage and sets the enemy gamobject as the current enemy
    /// </summary>
    /// <param name="enemy"></param>
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

    /// <summary>
    /// Consume the food. This function is called when the animal is touching the food
    /// </summary>
    /// <param name="val"></param>
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

    #region Sensory System

    /// <summary>
    /// Checks collisions with the wall, animal and food
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.tag.ToLower().Contains("wall"))
        {
            Vector2 collisionVector = collision.contacts[0].point - (Vector2)transform.position;

            transform.up = -collisionVector.normalized;
        }

        if (collision.gameObject.tag.ToLower().Contains("animal") && attackTarget != null)
        {
            if (collision.gameObject == attackTarget && actionMode == ActionMode.Attack)
            {
                attackTarget.GetComponent<AnimalController>().TakeDamage(this);
            }
        }

        if (collision.gameObject.tag.ToLower().Contains("food") && seekTarget != null)
        {
            EatFood(collision.gameObject.GetComponent<Food>().TrasferHPToAnimal());
        }
    }

    /// <summary>
    /// Sense nearby food and enemy if food is present.
    /// </summary>
    /// <param name="collider"></param>
    private void OnTriggerStay2D(Collider2D collider)
    {
        if (collider.gameObject.tag.ToLower().Contains("food") && seekTarget == null)
        {
            seekTarget = collider.gameObject;
        }

        if (collider.gameObject.tag.ToLower().Contains("animal") && seekTarget != null && attackTarget == null)
        {
            if (!collider.isTrigger)
            {
                Vector2 distanceVec = transform.position - collider.gameObject.transform.position;

                if (distanceVec.magnitude < 2)
                    attackTarget = collider.gameObject;
            }
        }
    }

    /// <summary>
    /// Detects when the enemy or food has left the sensory radius
    /// </summary>
    /// <param name="collider"></param>
    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag.ToLower().Contains("food"))
        {
            if (collider.gameObject == seekTarget && healthCond == HealthCondition.Fatal)
            {
                seekTarget = null;
                if (attackTarget)
                    attackTarget = null;
            }
        }

        if (collider.gameObject.tag.ToLower().Contains("animal"))
        {
            if (collider.gameObject == attackTarget && !collider.isTrigger)
            {
                attackTarget = null;
            }
        }
    }

    #endregion
}
