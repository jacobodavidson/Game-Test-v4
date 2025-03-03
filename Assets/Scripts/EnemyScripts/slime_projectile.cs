using UnityEngine;

public class slime_projectile : MonoBehaviour
{
    public float speed = 5f;
    public int damage = 1;
    public float lifetime = 3f;

    private Vector2 direction;
    public Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0; // No gravity
        rb.linearVelocity = direction * speed; // Set velocity
    }

    public void Initialize(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, lifetime); // Destroy after a set time
    }

 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player_health playerHealth = other.GetComponent<player_health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject); // Destroy projectile on impact
        }
        else if (other.CompareTag("Obstacle"))
        {
            Destroy(gameObject); // Destroy projectile if it hits a wall
        }
    }
}
