using UnityEngine;

namespace Tripo3D
{
    /// <summary>
    /// Configuration settings for Tripo3D API
    /// </summary>
    [CreateAssetMenu(fileName = "TripoConfig", menuName = "Tripo3D/Configuration")]
    public class TripoConfig : ScriptableObject
    {
        [Header("API Settings")]
        [Tooltip("Your Tripo3D API key")]
        public string apiKey = "tsk_xxx_xxx";
        
        [Header("Endpoints")]
        public string baseUrl = "https://api.tripo3d.ai/v2/openapi";
        
        [Header("Request Settings")]
        [Tooltip("Maximum number of retry attempts for failed requests")]
        [Range(1, 10)]
        public int maxRetries = 3;
        
        [Tooltip("Delay in seconds between retry attempts")]
        [Range(1f, 10f)]
        public float retryDelay = 3f;
        
        [Tooltip("Interval in seconds for polling task status")]
        [Range(1f, 10f)]
        public float pollingInterval = 2f;
        
        [Tooltip("Request timeout in seconds")]
        [Range(10, 120)]
        public int requestTimeout = 30;
        
        [Header("Model Generation Settings")]
        [Tooltip("Model version to use")]
        public string modelVersion = "v3.0-20250812";
        
        [Tooltip("Use quad mesh topology")]
        public bool useQuadMesh = true;
        
        [Tooltip("Generate textures")]
        public bool generateTexture = true;
        
        [Tooltip("Generate PBR materials")]
        public bool generatePBR = true;
        
        [Tooltip("Texture quality level")]
        public string textureQuality = "detailed";
        
        [Tooltip("Model orientation")]
        public string orientation = "align_image";
        
        [Header("Export Settings")]
        [Tooltip("Export format for model conversion")]
        public string exportFormat = "GLTF";
        
        [Tooltip("Texture format for export")]
        public string textureFormat = "PNG";
        
        [Tooltip("FBX preset for export")]
        public string fbxPreset = "3dsmax";
        
        // Computed properties
        public string TaskEndpoint => $"{baseUrl}/task";
        public string UploadEndpoint => $"{baseUrl}/upload/sts";
    }
}
