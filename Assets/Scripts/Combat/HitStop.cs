using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    bool waiting;

    public void Stop(float duration)
    {
        if (waiting)
            return;
        StartCoroutine(StopEnemyForDuration(duration));
    }

    IEnumerator StopEnemyForDuration(float duration)
    {
        waiting = true;

        // Store the current time scale (for restoring after the hit stop)
        float originalTimeScale = Time.timeScale;

        // Freeze the time only for the enemy, using Time.timeScale for global time freezing
        Time.timeScale = 0.0f;

        // Run through a custom time step without affecting the whole world
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;  // Use unscaled time to avoid affecting other systems
            yield return null;
        }

        // Restore normal time scale
        Time.timeScale = originalTimeScale;

        waiting = false;
    }
}
