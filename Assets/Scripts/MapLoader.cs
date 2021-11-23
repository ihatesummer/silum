using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.SumoImporter.NetFileComponents;
using System;
using System.Xml;

public class MapLoader
{
    static GameObject network;
    public static string streetsFileName = "grid.net.xml";
    public static Dictionary<string, NetFileJunction> junctions;
    public static Dictionary<string, NetFileEdge> edges;

    static string mapFilePath;
    static float map_x_min;
    static float map_x_max;
    static float map_y_min;
    static float map_y_max;
    public static List<Vector3[]> polygons;

    public static void parseXML(string sumoPath)
    {
        network = new GameObject("StreetNetwork");
        junctions = new Dictionary<string, NetFileJunction>();
        edges = new Dictionary<string, NetFileEdge>();
        parseJunctions(sumoPath);
        parseEdges(sumoPath);
    }

    public static void parseJunctions(string sumoPath)
    {
        string mapFilePath = sumoPath +
                             "/" +
                             streetsFileName;

        XmlReaderSettings xmlReaderSetting =
            new XmlReaderSettings();
        xmlReaderSetting.IgnoreComments = true;
        xmlReaderSetting.IgnoreWhitespace = true;

        using (XmlReader reader = XmlReader.Create(mapFilePath,
                                                   xmlReaderSetting))
        {
            reader.ReadToFollowing("junction");
            while (reader.Read() && !reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name.ToString())
                    {
                        case "junction":
                            if (reader.HasAttributes)
                            {
                                if (reader.GetAttribute("type") != "internal")
                                {
                                    string junctionId = reader.GetAttribute("id");
                                    junctionTypeType type = 
                                        (junctionTypeType)Enum.Parse(typeof(junctionTypeType),
                                                                     reader.GetAttribute("type"));
                                    float x = float.Parse(reader.GetAttribute("x"));
                                    float y = float.Parse(reader.GetAttribute("y"));
                                    float z = 0f;
                                    string incLanes = reader.GetAttribute("incLanes");
                                    string junctionShape = reader.GetAttribute("shape");

                                    NetFileJunction junction = new NetFileJunction(junctionId,
                                                                                   type,
                                                                                   x, y, z,
                                                                                   incLanes,
                                                                                   junctionShape);

                                    if (!junctions.ContainsKey(junction.id))
                                    {
                                        junctions.Add(junction.id, junction);
                                    }
                                    }
                            }
                            break;

                        default:
                            reader.Skip();
                            break;
                    } // end of switch statement
                } // end of if statement
            }
        }
    }

    public static void parseEdges(string sumoPath)
    {
        string mapFilePath = sumoPath +
                             "/" +
                             streetsFileName;

        XmlReaderSettings xmlReaderSetting =
            new XmlReaderSettings();
        xmlReaderSetting.IgnoreComments = true;
        xmlReaderSetting.IgnoreWhitespace = true;
        using (XmlReader reader = XmlReader.Create(mapFilePath,
                                                   xmlReaderSetting))
        {
            reader.ReadToFollowing("net");
            while (reader.Read() && !reader.EOF)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name.ToString())
                    {
                        case "location":
                            if (reader.HasAttributes)
                            {
                                string boundary = reader.GetAttribute("convBoundary");
                                string[] boundaries = boundary.Split(',');
                                map_x_min = float.Parse(boundaries[0]);
                                map_y_min = float.Parse(boundaries[1]);
                                map_x_max = float.Parse(boundaries[2]);
                                map_y_max = float.Parse(boundaries[3]);
                                UnityEngine.Debug.Log("Map boundaries:" +
                                                      "\nxmin: " + map_x_min +
                                                      "\tymin: " + map_y_min +
                                                      "\nxmax: " + map_x_max +
                                                      "\tymax: " + map_y_max);
                            }
                            break;
                        case "edge":
                            if (reader.HasAttributes)
                            {
                                if (reader.GetAttribute("function") != "internal")
                                {
                                    string edgeId = reader.GetAttribute("id");
                                    string from = reader.GetAttribute("from");
                                    string to = reader.GetAttribute("to");
                                    string priority = reader.GetAttribute("priority");
                                    string edgeShape = "null";

                                    NetFileEdge edge = new NetFileEdge(edgeId,
                                                                       from, to,
                                                                       priority,
                                                                       edgeShape);
                                    if (!edges.ContainsKey(edgeId))
                                    {
                                        edges.Add(edgeId, edge);
                                    }

                                    XmlReader innerReader = reader.ReadSubtree();
                                    while (innerReader.Read())
                                    {
                                        if ((innerReader.Name.ToString()=="lane") &&
                                            innerReader.HasAttributes)
                                        {
                                            string laneId = innerReader.GetAttribute("id");
                                            string index = innerReader.GetAttribute("index");
                                            float speed = float.Parse(innerReader.GetAttribute("speed"));
                                            float length = float.Parse(innerReader.GetAttribute("length"));
                                            string laneShape = innerReader.GetAttribute("shape");
                                            edges[edgeId].addLane(laneId, index, speed, length, laneShape);
                                        }
                                    }
                                    innerReader.Close();
                                }
                            }
                            break;
                        case "junction":
                            reader.Skip();
                            break;
                        default:
                            reader.Skip();
                            break;
                    } // end of switch(reader.Name.ToString()) statement
                } // end of if(reader.NodeType == XmlNodeType.Element) statement
            }
        }
    }

    public static void drawStreetNetwork()
    {
        polygons = new List<Vector3[]>();
        bool linearOption = true;
        int laneCounter = 0;
        int streetLightCounter = 0;

        // (1) Draw all Edges ------------------------------------
        UnityEngine.Debug.Log("Inserting 3d Streets");

        foreach (NetFileEdge e in edges.Values)
        {
            int edgeCounter = 0;
            GameObject spline = new GameObject("StreetSegment_" + laneCounter++);
            spline.transform.SetParent(network.transform);

            Spline splineObject = spline.AddComponent<Spline>();

            if (linearOption)
                splineObject.interpolationMode = Spline.InterpolationMode.Linear;
            else
                splineObject.interpolationMode = Spline.InterpolationMode.BSpline;

            foreach (NetFileLane l in e.getLanes())
            {
                foreach (double[] coordPair in l.shape)
                {
                    // Add Node
                    GameObject splineNode = new GameObject("Node_" + edgeCounter++);
                    splineNode.transform.SetParent(spline.transform);
                    SplineNode splineNodeObject = splineNode.AddComponent<SplineNode>();
                    splineNode.transform.position = new Vector3((float)coordPair[0]- xmin, 0, (float)coordPair[1]-ymin);
                    splineObject.splineNodesArray.Add(splineNodeObject);
                }

                // Add meshes
                Material material = AssetDatabase.LoadAssetAtPath<Material>(PathConstants.pathRoadMaterial);
                // --------- 20.10.13 ---------//
                material.shader = Shader.Find("Standard");
                material.SetFloat("_Glossiness", 0f);
                /////////////////////////////////
                MeshRenderer mRenderer = mRenderer = spline.GetComponent<MeshRenderer>();
                if (mRenderer == null)
                {
                    mRenderer = spline.AddComponent<MeshRenderer>();
                }
                mRenderer.material = material;
                // --------- 20.10.13 ---------//
                mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mRenderer.receiveShadows = false;
                /////////////////////////////////


                SplineMesh sMesh = spline.AddComponent<SplineMesh>();
                sMesh.spline = splineObject;
                sMesh.baseMesh = AssetDatabase.LoadAssetAtPath<Mesh>(PathConstants.pathSuperSplinesBox);
                sMesh.startBaseMesh = AssetDatabase.LoadAssetAtPath<Mesh>(PathConstants.pathSuperSplinesBox);
                sMesh.endBaseMesh = AssetDatabase.LoadAssetAtPath<Mesh>(PathConstants.pathSuperSplinesBox);
                sMesh.uvScale = new Vector2(uvScaleU, uvScaleV);
                sMesh.xyScale = new Vector2(meshScaleX, 0);
                //sMesh.segmentCount = 500;
                //sMesh.segmentCount = 500 * 4;
                sMesh.segmentCount = 50;


                // (1.1) Add Lanes to polygon list for tree placement check
                for (int i = 0; i < l.shape.Count - 1; i++)
                {
                    double length = Math.Sqrt(Math.Pow(l.shape[i][0]-xmin - (l.shape[i + 1][0]-xmin), 2) + Math.Pow(l.shape[i][1]-ymin - (l.shape[i + 1][1]-ymin), 2));
                    // Calc the position (in line with the lane)
                    float x1 = (float)l.shape[i][0]-xmin;
                    float y1 = (float)l.shape[i][1]-ymin;
                    float x2 = (float)l.shape[i + 1][0] - xmin;
                    float y2 = (float)l.shape[i + 1][1] - ymin;
                    double Dx = x2 - x1;
                    double Dy = y2 - y1;
                    double D = Math.Sqrt(Dx * Dx + Dy * Dy);
                    double W = 10;
                    Dx = 0.5 * W * Dx / D;
                    Dy = 0.5 * W * Dy / D;
                    Vector3[] polygon = new Vector3[] { new Vector3((float)(x1 - Dy), 0, (float)(y1 + Dx)),
                                                    new Vector3((float)(x1 + Dy), 0, (float)(y1 - Dx)),
                                                    new Vector3((float)(x2 + Dy), 0, (float)(y2 - Dx)),
                                                    new Vector3((float)(x2 - Dy), 0, (float)(y2 + Dx)) };
                    polygons.Add(polygon);


                    // (2) Add Street Lamps (only if long enough)
                    if (length >= minLengthForStreetLamp)
                    {
                        float angle = Mathf.Atan2(y2 - y1, x2 - x1) * 180 / Mathf.PI;

                        // Allway located at the middle of a street
                        double ratioRotPoint = 0.5;
                        double ratio = 0.5 + streeLampDistance / length;

                        float xDest = (float)((1 - ratio) * x1 + ratio * x2);
                        float yDest = (float)((1 - ratio) * y1 + ratio * y2);

                        float xRotDest = (float)((1 - ratioRotPoint) * x1 + ratioRotPoint * x2);
                        float yRotDest = (float)((1 - ratioRotPoint) * y1 + ratioRotPoint * y2);

#if streetLamp
                        GameObject streetLampPrefab = AssetDatabase.LoadMainAssetAtPath(PathConstants.pathLaterne) as GameObject;
                        GameObject streetLamp = GameObject.Instantiate(streetLampPrefab, new Vector3(xDest, 0, yDest), Quaternion.Euler(new Vector3(0, 0, 0)));
                        streetLamp.name = "StreetLight_" + streetLightCounter++;
                        streetLamp.transform.SetParent(network.transform);
                        streetLamp.transform.RotateAround(new Vector3(xRotDest, 0, yRotDest), Vector3.up, -90.0f);
                        streetLamp.transform.Rotate(Vector3.up, -angle);
#endif
                    }
                }

            }


        }

        // (3) Draw all Junction areas ------------------------------------
        MonoBehaviour.print("Inserting 3d Junctions");

        int junctionCounter = 0;
        foreach (NetFileJunction j in junctions.Values)
        {
            List<int> indices = new List<int>();

            Vector2[] vertices2D = new Vector2[j.shape.Count];
            for (int i = 0; i < j.shape.Count; i++)
            {
                vertices2D[i] = new Vector3((float)(j.shape[i])[0] - xmin, (float)(j.shape[i])[1] - ymin);
            }

            // Use the triangulator to get indices for creating triangles
            Triangulator tr = new Triangulator(vertices2D);
            List<int> bottomIndices = new List<int>(tr.Triangulate());
            indices.AddRange(bottomIndices);


            // Create the Vector3 vertices
            Vector3[] vertices = new Vector3[vertices2D.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices2D[i].x, 0, vertices2D[i].y);
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = indices.ToArray();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            Bounds bounds = mesh.bounds;
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                uvs[i] = new Vector2(vertices[i].x / bounds.size.x, vertices[i].z / bounds.size.z);
            }
            mesh.uv = uvs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Set up game object with mesh;
            GameObject junction3D = new GameObject("junction_" + junctionCounter++);
            //MeshRenderer r = (MeshRenderer)junction3D.AddComponent(typeof(MeshRenderer));
            //--- 2020 10 08 ---//
            MeshRenderer r = junction3D.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            // ----------------//
            //Material material = Resources.Load<Material>(PathConstants.pathJunctionMaterial);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(PathConstants.pathJunctionMaterial);
            r.material = material;
            //--- 2020 10 13 ---//
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            // ----------------//
            MeshFilter filter = junction3D.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = mesh;
            junction3D.transform.SetParent(network.transform);

            // (3.1) Add junctions to polygon list for tree placement check
            polygons.Add(vertices);
        }

        // (4) Draw Traffic Lights
