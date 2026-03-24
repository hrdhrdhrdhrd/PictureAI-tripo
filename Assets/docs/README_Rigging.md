# Tripo3D 带骨骼模型生成功能

## 功能说明

此代码实现了使用 Tripo3D API 从图片生成带骨骼（rigging）的3D模型的完整流程。

## 工作流程

1. **上传图片** - 将图片上传到 Tripo3D 服务器，获取 image_token
2. **创建模型生成任务** - 使用 image_token 创建 image_to_model 任务
3. **轮询模型生成状态** - 等待模型生成完成
4. **创建骨骼绑定任务** - 使用 rig_model 类型为模型添加骨骼
5. **轮询骨骼绑定状态** - 等待骨骼绑定完成
6. **转换模型格式** - 将模型转换为 GLTF 格式
7. **轮询转换状态** - 等待格式转换完成
8. **加载并显示模型** - 在 Unity 场景中显示生成的模型
9. **下载模型** - 将模型保存到本地持久化存储

## 使用方法

### 1. 在 Unity Inspector 中配置

- **Generate Button**: 拖入触发生成的 UI Button
- **Image Path**: 设置要转换的图片路径（默认: Assets/images/7.jpg）
- **Max Retries**: 上传失败时的最大重试次数（默认: 3）
- **Retry Delay**: 重试间隔秒数（默认: 3秒）
- **Polling Interval**: 轮询任务状态的间隔（默认: 2秒）
- **Request Timeout**: 请求超时时间（默认: 30秒）

### 2. 运行

点击配置的按钮即可开始生成流程。

## API 端点

- **上传图片**: `POST https://api.tripo3d.ai/v2/openapi/upload/sts`
- **创建任务**: `POST https://api.tripo3d.ai/v2/openapi/task`
- **查询任务**: `GET https://api.tripo3d.ai/v2/openapi/task/{task_id}`

## 任务类型

### image_to_model
从图片生成3D模型的基础任务。

参数：
- `type`: "image_to_model"
- `model_version`: "v3.0-20250812"
- `quad`: true (四边形网格)
- `texture`: true (生成纹理)
- `pbr`: true (PBR材质)
- `texture_quality`: "detailed" (详细纹理)
- `orientation`: "align_image" (对齐图片)

### rig_model
为模型添加骨骼的任务。

参数：
- `type`: "rig_model"
- `original_model_task_id`: 原始模型任务ID

### convert_model
转换模型格式的任务。

参数：
- `type`: "convert_model"
- `format`: "GLTF"
- `texture_format`: "PNG"
- `original_model_task_id`: 要转换的模型任务ID

## 输出

- 生成的模型会显示在场景中（作为当前 GameObject 的子对象）
- 模型会保存到: `Application.persistentDataPath/models/{sessionId}/rigged_model.glb`

## 日志标签

代码使用以下日志标签便于调试：
- `[Upload]` - 图片上传相关
- `[CreateTask]` - 创建模型生成任务
- `[PollTask]` - 轮询模型生成状态
- `[RiggingTask]` - 创建骨骼绑定任务
- `[PollRigging]` - 轮询骨骼绑定状态
- `[ConvertTask]` - 创建格式转换任务
- `[PollConversion]` - 轮询转换状态
- `[LoadModel]` - 加载和显示模型
- `[Download]` - 下载模型到本地
- `[Error]` - 错误信息

## 注意事项

1. 确保 API_KEY 有效且有足够的配额
2. 图片路径必须存在且可访问
3. 骨骼绑定适用于人形或类人形模型
4. 整个流程可能需要几分钟时间，请耐心等待
5. 需要安装 UniGLTF 包来加载 GLTF 模型

## 错误处理

代码包含完整的错误处理：
- 自动重试机制（上传失败时）
- 详细的错误日志
- 任务失败检测
- 网络错误恢复

## 数据模型

所有 API 请求和响应的数据模型定义在 `TaskData.cs` 文件中。
