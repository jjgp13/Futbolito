using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Ensure the GameObject has a Renderer component
public class BallRotationShader : MonoBehaviour
{
    private Material ballMaterial;
    private Rigidbody2D rb;
    private Vector2 accumulatedRotation;

    void Start()
    {
        ballMaterial = GetComponent<Renderer>().material;
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Vector2 velocity = rb.linearVelocity;
        ballMaterial.SetVector("_Velocity", velocity);

        // Optional: Pass accumulated rotation for more control
        accumulatedRotation += velocity * Time.deltaTime;
        ballMaterial.SetVector("_AccumulatedRotation", accumulatedRotation);
    }
}