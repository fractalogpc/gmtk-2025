using UnityEngine;

public class SimpleSheepSpawner : MonoBehaviour
{
    public float radius = 50f;
    public Vector3 center;

    public GameObject sheepPrefab;
    public int count = 50;

    public Vector3[] GetPointsInDisc(int count)
    {
        Vector3[] points = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float r = radius * Mathf.Sqrt(Random.Range(0f, 1f)); // sqrt for uniform disc
            float x = center.x + r * Mathf.Cos(angle);
            float z = center.z + r * Mathf.Sin(angle);
            float y = center.y;
            points[i] = new Vector3(x, y, z);
        }
        return points;
    }

    public void SpawnSheep(GameObject sheepPrefab, int count)
    {
        Vector3[] spawnPoints = GetPointsInDisc(count);
        for (int i = 0; i < count; i++)
        {
            Instantiate(sheepPrefab, spawnPoints[i], Quaternion.identity);
        }
    }

    void Start()
    {
        SpawnSheep(sheepPrefab, count);
    }

}
