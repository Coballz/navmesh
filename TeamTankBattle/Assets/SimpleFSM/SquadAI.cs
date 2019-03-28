//using System.Collections.Generic;
//using UnityEngine;

//public class SquadAI : MonoBehaviour
//{
//    private GameObject[] ownTanks;              //List with our own tanks

//    private GameObject[] squadOneTanks;         //enemy team one
//    private GameObject[] squadTwoTanks;         //enemy team two
//    private GameObject[] squadThreeTanks;       //enemy team three

//    private GameObject targetTank;              //The tank to be used for the state logic
//    private Vector3 flockingPosition;           //The average position of our own squad
//    private List<Vector3> patrolPoints;         //The positions used for patrolling

//    private float distToClosestTank = 300.0f;   //Minimum distance for patrol state

//    private enum FSMState                        //States to put the individual tanks in
//    {   
//        None,
//        Patrol,
//        Chase,
//        Attack,
//        Dead,
//        Flee
//    }

//    void Initialize()
//    {
//        UpdateEnemySquads();                    //Make sure we know abut the enemy tanks at start
//        InitializePatrolPoints();               //Make sure we can start patrolling right away
//        SetSquadStates();
//    }

//    private void Update()
//    {
//        GameObject closestTank = GetClosestEnemyTank();
//        if (closestTank != null)
//        {
//            foreach (GameObject tank in ownTanks)
//            {
//                SetSquadStates(Chase);
//            }
//        }
//    }

//    private void SetTanksState(FSMState state)
//    {
//        foreach (GameObject tank in ownTanks)
//        {
//            tank.SetState(state);
//        }
//    }

//    private GameObject GetClosestEnemyTank()
//    {
//        foreach (GameObject tank in squadOneTanks)
//        {
//            UpdateClosestTank(tank);
//        }

//        foreach (GameObject tank in squadTwoTanks)
//        {
//            UpdateClosestTank(tank);
//        }

//        foreach (GameObject tank in squadThreeTanks)
//        {
//            UpdateClosestTank(tank);
//        }

//        if (targetTank != null)
//        {
//            return targetTank;
//        }
//        return null;
//    }

//    private void UpdateClosestTank(GameObject tank)
//    {
//        foreach (GameObject tank in ownTanks)
//        {
//            float distance = Vector3.Distance(flockingPosition, tank.transform.position);
//            if (tank != null && distance < distToClosestTank)
//            {
//                distToClosestTank = distance;
//                targetTank = tank;
//            }
//        }
//    }

//    void UpdateEnemySquads()
//    {
//        squadOneTanks = GameObject.FindGameObjectsWithTag("squadOneTank");
//        squadOneTanks = GameObject.FindGameObjectsWithTag("squadTwoTank");
//        squadOneTanks = GameObject.FindGameObjectsWithTag("squadThreeTank");
//    }

//    protected void InitializePatrolPoints()
//    {
//        patrolPoints = new List<Vector3>();

//        patrolPoints.Add(new Vector3(2450, 100, 2450));
//        patrolPoints.Add(new Vector3(2125, 100, 1905));
//        patrolPoints.Add(new Vector3(1520, 100, 2270));
//        patrolPoints.Add(new Vector3(725, 100, 2320));
//        patrolPoints.Add(new Vector3(570, 100, 1420));
//        patrolPoints.Add(new Vector3(740, 100, 880));
//        patrolPoints.Add(new Vector3(1440, 100, 890));
//        patrolPoints.Add(new Vector3(1950, 100, 660));
//        patrolPoints.Add(new Vector3(2330, 100, 820));
//        patrolPoints.Add(new Vector3(1960, 100, 1180));

//        //Return along the same patrol
//        patrolPoints.Add(new Vector3(1440, 100, 890));
//        patrolPoints.Add(new Vector3(740, 100, 880));
//        patrolPoints.Add(new Vector3(570, 100, 1420));
//        patrolPoints.Add(new Vector3(725, 100, 2320));
//        patrolPoints.Add(new Vector3(1520, 100, 2270));
//        patrolPoints.Add(new Vector3(2125, 100, 1905));
//    }
//}
