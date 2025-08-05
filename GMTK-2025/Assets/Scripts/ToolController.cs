using NUnit.Framework;
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
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetTool(ToolType tool, AdvancedSheepController sheep = null, InventoryController.WoolData? wool = null)
    {
        if (currentTool == tool && (tool != ToolType.Wool))
        {
            // Debug.Log($"Tool is already set to {tool}. No changes made.");
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
                CartController.Instance.SetInteractableColliderActive(false);

                // Enable interaction on all sheep in carts
                AdvancedSheepController[] sheeps = SheepSpawner.Instance.SheepControllers;
                foreach (var s in sheeps)
                {
                    if (s != null && s.inCart)
                    {
                        s.DisableInteraction();
                    }
                }

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
                int index = wool.HasValue ? wool.Value.ColorIndex : -1;
                woolobject.GetComponent<WoolController>().SetMaterial(SheepDataHolder.Instance.sheeps[index].color);
                break;
            case ToolType.Sheep:
                Debug.Log("Setting tool to Sheep");
                if (sheep != null)
                {
                    currentlyHeldSheep = sheep;
                }
                else
                {
                    Debug.LogWarning("No sheep provided to hold.");
                }

                // Enable interaction on all sheep in carts
                AdvancedSheepController[] sheeps = SheepSpawner.Instance.SheepControllers;
                foreach (var s in sheeps)
                {
                    if (s != null && s.inCart)
                    {
                        s.EnableInteraction();
                    }
                }

                currentlyHeldSheep = sheep;
                currentlyHeldSheep.Show();

                // Allow the cart to be interacted with
                CartController.Instance.SetInteractableColliderActive(true);
                break;
            default:
                break;
        }
    }

    public void SetTool(string tool)
    {
        if (System.Enum.TryParse(tool, out ToolType parsedTool))
        {
            if (parsedTool == ToolType.Wool)
            {
                Debug.Log("Setting tool to Wool with wool data from inventory.");
                SetTool(ToolType.Wool, wool: InventoryController.Instance.GetWoolData());
            }
            else
            {
                SetTool(parsedTool);
            }
            return;
        }
    }

    void Start()
    {
        SetTool(defaultTool);
    }

    public void ResetAllTools()
    {
        SetTool(defaultTool);
    }
}
