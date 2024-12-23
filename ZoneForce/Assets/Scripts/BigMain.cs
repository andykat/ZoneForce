﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BigMain : MonoBehaviour
{
    public GameObject halberdierUnit;
    public GameObject swordUnit;
    public GameObject spearUnit;
    public GameObject archerUnit;
    public GameObject selectRectCanvas;
    public GameObject attackPointDisplay;
    public Camera cam;
    private List<List<GameObject>> UnitGOs = new List<List<GameObject>>();
    private List<List<Animator>> UnitGOAnimators = new List<List<Animator>>();
    private List<List<UnitInfo>> UnitInfos = new List<List<UnitInfo>>();
    private List<List<Image>> UnitHealthBars = new List<List<Image>>();
    private List<List<Transform>> UnitHealthBarCanvas = new List<List<Transform>>();
    private int id = 0;
    private const float Y = 1.2f;
    private float unitRadius = 0.5f;
    private string[] playerUnitTypes;
    private Dictionary<string, Dictionary<string, float>> unitStats = 
    new Dictionary<string, Dictionary<string, float>>();


    private bool mouseDown = false;
    private float mouseDownX = 0.0f;
    private float mouseDownZ = 0.0f;
    private Vector3 squareStartPos;
    private Vector3 squareEndPos;
    private bool selectedUnits = false;
    private List<int> unitsSelected;
    private float attackPointAlpha = 1.0f;
    private bool attackPointDisplayed = false;
    private List<List<Vector2Int>> groupMovementIndexes = new List<List<Vector2Int>>();
    private List<Vector2> groupMovementGoals = new List<Vector2>();
    private List<List<Vector2>> groupUnclutterPositions = new List<List<Vector2>>();
    private List<int> groupUnclutterIndexes = new List<int>();
    private float moveToAttackRadius = 8.0f;
    private List<Vector2> unitMeleeAttackPositions;
    private Dictionary<int, Vector2Int> unitIdToIndex = new Dictionary<int, Vector2Int>();

    private List<List<Vector2>> unitPositions = new List<List<Vector2>>();

    private List<float> playerUnitSpawnTime;
    private List<float> timeSinceLastSpawn;

    float[] playerStartX;
    float[] playerStartZ;
    float[] playerStartYRot;

    private int[] unitSpawnMax = { 12, 12, 12, 12 };

    void Start()
    {
        // initialize constant variables and units
        StartInitialization();

        unitMeleeAttackPositions = calculateUnclutterPositions(0.0f, 0.0f, 9, 1.0f);
        unitMeleeAttackPositions.RemoveAt(0);
        for(int i=0;i<4;i++)
        {
            for(int j=0;j<UnitInfos[i].Count;j++)
            {
                unitIdToIndex.Add(UnitInfos[i][j].id, new Vector2Int(i, j));
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        float dtime = Time.deltaTime;
        //chars.transform.Translate((new Vector3(0.0f, 0.0f, 2.0f)) * dtime);
        //cAnim.Play("polearm_02_walk");

        if (Input.GetMouseButtonDown(0))
        {
            //Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //Vector3 dir = ray.direction;
            //Vector3 orig = ray.origin;
            //(float x, float z) = getClickPoint(orig, dir);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
            Vector3 orig = ray.origin;
            (float x, float z) = getClickPoint(orig, dir);
            mouseDownX = x;
            mouseDownZ = z;

            squareStartPos = Input.mousePosition;
            squareEndPos = Input.mousePosition;
        }
        else if(Input.GetMouseButton(0))
        {
            squareEndPos = Input.mousePosition;
            drawSelectRect();
        }
        if(Input.GetMouseButtonUp(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
            Vector3 orig = ray.origin;
            (float x, float z) = getClickPoint(orig, dir);
            attackPointDisplay.transform.position = new Vector3(x, 1.15f, z);
            attackPointDisplayed = true;
            attackPointAlpha = 1.0f;
            attackPointAlphaChange();

            if(selectedUnits)
            {
                List<Vector2Int> group = new List<Vector2Int>();
                for (int i=0; i<unitsSelected.Count; i++)
                {
                    int index = unitsSelected[i];
                    removeUnitFromPreviousGroupMovements(0, index);
                    group.Add(new Vector2Int(0, index));
                    UnitGOAnimators[0][index].Play(getAnimationName(UnitInfos[0][index].type, "walk"));
                    UnitInfos[0][index].status = "hasgoal";
                }
                groupMovementIndexes.Add(group);
                groupMovementGoals.Add(new Vector2(x, z));
                groupUnclutterIndexes.Add(0);
                groupUnclutterPositions.Add(calculateUnclutterPositions(x, z, unitsSelected.Count, 1.7f));
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 dir = ray.direction;
            Vector3 orig = ray.origin;
            (float x, float z) = getClickPoint(orig, dir);


            if (distance_squared(x, z, mouseDownX, mouseDownZ) < 4.0f)
            {
                int unitIndex = getUnitAtLocation(x, z);
                //tapped unit
                if (unitIndex > -1)
                {
                    if(selectedUnits)
                    {
                        unSelectUnits(unitsSelected);
                    }
                    unitsSelected = new List<int>();
                    unitsSelected.Add(unitIndex);
                    selectedUnits = true;
                    showSelected();
                    //print("tapped");
                    //UnitGOAnimators[0][unitIndex].Play(getAnimationName(playerUnitTypes[0], "walk"));
                }
            }
            else
            {

                //select square
                List<int> selectSave = unitsSelected;
                unitsSelected = getUnitsInRect(mouseDownX, mouseDownZ, x, z);
                if(unitsSelected.Count > 0)
                {
                    if (selectedUnits)
                    {
                        unSelectUnits(selectSave);
                    }
                    selectedUnits = true;
                    showSelected();
                }


            }
            RectTransform selectionSquareTrans = selectRectCanvas.transform.Find("rect").GetComponent<RectTransform>();
            selectionSquareTrans.position = new Vector3(-1000.0f, -1000.0f, 0.0f);
        }
        unitMovement();
        attackPointAlphaChange();
        healthBarRotation();
        unitSpawn();
    }

    void unitSpawn()
    {
        for(int i=0;i<4;i++)
        {
            timeSinceLastSpawn[i] += Time.deltaTime;
            if(timeSinceLastSpawn[i] > playerUnitSpawnTime[i] && 
            UnitGOs[i].Count < unitSpawnMax[i])
            {
                timeSinceLastSpawn[i] = 0.0f;
                bool openPosAvailable = false;
                int count = 0;
                Vector2 openPos = new Vector2(0.0f, 0.0f);
                while(!openPosAvailable || count < 1000)
                {
                    Vector2 pos = new Vector2(Random.Range(playerStartX[i] - 3.0f, playerStartX[i] + 3.0f),
                    Random.Range(playerStartZ[i] - 3.0f, playerStartZ[i] + 3.0f));
                    openPosAvailable = true;
                    for(int j=0;j<unitPositions.Count;j++)
                    {
                        for(int k=0;k<unitPositions[j].Count;k++)
                        {
                            if(Vector2.Distance(pos, unitPositions[j][k]) < 1.0f)
                            {
                                openPosAvailable = false;
                                break;
                            }
                        }
                        if(!openPosAvailable)
                        {
                            break;
                        }
                    }
                    if(openPosAvailable)
                    {
                        openPos = pos;
                        break;
                    }
                    count += 1;
                }

                if(openPosAvailable)
                {
                    Vector3 startVector = new Vector3(openPos.x, Y, openPos.y);
                    UnitGOs[i].Add(Instantiate(GetUnitObject(playerUnitTypes[i]), startVector, Quaternion.identity));
                    int j = UnitGOs[i].Count - 1;
                    UnitGOs[i][j].transform.eulerAngles = new Vector3(0.0f, playerStartYRot[i], 0.0f);
                    UnitGOs[i][j].name = i.ToString() + "_" + id.ToString();
                    UnitGOAnimators[i].Add(UnitGOs[i][j].GetComponent<Animator>());
                    Transform canvas = UnitGOs[i][j].transform.Find("Canvas");
                    UnitHealthBarCanvas[i].Add(canvas);
                    //Transform healthBG = canvas.GetChild(0);
                    //Transform healthBar = healthBG.GetChild(0);
                    Transform healthBG = canvas.Find("HealthBG");
                    Transform healthBar = healthBG.Find("HealthBar");
                    UnitHealthBars[i].Add(healthBar.gameObject.GetComponent<Image>());

                    UnitInfo ui = new UnitInfo(id, playerUnitTypes[i], unitStats[playerUnitTypes[i]]["maxHealth"],
                     unitStats[playerUnitTypes[i]]["attack"], unitStats[playerUnitTypes[i]]["attackWaitTime"],
                     unitStats[playerUnitTypes[i]]["movementSpeed"], unitStats[playerUnitTypes[i]]["attackRadius"]);
                    ui.x = openPos.x;
                    ui.z = openPos.y;
                    UnitInfos[i].Add(ui);

                    unitIdToIndex.Add(UnitInfos[i][j].id, new Vector2Int(i, j));


                    id += 1;
                }
                else
                {
                    timeSinceLastSpawn[i] = -999999.0f;

                }
            }

        }
    }

    void unitMovement()
    {
        unitPositions = new List<List<Vector2>>();

        for (int i = 0; i < 4; i++)
        {
            List<Vector2> row = new List<Vector2>();
            for (int j = 0; j < UnitGOs[i].Count; j++)
            {
                row.Add(new Vector2(UnitGOs[i][j].transform.position.x,
                 UnitGOs[i][j].transform.position.z));
            }
            unitPositions.Add(row);
        }

        moveUnits();

        moveToAttack();

        findAttackTargetsForIdleUnits();
    }
    // a - attacker
    // v - victim
    void attackUnit(int ai, int aj, int vi, int vj, Vector2 aPos, Vector2 vPos)
    {
        if(UnitInfos[ai][aj].timeSinceLastAttack > UnitInfos[ai][aj].attackWaitTime)
        {
            if(ai == 0 && aj == 0)
            {
                print(UnitInfos[ai][aj].timeSinceLastAttack);
                print(UnitInfos[ai][aj].attackWaitTime);
                print("");
            }
            //rotate to face enemy
            float angle = -Mathf.Atan2(vPos.y - aPos.y, vPos.x - aPos.x) * Mathf.Rad2Deg + 90.0f;
            UnitGOs[ai][aj].transform.eulerAngles = new Vector3(0.0f, angle, 0.0f);
            //attack animation
            //UnitGOAnimators[ai][aj].
            //UnitGOAnimators[ai][aj].enabled = true;
            UnitGOAnimators[ai][aj].Play(getAnimationName(UnitInfos[ai][aj].type, "attack"));

            //health reduction
            float animationLength = 0.8f;
            UnitInfos[vi][vj].currentHealth -= UnitInfos[ai][aj].attack;
            StartCoroutine(reduceHealthBar(animationLength, vi, vj));

            UnitInfos[ai][aj].timeSinceLastAttack = 0.0f;
        }

    }
    void moveUnits()
    {
        //move units
        for (int i = 0; i < groupMovementIndexes.Count; i++)
        {
            List<Vector2Int> group = groupMovementIndexes[i];
            Vector2 goal = groupMovementGoals[i];
            for (int j = 0; j < group.Count; j++)
            {
                if (UnitInfos[group[j].x][group[j].y].status == "hasgoal")
                {
                    GameObject unit = UnitGOs[group[j].x][group[j].y];
                    Vector2 unitPosition = unitPositions[group[j].x][group[j].y];
                    float angle = -Mathf.Atan2(goal.y - unitPosition.y, goal.x - unitPosition.x) * Mathf.Rad2Deg + 90.0f;


                    Vector2 direction = goal - unitPosition;
                    direction.Normalize();
                    unit.transform.eulerAngles = new Vector3(0.0f, angle, 0.0f);
                    Vector3 newPos = unit.transform.position + (new Vector3(direction.x, 0.0f, direction.y))
                     * UnitInfos[group[j].x][group[j].y].movementSpeed * Time.deltaTime;
                    unit.transform.position = newPos;
                    float distance = Vector2.Distance(newPos, new Vector3(goal.x, Y, goal.y));
                    if (distance < 0.2f)
                    {
                        UnitInfos[group[j].x][group[j].y].unclutterx =
                        groupUnclutterPositions[i][groupUnclutterIndexes[i]].x;
                        UnitInfos[group[j].x][group[j].y].unclutterz =
                        groupUnclutterPositions[i][groupUnclutterIndexes[i]].y;
                        UnitInfos[group[j].x][group[j].y].status = "unclutter";
                        groupUnclutterIndexes[i] += 1;

                    }
                }
                else if (UnitInfos[group[j].x][group[j].y].status == "unclutter")
                {
                    GameObject unit = UnitGOs[group[j].x][group[j].y];
                    Vector2 unitPosition = unitPositions[group[j].x][group[j].y];
                    Vector2 unclutterGoal = new Vector2(UnitInfos[group[j].x][group[j].y].unclutterx,
                     UnitInfos[group[j].x][group[j].y].unclutterz);
                    float angle = -Mathf.Atan2(unclutterGoal.y - unitPosition.y,
                     unclutterGoal.x - unitPosition.x) * Mathf.Rad2Deg + 90.0f;


                    Vector2 direction = unclutterGoal - unitPosition;
                    direction.Normalize();
                    unit.transform.eulerAngles = new Vector3(0.0f, angle, 0.0f);
                    Vector3 newPos = unit.transform.position + (new Vector3(direction.x, 0.0f, direction.y))
                     * UnitInfos[group[j].x][group[j].y].movementSpeed * Time.deltaTime;
                    unit.transform.position = newPos;
                    float distance = Vector2.Distance(newPos, new Vector3(unclutterGoal.x, Y, unclutterGoal.y));
                    if (distance < 0.1f)
                    {
                        UnitInfos[group[j].x][group[j].y].status = "idle";
                        UnitGOAnimators[group[j].x][group[j].y].Play(
                        getAnimationName(UnitInfos[group[j].x][group[j].y].type, "idle"));
                    }
                }
            }
        }
    }

    void moveToAttack()
    {
        //handle move to attack
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < unitPositions[i].Count; j++)
            {
                if (UnitInfos[i][j].status == "movetoattack")
                {
                    // update time since last attack
                    UnitInfos[i][j].timeSinceLastAttack += Time.deltaTime;

                    //check if need to recalculate attack target
                    UnitInfos[i][j].recalculateAttackMoveTime += Time.deltaTime;
                    Vector2Int enemyIndex = unitIdToIndex[UnitInfos[i][j].moveattackTargetId];
                    if (UnitInfos[i][j].recalculateAttackMoveTime > 0.1f)
                    {
                        UnitInfos[i][j].recalculateAttackMoveTime = 0.0f;
                        //did not find an attackable position
                        if (!setMoveToAttack(new Vector2Int(i, j), enemyIndex))
                        {
                            UnitInfos[i][j].status = "idle";
                            UnitGOAnimators[i][j].Play(
                        getAnimationName(UnitInfos[i][j].type, "idle"));
                        }
                    }

                    // check if can attack target
                    Vector2 unitPosition = unitPositions[i][j];
                    Vector2 enemyPosition = unitPositions[enemyIndex.x][enemyIndex.y];
                    GameObject unit = UnitGOs[i][j];
                    Vector2 goal = UnitInfos[i][j].moveAttackCoordinate;

                    float distance = Vector2.Distance(unitPosition, new Vector2(goal.x, goal.y));
                    if (distance < 0.1f || Vector2.Distance(unitPosition, enemyPosition) < UnitInfos[i][j].attackRadius)
                    {
                        // attack if within radius
                        if (Vector2.Distance(unitPosition, enemyPosition) < UnitInfos[i][j].attackRadius)
                        {
                            attackUnit(i, j, enemyIndex.x, enemyIndex.y, unitPosition, enemyPosition);
                        }
                        else
                        {
                            //goal was not close enough to enemy, find new enemy.
                            //!!! or try to find enemy new position again. could be handled by recalculate attack move
                            UnitInfos[i][j].status = "idle";
                            UnitGOAnimators[i][j].Play(
                            getAnimationName(UnitInfos[i][j].type, "idle"));
                        }

                    }
                    else
                    {
                        //not close enough to target
                        //move to the target

                        float angle = -Mathf.Atan2(goal.y - unitPosition.y, goal.x - unitPosition.x) * Mathf.Rad2Deg + 90.0f;


                        Vector2 direction = goal - unitPosition;
                        direction.Normalize();
                        unit.transform.eulerAngles = new Vector3(0.0f, angle, 0.0f);
                        Vector3 newPos = unit.transform.position + (new Vector3(direction.x, 0.0f, direction.y))
                         * UnitInfos[i][j].movementSpeed * Time.deltaTime;
                        unit.transform.position = newPos;
                    }
                }
            }
        }
    }

    void findAttackTargetsForIdleUnits()
    {
        // calculate whether units are in range of each other to attack
        //!!!
        for (int i = 0; i < 1; i++)
        {
            for (int j = 0; j < unitPositions[i].Count; j++)
            {
                if (UnitInfos[i][j].status != "idle")
                {
                    continue;
                }

                List<Vector2> attackablePositions = new List<Vector2>();
                List<float> attackablePositionDistances = new List<float>();
                List<Vector2Int> attackableIndexes = new List<Vector2Int>();
                for (int k = 0; k < 4; k++)
                {
                    if (i == k)
                    {
                        continue;
                    }

                    for (int l = 0; l < unitPositions[k].Count; l++)
                    {
                        if (Vector2.Distance(unitPositions[i][j], unitPositions[k][l]) < moveToAttackRadius)
                        {
                            attackablePositionDistances.Add(Vector2.Distance(unitPositions[i][j], unitPositions[k][l]));
                            attackableIndexes.Add(new Vector2Int(k, l));
                            //setMoveToAttack(new Vector2Int(i, k), new Vector2Int(j, l));

                        }

                    }
                }

                // find the closest unit that we can attack
                while (attackablePositionDistances.Count > 0)
                {
                    //find shortest distance
                    int minIndex = findListMin(attackablePositionDistances);
                    if (setMoveToAttack(new Vector2Int(i, j), attackableIndexes[minIndex]))
                    {
                        break;
                    }
                    else
                    {
                        attackablePositionDistances.RemoveAt(minIndex);
                        attackableIndexes.RemoveAt(minIndex);
                    }
                }
            }

        }
    }

    IEnumerator reduceHealthBar(float time, int vi, int vj)
    {
        yield return new WaitForSeconds(time);
        //health bar change
        UnitHealthBars[vi][vj].fillAmount = UnitInfos[vi][vj].currentHealth / UnitInfos[vi][vj].maxHealth;
    }

    bool setMoveToAttack(Vector2Int attackerIndex, Vector2Int victimIndex)
    {
        Vector2 attackerPos = unitPositions[attackerIndex.x][attackerIndex.y];
        Vector2 enemyPos = unitPositions[victimIndex.x][victimIndex.y];
        //find attack move square
        float minDistance = 99999.0f;
        Vector2 finalPos = new Vector2(0.0f, 0.0f);
        for (int i=0;i<unitMeleeAttackPositions.Count;i++)
        {
            Vector2 tPos = enemyPos + unitMeleeAttackPositions[i] *
            (UnitInfos[attackerIndex.x][attackerIndex.y].attackRadius - 0.2f);
            float dist = Vector2.Distance(tPos, attackerPos);
            if(dist < minDistance)
            {
                bool squareOccupied = false;
                for (int j=0;j<unitPositions.Count;j++)
                {
                    for(int k=0;k<unitPositions[j].Count;k++)
                    {
                        if(j == attackerIndex.x && k == attackerIndex.y)
                        {
                            continue;
                        }
                        if (Vector2.Distance(unitPositions[j][k], tPos ) < 2 * unitRadius)
                        {
                            squareOccupied = true;
                            break;
                        }
                    }
                    if(squareOccupied)
                    {
                        break;
                    }
                }
                if (!squareOccupied)
                {
                    minDistance = dist;
                    finalPos = tPos;
                }
            }
        }
        if(minDistance < 99998.0f)
        {
            UnitInfos[attackerIndex.x][attackerIndex.y].moveAttackCoordinate = finalPos;
            if(UnitInfos[attackerIndex.x][attackerIndex.y].status != "movetoattack")
            {
                UnitInfos[attackerIndex.x][attackerIndex.y].timeSinceLastAttack = 10.0f;
                UnitGOAnimators[attackerIndex.x][attackerIndex.y].Play(
                getAnimationName(UnitInfos[attackerIndex.x][attackerIndex.y].type, "walk"));
            }
            UnitInfos[attackerIndex.x][attackerIndex.y].status = "movetoattack";
            UnitInfos[attackerIndex.x][attackerIndex.y].moveattackTargetId =
            UnitInfos[victimIndex.x][victimIndex.y].id;

            UnitInfos[attackerIndex.x][attackerIndex.y].recalculateAttackMoveTime = 0.0f;

            return true;
        }
        return false;
    }

    void removeUnitFromPreviousGroupMovements(int playerIndex, int unitIndex)
    {
        //check to see if the unit is in any other groups
        for (int i = 0; i < groupMovementIndexes.Count; i++)
        {
            for (int j = 0; j < groupMovementIndexes[i].Count; j++)
            {
                if(playerIndex == groupMovementIndexes[i][j].x && 
                unitIndex == groupMovementIndexes[i][j].y)
                {
                    groupMovementIndexes[i].RemoveAt(j);
                    j--;
                }
            }

        }

    }

    (int,int) unitCollide(int ti, int tj)
    {
        float ux = UnitInfos[ti][tj].x;
        float uz = UnitInfos[ti][tj].z;

        for(int i=0;i<4;i++)
        {
            for(int j=0;j<4;j++)
            {
                if(ti == i && tj == j)
                {
                    continue;
                }

                if(distance_squared(ux, uz, UnitInfos[i][j].x, UnitInfos[i][j].z) < 4*unitRadius*unitRadius)
                {
                    return (i, j);
                }
            }

        }
        return (-1, -1);
    }
    void attackPointAlphaChange()
    {
        if(attackPointDisplayed)
        {
            attackPointAlpha -= Time.deltaTime * 0.66f;
            if(attackPointAlpha < 0.0f)
            {
                //move attack point off screen
                attackPointDisplay.transform.position = new Vector3(-37.0f, 1.12f, -37.0f);
                attackPointDisplayed = false;
            }
            else
            {
                float alpha = attackPointAlpha;
                if(alpha > 1.0f)
                {
                    alpha = 1.0f;
                }
                //attackPointDisplay.GetComponent<MeshRenderer>().material.color
                 //= new Color(1.0f, 0.0f, 0.0f, alpha);
                attackPointDisplay.GetComponent<MeshRenderer>().
                material.SetColor("_TintColor", new Color(1.0f, 0.0f, 0.0f, alpha));

            }

        }

    }

    void showSelected()
    {
        for(int i=0; i<unitsSelected.Count; i++)
        {
            int index = unitsSelected[i];

            UnitGOs[0][index].transform.Find("SelectCircle").gameObject.SetActive(true);

        }
    }
    void unSelectUnits(List<int> unitIndexes)
    {
        for (int i = 0; i < unitIndexes.Count; i++)
        {
            int index = unitIndexes[i];

            UnitGOs[0][index].transform.Find("SelectCircle").gameObject.SetActive(false);

        }
    }

    void drawSelectRect()
    {
        RectTransform selectionSquareTrans = selectRectCanvas.transform.Find("rect").GetComponent<RectTransform>();
        //Vector3 squareStartScreen = Camera.main.WorldToScreenPoint(squareStartPos);
        Vector3 squareStartScreen = squareStartPos;
        squareStartScreen.z = 0f;

        //Get the middle position of the square
        Vector3 middle = (squareStartScreen + squareEndPos) / 2f;

        //Set the middle position of the GUI square
        selectionSquareTrans.position = middle;

        // Change the size of the square
        float sizeX = Mathf.Abs(squareStartScreen.x - squareEndPos.x) / 4.0f;
        float sizeY = Mathf.Abs(squareStartScreen.y - squareEndPos.y) / 4.0f;

        //Set the size of the square
        selectionSquareTrans.sizeDelta = new Vector2(sizeX, sizeY);
    }

    void healthBarRotation()
    {
        for(int i=0;i<4;i++)
        {
            for(int j=0;j<UnitHealthBarCanvas[i].Count;j++)
            {
                UnitHealthBarCanvas[i][j].eulerAngles = new Vector3(45.0f, 30.0f, 0.0f);
            }
        }
    }


    string getAnimationName(string type, string action)
    {
        if(type == "halberdier")
        {
            if(action == "walk")
            {
                return "polearm_02_walk";
            }
            else if(action == "idle")
            {
                return "polearm_01_idle";

            }
            else if(action == "attack")
            {
                return "polearm_04_attack_A";
            }

        }

        return "";
    }
    List<int> getUnitsInRect(float x1, float z1, float x2, float z2)
    {
        int player = 0;
        float minx = min(squareStartPos.x, squareEndPos.x);
        float miny = min(squareStartPos.y, squareEndPos.y);
        float maxx = max(squareStartPos.x, squareEndPos.x);
        float maxy = max(squareStartPos.y, squareEndPos.y);
        float radius = 1.0f;
        List<int> unitIndexes = new List<int>();

        for (int i = 0; i < UnitGOs[player].Count; i++)
        {
            Vector3 point2d = cam.WorldToScreenPoint(UnitGOs[player][i].transform.position);

            Vector3 unitHeadPosition = UnitGOs[player][i].transform.position + (new Vector3(0.0f, 1.0f, 0.0f));
            Vector3 unitHeadPos2d = cam.WorldToScreenPoint(unitHeadPosition);
            if ((minx < point2d.x + radius && point2d.x - radius < maxx 
            && miny < point2d.y + radius && point2d.y - radius < maxy) || 
                (minx < unitHeadPos2d.x + radius && unitHeadPos2d.x - radius < maxx
            && miny < unitHeadPos2d.y + radius && unitHeadPos2d.y - radius < maxy))
            {
                unitIndexes.Add(i);
            }

        }

        return unitIndexes;
    }
    int getUnitAtLocation(float x, float z)
    {
        int player = 0;
        for(int i=0; i<UnitGOs[player].Count; i++)
        {
            if(distance_squared(x, z, UnitGOs[player][i].transform.position.x, 
            UnitGOs[player][i].transform.position.z) < unitRadius * 2.0)
            {
                return i;
            }
        }

        return -1;
    }

    (float, float) getClickPoint(Vector3 orig, Vector3 dir)
    {
        float t = (Y - orig.y) / dir.y;
        float x = dir.x * t + orig.x;
        float z = dir.z * t + orig.z;
        return (x, z);
    }
    void StartInitialization()
    {
        id = 0;
        playerUnitTypes = new string[4] { "halberdier", "sword", "spear", "archer" };

        Dictionary<string, float> halberdierStats = new Dictionary<string, float>();
        halberdierStats["maxHealth"] = 120.0f;
        halberdierStats["attack"] = 40.0f;
        halberdierStats["attackWaitTime"] = 4.0f;
        halberdierStats["movementSpeed"] = 1.7f;
        halberdierStats["attackRadius"] = 3.5f;

        Dictionary<string, float> swordStats = new Dictionary<string, float>();
        swordStats["maxHealth"] = 100.0f;
        swordStats["attack"] = 25.0f;
        swordStats["attackWaitTime"] = 3.0f;
        swordStats["movementSpeed"] = 2.3f;
        swordStats["attackRadius"] = 3.5f;

        Dictionary<string, float> spearStats = new Dictionary<string, float>();
        spearStats["maxHealth"] = 100.0f;
        spearStats["attack"] = 30.0f;
        spearStats["attackWaitTime"] = 4.50f;
        spearStats["movementSpeed"] = 2.0f;
        spearStats["attackRadius"] = 5.0f;

        Dictionary<string, float> archerStats = new Dictionary<string, float>();
        archerStats["maxHealth"] = 80.0f;
        archerStats["attack"] = 15.0f;
        archerStats["attackWaitTime"] = 2.0f;
        archerStats["movementSpeed"] = 1.7f;
        archerStats["attackRadius"] = 12.0f;

        unitStats["halberdier"] = halberdierStats;
        unitStats["sword"] = swordStats;
        unitStats["spear"] = spearStats;
        unitStats["archer"] = archerStats;

        float edgeValue = 23.0f;
        //float[] playerStartX = { -edgeValue, -edgeValue, edgeValue, edgeValue };
        //float[] playerStartZ = { -edgeValue, edgeValue, edgeValue, -edgeValue };
        playerStartX = new float[]{ -edgeValue, -edgeValue, edgeValue, edgeValue };
        playerStartZ = new float[] { -edgeValue, -edgeValue + 10.0f, edgeValue, -edgeValue };
        playerStartYRot = new float[] { 0.0f, 180.0f, 180.0f, 0.0f };

        playerUnitSpawnTime = new List<float>();
        timeSinceLastSpawn = new List<float>();
        //create units
        for (int i = 0; i < 4; i++)
        {
            playerUnitSpawnTime.Add(3.0f);
            timeSinceLastSpawn.Add(0.0f);

            UnitGOs.Add(new List<GameObject>());
            UnitGOAnimators.Add(new List<Animator>());
            UnitInfos.Add(new List<UnitInfo>());
            UnitHealthBars.Add(new List<Image>());
            UnitHealthBarCanvas.Add(new List<Transform>());
            for (int j = 0; j < 9; j++)
            {
                float fj = (float)(j - 1);
                Vector3 startVector = new Vector3(playerStartX[i] + fj * 1.5f, Y, playerStartZ[i]);
                UnitGOs[i].Add(Instantiate(GetUnitObject(playerUnitTypes[i]), startVector, Quaternion.identity));
                UnitGOs[i][j].transform.eulerAngles = new Vector3(0.0f, playerStartYRot[i], 0.0f);
                UnitGOs[i][j].name = i.ToString() + "_" + id.ToString();
                UnitGOAnimators[i].Add(UnitGOs[i][j].GetComponent<Animator>());
                Transform canvas = UnitGOs[i][j].transform.Find("Canvas");
                UnitHealthBarCanvas[i].Add(canvas);
                //Transform healthBG = canvas.GetChild(0);
                //Transform healthBar = healthBG.GetChild(0);
                Transform healthBG = canvas.Find("HealthBG");
                Transform healthBar = healthBG.Find("HealthBar");
                UnitHealthBars[i].Add(healthBar.gameObject.GetComponent<Image>());

                UnitInfo ui = new UnitInfo(id, playerUnitTypes[i], unitStats[playerUnitTypes[i]]["maxHealth"],
                 unitStats[playerUnitTypes[i]]["attack"], unitStats[playerUnitTypes[i]]["attackWaitTime"],
                 unitStats[playerUnitTypes[i]]["movementSpeed"], unitStats[playerUnitTypes[i]]["attackRadius"]);
                ui.x = playerStartX[i] + fj * 1.5f;
                ui.z = playerStartZ[i];
                UnitInfos[i].Add(ui);
                
                id += 1;

            }

        }

        //test
        //UnitHealthBars[0][0].fillAmount = 0.5f;
        //UnitGOs[0][0].transform.Find("SelectCircle").gameObject.SetActive(true);
    }
    float distance_squared(float x1, float y1, float x2, float y2)
    {
        return ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
    }
    GameObject GetUnitObject(string type)
    {
        if(type == "halberdier")
        {
            return halberdierUnit;
        }
        else if(type == "sword")
        {
            return swordUnit;
        }
        else if(type == "spear")
        {
            return spearUnit;
        }
        else if(type == "archer")
        {
            return archerUnit;
        }
        return halberdierUnit;
    }

    float max(float a, float b)
    {
        if(a>b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    float min(float a, float b)
    {
        if (a < b)
        {
            return a;
        }
        else
        {
            return b;
        }
    }

    List<Vector2> calculateUnclutterPositions(float x, float z, int n, float unclutterRadius)
    {
        //float unclutterRadius = 1.7f;
        List<Vector2> unclutterPositions = new List<Vector2>();
        unclutterPositions.Add(new Vector2(x, z));
        List<Vector2> left = new List<Vector2>();
        left.Add(new Vector2(x, z));
        List<Vector2> right = new List<Vector2>();
        right.Add(new Vector2(x, z));
        List<Vector2> top = new List<Vector2>();
        top.Add(new Vector2(x, z));
        List<Vector2> bottom = new List<Vector2>();
        bottom.Add(new Vector2(x, z));
        Vector2 cornerTopLeft = new Vector2(x, z);
        Vector2 cornerTopRight = new Vector2(x, z);
        Vector2 cornerBottomLeft = new Vector2(x, z);
        Vector2 cornerBottomRight = new Vector2(x, z);

        int count = 1;

        while (count < n)
        {
            //left
            List<Vector2> newLeft = new List<Vector2>();
            for (int i = 0; i < left.Count; i++)
            {
                Vector2 newpos = left[i] + (new Vector2(-unclutterRadius, 0.0f));
                unclutterPositions.Add(newpos);
                newLeft.Add(newpos);
                count += 1;
                if (count >= n)
                {
                    break;
                }
            }
            if (count >= n)
            {
                break;
            }
            left = newLeft;

            //right
            List<Vector2> newRight = new List<Vector2>();
            for (int i = 0; i < right.Count; i++)
            {
                Vector2 newpos = right[i] + (new Vector2(unclutterRadius, 0.0f));
                unclutterPositions.Add(newpos);
                newRight.Add(newpos);
                count += 1;
                if (count >= n)
                {
                    break;
                }
            }
            if (count >= n)
            {
                break;
            }
            right = newRight;

            //top
            List<Vector2> newTop = new List<Vector2>();
            for (int i = 0; i < top.Count; i++)
            {
                Vector2 newpos = top[i] + (new Vector2(0.0f, unclutterRadius));
                unclutterPositions.Add(newpos);
                newTop.Add(newpos);
                count += 1;
                if (count >= n)
                {
                    break;
                }
            }
            if (count >= n)
            {
                break;
            }
            top = newTop;

            //bottom
            List<Vector2> newBottom = new List<Vector2>();
            for (int i = 0; i < bottom.Count; i++)
            {
                Vector2 newpos = bottom[i] + (new Vector2(0.0f, -unclutterRadius));
                unclutterPositions.Add(newpos);
                newBottom.Add(newpos);
                count += 1;
                if (count >= n)
                {
                    break;
                }
            }
            if (count >= n)
            {
                break;
            }
            bottom = newBottom;

            //topleft corner
            Vector2 newTopLeft = cornerTopLeft + (new Vector2(-unclutterRadius, unclutterRadius));
            unclutterPositions.Add(newTopLeft);
            cornerTopLeft = newTopLeft;
            left.Add(newTopLeft);
            top.Add(newTopLeft);
            count += 1;
            if (count >= n)
            {
                break;
            }

            //top right corner
            Vector2 newTopRight = cornerTopRight + (new Vector2(unclutterRadius, unclutterRadius));
            unclutterPositions.Add(newTopRight);
            cornerTopRight = newTopRight;
            right.Add(newTopRight);
            top.Add(newTopRight);
            count += 1;
            if (count >= n)
            {
                break;
            }

            //bottom left corner
            Vector2 newBottomLeft = cornerBottomLeft + (new Vector2(-unclutterRadius, -unclutterRadius));
            unclutterPositions.Add(newBottomLeft);
            cornerBottomLeft = newBottomLeft;
            left.Add(newBottomLeft);
            bottom.Add(newBottomLeft);
            count += 1;
            if (count >= n)
            {
                break;
            }

            //bottom right corner
            Vector2 newBottomRight = cornerBottomRight + (new Vector2(unclutterRadius, -unclutterRadius));
            unclutterPositions.Add(newBottomRight);
            cornerBottomRight = newBottomRight;
            right.Add(newBottomRight);
            bottom.Add(newBottomRight);
            count += 1;
            if (count >= n)
            {
                break;
            }
        }

        return unclutterPositions;
    }

    int findListMin(List<float> a)
    {
        float tmin = 99999.0f;
        int minIndex = 0;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i] < tmin)
            {
                tmin = a[i];
                minIndex = i;
            }
        }
        return minIndex;
    }


}
