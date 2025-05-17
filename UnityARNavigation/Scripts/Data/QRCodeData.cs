using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ARNavigation.Data
{
    /// <summary>
    /// ScriptableObject that represents QR Code data for the AR Navigation system
    /// </summary>
    [CreateAssetMenu(fileName = "New QR Code", menuName = "AR Navigation/QR Code")]
    public class QRCodeData : ScriptableObject
    {
        [Header("Basic Information")]
        public string qrCodeID;
        public string buildingID;
        public int floorNumber;
        
        [Header("Position Information")]
        public Vector3 worldPosition;    // Position in the real world
        public Vector3 modelPosition;    // Corresponding position on the 3D model
        public Vector3 modelRotation;    // How the model should be oriented relative to the QR code
        
        [Header("Additional Information")]
        public string locationDescription;
        [TextArea(3, 5)]
        public string additionalInfo;
        
        [Header("Debug")]
        public bool isActive = true;
        public Color debugColor = Color.green;
    }
}
