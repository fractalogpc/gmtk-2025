using System.Data.Common;
using UnityEngine;

public class InventoryController : InputHandlerBase
{
    public static InventoryController Instance { get; private set; }

    public enum ItemType
    {
        None,
        Lasso,
        Shears,
        Wool,
        Sheep
    }

    public Sprite lassoSprite;
    public Sprite shearsSprite;
    public Sprite[] woolSprites;
    public Sprite sheepSprite;

    private int lassoLevel = 0;
    private int shearsLevel = 0;

    public int LassoLevel => lassoLevel;
    public int ShearsLevel => shearsLevel;

    private int selectedSlot = 0;

    public ItemType[] inventory = new ItemType[3];

    public InventorySlot[] inventorySlots;

    private bool canSelect = true;

    public int SelectedSlot => selectedSlot;
    public int SelectedWoolColorIndex => inventory[selectedSlot] == ItemType.Wool ? heldWool[selectedSlot].ColorIndex : -1;
    public int SelectedWoolSize => inventory[selectedSlot] == ItemType.Wool ? heldWool[selectedSlot].Size : -1;

    public struct WoolData
    {
        public int ColorIndex;
        public int Size;
    }

    private WoolData[] heldWool = new WoolData[3];
    private AdvancedSheepController[] heldSheep = new AdvancedSheepController[3];

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetSelectingOnOff(bool selecting)
    {
        canSelect = selecting;
        if (!canSelect)
        {
            ToolController.Instance.SetTool(ToolController.ToolType.None);
            selectedSlot = -1;
        }
    }

    public bool TryAddItem(ItemType item, int slot, int colorIndex = 0, int size = 1, AdvancedSheepController sheep = null, int level = 0)
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
                lassoLevel = level;
                break;
            case ItemType.Shears:
                inventorySlots[slot].SetImage(shearsSprite);
                shearsLevel = level;
                break;
            case ItemType.Wool:
                inventorySlots[slot].SetImage(woolSprites[colorIndex]);
                heldWool[slot] = new WoolData { ColorIndex = colorIndex, Size = size };
                // Debug.Log($"Added wool of color {colorIndex} and size {size} to slot {slot}");
                break;
            case ItemType.Sheep:
                if (sheep == null)
                {
                    Debug.LogError("Sheep cannot be null when adding to inventory.");
                }
                heldSheep[slot] = sheep;
                inventorySlots[slot].SetImage(sheepSprite);
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

        if (inventory[slot] == ItemType.Sheep && heldSheep != null)
        {
            // heldSheep.Reset(); // Reset the sheep if it's being held
            heldSheep[slot] = null;
        }

        inventory[slot] = ItemType.None;

        inventorySlots[slot].SetImage(null);

        if (selectedSlot == slot)
        {
            ToolController.Instance.SetTool(ToolController.ToolType.None);
        }
        return true;
    }

    protected override void InitializeActionMap()
    {
        RegisterAction(_inputActions.Player.Slot1, _ => SelectItem(0));
        RegisterAction(_inputActions.Player.Slot2, _ => SelectItem(1));
        RegisterAction(_inputActions.Player.Slot3, _ => SelectItem(2));

        RegisterAction(_inputActions.Player.Scroll, ctx => HandleScroll(ctx));
    }

    public void SelectItem(int slot)
    {
        if (!canSelect)
        {
            return; // Prevent selection if not allowed
        }

        if (slot == selectedSlot)
        {
            ToolController.Instance.SetTool(ToolController.ToolType.None);
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                inventorySlots[i].Select(false);
            }
            selectedSlot = -1;
            return;
        }

        switch (inventory[slot])
            {
                case ItemType.Lasso:
                    ToolController.Instance.SetTool(ToolController.ToolType.Lasso);
                    break;
                case ItemType.Shears:
                    ToolController.Instance.SetTool(ToolController.ToolType.Shears);
                    break;
                case ItemType.Wool:
                    ToolController.Instance.SetTool(ToolController.ToolType.Wool, wool: heldWool[slot]);
                    break;
                case ItemType.Sheep:
                    ToolController.Instance.SetTool(ToolController.ToolType.Sheep, sheep: heldSheep[slot]);
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

    private void HandleScroll(UnityEngine.InputSystem.InputAction.CallbackContext ctx)
    {
        float value = ctx.ReadValue<float>();

        if (value > 0)
        {
            switch (selectedSlot)
            {
                case 0:
                    SelectItem(1);
                    break;
                case 1:
                    SelectItem(2);
                    break;
                case 2:
                    SelectItem(0);
                    break;
            }
        }
        else
        {
            switch (selectedSlot)
            {
                case 0:
                    SelectItem(2);
                    break;
                case 1:
                    SelectItem(0);
                    break;
                case 2:
                    SelectItem(1);
                    break;
            }
        }
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

    public WoolData GetWoolData(int? slot = null)
    {
        int Slot;
        if (slot == null)
        {
            Slot = selectedSlot;
        }
        else Slot = slot.Value;
        if (slot < 0 || slot >= heldWool.Length)
        {
            Debug.LogError("Invalid wool slot index.");
            return new WoolData { ColorIndex = -1, Size = -1 };
        }
        return heldWool[Slot];
    }

    public AdvancedSheepController GetHeldSheep(int? slot = null)
    {
        int Slot;
        if (slot == null)
        {
            Slot = selectedSlot;
        }
        else Slot = slot.Value;
        if (Slot < 0 || Slot >= heldSheep.Length)
        {
            Debug.LogError("Invalid sheep slot index.");
            return null;
        }
        return heldSheep[Slot];
    }
}
