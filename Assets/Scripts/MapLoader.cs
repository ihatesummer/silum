using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Assets.Scripts.SumoImporter.NetFileComponents;
using System;
using System.Xml;

public class MapLoader
{
    static GameObject network;
    public static Dictionary<string, NetFileJunction> junctions;
    public static Dictionary<string, NetFileEdge> edges;
    public static Dictionary<string, NetFileLane> lanes;

    static string mapFilePath;
    static float map_x_min;
    static float map_x_max;
    static float map_y_min;
    static float map_y_max;
    public static List<Vector3[]> polygons;
    static float uvScaleV = 50;
    static float uvScaleU = 1;
    static float meshScaleX = 3.3f;

    public static void parseNetXML(string mapFilePath)
    {
        network = new GameObject("StreetNetwork");
        lanes = new Dictionary<string, NetFileLane>();
        junctions = new Dictionary<string, NetFileJunction>();
        edges = new Dictionary<string, NetFileEdge>();
        parseJunctions(mapFilePath);
        parseEdges(mapFilePath);
    }

    public static void parseJunctions(string mapFilePath)
    {
        XmlReaderSettings xmlReaderSetting =
            new XmlReaderSettings();
        xmlReaderSetting.IgnoreComments = true;
        xmlReaderSetting.IgnoreWhitespace = true;

        using (XmlReader reader = XmlReader.Create(
            mapFilePath, xmlReaderSetting))
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
                    } // end of switch (reader.Name.ToString())
                } // end of reader.NodeType == XmlNodeType.Element
            } // end of while (reader.Read() && !reader.EOF)
        }
    }

    public static void parseEdges(string mapFilePath)
    {
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
                                                      "\nmap_x_min: " + map_x_min +
                                                      "\tmap_y_min: " + map_y_min +
                                                      "\nmap_x_max: " + map_x_max +
                                                      "\tmap_y_max: " + map_y_max);
                            }
                            break;
                        case "edge":
                            string edgeId = "null";
                            string laneId = "null";
                            if (reader.HasAttributes)
                            {
                                if (reader.GetAttribute("function") != "internal")
                                {
                                    try
                                    {
                                        string edgeShape = reader.GetAttribute("shape");
                                    }
                                    catch (KeyNotFoundException)
                                    {
                                        UnityEngine.Debug.LogWarning("not found error: edge attribute - shape ");
                                    }
                                    finally
                                    {
                                        string edgeShape = "null";
                                        edgeId = reader.GetAttribute("id");
                                        string from = reader.GetAttribute("from");
                                        string to = reader.GetAttribute("to");
                                        string priority = reader.GetAttribute("priority");
                                        NetFileEdge edge = new NetFileEdge(edgeId,
                                                                        from, to,
                                                                        priority,
                                                                        edgeShape);
                                        if (!edges.ContainsKey(edgeId))
                                        {
                                            Debug.Log("Added edge " + edgeId);
                                            edges.Add(edgeId, edge);
                                        }

                                        XmlReader innerReader = reader.ReadSubtree();
                                        while (innerReader.Read())
                                        {
                                            switch(innerReader.Name.ToString())
                                            {
                                                case "lane":
                                                    if(innerReader.HasAttributes)
                                                    {
                                                        laneId = innerReader.GetAttribute(
                                                            "id");
                                                        string index = innerReader.GetAttribute(
                                                            "index");
                                                        float speed = float.Parse(
                                                            innerReader.GetAttribute("speed"));
                                                        float length = float.Parse(
                                                            innerReader.GetAttribute("length"));
                                                        string laneShape = innerReader.GetAttribute(
                                                            "shape");
                                                        edges[edgeId].addLane(
                                                            laneId, index, speed, length, laneShape);
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                        innerReader.Close();
                                    }
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
        bool bLinearInterpolation = true;
        int laneCounter = 0;
        // (1) Draw all Edges
        UnityEngine.Debug.Log("Inserting street segments...");
        foreach (NetFileEdge e in edges.Values)
        {
            int nodeCounter = 0;
            GameObject streetSegment = new GameObject("StreetSegment_" + laneCounter++);
            streetSegment.transform.SetParent(network.transform);

            Spline splineComponent = streetSegment.AddComponent<Spline>();

            if (bLinearInterpolation)
                splineComponent.interpolationMode = Spline.InterpolationMode.Linear;
            else
                splineComponent.interpolationMode = Spline.InterpolationMode.BSpline;

            foreach (NetFileLane l in e.getLanes())
            {
                foreach (double[] coordPair in l.shape)
                {
                    GameObject splineNode = new GameObject("Node_" + nodeCounter++);
                    splineNode.transform.SetParent(streetSegment.transform);
                    SplineNode splineNodeComponent = splineNode.AddComponent<SplineNode>();
                    splineNode.transform.position = new Vector3((float)coordPair[0]- map_x_min,
                                                                0,
                                                                (float)coordPair[1] - map_y_min);
                    splineComponent.splineNodesArray.Add(splineNodeComponent);
                }

                // Add meshes
                Material material = AssetDatabase.LoadAssetAtPath<Material>(
                    PathConstants.pathRoadMaterial);
                material.shader = Shader.Find("Standard");
                material.SetFloat("_Glossiness", 0f);
                MeshRenderer mRenderer = streetSegment.GetComponent<MeshRenderer>();
                if (mRenderer == null)
                {
                    mRenderer = streetSegment.AddComponent<MeshRenderer>();
                }
                mRenderer.material = material;
                mRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mRenderer.receiveShadows = false;

                SplineMesh sMesh = streetSegment.AddComponent<SplineMesh>();
                sMesh.spline = splineComponent;
                sMesh.baseMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                    PathConstants.pathSuperSplinesBox);
                sMesh.startBaseMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                    PathConstants.pathSuperSplinesBox);
                sMesh.endBaseMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
                    PathConstants.pathSuperSplinesBox);
                sMesh.uvScale = new Vector2(uvScaleU, uvScaleV);
                sMesh.xyScale = new Vector2(meshScaleX, 0);
                sMesh.segmentCount = 50;

                for(int i = 0; i < l.shape.Count - 1; i++)
                {
                    // (1.1) Add Lanes to polygon list for tree placement check
                    double length = Math.Sqrt(
                        Math.Pow(l.shape[i][0] - l.shape[i + 1][0], 2) +
                        Math.Pow(l.shape[i][1] - l.shape[i + 1][1], 2));
                    // Calc the position (in line with the lane)
                    float x1 = (float)l.shape[i][0] - map_x_min;
                    float y1 = (float)l.shape[i][1] - map_y_min;
                    float x2 = (float)l.shape[i + 1][0] - map_x_min;
                    float y2 = (float)l.shape[i + 1][1] - map_y_min;
                    double Dx = x2 - x1;
                    double Dy = y2 - y1;
                    double D = Math.Sqrt(Dx * Dx + Dy * Dy);
                    double W = 5;
                    Dx = W * Dx / D;
                    Dy = W * Dy / D;
                    Vector3[] polygon = new Vector3[] {
                        new Vector3((float)(x1 - Dy), 0, (float)(y1 + Dx)),
                        new Vector3((float)(x1 + Dy), 0, (float)(y1 - Dx)),
                        new Vector3((float)(x2 + Dy), 0, (float)(y2 - Dx)),
                        new Vector3((float)(x2 - Dy), 0, (float)(y2 + Dx))};
                    polygons.Add(polygon);
                } // end of for(int i = 0; i < l.shape.Count - 1; i++) statement
            } // end of foreach (NetFileLane l in e.getLanes()) statement
        } // end of foreach (NetFileEdge e in edges.Values) statement

        // (3) Draw all Junction areas ------------------------------------
        UnityEngine.Debug.Log("Inserting junctions...");

        int junctionCounter = 0;
        foreach (NetFileJunction j in junctions.Values)
        {
            List<int> indices = new List<int>();
            Vector2[] vertices2D = new Vector2[j.shape.Count];
            for (int i = 0; i < j.shape.Count; i++)
            {
                vertices2D[i] = new Vector3((float)(j.shape[i])[0] - map_x_min,
                                            (float)(j.shape[i])[1] - map_y_min);
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
            MeshRenderer r = junction3D.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
            Material material = AssetDatabase.LoadAssetAtPath<Material>(PathConstants.pathJunctionMaterial);
            r.material = material;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            MeshFilter filter = junction3D.AddComponent(typeof(MeshFilter)) as MeshFilter;
            filter.mesh = mesh;
            junction3D.transform.SetParent(network.transform);

            // (3.1) Add junctions to polygon list for tree placement check
            polygons.Add(vertices);
        }
    }
}
