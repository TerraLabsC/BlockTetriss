using UnityEngine;

public class ChaneColorMateriaParticles : MonoBehaviour
{
    public ParticleSystem particleColor;

    [System.Obsolete]
    public void ChangeColor(Color color) => particleColor.startColor = color;
}
