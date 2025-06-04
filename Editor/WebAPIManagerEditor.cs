#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Unity.WebAPI.Runtime; 

namespace Unity.WebAPI.Editor
{
    [CustomEditor(typeof(WebAPIManager))]
    public class WebAPIManagerEditor : UnityEditor.Editor
    {
        private SerializedProperty _configurationProp;
        private SerializedProperty _dontDestroyOnLoadProp;
        private SerializedProperty _initializeOnStartProp;
        
        private bool _showAdvancedSettings = false;
        private bool _showTestingTools = false;
        private bool _showRuntimeInfo = false;
        
        private string _testEndpoint = "health";
        private string _testToken = "";
        private AuthenticationType _testAuthType = AuthenticationType.Bearer;

        private void OnEnable()
        {
            _configurationProp = serializedObject.FindProperty("_configuration");
            _dontDestroyOnLoadProp = serializedObject.FindProperty("_dontDestroyOnLoad");
            _initializeOnStartProp = serializedObject.FindProperty("_initializeOnStart");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var manager = (WebAPIManager)target;

            DrawHeader();
            DrawBasicSettings();
            DrawConfigurationSettings();
            DrawAdvancedSettings();
            
            if (Application.isPlaying)
            {
                DrawRuntimeInfo(manager);
                DrawTestingTools(manager);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space();
            
            var headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleCenter
            };
            
            EditorGUILayout.LabelField("Unity Web API Manager", headerStyle);
            EditorGUILayout.LabelField("Robust HTTP Client for Unity", EditorStyles.centeredGreyMiniLabel);
            
            EditorGUILayout.Space();
            
            // Status indicator
            if (Application.isPlaying)
            {
                var manager = (WebAPIManager)target;
                bool isInitialized = manager.Client != null;
                
                var statusStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = isInitialized ? Color.green : Color.red }
                };
                
                EditorGUILayout.LabelField($"Status: {(isInitialized ? "✅ Initialized" : "❌ Not Initialized")}", statusStyle);
            }
            else
            {
                EditorGUILayout.LabelField("Status: ⏸️ Not Playing", EditorStyles.centeredGreyMiniLabel);
            }
            
