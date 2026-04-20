using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SimpleShapeSprite : MonoBehaviour
{
    [SerializeField] private int size = 32;
    [SerializeField] private float pixelsPerUnit = 32f;
    [SerializeField] private Color color = Color.white;

    private void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        Sprite generatedSprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            pixelsPerUnit);

        spriteRenderer.sprite = generatedSprite;
    }
}
