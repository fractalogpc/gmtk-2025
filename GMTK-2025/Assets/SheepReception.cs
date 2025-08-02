using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using TMPro;
using NUnit.Framework;

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

        Debug.Log("Interacted with Sheep Reception");

        foreach (Pen pen in pens)
        {
            if (pen.IsFull) continue;
            Debug.Log($"Trying to fill pen: {pen.Name}");

            int resultingSheepCount = pen.FillSheep(currentSheepCount, out Pen.SubPen[] subPens);

            foreach (var subPen in subPens)
            {
                Debug.Log($"Filling subPen: {subPen.mesh.name} with {subPen.CurrentSheep}/{subPen.MaximumSheep}");
                LassoController.ReleaseSheep(1, subPen);
            }

            currentSheepCount = resultingSheepCount;

            pen.penText.text = $"{pen.Name} - {pen.CurrentSheep()}/{pen.MaximumSheep()}";
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

    public bool TrySendSheepToAvailablePen(AdvancedSheepController sheep, out Pen.SubPen? pen)
    {
        Debug.Log("Trying to send sheep to available pen...");
        pen = null;
        if (sheep == null) return false;

        foreach (Pen p in pens)
        {
            if (p.IsFull) continue;

            p.FillSheep(1, out Pen.SubPen[] filledPens);
            pen = filledPens[0];
            return true;
        }
        return false;
    }

    public GameObject errorText;

    private Pen pen1;
    public GameObject pen1Text;
    public int[] pen1APathing;
    public BoxCollider pen1AMesh;
    public void UnlockPen1()
    {
        errorText.SetActive(false);

        pen1 = new Pen
        {
            Name = "Pen 1",
            penText = pen1Text.GetComponentInChildren<TextMeshProUGUI>()
        };

        Pen.SubPen subPen1A = new Pen.SubPen
        {
            MaximumSheep = 1,
            mesh = pen1AMesh,
            pathingIndices = pen1APathing
        };
        pen1.AddSubPen(subPen1A);

        pens.Add(pen1);

        pen1Text.SetActive(true);
        pen1Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen1.Name} - {pen1.CurrentSheep()}/{pen1.MaximumSheep()}");
    }

    public BoxCollider pen1BMesh;
    public void UpgradePen1()
    {
        Pen.SubPen subPen1A = pen1.subPens[0];
        subPen1A.MaximumSheep = 1;
        subPen1A.mesh = pen1BMesh;

        pen1Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen1.Name} - {pen1.CurrentSheep()}/{pen1.MaximumSheep()}");
    }

    public int[] pen1BPathing;
    public BoxCollider pen1CMesh;
    public void UpgradePen1C()
    {
        Pen.SubPen subPen1B = new Pen.SubPen
        {
            MaximumSheep = 20,
            mesh = pen1CMesh,
            pathingIndices = pen1BPathing
        };
        pen1.AddSubPen(subPen1B);

        pen1Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen1.Name} - {pen1.CurrentSheep()}/{pen1.MaximumSheep()}");

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
            penText = pen2Text.GetComponentInChildren<TextMeshProUGUI>()
        };

        Pen.SubPen subPen2A = new Pen.SubPen
        {
            MaximumSheep = 1,
            mesh = pen2AMesh,
            pathingIndices = pen2Pathing
        };
        pen2.AddSubPen(subPen2A);

        pens.Add(pen2);

        pen2Text.SetActive(true);
        pen2Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen2.Name} - {pen2.CurrentSheep()}/{pen2.MaximumSheep()}");
    }

    public BoxCollider pen2BMesh;
    public void UpgradePen2()
    {
        Pen.SubPen subPen2A = pen2.subPens[0];
        subPen2A.MaximumSheep = 1;
        subPen2A.mesh = pen2BMesh;

        pen2Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen2.Name} - {pen2.CurrentSheep()}/{pen2.MaximumSheep()}");
    }

    public int[] pen2BPathing;
    public BoxCollider pen2CMesh;
    public void UpgradePen2C()
    {
        Pen.SubPen subPen2B = new Pen.SubPen
        {
            MaximumSheep = 20,
            mesh = pen2CMesh,
            pathingIndices = pen2BPathing
        };
        pen2.AddSubPen(subPen2B);

        pen2Text.GetComponentInChildren<TextMeshProUGUI>().text = ($"{pen2.Name} - {pen2.CurrentSheep()}/{pen2.MaximumSheep()}");

    }
}

[System.Serializable]
public class Pen
{
    [System.Serializable]
    public class SubPen
    {
        public int MaximumSheep;
        public int CurrentSheep;
        public int[] pathingIndices;
        public BoxCollider mesh;

        public int AvailableSpace => MaximumSheep - CurrentSheep;
        public bool IsFull => CurrentSheep >= MaximumSheep;

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

        public void SetMaximumSheep(int count)
        {
            MaximumSheep = count;
        }

        public void SetCurrentSheep(int count)
        {
            CurrentSheep = count;
        }

    }

    public string Name;
    public TextMeshProUGUI penText;

    public List<SubPen> subPens = new List<SubPen>();
    public int MaximumSheep()
    {
        int total = 0;
        foreach (var subPen in subPens)
        {
            total += subPen.MaximumSheep;
        }
        return total;
    }

    public int CurrentSheep()
    {
        int total = 0;
        foreach (var subPen in subPens)
        {
            total += subPen.CurrentSheep;
        }
        return total;
    }

    public void AddSubPen(SubPen subPen)
    {
        subPens.Add(subPen);
    }

    public int FillSheep(int count, out SubPen[] filledPens)
    {
        List<SubPen> filled = new List<SubPen>();
        for (int i = 0; i < subPens.Count; i++)
        {
            if (subPens[i].IsFull) continue;

            if (count < subPens[i].AvailableSpace)
            {
                // Fill the subPen with the remaining count

                subPens[i].SetCurrentSheep(subPens[i].CurrentSheep + count);

                filled.AddRange(System.Linq.Enumerable.Repeat(subPens[i], count));
                filledPens = filled.ToArray();
                return 0;
            }
            else
            {
                // Fill the subPen to its maximum capacity then continue to the next subPen
                int sheepToFill = subPens[i].AvailableSpace;
                count -= sheepToFill;
                subPens[i].SetCurrentSheep(subPens[i].MaximumSheep);

                filled.AddRange(System.Linq.Enumerable.Repeat(subPens[i], sheepToFill));
            }
        }
        filledPens = filled.ToArray();
        return count;
    }


    public int AvailableSpace => MaximumSheep() - CurrentSheep();
    public bool IsFull => CurrentSheep() >= MaximumSheep();

}
