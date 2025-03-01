using UnityEngine;
using UnityEngine.UI; // Para Text o TextMeshProUGUI
using TMPro; // Para TextMeshProUGUI

public class HPBarController : MonoBehaviour
{
    public Sphere playerSphere;  // Arrastra tu objeto Sphere aquí
    public TextMeshProUGUI healthText; // Arrastra aquí el texto que mostrará los valores de vida
    
    private RectTransform rectTransform;
    private Vector3 originalScale;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    void Update()
    {
        if (playerSphere == null) return;
        
        // Calcular el ratio de vida actual
        float healthRatio = playerSphere.currentHealth / playerSphere.maxHealth;
        
        // Asegurar que el ratio está entre 0 y 1
        healthRatio = Mathf.Clamp01(healthRatio);
        
        // Actualizar solo la escala X
        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * healthRatio;
        rectTransform.localScale = newScale;
        
        // Asegurar que el pivote esté a la izquierda para que se encoja desde la derecha
        rectTransform.pivot = new Vector2(0, rectTransform.pivot.y);
        
        // Actualizar el texto con los valores de vida actuales
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(playerSphere.currentHealth)}/{playerSphere.maxHealth}";
        }
    }
}