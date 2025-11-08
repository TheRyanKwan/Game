// Assets/Scripts/Enemy/BaseEnemy.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("基礎屬性")]
    [SerializeField] protected float maxHP = 50f;
    protected float currentHP;

    [Header("移動")]
    [SerializeField] protected float moveSpeed = 5f;
    [SerializeField] protected float stoppingDistance = 2f;

    [Header("偵測")]
    [SerializeField] protected float sightRange = 20f;
    [SerializeField] protected float fieldOfViewAngle = 90f;
    [SerializeField] protected LayerMask playerLayer;
    [SerializeField] protected LayerMask wallLayer;           // 普通墻
    [SerializeField] protected LayerMask invisibleWallLayer;  // 隱形墻

    [Header("走位逃亡 (方案B)")]
    [SerializeField] protected float evadeRangeMin = 10f;
    [SerializeField] protected float evadeRangeMax = 20f;
    [SerializeField] protected float evadeDurationMin = 2f;   // N: 走位時長最小值
    [SerializeField] protected float evadeDurationMax = 5f;   // M: 走位時長最大值
    [SerializeField] protected float directionChangeInterval = 1f;  // 每秒檢查一次改變方向的概率
    [SerializeField] protected float initialDirectionChangeProbability = 0.1f;  // 初始概率 (初期低)
    [SerializeField] protected float maxDirectionChangeProbability = 0.8f;      // 最大概率 (後期高)

    [Header("重生設定")]
    [SerializeField] protected bool shouldRespawn = true;  // true = 普通敵人，false = Boss

    [Header("元件參考")]
    protected CharacterController characterController;
    protected Transform playerTransform;

    // 狀態機
    protected enum EnemyState { Idle, Attacking, Evading, Dead }
    protected EnemyState currentState = EnemyState.Idle;
    protected bool playerInSight = false;

    // 垂直速度與重力
    protected Vector3 velocity = Vector3.zero;
    protected float gravity = 9.81f;

    // 敵人 ID
    protected int enemyID;

    // 走位相關變數 (方案B)
    protected Vector3 currentEvadeDirection = Vector3.zero;
    protected float evadeDuration = 0f;                       // 走位總時長
    protected float evadeElapsedTime = 0f;                    // 走位已耗時
    protected float timeSinceLastDirectionChange = 0f;        // 上次改變方向後的耗時
    protected float currentDirectionChangeProbability = 0.1f; // 當前改變方向的概率

    // 屬性訪問
    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;
    public bool IsDead => currentState == EnemyState.Dead;
    public bool ShouldRespawn => shouldRespawn;

    protected virtual void Awake()
    {
        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError($"BaseEnemy: {gameObject.name} 需要 CharacterController 元件。");
            enabled = false;
            return;
        }

        currentHP = maxHP;
        enemyID = GetInstanceID();
    }

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("BaseEnemy: 找不到玩家。敵人將保持待機。");
        }
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead)
            return;

        CheckForPlayer();
        UpdateState();
        ApplyGravity();

        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// 主狀態機更新。
    /// </summary>
    protected virtual void UpdateState()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // 待機狀態：當看到玩家時進入攻擊
                if (playerInSight && playerTransform != null)
                {
                    currentState = EnemyState.Attacking;
                    OnStateChange(EnemyState.Attacking);
                }
                break;

            case EnemyState.Attacking:
                // 攻擊狀態
                if (!playerInSight || playerTransform == null)
                {
                    // 玩家逃出視線，回到待機
                    currentState = EnemyState.Idle;
                    velocity.x = 0;
                    velocity.z = 0;
                    OnAttackEnd();  // 通知派生類攻擊結束（重置內部狀態）
                    OnStateChange(EnemyState.Idle);
                }
                else
                {
                    // 執行符卡邏輯
                    ExecuteSpellCard();

                    // 派生類決定符卡是否結束
                    if (IsSpellCardFinished())
                    {
                        // 符卡完成，進入走位
                        PrepareEvadePhase();
                        currentState = EnemyState.Evading;
                        OnAttackEnd();  // 通知派生類攻擊結束
                        OnStateChange(EnemyState.Evading);
                    }
                }
                break;

            case EnemyState.Evading:
                // 走位狀態
                if (!playerInSight || playerTransform == null)
                {
                    // 玩家逃出視線，回到待機
                    currentState = EnemyState.Idle;
                    velocity.x = 0;
                    velocity.z = 0;
                    OnStateChange(EnemyState.Idle);
                }
                else
                {
                    // 更新走位邏輯 (方案B)
                    UpdateEvadePhase();

                    // 走位時長已到，回到攻擊
                    if (evadeElapsedTime >= evadeDuration)
                    {
                        currentState = EnemyState.Attacking;
                        velocity.x = 0;
                        velocity.z = 0;
                        OnStateChange(EnemyState.Attacking);
                    }
                }
                break;
        }
    }

    /// <summary>
    /// 檢查玩家是否在視野範圍內。
    /// </summary>
    protected virtual void CheckForPlayer()
    {
        playerInSight = false;

        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > sightRange)
            return;

        Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer < fieldOfViewAngle / 2f)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
            // 檢查普通墻或隱形墻是否阻擋視線
            LayerMask obstructionMask = wallLayer | invisibleWallLayer;
            if (Physics.Raycast(rayOrigin, directionToPlayer, out RaycastHit hit, sightRange, playerLayer | obstructionMask))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    playerInSight = true;
                }
            }
        }
    }

    /// <summary>
    /// 執行符卡邏輯。派生類在此實作具體的彈幕發射。
    /// </summary>
    protected virtual void ExecuteSpellCard()
    {
        // 派生類覆寫此方法以實現攻擊邏輯
    }

    /// <summary>
    /// 判斷符卡是否已結束。派生類決定什麼代表「結束」。
    /// </summary>
    protected virtual bool IsSpellCardFinished()
    {
        // 派生類覆寫此方法以決定符卡結束條件
        return false;
    }

    /// <summary>
    /// 攻擊結束時調用。派生類可用此重置內部狀態（如計數器、計時器）。
    /// </summary>
    protected virtual void OnAttackEnd()
    {
        // 派生類可覆寫以重置符卡相關狀態
    }

    /// <summary>
    /// 準備走位階段 (方案B)。
    /// 隨機挑選一個 N~M 秒的走位時長，並選擇初始逃亡方向。
    /// </summary>
    protected virtual void PrepareEvadePhase()
    {
        // 隨機決定本次走位的總時長
        evadeDuration = Random.Range(evadeDurationMin, evadeDurationMax);
        evadeElapsedTime = 0f;
        timeSinceLastDirectionChange = 0f;
        currentDirectionChangeProbability = initialDirectionChangeProbability;

        // 選擇初始逃亡方向（遠離玩家）
        ChooseNewEvadeDirection();
    }

    /// <summary>
    /// 選擇一個新的遠離玩家的方向。
    /// </summary>
    protected virtual void ChooseNewEvadeDirection()
    {
        Vector3 dirFromPlayer = (transform.position - playerTransform.position).normalized;
        float randomAngle = Random.Range(0f, 360f);

        currentEvadeDirection = Quaternion.Euler(0, randomAngle, 0) * dirFromPlayer;
        currentEvadeDirection.y = 0;
        currentEvadeDirection = currentEvadeDirection.normalized;

        timeSinceLastDirectionChange = 0f;
    }

    /// <summary>
    /// 更新走位邏輯 (方案B)。
    /// 每秒根據逐漸升高的概率改變方向，遇到普通墻或隱形墻時立即改變方向。
    /// </summary>
    protected virtual void UpdateEvadePhase()
    {
        evadeElapsedTime += Time.deltaTime;
        timeSinceLastDirectionChange += Time.deltaTime;

        // 更新改變方向的概率（從低到高）
        float progressRatio = evadeElapsedTime / evadeDuration;  // 0 ~ 1
        currentDirectionChangeProbability = Mathf.Lerp(initialDirectionChangeProbability, maxDirectionChangeProbability, progressRatio);

        // 每 directionChangeInterval 秒檢查一次是否改變方向
        if (timeSinceLastDirectionChange >= directionChangeInterval)
        {
            if (Random.value < currentDirectionChangeProbability)
            {
                ChooseNewEvadeDirection();
            }
            timeSinceLastDirectionChange = 0f;
        }

        // 移動敵人
        Vector3 moveDirection = currentEvadeDirection;
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized;

        // 檢查前方是否有普通墻或隱形墻
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        LayerMask obstacleMask = wallLayer | invisibleWallLayer;
        if (Physics.Raycast(rayOrigin, moveDirection, out RaycastHit hit, 1f, obstacleMask))
        {
            // 遇到障礙，立即選擇新方向
            ChooseNewEvadeDirection();
            moveDirection = currentEvadeDirection.normalized;
        }

        // 旋轉敵人面向移動方向
        transform.rotation = Quaternion.LookRotation(moveDirection);

        // 設置速度
        velocity.x = moveDirection.x * moveSpeed;
        velocity.z = moveDirection.z * moveSpeed;
    }

    /// <summary>
    /// 敵人受傷。
    /// </summary>
    public virtual void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        EventManager.TriggerEvent($"OnEnemy{enemyID}Damaged");

        Debug.Log($"{gameObject.name} 受到 {damage} 點傷害。血量: {currentHP}/{maxHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 敵人死亡。
    /// </summary>
    protected virtual void Die()
    {
        currentState = EnemyState.Dead;
        velocity = Vector3.zero;

        EventManager.TriggerEvent($"OnEnemy{enemyID}Died");
        EventManager.TriggerEvent("OnEnemyDefeated");

        Debug.Log($"{gameObject.name} 已被擊敗！");

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 狀態改變時調用。派生類可覆寫以處理動畫、音效等。
    /// </summary>
    protected virtual void OnStateChange(EnemyState newState)
    {
        // 派生類可覆寫
    }

    /// <summary>
    /// 應用重力。
    /// </summary>
    protected virtual void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            velocity.y = -0.5f;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    /// <summary>
    /// 重置敵人狀態（檢查點重生）。
    /// </summary>
    public virtual void ResetEnemy()
    {
        currentHP = maxHP;
        currentState = EnemyState.Idle;
        playerInSight = false;
        velocity = Vector3.zero;
        currentEvadeDirection = Vector3.zero;
        evadeElapsedTime = 0f;
        timeSinceLastDirectionChange = 0f;
        currentDirectionChangeProbability = initialDirectionChangeProbability;
        OnAttackEnd();  // 重置符卡內部狀態
    }
}