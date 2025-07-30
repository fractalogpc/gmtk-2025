using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    public SheepObject[] sheep;

    public List<SheepObject> tempSpawning = new List<SheepObject>();
    public List<GameObject> spawning = new List<GameObject>();
    public List<Vector2> spawnpoint = new List<Vector2>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSheepWave(new Vector2(600, 600), new Vector2(100, 100), new Vector2(300, 300), .2f);
        }
    }


    public void SpawnSheepWave(Vector2 scale, Vector2 amount, Vector2 corner, float jitter)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < amount.x; i++)
        {
            for (int j = 0; j < amount.y; j++)
            {
                points.Add(new Vector2(i * (scale.x / amount.x), j * (scale.y / amount.y)) + Random.insideUnitCircle * jitter);
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            tempSpawning.Clear();
            for (int j = 0; j < sheep.Count(); j++)
            {
                if (sheep[j].heatmap.GetPixel(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y)).r > .5f)
                {
                    tempSpawning.Add(sheep[j]);
                }

            }

            if (tempSpawning.Count > 0)
            {
                int r = 0;
                for (int j = 0; j < tempSpawning.Count(); j++)
                {
                    if (tempSpawning[r].priority < tempSpawning[j].priority)
                        r = j;
                }

                spawning.Add(tempSpawning[r].sheep);
                spawnpoint.Add(points[i]);
            }
            
        }

        for (int i = 0; i < spawning.Count(); i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(spawnpoint[i].x - corner.x, 100, spawnpoint[i].y - corner.y), Vector3.down, out hit, 100f))
                Instantiate(spawning[i], hit.point, Quaternion.identity);
        }


    }
}
