# 迁移指南 - 从旧代码到新架构

本指南帮助你从单文件的APIClient迁移到新的模块化架构。

## 🔄 主要变化

### 代码组织

**旧架构**:
```
Assets/
├── APIClient.cs (800+ 行)
└── TaskData.cs
```

**新架构**:
```
Assets/
├── APIClient.cs (60 行)
└── Tripo3D/
    ├── TripoConfig.cs
    ├── TripoAPIService.cs
    ├── TripoWorkflowManager.cs
    ├── TripoModelLoader.cs
    ├── TripoLogger.cs
    └── TripoDataModels.cs
```

## 📋 迁移步骤

### 步骤 1: 创建配置文件

1. 在Unity中右键 → Create → Tripo3D → Configuration
2. 命名为 `TripoConfig`
3. 设置API Key和其他参数

### 步骤 2: 更新场景引用

**旧代码**:
```csharp
public class APIClient : MonoBehaviour
{
    public Button yourButton;
    private const string API_KEY = "your_key";
}
```

**新代码**:
```csharp
public class APIClient : MonoBehaviour
{
    [SerializeField] private Button generateButton;
    [SerializeField] private TripoConfig config;
    [SerializeField] private string imagePath;
}
```

在Inspector中：
1. 将 `yourButton` 引用改为 `generateButton`
2. 添加 `TripoConfig` 引用
3. 设置 `imagePath`

### 步骤 3: 删除旧文件（可选）

如果确认新代码工作正常，可以删除：
- 旧的 `Assets/APIClient.cs`（如果有备份）
- `Assets/TaskData.cs`（已移至 Tripo3D/TripoDataModels.cs）

## 🔧 代码对比

### 初始化

**旧代码**:
```csharp
void Start()
{
    if (yourButton != null)
    {
        yourButton.onClick.AddListener(() =>
        {
            newGuid = Guid.NewGuid().ToString();
            string FILE_PATH = @"Assets/images/7.jpg";
            StartCoroutine(UploadImage(FILE_PATH));
        });
    }
}
```

**新代码**:
```csharp
private void Start()
{
    _workflowManager = new TripoWorkflowManager(config, transform, this);
    generateButton.onClick.AddListener(OnGenerateButtonClicked);
}

private void OnGenerateButtonClicked()
{
    _workflowManager.StartWorkflow(imagePath);
}
```

### 上传图片

**旧代码**:
```csharp
IEnumerator UploadImage(string FILE_PATH)
{
    byte[] fileData = File.ReadAllBytes(FILE_PATH);
    WWWForm form = new WWWForm();
    form.AddBinaryData("file", fileData, FILE_PATH, "image/jpeg");
    
    using (UnityWebRequest request = UnityWebRequest.Post(UPLOAD_URL, form))
    {
        request.SetRequestHeader("Authorization", $"Bearer {API_KEY}");
        yield return request.SendWebRequest();
        // ... 处理响应
    }
}
```

**新代码**:
```csharp
// 在 TripoAPIService.cs 中
public IEnumerator UploadImage(byte[] fileData, string fileName, Action<string> onSuccess)
{
    WWWForm form = new WWWForm();
    form.AddBinaryData("file", fileData, fileName, "image/jpeg");
    
    using (UnityWebRequest request = UnityWebRequest.Post(_config.UploadEndpoint, form))
    {
        ConfigureRequest(request);
        yield return request.SendWebRequest();
        // ... 处理响应
    }
}
```

### 创建任务

**旧代码**:
```csharp
public IEnumerator CreateTask(string file_token)
{
    RequestData requestData = new RequestData
    {
        type = "image_to_model",
        // ... 其他参数硬编码
    };
    
    string jsonData = JsonUtility.ToJson(requestData);
    // ... 发送请求
}
```

**新代码**:
```csharp
// 在 TripoAPIService.cs 中
public IEnumerator CreateModelTask(string fileToken, Action<string> onSuccess)
{
    RequestData requestData = new RequestData
    {
        type = "image_to_model",
        model_version = _config.modelVersion,  // 从配置读取
        quad = _config.useQuadMesh,
        // ... 其他参数从配置读取
    };
    
    yield return CreateTask(requestData, "CreateTask", onSuccess);
}
```

### 轮询状态

**旧代码**:
```csharp
IEnumerator FetchTaskData(string task_id)
{
    bool isCompleted = false;
    while (!isCompleted)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(TASK_URL))
        {
            // ... 轮询逻辑
            if (taskData.data.status == "success")
            {
                isCompleted = true;
                DownloadModelAsync(model_url, "1.fbx");
            }
        }
    }
}
```

**新代码**:
```csharp
// 在 TripoAPIService.cs 中 - 泛型方法支持多种任务类型
public IEnumerator PollTaskStatus<T>(
    string taskId, 
    string taskType,
    Func<string, T> parseResponse,
    Func<T, string> getStatus,
    // ... 其他参数
    Action<string> onSuccess)
{
    while (true)
    {
        // ... 统一的轮询逻辑
        if (status == "success")
        {
            onSuccess?.Invoke(modelUrl);
            yield break;
        }
    }
}
```

