# 3D模型显示功能使用指南

## ✨ 功能概述

Tripo3D SDK已经内置了完整的3D模型显示功能，可以：
- 🎮 自动加载生成的模型到Unity场景
- 📊 显示模型统计信息（顶点数、三角形数、骨骼数）
- 🎯 自动定位和缩放模型
- 💾 保存模型到本地
- 🦴 支持带骨骼的模型

## 🚀 快速使用

### 自动显示（推荐）

使用完整工作流程时，模型会自动显示：

```csharp
public class MyController : MonoBehaviour
{
    [SerializeField] private TripoConfig config;
    [SerializeField] private Button generateButton;
    [SerializeField] private string imagePath = "Assets/images/character.jpg";
    
    private TripoWorkflowManager workflow;
    
    void Start()
    {
        // 模型会显示为当前GameObject的子对象
        workflow = new TripoWorkflowManager(config, transform, this);
        generateButton.onClick.AddListener(() => workflow.StartWorkflow(imagePath));
    }
}
```

**结果**: 模型会自动加载并显示在场景中，作为当前GameObject的子对象。

## 🎯 模型显示位置

### 默认行为

模型会显示在APIClient所在GameObject的位置：

```csharp
// 在TripoModelLoader中
model.transform.SetParent(_parentTransform, false);
model.transform.localPosition = Vector3.zero;      // 相对父对象的位置
model.transform.localRotation = Quaternion.identity; // 无旋转
model.transform.localScale = Vector3.one;           // 原始大小
```

### 自定义显示位置

#### 方法1: 移动APIClient GameObject

在Unity场景中移动APIClient所在的GameObject：

```
Hierarchy:
  ModelContainer (Position: 0, 0, 0)
    └── APIClient Component
        └── Generated Model (自动添加)
```

#### 方法2: 指定父对象

```csharp
public class CustomModelDisplay : MonoBehaviour
{
    [SerializeField] private TripoConfig config;
    [SerializeField] private Transform modelParent; // 指定父对象
    
    void Start()
    {
        // 模型会显示在modelParent下
        var workflow = new TripoWorkflowManager(config, modelParent, this);
        workflow.StartWorkflow("Assets/images/character.jpg");
    }
}
```

#### 方法3: 手动加载到指定位置

```csharp
public class ManualModelLoader : MonoBehaviour
{
    [SerializeField] private Transform targetPosition;
    
    public IEnumerator LoadModelToPosition(string modelUrl)
    {
        var loader = new TripoModelLoader(targetPosition);
        yield return loader.LoadAndDisplayModel(modelUrl);
    }
}
```

## 🎨 模型显示增强

### 添加光照

```csharp
void SetupLighting(GameObject model)
{
    // 添加定向光
    GameObject light = new GameObject("Model Light");
    light.transform.SetParent(model.transform);
    Light lightComponent = light.AddComponent<Light>();
    lightComponent.type = LightType.Directional;
    lightComponent.intensity = 1.0f;
    light.transform.rotation = Quaternion.Euler(50, -30, 0);
}
```

### 添加旋转动画

```csharp
public class ModelRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 30f;
    
    void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
}

// 使用
void OnModelLoaded(GameObject model)
{
    model.AddComponent<ModelRotator>();
}
```

### 自动缩放到合适大小

```csharp
void AutoScaleModel(GameObject model, float targetSize = 2f)
{
    // 计算模型边界
    Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
    if (renderers.Length == 0) return;
    
    Bounds bounds = renderers[0].bounds;
    foreach (var renderer in renderers)
    {
        bounds.Encapsulate(renderer.bounds);
    }
    
    // 计算缩放比例
    float maxSize = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
    float scale = targetSize / maxSize;
    
    model.transform.localScale = Vector3.one * scale;
    
    Debug.Log($"🎯 Model auto-scaled to {scale:F2}x");
}
```

### 居中模型

```csharp
void CenterModel(GameObject model)
{
    Renderer[] renderers = model.GetComponentsInChildren<Renderer>();
    if (renderers.Length == 0) return;
    
    Bounds bounds = renderers[0].bounds;
    foreach (var renderer in renderers)
    {
        bounds.Encapsulate(renderer.bounds);
    }
    
    // 移动到中心
    Vector3 offset = bounds.center - model.transform.position;
    model.transform.position -= offset;
    
    Debug.Log($"🎯 Model centered at {model.transform.position}");
}
```

## 📊 获取模型信息

### 在工作流程完成后获取模型

