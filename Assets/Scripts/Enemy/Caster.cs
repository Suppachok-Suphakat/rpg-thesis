using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caster : MonoBehaviour, IEnemy
{
    [SerializeField] private GameObject magicPrefab;
    [SerializeField] private float restTime = 1f;
    [SerializeField] private float animationDelay = 0.5f; // Delay before shooting for animation sync

    private bool isShooting = false;

    public void Attack()
    {
        if (!isShooting)
        {
            if (!isShooting)
            {
                StartCoroutine(MagicRoutine());
            }
        }
    }

    private void FlipSprite(Transform flipTo)
    {
        if (flipTo.position.x < transform.position.x)
            transform.localScale = new Vector3(1, 1, 1);
        else
            transform.localScale = new Vector3(-1, 1, 1);
    }

    private IEnumerator MagicRoutine()
    {
        isShooting = true;

        // Flip to face the player at the start of shooting
        FlipSprite(PlayerController.instance.transform);

        yield return new WaitForSeconds(animationDelay); // Add delay for animation

        GameObject newMagic = Instantiate(magicPrefab, PlayerController.instance.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(restTime);
        isShooting = false;
    }
}
