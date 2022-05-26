using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDirector : MonoBehaviour
{
    private TextMeshProUGUI Time;
    private TextMeshProUGUI LerpTimer;

    void Start()
    {
        Time = GameObject.Find("Time").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        Time.text = "Time [s]: " + VehicleManager.simulationTime.ToString("F2") + "/" + VehicleManager.maxTime.ToString();
    }

}
