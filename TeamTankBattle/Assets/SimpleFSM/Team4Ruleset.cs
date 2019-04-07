using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Team4;

[CreateAssetMenu(fileName = "New Ruleset", menuName = "Ruleset", order = 1)]
public class Team4Ruleset : ScriptableObject
{
    public float moveSpeed = 150.0f;
    public float moveSpeedWhenDamaged = 75.0f;
    public float spottingRange = 300.0f;
    public float GTFORange = 300.0f;
    public float attackRange = 150.0f;
    public float rotationSpeed = 5.0f;
    public float fireRate = 3.0f;
    public float cohC = 6;
    public float sepC = 9;
    public float aliC = 90;

    public enum FSMState
    {
        None,
        Patrol,
        Offense,
        Dead,
        GTFO
    }
}
