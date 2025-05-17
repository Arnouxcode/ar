# Unity AR Navigation for University Campus

## Overview
This project is an Augmented Reality (AR) navigation application designed for university campuses or buildings. It uses Unity's AR Foundation to provide an interactive navigation experience through 3D floor plans, allowing users to efficiently find their way to various points of interest.

## Features
- **Cross-Platform AR Support**: Built with Unity's AR Foundation for iOS and Android compatibility
- **QR Code Location Initialization**: Scan QR codes to determine user's position within the building
- **3D Floor Plan Integration**: Navigate through an accurate 3D model of the campus
- **Pathfinding**: A* algorithm implementation for optimal path calculation
- **Points of Interest (POI)**: Easily locate classrooms, offices, and other important locations
- **Dynamic Path Visualization**: Visual guides showing the way to selected destinations
- **Modular Design**: Flexible architecture for easy expansion to additional buildings or floors

## Setup Requirements
- Unity 2021.3 LTS or newer
- AR Foundation package
- AR Subsystems package (ARCore XR Plugin for Android, ARKit XR Plugin for iOS)
- TextMeshPro for UI elements
- Optional: ZXing for QR code scanning (if not using AR Foundation's built-in functionality)

## Project Structure
- `/Scripts`: Core C# scripts
  - `/AR`: AR Foundation integration components
  - `/Navigation`: Pathfinding and waypoint systems
  - `/UI`: User interface elements
  - `/QR`: QR code scanning functionality
  - `/Data`: Data structures and scriptable objects
- `/Prefabs`: Reusable Unity components
- `/Scenes`: Unity scenes for different parts of the application
- `/Models`: 3D models for floor plans
- `/Resources`: Scriptable objects and configuration files

## Getting Started
1. Clone this repository
2. Open the project in Unity
3. Install required packages through the Package Manager
4. Import your 3D floor plan models
5. Configure POIs and QR code positions using the provided editor tools
6. Build for your target mobile platform

## Adding New Locations
The system is designed to be modular. To add new buildings, floors, or POIs:
1. Use the POI Manager tool in the Unity Editor
2. Create new scriptable objects for each building/floor
3. Position QR codes in the real world and configure their corresponding locations in the app

## Implementation Details
See the individual script files for detailed documentation on implementation.
