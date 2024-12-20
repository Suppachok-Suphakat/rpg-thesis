using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LineTrigger : MonoBehaviour
{
    public LineEffect lineEffect;
    public Transform hero;

    [SerializeField] private WeaponInfo weaponInfo;

    public Transform currentTarget;
    private Transform previousTarget;

    private float fusionCooldown = 1f;
    private float lastFusionTime = 0f;

    public bool isInFusion = false;

    public ConversationManager conversationManager;

    public PartnerSkillManager partnerSkillManager;

    private int currentHeroIndex = 0;

    public AudioClip linkSound;
    public AudioClip unlinkSound;

    void Start()
    {
        CursorManager.Instance.OnCursorChanged += Instance_OnCursorChanged;
    }

    void Instance_OnCursorChanged(object sender, CursorManager.OnCursorChangedEventArgs e)
    {
        if (e.cursorType == CursorManager.CursorType.Arrow && isInFusion)
        {
            CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Aim);
        }
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        // Check for middle mouse button click (button 2)
        if (Input.GetMouseButtonDown(2)) // 2 is the button index for the middle mouse button
        {
            SwitchToNextHero();
        }

        if (Input.GetMouseButtonDown(1) && Time.time >= lastFusionTime + fusionCooldown)
        {
            lastFusionTime = Time.time;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            Transform closestHero = FindClosestHero(mousePosition);

            if (closestHero != null)
            {
                if (currentTarget == closestHero)
                {
                    UnlinkHero(closestHero);
                }
                else
                {
                    if (previousTarget != null && previousTarget != closestHero)
                    {
                        UnlinkHero(previousTarget);
                    }
                    LinkHero(closestHero);
                }
            }
        }
    }

    private void SwitchToNextHero()
    {
        if (partnerSkillManager == null || partnerSkillManager.activePartners.Count == 0)
        {
            Debug.LogWarning("No active partners available for switching.");
            return;
        }

        // Find the index of the current target hero
        int currentHeroIndex = -1;
        for (int i = 0; i < partnerSkillManager.partners.Count; i++)
        {
            if (partnerSkillManager.partners[i].partnerObject.transform == currentTarget)
            {
                currentHeroIndex = i;
                break;
            }
        }

        // Determine the next hero index (circular selection)
        int nextHeroIndex = (currentHeroIndex + 1) % partnerSkillManager.activePartners.Count;
        Transform nextHero = partnerSkillManager.partners[partnerSkillManager.activePartners[nextHeroIndex]].partnerObject.transform;

        if (currentTarget != nextHero)
        {
            if (previousTarget != null && previousTarget != nextHero)
            {
                UnlinkHero(previousTarget);
            }
            LinkHero(nextHero);
        }
    }

    private void HandleKeyboardInput()
    {
        if (partnerSkillManager == null || partnerSkillManager.activePartners.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && partnerSkillManager.activePartners.Count >= 1 && Time.time >= lastFusionTime + fusionCooldown)
        {
            SelectHeroByOrder(0);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && partnerSkillManager.activePartners.Count >= 2 && Time.time >= lastFusionTime + fusionCooldown)
        {
            SelectHeroByOrder(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && partnerSkillManager.activePartners.Count >= 3 && Time.time >= lastFusionTime + fusionCooldown)
        {
            SelectHeroByOrder(2);
        }
    }


    private void SelectHeroByOrder(int orderIndex)
    {
        Debug.Log("SelectHeroByOrder called with order index: " + orderIndex);

        if (partnerSkillManager == null || orderIndex >= partnerSkillManager.activePartners.Count)
        {
            Debug.LogWarning("Invalid hero index or PartnerSkillManager is null");
            return;
        }

        int heroIndex = partnerSkillManager.activePartners[orderIndex];
        if (heroIndex >= partnerSkillManager.partners.Count)
        {
            Debug.LogWarning("Hero index out of range in partners list");
            return;
        }

        lastFusionTime = Time.time;
        Transform selectedHero = partnerSkillManager.partners[heroIndex].partnerObject.transform;

        if (currentTarget == selectedHero)
        {
            UnlinkHero(selectedHero);
        }
        else
        {
            if (previousTarget != null && previousTarget != selectedHero)
            {
                UnlinkHero(previousTarget);
            }
            LinkHero(selectedHero);
        }
    }

    private Transform FindClosestHero(Vector3 position)
    {
        float detectionRadius = 1f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, detectionRadius);

        Transform closestHero = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Hero"))
            {
                float distance = Vector3.Distance(position, hit.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestHero = hit.transform;
                }
            }
        }

        return closestHero;
    }

    private void LinkHero(Transform newHero)
    {
        if (isInFusion) return;

        if (newHero.TryGetComponent(out KnightHeroAI knightHero))
        {
            SetupLineRenderer(Color.white, Color.blue);
            SetupHero(knightHero.selectedIndicator, knightHero.skillbutton1, knightHero.skillbutton2);
            knightHero.knightHeroSkill.FusionActivate();
            conversationManager.ShowConversation("Let's do this!", knightHero.heroFaceSprite);
        }
        else if (newHero.TryGetComponent(out ArcherHeroAI archerHero))
        {
            SetupLineRenderer(Color.white, Color.green);
            SetupHero(archerHero.selectedIndicator, archerHero.skillbutton1, archerHero.skillbutton2);
            archerHero.archerHeroSkill.FusionActivate();
            conversationManager.ShowConversation("Ready to strike!", archerHero.heroFaceSprite);
        }
        else if (newHero.TryGetComponent(out PriestessHeroAI priestessHero))
        {
            SetupLineRenderer(Color.white, Color.yellow);
            SetupHero(priestessHero.selectedIndicator, priestessHero.skillbutton1, priestessHero.skillbutton2);
            priestessHero.priestessHeroSkill.FusionActivate();
            conversationManager.ShowConversation("Let the light guide us.", priestessHero.heroFaceSprite);
        }

        SoundManager.instance.RandomizeSfx(linkSound);
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Aim);
        lineEffect.StartHealing(newHero);
        currentTarget = newHero;
        previousTarget = newHero;
        isInFusion = true;
    }

    public void UnlinkHero(Transform hero)
    {
        if (!isInFusion) return;

        lineEffect.StopHealing();

        if (hero.TryGetComponent(out KnightHeroAI knightHero))
        {
            CleanupHero(knightHero.selectedIndicator, knightHero.skillbutton1, knightHero.skillbutton2);
            knightHero.knightHeroSkill.DeFusionActivate();
        }
        else if (hero.TryGetComponent(out ArcherHeroAI archerHero))
        {
            CleanupHero(archerHero.selectedIndicator, archerHero.skillbutton1, archerHero.skillbutton2);
            archerHero.archerHeroSkill.DeFusionActivate();
        }
        else if (hero.TryGetComponent(out PriestessHeroAI priestessHero))
        {
            CleanupHero(priestessHero.selectedIndicator, priestessHero.skillbutton1, priestessHero.skillbutton2);
            priestessHero.priestessHeroSkill.DeFusionActivate();
        }

        SoundManager.instance.RandomizeSfx(unlinkSound);
        currentTarget = null;
        isInFusion = false;
        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Arrow);
    }

    private void SetupHero(GameObject indicator, GameObject skill1, GameObject skill2)
    {
        indicator.SetActive(true);
        skill1.SetActive(true);
        skill2.SetActive(true);
    }

    private void CleanupHero(GameObject indicator, GameObject skill1, GameObject skill2)
    {
        indicator.SetActive(false);
        skill1.SetActive(false);
        skill2.SetActive(false);
    }

    private void SetupLineRenderer(Color startColor, Color endColor)
    {
        if (lineEffect.lineRenderer == null) return;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
            new GradientColorKey(startColor, 0f), // Start color at the beginning of the line
            new GradientColorKey(endColor, 1f)    // End color at the end of the line
            },
            new GradientAlphaKey[] {
            new GradientAlphaKey(1f, 0f), // Full opacity at the start
            new GradientAlphaKey(1f, 1f)  // Full opacity at the end
            }
        );

        lineEffect.lineRenderer.colorGradient = gradient;
    }
}
