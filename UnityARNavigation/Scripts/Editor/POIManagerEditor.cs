using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ARNavigation.Data;

namespace ARNavigation.Editor
{
    /// <summary>
    /// Custom editor window for managing Points of Interest
    /// </summary>
    public class POIManagerEditor : EditorWindow
    {
        private BuildingData currentBuilding;
        private Vector2 scrollPosition;
        private bool showCreatePOI = false;
        private string newPOIName = "New POI";
        private string newPOIDescription = "";
        private POICategory newPOICategory = POICategory.Classroom;
        private int newPOIFloor = 1;
        private Vector3 newPOIPosition = Vector3.zero;
        private Sprite newPOIIcon;
        private GameObject newPOIMarkerPrefab;

        [MenuItem("AR Navigation/POI Manager")]
        public static void ShowWindow()
        {
            GetWindow<POIManagerEditor>("POI Manager");
        }

        private void OnGUI()
        {
            GUILayout.Label("AR Navigation POI Manager", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Building selection
            EditorGUILayout.LabelField("Select Building", EditorStyles.boldLabel);
            currentBuilding = (BuildingData)EditorGUILayout.ObjectField("Building Data", currentBuilding, typeof(BuildingData), false);

            EditorGUILayout.Space(10);

            if (currentBuilding == null)
            {
                EditorGUILayout.HelpBox("Please select a Building Data asset to manage its POIs.", MessageType.Info);
                return;
            }

            // Display floors and POIs
            EditorGUILayout.LabelField("Building Floors and POIs", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            foreach (FloorData floor in currentBuilding.floors)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField($"Floor {floor.floorNumber}: {floor.floorName}", EditorStyles.boldLabel);

                // Display POIs for this floor
                if (floor.pointsOfInterest.Count > 0)
                {
                    foreach (POIData poi in floor.pointsOfInterest)
                    {
                        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                        
                        // POI basic info
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField($"Name: {poi.poiName}");
                        EditorGUILayout.LabelField($"Category: {poi.category}");
                        EditorGUILayout.EndVertical();

                        // Buttons
                        if (GUILayout.Button("Edit", GUILayout.Width(50)))
                        {
                            Selection.activeObject = poi;
                            EditorGUIUtility.PingObject(poi);
                        }

                        if (GUILayout.Button("Delete", GUILayout.Width(50)))
                        {
                            if (EditorUtility.DisplayDialog("Delete POI", 
                                $"Are you sure you want to delete POI '{poi.poiName}'?", 
                                "Delete", "Cancel"))
                            {
                                floor.pointsOfInterest.Remove(poi);
                                EditorUtility.SetDirty(floor);
                                AssetDatabase.SaveAssets();
                                break;
                            }
                        }

                        EditorGUILayout.EndHorizontal();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No POIs on this floor.");
                }

                // Add POI button
                if (GUILayout.Button("Add POI to this floor"))
                {
                    newPOIFloor = floor.floorNumber;
                    showCreatePOI = true;
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            // Create new POI form
            if (showCreatePOI)
            {
                DisplayCreatePOIForm();
            }
        }

        private void DisplayCreatePOIForm()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Create New POI", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            newPOIName = EditorGUILayout.TextField("Name", newPOIName);
            newPOIDescription = EditorGUILayout.TextField("Description", newPOIDescription);
            newPOICategory = (POICategory)EditorGUILayout.EnumPopup("Category", newPOICategory);
            newPOIFloor = EditorGUILayout.IntField("Floor", newPOIFloor);
            newPOIPosition = EditorGUILayout.Vector3Field("Position", newPOIPosition);
            newPOIIcon = (Sprite)EditorGUILayout.ObjectField("Icon", newPOIIcon, typeof(Sprite), false);
            newPOIMarkerPrefab = (GameObject)EditorGUILayout.ObjectField("Marker Prefab", newPOIMarkerPrefab, typeof(GameObject), false);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create POI"))
            {
                CreateNewPOI();
                showCreatePOI = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                showCreatePOI = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void CreateNewPOI()
        {
            // Find the target floor
            FloorData targetFloor = null;
            foreach (FloorData floor in currentBuilding.floors)
            {
                if (floor.floorNumber == newPOIFloor)
                {
                    targetFloor = floor;
                    break;
                }
            }

            if (targetFloor == null)
            {
                EditorUtility.DisplayDialog("Error", $"Floor {newPOIFloor} does not exist in this building.", "OK");
                return;
            }

            // Create POI scriptable object
            POIData newPOI = ScriptableObject.CreateInstance<POIData>();
            newPOI.poiName = newPOIName;
            newPOI.description = newPOIDescription;
            newPOI.category = newPOICategory;
            newPOI.buildingID = currentBuilding.buildingID;
            newPOI.floorNumber = newPOIFloor;
            newPOI.position = newPOIPosition;
            newPOI.icon = newPOIIcon;
            newPOI.markerPrefab = newPOIMarkerPrefab;

            // Save POI asset
            string path = AssetDatabase.GetAssetPath(currentBuilding);
            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, $"POI_{newPOIName.Replace(" ", "_")}.asset");
            
            AssetDatabase.CreateAsset(newPOI, path);
            AssetDatabase.SaveAssets();

            // Add POI to floor
            targetFloor.pointsOfInterest.Add(newPOI);
            EditorUtility.SetDirty(targetFloor);
            AssetDatabase.SaveAssets();

            // Reset form
            newPOIName = "New POI";
            newPOIDescription = "";
            newPOIPosition = Vector3.zero;
            newPOIIcon = null;
            newPOIMarkerPrefab = null;

            EditorUtility.DisplayDialog("Success", $"POI '{newPOI.poiName}' has been created.", "OK");
        }
    }
}
