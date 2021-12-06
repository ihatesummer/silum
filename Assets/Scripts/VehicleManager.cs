using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;

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
    private float simulationTime = 0f;
    private int simulationStep = 0;
    private GameObject Vehicles;
    private GameObject vehicle;

    void Start()
    {
        string mobilityDataPath = Application.dataPath + "/SUMO/" +
                                  mapChoice + "/" + mobilityDataFile;

        parseMobility(mobilityDataPath);
        Vehicles = new GameObject("Vehicles");
    }

    void Update()
    {
        int maxTime = 60;
        simulationTime += Time.deltaTime;
        if (Mathf.Floor(simulationTime) > simulationStep)
        {
            simulationStep ++;
        }
        
        if (simulationStep <= maxTime)
        {
            foreach( VehicleInfo v_info in vehicleInfos[simulationStep])
            {
                string vehicleObjectName = "vehicle_no." + v_info.v_id + "_real";
                if (! GameObject.Find(vehicleObjectName))
                {
                    vehicle = Instantiate(Prefab_Real) as GameObject;
                    vehicle.name = "vehicle_no." + v_info.v_id + "_real";
                    vehicle.transform.SetParent(Vehicles.transform);
                }
                else
                {
                    vehicle = GameObject.Find(vehicleObjectName);
                }
                vehicle.transform.position = new Vector3(
                    v_info.v_posX,
                    vehicle_elevation,
                    v_info.v_posZ);
                vehicle.transform.rotation = UnityEngine.Quaternion.Euler(
                    0,
                    v_info.v_angle,
                    0);
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
                                vehicleInfos[time].Add( new VehicleInfo {
                                    v_id=id,
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

}
