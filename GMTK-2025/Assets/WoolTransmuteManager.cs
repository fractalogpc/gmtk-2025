using UnityEngine;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class WoolTransmuteManager : MonoBehaviour
{

    [System.Serializable]
    public class WoolCount
    {
        public int amount;
        public int colorIndex;
        public string name;
    }

    [System.Serializable]
    public class Recipe
    {
        public WoolCount[] inputs;
        public WoolCount[] outputs;
        public float transmuteTime;
    }

    [SerializeField] private Recipe[] recipes;
    [SerializeField] private Sprite[] woolIcons;
    [SerializeField] private GameObject transmutingUI;
    [SerializeField] private GameObject recipeSelectionUI;
    [SerializeField] private GameObject[] inputUIElements;
    [SerializeField] private GameObject[] outputUIElements;
    [SerializeField] private TextMeshProUGUI recipeTimeText;
    [SerializeField] private TextMeshProUGUI transmutingLabelText;
    [SerializeField] private TextMeshProUGUI transmutingProgressText;
    [SerializeField] private Light transmutingLight;
    [SerializeField] private AnimationCurve transmutingLightCurve;
    [SerializeField] private float transmutingLightMaxIntensity = 2f;
    [SerializeField] private Transform shakingPipeTransform;
    [SerializeField] private float shakingPipeMaxDisplacement = 0.1f;
    [SerializeField] private float shakingSpeed = 5f;

    private int selectedRecipeIndex = 0;
    private bool isTransmuting = false;

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        for (int i = 0; i < inputUIElements.Length; i++)
        {
            if (i < recipes[selectedRecipeIndex].inputs.Length)
            {
                inputUIElements[i].SetActive(true);
                inputUIElements[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{recipes[selectedRecipeIndex].inputs[i].name} x{recipes[selectedRecipeIndex].inputs[i].amount}";
                inputUIElements[i].GetComponentInChildren<Image>().sprite = woolIcons[recipes[selectedRecipeIndex].inputs[i].colorIndex];
            }
            else
            {
                inputUIElements[i].SetActive(false);
            }
        }

        for (int i = 0; i < outputUIElements.Length; i++)
        {
            if (i < recipes[selectedRecipeIndex].outputs.Length)
            {
                outputUIElements[i].SetActive(true);
                outputUIElements[i].GetComponentInChildren<TextMeshProUGUI>().text = $"{recipes[selectedRecipeIndex].outputs[i].name} x{recipes[selectedRecipeIndex].outputs[i].amount}";
                outputUIElements[i].GetComponentInChildren<Image>().sprite = woolIcons[recipes[selectedRecipeIndex].outputs[i].colorIndex];
            }
            else
            {
                outputUIElements[i].SetActive(false);
            }
        }

        recipeTimeText.text = $"Transmuting will take {recipes[selectedRecipeIndex].transmuteTime} seconds";
    }

    public void StartTransmute()
    {
        if (isTransmuting)
        {
            return; // Already transmuting
        }

        // Check if the player can afford the recipe
        int combinedWBGCount = 0;
        if (UpgradeManager.Instance.OwnsUpgrade("Nuclear Reactor"))
        {
            combinedWBGCount += UpgradeManager.Instance.GetWoolCount(0);
            combinedWBGCount += UpgradeManager.Instance.GetWoolCount(1);
            combinedWBGCount += UpgradeManager.Instance.GetWoolCount(2);
        }

        bool canAfford = true;
        foreach (var input in recipes[selectedRecipeIndex].inputs)
        {
            if (combinedWBGCount > 0 && new[] { 0, 1, 2 }.Contains(input.colorIndex))
            {
                // If the player has the reactor, check their white + gray + brown wool count as one pool
                if (input.amount > combinedWBGCount)
                {
                    canAfford = false;
                    break;
                }
                combinedWBGCount -= input.amount;
                continue;
            }

            int availableAmount = UpgradeManager.Instance.GetWoolCount(input.colorIndex);
            if (availableAmount < input.amount)
            {
                canAfford = false;
                break;
            }
        }

        if (!canAfford)
        {
            Debug.LogError("Not enough wool to transmute this recipe.");
            return; // Not enough wool to transmute
        }

        // Start the transmute coroutine
        StartCoroutine(TransmuteCoroutine());
    }

    private IEnumerator TransmuteCoroutine()
    {
        isTransmuting = true;
        transmutingUI.SetActive(true);
        recipeSelectionUI.SetActive(false);
        
        // Remove inputs from the player's wool counts
        foreach (var input in recipes[selectedRecipeIndex].inputs)
        {
            if (UpgradeManager.Instance.OwnsUpgrade("Nuclear Reactor") && new[] { 0, 1, 2 }.Contains(input.colorIndex))
            {
                // If the player owns the reactor, remove from the combined pool
                int remainingCost = input.amount;
                for (int i = 0; i < 3; i++)
                {
                    int availableAmount = UpgradeManager.Instance.GetWoolCount(i);
                    if (availableAmount >= remainingCost)
                    {
                        UpgradeManager.Instance.RemoveWool(i, remainingCost);
                        break;
                    }
                    else
                    {
                        UpgradeManager.Instance.RemoveWool(i, availableAmount);
                        remainingCost -= availableAmount;
                    }
                }
            }
            else
            {
                UpgradeManager.Instance.RemoveWool(input.colorIndex, input.amount);
            }
        }

        // Wait for the transmute time
        yield return StartCoroutine(AnimateTransmutation(recipes[selectedRecipeIndex].transmuteTime));

        // Add outputs to the player's wool counts
        foreach (var output in recipes[selectedRecipeIndex].outputs)
        {
            UpgradeManager.Instance.DepositWool(output.colorIndex, output.amount);
        }

        transmutingUI.SetActive(false);
        recipeSelectionUI.SetActive(true);
        isTransmuting = false;
    }

    private IEnumerator AnimateTransmutation(float duration)
    {
        float elapsed = 0f;
        int PROGRESS_STEPS = 31;
        transmutingLight.enabled = true;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            string progressText = "";
            for (int i = 0; i < PROGRESS_STEPS; i++)
            {
                if (i < progress * PROGRESS_STEPS)
                {
                    progressText += "#";
                }
                else
                {
                    progressText += "-";
                }
            }
            transmutingProgressText.text = progressText;
            transmutingLabelText.text = $"Transmuting" + new string('.', (int)(elapsed * 0.5f) % 4);
            float intensity = transmutingLightCurve.Evaluate(progress);
            transmutingLight.intensity = intensity * transmutingLightMaxIntensity;
            shakingPipeTransform.localPosition = new Vector3(
                Mathf.Sin(elapsed * shakingSpeed * intensity) * shakingPipeMaxDisplacement,
                Mathf.Cos(elapsed * shakingSpeed * intensity) * shakingPipeMaxDisplacement,
                0f
            ) * transmutingLightCurve.Evaluate(progress);
            yield return null;
        }

        transmutingLight.enabled = false;
        shakingPipeTransform.localPosition = Vector3.zero; // Reset pipe
    }

    public void SwitchSelection(int direction)
    {
        if (isTransmuting)
        {
            return; // Can't switch while transmuting
        }
        // Swap selected recipe
        selectedRecipeIndex += direction;
        if (selectedRecipeIndex < 0)
        {
            selectedRecipeIndex = recipes.Length - 1; // Wrap around to last recipe
        }
        else if (selectedRecipeIndex >= recipes.Length)
        {
            selectedRecipeIndex = 0; // Wrap around to first recipe
        }
        UpdateUI();
    }

}
