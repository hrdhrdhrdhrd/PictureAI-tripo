# Tripo3D API Client - 日志示例

## 日志特性

### 🎨 丰富的视觉标识
- ✅ 成功操作
- ❌ 失败/错误
- ⚠️  警告
- 🔄 重试操作
- 📊 统计信息
- ⏱️  时间信息
- 📦 文件大小
- 🔗 URL链接
- 🆔 任务ID
- 📍 步骤进度
- 🦴 骨骼信息
- 🎮 模型加载
- 💾 文件下载

### 📋 日志分类

#### 1. 工作流程日志
```
╔════════════════════════════════════════════════════════════╗
║          TRIPO3D MODEL GENERATION STARTED                  ║
╚════════════════════════════════════════════════════════════╝
📋 Session ID: 12345678-1234-1234-1234-123456789012
🖼️  Image Path: Assets/images/7.jpg
⚙️  Configuration:
   - Max Retries: 3
   - Retry Delay: 3s
   - Polling Interval: 2s
   - Request Timeout: 30s
🕐 Start Time: 2024-03-11 14:30:25
════════════════════════════════════════════════════════════
```

#### 2. 步骤进度日志
```
────────────────────────────────────────────────────────────
📍 Step 1/9: Uploading Image
⏱️  Elapsed Time: 0.5s
────────────────────────────────────────────────────────────
```

#### 3. 上传日志
```
📤 [Upload] Reading image from: Assets/images/7.jpg
📂 [ReadFile] Attempting to read: Assets/images/7.jpg
✅ [ReadFile] File read successfully
   📦 Size: 2.45 MB
📦 [Upload] File size: 2.45 MB
🔄 [Upload] Attempt 1/3 - Sending request...
✅ [Upload] Success!
   🎫 Token: abc123def456
   ⏱️  Duration: 1.23s
   📊 Response Size: 156 B
✅ Image Upload completed in 1.25s
```

#### 4. 任务创建日志
```
🎨 [CreateTask] Preparing model generation request...
📋 [CreateTask] Request Parameters:
   - Type: image_to_model
   - Model Version: v3.0-20250812
   - Quad Mesh: True
   - Texture: True
   - PBR: True
   - Texture Quality: detailed
🚀 [CreateTask] Sending request to API...
✅ [CreateTask] Task created successfully!
   🆔 Task ID: task_abc123
   ⏱️  Duration: 0.45s
✅ Model Task Creation completed in 0.47s
```

#### 5. 轮询状态日志
```
🔍 [PollTask] Starting to monitor task: task_abc123
   ⏱️  Polling interval: 2s
📊 [PollTask] Status Update (Poll #1):
   - Status: processing
   - Progress: 25%
   - Elapsed Time: 2.1s
📊 [PollTask] Status Update (Poll #5):
   - Status: processing
   - Progress: 75%
   - Elapsed Time: 10.3s
✅ [PollTask] Task completed successfully!
   🔗 Model URL: https://...
   📊 Total Polls: 8
   ⏱️  Total Duration: 16.2s
   📅 Created: 2024-03-11T14:30:30Z
✅ Model Generation completed in 16.25s
```

#### 6. 骨骼绑定日志
```
🦴 [RiggingTask] Preparing rigging request...
   📌 Original Task ID: task_abc123
🚀 [RiggingTask] Sending rigging request to API...
✅ [RiggingTask] Rigging task created successfully!
   🆔 Task ID: task_def456
   ⏱️  Duration: 0.38s

🔍 [PollRigging] Starting to monitor rigging task: task_def456
   ⏱️  Polling interval: 2s
📊 [PollRigging] Status Update (Poll #1):
   - Status: processing
   - Elapsed Time: 2.0s
✅ [PollRigging] Rigging completed successfully!
   🔗 Rigged Model URL: https://...
   📊 Total Polls: 12
   ⏱️  Total Duration: 24.5s
   📅 Created: 2024-03-11T14:30:50Z
✅ Rigging Process completed in 24.88s
```

