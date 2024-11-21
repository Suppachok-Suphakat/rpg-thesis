using System.Collections;
using System.Collections.Generic;
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

    void Start()
    {
        CursorManager.Instance.OnCursorChanged += Instance_OnCursorChanged;
    }

    void Instance_OnCursorChanged(object sender, CursorManager.OnCursorChangedEventArgs e)
    {
        if(e.cursorType == CursorManager.CursorType.Arrow)
        {
            if (isInFusion)
            {
                CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Archer);
            }
        }
    }

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(1) && Time.time >= lastFusionTime + fusionCooldown)
        {
            lastFusionTime = Time.time;
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f;

            float detectionRadius = 1f; // Increase radius to allow easier detection
            Collider2D[] hits = Physics2D.OverlapCircleAll(mousePosition, detectionRadius);

            // Check for the closest hero in detected colliders
            Transform closestHero = null;
            float closestDistance = float.MaxValue;

            foreach (Collider2D hit in hits)
            {
                if (hit.CompareTag("Hero"))
                {
                    float distance = Vector3.Distance(mousePosition, hit.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestHero = hit.transform;
                    }
                }
            }

            // If no hero is found, fall back to raycast precision
            if (closestHero == null)
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
                if (hit.collider != null && hit.collider.CompareTag("Hero"))
                {
                    closestHero = hit.transform;
                }
            }

            // Handle linking or unlinking hero
            if (closestHero != null && lineEffect != null)
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

    public void UnlinkHero(Transform hero)
    {
        if (!isInFusion) return;

        lineEffect.StopHealing();

        if (hero.TryGetComponent(out KnightHeroAI knightHero))
        {
            knightHero.selectedIndicator.SetActive(false);
            knightHero.skillbutton1.SetActive(false);
            knightHero.skillbutton2.SetActive(false);
            knightHero.knightHeroSkill.DeFusionActivate();
        }
        else if (hero.TryGetComponent(out ArcherHeroAI archerHero))
        {
            archerHero.selectedIndicator.SetActive(false);
            archerHero.skillbutton1.SetActive(false);
            archerHero.skillbutton2.SetActive(false);
            archerHero.archerHeroSkill.DeFusionActivate();
        }
        else if (hero.TryGetComponent(out PriestessHeroAI priestessHero))
        {
            priestessHero.selectedIndicator.SetActive(false);
            priestessHero.skillbutton1.SetActive(false);
            priestessHero.skillbutton2.SetActive(false);
            priestessHero.priestessHeroSkill.DeFusionActivate();
        }

        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Arrow);
        currentTarget = null;
        isInFusion = false;
    }

    private void LinkHero(Transform newHero)
    {
        if (isInFusion) return;

        if (newHero.TryGetComponent(out KnightHeroAI knightHero))
        {
            SetupLineRenderer(Color.white, Color.blue);
            knightHero.selectedIndicator.SetActive(true);
            knightHero.skillbutton1.SetActive(true);
            knightHero.skillbutton2.SetActive(true);
            knightHero.knightHeroSkill.FusionActivate();
            conversationManager.ShowConversation("Let's do this!", knightHero.heroFaceSprite);
        }
        else if (newHero.TryGetComponent(out ArcherHeroAI archerHero))
        {
            SetupLineRenderer(Color.white, Color.green);
            archerHero.selectedIndicator.SetActive(true);
            archerHero.skillbutton1.SetActive(true);
            archerHero.skillbutton2.SetActive(true);
            archerHero.archerHeroSkill.FusionActivate();
            conversationManager.ShowConversation("Ready to strike!", archerHero.heroFaceSprite);
        }
        else if (newHero.TryGetComponent(out PriestessHeroAI priestessHero))
        {
            SetupLineRenderer(Color.white, Color.yellow);
            priestessHero.selectedIndicator.SetActive(true);
            priestessHero.skillbutton1.SetActive(true);
            priestessHero.skillbutton2.SetActive(true);
            priestessHero.priestessHeroSkill.FusionActivate();
            conversationManager.ShowConversation("Let the light guide us.", priestessHero.heroFaceSprite);
        }

        CursorManager.Instance.SetActiveCursorType(CursorManager.CursorType.Archer);
        lineEffect.StartHealing(newHero);
        currentTarget = newHero;
        previousTarget = newHero;
        isInFusion = true;
    }

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
