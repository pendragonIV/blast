using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool IsAlive = true;

    public void SetRendererColour(Renderer renderer, Color colour)
    {
        renderer.material.color = colour;
    }

    public void SetRendererTexture(Renderer renderer, Texture2D texture)
    {
        renderer.material.SetTexture("_MainTex", texture);
    }
}
