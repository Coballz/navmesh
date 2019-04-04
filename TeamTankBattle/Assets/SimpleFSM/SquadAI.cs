using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Ruleset;

public class SquadAI : MonoBehaviour
{
    public GameObject[] ownTanks;              //List with our own tanks

    public GameObject[] squadOneTanks;         //enemy team one
    public GameObject[] squadTwoTanks;         //enemy team two
    public GameObject[] squadThreeTanks;       //enemy team three

    public GameObject targetTank;              //The tank to be used for the state logic
    public Vector3 squadPosition;              //The average position of our own squad
    public List<Vector3> patrolPoints;          //The positions used for patrolling

    public FSMState currentState;

    private Ruleset ruleset;

    private float distToClosestTank = 0;        //Minimum distance for patrol state
    private int noNearbyTanks = 0;

    void Start()
    {
        currentState = FSMState.Patrol;
        InitializePatrolPoints();               //Make sure we can start patrolling right away
        InitializeRuleset();                    //Make sure all rules are enforced
        SetOwnTanks();                          //Make sure we know about our own squadron
        SetEnemyTanks();                        //Make sure we know about the enemy tanks at start
    }

    private void Update()
    {
        UpdateSquadPosition();
        CycleEnemyTanks();
        UpdateNearbyTanks();
        SquadLogic();
        UpdateSquadTarget();
        UpdateSquadState();
    }

    //Determines what state the tanks should be set to
    private void SquadLogic()
    {
        if (currentState != FSMState.GTFO)
        {
            if (noNearbyTanks >= NumberOfAliveTanks())
            {
                SetFurthestPatrolPoint();
                currentState = FSMState.GTFO;     
                return;
            }
            if (Vector3.Distance(squadPosition, targetTank.transform.position) <= ruleset.spottingRange)
            {
                currentState = FSMState.Offense;
                return;
            }
            currentState = FSMState.Patrol;
        }
        else if (noNearbyTanks == 0)
        {
            SetPrevPatrolPoint();
            currentState = FSMState.Patrol;
        }
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
            if (((tank != null && distance < distToClosestTank) && distance <= ruleset.spottingRange) || distToClosestTank == 0)
            {
                targetTank = enemyTank;
                distToClosestTank = distance;
            }
        }
    }

    //Returns the number of living tanks in our squadron
    private int NumberOfAliveTanks()
    {
        int value = 0;
        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
                value++;
        }
        return value;
    }

    //Updates the number of nearby tanks
    private void UpdateNearbyTanks()
    {
        int i = 0;
        foreach (GameObject tank in squadOneTanks)
        {
            if (Vector3.Distance(squadPosition, tank.transform.position) <= ruleset.GTFORange)
                i++;
        }
        foreach (GameObject tank in squadTwoTanks)
        {
            if (Vector3.Distance(squadPosition, tank.transform.position) <= ruleset.GTFORange)
                i++;
        }
        foreach (GameObject tank in squadThreeTanks)
        {
            if (Vector3.Distance(squadPosition, tank.transform.position) <= ruleset.GTFORange)
                i++;
        }
        noNearbyTanks = i;
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

    //Set the squad's targeted tank
    private void UpdateSquadTarget()
    {
        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
                tank.GetComponent<TankAI>().SetTargetTank(targetTank);
        }
    }

    //Update the squad's central position for reference
    private void UpdateSquadPosition()
    {
        Vector3 center = new Vector3();
        int noTanks = 0;
        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
            center += tank.transform.position;
            noTanks++;
        }
        squadPosition = center / noTanks;
    }

    //Update the squad's patrol point in case of state switching
    private void UpdateSquadPatrolPoint(int i)
    {
        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
                tank.GetComponent<TankAI>().SetPatrolPoint(i);
        }
    }

    //Update the next patrolpoint for the entire squad to chase after breaking out of Offense state
    private void SetClosestPatrolPoint()
    {
        UpdateSquadPosition();
        Vector3 target = new Vector3();
        int point = 0;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (target == new Vector3() || Vector3.Distance(squadPosition, patrolPoints[i]) < Vector3.Distance(squadPosition, target))
            {
                point = i;
                target = patrolPoints[i];
            }
        }
        UpdateSquadPatrolPoint(point);
    }

    //Update the next patrolpoint for the entire squad to chase after running into GTFO state
    private void SetFurthestPatrolPoint()
    {
        UpdateSquadPosition();
        Vector3 target = new Vector3();
        int point = 0;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (target == new Vector3() || Vector3.Distance(squadPosition, patrolPoints[i]) > Vector3.Distance(squadPosition, target))
            {
                point = i;
                target = patrolPoints[i];
            }
        }
        UpdateSquadPatrolPoint(point);
    }

    //Sets previous patrol point after breaking out of GTFO state
    private void SetPrevPatrolPoint()
    {
        foreach (GameObject tank in ownTanks)
        {
            if (tank != null)
                tank.GetComponent<TankAI>().SetPatrolPoint(tank.GetComponent<TankAI>().currentPatrolPoint);
        }
    }

    //Add all tanks of this squadron to an array
    protected void SetOwnTanks()
    {
        ownTanks = GameObject.FindGameObjectsWithTag("Team4");
    }

    //Gets all enemy tanks and adds them to the GameObject lists
    protected void SetEnemyTanks()
    {
        squadOneTanks = GameObject.FindGameObjectsWithTag("Team1");
        squadOneTanks = GameObject.FindGameObjectsWithTag("Team2");
        squadOneTanks = GameObject.FindGameObjectsWithTag("Team3");
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
