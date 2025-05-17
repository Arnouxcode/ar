using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ARNavigation.Data;
using ARNavigation.Navigation;
using ARNavigation.AR;
using ARNavigation.QR;

namespace ARNavigation.UI
{
    /// <summary>
    /// Manages the UI components and interactions for the AR Navigation app
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [Header("UI References")]
        public Canvas mainCanvas;
        public CanvasGroup navigationPanel;
        public CanvasGroup scanPanel;
        public CanvasGroup infoPanel;
        
        [Header("Navigation UI")]
        public TMP_Dropdown poiDropdown;
        public TMP_InputField searchInputField;
        public Button navigateButton;
        public Button stopNavigationButton;
        public Slider progressSlider;
        
        [Header("Scanning UI")]
        public Button scanQRButton;
        public Button cancelScanButton;
        public GameObject scanningIndicator;
        public TextMeshProUGUI scanStatusText;
        
        [Header("Control UI")]
        public Button resetButton;
        public Button settingsButton;
        
        [Header("Info UI")]
        public TextMeshProUGUI poiNameText;
        public TextMeshProUGUI poiDescriptionText;
        public Image poiIcon;
        public Button closeInfoButton;
        
        // References to other controllers
        private NavigationController navigationController;
        private QRCodeScanner qrScanner;
        
        // Currently displayed POIs
        private List<POIData> currentPOIs = new List<POIData>();
        private POIData selectedPOI;
        
        // Singleton instance
        public static UIManager Instance { get; private set; }
        
        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Get references if not set
            if (navigationController == null)
                navigationController = FindObjectOfType<NavigationController>();
                
