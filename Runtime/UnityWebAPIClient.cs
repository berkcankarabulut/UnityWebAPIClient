using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace Unity.WebAPI.Runtime
{
    public class UnityWebAPIClient : IWebAPIClient
    {
        private APIConfiguration _config;
        private readonly Dictionary<string, string> _defaultHeaders;
        private string _authToken;
        private AuthenticationType _authType;

        public event Action<RequestMetrics> OnRequestCompleted;
        public event Action<Exception> OnRequestFailed;

        public UnityWebAPIClient(APIConfiguration config = null)
        {
            _config = config ?? new APIConfiguration();
            _defaultHeaders = new Dictionary<string, string>();
            _authType = AuthenticationType.None;
        }

#if UNITASK_SUPPORT
        public async UniTask<APIResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        public async Task<APIResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            var startTime = DateTime.UtcNow;
            var url = BuildUrl(endpoint);
            
            try
            {
                using var request = UnityWebRequest.Get(url);
                SetupRequest(request, headers);
                
                var response = await ExecuteRequest<T>(request, cancellationToken);
                LogMetrics(url, "GET", startTime, response);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = CreateErrorResponse<T>("GET request failed", ex);
                LogMetrics(url, "GET", startTime, errorResponse);
                OnRequestFailed?.Invoke(ex);
                return errorResponse;
            }
        }

#if UNITASK_SUPPORT
        public async UniTask<APIResponse<T>> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        public async Task<APIResponse<T>> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            var startTime = DateTime.UtcNow;
            var url = BuildUrl(endpoint);
            
            try
            {
                var jsonData = data != null ? JsonUtility.ToJson(data) : "";
                var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                
                using var request = new UnityWebRequest(url, "POST")
                {
                    uploadHandler = new UploadHandlerRaw(bodyRaw),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                
                SetupRequest(request, headers);
                request.SetRequestHeader("Content-Type", "application/json");
                
                var response = await ExecuteRequest<T>(request, cancellationToken);
                LogMetrics(url, "POST", startTime, response);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = CreateErrorResponse<T>("POST request failed", ex);
                LogMetrics(url, "POST", startTime, errorResponse);
                OnRequestFailed?.Invoke(ex);
                return errorResponse;
            }
        }

#if UNITASK_SUPPORT
        public async UniTask<APIResponse<T>> PutAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        public async Task<APIResponse<T>> PutAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            var startTime = DateTime.UtcNow;
            var url = BuildUrl(endpoint);
            
            try
            {
                var jsonData = data != null ? JsonUtility.ToJson(data) : "";
                var bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                
                using var request = new UnityWebRequest(url, "PUT")
                {
                    uploadHandler = new UploadHandlerRaw(bodyRaw),
                    downloadHandler = new DownloadHandlerBuffer()
                };
                
                SetupRequest(request, headers);
                request.SetRequestHeader("Content-Type", "application/json");
                
                var response = await ExecuteRequest<T>(request, cancellationToken);
                LogMetrics(url, "PUT", startTime, response);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = CreateErrorResponse<T>("PUT request failed", ex);
                LogMetrics(url, "PUT", startTime, errorResponse);
                OnRequestFailed?.Invoke(ex);
                return errorResponse;
            }
        }

#if UNITASK_SUPPORT
        public async UniTask<APIResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        public async Task<APIResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            var startTime = DateTime.UtcNow;
            var url = BuildUrl(endpoint);
            
            try
            {
                using var request = UnityWebRequest.Delete(url);
                request.downloadHandler = new DownloadHandlerBuffer();
                SetupRequest(request, headers);
                
                var response = await ExecuteRequest<T>(request, cancellationToken);
                LogMetrics(url, "DELETE", startTime, response);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = CreateErrorResponse<T>("DELETE request failed", ex);
                LogMetrics(url, "DELETE", startTime, errorResponse);
                OnRequestFailed?.Invoke(ex);
                return errorResponse;
            }
        }

