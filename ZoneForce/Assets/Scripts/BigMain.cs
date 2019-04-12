using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BigMain : MonoBehaviour
{
    public GameObject halberdierUnit;
    public GameObject swordUnit;
    public GameObject spearUnit;
    public GameObject archerUnit;

    private List<List<GameObject>> UnitGOs = new List<List<GameObject>>();
    private List<List<Animator>> UnitGOAnimators = new List<List<Animator>>();

    private const float Y = 1.2f;
    private string[] playerUnitTypes;
    void Start()
    {
        playerUnitTypes = new string[4] { "halberdier", "sword", "spear", "archer" };
        float edgeValue = 23.0f;
        float[] playerStartX = new float[4] { -edgeValue, -edgeValue, edgeValue, edgeValue };
        float[] playerStartZ = new float[4] { -edgeValue, edgeValue, edgeValue, -edgeValue };
        float[] playerStartYRot = new float[4] { 0.0f, 180.0f, 180.0f, 0.0f };
        float unitRadius = 1.5f;
        for (int i=0; i<4; i++)
        {
            UnitGOs.Add(new List<GameObject>());
            UnitGOAnimators.Add(new List<Animator>());
            for(int j=0; j<3; j++)
            {
                float fj = (float) (j - 1);
                GameObject tGO = new GameObject();
                Vector3 startVector = new Vector3(playerStartX[i] + fj * unitRadius, Y, playerStartZ[i]);

                UnitGOs[i].Add(Instantiate(GetUnitObject(playerUnitTypes[i]), startVector, Quaternion.identity));
                UnitGOs[i][j].transform.eulerAngles = new Vector3(0.0f, playerStartYRot[i], 0.0f);
                UnitGOAnimators[i].Add(UnitGOs[i][j].GetComponent<Animator>());
                if(i>0)
                {
                    print("hi");
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        float dtime = Time.deltaTime;
        //chars.transform.Translate((new Vector3(0.0f, 0.0f, 2.0f)) * dtime);
        //cAnim.Play("polearm_02_walk");
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

}
