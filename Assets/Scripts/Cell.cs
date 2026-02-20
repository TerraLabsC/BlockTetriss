using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Sprite[] normalSprites;
    [SerializeField] private Sprite[] highlightSprites;

    private SpriteRenderer spriteRenderer;
    private int colorIndex = 0;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetColor(int index)
    {
        colorIndex = index;
    }

    public void Normal()
    {
        gameObject.SetActive(true);
        spriteRenderer.color = Color.white;

        spriteRenderer.sprite = normalSprites[colorIndex];
    }

    public void Highlight() 
    {
        gameObject.SetActive(true);
        spriteRenderer.color = Color.white;

        spriteRenderer.sprite = highlightSprites[colorIndex];
    }

    public void Hover()
    {
        gameObject.SetActive(true);

        spriteRenderer.color = new(1.0f, 1.0f, 1.0f, 0.5f);
        spriteRenderer.sprite = normalSprites[colorIndex];
    }

    public void Hide() 
    { 
        gameObject?.SetActive(false);
    }
}
