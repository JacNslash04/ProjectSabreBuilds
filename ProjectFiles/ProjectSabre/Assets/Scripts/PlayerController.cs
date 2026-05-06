using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public Rigidbody2D rb;
    public float moveSpeed;
    private Vector2 moveDirection;
    public float facingDirection;
 
    [Header("Jumping")]
    public float jumpForce;
    public float gravityScale = 4f;
    public float gravityMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    private bool jumpButtonPressed;
    private bool jumpButtonHeld;
    private float defaultGravityScale;
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Dashing")]
    public float dashVelocity;
    public float dashTime;
    private bool isDashing;
    private bool canDash = true;
    public float dashCooldown;

    [Header("Attacking")]
    [SerializeField] bool isAttacking = false;
    [SerializeField] private float attackCooldown;
    public float timeSinceAttack;
    public Transform forwardAttackPoint;
    public Vector2 forwardAttackHitBox;
    [SerializeField] private LayerMask enemyLayer;
    public float slashDamage = 25f;
    
    [Header("Input References")]
    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference dash;
    public InputActionReference attack;
    public InputActionReference pause;
    public InputActionReference resume;

    [Header("Animation Settings")]
    Animator anim;
    private Vector3 originalScale;

    public int enemyDefeatedCount = 0;
    public TextMeshProUGUI enemyDefeatedText;

    public bool isPaused;
    public GameObject pausePanel;

    // Creating an singleton here to allow other scripts to reference this one.
    public static PlayerController Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        move.action.Enable();
        jump.action.Enable();
        dash.action.Enable();
        attack.action.Enable();
        pause.action.Enable();
        resume.action.Enable();
    }

    private void OnDisable()
    {
        move.action.Disable();
        jump.action.Disable();
        dash.action.Disable();
        attack.action.Disable();
        pause.action.Disable();
        resume.action.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultGravityScale = gravityScale;
        rb.gravityScale = defaultGravityScale;
        anim = GetComponent<Animator>();
        originalScale = transform.localScale;
        setCountText();
        isPaused = false;
        pausePanel.SetActive(false); 
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(forwardAttackPoint.position, forwardAttackHitBox);
    }
    // Update is called once per frame
    private void Update()
    {
        moveDirection = move.action.ReadValue<Vector2>();
        if (moveDirection.x != 0)
        {
            facingDirection = Mathf.Sign(moveDirection.x);
        }

        if (isDashing)
        {
            return;
        }

        if (jump.action.WasPerformedThisFrame())
        {
            jumpButtonPressed = true;
            jumpButtonHeld = true;
        }

        if (jump.action.WasReleasedThisFrame()) 
        {
            jumpButtonHeld = false;
        }

        StartDash();

        if (attack.action.WasPressedThisFrame())
        {
            isAttacking = true;
            Debug.Log("Attacked!");
            Attack();
        }

        anim.SetBool("Jumping", !isGrounded());
        Flip();
        
        if (pause.action.WasPerformedThisFrame() && !isPaused)
        {
            PauseGame();
        }
        else if (resume.action.WasPerformedThisFrame() && isPaused)
        {
            ResumeGame();
        }
    }

    private void FixedUpdate()
    {

        if (isDashing)
        {
            return;
        }

       // rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);

        if (jumpButtonPressed && isGrounded())
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpButtonPressed = false;
        }

        Move();
        AdjustGravity();
    }

    private void Move()
    {
        rb.linearVelocity = new Vector2(moveDirection.x * moveSpeed, rb.linearVelocity.y);
        anim.SetBool("Running", rb.linearVelocity.x != 0 && isGrounded());
    }
    private void AdjustGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = defaultGravityScale * gravityMultiplier;
        }

        else if (rb.linearVelocity.y > 0f && !jumpButtonHeld)
        {
            rb.gravityScale = defaultGravityScale * lowJumpMultiplier;
        }

        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }
   
    public bool isGrounded()
    {
        if(Physics2D.Raycast(groundCheckPos.position, Vector2.down, groundCheckY, groundLayer) || 
            Physics2D.Raycast(groundCheckPos.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer) ||
            Physics2D.Raycast(groundCheckPos.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, groundLayer))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void StartDash()
    {
        if (dash.action.WasPressedThisFrame() && canDash && !isDashing)
        {
            StartCoroutine(Dash());
            isDashing = true;
        }
    }
    IEnumerator Dash()
    {
        Debug.Log("This part's workin!");
        canDash = false;
        isDashing = true;
        anim.SetBool("Dashing", true);
        rb.gravityScale = 0;
        rb.linearVelocity = new Vector2(facingDirection * dashVelocity, 0);
        yield return new WaitForSeconds(dashTime);
        rb.gravityScale = defaultGravityScale;
        isDashing = false;
        anim.SetBool("Dashing", false);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void Attack()
    {
        timeSinceAttack += Time.deltaTime;
        if (isAttacking && timeSinceAttack >= attackCooldown)
        {
            timeSinceAttack = 0;
            anim.SetTrigger("Attacking");

            if (moveDirection.y == 0 || moveDirection.y < 0)
            {
                Hit(forwardAttackPoint, forwardAttackHitBox);
                // Logic is written this way in case I feel like adding more attack direction later on in development
            }
        }
    }

    void Hit(Transform attackLocation, Vector2 attackHitBox) 
    {
        Collider2D[] hitEnemies = Physics2D.OverlapBoxAll(attackLocation.position, attackHitBox, 0, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            Debug.Log("Hit enemy!");
        }

        for(int i = 0; i < hitEnemies.Length; i++)
        {
            if (hitEnemies[i].GetComponent<EnemyController>() != null)
            {
                hitEnemies[i].GetComponent<EnemyController>().TakeDamage(slashDamage, (transform.position - hitEnemies[i].transform.position).normalized, 50);
            }
        }
    }

    void Flip()
    {
        if (facingDirection < 0)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
        }
        else if(facingDirection > 0)
        {
            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
        }
    }

     public void setCountText()
    {
        enemyDefeatedText.text = "Enemies Defeated: " + enemyDefeatedCount.ToString() + "/33";
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
        Cursor.visible = true;
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
        Cursor.visible = false;
    }
}
