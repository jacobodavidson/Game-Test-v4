using UnityEngine;
using UnityEngine.UI;

public class player_movement : MonoBehaviour
{
    // Camera Attributes
    private Camera mainCamera;

    // Player Attributes
    private Rigidbody2D rb;
    public float moveSpeed = 4f;
    private Vector2 moveDirection;
    public Animator anim;

    // Teleportation Range Attributes
    public float teleportRange = 8f; // Fixed teleport range (radius of circle)
    private LineRenderer lineRenderer;
    private int circleResolution = 100; // Smoothness of the circle

    // Teleportation Cooldown Attributes
    private bool isTeleportOnCooldown = false;
    private float teleportCooldownTime = 1f; // Cooldown time in seconds
    private float teleportCooldownTimer = 0f;
    public Slider cooldownSlider;

    // Barrier Layer (Tilemap Colliders & Box Colliders)
    public LayerMask barrierLayer; // Assign in Inspector

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        // Set up LineRenderer
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;
        lineRenderer.positionCount = circleResolution + 1;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.cyan;
        lineRenderer.endColor = Color.cyan;

        // Set sorting layer and order to render on top
        lineRenderer.sortingLayerName = "SolidObjects";
        lineRenderer.sortingOrder = 15;

        DrawTeleportRange();

        // Initialize the cooldown slider
        if (cooldownSlider != null)
        {
            cooldownSlider.maxValue = teleportCooldownTime;
            cooldownSlider.value = teleportCooldownTime;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isTeleportOnCooldown)
        {
            AttemptTeleport();
        }

        // Right-click to attack
        if (Input.GetMouseButtonDown(1) && !isTeleportOnCooldown)
        {
        AttemptAttack();
        }

        MovePlayer();
        anim.SetBool("isMoving", moveDirection != Vector2.zero);

        // Handle teleport cooldown
        if (isTeleportOnCooldown)
        {
            teleportCooldownTimer -= Time.deltaTime;
            if (teleportCooldownTimer <= 0f)
            {
                isTeleportOnCooldown = false;
            }

            // Update the cooldown slider
            if (cooldownSlider != null)
            {
                cooldownSlider.value = teleportCooldownTime - teleportCooldownTimer;
            }
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void AttemptAttack()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            transform.position = hit.collider.transform.position; // Teleport to the slime
            slime_health slimeHealth = hit.collider.GetComponent<slime_health>();
            if (slimeHealth != null)
            {
                slimeHealth.TakeDamage(1); // Deal damage
            }
        }
    }
    void AttemptTeleport()
    {
        // Convert mouse position to world position
        Vector3 targetPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = 0f;

        // Check if clicking on a barrier
        RaycastHit2D hit = Physics2D.Raycast(targetPosition, Vector2.zero, Mathf.Infinity, barrierLayer);

        if (hit.collider != null)
        {
            Debug.Log("Teleport blocked: Clicked on a barrier!");
            return; // Block teleport if hitting a barrier
        }

        // Check if within teleport range
        if (Vector3.Distance(transform.position, targetPosition) <= teleportRange)
        {
            TeleportPlayer(targetPosition);
        }
    }

    void TeleportPlayer(Vector3 targetPosition)
    {
        transform.position = targetPosition;
        DrawTeleportRange(); // Update teleport range circle

        // Start cooldown
        isTeleportOnCooldown = true;
        teleportCooldownTimer = teleportCooldownTime;

        // Update the cooldown slider
        if (cooldownSlider != null)
        {
            cooldownSlider.value = 0f;
        }
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;
        anim.SetFloat("Horizontal", moveDirection.x);
        anim.SetFloat("Vertical", moveDirection.y);

        DrawTeleportRange(); // Update teleport range while moving
    }

    void DrawTeleportRange()
    {
        // Draws a circular range around the player
        for (int i = 0; i <= circleResolution; i++)
        {
            float angle = i * 2 * Mathf.PI / circleResolution;
            float x = transform.position.x + teleportRange * Mathf.Cos(angle);
            float y = transform.position.y + teleportRange * Mathf.Sin(angle);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }
}
