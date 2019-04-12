using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitInfo
{
    public string type;
    public float maxHealth;
    public float currentHealth;
    public float attack;
    public float attackWaitTime;
    public float movementSpeed;
    public float attackRadius;

    public float timeSinceLastAttack;
    public bool active;
    public float goalx;
    public float goaly;
    public bool hold;


    public UnitInfo(string tType, float tMaxHealth, float tCurrentHealth, float tAttack, float tAttackWaitTime, 
    float tMovementSpeed, float tAttackRadius)
    {
        type = tType;
        maxHealth = tMaxHealth;
        currentHealth = tCurrentHealth;
        attack = tAttack;
        attackWaitTime = tAttackWaitTime;
        movementSpeed = tMovementSpeed;
        attackRadius = tAttackRadius;

        timeSinceLastAttack = 0.0f;
        active = false;
        goalx = 0.0f;
        goaly = 0.0f;
        hold = false;
    }
}
