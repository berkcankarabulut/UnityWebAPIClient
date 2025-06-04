using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace Unity.WebAPI.Runtime
{
    /// <summary>
    /// Base class for all API services
    /// </summary>
    public abstract class BaseAPIService : IAPIService
    {
        protected IWebAPIClient _client;
        protected string _serviceEndpoint;

        protected BaseAPIService(string serviceEndpoint = "")
        {
            _serviceEndpoint = serviceEndpoint;
        }

        public virtual void Initialize(IWebAPIClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        protected string BuildEndpoint(string endpoint)
        {
            if (string.IsNullOrEmpty(_serviceEndpoint))
                return endpoint;
                
            return $"{_serviceEndpoint.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        }

        protected APIResponse<T> CreateErrorResponse<T>(string message, Exception ex = null)
        {
            Debug.LogError($"Service Error [{GetType().Name}]: {message}. Exception: {ex?.Message}");
            
            return new APIResponse<T>
            {
                success = false,
                message = message,
                data = default(T),
                statusCode = ex is WebAPIException webEx ? webEx.StatusCode : 500
            };
        }

#if UNITASK_SUPPORT
        protected async UniTask<APIResponse<T>> SafeExecuteAsync<T>(Func<UniTask<APIResponse<T>>> operation, string operationName = "API Operation")
#else
        protected async Task<APIResponse<T>> SafeExecuteAsync<T>(Func<Task<APIResponse<T>>> operation, string operationName = "API Operation")
#endif
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>($"{operationName} failed", ex);
            }
        }

        // Helper methods for common operations
#if UNITASK_SUPPORT
        protected UniTask<APIResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        protected Task<APIResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            return _client.GetAsync<T>(BuildEndpoint(endpoint), headers, cancellationToken);
        }

#if UNITASK_SUPPORT
        protected UniTask<APIResponse<T>> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        protected Task<APIResponse<T>> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            return _client.PostAsync<T>(BuildEndpoint(endpoint), data, headers, cancellationToken);
        }

#if UNITASK_SUPPORT
        protected UniTask<APIResponse<T>> PutAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        protected Task<APIResponse<T>> PutAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            return _client.PutAsync<T>(BuildEndpoint(endpoint), data, headers, cancellationToken);
        }

#if UNITASK_SUPPORT
        protected UniTask<APIResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        protected Task<APIResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            return _client.DeleteAsync<T>(BuildEndpoint(endpoint), headers, cancellationToken);
        }
    }
}