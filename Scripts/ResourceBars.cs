using UnityEngine;
using UnityEngine.UI;
using TMPro; // Solo si usas textos

public class ResourceBars : MonoBehaviour
{
    public Image healthBarFill;
    public Image manaBarFill;
    public Sphere playerSphere;
    
    // Variables de depuración
    [Header("Depuración")]
    public bool debugMode = false;
    [Range(0, 1)]
    public float debugHealthFill = 1.0f;
    [Range(0, 1)]
    public float debugManaFill = 1.0f;

    void Update()
    {
        // Modo de depuración para probar manualmente
        if (debugMode)
        {
            if (healthBarFill != null)
                healthBarFill.fillAmount = debugHealthFill;
            
            if (manaBarFill != null)
                manaBarFill.fillAmount = debugManaFill;
            
            // Si está en modo debug, salir para no modificar las barras con valores reales
            return;
        }
        
        // Verificar referencias
        if (playerSphere == null)
        {
            Debug.LogError("ResourceBars: No hay referencia al jugador asignada.");
            return;
        }
        
        if (healthBarFill == null || manaBarFill == null)
        {
            Debug.LogError("ResourceBars: Faltan referencias a las barras de vida o mana.");
            return;
        }
        
        // Actualizar barra de vida
        float healthRatio = Mathf.Clamp01(playerSphere.currentHealth / playerSphere.maxHealth);
        healthBarFill.fillAmount = healthRatio;
        
        // Actualizar barra de mana
        float manaRatio = Mathf.Clamp01(playerSphere.currentMana / playerSphere.maxMana);
        manaBarFill.fillAmount = manaRatio;
    }
}