using System;
using System.Collections.Generic;
using System.Threading;

#if UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#else
using System.Threading.Tasks;
#endif

namespace Unity.WebAPI.Runtime
{
    /// <summary>
    /// Main interface for HTTP operations
    /// </summary>
    public interface IWebAPIClient
    {
#if UNITASK_SUPPORT
        UniTask<APIResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<APIResponse<T>> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<APIResponse<T>> PutAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<APIResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<APIResponse<string>> UploadAsync(string endpoint, byte[] data, string fileName, string contentType = "application/octet-stream", Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        UniTask<byte[]> DownloadAsync(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
#else
        Task<APIResponse<T>> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Task<APIResponse<T>> PostAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Task<APIResponse<T>> PutAsync<T>(string endpoint, object data, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Task<APIResponse<T>> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Task<APIResponse<string>> UploadAsync(string endpoint, byte[] data, string fileName, string contentType = "application/octet-stream", Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
        Task<byte[]> DownloadAsync(string endpoint, Dictionary<string, string> headers = null, CancellationToken cancellationToken = default);
#endif

        void SetAuthentication(string token, AuthenticationType type = AuthenticationType.Bearer);
        void ClearAuthentication();
        void AddDefaultHeader(string key, string value);
        void RemoveDefaultHeader(string key);
        void UpdateConfiguration(APIConfiguration config);
        
        event Action<RequestMetrics> OnRequestCompleted;
        event Action<Exception> OnRequestFailed;
    }

    /// <summary>
    /// Interface for service base classes
    /// </summary>
    public interface IAPIService
    {
        void Initialize(IWebAPIClient client);
    }
}