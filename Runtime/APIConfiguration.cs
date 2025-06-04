using System;
using UnityEngine;

namespace Unity.WebAPI.Runtime
{
    /// <summary>
    /// Generic API response wrapper
    /// </summary>
    [Serializable]
    public class APIResponse<T>
    {
        public bool success;
        public string message;
        public T data;
        public int statusCode;
        public string requestId;
        public DateTime timestamp;

        public APIResponse()
        {
            timestamp = DateTime.UtcNow;
            requestId = Guid.NewGuid().ToString("N")[..8];
        }
    }

    /// <summary>
    /// API configuration settings
    /// </summary>
    [Serializable]
    public class APIConfiguration
    {
        [Header("Base Settings")]
        public string baseURL = "https://api.example.com";
        public string apiVersion = "v1";
        
        [Header("Authentication")]
        public string apiKey = "";
        public AuthenticationType authenticationType = AuthenticationType.Bearer;
        
        [Header("Request Settings")]
        [Range(5f, 120f)]
        public float timeout = 30f;
        
        [Range(0, 10)]
        public int maxRetries = 3;
        
        [Range(0.1f, 5f)]
        public float retryDelay = 1f;
        
        [Header("Advanced")]
        public bool enableLogging = true;
        public bool enableMetrics = false;
        public string userAgent = "";

        public string GetFullBaseURL()
        {
            return $"{baseURL.TrimEnd('/')}/{apiVersion}";
        }

        public string GetUserAgent()
        {
            return string.IsNullOrEmpty(userAgent) 
                ? $"Unity/{Application.unityVersion} Game/{Application.version}"
                : userAgent;
        }
    }

    public enum AuthenticationType
    {
        None,
        Bearer,
        ApiKey,
        Basic
    }

    /// <summary>
    /// Request metrics for monitoring
    /// </summary>
    [Serializable]
    public class RequestMetrics
    {
        public string url;
        public string method;
        public float duration;
        public int statusCode;
        public bool success;
        public DateTime timestamp;
        public int retryCount;
    }
}