#### 7. 格式转换日志
```
🔄 [ConvertTask] Preparing format conversion request...
   📌 Original Task ID: task_def456
📋 [ConvertTask] Conversion Parameters:
   - Format: GLTF
   - Texture Format: PNG
   - FBX Preset: 3dsmax
🚀 [ConvertTask] Sending conversion request to API...
✅ [ConvertTask] Conversion task created successfully!
   🆔 Task ID: task_ghi789
   ⏱️  Duration: 0.42s

🔍 [PollConversion] Starting to monitor conversion task: task_ghi789
   ⏱️  Polling interval: 2s
✅ [PollConversion] Conversion completed successfully!
   🔗 Converted Model URL: https://...
   📊 Total Polls: 6
   ⏱️  Total Duration: 12.3s
   📅 Created: 2024-03-11T14:31:15Z
✅ Format Conversion completed in 12.75s
```

#### 8. 模型加载日志
```
🎮 [LoadModel] Starting model download...
   🔗 URL: https://...
✅ [LoadModel] Model downloaded
   📦 Size: 15.8 MB
   ⏱️  Download Time: 3.45s
🔧 [LoadModel] Parsing GLTF data...
✅ [LoadModel] Model loaded and displayed successfully!
   ⏱️  Parse Time: 0.89s
   🎯 Position: (0.0, 0.0, 0.0)
   📐 Scale: (1.0, 1.0, 1.0)
   📊 Mesh Stats:
      - Mesh Count: 3
      - Total Vertices: 12,456
      - Total Triangles: 8,234
   🦴 Skeleton Info:
      - Skinned Meshes: 1
      - Total Bones: 24
✅ Model Display completed in 4.35s
```

#### 9. 下载保存日志
```
📁 [Download] Created directory: C:/Users/.../models/session_id/
💾 [Download] Starting download...
   🔗 URL: https://...
   📂 Destination: C:/Users/.../rigged_model.glb
📥 [Download] Progress: 10.0%
📥 [Download] Progress: 25.5%
📥 [Download] Progress: 50.2%
📥 [Download] Progress: 75.8%
📥 [Download] Progress: 100.0%
✅ [Download] Model saved successfully!
   📦 File Size: 15.8 MB
   📂 Location: C:/Users/.../rigged_model.glb
✅ Model Download completed in 3.52s
```

#### 10. 工作流程完成日志
```
════════════════════════════════════════════════════════════
╔════════════════════════════════════════════════════════════╗
║          WORKFLOW COMPLETED SUCCESSFULLY! 🎉               ║
╚════════════════════════════════════════════════════════════╝
✅ Status: SUCCESS
📊 Steps Completed: 9/9
⏱️  Total Duration: 1m 5s
🕐 End Time: 2024-03-11 14:31:30
📁 Session ID: 12345678-1234-1234-1234-123456789012
💾 Model saved to: Application.persistentDataPath/models/12345678-1234-1234-1234-123456789012/
════════════════════════════════════════════════════════════
```

### ❌ 错误日志示例

#### 上传失败
```
❌ [Error] Failed to upload image
   📊 HTTP Status: 401
   ⚠️  Error: Unauthorized
   🔄 Attempt: 1/3
   📄 Response Body: {"error": "Invalid API key"}
⏳ [Upload] Retrying in 3 seconds...
```

#### 任务失败
```
❌ [PollTask] Task failed!
   📊 Total Polls: 5
   ⏱️  Duration: 10.2s
```

#### 工作流程失败
```
════════════════════════════════════════════════════════════
╔════════════════════════════════════════════════════════════╗
║          WORKFLOW FAILED ❌                                ║
╚════════════════════════════════════════════════════════════╝
❌ Status: FAILED
📊 Steps Completed: 3/9
⏱️  Duration Before Failure: 18.5s
🕐 Failure Time: 2024-03-11 14:30:43
════════════════════════════════════════════════════════════
```

## 日志级别

### Debug.Log (信息)
- 正常操作流程
- 状态更新
- 进度信息
- 成功消息

### Debug.LogWarning (警告)
- 重试操作
- 临时性错误
- 非致命问题

### Debug.LogError (错误)
- 操作失败
- 致命错误
- 工作流程中断

## 性能指标追踪

代码自动追踪以下性能指标：
- 每个步骤的执行时间
- 总工作流程时间
- 网络请求时间
- 文件大小
- 轮询次数
- 下载进度

## 调试建议

1. **查找特定操作**: 使用日志标签如 `[Upload]`, `[PollTask]` 等
2. **追踪时间**: 查找 ⏱️ 符号
3. **识别错误**: 查找 ❌ 符号
4. **查看进度**: 查找 📍 和 📊 符号
5. **检查文件大小**: 查找 📦 符号
6. **追踪任务ID**: 查找 🆔 符号
