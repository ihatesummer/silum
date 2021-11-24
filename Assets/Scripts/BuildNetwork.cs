#undef terrain
#undef trees
#undef grass
#undef buildings
#undef randomBuildings
#undef polyBuildings
#undef cameraScriptEnable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;


public class BuildNetwork : EditorWindow
{
    bool terrain = false;
    bool trees = false;
    bool grass = false;
    bool buildings = false;
    bool randomBuildings = false;
    bool polyBuildings = false;
    string buildingsCount = "300";
    bool cameraScriptEnable = false;

    bool streets = true;

    static string sumoFilePath;
    static string sumoAddr = "127.0.0.1:4042";

    [MenuItem("Sumo Network/Load Network")]
    public static void ShowWindow()
    {
        sumoFilePath = Application.dataPath;
        sumoFilePath += "/Scripts/environment";
        //BuildNetwork window = (BuildNetwork)EditorWindow.GetWindow(typeof(BuildNetwork));
        //window.Show();
        CreateInstance<BuildNetwork>().Show();
    }

    public void OnGUI()
    {
        GUILayout.Label("Terrain + Street Network Generation Settigns", EditorStyles.boldLabel);
        terrain = EditorGUILayout.Toggle("Generate Terrain?", terrain);
        trees = EditorGUILayout.Toggle("Generate Trees?", trees);
        grass = EditorGUILayout.Toggle("Generate grass?", grass);
        buildings = EditorGUILayout.Toggle("Generate Buildings?", buildings);
        polyBuildings = EditorGUILayout.Toggle("| -> PolyBuildings?", polyBuildings);
        randomBuildings = EditorGUILayout.Toggle("| -> Random Buildings?", randomBuildings);
        buildingsCount = EditorGUILayout.TextField("Buildings Count?", buildingsCount);
        GUILayout.Space(16);

        streets = EditorGUILayout.Toggle("Generate Streets?", streets);

        sumoAddr = EditorGUILayout.TextField("SUMO IP:Port", sumoAddr);
        GUILayout.Space(16);

        sumoFilePath = EditorGUILayout.TextField("SUMO Files", sumoFilePath);
        if (GUILayout.Button("Change SUMO Files"))
        {
            sumoFilePath = EditorUtility.OpenFolderPanel("Chose the folder containing the SUMO files" +
                " (map.edg.xml, map.nod.xml, map.net.xml, map.rou.xml)", Application.dataPath, "");
            EditorGUILayout.TextField("SUMO Files", sumoFilePath);
        }
        GUILayout.Space(16);
        GUILayout.Label("Processing", EditorStyles.boldLabel);
        if (GUILayout.Button("Start"))
        {
            try
            {
                GameObject MainCamera = GameObject.FindGameObjectWithTag("MainCamera");
                //MainCamera.SetActive(false);
                MainCamera.SetActive(true);
            }
            catch (Exception e)
            {
                MonoBehaviour.print(e.GetBaseException());
//                Debug.Log(e.GetBaseException());

            }

            if (streets)
            {
                EditorUtility.DisplayProgressBar("Generation Progress", "Parsing SUMO files", 0.0f);
                //ImportAndGenerate.parseXMLfiles(sumoFilePath);
                SumoNetLoader.parseXMLfiles(sumoFilePath);

                EditorUtility.DisplayProgressBar("Generation Progress", "Generating Street Network", 0.2f);
                //ImportAndGenerate.drawStreetNetwork();                
                SumoNetLoader.drawStreetNetwork();                

            }
#if terrain
            if (terrain)
            {
                EditorUtility.DisplayProgressBar("Generation Progress", "Generating Terrain, Trees, Grass", 0.3f);
                ImportAndGenerate.generateTerrain(trees, grass);                
            }
#endif
#if buildings
            if (buildings)
            {
                EditorUtility.DisplayProgressBar("Generation Progress", "Generating buildings", 0.4f);
                if(randomBuildings)
                    ImportAndGenerate.generatingBuildings(Convert.ToInt32(buildingsCount));
                else if(polyBuildings)
                    ImportAndGenerate.generatingPolyBuildings();
            }
#endif
#if cameraScriptEnable
            if (cameraScriptEnable)
            {
                EditorUtility.DisplayProgressBar("Generation Progress", "Generating cameras", 1.0f);
                ImportAndGenerate.addCameraScript();
            }
#endif

            EditorUtility.ClearProgressBar();
            this.Close();
                
        }

        GUILayout.Space(16);
    }

    void OnInspectorUpdate()
    {
        Repaint();
    }
}
