using UnityEngine;
using UnityEngine.UI;

public class AbilitySystem : MonoBehaviour
{
    // Referencias a los elementos UI
    public Image qCooldownImage;
    public Image wCooldownImage;
    public Image eCooldownImage;
    public Image rCooldownImage;
    
    // Colores para indicar disponibilidad de mana
    public Color normalColor = Color.white;
    public Color insufficientManaColor = Color.gray;
    
    // Referencias a los iconos de habilidades (opcional)
    public Image qIcon;
    public Image wIcon;
    public Image eIcon;
    public Image rIcon;

    // Referencia al objeto Sphere
    public Sphere playerSphere;

    void Update()
    {
        if (playerSphere == null) return;
        
        // Actualizar la visualización de cooldowns desde la esfera del jugador
        UpdateCooldownVisuals();
        
        // Actualizar indicadores de mana suficiente
        UpdateManaAvailability();
    }

    void UpdateCooldownVisuals()
    {
        if (playerSphere == null) return;

        // Actualizar visuales de cooldown Q (Dash)
        if (playerSphere.canDash)
        {
            qCooldownImage.fillAmount = 0f; // Sin cooldown
        }
        else
        {
            float dashCooldownRemaining = playerSphere.GetDashCooldownRemaining();
            qCooldownImage.fillAmount = dashCooldownRemaining / playerSphere.dashCooldown;
        }

        // Actualizar visuales de cooldown W (Harden)
        if (playerSphere.canHarden)
        {
            wCooldownImage.fillAmount = 0f; // Sin cooldown
        }
        else
        {
            float hardenCooldownRemaining = playerSphere.GetHardenCooldownRemaining();
            wCooldownImage.fillAmount = hardenCooldownRemaining / playerSphere.hardenCooldown;
        }

        // Actualizar visuales de cooldown E (Vanish)
        if (playerSphere.canVanish)
        {
            eCooldownImage.fillAmount = 0f; // Sin cooldown
        }
        else
        {
            float vanishCooldownRemaining = playerSphere.GetVanishCooldownRemaining();
            eCooldownImage.fillAmount = vanishCooldownRemaining / playerSphere.vanishCooldown;
        }

        // Actualizar visuales de cooldown R (Grow)
        if (playerSphere.canGrow)
        {
            rCooldownImage.fillAmount = 0f; // Sin cooldown
        }
        else
        {
            float growCooldownRemaining = playerSphere.GetGrowCooldownRemaining();
            rCooldownImage.fillAmount = growCooldownRemaining / playerSphere.growCooldown;
        }
    }
    
    void UpdateManaAvailability()
    {
        // Verificar si hay iconos configurados
        bool hasIcons = qIcon != null && wIcon != null && eIcon != null && rIcon != null;
        if (!hasIcons) return;
        
        // Verificar mana para cada habilidad
        bool enoughManaForQ = playerSphere.currentMana >= playerSphere.dashManaCost;
        bool enoughManaForW = playerSphere.currentMana >= playerSphere.hardenManaCost;
        bool enoughManaForE = playerSphere.currentMana >= playerSphere.vanishManaCost;
        bool enoughManaForR = playerSphere.currentMana >= playerSphere.growManaCost;
        
        // Actualizar colores de iconos
        qIcon.color = enoughManaForQ ? normalColor : insufficientManaColor;
        wIcon.color = enoughManaForW ? normalColor : insufficientManaColor;
        eIcon.color = enoughManaForE ? normalColor : insufficientManaColor;
        rIcon.color = enoughManaForR ? normalColor : insufficientManaColor;
    }
}