```csharp
public class ModelInfoDisplay : MonoBehaviour
{
    private TripoWorkflowManager workflow;
    
    void Start()
    {
        workflow = new TripoWorkflowManager(config, transform, this);
        StartCoroutine(GenerateAndDisplayInfo());
    }
    
    IEnumerator GenerateAndDisplayInfo()
    {
        workflow.StartWorkflow(imagePath);
        
        // 等待模型加载
        yield return new WaitForSeconds(60); // 根据实际情况调整
        
        // 获取生成的模型
        Transform modelTransform = transform.GetChild(transform.childCount - 1);
        if (modelTransform != null)
        {
            DisplayModelInfo(modelTransform.gameObject);
        }
    }
    
    void DisplayModelInfo(GameObject model)
    {
        Debug.Log("=== Model Information ===");
        Debug.Log($"Name: {model.name}");
        Debug.Log($"Position: {model.transform.position}");
        Debug.Log($"Rotation: {model.transform.rotation.eulerAngles}");
        Debug.Log($"Scale: {model.transform.localScale}");
        
        // 网格信息
        MeshFilter[] meshes = model.GetComponentsInChildren<MeshFilter>();
        Debug.Log($"Mesh Count: {meshes.Length}");
        
        // 骨骼信息
        SkinnedMeshRenderer[] skinnedMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshes.Length > 0)
        {
            Debug.Log($"Has Skeleton: Yes");
            Debug.Log($"Skinned Meshes: {skinnedMeshes.Length}");
        }
    }
}
```

## 🎮 交互功能

### 添加鼠标拖拽旋转

```csharp
public class ModelDragRotate : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 5f;
    private Vector3 lastMousePosition;
    
    void OnMouseDrag()
    {
        Vector3 delta = Input.mousePosition - lastMousePosition;
        transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
        transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
        lastMousePosition = Input.mousePosition;
    }
    
    void OnMouseDown()
    {
        lastMousePosition = Input.mousePosition;
    }
}
```

### 添加缩放控制

```csharp
public class ModelZoom : MonoBehaviour
{
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 3f;
    
    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 newScale = transform.localScale + Vector3.one * scroll * zoomSpeed;
            newScale = Vector3.Max(Vector3.one * minScale, newScale);
            newScale = Vector3.Min(Vector3.one * maxScale, newScale);
            transform.localScale = newScale;
        }
    }
}
```

## 🎬 完整示例：增强的模型显示器

```csharp
using UnityEngine;
using UnityEngine.UI;
using Tripo3D;
using System.Collections;

public class EnhancedModelDisplay : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TripoConfig config;
    [SerializeField] private string imagePath = "Assets/images/character.jpg";
    
    [Header("UI")]
    [SerializeField] private Button generateButton;
    [SerializeField] private Text statusText;
    
    [Header("Display Settings")]
    [SerializeField] private Transform modelParent;
    [SerializeField] private bool autoRotate = true;
    [SerializeField] private bool autoScale = true;
    [SerializeField] private bool centerModel = true;
    [SerializeField] private float targetSize = 2f;
    
    [Header("Lighting")]
    [SerializeField] private bool addLight = true;
    [SerializeField] private Color lightColor = Color.white;
    [SerializeField] private float lightIntensity = 1f;
    
    private TripoWorkflowManager workflow;
    private GameObject currentModel;
    
    void Start()
    {
        if (modelParent == null)
            modelParent = transform;
            
        workflow = new TripoWorkflowManager(config, modelParent, this);
        generateButton.onClick.AddListener(OnGenerateClicked);
    }
    
    void OnGenerateClicked()
    {
        StartCoroutine(GenerateAndEnhance());
    }
    
    IEnumerator GenerateAndEnhance()
    {
        UpdateStatus("开始生成模型...");
        
        // 清除旧模型
        if (currentModel != null)
        {
            Destroy(currentModel);
        }
        
        // 开始工作流程
        workflow.StartWorkflow(imagePath);
        
        // 等待模型加载（监听子对象变化）
        int initialChildCount = modelParent.childCount;
        while (modelParent.childCount == initialChildCount)
        {
            yield return new WaitForSeconds(1f);
        }
        
        // 获取新加载的模型
        currentModel = modelParent.GetChild(modelParent.childCount - 1).gameObject;
        
        UpdateStatus("模型加载完成，应用增强效果...");
        
        // 应用增强效果
        yield return new WaitForSeconds(0.5f);
        
        if (autoScale)
        {
            AutoScaleModel(currentModel, targetSize);
        }
        
        if (centerModel)
        {
            CenterModel(currentModel);
        }
        
        if (autoRotate)
        {
            currentModel.AddComponent<ModelRotator>();
        }
        
        if (addLight)
        {
            SetupLighting(currentModel);
        }
        
        // 添加交互
        currentModel.AddComponent<ModelDragRotate>();
        currentModel.AddComponent<ModelZoom>();
        
        UpdateStatus("模型显示完成！");
        
        // 显示模型信息
        DisplayModelInfo(currentModel);
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
        float scale = targetSize / maxSize;
        model.transform.localScale = Vector3.one * scale;
        
        Debug.Log($"🎯 Model auto-scaled to {scale:F2}x");
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
    }
    
    void SetupLighting(GameObject model)
    {
        GameObject light = new GameObject("Model Light");
        light.transform.SetParent(model.transform);
        Light lightComponent = light.AddComponent<Light>();
        lightComponent.type = LightType.Directional;
        lightComponent.color = lightColor;
        lightComponent.intensity = lightIntensity;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        
        Debug.Log("💡 Lighting added to model");
    }
    
    void DisplayModelInfo(GameObject model)
    {
        Debug.Log("╔════════════════════════════════════════╗");
        Debug.Log("║         Model Information              ║");
        Debug.Log("╚════════════════════════════════════════╝");
        Debug.Log($"📦 Name: {model.name}");
        Debug.Log($"📍 Position: {model.transform.position}");
        Debug.Log($"📐 Scale: {model.transform.localScale}");
        
        MeshFilter[] meshes = model.GetComponentsInChildren<MeshFilter>();
        Debug.Log($"🎨 Meshes: {meshes.Length}");
        
        SkinnedMeshRenderer[] skinnedMeshes = model.GetComponentsInChildren<SkinnedMeshRenderer>();
        if (skinnedMeshes.Length > 0)
        {
            Debug.Log($"🦴 Has Skeleton: Yes ({skinnedMeshes.Length} skinned meshes)");
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
}
```

