using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARNavigation.AR;
using ARNavigation.Data;
using ARNavigation.Navigation;
using ARNavigation.QR;
using ARNavigation.UI;

namespace ARNavigation
{
    /// <summary>
    /// Main application controller that ties together all AR Navigation components
    /// </summary>
    public class ARNavigationApp : MonoBehaviour
    {
        [Header("System Components")]
        public ARManager arManager;
        public NavigationController navigationController;
        public QRCodeScanner qrScanner;
        public UIManager uiManager;
        
        [Header("Application Settings")]
        public BuildingData defaultBuilding;
        public bool loadDefaultBuildingOnStart = false;
        public bool enableDebugMode = true;
        
        // Singleton instance
        public static ARNavigationApp Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Make sure this object persists between scenes if needed
            DontDestroyOnLoad(gameObject);
            
            // Get required components if not assigned
            if (arManager == null)
                arManager = FindObjectOfType<ARManager>();
                
            if (navigationController == null)
                navigationController = FindObjectOfType<NavigationController>();
                
            if (qrScanner == null)
                qrScanner = FindObjectOfType<QRCodeScanner>();
                
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
        }
        
        private void Start()
        {
            // Configure debug mode
            if (arManager != null)
            {
                arManager.showDebugInfo = enableDebugMode;
                arManager.showPlanes = enableDebugMode;
            }
            
            // Load default building if enabled
            if (loadDefaultBuildingOnStart && defaultBuilding != null && arManager != null)
            {
                arManager.currentBuilding = defaultBuilding;
                
                // Update UI with building data
                if (uiManager != null)
                {
                    uiManager.UpdatePOIDropdown();
                }
                
                Debug.Log($"Loaded default building: {defaultBuilding.buildingName}");
            }
            
            // Subscribe to events
            if (qrScanner != null)
            {
                qrScanner.OnQRCodeDataResolved += OnQRCodeResolved;
            }
        }
        
        /// <summary>
        /// Handle QR code data resolved event
        /// </summary>
        private void OnQRCodeResolved(QRCodeData qrData)
        {
            Debug.Log($"QR Code resolved: {qrData.qrCodeID} for building {qrData.buildingID}");
            
            // Load corresponding building if not already loaded
            if (arManager != null && arManager.currentBuilding != null && 
                arManager.currentBuilding.buildingID != qrData.buildingID)
            {
                // In a real app, you would look up the building from a database or resource manager
                // For this example, we'll use the default building if available
                if (defaultBuilding != null && defaultBuilding.buildingID == qrData.buildingID)
                {
                    arManager.currentBuilding = defaultBuilding;
                    Debug.Log($"Switched to building: {defaultBuilding.buildingName}");
                }
            }
        }
        
        /// <summary>
        /// Load a specific building
        /// </summary>
        public void LoadBuilding(BuildingData building)
        {
            if (building == null)
                return;
                
            // Set current building
            if (arManager != null)
            {
                arManager.currentBuilding = building;
                
                // Update UI with building data
                if (uiManager != null)
                {
                    uiManager.UpdatePOIDropdown();
                }
                
                Debug.Log($"Loaded building: {building.buildingName}");
            }
        }
        
        /// <summary>
        /// Navigate to a specific POI
        /// </summary>
        public void NavigateToPOI(POIData destination)
        {
            if (destination == null)
                return;
                
            // Start navigation
            if (navigationController != null)
            {
                navigationController.NavigateToPOI(destination);
                Debug.Log($"Starting navigation to: {destination.poiName}");
            }
        }
        
        /// <summary>
        /// Reset the application state
        /// </summary>
        public void ResetApplication()
        {
            // Reset AR
            if (arManager != null)
            {
                arManager.ResetARSession();
            }
            
            // Stop navigation
            if (navigationController != null)
            {
                navigationController.StopNavigation();
            }
            
            // Update UI
            if (uiManager != null)
            {
                uiManager.UpdatePOIDropdown();
            }
            
            Debug.Log("Application reset");
        }
    }
}
