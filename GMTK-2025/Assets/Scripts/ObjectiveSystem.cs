using UnityEngine;
using TMPro;

public class ObjectiveSystem : MonoBehaviour
{

    [System.Serializable]
    public class Objective
    {
        public string name;
        public string label;
        public GameObject[] enableWhenActive;
        public bool isCompleted;
    }

    [SerializeField] private Objective[] objectives;
    [SerializeField] private TextMeshProUGUI objectiveText;
    
    private int currentObjectiveIndex = 0;
    
    public static ObjectiveSystem Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void UpdateObjectiveText()
    {
        if (currentObjectiveIndex < 0 || currentObjectiveIndex >= objectives.Length)
        {
            objectiveText.text = "";
            return;
        }
        else
        {
            objectiveText.text = objectives[currentObjectiveIndex].label;
        }
    }

    private void Start()
    {
        if (objectives.Length > 0)
        {
            UpdateObjectiveText();
        }
        else
        {
            Debug.LogWarning("No objectives set in ObjectiveSystem.");
        }

        // Enable objects for the first objective
        if (objectives.Length > 0)
        {
            foreach (GameObject obj in objectives[0].enableWhenActive)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        
        // Disable objects for all other objectives
        for (int i = 1; i < objectives.Length; i++)
        {
            foreach (GameObject obj in objectives[i].enableWhenActive)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
        
    }

    public void CompleteObjectiveByName(string name)
    {
        for (int i = 0; i < objectives.Length; i++)
        {
            if (objectives[i].name == name)
            {
                CompleteObjective(i);
                return;
            }
        }
        Debug.LogWarning("Objective with name " + name + " not found.");
    }

    public void CompleteObjectiveByIndex(int index)
    {
        if (index >= 0 && index < objectives.Length)
        {
            CompleteObjective(index);
        }
        else
        {
            Debug.LogWarning("Invalid objective index: " + index);
        }
    }

    public void CompleteObjective(int index = -1, string name = null)
    {
        // Interpret name to an index
        if (name != null && index == -1)
        {
            for (int i = 0; i < objectives.Length; i++)
            {
                if (objectives[i].name == name)
                {
                    index = i;
                    break;
                }
            }
        }

        if (objectives[index].isCompleted)
        {
            Debug.LogWarning("Objective " + objectives[index].name + " is already completed.");
            return;
        }

        // Check if all prior objectives are completed
        for (int i = 0; i < index; i++)
        {
            if (!objectives[i].isCompleted)
            {
                Debug.LogWarning("Cannot complete objective before completing all prior objectives.");
                return;
            }
        }

        // Complete the objective
        if (index >= 0 && index < objectives.Length)
        {
            objectives[index].isCompleted = true;

            // Enable objects for the next objective
            if (index + 1 < objectives.Length)
            {
                currentObjectiveIndex = index + 1;
                foreach (GameObject obj in objectives[index + 1].enableWhenActive)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                    }
                }
            }

            // Disable objects for the completed objective
            foreach (GameObject obj in objectives[index].enableWhenActive)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }

            UpdateObjectiveText();
        }
        else
        {
            Debug.LogWarning("Invalid objective index: " + index);
        }
    }

}
