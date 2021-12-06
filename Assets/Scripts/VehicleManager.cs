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
    public string mapChoice = "grid";
    private string mobilityDataFile = "mobility.xml";
    private Dictionary<float, List<VehicleInfo>> vehicleInfos = 
        new Dictionary<float, List<VehicleInfo>>();
        // set of vehicle information by time 
    void Start()
    {
        string mobilityDataPath = Application.dataPath + "/SUMO/" +
                                  mapChoice + "/" + mobilityDataFile;

        parseMobility(mobilityDataPath);
        foreach( VehicleInfo v_info in vehicleInfos[1])
        {
            Debug.Log( v_info.v_id + ": (" +
                       v_info.v_posX + "," +
                       v_info.v_posZ + ")");
        }

        GameObject vehicle = Instantiate(Prefab_Real) as GameObject;
        vehicle.transform.position = new Vector3(
            212.30f,
            0.5f,
            98.40f);
        vehicle.transform.rotation = UnityEngine.Quaternion.Euler(
            0,
            90f,
            0);
        vehicle.name = "v1_real";
    }

    void Update()
    {
        
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
