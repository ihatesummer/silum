using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagScript : MonoBehaviour
{
    //시간 설정
    float timecount = 0;
    public float oneSecond = 0.25f;
    int realSecond = 0;

    //배열 변수

    float[] carList = new float[100];

    bool[] carStatus = new bool[100];
    void Start()
    {
        carStatus[0] = true;
        carList[0] = 1f;
        Debug.Log(carList[0]);

    }

    // Update is called once per frame
    void Update()
    {

        
        if (oneSecond < timecount)
        {
            timecount = 0;
            realSecond++;
            
        }
        timecount += 1f * Time.deltaTime;
    }
}
