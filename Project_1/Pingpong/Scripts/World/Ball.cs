using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Refs")]
    public Rigidbody2D rb2d;
    public SpriteRenderer spriteRenderer;
    public BallAudio ballAudio;
    public ParticleSystem collisionParticle;

    [Header("Config")]
    [Range(0f, 1f)]
    public float maxInitialAngle = 0.67f;
    [Tooltip("The maximum ball angle after colliding with a paddle")]
    public float maxCollisionAngle = 45f;
    [Space]
    public float moveSpeed = 1f;
    public float maxStartY = 4f;
    [SerializeField]
    private float speedMultiplier = 1.1f;

    private float startX = 0f;

    private void Start()
    {
        GameManager.instance.onReset += ResetBall;
        GameManager.instance.gameUI.onStartGame += ResetBall;
    }

    private void ResetBall()
    {
        ResetBallPosition();
        InitialPush();
    }

    private void InitialPush()
    {
        Vector2 dir = Random.value < 0.5f ? Vector2.left : Vector2.right;

        dir.y = Random.Range(-maxInitialAngle, maxInitialAngle);
        rb2d.linearVelocity = dir * moveSpeed;

        EmitParticle(32);
    }

    private void ResetBallPosition()
    {
        float posY = Random.Range(-maxStartY, maxStartY);
        Vector2 position = new Vector2(startX, posY);
        transform.position = position;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ScoreZone scoreZone = collision.GetComponent<ScoreZone>();
        if(scoreZone)
        {
            GameManager.instance.OnScoreZoneReached(scoreZone.id);
            GameManager.instance.screenshake.StartShake(0.33f, 0.1f);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Paddle paddle = collision.collider.GetComponent<Paddle>();
        if(paddle)
        {
            ballAudio.PlayPaddleSound();
            rb2d.linearVelocity *= speedMultiplier;
            EmitParticle(16);
            AdjustAngle(paddle, collision);
            GameManager.instance.screenshake.StartShake(Mathf.Sqrt(rb2d.linearVelocity.magnitude) * 0.02f, 0.075f);
        }

        Wall wall = collision.collider.GetComponent<Wall>();
        if (wall)
        {
            ballAudio.PlayWallSound();
            EmitParticle(8);
            GameManager.instance.screenshake.StartShake(0.033f, 0.033f);
        }

        AdjustSpriteRotation();
    }

    private void AdjustAngle(Paddle paddle, Collision2D collision)
    {
        
        Vector2 median = Vector2.zero;
        foreach (ContactPoint2D point in collision.contacts)
        {
            median += point.point;
           
        }
        median /= collision.contactCount;
       

        
        float absoluteDistanceFromCenter = median.y - paddle.transform.position.y;
        float relativeDistanceFromCenter = absoluteDistanceFromCenter * 2 / paddle.GetHeight();
       

        // calc rotn using quaternion
        int angleSign = paddle.IsLeftPaddle() ? 1 : -1;
        float angle = relativeDistanceFromCenter * maxCollisionAngle * angleSign;
        Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
        Debug.DrawRay(median, Vector3.forward, Color.yellow, 1f);

        // calc dirn / velocity
        Vector2 dir = paddle.IsLeftPaddle() ? Vector2.right : Vector2.left;
        Vector2 velocity = rot * dir * rb2d.linearVelocity.magnitude;
        rb2d.linearVelocity = velocity;
        Debug.DrawRay(median, velocity, Color.green, 1f);
    }

    private void AdjustSpriteRotation()
    {
        spriteRenderer.flipY = rb2d.linearVelocity.x < 0f;
    }

    private void EmitParticle(int amount)
    {
        collisionParticle.Emit(amount);
    }
}
