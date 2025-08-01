using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.VisualScripting;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    public SheepObject[] sheeps;
    public SheepObject[] rareSheeps;
    public Transform[] rareSheepSpawnpoint;
    public Vector2 _scale, _amount;
    public float jitter;
    public LayerMask layer;
    public PlayerController playerController;
    public Transform sheepParent;

    private void Start()
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
            for (int j = 0; j < sheeps.Count(); j++)
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
                GameObject sheep = Instantiate(spawning[i].sheep, hit.point + new Vector3(0, 0.5f, 0), randomRotation);
                sheep.name = "Sheep_" + i;
                sheep.transform.SetParent(sheepParent);

                sheep.GetComponent<AdvancedSheepController>().PlayerTransform = playerController.transform;

                int sheepSize = Random.Range(spawning[i].minSize, spawning[i].maxSize + 1);
                float myScale = sheepSize / 10f;
                myScale += 1;
                sheep.transform.localScale *= myScale;

                sheep.GetComponent<AdvancedSheepController>().woolSize = sheepSize;
                sheep.GetComponent<AdvancedSheepController>().woolColorIndex = spawning[i].colorIndex;

                foreach (var rend in sheep.GetComponent<AdvancedSheepController>().woolObjects)
                {
                    rend.GetComponent<Renderer>().material = spawning[i].color;   
                }
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
                int randomSheep = Random.Range(0, rareSheeps.Count());
                Vector3 jit = Random.insideUnitSphere * jitter;
                jit.y = 0;
                RaycastHit hit;

                if (Physics.Raycast(rareSheepSpawnpoint[i].position + jit, Vector3.down, out hit, 100f, layer))
                {
                    GameObject sheep = Instantiate(rareSheeps[randomSheep].sheep, hit.point + new Vector3(0, 0.5f, 0), randomRotation);
                    sheep.name = "RareSheep_" + i;
                    sheep.transform.SetParent(sheepParent);

                    sheep.GetComponent<AdvancedSheepController>().PlayerTransform = playerController.transform;

                    int sheepSize = Random.Range(rareSheeps[randomSheep].minSize, rareSheeps[randomSheep].maxSize + 1);
                    float myScale = sheepSize / 10f;
                    myScale += 1;
                    sheep.transform.localScale *= myScale;

                    sheep.GetComponent<AdvancedSheepController>().woolSize = sheepSize;
                    sheep.GetComponent<AdvancedSheepController>().woolColorIndex = rareSheeps[randomSheep].colorIndex;

                    foreach (var rend in sheep.GetComponentsInChildren<Renderer>(true))
                    {
                        if (rend.gameObject.name == "sheep-colorable")
                        {
                            rend.material = rareSheeps[randomSheep].color;
                            break;
                        }
                    }
                    count++;
                }
            }

            if (count >= amount)
                break;
        }
    }
}
