using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using ZXing.QrCode;
using ARNavigation.Data;
using ARNavigation.AR;

namespace ARNavigation.QR
{
    /// <summary>
    /// Handles QR code scanning functionality using ZXing library and AR Foundation
    /// </summary>
    [RequireComponent(typeof(ARCameraManager))]
    public class QRCodeScanner : MonoBehaviour
    {
        [Header("QR Scanner Settings")]
        public bool enableScanning = true;
        public float scanInterval = 0.5f;
        public int scanWidth = 512;
        public int scanHeight = 512;
        
        [Header("QR References")]
        public ARCameraManager cameraManager;
        
        // Events
        public event Action<string> OnQRCodeDetected;
        public event Action<QRCodeData> OnQRCodeDataResolved;
        
        // Internal variables
        private Texture2D cameraImageTexture;
        private Color32[] cameraColorData;
        private bool isInitialized = false;
        private bool isBusy = false;
        private float lastScanTime = 0;
        
        // Barcode reader setup
        private IBarcodeReader barcodeReader;
        
        void Awake()
        {
            // Get required components
            if (cameraManager == null)
                cameraManager = GetComponent<ARCameraManager>();
            
            // Initialize barcode reader
            barcodeReader = new BarcodeReader
            {
                AutoRotate = true,
                Options = new ZXing.Common.DecodingOptions
                {
                    TryHarder = true,
                    PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
                }
            };
            
            // Initialize texture
            cameraImageTexture = new Texture2D(scanWidth, scanHeight, TextureFormat.RGBA32, false);
            cameraColorData = new Color32[scanWidth * scanHeight];
            
            isInitialized = true;
        }
        
        void OnEnable()
        {
            if (cameraManager != null)
                cameraManager.frameReceived += OnCameraFrameReceived;
        }
        
        void OnDisable()
        {
            if (cameraManager != null)
                cameraManager.frameReceived -= OnCameraFrameReceived;
        }
        
        /// <summary>
        /// Process camera frame for QR code detection
        /// </summary>
        private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
        {
            if (!enableScanning || isBusy || Time.time - lastScanTime < scanInterval)
                return;
            
            isBusy = true;
            lastScanTime = Time.time;
            
            StartCoroutine(ProcessCameraImage());
        }
        
        /// <summary>
        /// Capture and process camera image to detect QR codes
        /// </summary>
        private IEnumerator ProcessCameraImage()
        {
            // Get camera image
            XRCpuImage image;
            if (!cameraManager.TryAcquireLatestCpuImage(out image))
            {
                isBusy = false;
                yield break;
            }
            
            using (image)
            {
                // Convert XRCpuImage to Color32 array for ZXing
                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, image.width, image.height),
                    outputDimensions = new Vector2Int(scanWidth, scanHeight),
                    outputFormat = TextureFormat.RGBA32
                };
                
                // Create buffer for converted image
                int bufferSize = image.GetConvertedDataSize(conversionParams);
                var buffer = new Unity.Collections.NativeArray<byte>(bufferSize, Unity.Collections.Allocator.Temp);
                
                // Convert image to RGBA32 format
                image.Convert(conversionParams, buffer.GetSubArray(0, bufferSize));
                
                // Copy to texture
                cameraImageTexture.LoadRawTextureData(buffer);
                cameraImageTexture.Apply();
                
                buffer.Dispose();
                
                // Get Color32 data for ZXing
                cameraColorData = cameraImageTexture.GetPixels32();
                
                // Process image in a separate frame to avoid freezing
                yield return null;
                
                try
                {
                    // Detect QR code
                    var result = barcodeReader.Decode(cameraColorData, scanWidth, scanHeight);
                    
                    if (result != null)
                    {
                        // QR code detected
                        Debug.Log($"QR Code detected: {result.Text}");
                        
                        // Trigger event with QR code content
                        OnQRCodeDetected?.Invoke(result.Text);
                        
                        // Try to resolve QR code data
                        QRCodeData qrData = ResolveQRCodeData(result.Text);
                        if (qrData != null)
                        {
                            OnQRCodeDataResolved?.Invoke(qrData);
                            
                            // Place building based on QR code
                            if (ARManager.Instance != null)
                            {
                                ARManager.Instance.PlaceBuildingFromQRCode(qrData);
                            }
                        }
                        
                        // Disable scanning temporarily
                        yield return new WaitForSeconds(1.0f);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error decoding QR code: {e.Message}");
                }
            }
            
            isBusy = false;
        }
        
        /// <summary>
        /// Resolve QR code data from the scanned content
        /// </summary>
        private QRCodeData ResolveQRCodeData(string qrCodeContent)
        {
            // In a real app, you would:
            // 1. Parse the QR code content (it could be a JSON string, or a key/identifier)
            // 2. Look up the QR code data in a database or config file
            
            // For this example, we'll assume the content is a simple ID that matches QRCodeData.qrCodeID
            if (ARManager.Instance != null && ARManager.Instance.currentBuilding != null)
            {
                return ARManager.Instance.currentBuilding.FindQRCodeByID(qrCodeContent);
            }
            
            return null;
        }
        
        /// <summary>
        /// Start or stop QR code scanning
        /// </summary>
        public void ToggleScanning(bool enable)
        {
            enableScanning = enable;
            Debug.Log($"QR Scanning: {(enable ? "Enabled" : "Disabled")}");
        }
    }
}
