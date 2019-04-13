using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo
{
    public int id;
    public string type;
    public float maxHealth;

    public float attack;
    public float attackWaitTime;
    public float movementSpeed;
    public float attackRadius;

    public float currentHealth;
    public float timeSinceLastAttack;
    public bool active;
    public float goalx;
    public float goalz;
    public float x;
    public float z;
    public bool hasGoal;
    public bool hold;
    public bool unclutterStatus;
    public float unclutterx;
    public float unclutterz;
    public bool finishedMovement;


    public UnitInfo(int tId, string tType, float tMaxHealth, float tAttack, float tAttackWaitTime, 
    float tMovementSpeed, float tAttackRadius)
    {
        id = tId;
        type = tType;
        maxHealth = tMaxHealth;
        attack = tAttack;
        attackWaitTime = tAttackWaitTime;
        movementSpeed = tMovementSpeed;
        attackRadius = tAttackRadius;

        currentHealth = maxHealth;
        timeSinceLastAttack = 0.0f;
        active = false;
        goalx = 0.0f;
        goalz = 0.0f;
        hold = false;
        hasGoal = false;

        unclutterStatus = false;
        unclutterx = 0.0f;
        unclutterz = 0.0f;
        finishedMovement = false;
    }
}
