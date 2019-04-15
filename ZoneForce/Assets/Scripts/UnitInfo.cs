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
    public float unclutterx;
    public float unclutterz;
    public string status = "idle";
    public Vector2 moveAttackCoordinate;
    public int moveattackTargetId;
    public float recalculateAttackMoveTime;



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

        unclutterx = 0.0f;
        unclutterz = 0.0f;

        moveAttackCoordinate = new Vector2(0.0f, 0.0f);
        moveattackTargetId = 0;
        recalculateAttackMoveTime = 0.0f;
    }
}
