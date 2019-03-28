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
        HandleRotation();
    }

    void SetState(FSMState state)
    {
        this.currentState = state;
    }

    private void HandleMovement()
    {
        Vector3 targetPosition = TargetPosition();
    }

    private Vector3 TargetPosition()
    {
        //TODO: Middelen flockingPosition en targetTank.transform.position
        return Vector3.zero;
    }

    private void HandleRotation()
    {
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * ruleset.rotationSpeed);
    }
}
