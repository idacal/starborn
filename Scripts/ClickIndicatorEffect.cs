using UnityEngine;

public class ClickIndicatorEffect : MonoBehaviour
{
    public float fadeSpeed = 2f;
    public float scaleSpeed = 2f;
    public float maxScale = 2f;
    
    private Material material;
    private float initialAlpha;
    private Vector3 initialScale;

    void Start()
    {
        // Obtener el material y guardarlo
        material = GetComponent<Renderer>().material;
        initialAlpha = material.color.a;
        initialScale = transform.localScale;
        
        // Hacer una copia del material para no afectar al prefab
        material = new Material(material);
        GetComponent<Renderer>().material = material;
    }

    void Update()
    {
        // Desvanecer
        Color color = material.color;
        color.a = Mathf.Lerp(color.a, 0, Time.deltaTime * fadeSpeed);
        material.color = color;

        // Escalar
        transform.localScale = Vector3.Lerp(transform.localScale, 
                                          initialScale * maxScale, 
                                          Time.deltaTime * scaleSpeed);

        // Destruir cuando esté casi invisible
        if (color.a < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}