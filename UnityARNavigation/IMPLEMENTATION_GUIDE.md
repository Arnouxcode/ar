# Unity AR Navigation Implementation Guide

This guide provides detailed instructions on how to set up and implement the AR Navigation system in your Unity project.

## Prerequisites

- Unity 2021.3 LTS or newer
- AR Foundation package installed
- ARCore XR Plugin (for Android) and/or ARKit XR Plugin (for iOS)
- TextMeshPro package for UI elements
- ZXing library for QR code scanning (can be imported via NuGet or manual DLL import)

## Project Setup

1. **Import Required Packages**

   Open the Unity Package Manager (Window > Package Manager) and install:
   - AR Foundation
   - ARCore XR Plugin (for Android)
   - ARKit XR Plugin (for iOS)
   - TextMeshPro

2. **Import ZXing Library**

   The QR code scanning functionality uses the ZXing library. You can import it via:
   - NuGet for Unity package
   - Manual import of the ZXing.Net DLL
   - OR use the alternative AR Foundation QR code detection if available in your version

3. **Configure AR Settings**

   - Enable AR in Player Settings
   - Set the appropriate permissions (camera access, etc.)
   - Set minimum API level for Android (24+) or iOS version (11+) as required

## Scene Setup

1. **Create a new AR scene**

   Create a new scene for your AR Navigation application with the following structure:

   ```
   AR Navigation Scene
   ├── AR Session Origin
   │   ├── AR Camera
   │   │   ├── QRCodeScanner
   │   │   └── MainCanvas
   │   └── Trackables Parent
   ├── AR Session
   ├── ARNavigationApp
   │   ├── ARManager
   │   ├── NavigationController
   │   │   └── PathVisualizer
   │   └── UIManager
   ```

2. **Configure AR Components**

   - Add the following components to the AR Session Origin:
     - ARPlaneManager
     - ARRaycastManager
     - ARPointCloudManager (optional)
     - ARAnchorManager (optional)

   - Add the AR Camera component to the Main Camera

3. **Set Up Prefabs**

   Create the following prefabs:
   - Navigation Arrow: For path visualization
   - Waypoint Marker: For path endpoints and turns
   - POI Marker: For visualizing POIs in AR

## 3D Model Preparation

1. **Prepare Building Models**

   - Create or acquire 3D models of your building/campus
   - Ensure proper scale (1 Unity unit = 1 meter recommended)
   - Optimize the models for mobile (reduce polygon count, texture size)
   - Split large buildings into floors if needed

2. **Configure Model Import Settings**

   - Set appropriate import scale and rotation
   - Enable Read/Write for mesh data
   - Consider using LOD (Level of Detail) for large models

## Creating Navigation Data

1. **Create Building Data**

   - Right-click in the Project window and select "Create > AR Navigation > Building"
   - Fill in the building details (ID, name, description)
   - Assign the 3D model prefab
   - Configure the model scale and rotation parameters

2. **Create Floor Data**

   - Right-click in the Project window and select "Create > AR Navigation > Floor"
   - Set floor number and name
   - Assign to a building
   - Add floor-specific 3D model if applicable

3. **Create Navigation Nodes**

   - Select a Floor Data asset
   - Use the Navigation Node Editor (visible in the Inspector)
   - Enable "Edit Navigation Nodes"
   - Use the Scene View to place and connect nodes
   - Ensure nodes cover all walkable areas and are properly connected

4. **Create Points of Interest (POIs)**

   - Use the POI Manager (AR Navigation > POI Manager)
   - Select your Building Data
   - Create POIs for each important location
   - Assign appropriate categories, descriptions, and icons

5. **Create QR Codes**

   - Use the QR Code Generator (AR Navigation > QR Code Generator)
   - Select your Building Data
   - Create QR codes for each entry/initialization point
   - Configure the world position, model position, and rotation
   - Generate and print QR codes for real-world placement

## UI Setup

1. **Main Canvas Configuration**

   - Create a Canvas with Screen Space - Camera render mode
   - Set the AR Camera as the render camera
   - Add the following UI panels:
     - Navigation Panel (with POI selection dropdown, search field, etc.)
     - Scanning Panel (for QR code scanning interface)
     - Info Panel (for displaying POI information)

2. **Configure UI Manager**

   - Assign the various UI elements to the UIManager component
   - Set references to the Navigation Controller and QR Scanner

## Testing and Deployment

1. **Testing in the Editor**

   - Test the pathfinding using Play Mode in the editor
   - Simulate QR code scanning by manually triggering code in the QRCodeScanner
   - Verify that the navigation path is displayed correctly

2. **Testing on Device**

   - Build and deploy to a mobile device
   - Test QR code scanning with printed QR codes
   - Verify AR plane detection and model placement
   - Test navigation to different POIs

3. **Optimizations**

   - Adjust path visualization for clarity
   - Fine-tune navigation node placement and connections
   - Optimize 3D models if performance issues occur
   - Adjust UI for better usability on smaller screens

## Extending the System

1. **Adding New Buildings/Floors**

   - Create new Building/Floor Data assets
   - Add 3D models and configure correctly
   - Create navigation nodes and POIs
   - Generate QR codes for initialization

2. **Custom Path Visualization**

   - Modify the PathVisualizer script to change the appearance
   - Create custom prefabs for arrows, waypoints, etc.
   - Adjust visualization parameters (height, spacing, etc.)

3. **Additional Features**

   - Indoor positioning improvements (using AR anchors, image markers, etc.)
   - User location tracking and automatic re-routing
   - Voice instructions for navigation
   - Integration with building systems (room booking, etc.)

## Troubleshooting

1. **AR Foundation Issues**

   - Ensure AR Foundation and XR plugins are compatible with your Unity version
   - Check device compatibility (ARCore/ARKit supported devices)
   - Verify camera permissions are granted

2. **Navigation Issues**

   - Check navigation node connections (may need to add more nodes)
   - Verify that the pathfinding algorithm is working (A* implementation)
   - Ensure the path visualization is properly configured

3. **QR Code Scanning Issues**

   - Verify ZXing library is properly imported
   - Check lighting conditions (QR codes need good lighting)
   - Ensure QR code size is appropriate for scanning distance

4. **3D Model Placement Issues**

   - Check QR code position/rotation configuration
   - Verify that AR plane detection is working
   - Adjust model scale if necessary
