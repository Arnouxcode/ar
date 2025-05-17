using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ARNavigation.Data;
using ARNavigation.AR;

namespace ARNavigation.Navigation
{
    /// <summary>
    /// Controls the navigation functionality, including path calculation and visualization
    /// </summary>
    public class NavigationController : MonoBehaviour
    {
        [Header("Navigation References")]
        public PathVisualizer pathVisualizer;
        
        [Header("Navigation Settings")]
        public float pathUpdateInterval = 2.0f;
        public float arrivalDistance = 0.5f;
        
        // Events
        public event Action<POIData> OnNavigationStarted;
        public event Action<float> OnNavigationProgress;
        public event Action OnNavigationCompleted;
        
        // Cached path data
        private POIData currentDestination;
        private List<Vector3> currentPath;
        private bool isNavigating = false;
        private float lastPathUpdateTime = 0;
        
        // Singleton instance
        public static NavigationController Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Get reference to path visualizer if not set
            if (pathVisualizer == null)
            {
                pathVisualizer = GetComponent<PathVisualizer>();
                if (pathVisualizer == null)
                {
                    pathVisualizer = gameObject.AddComponent<PathVisualizer>();
                }
            }
        }
        
        private void Update()
        {
            if (isNavigating)
            {
                // Check if AR camera position has moved significantly to recalculate path
                if (Time.time - lastPathUpdateTime > pathUpdateInterval)
                {
                    UpdateNavigationPath();
                }
                
                // Check if destination reached
                if (currentPath != null && currentPath.Count > 0)
                {
                    Vector3 targetPosition = currentPath[currentPath.Count - 1];
                    Vector3 currentPosition = Camera.main.transform.position;
                    float distanceToTarget = Vector3.Distance(
                        new Vector3(currentPosition.x, 0, currentPosition.z),
                        new Vector3(targetPosition.x, 0, targetPosition.z)
                    );
                    
                    // Calculate and report navigation progress
                    if (currentPath.Count > 1)
                    {
                        Vector3 startPosition = currentPath[0];
                        float totalDistance = Vector3.Distance(
                            new Vector3(startPosition.x, 0, startPosition.z),
                            new Vector3(targetPosition.x, 0, targetPosition.z)
                        );
                        float distanceTraveled = totalDistance - distanceToTarget;
                        float progress = Mathf.Clamp01(distanceTraveled / totalDistance);
                        OnNavigationProgress?.Invoke(progress);
                    }
                    
                    // Check if we have arrived at destination
                    if (distanceToTarget <= arrivalDistance)
                    {
                        CompleteNavigation();
                    }
                }
            }
        }
        
        /// <summary>
        /// Start navigation to a specific POI
        /// </summary>
        public void NavigateToPOI(POIData destination)
        {
            if (destination == null)
            {
                Debug.LogError("Cannot navigate to null destination");
                return;
            }
            
            currentDestination = destination;
            isNavigating = true;
            lastPathUpdateTime = 0; // Force immediate path calculation
            
            Debug.Log($"Starting navigation to: {destination.poiName}");
            
            // Trigger navigation started event
            OnNavigationStarted?.Invoke(destination);
            
            // Calculate initial path
            UpdateNavigationPath();
        }
        
        /// <summary>
        /// Update the navigation path based on current position
        /// </summary>
        public void UpdateNavigationPath()
        {
            if (currentDestination == null || !isNavigating)
                return;
            
            if (ARManager.Instance == null || ARManager.Instance.currentBuilding == null)
            {
                Debug.LogError("AR Manager or building not available");
                return;
            }
            
            // Get the current floor data
            FloorData floorData = null;
            foreach (FloorData floor in ARManager.Instance.currentBuilding.floors)
            {
                if (floor.floorNumber == currentDestination.floorNumber)
                {
                    floorData = floor;
                    break;
                }
            }
            
            if (floorData == null)
            {
                Debug.LogError($"Could not find floor data for floor {currentDestination.floorNumber}");
                return;
            }
            
            // Calculate path from current position to destination
            Vector3 startPosition = Camera.main.transform.position;
            Vector3 endPosition = currentDestination.position;
            
            // Calculate path using A* algorithm
            currentPath = AStar.FindPath(floorData, startPosition, endPosition);
            
            if (currentPath != null && currentPath.Count > 0)
            {
                // Visualize the path
                pathVisualizer.VisualizePath(currentPath);
                
                Debug.Log($"Path updated with {currentPath.Count} points");
            }
            else
            {
                Debug.LogError("Failed to calculate path to destination");
                pathVisualizer.ClearPath();
            }
            
            lastPathUpdateTime = Time.time;
        }
        
        /// <summary>
        /// Stop navigation and clear path
        /// </summary>
        public void StopNavigation()
        {
            if (!isNavigating)
                return;
            
            isNavigating = false;
            currentDestination = null;
            currentPath = null;
            
            // Clear path visualization
            if (pathVisualizer != null)
            {
                pathVisualizer.ClearPath();
            }
            
            Debug.Log("Navigation stopped");
        }
        
        /// <summary>
        /// Complete navigation when destination is reached
        /// </summary>
        private void CompleteNavigation()
        {
            Debug.Log($"Destination reached: {currentDestination.poiName}");
            
            // Trigger navigation completed event
            OnNavigationCompleted?.Invoke();
            
            // Stop navigation
            StopNavigation();
        }
    }
}
