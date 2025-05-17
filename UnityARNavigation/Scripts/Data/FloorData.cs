using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARNavigation.Data
{
    /// <summary>
    /// ScriptableObject that represents a Floor in a Building for the AR Navigation system
    /// </summary>
    [CreateAssetMenu(fileName = "New Floor", menuName = "AR Navigation/Floor")]
    public class FloorData : ScriptableObject
    {
        [Header("Basic Information")]
        public string floorID;
        public string floorName;
        public int floorNumber;
        public Sprite floorMap2D;

        [Header("3D Model Information")]
        public GameObject floorModelPrefab;
        
        [Header("Navigation Data")]
        public List<POIData> pointsOfInterest = new List<POIData>();
        public List<NavigationNode> navigationNodes = new List<NavigationNode>();
        
        [System.Serializable]
        public class NavigationNode
        {
            public string nodeID;
            public Vector3 position;
            public List<string> connectedNodes = new List<string>(); // IDs of connected nodes
            public bool isWalkable = true;
            public float costMultiplier = 1f; // For pathfinding, higher means less preferred
        }

        /// <summary>
        /// Find a node by its ID
        /// </summary>
        public NavigationNode FindNodeByID(string id)
        {
            foreach (NavigationNode node in navigationNodes)
            {
                if (node.nodeID == id)
                {
                    return node;
                }
            }
            return null;
        }

        /// <summary>
        /// Find the nearest navigation node to a given position
        /// </summary>
        public NavigationNode FindNearestNode(Vector3 position)
        {
            NavigationNode nearestNode = null;
            float nearestDistance = float.MaxValue;

            foreach (NavigationNode node in navigationNodes)
            {
                float distance = Vector3.Distance(position, node.position);
                if (distance < nearestDistance && node.isWalkable)
                {
                    nearestDistance = distance;
                    nearestNode = node;
                }
            }

            return nearestNode;
        }

        /// <summary>
        /// Find POIs by category
        /// </summary>
        public List<POIData> FindPOIsByCategory(POICategory category)
        {
            List<POIData> result = new List<POIData>();
            foreach (POIData poi in pointsOfInterest)
            {
                if (poi.category == category)
                {
                    result.Add(poi);
                }
            }
            return result;
        }
    }
}
