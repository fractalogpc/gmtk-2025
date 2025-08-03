using UnityEngine;
using TMPro;

public class WaypointManager : MonoBehaviour
{

    [SerializeField] private Transform waypointsParent;
    [SerializeField] private GameObject waypointRenderPrefab;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float waypointDeactivateRadius = 10f;

    private GameObject[] waypointRenderObjects;
    private GameObject[] waypoints;

    private void Start()
    {
        waypoints = new GameObject[waypointsParent.childCount];
        for (int i = 0; i < waypointsParent.childCount; i++)
        {
            waypoints[i] = waypointsParent.GetChild(i).gameObject;
        }

        waypointRenderObjects = new GameObject[waypoints.Length];
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] != null)
            {
                waypointRenderObjects[i] = Instantiate(waypointRenderPrefab, waypoints[i].transform.position, Quaternion.identity, transform);
                waypointRenderObjects[i].name = "WaypointRender_" + i;
                waypointRenderObjects[i].GetComponent<TextMeshProUGUI>().text = waypoints[i].name;
            }
        }
    }

    private void LateUpdate()
    {
        UpdateWaypoints();
    }

    private void UpdateWaypoints()
    {
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;

            if (waypoints[i].activeSelf)
            {
                waypointRenderObjects[i].SetActive(true);

                // Check if the waypoint is close to the player
                if (Vector3.Distance(mainCamera.transform.position, waypoints[i].transform.position) < waypointDeactivateRadius)
                {
                    waypointRenderObjects[i].SetActive(false);
                    continue;
                }

                // Update position of renderer on canvas
                    Vector3 screenPos = mainCamera.WorldToScreenPoint(waypoints[i].transform.position);
                if (screenPos.z < 0) waypointRenderObjects[i].SetActive(false);
                waypointRenderObjects[i].transform.position = screenPos;
            }
            else
            {
                waypointRenderObjects[i].SetActive(false);
            }
        }
    }

}
