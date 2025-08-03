using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimateImagesOverTime : MonoBehaviour
{
    [SerializeField] private List<Sprite> sprites;
    [SerializeField] private float timePerSprite = 0.5f;
    [SerializeField] private Image imageComponent;

    private int currentSpriteIndex = 0;
    private float timer = 0f;

    private void Start()
    {
        if (imageComponent == null)
        {
            imageComponent = GetComponent<Image>();
        }

        if (sprites.Count > 0)
        {
            imageComponent.sprite = sprites[currentSpriteIndex];
        }
    }

    private void Update()
    {
        if (sprites.Count == 0 || imageComponent == null) return;

        timer += Time.deltaTime;
        if (timer >= timePerSprite)
        {
            timer = 0f;
            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Count;
            imageComponent.sprite = sprites[currentSpriteIndex];
        }
    }
}
