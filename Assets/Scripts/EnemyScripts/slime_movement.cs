using UnityEngine;
using System.Collections;

public class slime_movement : MonoBehaviour
{
    // Movement Attributes
    public float moveSpeed = 3f;
    public float changeDirectionTime = 2f;
    private Rigidbody2D rb;
    private Vector2 movementDirection;

    // Damage Attributes
    public int damageAmount = 1;

    // Projectile Attributes
    public float attackRange = 4f;
    public float fireRate = 2f;
    public GameObject slime_projectile;  // Assign the projectile prefab
    public Transform firePoint; // projectile spawns
    private bool canShoot = true;

    // Aggro Attributes
    public float chaseSpeed = 5f;
    public float detectionRange = 8f;
    public float stopChaseRange = 10f;
    private Transform player;
    private bool isChasing = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(ChangeDirectionRoutine());

        // Find the player by tag
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FixedUpdate()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position,
                player.position);

            if (distanceToPlayer <= detectionRange)
                isChasing = true;
            else if (distanceToPlayer >= stopChaseRange)
                isChasing = false;

            if (isChasing && distanceToPlayer > attackRange)
            {
                ChasePlayer();
            }
            else if (distanceToPlayer <= attackRange && canShoot)
            {
                StartCoroutine(ShootAtPlayer());
            }
            else
            {
                rb.linearVelocity = movementDirection * moveSpeed;
            }
        }
    }

    IEnumerator ChangeDirectionRoutine()
    {
        while (true)
        {
            if (!isChasing) // Only change direction if NOT chasing
            {
                ChangeDirection();
            }
            yield return new WaitForSeconds(changeDirectionTime);
        }
    }

    void ChangeDirection()
    {
        float x = Random.Range(-1f, 1f);
        float y = Random.Range(-1f, 1f);
        movementDirection = new Vector2(x, y).normalized;
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;
    }

    IEnumerator ShootAtPlayer()
    {
        canShoot = false;
        rb.linearVelocity = Vector2.zero; // Stop moving while attacking

        if (player != null)
        {
            Vector2 direction = (player.position -
                transform.position).normalized;
            GameObject projectile = Instantiate(slime_projectile,
                firePoint.position, Quaternion.identity);
            projectile.GetComponent<slime_projectile>().Initialize(direction);
        }

        yield return new WaitForSeconds(fireRate);
        canShoot = true;
    }

    // Damage the player on contact
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player_health playerHealth = other.GetComponent<player_health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
            }
        }
    }
}
