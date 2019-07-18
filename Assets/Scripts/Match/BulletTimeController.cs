using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTimeController : MonoBehaviour
{
    public static BulletTimeController instance;

    //Variables to hanlde bullet time
    [Header("Variables for bullet time")]
    public float slowDownTime;
    public float timesSlower;
    public bool bulletTime;
    private float bulletTimeTimer;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        bulletTime = false;
        bulletTimeTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (bulletTime)
        {
            bulletTimeTimer += Time.unscaledDeltaTime;
            if (bulletTimeTimer > slowDownTime)
            {
                Time.timeScale = 1f;
                bulletTime = false;
                bulletTimeTimer = 0;
            }
        }
    }

    //Animation for bullet time hit.
    //Fix
    public void PlayBulletTimeAnimation(Vector2 pos)
    {
        Time.timeScale = 0.02f;
        bulletTime = true;
    }

    public IEnumerator BallHittedEffect()
    {
        Time.timeScale = 0.5f;
        yield return new WaitForSecondsRealtime(0.1f);
        Time.timeScale = 1;
    }
}
