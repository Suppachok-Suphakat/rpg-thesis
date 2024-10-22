using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    public LineEffect lineEffect;
    public Transform hero;

    [SerializeField] private WeaponInfo weaponInfo;

    public Transform currentTarget;
    private Transform previousTarget; // Track the previous partner

    private float fusionCooldown = 1f;
    private float lastFusionTime = 0f; // Track the time of the last link/unlink

    private bool isInFusion = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && Time.time >= lastFusionTime + fusionCooldown)
        {
            lastFusionTime = Time.time; // Reset the cooldown timer

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            float detectionRadius = 0.5f;
            Collider2D hit = Physics2D.OverlapCircle(mousePosition, detectionRadius);

            if (hit != null && hit.CompareTag("Hero"))
            {
                Transform newHero = hit.transform;

                if (lineEffect != null)
                {
                    if (currentTarget == newHero)
                    {
                        UnlinkHero(newHero);
                    }
                    else
                    {
                        if (previousTarget != null && previousTarget != newHero)
                        {
                            UnlinkHero(previousTarget);
                        }
                        LinkHero(newHero);
                    }
                }
            }
        }
    }

    private void UnlinkHero(Transform hero)
    {
        if (!isInFusion) return; // Prevent unlinking if not in fusion mode

        lineEffect.StopHealing();

        if (hero.GetComponent<KnightHeroAI>())
        {
            hero.GetComponent<KnightHeroAI>().skillBar.SetActive(false);
            hero.GetComponent<KnightHeroAI>().weaponBar.SetActive(false);
            hero.GetComponent<KnightHeroAI>().knightHeroSkill.DeFusionActivate();
        }
        else if (hero.GetComponent<ArcherHeroAI>())
        {
            hero.GetComponent<ArcherHeroAI>().skillBar.SetActive(false);
            hero.GetComponent<ArcherHeroAI>().weaponBar.SetActive(false);
            hero.GetComponent<ArcherHeroAI>().archerHeroSkill.DeFusionActivate();
        }
        else if (hero.GetComponent<PriestessHeroAI>())
        {
            Debug.Log("Unlink");
            hero.GetComponent<PriestessHeroAI>().skillBar.SetActive(false);
            hero.GetComponent<PriestessHeroAI>().weaponBar.SetActive(false);
            hero.GetComponent<PriestessHeroAI>().priestessHeroSkill.DeFusionActivate();
        }

        currentTarget = null;
        isInFusion = false; // Reset fusion state
    }

    private void LinkHero(Transform newHero)
    {
        if (isInFusion) return; // Prevent linking if already in fusion mode

        if (newHero.GetComponent<KnightHeroAI>())
        {
            SetupLineRenderer(Color.white, Color.blue);
            newHero.GetComponent<KnightHeroAI>().skillBar.SetActive(true);
            newHero.GetComponent<KnightHeroAI>().weaponBar.SetActive(true);
            newHero.GetComponent<KnightHeroAI>().knightHeroSkill.FusionActivate();
        }
        else if (newHero.GetComponent<ArcherHeroAI>())
        {
            SetupLineRenderer(Color.white, Color.green);
            newHero.GetComponent<ArcherHeroAI>().skillBar.SetActive(true);
            newHero.GetComponent<ArcherHeroAI>().weaponBar.SetActive(true);
            newHero.GetComponent<ArcherHeroAI>().archerHeroSkill.FusionActivate();
        }
        else if (newHero.GetComponent<PriestessHeroAI>())
        {
            SetupLineRenderer(Color.white, Color.yellow);
            newHero.GetComponent<PriestessHeroAI>().skillBar.SetActive(true);
            newHero.GetComponent<PriestessHeroAI>().weaponBar.SetActive(true);
            newHero.GetComponent<PriestessHeroAI>().priestessHeroSkill.FusionActivate();
        }

        lineEffect.StartHealing(newHero);
        currentTarget = newHero;
        previousTarget = newHero;
        isInFusion = true; // Set fusion state
    }

    // Method to setup the LineRenderer's color gradient
    private void SetupLineRenderer(Color startColor, Color endColor)
    {
        Gradient gradient = new Gradient();
        GradientColorKey[] colorKey = new GradientColorKey[2];
        colorKey[0].color = startColor;
        colorKey[0].time = 0.0f;
        colorKey[1].color = endColor;
        colorKey[1].time = 1.0f;

        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
        lineEffect.lineRenderer.colorGradient = gradient;
    }
}
