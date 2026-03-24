using System;

namespace Tripo3D
{
    #region API Response Models

    /// <summary>
    /// Response from image upload endpoint
    /// </summary>
    [Serializable]
    public class UploadResponse
    {
        public string code;
        public UploadFileData data;
    }

    [Serializable]
    public class UploadFileData
    {
        public string image_token;
    }

    /// <summary>
    /// Generic API response containing task ID
    /// </summary>
    [Serializable]
    public class ApiResponse
    {
        public string code;
        public TaskID data;
    }

    [Serializable]
    public class TaskID
    {
        public string task_id;
    }

    /// <summary>
    /// Response for model generation task status
    /// </summary>
    [Serializable]
    public class TaskData
    {
        public string code;
        public TaskDataDetail data;
    }

    [Serializable]
    public class TaskDataDetail
    {
        public string task_id;
        public string type;
        public string status;
        public string input;
        public OutPut output;
        public string progress;
        public string create_time;
    }

    [Serializable]
    public class OutPut
    {
        public string pbr_model;
    }

    /// <summary>
    /// Response for model conversion task status
    /// </summary>
    [Serializable]
    public class FinalTaskData
    {
        public string code;
        public FinalTaskDataDetail data;
    }

    [Serializable]
    public class FinalTaskDataDetail
    {
        public string task_id;
        public string type;
        public string status;
        public string input;
        public FinalOutPut output;
        public string progress;
        public string create_time;
    }

    [Serializable]
    public class FinalOutPut
    {
        public string model;
    }

    /// <summary>
    /// Response for rigging task status
    /// </summary>
    [Serializable]
    public class RiggingTaskData
    {
        public string code;
        public RiggingTaskDataDetail data;
    }

    [Serializable]
    public class RiggingTaskDataDetail
    {
        public string task_id;
        public string type;
        public string status;
        public string input;
        public RiggingOutput output;
        public string progress;
        public string create_time;
    }

    [Serializable]
    public class RiggingOutput
    {
        public string model;
    }

    #endregion

    #region API Request Models

    /// <summary>
    /// Request data for creating image-to-model task
    /// </summary>
    [Serializable]
    public class RequestData
    {
        public string type;
        public FileData file;
        public bool quad;
        public string model_version;
        public bool texture;
        public bool pbr;
        public string orientation;
        public string texture_quality;
    }

    [Serializable]
    public class FileData
    {
        public string type;
        public string file_token;
    }

    /// <summary>
    /// Request data for converting model format
    /// </summary>
    [Serializable]
    public class RequestConvertTaskData
    {
        public string type;
        public string format;
        public string original_model_task_id;
        public string texture_format;
        public string fbx_preset;
    }

    /// <summary>
    /// Request data for rigging model (adding skeleton)
    /// </summary>
    [Serializable]
    public class RequestRiggingTaskData
    {
        public string type;
        public string original_model_task_id;
    }

    /// <summary>
    /// Request data for texture operations
    /// </summary>
    [Serializable]
    public class TextureData
    {
        public string type;
        public string original_model_task_id;
    }

    #endregion
}
