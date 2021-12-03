using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


public class MapImporterUI : EditorWindow
{
    bool streets = true;
    static string sumoFilePath;
    static string sumoAddr = "127.0.0.1:4042";

    [MenuItem("Sumo Network/Load Network")]
    public static void ShowWindow()
    {
        sumoFilePath = Application.dataPath + "/SUMO/grid";
        CreateInstance<MapImporterUI>().Show();
    }

    public void OnGUI()
    {
        GUILayout.Label("Map Import Settigns", EditorStyles.boldLabel);
        GUILayout.Space(10);

        streets = EditorGUILayout.Toggle("Street generation", streets);
        GUILayout.Space(4);
        sumoAddr = EditorGUILayout.TextField("SUMO address (IP:Port)", sumoAddr);
        GUILayout.Space(4);
        sumoFilePath = EditorGUILayout.TextField("SUMO location", sumoFilePath);
        GUILayout.Space(4);

        if (GUILayout.Button("Change SUMO location"))
        {
            sumoFilePath = EditorUtility.OpenFolderPanel(
                "Choose the folder containing the SUMO files." +
                "(*.net.xml, *.rou.xml)", Application.dataPath + "/SUMO", "");
            EditorGUILayout.TextField("SUMO location");
        }
        GUILayout.Space(15);
        if (GUILayout.Button("Load map"))
        {
            EditorUtility.DisplayProgressBar("Generation Progress",
                                             "Parsing SUMO files", 0.0f);
            string streetsFileName = "road.net.xml";
            string mapFilePath = sumoFilePath +
                                 "/" +
                                 streetsFileName;
            MapImporter.parseNetXML(mapFilePath);
            EditorUtility.DisplayProgressBar("Generation Progress",
                                             "Generating Street Network",
                                             0.2f);
            MapImporter.drawStreetNetwork();
            EditorUtility.ClearProgressBar();
            this.Close();
        }
        GUILayout.Space(10);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}
