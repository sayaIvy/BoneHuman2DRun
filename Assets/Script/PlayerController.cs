using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5.0f;

    // ジャンプ用
    public float jumpForce = 300f;
    public float smallJumpMultiplier = 0.5f;


    public float slideDuration = 0.5f;

    // レイキャスト処理用
    public LayerMask wallLayer;
    public float wallCheckDistance = 0.5f;
    public Vector2 footOffset;


    private Rigidbody2D rb;
    private BoxCollider2D boxCol;

    private bool isJumping = false;

    // スライド用
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    private bool isGrounded = true;

    // レイキャスト処理用
    private float horizontalInput;
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCol = GetComponent<BoxCollider2D>();
        originalColliderSize = boxCol.size;
        originalColliderOffset = boxCol.offset;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void FixedUpdate()
    {
        // 壁検出レイキャスト
        Vector2 footPosition = (Vector2)transform.position + footOffset;
        RaycastHit2D hit = Physics2D.Raycast(footPosition, isFacingRight ? Vector2.right : Vector2.left, wallCheckDistance, wallLayer);

        if (hit.collider == null || (isFacingRight && horizontalInput < 0) || (!isFacingRight && horizontalInput > 0))
        {
            rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }
        }
    }

    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            isJumping = true;
            Jump();
        }
        if (isJumping && rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            isJumping = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * smallJumpMultiplier);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            StartCoroutine(Slide());
        }

        Debug.Log(isGrounded);
    }

    void Flip()
    {
        // キャラクターの向きを反転
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Jump()
    {
        rb.AddForce(new Vector2(0, jumpForce));
    }

    IEnumerator Slide()
    {
        // スライド中はコライダーのサイズを半分にする
        boxCol.size = new Vector2(boxCol.size.x, originalColliderSize.y / 2);
        boxCol.offset = new Vector2(boxCol.offset.x, originalColliderOffset.y - originalColliderSize.y / 4);
        
        yield return new WaitForSeconds(slideDuration);

        // スライド終了後は元のサイズに戻す
        boxCol.size = originalColliderSize;
        boxCol.offset = originalColliderOffset;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            isJumping = false;
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }

    void OnDrawGizmos()
    {
        Vector2 footPosition = (Vector2)transform.position + footOffset;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(footPosition, footPosition + (isFacingRight ? Vector2.right : Vector2.left) * wallCheckDistance);
    }
}
