using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ZXing;
using ZXing.QrCode;
using ARNavigation.Data;

namespace ARNavigation.Editor
{
    /// <summary>
    /// Custom editor window for generating QR codes for the AR Navigation system
    /// </summary>
    public class QRCodeGenerator : EditorWindow
    {
        private BuildingData currentBuilding;
        private Vector2 scrollPosition;
        private int selectedQRIndex = -1;
        private bool showCreateQR = false;
        private string newQRID = "qr_code_01";
        private string newQRDescription = "Main Entrance";
        private Vector3 newQRWorldPosition = Vector3.zero;
        private Vector3 newQRModelPosition = Vector3.zero;
        private Vector3 newQRModelRotation = Vector3.zero;
        private float qrCodeSize = 512f;

        [MenuItem("AR Navigation/QR Code Generator")]
        public static void ShowWindow()
        {
            GetWindow<QRCodeGenerator>("QR Code Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("AR Navigation QR Code Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space(10);

            // Building selection
            EditorGUILayout.LabelField("Select Building", EditorStyles.boldLabel);
            currentBuilding = (BuildingData)EditorGUILayout.ObjectField("Building Data", currentBuilding, typeof(BuildingData), false);

            EditorGUILayout.Space(10);

            if (currentBuilding == null)
            {
                EditorGUILayout.HelpBox("Please select a Building Data asset to manage its QR codes.", MessageType.Info);
                return;
            }

            // Display existing QR codes
            EditorGUILayout.LabelField("Building QR Codes", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (currentBuilding.qrCodes.Count > 0)
            {
                for (int i = 0; i < currentBuilding.qrCodes.Count; i++)
                {
                    QRCodeData qrCode = currentBuilding.qrCodes[i];

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    
                    // QR selection
                    bool isSelected = (i == selectedQRIndex);
                    bool newSelected = EditorGUILayout.Toggle(isSelected, GUILayout.Width(20));
                    
                    if (newSelected && !isSelected)
                    {
                        selectedQRIndex = i;
                    }
                    else if (!newSelected && isSelected)
                    {
                        selectedQRIndex = -1;
                    }
                    
                    // QR basic info
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField($"ID: {qrCode.qrCodeID}");
                    EditorGUILayout.LabelField($"Location: {qrCode.locationDescription}");
                    EditorGUILayout.LabelField($"Floor: {qrCode.floorNumber}");
                    EditorGUILayout.EndVertical();

                    // Buttons
                    if (GUILayout.Button("Generate", GUILayout.Width(70)))
                    {
                        GenerateQRCodeImage(qrCode);
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        if (EditorUtility.DisplayDialog("Delete QR Code", 
                            $"Are you sure you want to delete QR code '{qrCode.qrCodeID}'?", 
                            "Delete", "Cancel"))
                        {
                            // Check if it's a scriptable object asset
                            if (AssetDatabase.Contains(qrCode))
                            {
                                string path = AssetDatabase.GetAssetPath(qrCode);
                                AssetDatabase.DeleteAsset(path);
                            }
                            
                            currentBuilding.qrCodes.RemoveAt(i);
                            EditorUtility.SetDirty(currentBuilding);
                            AssetDatabase.SaveAssets();
                            
                            if (selectedQRIndex == i)
                            {
                                selectedQRIndex = -1;
                            }
                            break;
                        }
                    }

                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.LabelField("No QR codes defined for this building.");
            }

            EditorGUILayout.EndScrollView();

            // Selected QR code details
            if (selectedQRIndex >= 0 && selectedQRIndex < currentBuilding.qrCodes.Count)
            {
                DisplaySelectedQRDetails();
            }

            EditorGUILayout.Space(10);

            // Add QR code button
            if (GUILayout.Button("Create New QR Code"))
            {
                showCreateQR = true;
            }

            // Create new QR code form
            if (showCreateQR)
            {
                DisplayCreateQRForm();
            }
        }

        private void DisplaySelectedQRDetails()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Selected QR Code Details", EditorStyles.boldLabel);
            
            QRCodeData qrCode = currentBuilding.qrCodes[selectedQRIndex];
            
            EditorGUI.BeginChangeCheck();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            qrCode.qrCodeID = EditorGUILayout.TextField("QR Code ID", qrCode.qrCodeID);
            qrCode.locationDescription = EditorGUILayout.TextField("Location Description", qrCode.locationDescription);
            qrCode.floorNumber = EditorGUILayout.IntField("Floor Number", qrCode.floorNumber);
            qrCode.worldPosition = EditorGUILayout.Vector3Field("World Position", qrCode.worldPosition);
            qrCode.modelPosition = EditorGUILayout.Vector3Field("Model Position", qrCode.modelPosition);
            qrCode.modelRotation = EditorGUILayout.Vector3Field("Model Rotation", qrCode.modelRotation);
            qrCode.isActive = EditorGUILayout.Toggle("Is Active", qrCode.isActive);
            qrCode.debugColor = EditorGUILayout.ColorField("Debug Color", qrCode.debugColor);
            
            EditorGUILayout.Space(5);
            
            if (GUILayout.Button("Generate QR Code"))
            {
                GenerateQRCodeImage(qrCode);
            }
            
            EditorGUILayout.EndVertical();
            
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(qrCode);
                EditorUtility.SetDirty(currentBuilding);
                AssetDatabase.SaveAssets();
            }
        }

        private void DisplayCreateQRForm()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Create New QR Code", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            newQRID = EditorGUILayout.TextField("QR Code ID", newQRID);
            newQRDescription = EditorGUILayout.TextField("Location Description", newQRDescription);
            int floorNumber = EditorGUILayout.IntField("Floor Number", 1);
            newQRWorldPosition = EditorGUILayout.Vector3Field("World Position", newQRWorldPosition);
            newQRModelPosition = EditorGUILayout.Vector3Field("Model Position", newQRModelPosition);
            newQRModelRotation = EditorGUILayout.Vector3Field("Model Rotation", newQRModelRotation);

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create QR Code"))
            {
                CreateNewQRCode(floorNumber);
                showCreateQR = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                showCreateQR = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void CreateNewQRCode(int floorNumber)
        {
            // Check if ID already exists
            foreach (QRCodeData qr in currentBuilding.qrCodes)
            {
                if (qr.qrCodeID == newQRID)
                {
                    EditorUtility.DisplayDialog("Error", $"QR Code ID '{newQRID}' already exists.", "OK");
                    return;
                }
            }

            // Create QR code scriptable object
            QRCodeData newQRCode = ScriptableObject.CreateInstance<QRCodeData>();
            newQRCode.qrCodeID = newQRID;
            newQRCode.buildingID = currentBuilding.buildingID;
            newQRCode.floorNumber = floorNumber;
            newQRCode.locationDescription = newQRDescription;
            newQRCode.worldPosition = newQRWorldPosition;
            newQRCode.modelPosition = newQRModelPosition;
            newQRCode.modelRotation = newQRModelRotation;
            newQRCode.isActive = true;
            newQRCode.debugColor = Color.green;

            // Save QR code asset
            string path = AssetDatabase.GetAssetPath(currentBuilding);
            path = System.IO.Path.GetDirectoryName(path);
            path = System.IO.Path.Combine(path, $"QRCode_{newQRID}.asset");
            
            AssetDatabase.CreateAsset(newQRCode, path);
            AssetDatabase.SaveAssets();

            // Add QR code to building
            currentBuilding.qrCodes.Add(newQRCode);
            EditorUtility.SetDirty(currentBuilding);
            AssetDatabase.SaveAssets();

            // Generate QR code image
            if (EditorUtility.DisplayDialog("Generate QR Code", 
                "Would you like to generate a QR code image now?", 
                "Yes", "No"))
            {
                GenerateQRCodeImage(newQRCode);
            }

            // Reset form
            newQRID = "qr_code_" + (currentBuilding.qrCodes.Count + 1).ToString("00");
            newQRDescription = "Location " + (currentBuilding.qrCodes.Count + 1);
            newQRWorldPosition = Vector3.zero;
            newQRModelPosition = Vector3.zero;
            newQRModelRotation = Vector3.zero;

            EditorUtility.DisplayDialog("Success", $"QR Code '{newQRCode.qrCodeID}' has been created.", "OK");
        }

        private void GenerateQRCodeImage(QRCodeData qrCode)
        {
            if (qrCode == null)
                return;

            // Create QR Code writer
            BarcodeWriter barcodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    Height = (int)qrCodeSize,
                    Width = (int)qrCodeSize,
                    Margin = 1
                }
            };

            // Generate QR code image
            var texture = new Texture2D((int)qrCodeSize, (int)qrCodeSize);
            Color32[] pixels;

            try
            {
                // Generate QR code with ID as content
                pixels = barcodeWriter.Write(qrCode.qrCodeID);
                texture.SetPixels32(pixels);
                texture.Apply();

                // Save as PNG
                byte[] bytes = texture.EncodeToPNG();
                string path = EditorUtility.SaveFilePanel("Save QR Code Image", 
                    "", $"QRCode_{qrCode.qrCodeID}.png", "png");

                if (!string.IsNullOrEmpty(path))
                {
                    File.WriteAllBytes(path, bytes);
                    Debug.Log($"QR Code saved to: {path}");
                    
                    // Open the file
                    EditorUtility.RevealInFinder(path);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to generate QR code: {e.Message}");
                EditorUtility.DisplayDialog("Error", $"Failed to generate QR code: {e.Message}", "OK");
            }
        }
    }
}
