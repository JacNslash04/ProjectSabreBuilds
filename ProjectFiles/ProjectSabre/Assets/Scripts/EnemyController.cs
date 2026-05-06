using UnityEngine;

public class EnemyController : MonoBehaviour
{

    [SerializeField] protected float enemyHealth = 100f;
    //[SerializeField] private float enemyMaxHealth = 100f;
    [SerializeField] protected float recoilLength;
    [SerializeField] protected float recoilStrength;
    [SerializeField] protected bool isRecoiling = false;
    [SerializeField] protected float recoilTimer;
    protected Rigidbody2D rb;

    [SerializeField] protected PlayerController player;
    [SerializeField] protected float enemyMoveSpeed;

    public Transform enemyDetectionCenter;
    public bool isFollowing = false;
    public Transform playerTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {

    }
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = PlayerController.Instance; 
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (player == null)
        {
            player = PlayerController.Instance;
            if (player == null) return;
        }

        if (enemyHealth <= 0)
        {
            Destroy(gameObject);
            player.enemyDefeatedCount++;
            player.setCountText();
        }

        if (isRecoiling)
        {
            if (recoilTimer < recoilLength)
            {
                recoilTimer += Time.deltaTime;
            }
            else
            {
                isRecoiling = false;
                recoilTimer = 0f;
            }
        }

        isFollowing = false;

        Collider2D[] perceptionZone = Physics2D.OverlapCircleAll(enemyDetectionCenter.position, 8.0f);
        if (perceptionZone.Length > 0)
        {
            foreach (Collider2D collider in perceptionZone)
            {
                if (collider.CompareTag("Player"))
                {
                    Debug.Log("Someone's here!");
                    isFollowing = true;
                    playerTransform = collider.transform;
                }
            }
        }

        if (isFollowing)
        {
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            rb.linearVelocity = directionToPlayer * enemyMoveSpeed;
        }
    }

    public void TakeDamage(float damageTaken, Vector2 _hitDirection, float _hitForce)
    {
        enemyHealth -= damageTaken;
        if (!isRecoiling) 
        {
            rb.AddForce(-_hitForce * recoilStrength * _hitDirection);   
        }
    }

    void OnDrawGizmos()
    { 
       Gizmos.DrawWireSphere(enemyDetectionCenter.position, 8.0f);
    }
}
