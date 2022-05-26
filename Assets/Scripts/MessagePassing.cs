using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VehicleMsg
{
    public bool IsAnchor {get; set;}
    public Vector2 Position_Estimate {get; set;}
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
        foreach (VehicleInfo v_info in VehicleManager.vehicleInfos[VehicleManager.simulationStep.ToString("F2")])
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
                        Position_Estimate = selfMeasurement_position(
                            vehicle.transform.position)
                    });
                    if (id == 0) { messages[id].IsAnchor = true; }
                    vehicle = generateVehicle_mp(
                        vehicleObjectName_mp,
                        messages[id].Position_Estimate,
                        vehicle.transform.rotation);
                }
                else
                {
                    // message updates
                    GameObject vehicle_mp = GameObject.Find(vehicleObjectName_mp);
                    messages[id].Position_Estimate = selfMeasurement_position(
                            vehicle.transform.position);
                    moveVehicle_mp(vehicle_mp, messages[id].Position_Estimate, vehicle.transform.rotation);
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
        float noiseX = NextGaussian(0, variance_SelfPos / Mathf.Sqrt(2));
        float noiseZ = NextGaussian(0, variance_SelfPos / Mathf.Sqrt(2));

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

    GameObject generateVehicle_mp(string vehicleObjectName,
                        Vector2 pos_estimate, Quaternion rotation)
    {

        vehicle = Instantiate(Prefab_MP) as GameObject;
        vehicle.name = vehicleObjectName;
        GameObject Vehicles = GameObject.Find("Vehicles");
        vehicle.transform.SetParent(Vehicles.transform);
        vehicle.transform.position = new Vector3(
            pos_estimate[0], vehicle_elevation, pos_estimate[1]);
        vehicle.transform.rotation = rotation;
        return vehicle;
    }

    void moveVehicle_mp(GameObject vehicle, Vector2 position, Quaternion rotation)
    {
        vehicle.transform.position = new Vector3(
            position[0], vehicle_elevation, position[1]);
        vehicle.transform.rotation = rotation;
    }
}
