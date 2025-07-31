using UnityEngine;

public class InventoryController : InputHandlerBase
{
    public static InventoryController Instance { get; private set; }

    public enum ItemType
    {
        None,
        Lasso,
        Shears,
        Wool
    }

    public Sprite lassoSprite;
    public Sprite shearsSprite;
    public Sprite woolSprite;

    private int selectedSlot = 0;

    public ItemType[] inventory = new ItemType[3];

    public InventorySlot[] inventorySlots;

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

    public bool TryAddItem(ItemType item, int slot)
    {
        if (slot < 0 || slot >= inventory.Length)
        {
            Debug.LogError("Invalid inventory slot index.");
            return false;
        }
        if (inventory[slot] != ItemType.None)
        {
            Debug.LogWarning("Inventory slot is already occupied.");
            return false;
        }
        inventory[slot] = item;

        switch (item)
        {
            case ItemType.Lasso:
                inventorySlots[slot].SetImage(lassoSprite);
                break;
            case ItemType.Shears:
                inventorySlots[slot].SetImage(shearsSprite);
                break;
            case ItemType.Wool:
                inventorySlots[slot].SetImage(woolSprite);
                break;
        }

        SelectItem(slot);

        return true;
    }

    public bool TryRemoveItem(int slot)
    {
        if (slot < 0 || slot >= inventory.Length)
        {
            Debug.LogError("Invalid inventory slot index.");
            return false;
        }
        if (inventory[slot] == ItemType.None)
        {
            Debug.LogWarning("Inventory slot is already empty.");
            return false;
        }
        inventory[slot] = ItemType.None;

        inventorySlots[slot].SetImage(null);
        return true;
    }

    protected override void InitializeActionMap()
    {
        RegisterAction(_inputActions.Player.Slot1, _ => SelectItem(0));
        RegisterAction(_inputActions.Player.Slot2, _ => SelectItem(1));
        RegisterAction(_inputActions.Player.Slot3, _ => SelectItem(2));
    }

    private void SelectItem(int slot)
    {
        switch (inventory[slot])
        {
            case ItemType.Lasso:
                ToolController.Instance.SetTool(ToolController.ToolType.Lasso);
                break;
            case ItemType.Shears:
                ToolController.Instance.SetTool(ToolController.ToolType.Shears);
                break;
            case ItemType.Wool:
                ToolController.Instance.SetTool(ToolController.ToolType.Wool);
                break;
            case ItemType.None:
                ToolController.Instance.SetTool(ToolController.ToolType.None);
                break;
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].Select(i == slot);
        }

        selectedSlot = slot;
    }

    public bool GetNextAvailableSlot(out int slot)
    {
        for (int i = 0; i < inventory.Length; i++)
        {
            if (inventory[i] == ItemType.None)
            {
                slot = i;
                return true;
            }
        }
        slot = -1;
        return false;
    }

    public bool IsHoldingObject(ItemType item)
    {
        return inventory[selectedSlot] == item;
    }
}
