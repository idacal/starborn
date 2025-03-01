using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float borderThickness = 10f;
    public float panSpeed = 20f;
    
    // Límites del movimiento de la cámara
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    // Variables de zoom
    public float zoomSpeed = 5f;
    public float minY = 10f;
    public float maxY = 50f;
    public float smoothSpeed = 5f;

    // Referencia a la esfera del jugador
    public GameObject playerSphere;

    // Variable para almacenar la altura objetivo
    private float targetY;

    // Variables para control de centrado
    private bool justCentered = false;
    private float centerCooldown = 0.1f;
    private float centerTimer = 0f;

    void Start()
    {
        // Inicializar la altura objetivo con la posición actual
        targetY = transform.position.y;
        
        // Buscar la esfera del jugador si no está asignada
        if (playerSphere == null)
        {
            playerSphere = GameObject.FindGameObjectWithTag("Player");
            if (playerSphere == null)
            {
                Debug.LogError("No se encontró un GameObject con el tag 'Player'. La función de centrado no funcionará.");
            }
            else
            {
                Debug.Log("Jugador encontrado automáticamente: " + playerSphere.name);
            }
        }
        else
        {
            Debug.Log("Jugador asignado manualmente: " + playerSphere.name);
        }
    }

    void LateUpdate()  // Cambiado a LateUpdate para asegurar que se ejecute después de otros scripts
    {
        // Manejamos el timer para el período de enfriamiento del centrado
        if (justCentered)
        {
            centerTimer += Time.deltaTime;
            if (centerTimer >= centerCooldown)
            {
                justCentered = false;
                centerTimer = 0f;
            }
            return; // Salimos temprano si acabamos de centrar, evitando cualquier otro movimiento
        }

        // Comprobar si se pulsa la tecla C para centrar en el jugador
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Tecla C presionada - Intentando centrar cámara");
            CenterOnPlayer();
            return; // Salimos inmediatamente después de centrar
        }
        
        // El resto del control de cámara regular solo se ejecuta si no estamos en enfriamiento
        Vector3 pos = transform.position;
        
        // Obtener la posición del mouse
        Vector2 mousePosition = Input.mousePosition;

        // Mover cámara basado en la posición del mouse
        // Derecha
        if (mousePosition.x >= Screen.width - borderThickness)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        // Izquierda
        if (mousePosition.x <= borderThickness)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }
        // Arriba
        if (mousePosition.y >= Screen.height - borderThickness)
        {
            pos.z += panSpeed * Time.deltaTime;
        }
        // Abajo
        if (mousePosition.y <= borderThickness)
        {
            pos.z -= panSpeed * Time.deltaTime;
        }

        // Actualizar la altura objetivo con el scroll del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            targetY -= scroll * zoomSpeed;
            targetY = Mathf.Clamp(targetY, minY, maxY);
        }

        // Aplicar smooth al movimiento vertical
        pos.y = Mathf.Lerp(pos.y, targetY, smoothSpeed * Time.deltaTime);

        // Limitar el movimiento de la cámara
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        // Aplicar la nueva posición
        transform.position = pos;
    }
    
    // Método para centrar la cámara sobre el jugador
    public void CenterOnPlayer()
    {
        // Verificar nuevamente si tenemos referencia al jugador
        if (playerSphere == null)
        {
            playerSphere = GameObject.FindGameObjectWithTag("Player");
            if (playerSphere == null)
            {
                Debug.LogError("No hay jugador para centrar la cámara. Asegúrate de que existe un objeto con tag 'Player'.");
                return;
            }
        }
        
        // Posición actual y del jugador para debug
        Debug.Log($"Posición actual de la cámara: {transform.position}");
        Debug.Log($"Posición del jugador: {playerSphere.transform.position}");
        
        // Crear la nueva posición centrada
        Vector3 newPosition = new Vector3(
            playerSphere.transform.position.x,
            transform.position.y,
            playerSphere.transform.position.z
        );
        
        // Aplicar la nueva posición inmediatamente
        transform.position = newPosition;
        
        // Activar el período de enfriamiento
        justCentered = true;
        centerTimer = 0f;
        
        Debug.Log($"Nueva posición de la cámara: {transform.position}");
    }
}