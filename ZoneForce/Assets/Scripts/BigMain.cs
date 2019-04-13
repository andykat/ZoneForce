using System.Collections;
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
    private float unitRadius = 1.5f;
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

    void Start()
    {
        // initialize constant variables and units
        StartInitialization();


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
            attackPointDisplay.transform.position = new Vector3(x, 1.12f, z);
            attackPointDisplayed = true;
            attackPointAlpha = 1.0f;
            attackPointAlphaChange();

            if(selectedUnits)
            {
                for(int i=0; i<unitsSelected.Count; i++)
                {
                    int index = unitsSelected[i];
                    UnitInfos[0][i].hasGoal = true;
                    UnitInfos[0][i].goalx = x;
                    UnitInfos[0][i].goalz = z;
                }

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
                        unSelectUnits(unitsSelected);
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
    }

    void unitMovement()
    {


    }
    (int,int) unitCollide(int ti, int tj)
    {
        int collideI = -1;
        int collideJ = -1;

        for(int i=0;i<4;i++)
        {
            for(int j=0;j<4;j++)
            {


            }

        }
        return (collideI, collideJ);
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
            if (minx < point2d.x + radius && point2d.x - radius < maxx 
            && miny < point2d.y + radius && point2d.y - radius < maxy)
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
        halberdierStats["attackWaitTime"] = 3.0f;
        halberdierStats["movementSpeed"] = 1.7f;
        halberdierStats["attackRadius"] = 5.0f;

        Dictionary<string, float> swordStats = new Dictionary<string, float>();
        swordStats["maxHealth"] = 100.0f;
        swordStats["attack"] = 25.0f;
        swordStats["attackWaitTime"] = 1.5f;
        swordStats["movementSpeed"] = 2.3f;
        swordStats["attackRadius"] = 3.5f;

        Dictionary<string, float> spearStats = new Dictionary<string, float>();
        spearStats["maxHealth"] = 100.0f;
        spearStats["attack"] = 30.0f;
        spearStats["attackWaitTime"] = 2.25f;
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
        float[] playerStartX = new float[4] { -edgeValue, -edgeValue, edgeValue, edgeValue };
        float[] playerStartZ = new float[4] { -edgeValue, edgeValue, edgeValue, -edgeValue };
        float[] playerStartYRot = new float[4] { 0.0f, 180.0f, 180.0f, 0.0f };


        //create units
        for (int i = 0; i < 4; i++)
        {
            UnitGOs.Add(new List<GameObject>());
            UnitGOAnimators.Add(new List<Animator>());
            UnitInfos.Add(new List<UnitInfo>());
            UnitHealthBars.Add(new List<Image>());
            UnitHealthBarCanvas.Add(new List<Transform>());
            for (int j = 0; j < 5; j++)
            {
                float fj = (float)(j - 1);
                Vector3 startVector = new Vector3(playerStartX[i] + fj * unitRadius, Y, playerStartZ[i]);
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
                ui.x = playerStartX[i] + fj * unitRadius;
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




}
