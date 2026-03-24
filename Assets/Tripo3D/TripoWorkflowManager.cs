using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace Tripo3D
{
    /// <summary>
    /// Manages the complete workflow for generating 3D models with rigging
    /// </summary>
    public class TripoWorkflowManager
    {
        private readonly TripoConfig _config;
        private readonly TripoAPIService _apiService;
        private readonly TripoModelLoader _modelLoader;
        private readonly MonoBehaviour _coroutineRunner;
        
        private string _currentSessionId;
        
        public TripoWorkflowManager(TripoConfig config, Transform modelParent, MonoBehaviour coroutineRunner)
        {
            _config = config;
            _apiService = new TripoAPIService(config);
            _modelLoader = new TripoModelLoader(modelParent);
            _coroutineRunner = coroutineRunner;
        }
        
        /// <summary>
        /// Start the complete model generation workflow
        /// </summary>
        public void StartWorkflow(string imagePath)
        {
            _currentSessionId = Guid.NewGuid().ToString();
            TripoLogger.StartWorkflow(_currentSessionId, imagePath, _config);
            _coroutineRunner.StartCoroutine(ExecuteWorkflow(imagePath));
        }
        
        private IEnumerator ExecuteWorkflow(string imageFilePath)
        {
            // Step 1: Upload image
            TripoLogger.LogStep("Uploading Image");
            float stepStartTime = Time.realtimeSinceStartup;
            
            byte[] imageData = ReadImageFile(imageFilePath);
            if (imageData == null)
            {
                TripoLogger.LogError("Failed to read image file. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            
            string imageToken = null;
            yield return _apiService.UploadImage(imageData, Path.GetFileName(imageFilePath), 
                token => imageToken = token);
            
            if (string.IsNullOrEmpty(imageToken))
            {
                TripoLogger.LogError("Failed to upload image. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Image Upload", stepStartTime);
            
            // Step 2: Create model generation task
            TripoLogger.LogStep("Creating Model Generation Task");
            stepStartTime = Time.realtimeSinceStartup;
            
            string taskId = null;
            yield return _apiService.CreateModelTask(imageToken, id => taskId = id);
            
            if (string.IsNullOrEmpty(taskId))
            {
                TripoLogger.LogError("Failed to create task. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Model Task Creation", stepStartTime);
            
            // Step 3: Poll for task completion
            TripoLogger.LogStep("Waiting for Model Generation");
            stepStartTime = Time.realtimeSinceStartup;
            
            string pbrModelUrl = null;
            yield return _apiService.PollTaskStatus<TaskData>(
                taskId, "PollTask",
                json => JsonUtility.FromJson<TaskData>(json),
                data => data.data.status,
                data => data.data.progress,
                data => data.data.output?.pbr_model,
                data => data.data.create_time,
                url => pbrModelUrl = url
            );
            
            if (string.IsNullOrEmpty(pbrModelUrl))
            {
                TripoLogger.LogError("Failed to get PBR model URL. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Model Generation", stepStartTime);
            
            // Step 4: Create rigging task
            TripoLogger.LogStep("Creating Rigging Task (Adding Skeleton)");
            stepStartTime = Time.realtimeSinceStartup;
            
            string riggingTaskId = null;
            yield return _apiService.CreateRiggingTask(taskId, id => riggingTaskId = id);
            
            if (string.IsNullOrEmpty(riggingTaskId))
            {
                TripoLogger.LogError("Failed to create rigging task. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Rigging Task Creation", stepStartTime);
            
            // Step 5: Poll for rigging completion
            TripoLogger.LogStep("Waiting for Rigging Process");
            stepStartTime = Time.realtimeSinceStartup;
            
            string riggedModelUrl = null;
            yield return _apiService.PollTaskStatus<RiggingTaskData>(
                riggingTaskId, "PollRigging",
                json => JsonUtility.FromJson<RiggingTaskData>(json),
                data => data.data.status,
                data => data.data.progress,
                data => data.data.output?.model,
                data => data.data.create_time,
                url => riggedModelUrl = url
            );
            
            if (string.IsNullOrEmpty(riggedModelUrl))
            {
                TripoLogger.LogError("Failed to get rigged model URL. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Rigging Process", stepStartTime);
            
            // Step 6: Convert to GLTF format
            TripoLogger.LogStep("Creating Format Conversion Task");
            stepStartTime = Time.realtimeSinceStartup;
            
            string conversionTaskId = null;
            yield return _apiService.CreateConversionTask(riggingTaskId, id => conversionTaskId = id);
            
            if (string.IsNullOrEmpty(conversionTaskId))
            {
                TripoLogger.LogError("Failed to create conversion task. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Conversion Task Creation", stepStartTime);
            
            // Step 7: Poll for conversion completion
            TripoLogger.LogStep("Waiting for Format Conversion");
            stepStartTime = Time.realtimeSinceStartup;
            
            string gltfModelUrl = null;
            yield return _apiService.PollTaskStatus<FinalTaskData>(
                conversionTaskId, "PollConversion",
                json => JsonUtility.FromJson<FinalTaskData>(json),
                data => data.data.status,
                data => data.data.progress,
                data => data.data.output?.model,
                data => data.data.create_time,
                url => gltfModelUrl = url
            );
            
            if (string.IsNullOrEmpty(gltfModelUrl))
            {
                TripoLogger.LogError("Failed to get GLTF model URL. Workflow aborted.");
                TripoLogger.LogWorkflowSummary(false, _currentSessionId);
                yield break;
            }
            TripoLogger.LogStepComplete("Format Conversion", stepStartTime);
            
            // Step 8: Load and display model
            TripoLogger.LogStep("Loading and Displaying Model");
            stepStartTime = Time.realtimeSinceStartup;
            yield return _modelLoader.LoadAndDisplayModel(gltfModelUrl);
            TripoLogger.LogStepComplete("Model Display", stepStartTime);
            
            // Step 9: Download model for persistence
            TripoLogger.LogStep("Downloading Model to Local Storage");
            stepStartTime = Time.realtimeSinceStartup;
            _modelLoader.DownloadModelAsync(gltfModelUrl, "rigged_model.glb", _currentSessionId);
            TripoLogger.LogStepComplete("Model Download", stepStartTime);
            
            TripoLogger.LogWorkflowSummary(true, _currentSessionId);
        }
        
        private byte[] ReadImageFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"❌ [ReadFile] File does not exist: {filePath}");
                return null;
            }
            
            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                TripoLogger.LogFileRead(filePath, data.Length);
                return data;
            }
            catch (Exception e)
            {
                Debug.LogError($"❌ [ReadFile] Error reading file: {e.Message}");
                Debug.LogError($"   📄 Stack Trace: {e.StackTrace}");
                return null;
            }
        }
    }
}
