using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ScreenShakeManager : Singleton<ScreenShakeManager>
{
    [SerializeField]
    private CinemachineImpulseSource playerHitShakeSource; // For when the player is hit
    [SerializeField]
    private CinemachineImpulseSource enemyHitShakeSource; // For when the player hits an enemy

    protected override void Awake()
    {
        base.Awake();

        // Ensure the sources are assigned or throw an error
        if (!playerHitShakeSource || !enemyHitShakeSource)
        {
            Debug.LogError("Cinemachine Impulse Sources are not assigned in ScreenShakeManager!");
        }
    }

    public void ShakeOnPlayer()
    {
        if (playerHitShakeSource)
        {
            playerHitShakeSource.GenerateImpulse();
        }
    }

    public void ShakeOnEnemy()
    {
        if (enemyHitShakeSource)
        {
            enemyHitShakeSource.GenerateImpulse();
        }
    }
}
