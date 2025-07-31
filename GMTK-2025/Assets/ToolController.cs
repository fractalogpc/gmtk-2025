using UnityEditor;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    public static ToolController Instance { get; private set; }


    public enum ToolType
    {
        None,
        Shears,
        Lasso
    }
    public ToolType defaultTool = ToolType.None;
    public ToolType currentTool = ToolType.None;

    public GameObject shearsObject;
    public GameObject lassoObject;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTool(ToolType tool)
    {
        if (currentTool == tool)
        {
            return;
        }

        switch (currentTool)
        {
            case ToolType.Shears:
                shearsObject.SetActive(false);
                break;
            case ToolType.Lasso:
                lassoObject.SetActive(false);
                break;
            default:
                break;
        }

        currentTool = tool;

        switch (tool)
        {
            case ToolType.Shears:
                shearsObject.SetActive(true);
                break;
            case ToolType.Lasso:
                lassoObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void SetTool(string tool)
    {
        if (System.Enum.TryParse(tool, out ToolType parsedTool))
        {
            SetTool(parsedTool);
            return;
        }
    }

    void Start()
    {
        SetTool(defaultTool);
    }
}
