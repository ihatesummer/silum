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

    // message variables
    public const int N = 10;
    public float[,] distance = new float[N,N];
    public float[,] AoA_theta = new float[N,N];
    public float[] x1_true = new float[N];
    public float[] x2_true = new float[N];
    public float[] x1_noisy = new float[N];
    public float[] x2_noisy = new float[N];

    public float[] v1t_true = new float[N];
    public float[] v2t_true = new float[N];


    public float[] q1 = new float[N];
    public float[] q2 = new float[N];

    public float[] phi_q1 = new float[N];
    public float[] phi_q2 = new float[N];

    public float[,] delta1 = new float[N,N];
    public float[,] delta2 = new float[N,N];
    public float[,] phi_delta1 = new float[N,N];
    public float[,] phi_delta2 = new float[N,N];

    public float[] sum_delta_phi1 = new float[N];
    public float[] sum_delta_phi2 = new float[N];

    public float[] sum_1_phi1 = new float[N];
    public float[] sum_1_phi2 = new float[N];

    public float[] phi_x1_old = new float[N];
    public float[] phi_x2_old = new float[N];

    public float[] phi_x1 = new float[N];
    public float[] phi_x2 = new float[N];
    
    public float[] x1 = new float[N];
    public float[] x2 = new float[N];

    public float[] x1_old = new float[N];
    public float[] x2_old = new float[N];

    public float[] vehicle_error = new float[N];

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

                    // x1, x2 initial
                    x1[id] = messages[id].Position_Estimate[0];
                    x2[id] = messages[id].Position_Estimate[1];

                    // For same initial
                    // x1[id] = 12000f;
                    // x2[id] = 6000f;
                }
                else
                {
                    // message updates
                    // GameObject vehicle_mp = GameObject.Find(vehicleObjectName_mp);
                    // messages[id].Position_Estimate = selfMeasurement_position(
                    //         vehicle.transform.position);
                    // moveVehicle_mp(vehicle_mp, messages[id].Position_Estimate, vehicle.transform.rotation);
                }

                vehicle = GameObject.Find(vehicleObjectName);
                if (x1_true[id]!=0){
                    v1t_true[id] = vehicle.transform.position.x - x1_true[id];
                    v2t_true[id] = vehicle.transform.position.z - x2_true[id];
                }
                
                x1_true[id] = vehicle.transform.position.x;
                x2_true[id] = vehicle.transform.position.z;
            }
        

        // Get distance and AoA
        for(int i = 0; i < N; i++){
                for(int j = 0; j < N; j++){
                    distance[i,j]= Mathf.Sqrt(Mathf.Pow(x1_true[i]-x1_true[j],2)+Mathf.Pow(x2_true[i]-x2_true[j],2));
                    distance[i,j]=distance[i,j]+NextGaussian(0, Mathf.Sqrt(variance_distance));
                    AoA_theta[i,j]= Mathf.Atan2(x2_true[i]-x2_true[j],x1_true[i]-x1_true[j]);
                    AoA_theta[i,j]=AoA_theta[i,j]+NextGaussian(0, Mathf.Sqrt(variance_AOA));
                }
        }

        moveVehicle_mp();
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

    void moveVehicle_mp()
    {

        for(int i = 0; i < N; i++){
            q1[i] = x1[i]+v1t_true[i];
            q2[i] = x2[i]+v2t_true[i];

            phi_q1[i] = 10;
            phi_q2[i] = 10;

            phi_x1[i] = 0;
            phi_x2[i] = 0;
        }


        int Lmax = 10;


        for(int l = 0; l < Lmax; l++){
            
            for(int i = 0; i < N; i++){
            phi_x1_old[i] = phi_x1[i];
            phi_x2_old[i] = phi_x2[i];
            x1_old[i] = x1[i];
            x2_old[i] = x2[i];
            }
            

            for(int j = 0; j < N; j++){
                sum_delta_phi1[j] = 0;
                sum_delta_phi2[j] = 0;
                sum_1_phi1[j] = 0;
                sum_1_phi2[j] = 0;
                for(int k = 0; k < N; k++){
                    if(k!=j){
                        delta1[k,j]=x1_old[k]-distance[k,j]*Mathf.Cos(AoA_theta[k,j]);
                        delta2[k,j]=x2_old[k]-distance[k,j]*Mathf.Sin(AoA_theta[k,j]);
                        phi_delta1[k,j]=phi_x1_old[k]+variance_distance*Mathf.Pow(Mathf.Cos(AoA_theta[k,j]),2)+variance_AOA*Mathf.Pow(distance[k,j],2)*Mathf.Pow(Mathf.Sin(AoA_theta[k,j]),2);
                        phi_delta2[k,j]=phi_x2_old[k]+variance_distance*Mathf.Sin(AoA_theta[k,j])*Mathf.Sin(AoA_theta[k,j])+distance[k,j]*distance[k,j]*variance_AOA*Mathf.Cos(AoA_theta[k,j])*Mathf.Cos(AoA_theta[k,j]);
                        sum_delta_phi1[j] += delta1[k,j]/phi_delta1[k,j];
                        sum_delta_phi2[j] += delta2[k,j]/phi_delta2[k,j];
                        sum_1_phi1[j] += (float)1/phi_delta1[k,j];
                        sum_1_phi2[j] += (float)1/phi_delta2[k,j];
                    }
                }
                phi_x1[j] = 1/(1/phi_q1[j]+sum_1_phi1[j]);
                phi_x2[j] = 1/(1/phi_q2[j]+sum_1_phi2[j]);
                x1[j] = phi_x1[j]*(q1[j]/phi_q1[j]+sum_delta_phi1[j]);
                x2[j] = phi_x2[j]*(q2[j]/phi_q2[j]+sum_delta_phi2[j]);
                x1[0] = x1_true[0];
                x2[0] = x2_true[0];
                x1[1] = x1_true[1];
                x2[1] = x2_true[1];
                x1[2] = x1_true[2];
                x2[2] = x2_true[2];
            }
            for(int i = 0; i < N; i++){
                vehicle_error[i] = Mathf.Sqrt(Mathf.Pow(x1_true[i]-x1[i], 2)+Mathf.Pow(x2_true[i]-x2[i], 2));
                }
                // Debug.Log(vehicle_error[0] + " " + vehicle_error[1] + " " + vehicle_error[2]);
                // Debug.Log(delta2[0,0] + " " + delta2[0,1] + " " + delta2[0,2]);
                // Debug.Log(delta2[1,0] + " " + delta2[1,1] + " " + delta2[1,2]);
                // Debug.Log(delta2[2,0] + " " + delta2[2,1] + " " + delta2[2,2]);
        }
        
        for(int i = 0; i < N; i++){
            string vehicleObjectName = "vehicle_no." + i + "_real";
            string vehicleObjectName_mp = "vehicle_no." + i + "_mp";
            GameObject vehicle = GameObject.Find(vehicleObjectName);
            GameObject vehicle_mp = GameObject.Find(vehicleObjectName_mp);
            vehicle_mp.transform.position = new Vector3(x1[i], vehicle_elevation, x2[i]);
            vehicle_mp.transform.rotation = vehicle.transform.rotation;
           
            // For scale up
            // vehicle.transform.localScale = new Vector3(20f, 20f, 20f);
            // vehicle_mp.transform.localScale = new Vector3(20f, 20f, 20f);
        }
    }
}
