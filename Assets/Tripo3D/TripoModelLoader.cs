using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UniGLTF;

namespace Tripo3D
{
    /// <summary>
    /// Handles model loading and downloading operations
    /// </summary>
    public class TripoModelLoader
    {
        private readonly Transform _parentTransform;
        
        public TripoModelLoader(Transform parentTransform)
        {
            _parentTransform = parentTransform;
        }
        
        /// <summary>
        /// Load and display GLTF model in the scene
        /// </summary>
        public IEnumerator LoadAndDisplayModel(string gltfUrl)
        {
            TripoLogger.LogModelLoadStart(gltfUrl);
            
            using (UnityWebRequest request = UnityWebRequest.Get(gltfUrl))
            {
                float downloadStartTime = Time.realtimeSinceStartup;
                yield return request.SendWebRequest();
                float downloadDuration = Time.realtimeSinceStartup - downloadStartTime;

                if (request.result == UnityWebRequest.Result.Success)
                {
                    byte[] data = request.downloadHandler.data;
                    
                    float parseStartTime = Time.realtimeSinceStartup;
                    GameObject model = LoadGltfFromBytes(data);
                    float parseDuration = Time.realtimeSinceStartup - parseStartTime;
                    
                    if (model != null)
                    {
                        model.transform.SetParent(_parentTransform, false);
                        model.transform.localPosition = Vector3.zero;
                        model.transform.localRotation = Quaternion.identity;
                        model.transform.localScale = Vector3.one;
                        
                        // Collect mesh statistics
                        var stats = CollectMeshStatistics(model);
                        
                        TripoLogger.LogModelLoadSuccess(
                            data.Length, downloadDuration, parseDuration,
                            stats.meshCount, stats.vertices, stats.triangles,
                            stats.skinnedMeshes, stats.bones
                        );
                    }
                    else
                    {
                        TripoLogger.LogError("[LoadModel] Failed to parse GLTF data");
                    }
                }
                else
                {
                    Debug.LogError($"❌ [LoadModel] Download failed: {request.error}");
                    Debug.LogError($"   📊 Status Code: {request.responseCode}");
                }
            }
        }
        
        /// <summary>
        /// Load GLTF model from byte array
        /// </summary>
        private GameObject LoadGltfFromBytes(byte[] bytes)
        {
            try
            {
                var parser = new GlbLowLevelParser("downloaded.glb", bytes);
                var gltfData = parser.Parse();

                using (var importer = new ImporterContext(gltfData))
                {
                    var instance = importer.Load();
                    return instance.Root;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LoadModel] GLTF load error: {ex}");
                return null;
            }
        }
        
        /// <summary>
        /// Download model file to persistent storage
        /// </summary>
        public async Task DownloadModelAsync(string modelUrl, string fileName, string sessionId)
        {
            string saveDirectory = Path.Combine(Application.persistentDataPath, "models", sessionId);
            string filePath = Path.Combine(saveDirectory, fileName);
            
            if (!Directory.Exists(saveDirectory))
            {
                Directory.CreateDirectory(saveDirectory);
                Debug.Log($"📁 [Download] Created directory: {saveDirectory}");
            }
            
            Debug.Log($"💾 [Download] Starting download...");
            Debug.Log($"   🔗 URL: {modelUrl}");
            Debug.Log($"   📂 Destination: {filePath}");
            
            using (UnityWebRequest request = UnityWebRequest.Get(modelUrl))
            {
                var operation = request.SendWebRequest();
                
                float lastProgress = 0f;
                while (!operation.isDone)
                {
                    if (request.downloadProgress - lastProgress >= 0.1f)
                    {
                        lastProgress = request.downloadProgress;
                        TripoLogger.LogDownloadProgress(request.downloadProgress);
                    }
                    await Task.Yield();
                }
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"❌ [Download] Failed: {request.error}");
                    Debug.LogError($"   📊 Status Code: {request.responseCode}");
                    return;
                }
                
                try
                {
                    byte[] modelData = request.downloadHandler.data;
                    File.WriteAllBytes(filePath, modelData);
                    
                    TripoLogger.LogDownloadSuccess(modelData.Length, filePath);
                }
                catch (Exception e)
                {
                    Debug.LogError($"❌ [Download] Error saving file: {e.Message}");
                    Debug.LogError($"   📄 Stack Trace: {e.StackTrace}");
                }
            }
        }
        
        /// <summary>
        /// Collect mesh statistics from a model
        /// </summary>
        private MeshStatistics CollectMeshStatistics(GameObject model)
        {
            var stats = new MeshStatistics();
            
            MeshFilter[] meshFilters = model.GetComponentsInChildren<MeshFilter>();
            stats.meshCount = meshFilters.Length;
            
            foreach (var mf in meshFilters)
            {
                if (mf.sharedMesh != null)
                {
                    stats.vertices += mf.sharedMesh.vertexCount;
                    stats.triangles += mf.sharedMesh.triangles.Length / 3;
                }
            }
            
            SkinnedMeshRenderer[] skinnedMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            stats.skinnedMeshes = skinnedMeshes.Length;
            
            foreach (var smr in skinnedMeshes)
            {
                if (smr.bones != null)
                {
                    stats.bones += smr.bones.Length;
                }
            }
            
            return stats;
        }
        
        private struct MeshStatistics
        {
            public int meshCount;
            public int vertices;
            public int triangles;
            public int skinnedMeshes;
            public int bones;
        }
    }
}
