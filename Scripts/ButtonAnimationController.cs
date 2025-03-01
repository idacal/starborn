using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonAnimationController : MonoBehaviour
{
    public Button qButton;
    public Button wButton;
    public Button eButton;
    public Button rButton;

    // Referencias para todos los textos de cooldown
    public TextMeshProUGUI qCooldownText;
    public TextMeshProUGUI wCooldownText;
    public TextMeshProUGUI eCooldownText;
    public TextMeshProUGUI rCooldownText;

    private Sphere sphereScript;

    void Start()
    {
        sphereScript = FindObjectOfType<Sphere>();
    }

    void Update()
    {
        // Habilidad Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            qButton.image.color = qButton.colors.pressedColor;
        }
        if (Input.GetKeyUp(KeyCode.Q))
        {
            qButton.image.color = qButton.colors.normalColor;
        }

        // Habilidad W
        if (Input.GetKeyDown(KeyCode.W))
        {
            wButton.image.color = wButton.colors.pressedColor;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            wButton.image.color = wButton.colors.normalColor;
        }

        // Habilidad E
        if (Input.GetKeyDown(KeyCode.E))
        {
            eButton.image.color = eButton.colors.pressedColor;
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            eButton.image.color = eButton.colors.normalColor;
        }

        // Habilidad R
        if (Input.GetKeyDown(KeyCode.R))
        {
            rButton.image.color = rButton.colors.pressedColor;
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            rButton.image.color = rButton.colors.normalColor;
        }

        // Actualizar todos los textos de cooldown
        if (sphereScript != null)
        {
            // Cooldown Q
            if (sphereScript.canDash)
            {
                qCooldownText.gameObject.SetActive(false);
            }
            else
            {
                float dashCooldown = sphereScript.GetDashCooldownRemaining();
                UpdateCooldownText(qCooldownText, dashCooldown);
            }

            // Cooldown W
            if (sphereScript.canHarden)
            {
                wCooldownText.gameObject.SetActive(false);
            }
            else
            {
                float hardenCooldown = sphereScript.GetHardenCooldownRemaining();
                UpdateCooldownText(wCooldownText, hardenCooldown);
            }

            // Cooldown E
            if (sphereScript.canVanish)
            {
                eCooldownText.gameObject.SetActive(false);
            }
            else
            {
                float vanishCooldown = sphereScript.GetVanishCooldownRemaining();
                UpdateCooldownText(eCooldownText, vanishCooldown);
            }

            // Cooldown R
            if (sphereScript.canGrow)
            {
                rCooldownText.gameObject.SetActive(false);
            }
            else
            {
                float growCooldown = sphereScript.GetGrowCooldownRemaining();
                UpdateCooldownText(rCooldownText, growCooldown);
            }
        }
    }

    // Función auxiliar para actualizar los textos de cooldown
    private void UpdateCooldownText(TextMeshProUGUI cooldownText, float cooldownRemaining)
    {
        if (cooldownRemaining > 0)
        {
            cooldownText.text = cooldownRemaining.ToString("0.0");
            cooldownText.gameObject.SetActive(true);
        }
        else
        {
            cooldownText.gameObject.SetActive(false);
        }
    }
}