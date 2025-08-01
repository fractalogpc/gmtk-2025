using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    public Image spriteRenderer;
    public GameObject selectIndicator;

    public void SetImage(Sprite sprite)
    {
        if (sprite == null)
        {
            spriteRenderer.enabled = false;
        }
        else
        {
            spriteRenderer.enabled = true;
        }
        spriteRenderer.sprite = sprite;
    }

    public void Select(bool isSelected) => selectIndicator.SetActive(isSelected);

}
