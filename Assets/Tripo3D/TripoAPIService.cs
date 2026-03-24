using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Tripo3D
{
    /// <summary>
    /// Service class for handling Tripo3D API requests
    /// </summary>
    public class TripoAPIService
    {
        private readonly TripoConfig _config;
        
        public TripoAPIService(TripoConfig config)
        {
            _config = config;
        }
        
        #region Upload
        public IEnumerator UploadImage(byte[] fileData, string fileName, Action<string> onSuccess)
        {
            TripoLogger.LogUploadStart(fileName, fileData.Length);
            
            WWWForm form = new WWWForm();
            form.AddBinaryData("file", fileData, fileName, "image/jpeg");
            
            for (int attempt = 0; attempt < _config.maxRetries; attempt++)
            {
                using (UnityWebRequest request = UnityWebRequest.Post(_config.UploadEndpoint, form))
                {
                    ConfigureRequest(request);
                    
                    TripoLogger.LogUploadAttempt(attempt + 1, _config.maxRetries);
                    float requestStartTime = Time.realtimeSinceStartup;
                    
                    yield return request.SendWebRequest();
                    
                    float requestDuration = Time.realtimeSinceStartup - requestStartTime;
                    
                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        string imageToken = ParseUploadResponse(request.downloadHandler.text);
                        if (!string.IsNullOrEmpty(imageToken))
                        {
                            TripoLogger.LogUploadSuccess(imageToken, requestDuration, request.downloadHandler.data.Length);
                            onSuccess?.Invoke(imageToken);
                            yield break;
                        }
                    }
                    
                    TripoLogger.LogRequestError(request, "upload image", attempt + 1, _config.maxRetries);
                    
                    if (attempt < _config.maxRetries - 1)
                    {
                        Debug.Log($"⏳ [Upload] Retrying in {_config.retryDelay} seconds...");
                        yield return new WaitForSeconds(_config.retryDelay);
                    }
                }
            }
            
            TripoLogger.LogError($"[Upload] Failed after {_config.maxRetries} attempts");
            onSuccess?.Invoke(null);
        }
        #endregion
        
        #region Task Creation
        public IEnumerator CreateModelTask(string fileToken, Action<string> onSuccess)
        {
            Debug.Log($"🎨 [CreateTask] Preparing model generation request...");
            
            RequestData requestData = new RequestData
            {
                type = "image_to_model",
                model_version = _config.modelVersion,
                quad = _config.useQuadMesh,
                texture = _config.generateTexture,
                pbr = _config.generatePBR,
                texture_quality = _config.textureQuality,
                orientation = _config.orientation,
                file = new FileData
                {
                    type = "jpg",
                    file_token = fileToken
                }
            };
            
            yield return CreateTask(requestData, "CreateTask", onSuccess);
        }
        
        public IEnumerator CreateRiggingTask(string originalTaskId, Action<string> onSuccess)
        {
            Debug.Log($"🦴 [RiggingTask] Preparing rigging request...");
            Debug.Log($"   📌 Original Task ID: {originalTaskId}");
            
            RequestRiggingTaskData requestData = new RequestRiggingTaskData
            {
                type = "rig_model",
                original_model_task_id = originalTaskId
            };
            
            yield return CreateTask(requestData, "RiggingTask", onSuccess);
        }
        
        public IEnumerator CreateConversionTask(string originalTaskId, Action<string> onSuccess)
        {
            Debug.Log($"🔄 [ConvertTask] Preparing format conversion request...");
            Debug.Log($"   📌 Original Task ID: {originalTaskId}");
            
            RequestConvertTaskData requestData = new RequestConvertTaskData
            {
                type = "convert_model",
                original_model_task_id = originalTaskId,
                format = _config.exportFormat,
                texture_format = _config.textureFormat,
                fbx_preset = _config.fbxPreset
            };
            
            yield return CreateTask(requestData, "ConvertTask", onSuccess);
        }
        
        private IEnumerator CreateTask(object requestData, string taskType, Action<string> onSuccess)
        {
            string jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest(_config.TaskEndpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                ConfigureRequest(request);
                
                Debug.Log($"🚀 [{taskType}] Sending request to API...");
                float requestStartTime = Time.realtimeSinceStartup;
                
                yield return request.SendWebRequest();
                
                float requestDuration = Time.realtimeSinceStartup - requestStartTime;
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string taskId = ParseTaskResponse(request.downloadHandler.text);
                    if (!string.IsNullOrEmpty(taskId))
                    {
                        TripoLogger.LogTaskCreation(taskType, taskId, requestDuration);
                        onSuccess?.Invoke(taskId);
                        yield break;
                    }
                }
                
                TripoLogger.LogRequestError(request, $"create {taskType}", 1, 1);
                onSuccess?.Invoke(null);
            }
        }
        #endregion
        
        #region Task Polling
        public IEnumerator PollTaskStatus<T>(string taskId, string taskType, 
            Func<string, T> parseResponse, Func<T, string> getStatus, 
            Func<T, string> getProgress, Func<T, string> getUrl, 
            Func<T, string> getCreateTime, Action<string> onSuccess) where T : class
        {
            string taskUrl = $"{_config.TaskEndpoint}/{taskId}";
            TripoLogger.LogPollStart(taskType, taskId, _config.pollingInterval);
            
            int pollCount = 0;
            float pollStartTime = Time.realtimeSinceStartup;
            string lastStatus = "";
            
            while (true)
            {
                pollCount++;
                using (UnityWebRequest request = UnityWebRequest.Get(taskUrl))
                {
                    ConfigureRequest(request);
                    yield return request.SendWebRequest();
                    
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        TripoLogger.LogWarning($"[{taskType}] Request failed (attempt {pollCount})");
                        TripoLogger.LogRequestError(request, $"poll {taskType} status", pollCount, -1);
                        yield return new WaitForSeconds(_config.pollingInterval);
                        continue;
                    }
                    
                    T taskData = parseResponse(request.downloadHandler.text);
                    string status = getStatus(taskData);
                    
                    if (status != lastStatus)
                    {
                        lastStatus = status;
                        string progress = getProgress(taskData);
                        float elapsed = Time.realtimeSinceStartup - pollStartTime;
                        TripoLogger.LogPollStatus(taskType, pollCount, status, progress, elapsed);
                    }
                    
                    if (status == "success")
                    {
                        string modelUrl = getUrl(taskData);
                        float totalDuration = Time.realtimeSinceStartup - pollStartTime;
                        string createTime = getCreateTime(taskData);
                        
                        TripoLogger.LogPollSuccess(taskType, modelUrl, pollCount, totalDuration, createTime);
                        onSuccess?.Invoke(modelUrl);
                        yield break;
                    }
                    else if (status == "failed")
                    {
                        TripoLogger.LogError($"[{taskType}] Task failed!");
                        Debug.LogError($"   📊 Total Polls: {pollCount}");
                        Debug.LogError($"   ⏱️  Duration: {(Time.realtimeSinceStartup - pollStartTime):F1}s");
                        onSuccess?.Invoke(null);
                        yield break;
                    }
                    
                    yield return new WaitForSeconds(_config.pollingInterval);
                }
            }
        }
        #endregion
        
        #region Helper Methods
        private void ConfigureRequest(UnityWebRequest request)
        {
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {_config.apiKey}");
            request.timeout = _config.requestTimeout;
        }
        
        private string ParseUploadResponse(string jsonResponse)
        {
            try
            {
                UploadResponse response = JsonUtility.FromJson<UploadResponse>(jsonResponse);
                return response?.data?.image_token;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Parse] Upload response error: {e.Message}");
                Debug.Log($"[Parse] Raw response: {jsonResponse}");
                return null;
            }
        }
        
        private string ParseTaskResponse(string jsonResponse)
        {
            try
            {
                ApiResponse response = JsonUtility.FromJson<ApiResponse>(jsonResponse);
                return response?.data?.task_id;
            }
            catch (Exception e)
            {
                Debug.LogError($"[Parse] Task response error: {e.Message}");
                Debug.Log($"[Parse] Raw response: {jsonResponse}");
                return null;
            }
        }
        #endregion
    }
}
