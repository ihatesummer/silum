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
    [Header("Etc")]
    public string mapChoice = "grid";
    private float vehicle_elevation = 0.5f;
    private string mobilityDataFile = "mobility.xml";
    public static Dictionary<float, List<VehicleInfo>> vehicleInfos;
    // set of vehicle information by time
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
        vehicleInfos = new Dictionary<float, List<VehicleInfo>>();
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
        float UI_pos_x = -750f;
        float UI_pos_y = 480f;
        float UI_linspacings = 45f;

        GameObject canvas = GameObject.Find("Canvas");
        GameObject UI_vehiclePos = new GameObject(vehicleObjectName + "_pos");
        TextMeshProUGUI TMPcomponent = UI_vehiclePos.AddComponent<TextMeshProUGUI>();
        string coord_xz = "(" + vehicle.transform.position.x +
                          "," + vehicle.transform.position.z + ")";
        TMPcomponent.text = "Vehicle " + 
                            nVehicle.ToString() +
                            ": " + coord_xz;
        UI_vehiclePos.transform.SetParent(canvas.transform);
        UI_vehiclePos.transform.localPosition = new Vector3(
            UI_pos_x,
            UI_pos_y - nVehicle*UI_linspacings,
            0);
        UI_vehiclePos.GetComponent<RectTransform>().sizeDelta = new Vector2(370, 50);
        TMPcomponent.fontSize = 30;
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
