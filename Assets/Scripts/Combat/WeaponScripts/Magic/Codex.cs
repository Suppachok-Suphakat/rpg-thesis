using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Codex : MonoBehaviour, IWeapon
{
    [SerializeField] private WeaponInfo weaponInfo;
    [SerializeField] private GameObject magic;
    private GameObject magicCircleInstance;
    [SerializeField] private Transform magicLaserSpawnPoint;
    [SerializeField] private int manaCost;

    [SerializeField] private int codexCooldown;

    private Animator myAnimator;
    private Charecter charecter;

    private void Awake()
    {
        myAnimator = GetComponent<Animator>();
        charecter = FindObjectOfType<Charecter>();
    }

    private void Update()
    {
        MouseFollowWithOffset();

        if(Charecter.instance.mana.currVal > 0)
        {
            if (Input.GetMouseButtonDown(0)) // Check for mouse button down
            {
                // Spawn the magic circle prefab at the player's position
                SpawnMagicCircle();
            }
            else if (Input.GetMouseButton(0)) // Check if the left mouse button is being held down
            {
                // Update the position of the magic circle to follow the player
                UpdateMagicCirclePosition();
                charecter.ReduceManaOverTime(manaCost);
            }
            else if (Input.GetMouseButtonUp(0) && magicCircleInstance != null) // Check for mouse button release
            {
                // Destroy the magic circle instance when the left mouse button is released
                DestroyMagicCircle();
            }
        }
        else
        {
            DestroyMagicCircle();
        }
    }

    public void Attack()
    {
        
    }

    //private IEnumerator ReduceManaRoutine()
    //{
    //    charecter.ReduceMana(manaCost);
    //    charecter.ReduceManaOverTime(int manaCost);
    //    yield return new WaitForSeconds(codexCooldown);
    //}

    void SpawnMagicCircle()
    {
        magicCircleInstance = Instantiate(magic, PlayerController.instance.transform.position, Quaternion.identity);
    }

    void UpdateMagicCirclePosition()
    {
        // Update the position of the magic circle to follow the player
        if (magicCircleInstance != null && PlayerController.instance != null)
        {
            Vector3 playerPosition = PlayerController.instance.transform.position;
            playerPosition.z = 0f; // Ensure the circle appears at the same depth as the player

            magicCircleInstance.transform.position = playerPosition;
        }
    }

    void DestroyMagicCircle()
    {
        // Destroy the magic circle instance when the left mouse button is released
        Destroy(magicCircleInstance);
    }

    public WeaponInfo GetWeaponInfo()
    {
        return weaponInfo;
    }

    private void MouseFollowWithOffset()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 playerScreenPoint = Camera.main.WorldToScreenPoint(PlayerController.instance.transform.position);

        float angle = Mathf.Atan2(mousePos.y, mousePos.x) * Mathf.Rad2Deg;

        if (mousePos.x < playerScreenPoint.x)
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, -180, angle);
        }
        else
        {
            ActiveWeapon.Instance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }
}
