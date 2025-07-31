using UnityEngine;

public class ToolController : MonoBehaviour
{
    public enum ToolType
    {
        None,
        Shears,
        Lasso
    }
    public ToolType currentTool = ToolType.None;

    public GameObject shearsObject;
    public GameObject lassoObject;

    public void SetTool(ToolType tool)
    {
        if (currentTool == tool)
        {
            Debug.Log("Tool already selected");
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
                Debug.Log("No tool to deselect");
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
                Debug.Log("No tool selected");
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
}
