using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using FMODUnity;

public class GuidebookManager : MonoBehaviour
{

    [SerializeField] private Sprite[] guidebookPages;
    [SerializeField] private Image leftPageImage;
    [SerializeField] private Image rightPageImage;
    [SerializeField] private float timeToBringUp = 0.5f;
    [SerializeField] private AnimationCurve bringUpCurve;
    [SerializeField] private float distanceToBringUp = 0.5f;
    [SerializeField] private float zoomInAmount = 0.1f;
    [SerializeField] private StudioEventEmitter guidebookOpenSound;
    [SerializeField] private GameObject openPrompt;
    [SerializeField] private GameObject secondaryOpenPrompt;

    private bool hasOpenedGuidebook = false;

    private int currentPageIndex = 0;
    private bool guidebookUp = false;
    private Vector3 initialPosition;
    private bool isAnimating = false;
    private int selectedHotbarSlot = 0;

    void Start()
    {
        UpdatePageImages();
        initialPosition = transform.localPosition;
        // StartCoroutine(AnimateGuidebook(true)); // Start with the guidebook up
    }

    private void UpdatePageImages()
    {
        if (currentPageIndex < 0 || currentPageIndex >= guidebookPages.Length)
        {
            return; // Invalid index
        }

        leftPageImage.sprite = guidebookPages[currentPageIndex];
        rightPageImage.sprite = (currentPageIndex + 1 < guidebookPages.Length) ? guidebookPages[currentPageIndex + 1] : null;
    }

    private IEnumerator AnimateGuidebook(bool up)
    {
        isAnimating = true;
        float elapsedTime = 0f;
        Vector3 targetPosition = initialPosition + (up ? Vector3.up * distanceToBringUp : Vector3.zero);
        Vector3 startPosition = transform.localPosition;
        float currentUpProgress = (transform.localPosition.y - initialPosition.y) / distanceToBringUp;
        float timeToBringUpAccountingForStartPosition = up ? timeToBringUp * (1 - currentUpProgress) : timeToBringUp * currentUpProgress;

        while (elapsedTime < timeToBringUpAccountingForStartPosition)
        {
            float t = elapsedTime / timeToBringUpAccountingForStartPosition;
            transform.localPosition = Vector3.Lerp(startPosition, targetPosition, bringUpCurve.Evaluate(t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;
        isAnimating = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            guidebookUp = !guidebookUp;
            // Deselect any currently selected item and if guidebook is going down, reselect whatever you had selected
            if (guidebookUp)
            {
                selectedHotbarSlot = InventoryController.Instance.SelectedSlot;
                InventoryController.Instance.SelectItem(selectedHotbarSlot);
                InventoryController.Instance.SetSelectingOnOff(false);

                if (openPrompt != null && !hasOpenedGuidebook)
                {
                    hasOpenedGuidebook = true;
                    openPrompt.SetActive(false);
                    secondaryOpenPrompt.SetActive(true);
                }
            }
            else
            {
                InventoryController.Instance.SetSelectingOnOff(true);
                InventoryController.Instance.SelectItem(selectedHotbarSlot);
            }

            // Animate guidebook going away/coming up
            StopAllCoroutines();
            StartCoroutine(AnimateGuidebook(guidebookUp));
        }

        if (!guidebookUp) return;

        if (!isAnimating)
        {
            if (Input.GetMouseButton(1))
            {
                // Zoom in on the guidebook
                Vector3 zoomedPosition = new Vector3(
                    initialPosition.x,
                    transform.localPosition.y,
                    initialPosition.z - zoomInAmount
                );
                transform.localPosition = Vector3.Lerp(transform.localPosition, zoomedPosition, Time.deltaTime * 5f);
            }
            else
            {
                Vector3 unZoomedPosition = new Vector3(
                    initialPosition.x,
                    transform.localPosition.y,
                    initialPosition.z
                );
                // Reset position when not zooming
                transform.localPosition = Vector3.Lerp(transform.localPosition, unZoomedPosition, Time.deltaTime * 5f);
            }
        }

        if (Input.GetKeyDown(KeyCode.E))
            {
                int originalIndex = currentPageIndex;
                currentPageIndex += 2;
                if (currentPageIndex >= guidebookPages.Length)
                {
                    currentPageIndex = guidebookPages.Length % 2 == 0 ? guidebookPages.Length - 2 : guidebookPages.Length - 1;
                }
                UpdatePageImages();
                if (guidebookOpenSound != null && originalIndex != currentPageIndex)
                {
                    guidebookOpenSound.Play();
                }
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                int originalIndex = currentPageIndex;
                currentPageIndex -= 2;
                if (currentPageIndex < 0)
                {
                    currentPageIndex = 0;
                }
                UpdatePageImages();
                if (guidebookOpenSound != null && originalIndex != currentPageIndex)
                {
                    guidebookOpenSound.Play();
                }
            }
    }
    
}
