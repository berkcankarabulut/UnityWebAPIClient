using System;
using System.Collections.Generic;
using UnityEngine; 

namespace Unity.WebAPI.Runtime
{
    /// <summary>
    /// Main manager for Web API operations. Singleton pattern for easy access.
    /// </summary>
    public class WebAPIManager : MonoBehaviour
    {
        public static WebAPIManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private APIConfiguration _configuration = new APIConfiguration();
        
        [Header("Settings")]
        [SerializeField] private bool _dontDestroyOnLoad = true;
        [SerializeField] private bool _initializeOnStart = true;

        private IWebAPIClient _client;
        private readonly Dictionary<Type, IAPIService> _services = new Dictionary<Type, IAPIService>();

        public IWebAPIClient Client => _client;
        public APIConfiguration Configuration => _configuration;

        // Events
        public event Action<RequestMetrics> OnRequestCompleted;
        public event Action<Exception> OnRequestFailed;
        public event Action OnInitialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        private void Start()
        {
            if (_initializeOnStart)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initialize the Web API client and services
        /// </summary>
        public void Initialize()
        {
            if (_client != null)
            {
                Debug.LogWarning("WebAPIManager is already initialized");
                return;
            }

            _client = new UnityWebAPIClient(_configuration);
            
            // Subscribe to client events
            _client.OnRequestCompleted += OnClientRequestCompleted;
            _client.OnRequestFailed += OnClientRequestFailed;

            // Initialize registered services
            foreach (var service in _services.Values)
            {
                service.Initialize(_client);
            }

            OnInitialized?.Invoke();
            
            if (_configuration.enableLogging)
            {
                Debug.Log($"WebAPIManager initialized with base URL: {_configuration.GetFullBaseURL()}");
            }
        }

        /// <summary>
        /// Register a service with the manager
        /// </summary>
        public void RegisterService<T>(T service) where T : class, IAPIService
        {
            var serviceType = typeof(T);
            
            if (_services.ContainsKey(serviceType))
            {
                Debug.LogWarning($"Service {serviceType.Name} is already registered. Replacing...");
            }

            _services[serviceType] = service;
            
            // Initialize immediately if client is ready
            if (_client != null)
            {
                service.Initialize(_client);
            }

            if (_configuration.enableLogging)
            {
                Debug.Log($"Service {serviceType.Name} registered");
            }
        }

        /// <summary>
        /// Get a registered service
        /// </summary>
        public T GetService<T>() where T : class, IAPIService
        {
            var serviceType = typeof(T);
            
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service as T;
            }

            Debug.LogError($"Service {serviceType.Name} is not registered");
            return null;
        }

        /// <summary>
        /// Unregister a service
        /// </summary>
        public void UnregisterService<T>() where T : class, IAPIService
        {
            var serviceType = typeof(T);
            
            if (_services.Remove(serviceType))
            {
                if (_configuration.enableLogging)
                {
                    Debug.Log($"Service {serviceType.Name} unregistered");
                }
            }
        }

        /// <summary>
        /// Update the API configuration
        /// </summary>
        public void UpdateConfiguration(APIConfiguration newConfiguration)
        {
            _configuration = newConfiguration ?? throw new ArgumentNullException(nameof(newConfiguration));
            
            _client?.UpdateConfiguration(_configuration);
            
            if (_configuration.enableLogging)
            {
                Debug.Log("API Configuration updated");
            }
        }

        /// <summary>
        /// Set authentication for all requests
        /// </summary>
        public void SetAuthentication(string token, AuthenticationType type = AuthenticationType.Bearer)
        {
            _client?.SetAuthentication(token, type);
            
            if (_configuration.enableLogging)
            {
                Debug.Log($"Authentication set: {type}");
            }
        }

        /// <summary>
        /// Clear authentication
        /// </summary>
        public void ClearAuthentication()
        {
            _client?.ClearAuthentication();
            
            if (_configuration.enableLogging)
            {
                Debug.Log("Authentication cleared");
            }
        }

        /// <summary>
        /// Add a default header to all requests
        /// </summary>
        public void AddDefaultHeader(string key, string value)
        {
            _client?.AddDefaultHeader(key, value);
        }

        /// <summary>
        /// Remove a default header
        /// </summary>
        public void RemoveDefaultHeader(string key)
        {
            _client?.RemoveDefaultHeader(key);
        }

        /// <summary>
        /// Check internet connectivity
        /// </summary>
        public bool IsInternetAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        /// <summary>
        /// Get network status information
        /// </summary>
        public NetworkReachability GetNetworkStatus()
        {
            return Application.internetReachability;
        }

        private void OnClientRequestCompleted(RequestMetrics metrics)
        {
            OnRequestCompleted?.Invoke(metrics);
            
            if (_configuration.enableLogging)
            {
                Debug.Log($"Request completed: {metrics.method} {metrics.url} - {metrics.statusCode} ({metrics.duration:F2}s)");
            }
        }

        private void OnClientRequestFailed(Exception exception)
        {
            OnRequestFailed?.Invoke(exception);
            
            if (_configuration.enableLogging)
            {
                Debug.LogError($"Request failed: {exception.Message}");
            }
        }

        private void OnDestroy()
        {
            if (_client != null)
            {
                _client.OnRequestCompleted -= OnClientRequestCompleted;
                _client.OnRequestFailed -= OnClientRequestFailed;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (_configuration.enableLogging)
            {
                Debug.Log($"Application pause: {pauseStatus}");
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (_configuration.enableLogging)
            {
                Debug.Log($"Application focus: {hasFocus}");
            }
        }

        #region Editor Helpers
#if UNITY_EDITOR
        [ContextMenu("Test Connection")]
        private void TestConnection()
        {
            if (_client == null)
            {
                Initialize();
            }

            TestConnectionAsync();
        }

        private async void TestConnectionAsync()
        {
            try
            {
                var response = await _client.GetAsync<object>("health");
                Debug.Log($"Connection test result: {response.success}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Connection test failed: {ex.Message}");
            }
        }
#endif
        #endregion
    }
    
}