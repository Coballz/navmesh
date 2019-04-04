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
    private Vector3 squadPosition;              //The average position of our own squad
    public List<Vector3> patrolPoints;          //The positions used for patrolling

    private FSMState currentState;

    private Ruleset ruleset;

    private float distToClosestTank = 0;        //Minimum distance for patrol state

    void Start()
    {
        currentState = FSMState.Patrol;
        InitializePatrolPoints();               //Make sure we can start patrolling right away
        InitializeRuleset();                    //Make sure all rules are enforced
        SetOwnTanks();                          //Make sure we know about our own squadron
        SetEnemyTanks();                        //Make sure we know about the enemy tanks at start
        UpdateSquadPosition();
    }

    private void Update()
    {
        CycleEnemyTanks();
    }

    //Determines what state the tanks should be set to
    private void SquadLogic(GameObject tank)
    {
        if (distToClosestTank <= ruleset.spottingRange)            
        {
            currentState = FSMState.Offense;
        }
        currentState = FSMState.Patrol;
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
    //Note: Uses the flockingPosition calculated by UpdateSquadPosition()
    private void UpdateClosestTank(GameObject enemyTank)
    {
        foreach (GameObject tank in ownTanks)
        {
            float distance = Vector3.Distance(squadPosition, enemyTank.transform.position);
            if ((tank != null && distance < distToClosestTank) || distToClosestTank == 0)
            {
                distToClosestTank = distance;
                targetTank = enemyTank;
            }
        }
    }

    //Set all tanks' state
    private void UpdateSquadState()
    {
        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
            tank.GetComponent<TankAI>().SetState(currentState);
        }
    }

    //Update the squad's central position for reference
    private void UpdateSquadPosition()
    {
        Vector3 center = new Vector3();
        int noTanks = 0;
        foreach (GameObject tank in ownTanks)
        {
            center += tank.transform.position;
            noTanks++;
        }
        squadPosition = center / noTanks;
    }

    //Update the next patrolpoint for the entire squad to chase after breaking out of Offense state
    private int GetClosestPatrolPoint()
    {
        UpdateSquadPosition();
        Vector3 target = new Vector3();
        int point = 0;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (target == new Vector3() || Vector3.Distance(squadPosition, patrolPoints[i]) < Vector3.Distance(squadPosition, target))
                point = i;
        }
        return point;
    }

    //Add all tanks of this squadron to an array
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

    //Initialize the ruleset
    protected void InitializeRuleset()
    {
        ruleset = ScriptableObject.CreateInstance<Ruleset>();
    }
}
