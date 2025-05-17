using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ARNavigation.Data;

namespace ARNavigation.AR
{
    /// <summary>
    /// Core AR Manager class that handles AR Foundation integration and AR session management
    /// </summary>
    [RequireComponent(typeof(ARSession))]
    [RequireComponent(typeof(ARSessionOrigin))]
    [RequireComponent(typeof(ARPlaneManager))]
    [RequireComponent(typeof(ARRaycastManager))]
    public class ARManager : MonoBehaviour
    {
        [Header("AR References")]
        public ARSession arSession;
        public ARSessionOrigin arSessionOrigin;
        public ARPlaneManager arPlaneManager;
        public ARRaycastManager arRaycastManager;
        public Camera arCamera;

        [Header("Building Management")]
        public BuildingData currentBuilding;
        public GameObject currentBuildingObject;
        public Transform buildingParent;

        [Header("AR Settings")]
        public bool autoDetectPlanes = true;
        public float planeDetectionThreshold = 0.02f;
        public LayerMask placementLayerMask;

        [Header("Debug")]
        public bool showDebugInfo = true;
        public bool showPlanes = true;

        private bool isInitialized = false;
        private bool isModelPlaced = false;

        // Singleton instance
        public static ARManager Instance { get; private set; }

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Get AR components if not assigned
            if (arSession == null) arSession = GetComponent<ARSession>();
            if (arSessionOrigin == null) arSessionOrigin = GetComponent<ARSessionOrigin>();
            if (arPlaneManager == null) arPlaneManager = GetComponent<ARPlaneManager>();
            if (arRaycastManager == null) arRaycastManager = GetComponent<ARRaycastManager>();
            if (arCamera == null) arCamera = arSessionOrigin.camera;
            
            // Create building parent if not assigned
            if (buildingParent == null)
            {
                GameObject buildingParentObj = new GameObject("BuildingParent");
                buildingParent = buildingParentObj.transform;
                buildingParent.SetParent(arSessionOrigin.trackablesParent);
            }
        }

        private void Start()
        {
            // Initialize AR Session
            StartARSession();
            
            // Configure plane detection
            ConfigurePlaneDetection(autoDetectPlanes);
            
            isInitialized = true;
        }

        /// <summary>
        /// Start or reset AR session
        /// </summary>
        public void StartARSession()
        {
            if (arSession.subsystem != null)
            {
                arSession.Reset();
            }
            else
            {
                arSession.enabled = true;
            }
            
            Debug.Log("AR Session started");
        }

        /// <summary>
        /// Configure plane detection settings
        /// </summary>
        public void ConfigurePlaneDetection(bool enabled)
        {
            arPlaneManager.enabled = enabled;
            
            // Configure plane visualization
            foreach (var plane in arPlaneManager.trackables)
            {
                plane.gameObject.SetActive(showPlanes);
            }
        }

        /// <summary>
        /// Place the building model at a specified position and rotation
        /// </summary>
        public void PlaceBuildingModel(Vector3 position, Quaternion rotation, BuildingData building = null)
        {
            // Use provided building or current building
            BuildingData buildingToPlace = building != null ? building : currentBuilding;
            
            if (buildingToPlace == null)
            {
                Debug.LogError("No building data provided for placement");
                return;
            }
            
            // Remove any existing building
            if (currentBuildingObject != null)
            {
                Destroy(currentBuildingObject);
            }
            
            // Instantiate the building model
            if (buildingToPlace.buildingModelPrefab != null)
            {
                currentBuildingObject = Instantiate(buildingToPlace.buildingModelPrefab, position, rotation, buildingParent);
                currentBuildingObject.transform.localScale = buildingToPlace.modelScale;
                currentBuilding = buildingToPlace;
                isModelPlaced = true;
                
                Debug.Log($"Building model placed: {buildingToPlace.buildingName}");
            }
            else
            {
                Debug.LogError("Building model prefab is missing");
            }
        }

        /// <summary>
        /// Place the building model based on a detected QR code
        /// </summary>
        public void PlaceBuildingFromQRCode(QRCodeData qrCodeData)
        {
            if (qrCodeData == null)
            {
                Debug.LogError("QR Code data is null");
                return;
            }
            
            // Find the corresponding building
            BuildingData targetBuilding = null;
            // In a real app, you would look up the building from a database or manager
            // For this example, we'll use the currently assigned building
            targetBuilding = currentBuilding;
            
            if (targetBuilding == null)
            {
                Debug.LogError("Could not find building for QR code");
                return;
            }
            
            // Get camera position and rotation
            Vector3 cameraPosition = arCamera.transform.position;
            Quaternion rotationFromQR = Quaternion.Euler(qrCodeData.modelRotation);
            
            // Calculate position offset based on QR code data
            Vector3 positionOffset = qrCodeData.worldPosition - qrCodeData.modelPosition;
            Vector3 targetPosition = cameraPosition + positionOffset;
            
            // Place the building
            PlaceBuildingModel(targetPosition, rotationFromQR, targetBuilding);
        }

        /// <summary>
        /// Perform AR raycast to find placement points
        /// </summary>
        public bool TryGetPlacementPose(out Pose pose)
        {
            pose = new Pose();
            
            // Raycast from the center of the screen
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            
            if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
            {
                // Use the first hit
                pose = hits[0].pose;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Reset the AR session and remove placed objects
        /// </summary>
        public void ResetARSession()
        {
            // Remove current building
            if (currentBuildingObject != null)
            {
                Destroy(currentBuildingObject);
                currentBuildingObject = null;
            }
            
            // Reset AR session
            arSession.Reset();
            
            isModelPlaced = false;
            Debug.Log("AR Session reset");
        }
        
        // Update is called once per frame
        void Update()
        {
            // Show planes based on setting
            if (arPlaneManager.enabled)
            {
                foreach (var plane in arPlaneManager.trackables)
                {
                    plane.gameObject.SetActive(showPlanes);
                }
            }
            
            // Debug info
            if (showDebugInfo)
            {
                Debug.Log($"AR Tracking state: {ARSession.state}");
            }
        }
    }
}
