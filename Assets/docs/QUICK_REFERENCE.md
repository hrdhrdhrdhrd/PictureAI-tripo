# Tripo3D Unity SDK - 快速参考

## 🚀 5分钟快速开始

```
1. 创建配置: 右键 → Create → Tripo3D → Configuration
2. 设置API Key
3. 添加APIClient组件到GameObject
4. 分配Button、Config、ImagePath
5. 点击按钮运行
```

## 📁 文件速查

| 文件 | 用途 | 何时使用 |
|------|------|----------|
| `APIClient.cs` | 主控制器 | 场景设置 |
| `TripoConfig.cs` | 配置管理 | 调整参数 |
| `TripoAPIService.cs` | API请求 | 自定义API调用 |
| `TripoWorkflowManager.cs` | 流程管理 | 自定义工作流 |
| `TripoModelLoader.cs` | 模型加载 | 自定义加载逻辑 |
| `TripoLogger.cs` | 日志系统 | 添加自定义日志 |
| `TripoDataModels.cs` | 数据模型 | 理解API结构 |

## 🎯 常用代码片段

### 基础使用
```csharp
using Tripo3D;

public class MyController : MonoBehaviour
{
    [SerializeField] private TripoConfig config;
    private TripoWorkflowManager workflow;
    
    void Start()
    {
        workflow = new TripoWorkflowManager(config, transform, this);
    }
    
    public void Generate()
    {
        workflow.StartWorkflow("Assets/images/character.jpg");
    }
}
```

### 单独上传图片
```csharp
var apiService = new TripoAPIService(config);
byte[] imageData = File.ReadAllBytes(imagePath);

StartCoroutine(apiService.UploadImage(
    imageData, 
    "image.jpg", 
    token => Debug.Log($"Token: {token}")
));
```

### 创建任务
```csharp
StartCoroutine(apiService.CreateModelTask(
    imageToken,
    taskId => Debug.Log($"Task ID: {taskId}")
));
```

### 轮询状态
```csharp
StartCoroutine(apiService.PollTaskStatus<TaskData>(
    taskId,
    "PollTask",
    json => JsonUtility.FromJson<TaskData>(json),
    data => data.data.status,
    data => data.data.progress,
    data => data.data.output?.pbr_model,
    data => data.data.create_time,
    url => Debug.Log($"Model URL: {url}")
));
```

### 加载模型
```csharp
var modelLoader = new TripoModelLoader(transform);
StartCoroutine(modelLoader.LoadAndDisplayModel(modelUrl));
```

### 下载模型
```csharp
await modelLoader.DownloadModelAsync(
    modelUrl, 
    "model.glb", 
    sessionId
);
```

### 自定义日志
```csharp
TripoLogger.LogStep("My Custom Step");
TripoLogger.LogError("Something went wrong");
TripoLogger.LogStepComplete("My Step", startTime);
```

## ⚙️ 配置速查

### 必需配置
```
API Key: your_api_key_here
```

### 推荐配置
```
Max Retries: 3
Retry Delay: 3s
Polling Interval: 2s
Request Timeout: 30s
```

### 模型质量配置
```
Model Version: v3.0-20250812
Texture Quality: detailed
Use Quad Mesh: true
Generate PBR: true
```

## 🔍 日志标签速查

| 标签 | 含义 | 何时出现 |
|------|------|----------|
| `[Upload]` | 图片上传 | 步骤1 |
| `[CreateTask]` | 创建模型任务 | 步骤2 |
| `[PollTask]` | 轮询模型生成 | 步骤3 |
| `[RiggingTask]` | 创建骨骼任务 | 步骤4 |
| `[PollRigging]` | 轮询骨骼绑定 | 步骤5 |
| `[ConvertTask]` | 创建转换任务 | 步骤6 |
| `[PollConversion]` | 轮询格式转换 | 步骤7 |
| `[LoadModel]` | 加载模型 | 步骤8 |
| `[Download]` | 下载模型 | 步骤9 |
| `[Error]` | 错误信息 | 任何失败 |

## 🐛 快速调试

### 问题: 按钮无反应
```
✓ 检查Config是否分配
✓ 检查API Key是否设置
✓ 查看Console错误
```

### 问题: 上传失败
```
✓ 检查图片路径
✓ 检查文件是否存在
✓ 检查网络连接
✓ 检查API Key有效性
```

### 问题: 任务一直轮询
```
✓ 查看日志中的status
✓ 检查API配额
✓ 访问Tripo3D控制台
```

### 问题: 模型加载失败
```
✓ 检查UniGLTF是否安装
✓ 查看模型URL是否有效
✓ 检查网络连接
```

## 📊 性能参考

| 操作 | 预计时间 |
|------|----------|
| 上传图片 | 1-3秒 |
| 模型生成 | 10-30秒 |
| 骨骼绑定 | 20-40秒 |
| 格式转换 | 10-20秒 |
| 模型下载 | 2-5秒 |
| **总计** | **45-100秒** |

## 🎨 日志符号速查

| 符号 | 含义 |
|------|------|
| ✅ | 成功 |
| ❌ | 失败 |
| ⚠️ | 警告 |
| 🔄 | 重试 |
| 📊 | 统计 |
| ⏱️ | 时间 |
| 📦 | 大小 |
| 🔗 | URL |
| 🆔 | ID |
| 📍 | 步骤 |
| 🦴 | 骨骼 |
| 🎮 | 模型 |
| 💾 | 下载 |

## 🔗 快速链接

| 资源 | 链接 |
|------|------|
| 完整文档 | [README.md](README.md) |
| 架构说明 | [README_Structure.md](README_Structure.md) |
| 迁移指南 | [MIGRATION_GUIDE.md](MIGRATION_GUIDE.md) |
| 架构图 | [ARCHITECTURE.md](ARCHITECTURE.md) |
| 日志示例 | [LOG_EXAMPLE.md](../LOG_EXAMPLE.md) |
| 文档索引 | [DOCUMENTATION_INDEX.md](DOCUMENTATION_INDEX.md) |
| Tripo3D API | https://platform.tripo3d.ai/docs |

## 💡 最佳实践

### ✓ 推荐做法
- 使用ScriptableObject配置
- 查看详细日志
- 先测试小图片
- 为不同环境创建不同配置
- 使用日志标签快速定位问题

### ✗ 避免做法
- 硬编码API Key
- 忽略错误日志
- 直接修改核心代码
- 跳过配置步骤
- 在生产环境测试大文件

## 🎯 工作流程速记

```
Upload → Generate → Rig → Convert → Load → Download
  1-3s    10-30s    20-40s   10-20s    2-5s
```

## 📝 Inspector设置清单

```
APIClient组件:
  ☐ Generate Button (UI Button)
  ☐ Config (TripoConfig)
  ☐ Image Path (string)

TripoConfig资源:
  ☐ API Key
  ☐ Max Retries (3)
  ☐ Retry Delay (3s)
  ☐ Polling Interval (2s)
  ☐ Request Timeout (30s)
```

## 🚨 常见错误代码

| 错误码 | 含义 | 解决方案 |
|--------|------|----------|
| 401 | 未授权 | 检查API Key |
| 404 | 未找到 | 检查端点URL |
| 429 | 请求过多 | 等待或升级配额 |
| 500 | 服务器错误 | 稍后重试 |

## 📞 获取帮助顺序

```
1. 查看本文档
2. 查看Console日志
3. 查看完整文档
4. 查看源代码注释
5. 提交Issue
```

---

**提示**: 将此页面加入书签，随时快速查阅！

**打印友好**: 此文档设计为可打印的快速参考卡片。