            if (qrScanner == null)
                qrScanner = FindObjectOfType<QRCodeScanner>();
        }
        
        void Start()
        {
            // Setup UI panels initial state
            ShowPanel(navigationPanel);
            HidePanel(scanPanel);
            HidePanel(infoPanel);
            
            // Setup event listeners
            SetupEventListeners();
            
            // Initialize POI dropdown
            UpdatePOIDropdown();
        }
        
        /// <summary>
        /// Setup UI event listeners
        /// </summary>
        private void SetupEventListeners()
        {
            // Navigation UI
            if (poiDropdown != null)
                poiDropdown.onValueChanged.AddListener(OnPOISelected);
                
            if (searchInputField != null)
                searchInputField.onValueChanged.AddListener(OnSearchTextChanged);
                
            if (navigateButton != null)
                navigateButton.onClick.AddListener(OnNavigateButtonClicked);
                
            if (stopNavigationButton != null)
                stopNavigationButton.onClick.AddListener(OnStopNavigationButtonClicked);
            
            // Scanning UI
            if (scanQRButton != null)
                scanQRButton.onClick.AddListener(OnScanQRButtonClicked);
                
            if (cancelScanButton != null)
                cancelScanButton.onClick.AddListener(OnCancelScanButtonClicked);
            
            // Control UI
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetButtonClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            
            // Info UI
            if (closeInfoButton != null)
                closeInfoButton.onClick.AddListener(OnCloseInfoButtonClicked);
            
            // Subscribe to navigation events
            if (navigationController != null)
            {
                navigationController.OnNavigationStarted += OnNavigationStarted;
                navigationController.OnNavigationProgress += OnNavigationProgress;
                navigationController.OnNavigationCompleted += OnNavigationCompleted;
            }
            
            // Subscribe to QR scanner events
            if (qrScanner != null)
            {
                qrScanner.OnQRCodeDetected += OnQRCodeDetected;
                qrScanner.OnQRCodeDataResolved += OnQRCodeDataResolved;
            }
        }
        
        /// <summary>
        /// Update POI dropdown with current building's POIs
        /// </summary>
        public void UpdatePOIDropdown()
        {
            if (poiDropdown == null)
                return;
                
            // Clear current items
            poiDropdown.ClearOptions();
            currentPOIs.Clear();
            
            // Get current building data
            if (ARManager.Instance != null && ARManager.Instance.currentBuilding != null)
            {
                // Get all POIs from current building
                List<POIData> allPOIs = ARManager.Instance.currentBuilding.GetAllPOIs();
                
                if (allPOIs != null && allPOIs.Count > 0)
                {
                    // Store POIs for reference
                    currentPOIs = new List<POIData>(allPOIs);
                    
                    // Create dropdown options
                    List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
                    options.Add(new TMP_Dropdown.OptionData("Select Destination"));
                    
                    foreach (POIData poi in allPOIs)
                    {
                        if (poi.icon != null)
                        {
                            options.Add(new TMP_Dropdown.OptionData(poi.poiName, poi.icon));
                        }
                        else
                        {
                            options.Add(new TMP_Dropdown.OptionData(poi.poiName));
                        }
                    }
                    
                    poiDropdown.AddOptions(options);
                }
                else
                {
                    // No POIs available
                    poiDropdown.AddOptions(new List<string> { "No destinations available" });
                    poiDropdown.interactable = false;
                }
            }
            else
            {
                // No building loaded
                poiDropdown.AddOptions(new List<string> { "Scan QR code to start" });
                poiDropdown.interactable = false;
            }
        }
        
        /// <summary>
        /// Filter POIs based on search text
        /// </summary>
        private void FilterPOIsBySearch(string searchText)
        {
            if (string.IsNullOrEmpty(searchText))
            {
                // If no search text, restore full list
                UpdatePOIDropdown();
                return;
            }
            
            // Filter POIs based on search text
            List<POIData> filteredPOIs = new List<POIData>();
            List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
            
            options.Add(new TMP_Dropdown.OptionData("Search Results"));
            
            foreach (POIData poi in currentPOIs)
            {
                if (poi.poiName.ToLower().Contains(searchText.ToLower()) || 
                    (poi.description != null && poi.description.ToLower().Contains(searchText.ToLower())) ||
                    poi.tags.Exists(tag => tag.ToLower().Contains(searchText.ToLower())))
                {
                    filteredPOIs.Add(poi);
                    
                    if (poi.icon != null)
                    {
                        options.Add(new TMP_Dropdown.OptionData(poi.poiName, poi.icon));
                    }
                    else
                    {
                        options.Add(new TMP_Dropdown.OptionData(poi.poiName));
                    }
                }
            }
            
            // Update dropdown with filtered list
            poiDropdown.ClearOptions();
            poiDropdown.AddOptions(options);
            
            // Update current POIs reference
            currentPOIs = filteredPOIs;
        }
        
        /// <summary>
        /// Show a specific panel and hide others
        /// </summary>
        private void ShowPanel(CanvasGroup panel)
        {
            if (panel == null)
                return;
                
            panel.alpha = 1;
            panel.interactable = true;
            panel.blocksRaycasts = true;
        }
        
        /// <summary>
        /// Hide a specific panel
        /// </summary>
        private void HidePanel(CanvasGroup panel)
        {
            if (panel == null)
                return;
                
            panel.alpha = 0;
            panel.interactable = false;
            panel.blocksRaycasts = false;
        }
        
        /// <summary>
        /// Display POI information in the info panel
        /// </summary>
        private void ShowPOIInfo(POIData poi)
        {
            if (poi == null)
                return;
                
            // Update info panel with POI data
            if (poiNameText != null)
                poiNameText.text = poi.poiName;
                
            if (poiDescriptionText != null)
                poiDescriptionText.text = poi.description + "\n\n" + poi.additionalInfo;
                
            if (poiIcon != null && poi.icon != null)
                poiIcon.sprite = poi.icon;
                
            // Show info panel
            ShowPanel(infoPanel);
        }
        
        #region UI Event Handlers
        
        /// <summary>
        /// Handle POI selection from dropdown
        /// </summary>
        private void OnPOISelected(int index)
        {
            // Skip if it's the header option (index 0)
            if (index <= 0 || index > currentPOIs.Count)
            {
                selectedPOI = null;
                return;
            }
            
            // Get selected POI
            selectedPOI = currentPOIs[index - 1]; // -1 because of the header item
            
            // Enable navigation button if a valid POI is selected
            if (navigateButton != null)
            {
                navigateButton.interactable = (selectedPOI != null);
            }
            
            // Show POI info
            ShowPOIInfo(selectedPOI);
        }
        
        /// <summary>
        /// Handle search text changes
        /// </summary>
        private void OnSearchTextChanged(string searchText)
        {
            FilterPOIsBySearch(searchText);
        }
        
        /// <summary>
        /// Handle navigate button click
        /// </summary>
        private void OnNavigateButtonClicked()
        {
            if (selectedPOI != null && navigationController != null)
            {
                navigationController.NavigateToPOI(selectedPOI);
            }
        }
        
        /// <summary>
        /// Handle stop navigation button click
        /// </summary>
        private void OnStopNavigationButtonClicked()
        {
            if (navigationController != null)
            {
                navigationController.StopNavigation();
            }
        }
        
        /// <summary>
        /// Handle QR scan button click
        /// </summary>
        private void OnScanQRButtonClicked()
        {
            if (qrScanner != null)
            {
                // Enable QR scanning
                qrScanner.ToggleScanning(true);
                
                // Show scanning UI
                ShowPanel(scanPanel);
                HidePanel(navigationPanel);
                
                // Update scanning indicator
                if (scanStatusText != null)
                    scanStatusText.text = "Scanning for QR codes...";
                    
                if (scanningIndicator != null)
                    scanningIndicator.SetActive(true);
            }
        }
        
        /// <summary>
        /// Handle cancel scan button click
        /// </summary>
        private void OnCancelScanButtonClicked()
        {
            if (qrScanner != null)
            {
                // Disable QR scanning
                qrScanner.ToggleScanning(false);
            }
            
            // Hide scanning UI
            HidePanel(scanPanel);
            ShowPanel(navigationPanel);
            
            // Update scanning indicator
            if (scanningIndicator != null)
                scanningIndicator.SetActive(false);
        }
        
        /// <summary>
        /// Handle reset button click
        /// </summary>
        private void OnResetButtonClicked()
        {
            // Reset AR session
            if (ARManager.Instance != null)
            {
                ARManager.Instance.ResetARSession();
            }
            
            // Stop any active navigation
            if (navigationController != null)
            {
                navigationController.StopNavigation();
            }
            
            // Reset UI state
            UpdatePOIDropdown();
            HidePanel(infoPanel);
            ShowPanel(navigationPanel);
        }
        
        /// <summary>
        /// Handle settings button click
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            // Show settings UI (not implemented in this example)
            Debug.Log("Settings button clicked");
        }
        
        /// <summary>
        /// Handle close info button click
        /// </summary>
        private void OnCloseInfoButtonClicked()
        {
            HidePanel(infoPanel);
        }
        
        #endregion
        
        #region Navigation Event Handlers
        
        /// <summary>
        /// Handle navigation started event
        /// </summary>
        private void OnNavigationStarted(POIData destination)
        {
            // Update UI for navigation mode
            if (stopNavigationButton != null)
                stopNavigationButton.gameObject.SetActive(true);
                
            if (navigateButton != null)
                navigateButton.gameObject.SetActive(false);
                
            // Reset progress
            if (progressSlider != null)
            {
                progressSlider.gameObject.SetActive(true);
                progressSlider.value = 0;
            }
            
            // Hide info panel
            HidePanel(infoPanel);
        }
        
        /// <summary>
        /// Handle navigation progress event
        /// </summary>
        private void OnNavigationProgress(float progress)
        {
            // Update progress slider
            if (progressSlider != null)
                progressSlider.value = progress;
        }
        
        /// <summary>
        /// Handle navigation completed event
        /// </summary>
        private void OnNavigationCompleted()
        {
            // Update UI for non-navigation mode
            if (stopNavigationButton != null)
                stopNavigationButton.gameObject.SetActive(false);
                
            if (navigateButton != null)
                navigateButton.gameObject.SetActive(true);
                
            // Hide progress
            if (progressSlider != null)
                progressSlider.gameObject.SetActive(false);
                
            // Show destination info if available
            if (selectedPOI != null)
                ShowPOIInfo(selectedPOI);
        }
        
        #endregion
        
        #region QR Scanner Event Handlers
        
        /// <summary>
        /// Handle QR code detected event
        /// </summary>
        private void OnQRCodeDetected(string qrContent)
        {
            // Update scanning status
            if (scanStatusText != null)
                scanStatusText.text = "QR Code detected: " + qrContent;
                
            // Disable scanning indicator
            if (scanningIndicator != null)
                scanningIndicator.SetActive(false);
        }
        
        /// <summary>
        /// Handle QR code data resolved event
        /// </summary>
        private void OnQRCodeDataResolved(QRCodeData qrData)
        {
            // QR code successfully resolved, hide scan panel and show navigation
            HidePanel(scanPanel);
            ShowPanel(navigationPanel);
            
            // Update POI list with new building data
            UpdatePOIDropdown();
            
            // Disable scanning
            if (qrScanner != null)
                qrScanner.ToggleScanning(false);
        }
        
        #endregion
    }
}
