# Tripo3D Unity SDK

一个用于Unity的Tripo3D API客户端，支持从图片生成带骨骼的3D模型。

## ✨ 特性

- 🖼️ 从图片生成3D模型
- 🦴 自动骨骼绑定（Rigging）
- 🔄 格式转换（GLTF/FBX）
- 📊 详细的日志系统
- ⚙️ 可配置的参数
- 🎮 自动加载到Unity场景
- 💾 本地模型保存

## 🚀 快速开始

### 1. 创建配置文件

在Unity中：
```
右键 → Create → Tripo3D → Configuration
```

### 2. 配置API密钥

在Inspector中设置你的Tripo3D API密钥：
```csharp
API Key: your_api_key_here
```

### 3. 设置场景

1. 创建一个空GameObject
2. 添加`APIClient`组件
3. 分配以下引用：
   - Generate Button（UI按钮）
   - Config（刚创建的TripoConfig）
   - Image Path（图片路径，如：Assets/images/character.jpg）

### 4. 运行

点击按钮，等待模型生成完成！

## 📁 项目结构

```
Assets/
├── APIClient.cs                    # 主控制器   80%kiro编写
├── docs/     100%kiro编写
│   ├── ARCHITECTURE.md         # 架构图 
│   ├── DOCUMENTATION_INDEX.md         # 文档索引 
│   ├── LOG_EXAMPLE.md         # 日志示例
│   ├── MIGRATION_GUIDE.md         # 迁移指南 - 从旧代码到新架构 
│   ├── MODEL_DISPLAY_GUIDE.md         # 3D模型显示功能使用指南 
│   ├── QUICK_REFERENCE.md         # SDK快速参考
│   ├── README_Rigging.md         # 带骨骼模型生成功能 
│   ├── README_Structure.md         # 代码结构说明
└── Tripo3D/  90%kiro编写  
    ├── TripoConfig.cs              # 配置（ScriptableObject）
    ├── TripoAPIService.cs          # API请求服务
    ├── TripoWorkflowManager.cs     # 工作流程管理
    ├── TripoModelLoader.cs         # 模型加载器
    ├── TripoLogger.cs              # 日志系统
    ├── TripoDataModels.cs          # 数据模型
    └── README_Structure.md         # 详细架构文档
```

## 🎯 工作流程

```
1. 上传图片 📤
   ↓
2. 创建模型生成任务 🎨
   ↓
3. 等待模型生成 ⏳
   ↓
4. 创建骨骼绑定任务 🦴
   ↓
5. 等待骨骼绑定 ⏳
   ↓
6. 创建格式转换任务 🔄
   ↓
7. 等待格式转换 ⏳
   ↓
8. 加载并显示模型 🎮
   ↓
9. 下载到本地 💾
```

## ⚙️ 配置选项

### API设置
- **API Key**: 你的Tripo3D API密钥
- **Base URL**: API基础地址（默认：https://api.tripo3d.ai/v2/openapi）

### 请求设置
- **Max Retries**: 失败重试次数（默认：3）
- **Retry Delay**: 重试延迟秒数（默认：3s）
- **Polling Interval**: 轮询间隔（默认：2s）
- **Request Timeout**: 请求超时（默认：30s）

### 模型生成设置
- **Model Version**: 模型版本（默认：v3.0-20250812）
- **Use Quad Mesh**: 使用四边形网格（默认：true）
- **Generate Texture**: 生成纹理（默认：true）
- **Generate PBR**: 生成PBR材质（默认：true）
- **Texture Quality**: 纹理质量（默认：detailed）

### 导出设置
- **Export Format**: 导出格式（默认：GLTF）
- **Texture Format**: 纹理格式（默认：PNG）
- **FBX Preset**: FBX预设（默认：3dsmax）

## 📊 日志示例