## 🎯 功能映射

| 旧功能 | 新位置 | 说明 |
|--------|--------|------|
| `UploadImage()` | `TripoAPIService.UploadImage()` | 更好的错误处理和重试 |
| `CreateTask()` | `TripoAPIService.CreateModelTask()` | 参数可配置 |
| `ConvertTask()` | `TripoAPIService.CreateConversionTask()` | 统一的任务创建 |
| `FetchTaskData()` | `TripoAPIService.PollTaskStatus<TaskData>()` | 泛型支持多种任务 |
| `FinalFetchTaskData()` | `TripoAPIService.PollTaskStatus<FinalTaskData>()` | 同上 |
| `DownloadModelAsync()` | `TripoModelLoader.DownloadModelAsync()` | 独立的模型加载器 |
| `LoadGltfFromBytes()` | `TripoModelLoader.LoadGltfFromBytes()` | 同上 |
| 日志输出 | `TripoLogger.*` | 集中的日志管理 |
| 硬编码配置 | `TripoConfig` | ScriptableObject配置 |

## ✨ 新功能优势

### 1. 配置管理

**旧方式**: 硬编码在代码中
```csharp
private const string API_KEY = "tsk_...";
private string URL = "https://api.tripo3d.ai/v2/openapi/task";
int maxRetries = 3;
```

**新方式**: ScriptableObject配置
```csharp
[SerializeField] private TripoConfig config;
// 在Inspector中可视化配置所有参数
```

### 2. 日志系统

**旧方式**: 分散的Debug.Log
```csharp
Debug.Log("Upload successful!");
Debug.Log($"Task ID: {taskId}");
```

**新方式**: 统一的日志系统
```csharp
TripoLogger.LogUploadSuccess(token, duration, size);
TripoLogger.LogTaskCreation(taskType, taskId, duration);
```

### 3. 错误处理

**旧方式**: 简单的错误输出
```csharp
Debug.LogError($"Error: {request.error}");
```

**新方式**: 详细的错误信息
```csharp
TripoLogger.LogRequestError(request, operation, attempt, maxAttempts);
// 输出: HTTP状态码、错误信息、重试次数、响应内容
```

### 4. 代码复用

**旧方式**: 重复的代码
```csharp
// CreateTask, ConvertTask, RiggingTask 有大量重复代码
```

**新方式**: 泛型和共享方法
```csharp
// 一个泛型方法处理所有任务类型
PollTaskStatus<T>(...)
```

## 🔍 常见问题

### Q: 旧代码中的 `newGuid` 在哪里？

A: 现在由 `TripoWorkflowManager` 管理，作为 `_currentSessionId`。

### Q: 如何访问上传的token？

A: 通过回调函数：
```csharp
yield return _apiService.UploadImage(data, fileName, token => {
    // 使用 token
});
```

### Q: 配置文件保存在哪里？

A: TripoConfig是ScriptableObject，保存在Assets文件夹中，可以创建多个配置用于不同场景。

### Q: 如何自定义工作流程？

A: 可以直接使用 `TripoAPIService` 和 `TripoModelLoader`，不使用 `TripoWorkflowManager`：

```csharp
var apiService = new TripoAPIService(config);
var modelLoader = new TripoModelLoader(transform);

// 自定义流程
StartCoroutine(apiService.UploadImage(...));
// ... 自定义步骤
```

### Q: 旧代码的日志还能看到吗？

A: 新日志系统更详细，包含所有旧日志的信息，并增加了：
- 时间统计
- 文件大小
- 进度百分比
- 网格统计
- 骨骼信息

## 📊 性能对比

| 指标 | 旧代码 | 新代码 |
|------|--------|--------|
| 代码行数 | ~800行 | ~60行（主文件） |
| 可维护性 | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| 可测试性 | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| 可扩展性 | ⭐⭐ | ⭐⭐⭐⭐⭐ |
| 日志详细度 | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| 配置灵活性 | ⭐⭐ | ⭐⭐⭐⭐⭐ |

## 🎓 学习资源

1. **README.md** - 快速开始指南
2. **README_Structure.md** - 详细架构说明
3. **LOG_EXAMPLE.md** - 日志示例
4. **代码注释** - 每个类和方法都有详细注释

## 💡 最佳实践

1. **使用配置文件**: 为不同环境创建不同的TripoConfig
2. **查看日志**: 使用日志标签快速定位问题
3. **错误处理**: 检查回调中的null值
4. **测试**: 先在小图片上测试，确认流程正常

## 🚀 下一步

1. 完成迁移后，测试所有功能
2. 根据需要调整TripoConfig参数
3. 查看日志输出，熟悉新的日志格式
4. 尝试自定义工作流程

---

如有问题，请参考其他文档或提交Issue。
