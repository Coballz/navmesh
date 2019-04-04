using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Ruleset;

public class TankAI : FSM
{
    public NavMeshAgent agent;
    public Vector3 patrolPoint;
    private GameObject targetTank;
    private GameObject bullet;
    private FSMState currentState;
    private SquadAI squadAI;
    private int currentPatrolPoint = 2;
    public float health = 100.0f;

    private Ruleset ruleset;

    // Start is called before the first frame update
    void Start()
    {
        squadAI = GetComponent<SquadAI>();
        ruleset = ScriptableObject.CreateInstance("Ruleset") as Ruleset;
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
        patrolPoint = GetPatrolPoint(currentPatrolPoint);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleRotation();
    }

    public void SetState(FSMState state)
    {
        this.currentState = state;
    }

    private void HandleMovement()
    {
        if (Vector3.Distance(transform.position, patrolPoint) < 100.0f)
            GetNextPatrolPoint();
        Vector3 dest = transform.position + Combine();
        print(this.name + "'s target position: " + dest);
        agent.SetDestination(patrolPoint);
    }

    private void HandleRotation()
    {
        Quaternion turretRotation = Quaternion.LookRotation(patrolPoint - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * ruleset.rotationSpeed);
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
        print("cohesion " + cohesion);
        return Vector3.Normalize(cohesion);
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separation = new Vector3();
        foreach (GameObject tank in squadAI.ownTanks)
        {
            if (tank != null && tank.name != this.name)
            {
                Vector3 difference = transform.position - tank.transform.position;
                separation += Vector3.Normalize(difference) / Vector3.Magnitude(difference) / Vector3.Magnitude(difference);
            }
        }
        separation = Vector3.Normalize(separation);
        return separation;
    }

    private Vector3 CalculateAlignment()
    {
        Vector3 alignment = patrolPoint - transform.position;
        alignment = Vector3.Normalize(alignment);
        return alignment;
    }

    private Vector3 Combine()
    {
        Vector3 cohesion = CalculateCohesion();         //raw cohesion vector
        Vector3 separation = CalculateSeparation();     //raw separation vector
        Vector3 alignment = CalculateAlignment();       //raw alignment vector

        print(this.name + ", cohesion: " + cohesion + ", alignment: " + alignment + ", separation: " + separation);

        Vector3 targetPoint = cohesion * ruleset.cohC + separation * ruleset.sepC + alignment * ruleset.aliC;
        return Vector3.Normalize(targetPoint);
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
        patrolPoint = GetPatrolPoint(currentPatrolPoint);
    }

    public Vector3 GetPatrolPoint(int i)
    {
        return squadAI.patrolPoints[i];
    }

    public void SetPatrolPoint(int i)
    {
        this.currentPatrolPoint = i;
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            health -= col.gameObject.GetComponent<Bullet>().damage;
            if (health <= 0)
                SetState(FSMState.Dead);
            else if (health <= 50)
                agent.speed /= 2;
        }
    }
}