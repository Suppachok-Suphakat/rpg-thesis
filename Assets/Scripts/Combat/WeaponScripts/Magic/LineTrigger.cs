using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    public LineEffect lineEffect;
    public Transform partner; // Assign the partner character here

    [SerializeField] private WeaponInfo weaponInfo;

    public Transform currentTarget;
    private Transform previousTarget; // Track the previous partner

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

                        if (hit.gameObject.GetComponent<KnightPartner>())
                        {
                            hit.gameObject.GetComponent<KnightPartner>().skillBar.SetActive(false);
                        }
                        else if (hit.gameObject.GetComponent<ArcherPartnerAI>())
                        {
                            hit.gameObject.GetComponent<ArcherPartnerAI>().skillBar.SetActive(false);
                        }

                        currentTarget = null;
                    }
                    else
                    {
                        // Hide the skill bar of the previous partner, if any
                        if (previousTarget != null && previousTarget != newPartner)
                        {
                            KnightPartner previousKnightPartner = previousTarget.GetComponent<KnightPartner>();
                            ArcherPartnerAI previousArcherPartner = previousTarget.GetComponent<ArcherPartnerAI>();
                            if (previousKnightPartner != null)
                            {
                                previousKnightPartner.skillBar.SetActive(false);
                            }
                            else if(previousArcherPartner != null)
                            {
                                previousArcherPartner.skillBar.SetActive(false);
                            }
                        }

                        // Start a new line effect for the new partner
                        if (currentTarget != null)
                        {
                            // Optionally, stop the previous line effect
                            lineEffect.StopHealing();
                        }

                        // Show the skill bar of the new partner
                        if (hit.gameObject.GetComponent<KnightPartner>())
                        {
                            // Create a new Gradient
                            Gradient gradient = new Gradient();

                            // Set up the color keys (color and time)
                            GradientColorKey[] colorKey = new GradientColorKey[2];
                            colorKey[0].color = Color.white;  // Start color
                            colorKey[0].time = 0.0f;
                            colorKey[1].color = Color.blue;   // End color
                            colorKey[1].time = 1.0f;

                            // Set up the alpha keys (alpha and time)
                            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
                            alphaKey[0].alpha = 1.0f;  // Full opacity at start
                            alphaKey[0].time = 0.0f;
                            alphaKey[1].alpha = 1.0f;  // Full opacity at end
                            alphaKey[1].time = 1.0f;

                            // Assign the color keys and alpha keys to the gradient
                            gradient.SetKeys(colorKey, alphaKey);

                            // Assign the gradient to the LineRenderer's color gradient
                            lineEffect.lineRenderer.colorGradient = gradient;

                            hit.gameObject.GetComponent<KnightPartner>().skillBar.SetActive(true);
                        }
                        else if (hit.gameObject.GetComponent<ArcherPartnerAI>())
                        {
                            // Create a new Gradient
                            Gradient gradient = new Gradient();

                            // Set up the color keys (color and time)
                            GradientColorKey[] colorKey = new GradientColorKey[2];
                            colorKey[0].color = Color.white;  // Start color
                            colorKey[0].time = 0.0f;
                            colorKey[1].color = Color.green;   // End color
                            colorKey[1].time = 1.0f;

                            // Set up the alpha keys (alpha and time)
                            GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
                            alphaKey[0].alpha = 1.0f;  // Full opacity at start
                            alphaKey[0].time = 0.0f;
                            alphaKey[1].alpha = 1.0f;  // Full opacity at end
                            alphaKey[1].time = 1.0f;

                            // Assign the color keys and alpha keys to the gradient
                            gradient.SetKeys(colorKey, alphaKey);

                            // Assign the gradient to the LineRenderer's color gradient
                            lineEffect.lineRenderer.colorGradient = gradient;

                            hit.gameObject.GetComponent<ArcherPartnerAI>().skillBar.SetActive(true);
                        }

                        lineEffect.StartHealing(newPartner);
                        currentTarget = newPartner;
                        previousTarget = newPartner; // Update the previous target
                    }
                }
            }
        }
    }
}