#if UNITASK_SUPPORT
        public async UniTask<APIResponse<string>> UploadAsync(string endpoint, byte[] data, string fileName, string contentType = "application/octet-stream", Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        public async Task<APIResponse<string>> UploadAsync(string endpoint, byte[] data, string fileName, string contentType = "application/octet-stream", Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            var startTime = DateTime.UtcNow;
            var url = BuildUrl(endpoint);
            
            try
            {
                var formData = new List<IMultipartFormSection>
                {
                    new MultipartFormFileSection("file", data, fileName, contentType)
                };

                using var request = UnityWebRequest.Post(url, formData);
                SetupRequest(request, headers);
                request.timeout = (int)(_config.timeout * 3); // Longer timeout for uploads
                
                var response = await ExecuteRequest<string>(request, cancellationToken);
                LogMetrics(url, "UPLOAD", startTime, response);
                return response;
            }
            catch (Exception ex)
            {
                var errorResponse = CreateErrorResponse<string>("Upload failed", ex);
                LogMetrics(url, "UPLOAD", startTime, errorResponse);
                OnRequestFailed?.Invoke(ex);
                return errorResponse;
            }
        }

#if UNITASK_SUPPORT
        public async UniTask<byte[]> DownloadAsync(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#else
        public async Task<byte[]> DownloadAsync(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default)
#endif
        {
            var url = BuildUrl(endpoint);
            
            try
            {
                using var request = UnityWebRequest.Get(url);
                SetupRequest(request, headers);
                request.timeout = (int)(_config.timeout * 3); // Longer timeout for downloads
                
#if UNITASK_SUPPORT
                await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
#else
                var operation = request.SendWebRequest();
                while (!operation.isDone && !cancellationToken.IsCancellationRequested)
                {
                    await Task.Yield();
                }
                cancellationToken.ThrowIfCancellationRequested();
#endif

                if (request.result == UnityWebRequest.Result.Success)
                {
                    return request.downloadHandler.data;
                }
                else
                {
                    throw new WebAPIException($"Download failed: {request.error}", (int)request.responseCode);
                }
            }
            catch (Exception ex)
            {
                OnRequestFailed?.Invoke(ex);
                throw;
            }
        }

        public void SetAuthentication(string token, AuthenticationType type = AuthenticationType.Bearer)
        {
            _authToken = token;
            _authType = type;
        }

        public void ClearAuthentication()
        {
            _authToken = null;
            _authType = AuthenticationType.None;
        }

        public void AddDefaultHeader(string key, string value)
        {
            _defaultHeaders[key] = value;
        }

        public void RemoveDefaultHeader(string key)
        {
            _defaultHeaders.Remove(key);
        }

        public void UpdateConfiguration(APIConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        private string BuildUrl(string endpoint)
        {
            var baseUrl = _config.GetFullBaseURL();
            return $"{baseUrl.TrimEnd('/')}/{endpoint.TrimStart('/')}";
        }

        private void SetupRequest(UnityWebRequest request, Dictionary<string, string> customHeaders)
        {
            request.timeout = (int)_config.timeout;
            
            // Default headers
            request.SetRequestHeader("User-Agent", _config.GetUserAgent());
            request.SetRequestHeader("Accept", "application/json");
            
            // Authentication
            if (!string.IsNullOrEmpty(_authToken))
            {
                switch (_authType)
                {
                    case AuthenticationType.Bearer:
                        request.SetRequestHeader("Authorization", $"Bearer {_authToken}");
                        break;
                    case AuthenticationType.ApiKey:
                        request.SetRequestHeader("X-API-Key", _authToken);
                        break;
                }
            }
            
            // Default headers from configuration
            foreach (var header in _defaultHeaders)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
            
            // Custom headers
            if (customHeaders != null)
            {
                foreach (var header in customHeaders)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }
        }

#if UNITASK_SUPPORT
        private async UniTask<APIResponse<T>> ExecuteRequest<T>(UnityWebRequest request, CancellationToken cancellationToken)
#else
        private async Task<APIResponse<T>> ExecuteRequest<T>(UnityWebRequest request, CancellationToken cancellationToken)
#endif
        {
            Exception lastException = null;
            
            for (int attempt = 0; attempt <= _config.maxRetries; attempt++)
            {
                try
                {
#if UNITASK_SUPPORT
                    await request.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
#else
                    var operation = request.SendWebRequest();
                    while (!operation.isDone && !cancellationToken.IsCancellationRequested)
                    {
                        await Task.Yield();
                    }
                    cancellationToken.ThrowIfCancellationRequested();
#endif

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        var responseText = request.downloadHandler.text;
                        
                        if (typeof(T) == typeof(string))
                        {
                            return new APIResponse<T>
                            {
                                success = true,
                                data = (T)(object)responseText,
                                statusCode = (int)request.responseCode
                            };
                        }
                        
                        try
                        {
                            var parsedResponse = JsonUtility.FromJson<APIResponse<T>>(responseText);
                            parsedResponse.statusCode = (int)request.responseCode;
                            return parsedResponse;
                        }
                        catch
                        {
                            // Fallback: treat response as direct data
                            var directData = JsonUtility.FromJson<T>(responseText);
                            return new APIResponse<T>
                            {
                                success = true,
                                data = directData,
                                statusCode = (int)request.responseCode
                            };
                        }
                    }
                    else
                    {
                        throw new WebAPIException($"Request failed: {request.error}", (int)request.responseCode);
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (attempt < _config.maxRetries)
                    {
                        if (_config.enableLogging)
                        {
                            Debug.LogWarning($"Request failed, retrying... ({attempt + 1}/{_config.maxRetries}): {ex.Message}");
                        }
                        
#if UNITASK_SUPPORT
                        await UniTask.Delay(TimeSpan.FromSeconds(_config.retryDelay * (attempt + 1)), cancellationToken: cancellationToken);
#else
                        await Task.Delay(TimeSpan.FromSeconds(_config.retryDelay * (attempt + 1)), cancellationToken);
#endif
                    }
                }
            }
            
            throw lastException ?? new WebAPIException("Request failed after all retries");
        }

        private APIResponse<T> CreateErrorResponse<T>(string message, Exception ex = null)
        {
            if (_config.enableLogging)
            {
                Debug.LogError($"API Error: {message}. Exception: {ex?.Message}");
            }
            
            return new APIResponse<T>
            {
                success = false,
                message = message,
                data = default(T),
                statusCode = ex is WebAPIException webEx ? webEx.StatusCode : 500
            };
        }

        private void LogMetrics<T>(string url, string method, DateTime startTime, APIResponse<T> response)
        {
            if (!_config.enableMetrics) return;

            var metrics = new RequestMetrics
            {
                url = url,
                method = method,
                duration = (float)(DateTime.UtcNow - startTime).TotalSeconds,
                statusCode = response.statusCode,
                success = response.success,
                timestamp = startTime
            };

            OnRequestCompleted?.Invoke(metrics);
        }
    }

    public class WebAPIException : Exception
    {
        public int StatusCode { get; }
        
        public WebAPIException(string message, int statusCode = 500) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}