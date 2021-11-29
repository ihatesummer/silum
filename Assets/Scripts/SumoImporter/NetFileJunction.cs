#if true
using System;
using System.Collections.Generic;

namespace Assets.Scripts.SumoImporter.NetFileComponents
{
    public class NetFileJunction
    {
        public string id;
        public junctionTypeType type;
        public float x;
        public float y;
        public float z;
        public List<NetFileLane> incLanes;
        public List<double[]> shape;

        public NetFileJunction(string id, junctionTypeType type, float x, float y, float z, string incLanes, string shape)
        {
            this.id = id;
            this.type = type;
            this.x = x;
            this.y = y;
            this.z = z;

            // Get incoming Lanes
            // UnityEngine.Debug.Log("NetFileJunction.cs incLanes passed: " + incLanes);
            foreach (string laneName in incLanes.Split(' '))
            {
                NetFileLane lane = new NetFileLane(laneName);
                // UnityEngine.Debug.Log("Incoming lane to NetFileJunction.cs: " + lane.id);
                if(!MapLoader.lanes.ContainsKey(lane.id))
                {
                    MapLoader.lanes.Add(lane.id, lane);
                    // UnityEngine.Debug.Log(lane.id + " added to MapLoader.lanes");
                }
            }

            // Get shape coordinates as List of tuple-arrays
            this.shape = new List<double[]>();
            foreach(string stringPiece in shape.Split(' '))
            {
                double xC = Convert.ToDouble(stringPiece.Split(',')[0]);
                double yC = Convert.ToDouble(stringPiece.Split(',')[1]);
                this.shape.Add(new double[] { xC, yC });
            }
        }
    }
}
#endif