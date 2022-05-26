using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Xml;
using System.IO;
using TMPro;

public class VehicleInfo
{
    public int v_id { get; set; }
    public float v_posX { get; set; }
    public float v_posZ { get; set; }
    public float v_angle { get; set; }
    public float v_speed { get; set; }
}

public class VehicleManager : MonoBehaviour
{
    [Header("Vehicle prefabs")]
    public GameObject Prefab_Real;
    public GameObject Prefab_Estimated;
    [Space(10f)]
    [Header("Etc")]
    public string mapChoice = "grid";
    private float vehicle_elevation = 0.5f;
    private string mobilityDataFile = "mobility.csv";
    public static Dictionary<string, List<VehicleInfo>> vehicleInfos;
    public static float simulationTime = 0f;
    public static int simulationStep = 0;
    public static float LerpTimer = 0f;
    public static float maxTime;
    private static GameObject Vehicles;
    private GameObject vehicle;
    public static int nVehicle;

    void Start()
    {
        Application.targetFrameRate = 60;
        nVehicle = 0;
        vehicleInfos = new Dictionary<string, List<VehicleInfo>>();
        string mobilityDataPath = Application.dataPath + "/SUMO/" +
                                  mapChoice + "/" + mobilityDataFile;

        parseMobility(mobilityDataPath);
        Vehicles = new GameObject("Vehicles");
    }

    void Update()
    {
        simulationTime += Time.deltaTime;
        string simulationTime_str = simulationTime.ToString("F2");
        if (simulationTime < maxTime)
        {
            foreach (VehicleInfo v_info in vehicleInfos[simulationTime_str])
            {
                string vehicleObjectName = "vehicle_no." + v_info.v_id + "_real";
                if (!GameObject.Find(vehicleObjectName))
                {
                    vehicle = generateVehicle(
                        true,
                        vehicleObjectName,
                        v_info.v_posX,
                        v_info.v_posZ,
                        v_info.v_angle);
                    attachLabel(vehicle, v_info);
                    createVehiclePosUI(vehicle, nVehicle, vehicleObjectName);
                    nVehicle++;
                }
                else
                {
                    vehicle = GameObject.Find(vehicleObjectName);

                    moveVehicle(vehicle, v_info);
                    // in case vehicle rotated, correct the label rotation
                    updateLabelRotation(vehicle);
                    updateVehiclePosUI(vehicle, vehicleObjectName);
                }
            }
        }
    }

    void parseMobility(string mobilityDataPath)
    {
        using (StreamReader sr = new StreamReader(mobilityDataPath)){
            string line = sr.ReadLine();
            string old_time = "dummy";
            string time_str = "";
            while ((line = sr.ReadLine()) != null)
            {
                string[] entries = line.Split(',');
                double time = float.Parse(entries[0]);
                time_str = time.ToString("F2");
                int id = (int)float.Parse(entries[1]);
                float x = float.Parse(entries[2]);
                float z = float.Parse(entries[3]);
                float ang = float.Parse(entries[4]);
                float vel = float.Parse(entries[5]);
                if (old_time != time_str){
                    vehicleInfos.Add(time_str, new List<VehicleInfo>());
                }
                vehicleInfos[time_str].Add(new VehicleInfo
                {
                    v_id = id,
                    v_posX = z, // intentional x and z swap
                    v_posZ = x, // intentional x and z swap
                    v_angle = ang,
                    v_speed = vel
                });
                maxTime = (float)time;
                old_time = time_str;
            }

        }
    }
    public GameObject generateVehicle(bool IsReal,
                        string vehicleObjectName,
                        float x, float z, float angle)
    {
        if (IsReal)
        {
            vehicle = Instantiate(Prefab_Real) as GameObject;
        }
        else
        {
            vehicle = Instantiate(Prefab_Estimated) as GameObject;
        }
        vehicle.name = vehicleObjectName;
        vehicle.transform.SetParent(Vehicles.transform);
        vehicle.transform.position = new Vector3(
            x, vehicle_elevation, z);
        vehicle.transform.rotation = UnityEngine.Quaternion.Euler(
            0, angle, 0);
        return vehicle;
    }

