using UnityEngine;

public class FoosballFigureMagnetAction : MonoBehaviour
{
    [SerializeField] private PointEffector2D effector;

    [SerializeField] public float attractionForce = -10f;
    
    public ParticleSystem attractBall;

    private bool isMagnetOff = true;

    // Start is called before the first frame update
    void Start()
    {
        if (!effector)
            // Try to get the PointEffector2D component from children
            effector = GetComponentInChildren<PointEffector2D>();
        MagnetOff();
    }

    public void MagnetOn()
    {
        isMagnetOff = false;
        
        if (effector != null)
        {
            effector.forceMagnitude = attractionForce;
        }
        
        if (attractBall && !attractBall.isPlaying)
            attractBall.Play();
    }

    public void MagnetOff()
    {
        isMagnetOff = true;
        
        if (effector)
            effector.forceMagnitude = 0;
        if (attractBall)
            attractBall.Stop();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball") && !isMagnetOff)
        {
            // Notify the parent FoosballFigureAnimationController
            var controller = GetComponentInParent<FoosballFigureAnimationController>();
            if (controller != null)
            {
                controller.OnBallEnterMagneticField();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Ball"))
        {
            // Notify the parent FoosballFigureAnimationController
            var controller = GetComponentInParent<FoosballFigureAnimationController>();
            if (controller != null)
            {
                controller.OnBallExitMagneticField();
            }
        }
    }

    private void OnDisable()
    {
        MagnetOff();
    }
}
