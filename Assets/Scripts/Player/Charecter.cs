using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.SceneManagement;

[Serializable]
public class Stat
{
    public int maxVal;
    public int currVal;

    public Stat(int curr, int max)
    {
        maxVal = max;
        currVal = curr;
    }

    public void Subtract(int amount)
    {
        currVal -= amount;
    }

    public void DeductHP(int amount)
    {
        int damageAfterArmor = Mathf.Max(amount - PlayerStats.instance.armor, 0);
        currVal -= damageAfterArmor;
        //currVal -= (amount - PlayerStats.instance.armor);
    }

    public void Add(int amount)
    {
        currVal += amount;

        if (currVal > maxVal)
        {
            currVal = maxVal;
        }
    }

    public void SetToMax()
    {
        currVal = maxVal;
    }
}

public class Charecter : MonoBehaviour, IDamageable
{
    public static Charecter instance;

    public bool isDead { get; private set; }
    const string TOWN_TEXT = "Map01";
    readonly int DEATH_HASH = Animator.StringToHash("Death");

    public Stat hp;
    [SerializeField] StatusBar hpBar;
    public Stat stamina;
    [SerializeField] StatusBar staminaBar;
    public Stat mana;
    [SerializeField] StatusBar manaBar;


    [SerializeField] private float knockBackThrustAmount = 10f;

    [SerializeField] private float damageRecoveryTime = 1f;

    [SerializeField] private int staminaRecoveryRate = 2;
    [SerializeField] private float staminaRecoveryDelay = 0.1f;
    private float staminaRecoveryTimer;
    public bool isLeftShiftPressed = false;

    [SerializeField] private int staminaReduceRate = 5;
    [SerializeField] private float staminaReduceDelay = 0.1f;
    private float staminaReduceTimer;

    [SerializeField] private int manaReduceRate = 5;
    [SerializeField] private float manaReduceDelay = 0.1f;
    private float manaReduceTimer;

    public bool isExhuasted;


    private bool canTakeDamage = true;
    private Knockback knockback;
    private Flash flash;

    DisableControls disableControls;
    PlayerRespawn playerRespawn;
    PlayerController playerController;

    private void Awake()
    {
        instance = this;

        playerController = GetComponent<PlayerController>();
        flash = GetComponent<Flash>();
        knockback = GetComponent<Knockback>();
        disableControls = GetComponent<DisableControls>();
        playerRespawn = GetComponent<PlayerRespawn>();

    }

    private void Start()
    {
        isDead = false;

        UpdateHpBar();
        UpdateStaminaBar();
        UpdateManaBar();
    }

    private void Update()
    {
        if(stamina.currVal == 0)
        {
            RestoreStamina(1);
        }

        if (!isLeftShiftPressed && !isExhuasted)
        {
            RestoreStaminaOverTime();
        }


        if (Input.GetKeyDown(KeyCode.LeftShift) && stamina.currVal >= 0f && 
            playerController.movement.magnitude > 0f)
        {
            StopCoroutine(StaminaRecovery());
            isLeftShiftPressed = true;
            ReduceStaminaOverTime();
            playerController.moveSpeed *= 2f;

            if (stamina.currVal <= 0f)
            {
                isExhuasted = true;
                playerController.moveSpeed = playerController.startingMoveSpeed;
                StartCoroutine(StaminaRecovery());
            }
        }
        else if(Input.GetKey(KeyCode.LeftShift) && stamina.currVal >= 0f && 
            playerController.movement.magnitude > 0f)
        {
            isLeftShiftPressed = true;
            ReduceStaminaOverTime();

            if (stamina.currVal <= 0f)
            {
                isExhuasted = true;
                playerController.moveSpeed = playerController.startingMoveSpeed;
                StartCoroutine(StaminaRecovery());
            }
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isLeftShiftPressed = false;
            playerController.moveSpeed = playerController.startingMoveSpeed;
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        EnemyAI enemy = other.gameObject.GetComponent<EnemyAI>();
        AttackerEnemyAI attackerEnemyAI = other.gameObject.GetComponent<AttackerEnemyAI>();

        if (enemy || attackerEnemyAI)
        {
            TakeDamage(1, other.transform);
        }
    }

    public void UpdateHpBar()
    {
        hpBar.Set(hp.currVal, hp.maxVal);
    }

    public void UpdateStaminaBar()
    {
        staminaBar.Set(stamina.currVal, stamina.maxVal);
    }

    public void UpdateManaBar()
    {
        manaBar.Set(mana.currVal, mana.maxVal);
    }

    public void TakeDamage(int amount, Transform hitTransform)
    {
        if (!canTakeDamage) { return; }
        knockback.GetKnockedBack(hitTransform, knockBackThrustAmount);
        StartCoroutine(flash.FlashRoutine());
        canTakeDamage = false;

        hp.DeductHP(amount);

        if(hp.currVal <= 0)
        {
            CheckIfPlayerDeath();
        }

        UpdateHpBar();
        StartCoroutine(DamageRecoveryRoutine());
    }

    private void CheckIfPlayerDeath()
    {
        if (hp.currVal <= 0 && !isDead)
        {
            isDead = true;
            Destroy(ActiveWeapon.Instance.gameObject);
            hp.currVal = 0;
            GetComponent<Animator>().SetTrigger(DEATH_HASH);
            StartCoroutine(DeathLoadSceneRoutine());
        }
    }

    private IEnumerator DeathLoadSceneRoutine()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
        SceneManager.LoadScene(TOWN_TEXT);
    }

