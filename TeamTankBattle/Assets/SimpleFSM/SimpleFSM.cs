using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System.Collections.Generic;

public class SimpleFSM : FSM
{
    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
        Dead,
        Flee
    }

    //Current state that the NPC is reaching
    public FSMState curState;

    //Speed of the tank
    private float curSpeed;

    //Tank Rotation Speed
    private float curRotSpeed;

    //Bullet
    public GameObject Bullet;

    //Whether the NPC is destroyed or not
    private bool bDead;
    private int health;

    public NavMeshAgent agent;
    private int currentPoint;

    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize()
    {
        curState = FSMState.Patrol;
        curSpeed = 150.0f;
        curRotSpeed = 1.5f;
        bDead = false;
        elapsedTime = 0.0f;
        shootRate = 3.0f;
        health = 100;

        //Set Random destination point first
        FindNextPoint();

        //Get the turret of the tank
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;

        //Set the reference to the agent component
    }

    //Update each frame
    protected override void FSMUpdate()
    {
        //Switch case for FSM
        switch (curState)
        {
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
            case FSMState.Flee: UpdateFleeState(); break;
        }

        //Update the time, used for shooting
        elapsedTime += Time.deltaTime;

        //Go to dead state is no health left
        if (health <= 0)
            curState = FSMState.Dead;
    }

    /// <summary>
    /// Patrol state
    /// </summary>
    protected void UpdatePatrolState()
    {
        //Check the distance with player tank
        //When the distance is near, transition to chase state
        //if (Vector3.Distance(transform.position, targetPos) <= 300.0f)
        //{
        //    if (health >= 50)
        //        curState = FSMState.Chase;
        //    else
        //        curState = FSMState.Flee;
        //}

        //Find another random patrol point if the current point is reached
        //if (Vector3.Distance(transform.position, destPos) <= 75.0f)
        //    FindNextPoint();

        MoveTank();
    }

    /// <summary>
    /// Chase state
    /// </summary>
    protected void UpdateChaseState()
    {       
        //TODO: Set the target position as the target squadron's flock position
        destPos = targetPos;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(transform.position, destPos);
        if (dist <= 200.0f)
            curState = FSMState.Attack;
        
        //Go back to patrol if the target is too far away
        else if (dist >= 500.0f)
        {
            FindNextPoint();
            curState = FSMState.Patrol;
        }

        //Move the tank
        MoveTank();
    }

    /// <summary>
    /// Attack state
    /// </summary>
    protected void UpdateAttackState()
    {
        //Set the target position as the player position
        //TODO: Set target position to center of target squadron

        //Check the distance with the player tank
        //TODO: calc distance to center of target squadron
        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist >= 200.0f && dist < 300.0f)
        {
            //Move the tank and move to chaseState
            MoveTank();
            curState = FSMState.Chase;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= 300.0f)
            curState = FSMState.Patrol;

        //TODO: Target selection
        //TODO: Target selection

        //Rotate the turret towards the target
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed);

        //Shoot the bullets
        ShootBullet();
    }

    protected void UpdateFleeState()
    {
        //TODO: Calculate distance to squadron to flee from
        if (Vector3.Distance(transform.position, targetPos) > 300.0f)
            curState = FSMState.Patrol;

        else if (Vector3.Angle(transform.forward, destPos) <= 30.0f)
        {
            if (Vector3.Angle(transform.forward, targetPos) <= 60.0f)
                FindNextPoint();
        }

        else if (Vector3.Distance(transform.position, destPos) <= 50.0f)
        {
            if (Vector3.Distance(transform.position, targetPos) > 300.0f)
                curState = FSMState.Patrol;
            else FindNextPoint();
        }

        MoveTank();
    }

    protected void FindNextPoint()
    {
        
    }

    /// <summary>
    /// Dead state
    /// </summary>
    protected void UpdateDeadState()
    {
        //Show the dead animation with some physics effects
        if (!bDead)
        {
            bDead = true;
            Explode();
        }
    }

    /// <summary>
    /// Shoot the bullet from the turret
    /// </summary>
    private void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            //Shoot the bullet
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }

    /// <summary>
    /// Check the collision with the bullet
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "Bullet")
        {
            health -= collision.gameObject.GetComponent<Bullet>().damage;
        }
    }

    /// <summary>
    /// Check whether the next random position is the same as current tank position
    /// </summary>
    /// <param name="pos">position to check</param>
    protected bool IsInCurrentRange(Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - transform.position.x);
        float zPos = Mathf.Abs(pos.z - transform.position.z);

        if (xPos <= 50 && zPos <= 50)
            return true;

        return false;
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);
    }

    protected void MoveTank()
    {
        //Acquire target rotation for tank movement
        //Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        //transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        //transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        agent.SetDestination(target);
    }
}