#if trafficLight
        MonoBehaviour.print("Inserting 3d Traffic Lights");

        foreach (NetFileJunction j in junctions.Values)
        {
            if (j.type == junctionTypeType.traffic_light)
            {
                int index = 0;
                foreach (NetFileLane l in j.incLanes)
                {
                    // Calc the position (in line with the lane)
                    float x1 = (float)l.shape[0][0] - xmin;
                    float y1 = (float)l.shape[0][1] - ymin;
                    float x2 = (float)l.shape[1][0] - xmin;
                    float y2 = (float)l.shape[1][1] - ymin;
                    float length = (float)Math.Sqrt(Math.Pow(y2 - y1, 2) + Math.Pow(x2 - x1, 2));
                    float angle = Mathf.Atan2(y2 - y1, x2 - x1) * 180 / Mathf.PI;

                    double ratio = (length - trafficLightDistance) / length;

                    float xDest = (float)((1 - ratio) * x1 + ratio * x2);
                    float yDest = (float)((1 - ratio) * y1 + ratio * y2);

                    // Insert the 3d object, rotate from lane 90° to the right side and then orientate the traffic light towards the vehicles
                    //--- 2020 10 08 ---//
                    //GameObject trafficLightPrefab = AssetDatabase.LoadMainAssetAtPath(PathConstants.pathTrafficLight) as GameObject;
                    GameObject trafficLightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PathConstants.pathTrafficLight);
                    // ----------------//
                    GameObject trafficLight = GameObject.Instantiate(trafficLightPrefab, new Vector3(xDest, 0, yDest), Quaternion.Euler(new Vector3(0, 0, 0)));
                    trafficLight.name = "TrafficLight_" + j.id;
                    trafficLight.transform.SetParent(network.transform);
                    trafficLight.transform.RotateAround(new Vector3(x2, 0, y2), Vector3.up, -90.0f);
                    trafficLight.transform.Rotate(Vector3.up, -angle);

                    // Insert traffic light index as empty GameObject into traffic light
                    GameObject TLindex = new GameObject("index");
                    GameObject TLindexVal = new GameObject(Convert.ToString(index++));
                    TLindexVal.transform.SetParent(TLindex.transform);
                    TLindex.transform.SetParent(trafficLight.transform);
                }
            }
        }
#endif
    }
}
