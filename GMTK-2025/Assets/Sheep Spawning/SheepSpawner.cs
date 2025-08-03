using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.VisualScripting;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{

    public static SheepSpawner Instance;

    private SheepObject[] sheeps;
    public int[] rareSheepIndices;
    public Transform[] rareSheepSpawnpoint;
    public Vector2 _scale, _amount;
    public float jitter;
    public LayerMask layer;
    public PlayerController playerController;
    public Transform sheepParent;
    public List<Transform> rareSheepsPositions = new List<Transform>();

    private List<AdvancedSheepController> sheepControllers = new List<AdvancedSheepController>();

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

        sheeps = SheepDataHolder.Instance.sheeps;
    }

    public Vector2[] GetRareSheepPositions()
    {
        Vector2[] positions = new Vector2[rareSheepsPositions.Count()];
        for (int i = 0; i < rareSheepsPositions.Count(); i++)
        {
            positions[i] = new Vector2(rareSheepsPositions[i].transform.position.x, rareSheepsPositions[i].transform.position.z);
        }
        return positions;
    }

    public void ClearWildSheep()
    {
        foreach (var sheep in sheepControllers)
        {
            if (sheep != null && !sheep.InPenValue)
            {
                Destroy(sheep.gameObject);
            }
        }
        sheepControllers.Clear();
        rareSheepsPositions.Clear();
    }

    public void RegenerateAllPenSheep()
    {
        foreach (var sheep in sheepControllers)
        {
            if (sheep != null && sheep.InPenValue)
            {
                sheep.ResetWool();
            }
        }
    }

    public void GenerateSheep()
    {
        SpawnSheepWave(_scale, _amount, jitter);
        SpawnRareSheep(3, .6f);
    }

    public void SpawnSheepWave(Vector2 scale, Vector2 amount, float jitter)
    {
        List<Vector2> points = new List<Vector2>();
        List<SheepObject> tempSpawning = new List<SheepObject>();
        List<SheepObject> spawning = new List<SheepObject>();
        List<Vector2> spawnpoint = new List<Vector2>();

        for (int i = 0; i < amount.x; i++)
        {
            for (int j = 0; j < amount.y; j++)
            {
                points.Add(new Vector2(i * (scale.x / amount.x), j * (scale.y / amount.y)) + Random.insideUnitCircle * jitter);
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            // Debug.Log(Mathf.RoundToInt(points[i].x));
            for (int j = 0; j < 3; j++) // Here we assume there are 3 default sheep types to check against
            {
                if (sheeps[j].heatmap.GetPixel(Mathf.RoundToInt((points[i].x / scale.x) * sheeps[j].heatmap.width), Mathf.RoundToInt((points[i].y / scale.y) * sheeps[j].heatmap.height)).r > .2f)
                {
                    // Debug.Log("Spawned");

                    tempSpawning.Add(sheeps[j]);
                }
                // Debug.Log(tempSpawning.Count());

            }

            if (tempSpawning.Count() > 0)
            {
                int r = 0;
                for (int j = 0; j < tempSpawning.Count(); j++)
                {
                    if (tempSpawning[r].priority < tempSpawning[j].priority)
                        r = j;
                }

                spawning.Add(tempSpawning[r]);
                spawnpoint.Add(points[i]);
                tempSpawning.Clear();
            }

        }

        List<int> indices = Enumerable.Range(0, spawning.Count()).ToList();

        // Fisher-Yates shuffle
        for (int i = indices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = indices[i];
            indices[i] = indices[j];
            indices[j] = temp;
        }

        for (int n = 0; n < indices.Count; n++)
        {
            int i = indices[n]; // Use shuffled index

            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            RaycastHit hit;
            Vector3 rayOrigin = new Vector3(spawnpoint[i].x - (scale.x / 2f), 100, spawnpoint[i].y - (scale.x / 2f)) + transform.position;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 100f, layer))
            {
                SheepObject randomSheepobject = spawning[i];

                GameObject sheep = Instantiate(randomSheepobject.sheep, hit.point + new Vector3(0, 0.5f, 0), randomRotation);
                sheep.name = "Sheep_" + i;
                sheep.transform.SetParent(sheepParent);

                AdvancedSheepController controller = sheep.GetComponent<AdvancedSheepController>();

                controller.PlayerTransform = playerController.transform;

                int sheepSize = GetRandomSize(false);
                float myScale = 0f;
                switch (sheepSize)
                {
                    case 1:
                        myScale = 0.75f;
                        break;
                    case 2:
                        myScale = 1.15f;
                        break;
                    case 3:
                        myScale = 1.3f;
                        break;
                }

                sheep.transform.localScale = Vector3.one * myScale;

                controller.woolSize = sheepSize;
                controller.woolColorIndex = spawning[i].colorIndex;

                foreach (var obj in controller.woolObjects)
                {
                    obj.GetComponent<Renderer>().material = randomSheepobject.color;
                }

                sheepControllers.Add(controller);
            }
        }

    }

    public void SpawnRareSheep(int amount, float probability)
    {
        int count = 0;
        for (int i = 0; i < rareSheepSpawnpoint.Count(); i++)
        {
            if (Random.Range(0f, 1f) > probability)
            {
                Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                int randomSheep = rareSheepIndices[Random.Range(0, rareSheepIndices.Length)];
                Vector3 jit = Random.insideUnitSphere * jitter;
                jit.y = 0;
                RaycastHit hit;

                if (Physics.Raycast(rareSheepSpawnpoint[i].position + jit, Vector3.down, out hit, 100f, layer))
                {
                    SheepObject randomSheepObject = sheeps[randomSheep];

                    GameObject sheep = Instantiate(randomSheepObject.sheep, hit.point + new Vector3(0, 0.5f, 0), randomRotation);
                    sheep.name = "RareSheep_" + i;
                    sheep.transform.SetParent(sheepParent);
                    rareSheepsPositions.Add(sheep.transform);

                    AdvancedSheepController controller = sheep.GetComponent<AdvancedSheepController>();

                    controller.PlayerTransform = playerController.transform;

                    int sheepSize = GetRandomSize(true);
                    float myScale = sheepSize / 10f;
                    myScale += 1;
                    sheep.transform.localScale *= myScale;

                    controller.woolSize = sheepSize;
                    controller.woolColorIndex = randomSheepObject.colorIndex;

                    foreach (var obj in controller.woolObjects)
                    {
                        obj.GetComponent<Renderer>().material = randomSheepObject.color;
                        break;
                    }
                    count++;

                    sheepControllers.Add(controller);
                }
            }

            if (count >= amount)
                break;
        }
    }

    private int GetRandomSize(bool isRare)
    {
        float randomValue = Random.Range(0f, 1f);
        if (isRare)
        {
            if (randomValue < 0.4f)
            {
                return 1; // 40% chance for size 1
            }
            else if (randomValue < 0.75f)
            {
                return 2; // 35% chance for size 2
            }
            else
            {
                return 3; // 25% chance for size 3
            }
        }
        else
        {
            if (randomValue < 0.5f)
            {
                return 1; // 50% chance for size 1
            }
            else if (randomValue < 0.85f)
            {
                return 2; // 35% chance for size 2
            }
            else
            {
                return 3; // 15% chance for size 3
            }
        }
    }

    private void Update()
    {
        foreach (var sheep in sheepControllers)
        {
            if (sheep != null)
            {
                sheep.ManualUpdate();
            }
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            RegenerateAllPenSheep();
        }
    }

    public void RemoveSheep(AdvancedSheepController sheep)
    {
        if (sheepControllers.Contains(sheep))
        {
            sheepControllers.Remove(sheep);
        }
    }

    public void AddSheep(AdvancedSheepController sheep)
    {
        if (!sheepControllers.Contains(sheep))
        {
            sheepControllers.Add(sheep);
        }
    }
}
