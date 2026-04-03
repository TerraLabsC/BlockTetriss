// Cell.cs
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private Sprite[] normalSprites;
    [SerializeField] private Sprite[] highlightSprites;
    [SerializeField] private Color[] colors;

    private SpriteRenderer spriteRenderer;
    private int colorIndex = 0;

    public ParticleSystem particle;

    private ChaneColorMateriaParticles particleScript;

    public ParticleSystem trailMagnet;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        particleScript = GetComponentInParent<ChaneColorMateriaParticles>();
    }

    public void SetColor(int index)
    {
        colorIndex = index;

        PariclesColor();
    }

    public void Normal()
    {
        gameObject.SetActive(true);
        spriteRenderer.color = Color.white;

        spriteRenderer.sprite = normalSprites[colorIndex];

        PariclesColor();
    }

    public void Highlight()
    {
        gameObject.SetActive(true);
        spriteRenderer.color = Color.white;

        spriteRenderer.sprite = highlightSprites[colorIndex];

        PariclesColor();
    }

    public void Hover()
    {
        gameObject.SetActive(true);

        spriteRenderer.color = new(1.0f, 1.0f, 1.0f, 0.5f);
        spriteRenderer.sprite = normalSprites[colorIndex];

        PariclesColor();
    }

    public void PariclesColor()
    {
        if (particleScript != null)
        {
            particleScript.ChangeColor(colors[colorIndex]);
        }

        if (trailMagnet != null)
        {
            trailMagnet.startColor = colors[colorIndex];
        }
    }

    public void Hide()
    {
        gameObject?.SetActive(false);
    }
}