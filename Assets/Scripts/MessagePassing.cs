using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VehicleMsg
{
    public bool IsAnchor {get; set;}
    public Vector2 XY_mean {get; set;}
    public Vector2 XY_var {get; set;}
    public float AOA_mean {get; set;}
    public float AOA_var {get; set;}
}

public class MessagePassing : MonoBehaviour
{
    [Header("Measurement presets [m, degree]")]
    public float variance_SelfPos;
    public float variance_SelfAngle;
    public float variance_distance;
    public float variance_AOA;
    [Space(10f)]
    [Range(0f, 1f)]
    public float velocity_noise_ratio;
    private Dictionary<int, VehicleMsg> messages;
    public GameObject Prefab_MP;
    private GameObject vehicle;
    private float vehicle_elevation = 0.5f;
    void Start()
    {
        messages = new Dictionary<int, VehicleMsg>();
    }

    void Update()
    {
        if (VehicleManager.nVehicle < 2)
        {
            return;
        }
        foreach (VehicleInfo v_info in VehicleManager.vehicleInfos[VehicleManager.simulationStep])
            {
                int id = v_info.v_id;
                string vehicleObjectName = "vehicle_no." + v_info.v_id + "_real";
                string vehicleObjectName_mp = "vehicle_no." + v_info.v_id + "_mp";
                vehicle = GameObject.Find(vehicleObjectName);
                if (!messages.ContainsKey(id))
                {
                    messages.Add(id, new VehicleMsg
                    {
                        IsAnchor = false,
                        XY_mean = selfMeasurement_position(
                            vehicle.transform.position),
                        XY_var = new Vector2(1, 1),
                        AOA_mean = selfMeasurement_angle(
                            vehicle.transform.eulerAngles.y),
                        AOA_var = 1
                    });
                    if (id == 0) { messages[id].IsAnchor = true; }

                    vehicle = generateVehicle_mp(
                        vehicleObjectName_mp,
                        messages[id].XY_mean[0],
                        messages[id].XY_mean[1],
                        messages[id].AOA_mean);
                }
                else
                {
                    // message updates
                    GameObject vehicle_mp = GameObject.Find(vehicleObjectName_mp);
                    moveVehicle_mp(vehicle_mp, messages[id]);
                }

            }
    }

    public static float NextGaussian()
    {
        float v1, v2, s;
        do {
            v1 = 2.0f * Random.Range(0f,1f) - 1.0f;
            v2 = 2.0f * Random.Range(0f,1f) - 1.0f;
            s = v1 * v1 + v2 * v2;
        } while (s >= 1.0f || s == 0f);
        s = Mathf.Sqrt((-2.0f * Mathf.Log(s)) / s);
    
        return v1 * s;
    }

    public static float NextGaussian(float mean, float standard_deviation)
    {
        return mean + NextGaussian() * standard_deviation;
    }

    Vector2 selfMeasurement_position(Vector3 realPosition)
    {
        float noiseX = variance_SelfPos / Mathf.Sqrt(2);
        float noiseZ = variance_SelfPos / Mathf.Sqrt(2);

        float estimation_x = realPosition.x + noiseX;
        float estimation_z = realPosition.z + noiseZ;

        return new Vector2(estimation_x, estimation_z);
    }

    float selfMeasurement_angle(float realAngle)
    {
        float noiseAngle = NextGaussian(0, variance_SelfAngle);

        float estimation_AOA = realAngle + noiseAngle;

        return estimation_AOA;
    }

    GameObject generateVehicle_mp(
                        string vehicleObjectName,
                        float x, float z, float angle)
    {

        vehicle = Instantiate(Prefab_MP) as GameObject;
        vehicle.name = vehicleObjectName;
        GameObject Vehicles = GameObject.Find("Vehicles");
        vehicle.transform.SetParent(Vehicles.transform);
        vehicle.transform.position = new Vector3(
            x, vehicle_elevation, z);
        vehicle.transform.rotation = UnityEngine.Quaternion.Euler(
            0, angle, 0);
        return vehicle;
    }

    void moveVehicle_mp(GameObject vehicle, VehicleMsg msg)
    {
        vehicle.transform.position = new Vector3(
            msg.XY_mean[0], vehicle_elevation, msg.XY_mean[1]);
    }
}
