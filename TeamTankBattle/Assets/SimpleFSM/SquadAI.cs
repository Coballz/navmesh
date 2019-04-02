using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ruleset;

public class SquadAI : MonoBehaviour
{
    public GameObject[] ownTanks;              //List with our own tanks

    private GameObject[] squadOneTanks;         //enemy team one
    private GameObject[] squadTwoTanks;         //enemy team two
    private GameObject[] squadThreeTanks;       //enemy team three

    private GameObject targetTank;              //The tank to be used for the state logic
    private Vector3 flockingPosition;           //The average position of our own squad
    private List<Vector3> patrolPoints;         //The positions used for patrolling

    private Ruleset ruleset;

    private float distToClosestTank = 0;   //Minimum distance for patrol state

    void Start()
    {
        SetOwnTanks();
        SetEnemyTanks();                        //Make sure we know abut the enemy tanks at start
        InitializePatrolPoints();               //Make sure we can start patrolling right away
        InitializeRuleset();                    //Make sure all rules are enforced
        CycleEnemyTanks();                      //Determine whether there's a tank to chase
    }

    private void Update()
    {
        //CycleEnemyTanks(); //Get the closest enemy tank
        //if (targetTank != null)                        //Make sure it's not a reference to a dead tank
        //{
        //    FSMState targetState = SquadLogic(targetTank);             //Determine what states our squad should be in
        //    foreach (GameObject tank in ownTanks)                        //Then for each of our own tanks:
        //    {
        //        TankAI tankScript = tank.GetComponent<TankAI>();        //Retrieve their tankAI
        //        tankScript.SetState(targetState);                       //Update the state
        //        tankScript.SetTargetTank(targetTank);                  //Update the target enemy tank
        //    }
        //}
    }

    //Determines what state the tanks should be set to
    private FSMState SquadLogic(GameObject tank)
    {
        if (distToClosestTank <= ruleset.spottingRange)            //MAGIC NUMBER VERWIJDEREN
        {
            if (distToClosestTank <= ruleset.attackRange)
                return FSMState.Attack;
            return FSMState.Chase;
        }
        return FSMState.Patrol;
    }

    //Loops through all enemy tanks and updates the targetTank if it finds one that is closer
    private void CycleEnemyTanks()
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
    }

    //Checks whether the tank is closer than the current targetTank
    //Note: Uses the flockingPosition calculated by UpdateFlockingPosition()
    private void UpdateClosestTank(GameObject enemyTank)
    {
        foreach (GameObject tank in ownTanks)
        {
            float distance = Vector3.Distance(flockingPosition, enemyTank.transform.position);
            if ((tank != null && distance < distToClosestTank) || distToClosestTank == 0)
            {
                distToClosestTank = distance;
                targetTank = enemyTank;
            }
        }
    }

    protected void SetOwnTanks()
    {
        ownTanks = GameObject.FindGameObjectsWithTag("Team4");
    }

    //Gets all enemy tanks and adds them to the GameObject lists
    protected void SetEnemyTanks()
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

        //Return along the same patrolpoints
        patrolPoints.Add(new Vector3(1440, 100, 890));
        patrolPoints.Add(new Vector3(740, 100, 880));
        patrolPoints.Add(new Vector3(570, 100, 1420));
        patrolPoints.Add(new Vector3(725, 100, 2320));
        patrolPoints.Add(new Vector3(1520, 100, 2270));
        patrolPoints.Add(new Vector3(2125, 100, 1905));
    }

    protected void InitializeRuleset()
    {
        ruleset = ScriptableObject.CreateInstance<Ruleset>();
    }
}
