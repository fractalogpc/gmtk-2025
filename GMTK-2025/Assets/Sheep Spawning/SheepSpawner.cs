using System.Collections.Generic;
using System.Linq;
using Player;
using Unity.VisualScripting;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    public SheepObject[] sheep;
    public Vector2 _scale, _amount;
    public float jitter;
    public LayerMask layer;
    public PlayerController playerController;
    public Transform shoopParent;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // SpawnSheepWave(_scale, _amount, jitter);
        }
    }

    private void Start()
    {
        SpawnSheepWave(_scale, _amount, jitter);
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
            for (int j = 0; j < sheep.Count(); j++)
            {
                if (sheep[j].heatmap.GetPixel(Mathf.RoundToInt((points[i].x / scale.x) * sheep[j].heatmap.width), Mathf.RoundToInt((points[i].y / scale.y) * sheep[j].heatmap.height)).r > .2f)
                {
                    // Debug.Log("Spawned");

                    tempSpawning.Add(sheep[j]);
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

        for (int i = 0; i < spawning.Count(); i++)
        {
            Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(spawnpoint[i].x - (scale.x / 2f), 100, spawnpoint[i].y - (scale.x / 2f)) + transform.position, Vector3.down, out hit, 100f, layer))
            {
                GameObject shoop = Instantiate(spawning[i].sheep, hit.point + new Vector3(0, .5f, 0), Quaternion.identity);
                shoop.name = "Sheep_" + i;
                shoop.transform.SetParent(shoopParent);
                shoop.GetComponent<AdvancedSheepController>().playerTransform = playerController.transform;
                float myScale = Random.Range(0.8f, 1.3f);
                shoop.transform.localScale *= myScale;
                foreach (var rend in shoop.GetComponentsInChildren<Renderer>(true))
                {
                    if (rend.gameObject.name == "sheep-colorable")
                    {
                        rend.material = spawning[i].color;
                        break;
                    }
                }
            }
        }


    }
}
