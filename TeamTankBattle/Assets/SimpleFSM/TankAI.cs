using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Ruleset;

public class TankAI : FSM
{
    public NavMeshAgent agent;
    private Vector3 flockingPosition;
    public Vector3 targetPosition;
    private GameObject targetTank;
    private GameObject bullet;
    private FSMState currentState;
    private SquadAI squadAI;

    private Ruleset ruleset;

    // Start is called before the first frame update
    void Start()
    {
        squadAI = GetComponent<SquadAI>();
        ruleset = squadAI.GetComponent<Ruleset>();
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    public void SetState(FSMState state)
    {
        this.currentState = state;
    }

    private void HandleMovement()
    {
        HandleRotation(targetPosition);
        agent.speed = ruleset.moveSpeedWhenDamaged;
        Vector3 dest = transform.position + Combine();
        print(this.name + ", " + dest);
        agent.SetDestination(dest);
    }

    private void SetTargetPosition(Vector3 target)
    {
        this.targetPosition = target;
    }

    private void HandleRotation(Vector3 destPos)
    {
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * ruleset.rotationSpeed);
    }

    private Vector3 Combine()
    {
        Vector3 cohesion = CalculateCohesion();         //raw cohesion vector
        Vector3 alignment = CalculateAlignment();       //raw alignment vector
        Vector3 separation = CalculateSeparation();     //raw separation vector

        print(this.name + ", " + cohesion + ", " + alignment + ", " + separation);

        //Vector3 cohModified = cohesion * (cohesion.magnitude / totalMag);
        //Vector3 aliModified = alignment * (alignment.magnitude / totalMag);
        //Vector3 sepModified = separation * (separation.magnitude / totalMag);

        Vector3 targetPoint = cohesion + separation + alignment;
        return targetPoint.normalized;
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
        return cohesion.normalized;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separation = new Vector3();
        foreach (GameObject tank in squadAI.ownTanks)
        {
            if (tank != null && tank.name != this.name)
            {
                Vector3 difference = transform.position - tank.transform.position;
                separation += difference.normalized / (difference.sqrMagnitude);
            }
        }
        print(separation.normalized);
        return separation.normalized.normalized;
    }


    private Vector3 CalculateAlignment()
    {
        return (targetPosition - transform.position).normalized;
    }

    public void SetTargetTank(GameObject targetEnemyTank)
    {
        this.targetTank = targetEnemyTank;
    }

    public void SetFlockingPosition(Vector3 newFlockingPosition)
    {
        this.flockingPosition = newFlockingPosition;
    }
}
