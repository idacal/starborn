using UnityEngine;
using System.Collections;

public class Sphere : MonoBehaviour
{
    // Variables de movimiento básico
    public float speed = 5f;
    public GameObject clickIndicatorPrefab;
    private Vector3 targetPosition;
    private bool isMoving = false;

    // Variables para repulsión
    public float repulsionForce = 10f;
    private Rigidbody rb;
    private bool isBeingRepelled = false;
    private float repelledTimer = 0f;
    private float repelledDuration = 0.5f;

    // Variables de dash (Q)
    public float dashSpeed = 20f;
    public float dashInitialDuration = 0.2f; // Duración inicial si no se mantiene presionado
    public float dashMaxDuration = 2.0f;     // Duración máxima si se mantiene presionado
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private Vector3 dashDirection;
    public float dashCooldown = 3f;
    private float dashCooldownTimeLeft = 0f;
    // Variables para acceso externo de estado de habilidades
    [HideInInspector] public bool canDash = true;
    public float dashInitialManaCost = 20f;  // Costo inicial de maná
    public float dashManaCost { get { return dashInitialManaCost; } set { dashInitialManaCost = value; } } // Para compatibilidad
    public float dashManaCostPerSecond = 30f; // Maná consumido por segundo al mantener
    
    // Variables para dash suave
    public enum DashType { Instant, Linear, EaseIn, EaseOut, EaseInOut }
    public DashType dashStyle = DashType.EaseInOut;
    private float dashProgress = 0f;
    public AnimationCurve dashCurve = new AnimationCurve(
        new Keyframe(0, 0, 0, 2),     // Inicio suave
        new Keyframe(0.5f, 1, 0, 0),  // Pico de velocidad
        new Keyframe(1, 0, -2, 0)     // Final suave
    );

    // Variables de endurecimiento (W)
    public float hardenDuration = 3f;
    public float hardenCooldown = 8f;
    private float hardenTimeLeft = 0f;
    private float hardenCooldownTimeLeft = 0f;
    private bool isHardened = false;
    [HideInInspector] public bool canHarden = true;
    private Material normalMaterial;
    private Material hardenedMaterial;
    public float hardenManaCost = 50f;

    // Variables de vanish (E)
    public float vanishDuration = 2f;
    public float vanishCooldown = 10f;
    private float vanishTimeLeft = 0f;
    private float vanishCooldownTimeLeft = 0f;
    private bool isVanished = false;
    [HideInInspector] public bool canVanish = true;
    public float vanishManaCost = 50f;

    // Variables de crecimiento (R)
    public float growDuration = 5f;
    public float growCooldown = 15f;
    private float growTimeLeft = 0f;
    private float growCooldownTimeLeft = 0f;
    private bool isGrown = false;
    [HideInInspector] public bool canGrow = true;
    public float growScale = 2f;
    private Vector3 originalScale;
    private bool canMove = true;
    public float growManaCost = 100f;

    // Variables de HP y Mana
    public float maxHealth = 100f;
    public float currentHealth;
    public float maxMana = 100f;
    public float currentMana;
    public float manaRegenRate = 10f; // Mana regenerado por segundo
    
    // Variables para colisiones
    public float collisionDamage = 10f;
    public GameObject collisionEffectPrefab; // Opcional: prefab para efecto visual de colisión
    public float collisionStunDuration = 1.0f; // Tiempo que la esfera queda aturdida tras una colisión

