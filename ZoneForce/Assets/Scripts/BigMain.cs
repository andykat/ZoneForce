using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigMain : MonoBehaviour
{
    public GameObject obj1;
    private GameObject chars;
    private GameObject chars2;
    private List<GameObject> UnitsGOs;

    private Animator cAnim;
    void Start()
    {
        chars = new GameObject();
        chars = Instantiate(obj1, new Vector3(-20.0f, 1.2f, -20.0f), Quaternion.identity);
        cAnim = chars.GetComponent<Animator>();

        chars2 = new GameObject();
        chars2 = Instantiate(obj1, new Vector3(-15.0f, 1.2f, -20.0f), Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        float dtime = Time.deltaTime;
        chars.transform.Translate((new Vector3(0.0f, 0.0f, 2.0f)) * dtime);
        cAnim.Play("polearm_02_walk");
    }
}
