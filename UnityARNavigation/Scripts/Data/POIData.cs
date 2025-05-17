using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARNavigation.Data
{
    /// <summary>
    /// ScriptableObject that represents a Point of Interest (POI) in the AR Navigation system
    /// </summary>
    [CreateAssetMenu(fileName = "New POI", menuName = "AR Navigation/Point of Interest")]
    public class POIData : ScriptableObject
    {
        [Header("Basic Information")]
        public string poiName;
        public string description;
        public POICategory category;
        public Sprite icon;

        [Header("Location Information")]
        public string buildingID;
        public int floorNumber;
        public Vector3 position;

        [Header("Additional Information")]
        [TextArea(3, 5)]
        public string additionalInfo;
        public List<string> tags = new List<string>();

        // For AR visualization
        public GameObject markerPrefab;
    }

    /// <summary>
    /// Enum defining different categories of POIs
    /// </summary>
    public enum POICategory
    {
        Classroom,
        Office,
        Laboratory,
        Cafeteria,
        Library,
        Restroom,
        EntryExit,
        Stairs,
        Elevator,
        Other
    }
}