    void Start()
    {
        // Configurar materiales y escala inicial
        currentHealth = maxHealth;
        currentMana = maxMana;
        normalMaterial = GetComponent<Renderer>().material;
        hardenedMaterial = new Material(normalMaterial);
        hardenedMaterial.color = Color.gray;
        originalScale = transform.localScale;
        
        // Configurar el Rigidbody correctamente para las colisiones
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            Debug.LogError("La esfera no tiene un componente Rigidbody!");
        }
    }

    void Update()
    {
        UpdateCooldowns();
        CheckAbilities();
        HandleMovement();
        UpdateAbilityEffects();

        // Regenerar maná
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Min(currentMana, maxMana);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Verificar si colisiona con otra esfera
        Sphere otherSphere = collision.gameObject.GetComponent<Sphere>();
        if (otherSphere != null)
        {
            Debug.Log($"¡COLISIÓN! entre {gameObject.name} y {collision.gameObject.name}");
            
            // INMEDIATAMENTE detener el movimiento - CAMBIO CLAVE
            isMoving = false;
            rb.velocity = Vector3.zero;
            
            // Si estábamos usando dash, terminarlo
            if (isDashing)
            {
                isDashing = false;
            }
            
            // Calcular daño
            float damage = collisionDamage;
            if (otherSphere.isDashing) damage *= 2f;
            if (isDashing) damage *= 0.5f;
            
            // Aplicar daño si no estamos protegidos
            if (!isHardened && !isGrown)
            {
                TakeDamage(damage);
            }
            
            // Aplicar daño a la otra esfera si no está protegida
            if (!otherSphere.isHardened && !otherSphere.isGrown)
            {
                otherSphere.TakeDamage(damage);
            }
            
            // Si estamos endurecidos o agrandados, no nos repele
            if (isHardened || isGrown) return;
            
            // Dirección desde el punto de contacto hacia nuestra posición
            Vector3 contactPoint = collision.contacts[0].point;
            Vector3 repulsionDirection = (transform.position - contactPoint).normalized;
            repulsionDirection.y = 0; // Mantener la repulsión en el plano horizontal
            
            // Aumentar significativamente la fuerza para depuración
            float finalRepulsionForce = repulsionForce * 2f; // Duplicar para pruebas
            
            // Aplicar una fuerte repulsión directamente a través del Rigidbody
            rb.AddForce(repulsionDirection * finalRepulsionForce, ForceMode.Impulse);
            
            // También: Empujar la posición ligeramente para evitar superposiciones
            transform.position += repulsionDirection * 0.1f;
            
            // Imprimir mensaje de depuración para verificar la aplicación de fuerza
            Debug.Log($"Aplicando fuerza de repulsión: {finalRepulsionForce} en dirección {repulsionDirection}");
            
            // Dibujar el rayo para depuración visual
            Debug.DrawRay(contactPoint, repulsionDirection * 3, Color.red, 5f);
            
            // Bloquear movimiento por un periodo breve
            StartCoroutine(StunAfterCollision(collisionStunDuration));
        }
    }
    
    // Método para aplicar repulsión desde otra esfera
    public void ApplyRepulsionForce(Vector3 force)
    {
        // Detener el movimiento actual
        isMoving = false;
        isDashing = false;
        rb.velocity = Vector3.zero;
        
        // Aplicar la fuerza de repulsión
        rb.AddForce(force, ForceMode.Impulse);
        
        // Aturdir brevemente
        StartCoroutine(StunAfterCollision(collisionStunDuration));
    }
    
    // Corrutina para aturdir a la esfera tras una colisión
    private IEnumerator StunAfterCollision(float duration)
    {
        canMove = false;
        
        yield return new WaitForSeconds(duration);
        
        canMove = true;
    }

    // Método para recibir daño
    public void TakeDamage(float amount)
    {
        if (isHardened)
        {
            // Reducir el daño recibido si está endurecido
            amount *= 0.25f;
        }
        
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log(gameObject.name + " recibió " + amount + " de daño. Salud restante: " + currentHealth);
        
        // Si la vida llega a 0, implementar lógica de derrota
        if (currentHealth <= 0)
        {
            Debug.Log("Esfera " + gameObject.name + " derrotada!");
            // Aquí puedes implementar lo que sucede cuando la esfera pierde toda su vida
            // Por ejemplo: Destroy(gameObject);
        }
    }

    void EnableMovement()
    {
        canMove = true;
    }

    void UpdateCooldowns()
    {
        if (!canDash)
        {
            dashCooldownTimeLeft -= Time.deltaTime;
            if (dashCooldownTimeLeft <= 0) canDash = true;
        }

        if (!canHarden)
        {
            hardenCooldownTimeLeft -= Time.deltaTime;
            if (hardenCooldownTimeLeft <= 0) canHarden = true;
        }

        if (!canVanish)
        {
            vanishCooldownTimeLeft -= Time.deltaTime;
            if (vanishCooldownTimeLeft <= 0) canVanish = true;
        }

        if (!canGrow)
        {
            growCooldownTimeLeft -= Time.deltaTime;
            if (growCooldownTimeLeft <= 0) canGrow = true;
        }
    }

    void UpdateAbilityEffects()
    {
        if (isHardened)
        {
            hardenTimeLeft -= Time.deltaTime;
            if (hardenTimeLeft <= 0) EndHarden();
        }

        if (isVanished)
        {
            vanishTimeLeft -= Time.deltaTime;
            if (vanishTimeLeft <= 0) EndVanish();
        }

        if (isGrown)
        {
            growTimeLeft -= Time.deltaTime;
            if (growTimeLeft <= 0) EndGrow();
        }
    }

    void CheckAbilities()
    {
        // No permitir habilidades si la esfera está aturdida
        if (!canMove) return;
        
        // Solo procesar entradas de teclado si es la esfera del jugador
        if (gameObject.CompareTag("Player"))
        {
            // Habilidad de Dash (Q)
            // Iniciar el dash cuando se presiona Q
            if (Input.GetKeyDown(KeyCode.Q) && canDash && !isDashing && isMoving)
            {
                if (currentMana >= dashInitialManaCost)
                {
                    Debug.Log("Iniciando dash");
                    currentMana -= dashInitialManaCost;
                    StartDash();
                }
            }
            
            // Terminar el dash cuando se suelta Q o se agota el maná
            if (isDashing && !Input.GetKey(KeyCode.Q))
            {
                EndDashEarly();
            }

            // Habilidad de Harden (W)
            if (Input.GetKeyDown(KeyCode.W) && canHarden)
            {
                if (currentMana >= hardenManaCost)
                {
                    currentMana -= hardenManaCost;
                    StartHarden();
                }
            }

            // Habilidad de Vanish (E)
            if (Input.GetKeyDown(KeyCode.E) && canVanish)
            {
                if (currentMana >= vanishManaCost)
                {
                    currentMana -= vanishManaCost;
                    StartVanish();
                }
            }

            // Habilidad de Grow (R)
            if (Input.GetKeyDown(KeyCode.R) && canGrow)
            {
                if (currentMana >= growManaCost)
                {
                    currentMana -= growManaCost;
                    StartGrow();
                }
            }
        }
    }

    void HandleMovement()
    {
        // No procesar movimiento si está deshabilitado
        if (!canMove) return;

        // VERIFICAR SI ES LA ESFERA DEL JUGADOR
        if (gameObject.CompareTag("Player"))
        {
            // Solo procesar clics si es la esfera del jugador
            if (Input.GetMouseButtonDown(1))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    targetPosition = hit.point;
                    isMoving = true;
                    Vector3 spawnPosition = hit.point + new Vector3(0, 0.1f, 0);
                    Instantiate(clickIndicatorPrefab, spawnPosition, Quaternion.identity);
                }
            }
        }

        // El resto del código de movimiento se mantiene igual para todas las esferas
        // Una vez que la esfera del jugador establece isMoving=true tras un clic
        if (isDashing)
        {
            UpdateDash();
        }
        else if (isMoving)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;
            
            Vector3 targetVelocity = direction * speed;
            rb.velocity = targetVelocity;

            // Verificar si llegamos al destino
            if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            {
                isMoving = false;
                rb.velocity = Vector3.zero;
            }
        }
        else
        {
            rb.velocity = Vector3.zero;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTimeLeft = dashMaxDuration; // Usamos la duración máxima posible
        dashProgress = 0f;
        dashDirection = (targetPosition - transform.position).normalized;
        dashDirection.y = 0;
        
        // Aplicamos un impulso inicial para que el dash se sienta más impactante
        rb.velocity = dashDirection * dashSpeed * 0.5f;
        
        // No activamos el cooldown aquí, lo haremos cuando termine el dash
    }

    void UpdateDash()
    {
        if (dashTimeLeft > 0 && isDashing)
        {
            // Consumir maná continuamente mientras se hace dash
            float manaCost = dashManaCostPerSecond * Time.deltaTime;
            
            // Si no hay suficiente maná, terminar el dash
            if (currentMana < manaCost)
            {
                EndDashEarly();
                return;
            }
            
            // Consumir maná
            currentMana -= manaCost;
            
            // Calcular el progreso del dash para la curva de velocidad
            // Usamos un valor fijo pequeño para que la curva se mantenga en la parte alta
            // mientras se mantiene la tecla presionada
            dashProgress = Mathf.Min(0.5f, 1 - (dashTimeLeft / dashMaxDuration));
            
            float speedMultiplier;
            
            switch (dashStyle)
            {
                case DashType.Instant:
                    speedMultiplier = 1.0f; // Dash constante (como era antes)
                    break;
                case DashType.Linear:
                    speedMultiplier = 1.0f; // Mantenemos velocidad constante mientras se presiona
                    break;
                case DashType.EaseIn:
                    speedMultiplier = 1.0f; // Mantenemos velocidad constante mientras se presiona
                    break;
                case DashType.EaseOut:
                    speedMultiplier = 1.0f; // Mantenemos velocidad constante mientras se presiona
                    break;
                case DashType.EaseInOut:
                    // Usamos la curva solo para el inicio, luego mantenemos constante
                    speedMultiplier = dashProgress < 0.3f ? dashCurve.Evaluate(dashProgress) : 1.0f;
                    break;
                default:
                    speedMultiplier = 1.0f;
                    break;
            }
            
            // Aplicar la velocidad actual con el multiplicador
            rb.velocity = dashDirection * dashSpeed * speedMultiplier;
            dashTimeLeft -= Time.deltaTime;
            
            // Reorientar el dash hacia donde apunta el cursor si sigue moviéndose
            if (isMoving && Vector3.Distance(transform.position, targetPosition) > 1.0f)
            {
                dashDirection = (targetPosition - transform.position).normalized;
                dashDirection.y = 0;
            }
        }
        else if (isDashing)
        {
            // El tiempo se ha agotado, terminamos el dash
            EndDashComplete();
        }
    }
    
    // Método para terminar el dash prematuramente (al soltar Q o quedarse sin maná)
    void EndDashEarly()
    {
        // Finalizar el dash suavemente
        isDashing = false;
        
        // Aplicar una desaceleración gradual
        StartCoroutine(SmoothDashEnd());
        
        // Activar cooldown
        canDash = false;
        dashCooldownTimeLeft = dashCooldown;
    }
    
    // Método para terminar el dash completamente (tiempo máximo alcanzado)
    void EndDashComplete()
    {
        // Finalizar el dash
        isDashing = false;
        
        // Aplicar una desaceleración gradual
        StartCoroutine(SmoothDashEnd());
        
        // Activar cooldown
        canDash = false;
        dashCooldownTimeLeft = dashCooldown;
    }
    
    // Corrutina para suavizar el final del dash
    private IEnumerator SmoothDashEnd()
    {
        float endDuration = 0.3f;
        float timer = 0f;
        Vector3 initialVelocity = rb.velocity;
        
        while (timer < endDuration)
        {
            timer += Time.deltaTime;
            float t = timer / endDuration;
            
            if (isMoving)
            {
                // Transición suave hacia el movimiento normal
                Vector3 direction = (targetPosition - transform.position).normalized;
                direction.y = 0;
                Vector3 targetVelocity = direction * speed;
                rb.velocity = Vector3.Lerp(initialVelocity, targetVelocity, t);
            }
            else
            {
                // Reducir gradualmente la velocidad hasta detenerse
                rb.velocity = Vector3.Lerp(initialVelocity, Vector3.zero, t);
            }
            
            yield return null;
        }
    }

    void StartHarden()
    {
        isHardened = true;
        hardenTimeLeft = hardenDuration;
        canHarden = false;
        hardenCooldownTimeLeft = hardenCooldown;
        GetComponent<Renderer>().material = hardenedMaterial;
    }

    void EndHarden()
    {
        isHardened = false;
        GetComponent<Renderer>().material = normalMaterial;
    }

    void StartVanish()
    {
        isVanished = true;
        vanishTimeLeft = vanishDuration;
        canVanish = false;
        vanishCooldownTimeLeft = vanishCooldown;
        
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Sphere"), LayerMask.NameToLayer("Sphere"), true);
        
        Color transparent = GetComponent<Renderer>().material.color;
        transparent.a = 0.3f;
        GetComponent<Renderer>().material.color = transparent;
    }

    void EndVanish()
    {
        isVanished = false;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Sphere"), LayerMask.NameToLayer("Sphere"), false);
        
        Color opaque = GetComponent<Renderer>().material.color;
        opaque.a = 1f;
        GetComponent<Renderer>().material.color = opaque;
    }

    void StartGrow()
    {
        isGrown = true;
        growTimeLeft = growDuration;
        canGrow = false;
        growCooldownTimeLeft = growCooldown;
        transform.localScale = originalScale * growScale;
    }

    void EndGrow()
    {
        isGrown = false;
        transform.localScale = originalScale;
    }

    // Funciones para UI
    public float GetDashCooldownRemaining() 
    { 
        // Si puede hacer dash, significa que no hay cooldown
        if (canDash) 
            return 0; 
        else
            return dashCooldownTimeLeft;
    }
    
    public float GetHardenCooldownRemaining() { return hardenCooldownTimeLeft; }
    public float GetVanishCooldownRemaining() { return vanishCooldownTimeLeft; }
    public float GetGrowCooldownRemaining() { return growCooldownTimeLeft; }
}