## 📋 Inspector设置

在Unity Inspector中设置：

```
EnhancedModelDisplay:
  Configuration:
    ☐ Config (TripoConfig)
    ☐ Image Path (string)
  
  UI:
    ☐ Generate Button (Button)
    ☐ Status Text (Text)
  
  Display Settings:
    ☐ Model Parent (Transform) - 可选，默认为当前对象
    ☑ Auto Rotate
    ☑ Auto Scale
    ☑ Center Model
    ☐ Target Size (2.0)
  
  Lighting:
    ☑ Add Light
    ☐ Light Color (White)
    ☐ Light Intensity (1.0)
```

## 🎯 常见场景

### 场景1: 展示台

```csharp
// 创建一个旋转展示台
GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
platform.transform.localScale = new Vector3(3, 0.1f, 3);
platform.transform.position = new Vector3(0, -0.5f, 0);

// 模型放在展示台上
modelParent.position = new Vector3(0, 0, 0);
```

### 场景2: 多模型对比

```csharp
public class ModelComparison : MonoBehaviour
{
    [SerializeField] private Transform[] displayPositions;
    
    public void LoadMultipleModels(string[] imagePaths)
    {
        for (int i = 0; i < imagePaths.Length && i < displayPositions.Length; i++)
        {
            var workflow = new TripoWorkflowManager(
                config, 
                displayPositions[i], 
                this
            );
            workflow.StartWorkflow(imagePaths[i]);
        }
    }
}
```

### 场景3: AR预览

```csharp
// 将模型放置在AR平面上
public class ARModelDisplay : MonoBehaviour
{
    void PlaceModelOnARPlane(GameObject model, Vector3 hitPosition)
    {
        model.transform.position = hitPosition;
        model.transform.localScale = Vector3.one * 0.1f; // AR通常需要更小的缩放
    }
}
```

## 🐛 故障排除

### 问题: 模型不显示

**检查清单**:
- ✓ 确认工作流程完成（查看日志）
- ✓ 检查模型父对象是否在相机视野内
- ✓ 检查模型缩放是否太小或太大
- ✓ 确认UniGLTF已正确安装

### 问题: 模型位置不对

**解决方案**:
```csharp
// 重置模型位置
model.transform.position = Vector3.zero;
model.transform.rotation = Quaternion.identity;
```

### 问题: 模型太大或太小

**解决方案**:
```csharp
// 使用自动缩放
AutoScaleModel(model, 2f); // 目标大小2单位
```

## 📚 相关文档

- [README.md](README.md) - 基础使用
- [README_Structure.md](README_Structure.md) - TripoModelLoader详解
- [QUICK_REFERENCE.md](QUICK_REFERENCE.md) - 快速参考

---

**提示**: 模型显示功能已经内置在工作流程中，开箱即用！
