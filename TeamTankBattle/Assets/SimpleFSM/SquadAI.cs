using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquadAI : MonoBehaviour
{
    public GameObject[] ownTanks;              //List with our own tanks

    private GameObject[] squadOneTanks;         //enemy team one
    private GameObject[] squadTwoTanks;         //enemy team two
    private GameObject[] squadThreeTanks;       //enemy team three

    private GameObject targetTank;              //The tank to be used for the state logic
    private Vector3 flockingPosition;           //The average position of our own squad
    private List<Vector3> patrolPoints;         //The positions used for patrolling

    private float distToClosestTank = 300.0f;   //Minimum distance for patrol state

    private enum FSMState                       //States to put the individual tanks in
    {   
        None,
        Patrol,
        Chase,
        Attack,
        Dead,
        FocusFire
    }

    void Initialize()
    {
        UpdateEnemySquads();                    //Make sure we know abut the enemy tanks at start
        InitializePatrolPoints();               //Make sure we can start patrolling right away
    }

    private void Update()
    {
        GameObject closestTank = GetClosestEnemyTank(); //Get the closest enemy tank
        if (closestTank != null)                        //If it's still in game
        {
            FSMState targetState = SquadLogic();             //Determine what states our squad should be in
            UpdateFlockingPosition();
            foreach (GameObject tank in ownTanks)       //Then for each of our own tanks:
            {
                TankAI tankScript = tank.GetComponent<TankAI>();
                tankScript.SetState(targetState);                 //Update the state
                tankScript.SetTargetTank(closestTank);            //Update the target enemy tank
                tankScript.SetFlockingPosition(flockingPosition); //Make sure it knows the average position of our squad
            }
        }
    }

    //Determines what state the tanks should be set to
    private FSMState SquadLogic()
    {
        if (distToClosestTank < 300)            //MAGIC NUMBER VERWIJDEREN
            return FSMState.Attack;
        return FSMState.Patrol;
    }

    //Loops through all enemy tanks and updates the targetTank if it finds one that is closer
    private GameObject GetClosestEnemyTank()
    {
        foreach (GameObject tank in squadOneTanks)
        {
            UpdateClosestTank(tank);
        }

        foreach (GameObject tank in squadTwoTanks)
        {
            UpdateClosestTank(tank);
        }

        foreach (GameObject tank in squadThreeTanks)
        {
            UpdateClosestTank(tank);
        }

        if (targetTank != null)
        {
            return targetTank;
        }
        return null;
    }

    //Updates the average position of our own squad
    private void UpdateFlockingPosition()
    {
        Vector3 totalPosition = new Vector3();
        int nTanks = 0;

        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
            {
                nTanks++;
                totalPosition += tank.transform.position;
            }
        }

        if (nTanks >= 1)
            flockingPosition = totalPosition / nTanks;
    }

    //Checks whether the tank is closer than the current targetTank
    //Note: Uses the flockingPosition calculated by UpdateFlockingPosition()
    private void UpdateClosestTank(GameObject enemyTank)
    {
        foreach (GameObject tank in ownTanks)
        {
            float distance = Vector3.Distance(flockingPosition, enemyTank.transform.position);
            if (tank != null && distance < distToClosestTank)
            {
                distToClosestTank = distance;
                targetTank = enemyTank;
            }
        }
    }

    //Gets all enemy tanks and adds them to the GameObject lists
    void UpdateEnemySquads()
    {
        squadOneTanks = GameObject.FindGameObjectsWithTag("squadOneTank");
        squadOneTanks = GameObject.FindGameObjectsWithTag("squadTwoTank");
        squadOneTanks = GameObject.FindGameObjectsWithTag("squadThreeTank");
    }

    //Initialize a route to patrol
    protected void InitializePatrolPoints()
    {
        patrolPoints = new List<Vector3>();

        patrolPoints.Add(new Vector3(2450, 100, 2450));
        patrolPoints.Add(new Vector3(2125, 100, 1905));
        patrolPoints.Add(new Vector3(1520, 100, 2270));
        patrolPoints.Add(new Vector3(725, 100, 2320));
        patrolPoints.Add(new Vector3(570, 100, 1420));
        patrolPoints.Add(new Vector3(740, 100, 880));
        patrolPoints.Add(new Vector3(1440, 100, 890));
        patrolPoints.Add(new Vector3(1950, 100, 660));
        patrolPoints.Add(new Vector3(2330, 100, 820));
        patrolPoints.Add(new Vector3(1960, 100, 1180));

        //Return along the same patrol
        patrolPoints.Add(new Vector3(1440, 100, 890));
        patrolPoints.Add(new Vector3(740, 100, 880));
        patrolPoints.Add(new Vector3(570, 100, 1420));
        patrolPoints.Add(new Vector3(725, 100, 2320));
        patrolPoints.Add(new Vector3(1520, 100, 2270));
        patrolPoints.Add(new Vector3(2125, 100, 1905));
    }
}
