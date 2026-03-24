using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Tripo3D
{
    /// <summary>
    /// Centralized logging system for Tripo3D operations
    /// </summary>
    public static class TripoLogger
    {
        private static float _workflowStartTime;
        private static int _currentStep;
        private static int _totalSteps = 9;
        
        public static void StartWorkflow(string sessionId, string imagePath, TripoConfig config)
        {
            _workflowStartTime = Time.realtimeSinceStartup;
            _currentStep = 0;
            
            Debug.Log("╔════════════════════════════════════════════════════════════╗");
            Debug.Log("║          TRIPO3D MODEL GENERATION STARTED                  ║");
            Debug.Log("╚════════════════════════════════════════════════════════════╝");
            Debug.Log($"📋 Session ID: {sessionId}");
            Debug.Log($"🖼️  Image Path: {imagePath}");
            Debug.Log($"⚙️  Configuration:");
            Debug.Log($"   - Max Retries: {config.maxRetries}");
            Debug.Log($"   - Retry Delay: {config.retryDelay}s");
            Debug.Log($"   - Polling Interval: {config.pollingInterval}s");
            Debug.Log($"   - Request Timeout: {config.requestTimeout}s");
            Debug.Log($"🕐 Start Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Debug.Log("════════════════════════════════════════════════════════════");
        }
        
        public static void LogStep(string stepName)
        {
            _currentStep++;
            float elapsed = Time.realtimeSinceStartup - _workflowStartTime;
            
            Debug.Log("────────────────────────────────────────────────────────────");
            Debug.Log($"📍 Step {_currentStep}/{_totalSteps}: {stepName}");
            Debug.Log($"⏱️  Elapsed Time: {elapsed:F1}s");
            Debug.Log("────────────────────────────────────────────────────────────");
        }
        
        public static void LogStepComplete(string stepName, float stepStartTime)
        {
            float stepDuration = Time.realtimeSinceStartup - stepStartTime;
            Debug.Log($"✅ {stepName} completed in {stepDuration:F2}s");
        }
        
        public static void LogWorkflowSummary(bool success, string sessionId)
        {
            float totalDuration = Time.realtimeSinceStartup - _workflowStartTime;
            
            Debug.Log("════════════════════════════════════════════════════════════");
            if (success)
            {
                Debug.Log("╔════════════════════════════════════════════════════════════╗");
                Debug.Log("║          WORKFLOW COMPLETED SUCCESSFULLY! 🎉               ║");
                Debug.Log("╚════════════════════════════════════════════════════════════╝");
                Debug.Log($"✅ Status: SUCCESS");
                Debug.Log($"📊 Steps Completed: {_currentStep}/{_totalSteps}");
                Debug.Log($"⏱️  Total Duration: {FormatDuration(totalDuration)}");
                Debug.Log($"🕐 End Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Debug.Log($"📁 Session ID: {sessionId}");
                Debug.Log($"💾 Model saved to: Application.persistentDataPath/models/{sessionId}/");
            }
            else
            {
                Debug.LogError("╔════════════════════════════════════════════════════════════╗");
                Debug.LogError("║          WORKFLOW FAILED ❌                                ║");
                Debug.LogError("╚════════════════════════════════════════════════════════════╝");
                Debug.LogError($"❌ Status: FAILED");
                Debug.LogError($"📊 Steps Completed: {_currentStep}/{_totalSteps}");
                Debug.LogError($"⏱️  Duration Before Failure: {FormatDuration(totalDuration)}");
                Debug.LogError($"🕐 Failure Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            }
            Debug.Log("════════════════════════════════════════════════════════════");
        }
        
        public static void LogUploadStart(string filePath, long fileSize)
        {
            Debug.Log($"📤 [Upload] Reading image from: {filePath}");
            Debug.Log($"📦 [Upload] File size: {FormatBytes(fileSize)}");
        }
        
        public static void LogUploadAttempt(int attempt, int maxAttempts)
        {
            Debug.Log($"🔄 [Upload] Attempt {attempt}/{maxAttempts} - Sending request...");
        }
        
        public static void LogUploadSuccess(string token, float duration, long responseSize)
        {
            Debug.Log($"✅ [Upload] Success!");
            Debug.Log($"   🎫 Token: {token}");
            Debug.Log($"   ⏱️  Duration: {duration:F2}s");
            Debug.Log($"   📊 Response Size: {FormatBytes(responseSize)}");
        }
        
        public static void LogTaskCreation(string taskType, string taskId, float duration)
        {
            Debug.Log($"✅ [{taskType}] Task created successfully!");
            Debug.Log($"   🆔 Task ID: {taskId}");
            Debug.Log($"   ⏱️  Duration: {duration:F2}s");
        }
        
        public static void LogPollStart(string taskType, string taskId, float interval)
        {
            Debug.Log($"🔍 [{taskType}] Starting to monitor task: {taskId}");
            Debug.Log($"   ⏱️  Polling interval: {interval}s");
        }
        
        public static void LogPollStatus(string taskType, int pollCount, string status, string progress, float elapsed)
        {
            Debug.Log($"📊 [{taskType}] Status Update (Poll #{pollCount}):");
            Debug.Log($"   - Status: {status}");
            if (!string.IsNullOrEmpty(progress))
            {
                Debug.Log($"   - Progress: {progress}");
            }
            Debug.Log($"   - Elapsed Time: {elapsed:F1}s");
        }
        
        public static void LogPollSuccess(string taskType, string url, int pollCount, float duration, string createTime)
        {
            Debug.Log($"✅ [{taskType}] Completed successfully!");
            Debug.Log($"   🔗 Model URL: {url}");
            Debug.Log($"   📊 Total Polls: {pollCount}");
            Debug.Log($"   ⏱️  Total Duration: {duration:F1}s");
            Debug.Log($"   📅 Created: {createTime}");
        }
        
        public static void LogModelLoadStart(string url)
        {
            Debug.Log($"🎮 [LoadModel] Starting model download...");
            Debug.Log($"   🔗 URL: {url}");
        }
        
        public static void LogModelLoadSuccess(long dataSize, float downloadTime, float parseTime, 
            int meshCount, int vertices, int triangles, int skinnedMeshes, int bones)
        {
            Debug.Log($"✅ [LoadModel] Model loaded and displayed successfully!");
            Debug.Log($"   📦 Size: {FormatBytes(dataSize)}");
            Debug.Log($"   ⏱️  Download Time: {downloadTime:F2}s");
            Debug.Log($"   ⏱️  Parse Time: {parseTime:F2}s");
            Debug.Log($"   📊 Mesh Stats:");
            Debug.Log($"      - Mesh Count: {meshCount}");
            Debug.Log($"      - Total Vertices: {vertices:N0}");
            Debug.Log($"      - Total Triangles: {triangles:N0}");
            
            if (skinnedMeshes > 0)
            {
                Debug.Log($"   🦴 Skeleton Info:");
                Debug.Log($"      - Skinned Meshes: {skinnedMeshes}");
                Debug.Log($"      - Total Bones: {bones}");
            }
        }
        
        public static void LogDownloadProgress(float progress)
        {
            Debug.Log($"📥 [Download] Progress: {(progress * 100):F1}%");
        }
        
        public static void LogDownloadSuccess(long fileSize, string filePath)
        {
            Debug.Log($"✅ [Download] Model saved successfully!");
            Debug.Log($"   📦 File Size: {FormatBytes(fileSize)}");
            Debug.Log($"   📂 Location: {filePath}");
        }
        
        public static void LogRequestError(UnityWebRequest request, string operation, int currentAttempt = 1, int maxAttempts = 1)
        {
            Debug.LogError($"❌ [Error] Failed to {operation}");
            Debug.LogError($"   📊 HTTP Status: {request.responseCode}");
            Debug.LogError($"   ⚠️  Error: {request.error}");
            
            if (maxAttempts > 1)
            {
                Debug.LogError($"   🔄 Attempt: {currentAttempt}/{maxAttempts}");
            }
            
            if (!string.IsNullOrEmpty(request.downloadHandler?.text))
            {
                Debug.LogError($"   📄 Response Body: {request.downloadHandler.text}");
            }
        }
        
        public static void LogError(string message)
        {
            Debug.LogError($"❌ {message}");
        }
        
        public static void LogWarning(string message)
        {
            Debug.LogWarning($"⚠️  {message}");
        }
        
        public static void LogFileRead(string filePath, long fileSize)
        {
            Debug.Log($"📂 [ReadFile] Attempting to read: {filePath}");
            Debug.Log($"✅ [ReadFile] File read successfully");
            Debug.Log($"   📦 Size: {FormatBytes(fileSize)}");
        }
        
        // Helper methods
        public static string FormatBytes(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            
            return $"{len:0.##} {sizes[order]}";
        }
        
        public static string FormatDuration(float seconds)
        {
            if (seconds < 60)
            {
                return $"{seconds:F1}s";
            }
            else if (seconds < 3600)
            {
                int minutes = (int)(seconds / 60);
                int secs = (int)(seconds % 60);
                return $"{minutes}m {secs}s";
            }
            else
            {
                int hours = (int)(seconds / 3600);
                int minutes = (int)((seconds % 3600) / 60);
                return $"{hours}h {minutes}m";
            }
        }
    }
}
