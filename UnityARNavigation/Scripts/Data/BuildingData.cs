using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARNavigation.Data
{
    /// <summary>
    /// ScriptableObject that represents a Building in the AR Navigation system
    /// </summary>
    [CreateAssetMenu(fileName = "New Building", menuName = "AR Navigation/Building")]
    public class BuildingData : ScriptableObject
    {
        [Header("Basic Information")]
        public string buildingID;
        public string buildingName;
        public string address;
        public Sprite buildingImage;

        [Header("Building Setup")]
        public List<FloorData> floors = new List<FloorData>();
        public List<QRCodeData> qrCodes = new List<QRCodeData>();
        
        [Header("3D Model Information")]
        public GameObject buildingModelPrefab;
        public Vector3 modelScale = Vector3.one;
        public Vector3 modelRotation = Vector3.zero;

        [Header("Additional Information")]
        [TextArea(3, 5)]
        public string description;

        /// <summary>
        /// Get all POIs across all floors in this building
        /// </summary>
        public List<POIData> GetAllPOIs()
        {
            List<POIData> allPOIs = new List<POIData>();
            foreach (FloorData floor in floors)
            {
                allPOIs.AddRange(floor.pointsOfInterest);
            }
            return allPOIs;
        }

        /// <summary>
        /// Find a POI by its name
        /// </summary>
        public POIData FindPOIByName(string name)
        {
            foreach (FloorData floor in floors)
            {
                foreach (POIData poi in floor.pointsOfInterest)
                {
                    if (poi.poiName.ToLower() == name.ToLower())
                    {
                        return poi;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Find a QR code by its ID
        /// </summary>
        public QRCodeData FindQRCodeByID(string qrID)
        {
            foreach (QRCodeData qrCode in qrCodes)
            {
                if (qrCode.qrCodeID == qrID)
                {
                    return qrCode;
                }
            }
            return null;
        }
    }
}
