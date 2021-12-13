using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIDirector : MonoBehaviour
{
    private TextMeshProUGUI MaxTime;
    private TextMeshProUGUI SimulTime;
    private TextMeshProUGUI SimulStep;
    private TextMeshProUGUI LerpTimer;

    void Start()
    {
        MaxTime = GameObject.Find("Max Time").GetComponent<TextMeshProUGUI>();
        SimulTime = GameObject.Find("Simul Time").GetComponent<TextMeshProUGUI>();
        SimulStep = GameObject.Find("Simul Step").GetComponent<TextMeshProUGUI>();
        LerpTimer = GameObject.Find("Lerp Timer").GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        MaxTime.text = "Max Time: " + VehicleManager.maxTime.ToString() + "s";
        SimulTime.text = "Simul. Time: " + VehicleManager.simulationTime.ToString("F2") + "s";
        SimulStep.text = "Simul. Step: " + VehicleManager.simulationStep.ToString() + "s";
        LerpTimer.text = "Lerp Timer: " + VehicleManager.LerpTimer.ToString("F6") + "s";
    }

}
