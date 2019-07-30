using UnityEngine;

public class BulletTimeController : MonoBehaviour
{
    public static BulletTimeController instance;

    
    public float slowDownFactor;
    public float slowDownTime;
    public bool inSlowMotion = false;


    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (inSlowMotion)
        {
            Time.timeScale += (1f / slowDownTime) * Time.unscaledDeltaTime;
            if(Time.timeScale > 1)
            {
                inSlowMotion = false;
                Time.timeScale = 1f;
            }
        }
    }

    public void DoSlowMotion()
    {
        inSlowMotion = true;
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
