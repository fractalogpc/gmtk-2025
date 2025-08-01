using Unity.Mathematics;
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

            LassoController.ReleaseSheep(sheepToTransfer, pen);
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

[System.Serializable]
public class Pen
{
    public int MaximumSheep;
    public int CurrentSheep;
    public Vector3 center;
    public BoxCollider mesh;

    public float2x4 GetCorners()
    {
        Bounds bounds = mesh.bounds;
        float2x4 corners = new float2x4(
        new float2(bounds.min.x + 0.5f, bounds.min.z + 0.5f), // Bottom Left
        new float2(bounds.max.x - 0.5f, bounds.min.z + 0.5f), // Bottom Right
        new float2(bounds.max.x - 0.5f, bounds.max.z - 0.5f), // Top Right
        new float2(bounds.min.x + 0.5f, bounds.max.z - 0.5f)  // Top Left
        );

        return corners;
    }

    public int AvailableSpace => MaximumSheep - CurrentSheep;
    public bool IsFull => CurrentSheep >= MaximumSheep;

}
