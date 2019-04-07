using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Team4Ruleset;
using Team4;

public class Team4TankAI : Team4FSM
{
    public NavMeshAgent agent;
    public Vector3 targetPosition;
    public float health = 100.0f;

    private GameObject targetTank;
    public GameObject Bullet;
    public FSMState currentState;
    private Team4SquadAI squadAI;
    public int currentPatrolPoint;
    private Team4Ruleset ruleset;

    // Start is called before the first frame update
    void Start()
    {
        squadAI = GetComponent<Team4SquadAI>();
        ruleset = ScriptableObject.CreateInstance("Team4Ruleset") as Team4Ruleset;
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
        targetPosition = GetPatrolPoint(currentPatrolPoint);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedTime += Time.deltaTime;
        HandleStates();
        HandleRotation();
        HandleMovement();
        HandleShooting();
    }

    private void HandleStates()
    {
        switch (currentState)
        {
            case FSMState.Patrol:
                if (Vector3.Distance(transform.position, targetPosition) < 100.0f)
                    GetNextPatrolPoint();
                break;
            case FSMState.Offense:
                if (Vector3.Distance(transform.position, targetTank.transform.position) <= ruleset.attackRange)
                    targetPosition = transform.position;
                else targetPosition = targetTank.transform.position;
                break;
            case FSMState.GTFO:
                print(this.name + " is " + Vector3.Distance(transform.position, targetPosition) + "units from GTFOpoint");
                if (Vector3.Distance(transform.position, targetPosition) <= 150.0f)
                    SetState(FSMState.Patrol);
                break;
        }
    }

    public void SetState(FSMState state)
    {
        if (state == FSMState.Dead)
        {
            Destroy(this.gameObject);
        }
        this.currentState = state;
    }

    private void HandleMovement()
    {
            agent.SetDestination(targetPosition);
    }

    private void HandleRotation()
    {
        if (Vector3.Distance(transform.position, targetTank.transform.position) <= ruleset.attackRange)
        {
            Quaternion turretRotation = Quaternion.LookRotation(targetTank.transform.position - turret.position);
            turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * ruleset.rotationSpeed);
        }
    }

    private void HandleShooting()
    {
        if (currentState == FSMState.Offense && Vector3.Distance(transform.position, targetTank.transform.position) < ruleset.attackRange)
            Shoot();
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 cohesion = new Vector3();
        int index = 0;

        foreach (GameObject tank in squadAI.ownTanks)
        {
            if (tank != null && tank.name != this.name)
            {
                index++;
                cohesion += tank.transform.position;
            }
        }
        if (index == 0)
            return cohesion;

        cohesion /= index;

        cohesion -= transform.position;
        if (cohesion.magnitude >= 150)
            return Vector3.Normalize(cohesion);
        else return new Vector3(0, 0, 0);
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separation = new Vector3();
        foreach (GameObject tank in squadAI.ownTanks)
        {
            if (tank != null && tank.name != this.name && Vector3.Distance(transform.position, tank.transform.position) < 50)
            {
                Vector3 difference = transform.position - tank.transform.position;
                separation += Vector3.Normalize(difference) / Vector3.Magnitude(difference) / Vector3.Magnitude(difference);
            }
        }
        
        separation = Vector3.Normalize(separation);
        return separation;
    }

    private Vector3 Combine()
    {
        Vector3 cohesion = CalculateCohesion();         //raw cohesion vector
        Vector3 separation = CalculateSeparation();     //raw separation vector

        Vector3 targetPoint = cohesion * ruleset.cohC + separation * ruleset.sepC + targetPosition * ruleset.aliC;
        return Vector3.Normalize(targetPoint) * 0.2f;
    }

    private void Shoot()
    {
        if (elapsedTime >= ruleset.fireRate)
        {
            //Shoot the bullet
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }

    public void SetTargetTank(GameObject targetEnemyTank)
    {
        this.targetTank = targetEnemyTank;
    }

    public void GetNextPatrolPoint()
    {
        currentPatrolPoint++;
        if (currentPatrolPoint == squadAI.patrolPoints.Count)
            currentPatrolPoint = 0;
        targetPosition = GetPatrolPoint(currentPatrolPoint);
    }

    public Vector3 GetPatrolPoint(int i)
    {
        return squadAI.patrolPoints[i];
    }

    public void SetPatrolPoint(int i)
    {
        this.targetPosition = GetPatrolPoint(i);
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            health -= col.gameObject.GetComponent<Team4Bullet>().damage;
            if (health <= 0)
                SetState(FSMState.Dead);
            else if (health <= 50)
                agent.speed /= 2;
        }
    }
}