using UnityEngine;
using System.Collections;

public class SimpleFSM : FSM
{
    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
        Dead,
        Evade,
        Flee
    }

    //Current state that the NPC is reaching
    public FSMState curState;

    //Speed of the tank
    private float curSpeed;

    //Tank Rotation Speed
    private float curRotSpeed;
    private float evadeRotSpeed;

    //Bullet
    public GameObject Bullet;

    //Whether the NPC is destroyed or not
    private bool bDead;
    private int health;
    private float evadeDistance;


    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize()
    {
        curState = FSMState.Patrol;
        curSpeed = 150.0f;
        curRotSpeed = 1.5f;
        evadeRotSpeed = 5.0f;
        bDead = false;
        elapsedTime = 0.0f;
        shootRate = 3.0f;
        health = 100;
        evadeDistance = 250f;

        //Get the list of points
        pointList = GameObject.FindGameObjectsWithTag("WandarPoint");
        tankList = GameObject.FindGameObjectsWithTag("Tank");

        //Set Random destination point first
        FindNextPoint();

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

        if (!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");

        //Get the turret of the tank
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
    }

    //Update each frame
    protected override void FSMUpdate()
    {
        switch (curState)
        {
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
            case FSMState.Flee: UpdateFleeState(); break;
            case FSMState.Evade: UpdateEvadeState(); break;
        }

        //Update the time
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
        //Check if there is a tank to be evaded
        foreach (GameObject tank in tankList)
        {
            //print("This is " + gameObject.name + " checking out " + tank.gameObject.name);
            if (ShouldEvade(tank))
            {
                print(gameObject.name + " evading " + tank.gameObject.name + ", set evasion point from " + transform.position + " to " + destPos);
                curState = FSMState.Evade;

                return;
            }
        }

        //Check the distance with player tank
        //When the distance is near, transition to chase state
        if (Vector3.Distance(transform.position, playerTransform.position) <= 300.0f)
        {
            if (health >= 50)
            {
                print("Switch to Chase Position");
                curState = FSMState.Chase;

            }
            else
            {
                print("Switch to Flee Position");

                curState = FSMState.Flee;
            }
        }
        //Find another random patrol point if the current point is reached
        else if (Vector3.Distance(transform.position, destPos) <= 75.0f)
        {
            //print("Reached the destination point\ncalculating the next point");
            //print(destPos);
            FindNextPoint();
        }

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Chase state
    /// </summary>
    protected void UpdateChaseState()
    {
        foreach (GameObject tank in tankList)
        {
            //print("This is " + gameObject.name + " checking out " + tank.gameObject.name);
            if (ShouldEvade(tank))
            {
                curState = FSMState.Evade;
                return;
            }
        }
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= 200.0f)
        {

            curState = FSMState.Attack;
        }
        //Go back to patrol is it become too far
        else if (dist >= 500.0f)
        {
            FindNextPoint();

            curState = FSMState.Patrol;
        }

        //Rotate towards the player
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Attack state
    /// </summary>
    protected void UpdateAttackState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with the player tank
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist >= 200.0f && dist < 300.0f)
        {
            //Rotate to the target point
            Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

            //Go Forward
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);


            curState = FSMState.Chase;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= 300.0f)
        {

            curState = FSMState.Patrol;
        }

        //Always Turn the turret towards the player
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed);

        //Shoot the bullets
        ShootBullet();
    }

    protected void UpdateFleeState()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) > 300.0f)
        {
            curState = FSMState.Patrol;
        }

        else if (Vector3.Angle(transform.forward, destPos) <= 30.0f)
        {
            if (Vector3.Angle(transform.forward, playerTransform.position) <= 60.0f)
                FindNextPoint();
        }

        else if (Vector3.Distance(transform.position, destPos) <= 50.0f)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > 300.0f)
            {

                curState = FSMState.Patrol;
            }
            else FindNextPoint();
        }

        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    //Evade state
    protected void UpdateEvadeState()
    {
        Vector3 targetPos = new Vector3(0, 0, 0);
        int nofTanks = 0;
        foreach (GameObject tank in tankList)
        {
            if (ShouldEvade(tank))
            {
                targetPos += tank.transform.position;
                nofTanks++;
            }
        }

        if (nofTanks == 0)
        {
            FindNextPoint();
            curState = FSMState.Patrol;
            return;
        }
        else
        {

            targetPos /= nofTanks;
            print(gameObject.name + ", " + targetPos);
            targetPos -= transform.position;
            targetPos *= -100;
            destPos = targetPos;

            if (Vector3.Distance(transform.position, destPos) > 25.0f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * evadeRotSpeed);

                transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
            }
            else
            {
                FindNextPoint();
                curState = FSMState.Patrol;
            }
        }
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
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= 300.0f)
            {
                FindNextPoint();
                curState = FSMState.Flee;
            }
        }
    }

    /// <summary>
    /// Find the next semi-random patrol point
    /// </summary>
    protected void FindNextPoint()
    {
        //print("Finding next point");
        int rndIndex = Random.Range(0, pointList.Length);
        float rndRadius = 10.0f;

        Vector3 rndPosition = Vector3.zero;
        destPos = pointList[rndIndex].transform.position + rndPosition;

        //Check Range
        //Prevent to decide the random point as the same as before
        if (IsInCurrentRange(destPos))
        {
            rndPosition = new Vector3(Random.Range(-rndRadius, rndRadius), 0.0f, Random.Range(-rndRadius, rndRadius));
            destPos = pointList[rndIndex].transform.position + rndPosition;
        }
    }

    protected bool ShouldEvade(GameObject tank)
    {
        if (tank != null && tank.gameObject.name != this.gameObject.name)
        {
            if (Vector3.Distance(tank.transform.position, transform.position) <= 200)
            {
                return true;
            }
            else return false;
        }
        else return false;
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
}