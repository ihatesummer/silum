# if (UNITY_EDITOR)
public class PathConstants
{
    public static string pathPolyBuildingsMaterial = "Materials/PolyBuildingsColor";
    public static string pathRoadMaterial = "Assets/Materials/road_material.mat";
    public static string pathJunctionMaterial = "Assets/Tarbo-CITY/Materials/Floor.mat";

    // SuperSpline Box for Street Mesh
    public static string pathSuperSplinesBox = "Assets/SuperSplinesPro/SuperSplinesSamples/SampleAssets/Models/box.fbx";

    // Vehicles
    public static string pathEgoVehicleWASD = "Assets/3DModelle/3DFahrzeuge/egoVehicle_Peugot_WASD.prefab";
    public static string pathEgoVehicleUDP = "Assets/3DModelle/3DFahrzeuge/egoVehicle_Peugot.prefab";    
    public static string pathForeignVehicle = "simpleVehicle_shiftedRotationAxis"; // (Located in Resources folder, since it's loadad at runtime)  

}
#endif
