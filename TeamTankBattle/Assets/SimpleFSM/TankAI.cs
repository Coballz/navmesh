using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TankAI : FSM
{
    private Vector3 flockingPosition;
    private Vector3 targetPosition;
    private GameObject targetTank;
    private GameObject bullet;
    public NavMeshAgent agent;
    private FSMState currentState;
    private SquadAI squadAI;

    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
        Dead,
        Flee
    }

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
        Vector3 targetPosition = TargetPosition();
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

    public void SetTargetTank(GameObject targetEnemyTank)
    {
        this.targetTank = targetEnemyTank;
    }

    public void SetFlockingPosition(Vector3 newFlockingPosition)
    {
        this.flockingPosition = newFlockingPosition;
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
        return new Vector3();
    }

    private Vector3 CalculateAlignment()
    {
        
        return new Vector3();
    }

    private void Combine()
    {
        Vector3 cohesion = CalculateCohesion();
        Vector3 alignemnt = CalculateAlignment();
        Vector3 separation = CalculateSeparation();

    }
}
