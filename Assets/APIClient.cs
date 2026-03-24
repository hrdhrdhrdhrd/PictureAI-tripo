using UnityEngine;
using UnityEngine.UI;
using Tripo3D;

/// <summary>
/// Main controller for Tripo3D model generation
/// Simplified version that delegates to workflow manager
/// </summary>
public class APIClient : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button generateButton;
    
    [Header("Configuration")]
    [SerializeField] private TripoConfig config;
    [SerializeField] private string imagePath = "Assets/images/7.jpg";
    
    private TripoWorkflowManager _workflowManager;
    
    private void Start()
    {
        InitializeWorkflowManager();
        InitializeButton();
    }
    
    private void InitializeWorkflowManager()
    {
        if (config == null)
        {
            Debug.LogError("TripoConfig is not assigned! Please create and assign a TripoConfig asset.");
            return;
        }
        
        _workflowManager = new TripoWorkflowManager(config, transform, this);
        Debug.Log("✅ APIClient initialized successfully");
    }
    
    private void InitializeButton()
    {
        if (generateButton == null)
        {
            Debug.LogError("Generate button is not assigned!");
            return;
        }
        
        generateButton.onClick.AddListener(OnGenerateButtonClicked);
    }
    
    private void OnGenerateButtonClicked()
    {
        if (_workflowManager == null)
        {
            Debug.LogError("Workflow manager is not initialized!");
            return;
        }
        
        _workflowManager.StartWorkflow(imagePath);
    }
    
    private void OnDestroy()
    {
        if (generateButton != null)
        {
            generateButton.onClick.RemoveListener(OnGenerateButtonClicked);
        }
    }
}
