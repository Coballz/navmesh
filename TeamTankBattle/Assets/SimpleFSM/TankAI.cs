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
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    void SetState(FSMState state)
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
        float distanceToPoint = Vector3.Distance(transform.position, targetPosition);
        float distanceToFlock = Vector3.Distance(transform.position, flockingPosition);
        float pointWeight = distanceToPoint / (distanceToPoint + distanceToFlock);
        float flockWeight = distanceToFlock / (distanceToPoint + distanceToFlock);
        Vector3 endPoint = ((targetPosition - transform.position) * pointWeight) + ((flockingPosition - transform.position) * flockWeight);
        //TODO: Middelen flockingPosition en targetTank.transform.position
        return endPoint;
    }

    private void HandleRotation(Vector3 destPos)
    {
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * ruleset.rotationSpeed);
    }
}