            EditorGUILayout.Space();
        }

        private void DrawBasicSettings()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(_dontDestroyOnLoadProp, new GUIContent("Don't Destroy On Load", "Keep this manager alive between scene loads"));
            EditorGUILayout.PropertyField(_initializeOnStartProp, new GUIContent("Initialize On Start", "Automatically initialize when the game starts"));
            
            EditorGUILayout.EndVertical();
        }

        private void DrawConfigurationSettings()
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("API Configuration", EditorStyles.boldLabel);
            
            var baseURLProp = _configurationProp.FindPropertyRelative("baseURL");
            var apiVersionProp = _configurationProp.FindPropertyRelative("apiVersion");
            var apiKeyProp = _configurationProp.FindPropertyRelative("apiKey");
            var authTypeProp = _configurationProp.FindPropertyRelative("authenticationType");
            
            EditorGUILayout.PropertyField(baseURLProp, new GUIContent("Base URL"));
            EditorGUILayout.PropertyField(apiVersionProp, new GUIContent("API Version"));
            
            // Show full URL
            if (!string.IsNullOrEmpty(baseURLProp.stringValue))
            {
                var fullURL = $"{baseURLProp.stringValue.TrimEnd('/')}/{apiVersionProp.stringValue}";
                EditorGUILayout.LabelField("Full Base URL:", fullURL, EditorStyles.helpBox);
            }
            
            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(authTypeProp, new GUIContent("Authentication Type"));
            
            if (authTypeProp.enumValueIndex != (int)AuthenticationType.None)
            {
                EditorGUILayout.PropertyField(apiKeyProp, new GUIContent("API Key/Token"));
                
                if (!string.IsNullOrEmpty(apiKeyProp.stringValue))
                {
                    EditorGUILayout.HelpBox("⚠️ API keys are visible in the inspector. Consider loading from secure sources in production.", MessageType.Warning);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawAdvancedSettings()
        {
            EditorGUILayout.BeginVertical("box");
            _showAdvancedSettings = EditorGUILayout.Foldout(_showAdvancedSettings, "Advanced Settings", true);
            
            if (_showAdvancedSettings)
            {
                var timeoutProp = _configurationProp.FindPropertyRelative("timeout");
                var maxRetriesProp = _configurationProp.FindPropertyRelative("maxRetries");
                var retryDelayProp = _configurationProp.FindPropertyRelative("retryDelay");
                var enableLoggingProp = _configurationProp.FindPropertyRelative("enableLogging");
                var enableMetricsProp = _configurationProp.FindPropertyRelative("enableMetrics");
                var userAgentProp = _configurationProp.FindPropertyRelative("userAgent");
                
                EditorGUILayout.PropertyField(timeoutProp, new GUIContent("Timeout (seconds)"));
                EditorGUILayout.PropertyField(maxRetriesProp, new GUIContent("Max Retries"));
                EditorGUILayout.PropertyField(retryDelayProp, new GUIContent("Retry Delay (seconds)"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(enableLoggingProp, new GUIContent("Enable Logging"));
                EditorGUILayout.PropertyField(enableMetricsProp, new GUIContent("Enable Metrics"));
                
                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(userAgentProp, new GUIContent("Custom User Agent"));
                
                if (string.IsNullOrEmpty(userAgentProp.stringValue))
                {
                    var defaultUserAgent = $"Unity/{Application.unityVersion} Game/{Application.version}";
                    EditorGUILayout.LabelField("Default User Agent:", defaultUserAgent, EditorStyles.helpBox);
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawRuntimeInfo(WebAPIManager manager)
        {
            EditorGUILayout.BeginVertical("box");
            _showRuntimeInfo = EditorGUILayout.Foldout(_showRuntimeInfo, "Runtime Information", true);
            
            if (_showRuntimeInfo)
            {
                EditorGUILayout.LabelField("Client Status", manager.Client != null ? "Initialized" : "Not Initialized");
                EditorGUILayout.LabelField("Internet Available", manager.IsInternetAvailable().ToString());
                EditorGUILayout.LabelField("Network Status", manager.GetNetworkStatus().ToString());
                
                EditorGUILayout.Space();
                
                // Show registered services (this would require additional implementation in WebAPIManager)
                EditorGUILayout.LabelField("Registered Services", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("(Service info would be displayed here in a full implementation)");
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Refresh Runtime Info"))
                {
                    Repaint();
                }
            }
            
            EditorGUILayout.EndVertical();
        }

        private void DrawTestingTools(WebAPIManager manager)
        {
            EditorGUILayout.BeginVertical("box");
            _showTestingTools = EditorGUILayout.Foldout(_showTestingTools, "Testing Tools", true);
            
            if (_showTestingTools)
            {
                EditorGUILayout.HelpBox("These tools allow you to test your API connection during play mode.", MessageType.Info);
                
                EditorGUILayout.Space();
                
                // Test endpoint
                _testEndpoint = EditorGUILayout.TextField("Test Endpoint", _testEndpoint);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Test GET Request"))
                {
                    TestGetRequest(manager);
                }
                if (GUILayout.Button("Test Connection"))
                {
                    TestConnection(manager);
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Authentication testing
                EditorGUILayout.LabelField("Authentication Testing", EditorStyles.boldLabel);
                _testAuthType = (AuthenticationType)EditorGUILayout.EnumPopup("Auth Type", _testAuthType);
                _testToken = EditorGUILayout.TextField("Test Token", _testToken);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Set Test Auth"))
                {
                    manager.SetAuthentication(_testToken, _testAuthType);
                    Debug.Log($"Test authentication set: {_testAuthType}");
                }
                if (GUILayout.Button("Clear Auth"))
                {
                    manager.ClearAuthentication();
                    Debug.Log("Authentication cleared");
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                // Utility buttons
                EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Log Current Config"))
                {
                    LogCurrentConfiguration(manager);
                }
                if (GUILayout.Button("Copy Base URL"))
                {
                    var config = manager.Configuration;
                    var fullURL = config.GetFullBaseURL();
                    GUIUtility.systemCopyBuffer = fullURL;
                    Debug.Log($"Copied to clipboard: {fullURL}");
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndVertical();
        }

        private async void TestGetRequest(WebAPIManager manager)
        {
            if (manager.Client == null)
            {
                Debug.LogError("WebAPIManager is not initialized!");
                return;
            }

            try
            {
                Debug.Log($"Testing GET request to: {_testEndpoint}");
                var response = await manager.Client.GetAsync<object>(_testEndpoint);
                
                if (response.success)
                {
                    Debug.Log($"✅ Test request successful! Status: {response.statusCode}");
                    if (response.data != null)
                    {
                        Debug.Log($"Response data: {JsonUtility.ToJson(response.data, true)}");
                    }
                }
                else
                {
                    Debug.LogWarning($"⚠️ Test request completed but not successful: {response.message}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Test request failed: {ex.Message}");
            }
        }

        private void TestConnection(WebAPIManager manager)
        {
            Debug.Log("=== Connection Test ===");
            Debug.Log($"Internet Available: {manager.IsInternetAvailable()}");
            Debug.Log($"Network Status: {manager.GetNetworkStatus()}");
            Debug.Log($"Base URL: {manager.Configuration.GetFullBaseURL()}");
            Debug.Log($"Client Initialized: {manager.Client != null}");
        }

        private void LogCurrentConfiguration(WebAPIManager manager)
        {
            var config = manager.Configuration;
            
            Debug.Log("=== Current API Configuration ===");
            Debug.Log($"Base URL: {config.baseURL}");
            Debug.Log($"API Version: {config.apiVersion}");
            Debug.Log($"Full URL: {config.GetFullBaseURL()}");
            Debug.Log($"Authentication: {config.authenticationType}");
            Debug.Log($"Timeout: {config.timeout}s");
            Debug.Log($"Max Retries: {config.maxRetries}");
            Debug.Log($"Retry Delay: {config.retryDelay}s");
            Debug.Log($"Logging Enabled: {config.enableLogging}");
            Debug.Log($"Metrics Enabled: {config.enableMetrics}");
            Debug.Log($"User Agent: {config.GetUserAgent()}");
        }

        // Custom property drawer for better URL display
        [CustomPropertyDrawer(typeof(APIConfiguration))]
        public class APIConfigurationDrawer : PropertyDrawer
        {
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                EditorGUI.BeginProperty(position, label, property);
                
                // This would be implemented for a more custom configuration display
                EditorGUI.PropertyField(position, property, label, true);
                
                EditorGUI.EndProperty();
            }

            public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            {
                return EditorGUI.GetPropertyHeight(property, label, true);
            }
        }
    }

    // Menu items for easy setup
    public static class WebAPIManagerMenuItems
    {
        [MenuItem("GameObject/Web API/Create Web API Manager", false, 10)]
        public static void CreateWebAPIManager()
        {
            var go = new GameObject("WebAPIManager");
            go.AddComponent<WebAPIManager>();
            Selection.activeGameObject = go;
            
            Debug.Log("WebAPIManager created! Configure the settings in the inspector.");
        }

        [MenuItem("Tools/Web API/Create Web API Manager Prefab")]
        public static void CreateWebAPIManagerPrefab()
        {
            // Create prefab directly without instantiating in scene
            string path = "Assets/WebAPIManager.prefab";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            try
            {
                // Create a temporary GameObject for prefab creation
                var tempGO = new GameObject("WebAPIManager");
                tempGO.AddComponent<WebAPIManager>();
                
                // Create the prefab asset
                var prefab = PrefabUtility.SaveAsPrefabAsset(tempGO, path);
                
                // Clean up the temporary object safely
                Object.DestroyImmediate(tempGO);
                
                // Select the created prefab
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                
                Debug.Log($"✅ WebAPIManager prefab created successfully at: {path}");
                Debug.Log("Configure the API settings in the prefab and add it to your scene.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"❌ Failed to create WebAPIManager prefab: {ex.Message}");
            }
        }

        [MenuItem("Tools/Web API/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/yourcompany/unity-web-api-client#readme");
        }
    }
}
#endif