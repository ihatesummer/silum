using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
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
    public string mapChoice = "grid";
    public float vehicle_elevation = 0.5f;
    private string mobilityDataFile = "mobility.xml";
    private Dictionary<float, List<VehicleInfo>> vehicleInfos = 
        new Dictionary<float, List<VehicleInfo>>();
        // set of vehicle information by time
    public float simulationTime = 0f;
    public int simulationStep = 0;
    public float LerpTimer = 0f;
    public float maxTime;
    private GameObject Vehicles;
    private GameObject vehicle;

    void Start()
    {
        Application.targetFrameRate = 60;
        string mobilityDataPath = Application.dataPath + "/SUMO/" +
                                  mapChoice + "/" + mobilityDataFile;

        parseMobility(mobilityDataPath);
        Vehicles = new GameObject("Vehicles");
    }

    void Update()
    {
        simulationTime += Time.deltaTime;
        LerpTimer += Time.deltaTime;

        if (simulationTime > simulationStep + 1)
        {
            simulationStep++;
            LerpTimer = 0;
        }

        if (simulationStep <= maxTime - 1)
        {
            foreach (VehicleInfo v_info in vehicleInfos[simulationStep])
            {
                string vehicleObjectName = "vehicle_no." + v_info.v_id + "_real";
                if (!GameObject.Find(vehicleObjectName))
                {
                    vehicle = generateVehicle(true, vehicleObjectName, v_info);
                    attachLabel(vehicle, v_info);
                }
                else
                {
                    vehicle = GameObject.Find(vehicleObjectName);
                    moveVehicle(vehicle, v_info);
                    // in case vehicle rotated, correct the label rotation
                    updateLabelRotation(vehicle);
                }
            }
        }
    }

    void parseMobility(string mobilityDataPath)
    {
        XmlReaderSettings xmlReaderSetting =
            new XmlReaderSettings();
        xmlReaderSetting.IgnoreComments = true;
        xmlReaderSetting.IgnoreWhitespace = true;

        using (XmlReader reader = XmlReader.Create(
            mobilityDataPath, xmlReaderSetting))
        {
            reader.ReadToFollowing("timestep");
            while (!reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name.ToString() == "timestep")
                    {
                        float time = float.Parse(
                            reader.GetAttribute("time"));
                        maxTime = time;
                        vehicleInfos.Add(time, new List<VehicleInfo>());

                        XmlReader subReader = reader.ReadSubtree();
                        while (!subReader.EOF)
                        {
                            if (subReader.Name.ToString() == "vehicle")
                            {
                                int id = (int)float.Parse(
                                    subReader.GetAttribute("id"));
                                float posX = float.Parse(
                                    subReader.GetAttribute("x"));
                                float posZ = float.Parse(
                                    subReader.GetAttribute("y"));
                                float angle = float.Parse(
                                    subReader.GetAttribute("angle"));
                                float speed = float.Parse(
                                    subReader.GetAttribute("speed"));
                                vehicleInfos[time].Add(new VehicleInfo
                                {
                                    v_id = id,
                                    v_posX = posX,
                                    v_posZ = posZ,
                                    v_angle = angle,
                                    v_speed = speed
                                });
                            }
                            subReader.Read();
                        }
                        subReader.Close();
                    }
                    else reader.Skip();
                }
                reader.Read();
            }
        }
    }
    GameObject generateVehicle(bool IsReal,
                        string vehicleObjectName,
                        VehicleInfo v_info)
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
            v_info.v_posX,
            vehicle_elevation,
            v_info.v_posZ);
        vehicle.transform.rotation = UnityEngine.Quaternion.Euler(
            0,
            v_info.v_angle,
            0);
        return vehicle;
    }

    void attachLabel(GameObject vehicle, VehicleInfo v_info)
    {
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

    void moveVehicle(GameObject vehicle, VehicleInfo v_info)
    {
        Vector3 previous_pos = new Vector3(
            vehicleInfos[simulationStep][v_info.v_id].v_posX,
            vehicle_elevation,
            vehicleInfos[simulationStep][v_info.v_id].v_posZ);
        Vector3 next_pos = new Vector3(
            vehicleInfos[simulationStep + 1][v_info.v_id].v_posX,
            vehicle_elevation,
            vehicleInfos[simulationStep + 1][v_info.v_id].v_posZ);
        vehicle.transform.position = Vector3.Lerp(
            previous_pos,
            next_pos,
            LerpTimer);
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
