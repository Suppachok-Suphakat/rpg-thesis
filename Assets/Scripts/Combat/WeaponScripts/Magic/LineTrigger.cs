using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    public LineEffect lineEffect;
    public Transform partner; // Assign the partner character here

    [SerializeField] private WeaponInfo weaponInfo;

    public Transform currentTarget;

    private void Awake()
    {
        // Optionally, initialize here
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Change this to your desired input key
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            float detectionRadius = 0.5f; // Adjust this radius as needed
            Collider2D hit = Physics2D.OverlapCircle(mousePosition, detectionRadius);

            if (hit != null && hit.CompareTag("Partner"))
            {
                Transform newPartner = hit.transform;

                if (lineEffect != null)
                {
                    if (currentTarget == newPartner)
                    {
                        // Cancel the line if the same partner is clicked
                        lineEffect.StopHealing();
                        currentTarget = null;
                    }
                    else
                    {
                        // Start a new line effect for the new partner
                        if (currentTarget != null)
                        {
                            // Optionally, stop the previous line effect
                            lineEffect.StopHealing();
                        }

                        lineEffect.StartHealing(newPartner);
                        currentTarget = newPartner;
                    }
                }
            }
        }
    }
}