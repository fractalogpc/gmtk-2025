using UnityEditor;
using UnityEngine;

public class ToolController : MonoBehaviour
{
    public static ToolController Instance { get; private set; }


    public enum ToolType
    {
        None,
        Shears,
        Lasso,
        Sheep,
        Wool
    }
    public ToolType defaultTool = ToolType.None;
    public ToolType currentTool = ToolType.None;

    public GameObject shearsObject;
    public GameObject lassoObject;
    public GameObject woolobject;

    public AdvancedSheepController currentlyHeldSheep;

    public Transform sheepHoldPosition;

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

    public void SetTool(ToolType tool, AdvancedSheepController sheep = null)
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
                lassoObject.GetComponent<LassoController>().DeselectLasso();
                lassoObject.SetActive(false);
                break;
            case ToolType.Wool:
                woolobject.SetActive(false);
                break;
            case ToolType.Sheep:
                currentlyHeldSheep.Hide();
                currentlyHeldSheep = null;
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
                lassoObject.GetComponent<LassoController>().ResetLasso();
                break;
            case ToolType.Wool:
                woolobject.SetActive(true);
                break;
            case ToolType.Sheep:
                if (sheep != null)
                {
                    currentlyHeldSheep = sheep;
                }
                else
                {
                    Debug.LogWarning("No sheep provided to hold.");
                }

                currentlyHeldSheep.Show();
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
