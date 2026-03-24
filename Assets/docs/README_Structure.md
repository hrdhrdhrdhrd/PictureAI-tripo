# Tripo3D Unity SDK - 代码结构说明

## 📁 文件组织

```
Assets/
├── APIClient.cs                          # 主控制器（简化版）
└── Tripo3D/
    ├── TripoConfig.cs                    # 配置管理
    ├── TripoAPIService.cs                # API请求服务
    ├── TripoWorkflowManager.cs           # 工作流程管理
    ├── TripoModelLoader.cs               # 模型加载器
    ├── TripoLogger.cs                    # 日志系统
    └── TripoDataModels.cs                # 数据模型定义
```

## 🎯 各文件职责

### 1. APIClient.cs
**职责**: 主入口点，UI交互控制器

**功能**:
- 管理UI按钮事件
- 初始化工作流程管理器
- 作为Unity MonoBehaviour的桥梁

**使用方式**:
```csharp
// 在Unity Inspector中配置
[SerializeField] private Button generateButton;
[SerializeField] private TripoConfig config;
[SerializeField] private string imagePath;
```

### 2. TripoConfig.cs (ScriptableObject)
**职责**: 集中管理所有配置参数

**功能**:
- API密钥和端点配置
- 请求设置（重试、超时、轮询间隔）
- 模型生成参数
- 导出格式设置

**创建方式**:
```
右键 → Create → Tripo3D → Configuration
```

**可配置项**:
- API Key
- 重试次数、延迟
- 轮询间隔
- 模型版本、质量
- 导出格式

### 3. TripoAPIService.cs
**职责**: 处理所有API请求

**功能**:
- 图片上传
- 任务创建（模型生成、骨骼绑定、格式转换）
- 任务状态轮询
- 请求配置和错误处理

**主要方法**:
```csharp
IEnumerator UploadImage(byte[] fileData, string fileName, Action<string> onSuccess)
IEnumerator CreateModelTask(string fileToken, Action<string> onSuccess)
IEnumerator CreateRiggingTask(string originalTaskId, Action<string> onSuccess)
IEnumerator CreateConversionTask(string originalTaskId, Action<string> onSuccess)
IEnumerator PollTaskStatus<T>(...)
```

### 4. TripoWorkflowManager.cs
**职责**: 编排完整的模型生成工作流程

**功能**:
- 管理9步工作流程
- 协调API服务和模型加载器
- 处理步骤间的数据传递
- 错误处理和工作流程中断

**工作流程**:
1. 上传图片
2. 创建模型生成任务
3. 轮询模型生成状态
4. 创建骨骼绑定任务
5. 轮询骨骼绑定状态
6. 创建格式转换任务
7. 轮询转换状态
8. 加载并显示模型
9. 下载模型到本地

### 5. TripoModelLoader.cs
**职责**: 处理模型加载和下载

**功能**:
- 从URL下载GLTF模型
- 解析并加载模型到场景
- 收集模型统计信息（网格、顶点、骨骼）
- 异步下载模型到本地存储

**主要方法**:
```csharp
IEnumerator LoadAndDisplayModel(string gltfUrl)
Task DownloadModelAsync(string modelUrl, string fileName, string sessionId)
```

### 6. TripoLogger.cs
**职责**: 统一的日志管理系统

**功能**:
- 格式化日志输出
- 工作流程追踪
- 性能指标记录
- 错误和警告日志
- 进度可视化

**日志类型**:
- 工作流程开始/结束
- 步骤进度
- 操作成功/失败
- 性能统计
- 文件大小格式化
- 时间格式化

### 7. TripoDataModels.cs
**职责**: 定义所有API数据模型

**包含**:
- 请求模型（RequestData, RequestRiggingTaskData, RequestConvertTaskData）
- 响应模型（UploadResponse, TaskData, RiggingTaskData, FinalTaskData）
- 数据传输对象

## 🚀 使用指南

### 快速开始

1. **创建配置文件**
   ```
   右键 → Create → Tripo3D → Configuration
   ```

2. **配置API密钥**
   - 在Inspector中设置API Key
   - 调整其他参数（可选）

3. **设置场景**
   - 添加APIClient组件到GameObject
   - 分配Button引用
   - 分配TripoConfig引用
   - 设置图片路径

4. **运行**
   - 点击按钮开始生成

### 高级用法

#### 自定义工作流程

```csharp
var config = ScriptableObject.CreateInstance<TripoConfig>();
var workflowManager = new TripoWorkflowManager(config, transform, this);
workflowManager.StartWorkflow("path/to/image.jpg");
```

#### 单独使用API服务

```csharp
var apiService = new TripoAPIService(config);
StartCoroutine(apiService.UploadImage(imageData, "image.jpg", token => {
    Debug.Log($"Token: {token}");
}));
```

#### 自定义日志

```csharp
TripoLogger.LogStep("Custom Step");
TripoLogger.LogError("Custom Error");
```

## 🎨 设计模式

### 1. 单一职责原则 (SRP)
每个类只负责一个功能领域

### 2. 依赖注入
通过构造函数注入依赖，便于测试

### 3. 回调模式
使用Action回调处理异步结果

### 4. 泛型方法
PollTaskStatus使用泛型支持不同任务类型

### 5. ScriptableObject配置
配置与代码分离，便于管理

## 📊 优势

### 相比原代码的改进

1. **可维护性** ✅
   - 代码分离清晰
   - 每个文件职责单一
   - 易于定位和修改

2. **可测试性** ✅
   - 依赖注入
   - 接口清晰
   - 易于mock

3. **可扩展性** ✅
   - 添加新功能不影响现有代码
   - 配置化设计
   - 泛型支持

4. **可读性** ✅
   - 代码组织清晰
   - 命名规范
   - 注释完整

5. **可重用性** ✅
   - 各组件独立
   - 可单独使用
   - 易于集成

## 🔧 扩展示例

### 添加新的任务类型

1. 在TripoDataModels.cs添加数据模型
2. 在TripoAPIService.cs添加创建方法
3. 在TripoWorkflowManager.cs集成到工作流程

### 自定义日志输出

```csharp
// 在TripoLogger.cs添加新方法
public static void LogCustomEvent(string message)
{
    Debug.Log($"🎯 [Custom] {message}");
}
```

### 支持其他模型格式

```csharp
// 在TripoConfig.cs添加新格式选项
public enum ExportFormat
{
    GLTF,
    FBX,
    OBJ
}
```

## 📝 注意事项

1. **命名空间**: 所有Tripo3D相关类都在`Tripo3D`命名空间下
2. **依赖**: 需要UniGLTF包来加载GLTF模型
3. **协程**: 需要MonoBehaviour来运行协程
4. **配置**: 必须创建并分配TripoConfig资源

## 🐛 调试技巧

1. 使用日志标签快速定位问题
2. 检查TripoConfig配置是否正确
3. 查看Unity Console的详细错误信息
4. 使用断点调试工作流程

## 📚 相关文档

- LOG_EXAMPLE.md - 日志示例
- README_Rigging.md - 骨骼绑定说明
- Tripo3D API文档 - https://platform.tripo3d.ai/docs