```
╔════════════════════════════════════════════════════════════╗
║          TRIPO3D MODEL GENERATION STARTED                  ║
╚════════════════════════════════════════════════════════════╝
📋 Session ID: 12345678-1234-1234-1234-123456789012
🖼️  Image Path: Assets/images/character.jpg
⚙️  Configuration:
   - Max Retries: 3
   - Retry Delay: 3s
   - Polling Interval: 2s
🕐 Start Time: 2024-03-11 14:30:25
════════════════════════════════════════════════════════════

────────────────────────────────────────────────────────────
📍 Step 1/9: Uploading Image
⏱️  Elapsed Time: 0.5s
────────────────────────────────────────────────────────────
📤 [Upload] Reading image from: Assets/images/character.jpg
📦 [Upload] File size: 2.45 MB
✅ [Upload] Success!
   🎫 Token: abc123def456
   ⏱️  Duration: 1.23s
✅ Image Upload completed in 1.25s

... (更多步骤)

╔════════════════════════════════════════════════════════════╗
║          WORKFLOW COMPLETED SUCCESSFULLY! 🎉               ║
╚════════════════════════════════════════════════════════════╝
✅ Status: SUCCESS
📊 Steps Completed: 9/9
⏱️  Total Duration: 1m 5s
💾 Model saved to: Application.persistentDataPath/models/session_id/
```

## 🔧 高级用法

### 编程方式使用

```csharp
using Tripo3D;

public class MyController : MonoBehaviour
{
    [SerializeField] private TripoConfig config;
    
    private TripoWorkflowManager workflowManager;
    
    void Start()
    {
        workflowManager = new TripoWorkflowManager(config, transform, this);
    }
    
    public void GenerateModel(string imagePath)
    {
        workflowManager.StartWorkflow(imagePath);
    }
}
```

### 单独使用API服务

```csharp
var apiService = new TripoAPIService(config);

// 上传图片
StartCoroutine(apiService.UploadImage(imageData, "image.jpg", token => {
    Debug.Log($"Image token: {token}");
    
    // 创建任务
    StartCoroutine(apiService.CreateModelTask(token, taskId => {
        Debug.Log($"Task ID: {taskId}");
    }));
}));
```

### 自定义模型加载

```csharp
var modelLoader = new TripoModelLoader(transform);

// 加载模型
StartCoroutine(modelLoader.LoadAndDisplayModel("https://model-url.glb"));

// 下载模型
await modelLoader.DownloadModelAsync("https://model-url.glb", "model.glb", sessionId);
```

## 🎨 架构设计

### 设计原则

1. **单一职责**: 每个类只负责一个功能
2. **依赖注入**: 通过构造函数注入依赖
3. **配置分离**: 使用ScriptableObject管理配置
4. **日志集中**: 统一的日志管理系统
5. **错误处理**: 完善的错误处理和重试机制

### 核心组件

| 组件 | 职责 | 依赖 |
|------|------|------|
| APIClient | UI控制器 | TripoWorkflowManager |
| TripoWorkflowManager | 工作流程编排 | TripoAPIService, TripoModelLoader |
| TripoAPIService | API请求处理 | TripoConfig |
| TripoModelLoader | 模型加载 | - |
| TripoLogger | 日志管理 | - |
| TripoConfig | 配置管理 | - |

## 📚 文档

- **README_Structure.md** - 详细的架构和代码结构说明
- **LOG_EXAMPLE.md** - 完整的日志示例
- **README_Rigging.md** - 骨骼绑定功能说明

## 🔍 调试

### 常见问题

**Q: 按钮点击没反应？**
- 检查TripoConfig是否已分配
- 检查API Key是否正确
- 查看Console是否有错误信息

**Q: 上传失败？**
- 检查图片路径是否正确
- 检查文件是否存在
- 检查网络连接

**Q: 任务一直在轮询？**
- 检查API配额是否充足
- 查看日志中的任务状态
- 检查Tripo3D服务状态

### 日志标签

使用以下标签快速定位问题：
- `[Upload]` - 图片上传
- `[CreateTask]` - 任务创建
- `[PollTask]` - 模型生成轮询
- `[RiggingTask]` - 骨骼绑定
- `[PollRigging]` - 骨骼绑定轮询
- `[ConvertTask]` - 格式转换
- `[PollConversion]` - 转换轮询
- `[LoadModel]` - 模型加载
- `[Download]` - 模型下载
- `[Error]` - 错误信息

## 📦 依赖

- **Unity 2020.3+**
- **UniGLTF** - GLTF模型加载（需要从Package Manager安装）

## 🤝 贡献

欢迎提交Issue和Pull Request！

## 📄 许可

MIT License

## 🔗 相关链接

- [Tripo3D官网](https://www.tripo3d.ai/)
- [Tripo3D API文档](https://platform.tripo3d.ai/docs)
- [UniGLTF](https://github.com/vrm-c/UniVRM)

## 📞 支持

如有问题，请查看：
1. 本文档和相关文档
2. Unity Console日志
3. Tripo3D API文档
4. 提交Issue

---

**Made with ❤️ for Unity Developers**