    public void ReduceHP(int amount)
    {
        hp.Subtract(amount);

        if (hp.currVal <= 0)
        {
            CheckIfPlayerDeath();
        }

        UpdateHpBar();
    }

    public void Heal(int amount)
    {
        hp.Add(amount);
        UpdateHpBar();
    }

    public void FullHeal()
    {
        hp.SetToMax();
        UpdateHpBar();
    }

    public void ReduceStamina(int amount)
    {
        stamina.Subtract(amount);

        if (stamina.currVal <= 0)
        {
            isExhuasted = true;
        }

        UpdateStaminaBar();
    }

    public void ReduceStaminaOverTime()
    {
        if (staminaReduceTimer <= 0f && stamina.currVal >= 0)
        {
            ReduceStamina(staminaReduceRate);

            staminaReduceTimer = staminaReduceDelay;
        }
        else
        {
            staminaReduceTimer -= Time.deltaTime;
        }
    }

    public void RestoreStaminaOverTime()
    {
        if (staminaRecoveryTimer <= 0f && stamina.currVal < stamina.maxVal)
        {
            RestoreStamina(staminaRecoveryRate);

            staminaRecoveryTimer = staminaRecoveryDelay;
        }
        else
        {
            staminaRecoveryTimer -= Time.deltaTime;
        }
    }

    public void RestoreStamina(int amount)
    {
        if (isExhuasted)
        {
            StartCoroutine(StaminaRecovery());
        }

        stamina.Add(amount);
        UpdateStaminaBar();
    }

    public void FullRestoreStamina(int amount)
    {
        stamina.SetToMax();
        UpdateStaminaBar();
    }

    public void ReduceMana(int amount)
    {
        mana.Subtract(amount);

        UpdateManaBar();
    }

    public void ReduceManaOverTime(int manaCost)
    {
        if (manaReduceTimer <= 0f && mana.currVal > 0)
        {
            ReduceMana(manaCost);

            manaReduceTimer = manaReduceDelay;
        }
        else
        {
            manaReduceTimer -= Time.deltaTime;
        }
    }

    public void RestoreMana(int amount)
    {
        mana.Add(amount);
        UpdateManaBar();
    }

    public void FullRestoreMana(int amount)
    {
        mana.SetToMax();
        UpdateManaBar();
    }

    public void CalculateDamage(ref int damage)
    {
        
    }

    public void ApplyDamage(int damage)
    {
        TakeDamage(damage, transform);
    }

    public void CheckState()
    {
        
    }

    private IEnumerator DamageRecoveryRoutine()
    {
        yield return new WaitForSeconds(damageRecoveryTime);
        canTakeDamage = true;
    }

    private IEnumerator StaminaRecovery()
    {
        yield return new WaitForSeconds(2);
        isExhuasted = false;
        RestoreStaminaOverTime();
    }
}
