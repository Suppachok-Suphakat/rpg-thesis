using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillSpawnPoint : MonoBehaviour
{
    [SerializeField] private float offsetY = 1.0f; // Adjust this value based on your desired offset

    void Update()
    {
        MouseFollowWithOffset();
    }

    private void MouseFollowWithOffset()
    {
        // Get the mouse position in world coordinates
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0; // Ensure the z-coordinate is at the same level as the spawn point

        // Calculate the direction from the player to the mouse position
        Vector3 direction = mousePos - PlayerController.instance.transform.position;

        // Set the position of the spawn point with an offset in the direction of the mouse
        transform.position = PlayerController.instance.transform.position + direction.normalized * offsetY;

        // Flip the spawn point horizontally based on the mouse position relative to the player
        if (mousePos.x < PlayerController.instance.transform.position.x)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f); // Flips horizontally
        }
        else
        {
            transform.localScale = new Vector3(1f, 1f, 1f); // Resets scale
        }
    }
}
