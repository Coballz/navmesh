﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ruleset", menuName = "Ruleset", order = 51)]
public class Ruleset : ScriptableObject
{
    public float moveSpeed = 150.0f;
    public float moveSpeedWhenDamaged = 75.0f;
    public float spottingRange = 300.0f;
    public float attackRange = 150.0f;
    public float rotationSpeed = 5.0f;
}