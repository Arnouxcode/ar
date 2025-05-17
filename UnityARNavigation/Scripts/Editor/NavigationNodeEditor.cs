using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ARNavigation.Data;

namespace ARNavigation.Editor
{
    /// <summary>
    /// Custom editor for creating and managing navigation nodes
    /// </summary>
    [CustomEditor(typeof(FloorData))]
    public class NavigationNodeEditor : UnityEditor.Editor
    {
        private FloorData floorData;
        private bool isEditingNodes = false;
        private int selectedNodeIndex = -1;
        private Vector3 newNodePosition = Vector3.zero;
        
        private void OnEnable()
        {
            floorData = (FloorData)target;
        }
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Navigation Node Editor", EditorStyles.boldLabel);
            
            // Node editing toggle
            isEditingNodes = EditorGUILayout.Toggle("Edit Navigation Nodes", isEditingNodes);
            
            if (isEditingNodes)
            {
                // Display node editing UI
                DisplayNodeEditingUI();
            }
            
            // Show node count summary
            EditorGUILayout.LabelField($"Total Nodes: {floorData.navigationNodes.Count}", EditorStyles.miniLabel);
            
            // Save changes button
            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        private void DisplayNodeEditingUI()
        {
            EditorGUILayout.Space(5);
            
            // Display list of nodes
            EditorGUILayout.LabelField("Navigation Nodes", EditorStyles.boldLabel);
            
            for (int i = 0; i < floorData.navigationNodes.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                
                // Node selection
                bool isSelected = (i == selectedNodeIndex);
                bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                
                if (newSelected && !isSelected)
                {
                    selectedNodeIndex = i;
                }
                else if (!newSelected && isSelected)
                {
                    selectedNodeIndex = -1;
                }
                
                // Node information
                EditorGUILayout.LabelField($"Node {i}: {floorData.navigationNodes[i].nodeID}");
                
                // Delete button
                if (GUILayout.Button("Delete", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Delete Node", 
                        $"Are you sure you want to delete node {floorData.navigationNodes[i].nodeID}?", 
                        "Delete", "Cancel"))
                    {
                        DeleteNode(i);
                        if (selectedNodeIndex == i)
                        {
                            selectedNodeIndex = -1;
                        }
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            // Selected node details
            if (selectedNodeIndex >= 0 && selectedNodeIndex < floorData.navigationNodes.Count)
            {
                DisplaySelectedNodeDetails();
            }
            
            EditorGUILayout.Space(10);
            
            // New node creation
            EditorGUILayout.LabelField("Create New Node", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Node ID", GUILayout.Width(60));
            string newNodeID = EditorGUILayout.TextField($"node_{floorData.navigationNodes.Count}");
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position", GUILayout.Width(60));
            newNodePosition = EditorGUILayout.Vector3Field("", newNodePosition);
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("Add New Node"))
            {
                AddNewNode(newNodeID, newNodePosition);
            }
        }
        
        private void DisplaySelectedNodeDetails()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Selected Node Details", EditorStyles.boldLabel);
            
            var node = floorData.navigationNodes[selectedNodeIndex];
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Node ID", GUILayout.Width(100));
            node.nodeID = EditorGUILayout.TextField(node.nodeID);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Position", GUILayout.Width(100));
            node.position = EditorGUILayout.Vector3Field("", node.position);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Is Walkable", GUILayout.Width(100));
            node.isWalkable = EditorGUILayout.Toggle(node.isWalkable);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cost Multiplier", GUILayout.Width(100));
            node.costMultiplier = EditorGUILayout.FloatField(node.costMultiplier);
            EditorGUILayout.EndHorizontal();
            
            // Connected nodes
            EditorGUILayout.LabelField("Connected Nodes", EditorStyles.boldLabel);
            
            // Display list of all other nodes for connections
            for (int i = 0; i < floorData.navigationNodes.Count; i++)
            {
                if (i == selectedNodeIndex)
                    continue;
                    
                var otherNode = floorData.navigationNodes[i];
                
                EditorGUILayout.BeginHorizontal();
                bool isConnected = node.connectedNodes.Contains(otherNode.nodeID);
                bool newConnected = EditorGUILayout.Toggle(isConnected, GUILayout.Width(20));
                EditorGUILayout.LabelField(otherNode.nodeID);
                EditorGUILayout.EndHorizontal();
                
                if (newConnected != isConnected)
                {
                    if (newConnected)
                    {
                        // Add connection
                        if (!node.connectedNodes.Contains(otherNode.nodeID))
                        {
                            node.connectedNodes.Add(otherNode.nodeID);
                        }
                    }
                    else
                    {
                        // Remove connection
                        node.connectedNodes.Remove(otherNode.nodeID);
                    }
                }
            }
            
            floorData.navigationNodes[selectedNodeIndex] = node;
        }
        
        private void AddNewNode(string nodeID, Vector3 position)
        {
            // Check if ID already exists
            foreach (var node in floorData.navigationNodes)
            {
                if (node.nodeID == nodeID)
                {
                    EditorUtility.DisplayDialog("Error", $"Node ID '{nodeID}' already exists.", "OK");
                    return;
                }
            }
            
            // Create new node
            FloorData.NavigationNode newNode = new FloorData.NavigationNode
            {
                nodeID = nodeID,
                position = position,
                isWalkable = true,
                costMultiplier = 1f,
                connectedNodes = new List<string>()
            };
            
            // Add to floor data
            floorData.navigationNodes.Add(newNode);
            
            // Select the new node
            selectedNodeIndex = floorData.navigationNodes.Count - 1;
            
            // Update serialized object
            serializedObject.Update();
            EditorUtility.SetDirty(target);
        }
        
        private void DeleteNode(int index)
        {
            if (index < 0 || index >= floorData.navigationNodes.Count)
                return;
                
            string nodeIDToDelete = floorData.navigationNodes[index].nodeID;
            
            // Remove node
            floorData.navigationNodes.RemoveAt(index);
            
            // Remove references to this node from other nodes
            foreach (var node in floorData.navigationNodes)
            {
                node.connectedNodes.Remove(nodeIDToDelete);
            }
            
            // Update serialized object
            serializedObject.Update();
            EditorUtility.SetDirty(target);
        }
        
        private void OnSceneGUI()
        {
            if (!isEditingNodes)
                return;
                
            // Draw nodes in scene view
            for (int i = 0; i < floorData.navigationNodes.Count; i++)
            {
                var node = floorData.navigationNodes[i];
                
                // Node color based on selection and walkability
                Color nodeColor = node.isWalkable ? Color.green : Color.red;
                if (i == selectedNodeIndex)
                {
                    nodeColor = Color.yellow;
                }
                
                // Draw node
                Handles.color = nodeColor;
                Handles.SphereHandleCap(0, node.position, Quaternion.identity, 0.2f, EventType.Repaint);
                
                // Draw node ID label
                Handles.Label(node.position + Vector3.up * 0.3f, node.nodeID);
                
                // Position handle for selected node
                if (i == selectedNodeIndex)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 newPosition = Handles.PositionHandle(node.position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Move Navigation Node");
                        node.position = newPosition;
                        floorData.navigationNodes[i] = node;
                        EditorUtility.SetDirty(target);
                    }
                }
                
                // Draw connections between nodes
                Handles.color = Color.gray;
                foreach (string connectedNodeID in node.connectedNodes)
                {
                    FloorData.NavigationNode connectedNode = null;
                    
                    // Find connected node
                    foreach (var otherNode in floorData.navigationNodes)
                    {
                        if (otherNode.nodeID == connectedNodeID)
                        {
                            connectedNode = otherNode;
                            break;
                        }
                    }
                    
                    if (connectedNode != null)
                    {
                        Handles.DrawLine(node.position, connectedNode.position);
                    }
                }
            }
        }
    }
}
