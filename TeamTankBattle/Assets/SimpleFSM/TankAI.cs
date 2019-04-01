using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static Ruleset;

public class TankAI : FSM
{
    public NavMeshAgent agent;
    private Vector3 flockingPosition;
    private Vector3 targetPosition;
    private GameObject targetTank;
    private GameObject bullet;
    private FSMState currentState;
    private SquadAI squadAI;

    private Ruleset ruleset;

    // Start is called before the first frame update
    void Start()
    {
        ruleset = new Ruleset();
        squadAI = gameObject.GetComponentInParent<SquadAI>();
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
        agent.SetDestination(targetPosition);
    }

    private Vector3 TargetPosition()
    {
        /*
        float distanceToPoint = Vector3.Distance(transform.position, targetPosition);
        float distanceToFlock = Vector3.Distance(transform.position, flockingPosition);
        float pointWeight = distanceToPoint / (distanceToPoint + distanceToFlock);
        float flockWeight = distanceToFlock / (distanceToPoint + distanceToFlock);
        Vector3 endPoint = ((targetPosition - transform.position) * pointWeight) + ((flockingPosition - transform.position) * flockWeight);
        //TODO: Middelen flockingPosition en targetTank.transform.position
        return endPoint;
        */
        return new Vector3();
    }

    private void HandleRotation(Vector3 destPos)
    {
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * ruleset.rotationSpeed);
    }

    private Vector3 Combine()
    {
        Vector3 cohesion = CalculateCohesion();
        Vector3 alignment = CalculateAlignment();
        Vector3 separation = CalculateSeparation();

        float totalMag = cohesion.magnitude + alignment.magnitude + separation.magnitude;

        Vector3 cohModified = cohesion * (cohesion.magnitude / totalMag);
        Vector3 aliModified = alignment * (alignment.magnitude / totalMag);
        Vector3 sepModified = separation * (separation.magnitude / totalMag);

        Vector3 targetPoint = cohModified + aliModified + sepModified;
        return targetPoint;
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 cohesion = new Vector3();
        int index = 0;

        foreach (GameObject tank in squadAI.ownTanks)
        {
            if (tank != null && tank != gameObject)
            {
                index++;
                cohesion += tank.transform.position;
            }
        }

        if (index == 0)
            return cohesion;

        cohesion /= index;
        cohesion.Normalize();

        return cohesion;
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separation = new Vector3();
        foreach (GameObject tank in squadAI.ownTanks)
        {
            if (tank != null && tank != gameObject)
            {
                Vector3 diff = transform.position - tank.transform.position;
                if (diff.magnitude > 0)
                    separation += (diff.normalized / (diff.magnitude * diff.magnitude));
            }
        }
        return separation;
    }

    private Vector3 CalculateAlignment()
    {
        return targetPosition - transform.position;
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
