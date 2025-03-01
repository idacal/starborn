using UnityEngine;
using UnityEngine.UI; // Para Text o TextMeshProUGUI
using TMPro; // Para TextMeshProUGUI

public class ManaBarController : MonoBehaviour
{
    public Sphere playerSphere;  // Arrastra tu objeto Sphere aquí
    public TextMeshProUGUI manaText; // Arrastra aquí el texto que mostrará los valores de mana
    
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
        
        // Calcular el ratio de mana actual
        float manaRatio = playerSphere.currentMana / playerSphere.maxMana;
        
        // Asegurar que el ratio está entre 0 y 1
        manaRatio = Mathf.Clamp01(manaRatio);
        
        // Actualizar solo la escala X para que la barra se ajuste horizontalmente
        Vector3 newScale = originalScale;
        newScale.x = originalScale.x * manaRatio;
        rectTransform.localScale = newScale;
        
        // Asegurar que el pivote esté a la izquierda para que se encoja desde la derecha
        rectTransform.pivot = new Vector2(0, rectTransform.pivot.y);
        
        // Actualizar el texto con los valores de mana actuales
        if (manaText != null)
        {
            manaText.text = $"{Mathf.Round(playerSphere.currentMana)}/{playerSphere.maxMana}";
        }
    }
}