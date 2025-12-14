using UnityEngine;
using System.Collections;

public class HitEffectManager : MonoBehaviour
{
    public static HitEffectManager Instance;
    private Sprite circleSprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        // Don't destroy if duplicate, just let it be (or handle singleton properly)
        
        if (circleSprite == null)
        {
            circleSprite = CreateCircleSprite();
        }
    }

    private Sprite CreateCircleSprite()
    {
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = (size / 2f) - 2; // Leave some padding

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                if (dist <= radius)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    public void ShowHitEffect(Vector3 position, Vector3 targetScale, bool isWhite)
    {
        StartCoroutine(SpawnEffect(position, targetScale, isWhite));
    }

    private IEnumerator SpawnEffect(Vector3 position, Vector3 targetScale, bool isWhite)
    {
        GameObject effect = new GameObject("HitEffect");
        effect.transform.position = position;
        
        // Use absolute scale to avoid negative flipping
        Vector3 scale = new Vector3(Mathf.Abs(targetScale.x), Mathf.Abs(targetScale.y), 1f) * 1.5f;
        effect.transform.localScale = scale;

        SpriteRenderer sr = effect.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite;
        sr.color = isWhite ? Color.white : Color.black;
        sr.sortingOrder = 20; // High sorting order to appear on top

        float duration = 0.1f;
        yield return new WaitForSeconds(duration);

        Destroy(effect);
    }
}