    void attachLabel(GameObject vehicle, VehicleInfo v_info)
    {
        GameObject canvas = GameObject.Find("Canvas");
        GameObject UIlabel = new GameObject("UI_label");
        TextMeshPro TMPcomponent = UIlabel.AddComponent<TextMeshPro>();
        TMPcomponent.text = v_info.v_id.ToString();
        UIlabel.transform.SetParent(vehicle.transform);
        UIlabel.transform.localPosition = new Vector3(0, 0, -10);
        UIlabel.GetComponent<RectTransform>().sizeDelta = new Vector2(5, 5);
        UIlabel.GetComponent<RectTransform>().rotation = UnityEngine.Quaternion.Euler(
            90, 0, 0);
        UIlabel.GetComponent<RectTransform>().localScale = new Vector3(2, 2, 1);
        TMPcomponent.fontSize = 46;
        TMPcomponent.alignment = TextAlignmentOptions.Center;
    }

    void createVehiclePosUI(GameObject vehicle, int nVehicle, string vehicleObjectName)
    {
        float UI_pos_x = -330f;
        float UI_pos_y = 170f;
        float UI_linspacings = 25f;

        GameObject canvas = GameObject.Find("Canvas");
        GameObject UI_vehiclePos = new GameObject(vehicleObjectName + "_pos");
        TextMeshProUGUI TMPcomponent = UI_vehiclePos.AddComponent<TextMeshProUGUI>();
        string coord_xz = " (" + vehicle.transform.position.x +
                          "," + vehicle.transform.position.z + ")";
        TMPcomponent.text = "V" + 
                            nVehicle.ToString() +
                            ":" + coord_xz;
        TMPcomponent.color = new Color32(0, 0, 0, 255);
        UI_vehiclePos.transform.SetParent(canvas.transform);
        UI_vehiclePos.transform.localPosition = new Vector3(
            UI_pos_x,
            UI_pos_y - nVehicle*UI_linspacings,
            0);
        UI_vehiclePos.GetComponent<RectTransform>().sizeDelta = new Vector2(370, 50);
        TMPcomponent.fontSize = 16;
        TMPcomponent.alignment = TextAlignmentOptions.TopLeft;
    }

    void updateVehiclePosUI(GameObject vehicle, string vehicleObjectName)
    {
        GameObject UI_vehiclePos = GameObject.Find(vehicleObjectName + "_pos");
        TextMeshProUGUI TMPcomponent = UI_vehiclePos.GetComponent<TextMeshProUGUI>();
        int correctionIndex = TMPcomponent.text.IndexOf(":") + 1;
        string coord_xz = get2DcoordPairs(vehicle.transform.position);
        TMPcomponent.text = TMPcomponent.text.Substring(0, correctionIndex) + coord_xz;
    }

    string get2DcoordPairs(Vector3 coord)
    {
        string coord_xz = "(" +
                          vehicle.transform.position.x.ToString("F2") +
                          "," +
                          vehicle.transform.position.z.ToString("F2")
                          + ")";
        return coord_xz;
    }

    void moveVehicle(GameObject vehicle, VehicleInfo v_info)
    {
        Vector3 new_pos = new Vector3(
            v_info.v_posX,
            vehicle_elevation,
            v_info.v_posZ);
        vehicle.transform.position = new_pos;
        vehicle.transform.rotation = UnityEngine.Quaternion.Euler(
            0,
            v_info.v_angle,
            0);
    }

    void updateLabelRotation(GameObject vehicle)
    {
        GameObject UIlabel = vehicle.transform.Find("UI_label").gameObject;
        UIlabel.GetComponent<RectTransform>().rotation = UnityEngine.Quaternion.Euler(
            90, 0, 0);
    }
}
