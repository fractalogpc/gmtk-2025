using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    public SheepObject[] sheep;
    public Vector2 _scale, _amount;
    public float jitter;


    public List<Vector2> points = new List<Vector2>();
    public List<SheepObject> tempSpawning = new List<SheepObject>();
    public List<GameObject> spawning = new List<GameObject>();
    public List<Vector2> spawnpoint = new List<Vector2>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SpawnSheepWave(_scale, _amount, jitter);
        }
    }


    public void SpawnSheepWave(Vector2 scale, Vector2 amount, float jitter)
    {
        
        for (int i = 0; i < amount.x; i++)
        {
            for (int j = 0; j < amount.y; j++)
            {
                points.Add(new Vector2(i * (scale.x / amount.x), j * (scale.y / amount.y)) + Random.insideUnitCircle * jitter);
            }
        }

        for (int i = 0; i < points.Count; i++)
        {
            for (int j = 0; j < sheep.Count(); j++)
            {
                if (sheep[j].heatmap.GetPixel(Mathf.RoundToInt(points[i].x), Mathf.RoundToInt(points[i].y)).r > .2f)
                {
                    Debug.Log("Spawned");

                    tempSpawning.Add(sheep[j]);
                }
                Debug.Log(tempSpawning.Count());

            }

            int r = 0;
            for (int j = 0; j < tempSpawning.Count(); j++)
            {
                if (tempSpawning[r].priority < tempSpawning[j].priority)
                    r = j;
            }

            spawning.Add(tempSpawning[r].sheep);
            spawnpoint.Add(points[i]);
            tempSpawning.Clear();
        }

        for (int i = 0; i < spawning.Count(); i++)
        {
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(spawnpoint[i].x - (scale.x / 2f), 100, spawnpoint[i].y - (scale.x / 2f)) + transform.position, Vector3.down, out hit, 100f))
                Instantiate(spawning[i], hit.point + new Vector3(0, .5f, 0), Quaternion.identity);
        }


    }
}
