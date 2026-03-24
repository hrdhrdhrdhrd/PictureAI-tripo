using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Tripo3D
{
    /// <summary>
    /// Enhanced model display with automatic positioning, scaling, and effects
    /// </summary>
    public class EnhancedModelDisplay : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private TripoConfig config;
        [SerializeField] private string imagePath = "Assets/images/character.jpg";
        
        [Header("UI")]
        [SerializeField] private Button generateButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Slider progressSlider;
        
        [Header("Display Settings")]
        [SerializeField] private Transform modelParent;
        [SerializeField] private bool autoRotate = true;
        [SerializeField] private float rotationSpeed = 30f;
        [SerializeField] private bool autoScale = true;
        [SerializeField] private float targetSize = 2f;
        [SerializeField] private bool centerModel = true;
        
        [Header("Camera")]
        [SerializeField] private bool autoFocusCamera = true;
        [SerializeField] private float cameraDistance = 5f;
        
        [Header("Lighting")]
        [SerializeField] private bool addLight = true;
        [SerializeField] private Color lightColor = Color.white;
        [SerializeField] private float lightIntensity = 1f;
        
        [Header("Effects")]
        [SerializeField] private bool addShadow = true;
        [SerializeField] private bool addOutline = false;
        [SerializeField] private Color outlineColor = Color.cyan;
        
        private TripoWorkflowManager _workflow;
        private GameObject _currentModel;
        private Camera _mainCamera;
        
        void Start()
        {
            Initialize();
        }
        
        void Initialize()
        {
            if (modelParent == null)
                modelParent = transform;
                
            _mainCamera = Camera.main;
            _workflow = new TripoWorkflowManager(config, modelParent, this);
            
            if (generateButton != null)
            {
                generateButton.onClick.AddListener(OnGenerateClicked);
            }
            
            UpdateStatus("Ready to generate model");
        }
        
        void OnGenerateClicked()
        {
            if (generateButton != null)
            {
                generateButton.interactable = false;
            }
            
            StartCoroutine(GenerateAndEnhanceModel());
        }
        
        IEnumerator GenerateAndEnhanceModel()
        {
            UpdateStatus("Starting model generation...");
            UpdateProgress(0f);
            
            // Clear old model
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
            }
            
            // Start workflow
            int initialChildCount = modelParent.childCount;
            _workflow.StartWorkflow(imagePath);
            
            // Wait for model to load (monitor child count changes)
            float timeout = 180f; // 3 minutes timeout
            float elapsed = 0f;
            
            while (modelParent.childCount == initialChildCount && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                UpdateProgress(Mathf.Min(0.9f, elapsed / timeout));
                UpdateStatus($"Generating model... {Mathf.RoundToInt(elapsed)}s");
                yield return null;
            }
            
            if (modelParent.childCount == initialChildCount)
            {
                UpdateStatus("Model generation timeout!");
                if (generateButton != null)
                {
                    generateButton.interactable = true;
                }
                yield break;
            }
            
            // Get the newly loaded model
            _currentModel = modelParent.GetChild(modelParent.childCount - 1).gameObject;
            
            UpdateStatus("Model loaded! Applying enhancements...");
            UpdateProgress(0.95f);
            
            yield return new WaitForSeconds(0.5f);
            
            // Apply enhancements
            ApplyEnhancements();
            
            UpdateStatus("Model ready!");
            UpdateProgress(1f);
            
            if (generateButton != null)
            {
                generateButton.interactable = true;
            }
            
            // Display model info
            LogModelInfo();
        }
        
        void ApplyEnhancements()
        {
            if (_currentModel == null) return;
            
            // Auto scale
            if (autoScale)
            {
                AutoScaleModel(_currentModel, targetSize);
            }
            
            // Center model
            if (centerModel)
            {
                CenterModel(_currentModel);
            }
            
            // Auto rotate
            if (autoRotate)
            {
                var rotator = _currentModel.AddComponent<ModelRotator>();
                rotator.rotationSpeed = rotationSpeed;
            }
            
            // Add lighting
            if (addLight)
            {
                SetupLighting(_currentModel);
            }
            
            // Add shadow
            if (addShadow)
            {
                EnableShadows(_currentModel);
            }
            
            // Add outline
            if (addOutline)
            {
                AddOutlineEffect(_currentModel);
            }
            
            // Focus camera
            if (autoFocusCamera && _mainCamera != null)
            {
                FocusCameraOnModel(_currentModel);
            }
            
            Debug.Log("✨ All enhancements applied!");
        }
        
        void AutoScaleModel(GameObject model, float targetSize)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (maxSize > 0)
            {
                float scale = targetSize / maxSize;
                model.transform.localScale = Vector3.one * scale;
                Debug.Log($"🎯 Model auto-scaled to {scale:F2}x (size: {targetSize})");
            }
        }
        
        void CenterModel(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            Vector3 offset = bounds.center - model.transform.position;
            model.transform.position -= offset;
            
            Debug.Log($"📍 Model centered at {model.transform.position}");
        }
        
        void SetupLighting(GameObject model)
        {
            GameObject lightObj = new GameObject("Model Light");
            lightObj.transform.SetParent(model.transform);
            
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightColor;
            light.intensity = lightIntensity;
            light.shadows = LightShadows.Soft;
            lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);
            
            Debug.Log("💡 Directional light added");
        }
        
        void EnableShadows(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                renderer.receiveShadows = true;
            }
            
            Debug.Log($"🌑 Shadows enabled for {renderers.Length} renderers");
        }
        
        void AddOutlineEffect(GameObject model)
        {
            // Simple outline using duplicate with scaled mesh
            // Note: For better outlines, consider using a shader-based solution
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            
            foreach (var renderer in renderers)
            {
                if (renderer is MeshRenderer meshRenderer)
                {
                    MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
                    if (meshFilter != null && meshFilter.sharedMesh != null)
                    {
                        GameObject outline = new GameObject($"{renderer.name}_Outline");
                        outline.transform.SetParent(renderer.transform);
                        outline.transform.localPosition = Vector3.zero;
                        outline.transform.localRotation = Quaternion.identity;
                        outline.transform.localScale = Vector3.one * 1.02f;
                        
                        MeshFilter outlineMeshFilter = outline.AddComponent<MeshFilter>();
                        outlineMeshFilter.sharedMesh = meshFilter.sharedMesh;
                        
                        MeshRenderer outlineRenderer = outline.AddComponent<MeshRenderer>();
                        Material outlineMaterial = new Material(Shader.Find("Standard"));
                        outlineMaterial.color = outlineColor;
                        outlineRenderer.material = outlineMaterial;
                        outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    }
                }
            }
            
            Debug.Log($"✏️ Outline effect added");
        }
        
        void FocusCameraOnModel(GameObject model)
        {
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0) return;
            
            Bounds bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            
            Vector3 targetPosition = bounds.center - _mainCamera.transform.forward * cameraDistance;
            _mainCamera.transform.position = targetPosition;
            _mainCamera.transform.LookAt(bounds.center);
            
            Debug.Log($"📷 Camera focused on model");
        }
        
        void LogModelInfo()
        {
            if (_currentModel == null) return;
            
            Debug.Log("╔════════════════════════════════════════╗");
            Debug.Log("║         Model Information              ║");
            Debug.Log("╚════════════════════════════════════════╝");
            Debug.Log($"📦 Name: {_currentModel.name}");
            Debug.Log($"📍 Position: {_currentModel.transform.position}");
            Debug.Log($"🔄 Rotation: {_currentModel.transform.rotation.eulerAngles}");
            Debug.Log($"📐 Scale: {_currentModel.transform.localScale}");
            
            MeshFilter[] meshes = _currentModel.GetComponentsInChildren<MeshFilter>();
            int totalVertices = 0;
            int totalTriangles = 0;
            
            foreach (var mesh in meshes)
            {
                if (mesh.sharedMesh != null)
                {
                    totalVertices += mesh.sharedMesh.vertexCount;
                    totalTriangles += mesh.sharedMesh.triangles.Length / 3;
                }
            }
            
            Debug.Log($"🎨 Meshes: {meshes.Length}");
            Debug.Log($"📊 Vertices: {totalVertices:N0}");
            Debug.Log($"🔺 Triangles: {totalTriangles:N0}");
            
            SkinnedMeshRenderer[] skinnedMeshes = _currentModel.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (skinnedMeshes.Length > 0)
            {
                int totalBones = 0;
                foreach (var smr in skinnedMeshes)
                {
                    if (smr.bones != null)
                    {
                        totalBones += smr.bones.Length;
                    }
                }
                Debug.Log($"🦴 Skeleton: Yes ({skinnedMeshes.Length} skinned meshes, {totalBones} bones)");
            }
            else
            {
                Debug.Log($"🦴 Skeleton: No");
            }
        }
        
        void UpdateStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            Debug.Log($"📢 {message}");
        }
        
        void UpdateProgress(float progress)
        {
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }
        }
        
        void OnDestroy()
        {
            if (generateButton != null)
            {
                generateButton.onClick.RemoveListener(OnGenerateClicked);
            }
        }
        
        // Public methods for external control
        public void SetImagePath(string path)
        {
            imagePath = path;
        }
        
        public GameObject GetCurrentModel()
        {
            return _currentModel;
        }
        
        public void ClearModel()
        {
            if (_currentModel != null)
            {
                Destroy(_currentModel);
                _currentModel = null;
                UpdateStatus("Model cleared");
            }
        }
    }
    
    /// <summary>
    /// Simple model rotator component
    /// </summary>
    public class ModelRotator : MonoBehaviour
    {
        public float rotationSpeed = 30f;
        
        void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
