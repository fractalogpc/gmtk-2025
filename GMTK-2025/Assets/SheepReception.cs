using UnityEngine;

public class SheepReception : MonoBehaviour, IInteractable
{
    public static SheepReception Instance { get; private set; }

    public int currentSheepCount = 0;

    public Pen[] pens;

    public LassoController LassoController;

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

    public void HoldInteract(float holdTime)
    {
    }

    public void Interact()
    {
        if (currentSheepCount == 0) return;

        int availableSpace = 0;
        foreach (var pen in pens)
        {
            availableSpace += pen.AvailableSpace;
        }

        if (availableSpace == 0)
        {
            Debug.Log("All pens are full.");
            return;
        }

        foreach (var pen in pens)
        {
            if (pen.IsFull) continue;

            int sheepToTransfer = Mathf.Min(currentSheepCount, pen.AvailableSpace);
            pen.CurrentSheep += sheepToTransfer;
            currentSheepCount -= sheepToTransfer;

            LassoController.ReleaseSheep(sheepToTransfer);
        }
    }

    public void OnHoverEnter()
    {
    }

    public void OnHoverExit()
    {
    }

    public void ReleaseInteract()
    {
    }
}

public class Pen
{
    public int MaximumSheep;
    public int CurrentSheep;

    public int AvailableSpace => MaximumSheep - CurrentSheep;
    public bool IsFull => CurrentSheep >= MaximumSheep;

}
