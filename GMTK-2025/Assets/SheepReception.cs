using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TMPro;

public class SheepReception : MonoBehaviour, IInteractable
{
    public static SheepReception Instance { get; private set; }

    public int currentSheepCount = 0;

    public List<Pen> pens;

    public LassoController LassoController;

    public Transform[] pathingPoints;

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

            pen.penText.text = $"{pen.Name} - {pen.CurrentSheep}/{pen.MaximumSheep}";

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


    public GameObject errorText;

    private Pen pen1;
    public GameObject pen1Text;
    public int[] pen1Pathing;
    public BoxCollider pen1AMesh;
    public void UnlockPen1()
    {
        errorText.SetActive(false);

        pen1 = new Pen
        {
            Name = "Pen 1",
            MaximumSheep = 10,
            mesh = pen1AMesh,
            pathingIndices = pen1Pathing,
            penText = pen1Text.GetComponentInChildren<TextMeshProUGUI>()
        };

        pens.Add(pen1);

        pen1Text.SetActive(true);
        pen1Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen1.Name} - {pen1.CurrentSheep}/{pen1.MaximumSheep}");
    }

    public BoxCollider pen1BMesh;
    public void UpgradePen1()
    {
        pen1.MaximumSheep = 20;
        pen1.mesh = pen1BMesh;

        pen1Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen1.Name} - {pen1.CurrentSheep}/{pen1.MaximumSheep}");
    }

    private Pen pen2;
    public GameObject pen2Text;
    public int[] pen2Pathing;
    public BoxCollider pen2AMesh;
    public void UnlockPen2()
    {
        errorText.SetActive(false);

        pen2 = new Pen
        {
            Name = "Pen 2",
            MaximumSheep = 10,
            mesh = pen2AMesh,
            pathingIndices = pen2Pathing,
            penText = pen2Text.GetComponentInChildren<TextMeshProUGUI>()
        };

        pens.Add(pen2);

        pen2Text.SetActive(true);
        pen2Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen2.Name} - {pen2.CurrentSheep}/{pen2.MaximumSheep}");
    }

    public BoxCollider pen2BMesh;
    public void UpgradePen2()
    {
        pen2.MaximumSheep = 20;
        pen2.mesh = pen2BMesh;

        pen2Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen2.Name} - {pen2.CurrentSheep}/{pen2.MaximumSheep}");
    }
}

[System.Serializable]
public class Pen
{
    public string Name;
    public int MaximumSheep;
    public int CurrentSheep;
    public BoxCollider mesh;
    public TextMeshProUGUI penText;

    public int[] pathingIndices;

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
