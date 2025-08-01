using UnityEngine;
using TMPro;

public class SignBehaviour : MonoBehaviour
{

    [SerializeField] private string signText = "Pen #1: 10 White Wool";
    [SerializeField] private TextMeshProUGUI signTextObject;

    [SerializeField] private string upgradeName = "Pen #1";

    private void Start()
    {
        if (signText != null)
        {
            UpdateSignText();
        }
    }

    private void UpdateSignText()
    {
        if (signTextObject != null)
        {
            signTextObject.text = signText;
        }
        else
        {
            Debug.LogWarning("Sign text component is not assigned.");
        }
    }

    public void Purchase()
    {
        // Debug.Log("Purchased: " + signTextObject.text);
        
        if (UpgradeManager.Instance.TryBuyUpgrade(upgradeName))
        {
            // Debug.Log("Wool removed successfully.");
            Destroy(gameObject);
        }
        else
        {
            // Debug.LogWarning("Not enough wool to complete the purchase.");
        }
    }
}
