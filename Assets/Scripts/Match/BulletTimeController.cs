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

    private void FixedUpdate()
    {
        //Debug.Log(Time.timeScale);
        if (inSlowMotion)
        {
            Time.timeScale += (1f / slowDownTime) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
            if(Time.timeScale >= 1)
            {
                inSlowMotion = false;
                Time.timeScale = 1f;
            }
        }
    }

    public void DoSlowMotion(float timesSlower, float motionTime)
    {
        slowDownFactor = timesSlower;
        slowDownTime = motionTime;
        inSlowMotion = true;
        Time.timeScale = slowDownFactor;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}
