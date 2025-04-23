
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ChampionController : MonoBehaviour
{

    public static int TEAM_PLAYER = 0;
    public static int TEAM_AI = 1;

    public GameObject levelupEffectPrefab;
    public GameObject projectileStart;

    [HideInInspector]
    public int gridType = 0;
    [HideInInspector]
    public int gridPositionX = 0;
    [HideInInspector]
    public int gridPositionZ = 0;
    [HideInInspector]
    public int teamID;

    [HideInInspector]
    //The level of the champion
    public int lvl = 1;
    [HideInInspector]
    public float maxHealth = 0;

    [HideInInspector]
    public float currentHealth = 0;

    [HideInInspector]
    public float currentDamage = 0;

    [HideInInspector]
    public Champion champion;

    private Map map;
    private AIEnemy aIEnemy;
    private GamePlayController gamePlayController;
    private ChampionAnimation championAnimation;
    private NavMeshAgent navMeshAgent;
    private WorldsCanvasController worldsCanvasController;

    private bool isDragged = false;
    [HideInInspector]
    private bool isAttacking = false;
    [HideInInspector]
    private bool isDead = false;

    private bool isInCombat = false;
    private float combatTimer = 0;

    private bool isStuned = false;
    private float stunTimer = 0;

    private GameObject target;
    private List<Effect> effects;


    private void Update()
    {
        if (isDragged)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float enter = 100.0f;
            if (map.m_Plane.Raycast(ray, out enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);

                //new character position
                Vector3 p = new Vector3(hitPoint.x, 1.0f, hitPoint.z);

                //move champipon to new position
                this.transform.position = Vector3.Lerp(this.transform.position, p, 0.1f);
            }
        }
        else
        {
            if (gamePlayController.currentGameStage == GameStage.Preparation)
            {
                float distance = Vector3.Distance(gridTargetPosition, this.transform.position);

                if (distance > 0.25f)
                {
                    this.transform.position = Vector3.Lerp(this.transform.position, gridTargetPosition, 0.1f);
                }
                else
                {
                    this.transform.position = gridTargetPosition;
                }
            }
        }

        if (isInCombat && isStuned == false)
        {
            if (target == null)
            {
                combatTimer += Time.deltaTime;
                if (combatTimer > 0.5f)
                {
                    combatTimer = 0;
                    TryAttackNewTarget();
                }
            }
            else
            {
                this.transform.LookAt(target.transform, Vector3.up);
                if (target.GetComponent<ChampionController>().isDead == true)
                {
                    target = null;
                    navMeshAgent.isStopped = true;
                }
                else
                {
                    if (isAttacking == false)
                    {
                        float distance = Vector3.Distance(this.transform.position, target.transform.position);
                        if (distance < champion.attackRange)
                        {
                            Debug.Log(champion.attackRange);
                            DoAttack();
                        }
                        else
                        {
                            navMeshAgent.destination = target.transform.position;
                        }
                    }
                }
            }
        }
    }
    public void Init(Champion _champion, int _teamID)
    {
        champion = _champion;
        teamID = _teamID;

        //store scripts
        map = GameObject.Find("Scripts").GetComponent<Map>();
        gamePlayController = GameObject.Find("Scripts").GetComponent<GamePlayController>();
        aIEnemy = GameObject.Find("Scripts").GetComponent<AIEnemy>();
        worldsCanvasController = GameObject.Find("Scripts").GetComponent<WorldsCanvasController>();

        championAnimation = this.GetComponent<ChampionAnimation>();
        navMeshAgent = this.GetComponent<NavMeshAgent>();

        // disable agent
        navMeshAgent.enabled = false;

        maxHealth = champion.health;
        currentHealth = champion.health;
        currentDamage = champion.attackDamage;

        worldsCanvasController.AddHealthBar(this.gameObject);


        effects = new List<Effect>();
    }
    public void Reset()
    {
        this.gameObject.SetActive(true);

        isDead = false;
        isInCombat = false;
        target = null;
        isAttacking = false;

        SetWorldPosition();
        SetWorldRotation();

        foreach (Effect e in effects)
        {
            e.Remove();
        }

        effects = new List<Effect>();
    }
    private Vector3 gridTargetPosition;

    // Assign new grid position
    public void SetGridPosition(int _gridType, int _gridPositionX, int _gridPositionZ)
    {
        gridType = _gridType;
        gridPositionX = _gridPositionX;
        gridPositionZ = _gridPositionZ;

        //set new target when chaning grid position
        gridTargetPosition = GetWorldPosition();
    }
    // Convert grid position to world position
    public Vector3 GetWorldPosition()
    {
        Vector3 worldPosition = Vector3.zero;

        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            worldPosition = map.ownInventoryGridPositions[gridPositionX];
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            worldPosition = map.mapGridPositions[gridPositionX, gridPositionZ];
        }

        return worldPosition;
    }
    //Move to correct position
    public void SetWorldPosition()
    {
        navMeshAgent.enabled = false;

        Vector3 worldPosition = GetWorldPosition();

        this.transform.position = worldPosition;

        gridTargetPosition = worldPosition;
    }
    // Set correct rotation
    public void SetWorldRotation()
    {
        Vector3 rotation = Vector3.zero;

        if (teamID == TEAM_PLAYER)
        {
            rotation = new Vector3(0, 180, 0);
        }
        else if (teamID == TEAM_AI)
        {
            rotation = new Vector3(0, 0, 0);
        }
        this.transform.rotation = Quaternion.Euler(rotation);
    }
    public bool IsDragged
    {
        get { return isDragged; }
        set { isDragged = value; }
    }
    private GameObject FindTarget()
    {
        GameObject closestEnemy = null;
        float closestDistance = Mathf.Infinity;


        //find enemy
        if (teamID == TEAM_PLAYER)
        {
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    if (aIEnemy.gridChampionsArray[x, z] != null)
                    {
                        ChampionController championController = aIEnemy.gridChampionsArray[x, z].GetComponent<ChampionController>();

                        if (championController.isDead == false)
                        {
                            float distance = Vector3.Distance(this.transform.position, aIEnemy.gridChampionsArray[x, z].transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestEnemy = aIEnemy.gridChampionsArray[x, z];
                            }
                        }
                    }
                }
            }
        }
        else if (teamID == TEAM_AI)
        {
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    if (gamePlayController.gridChampionsArray[x, z] != null)
                    {
                        ChampionController championController = gamePlayController.gridChampionsArray[x, z].GetComponent<ChampionController>();

                        if (championController.isDead == false)
                        {
                            float distance = Vector3.Distance(this.transform.position, gamePlayController.gridChampionsArray[x, z].transform.position);
                            if (distance < closestDistance)
                            {
                                closestDistance = distance;
                                closestEnemy = gamePlayController.gridChampionsArray[x, z];
                            }
                        }
                    }
                }
            }
        }
        return closestEnemy;
    }

    public void TryAttackNewTarget()
    {
        target = FindTarget();

        if (target != null)
        {
            navMeshAgent.destination = target.transform.position;

            navMeshAgent.isStopped = false;
        }

    }

    public void UpgradeLevel()
    {
        lvl++;

        float newSize = 1;

        if (lvl == 2)
        {
            newSize = 1.5f;
        }

        if (lvl == 3)
        {
            newSize = 2.0f;
        }

        this.transform.localScale = new Vector3(newSize, newSize, newSize);
        GameObject levelupEffect = Instantiate(levelupEffectPrefab);

        levelupEffect.transform.position = this.transform.position;
        Destroy(levelupEffect, 2.0f);
    }
    public void OnCombatStart()
    {
        isDragged = false;

        this.transform.position = gridTargetPosition;

        if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            isInCombat = true;
            navMeshAgent.enabled = true;

            TryAttackNewTarget();
        }

    }
    private void DoAttack()
    {
        isAttacking = true;

        navMeshAgent.isStopped = true;
        championAnimation.DoAttack(true);


    }

    #region Effect
    public bool OnGotHit(float d)
    {
        List<ChampionBonus> activeBonuses = null;

        if (teamID == TEAM_PLAYER)
        {
            activeBonuses = gamePlayController.activeBonusList;
        }
        else if (teamID == TEAM_AI)
        {
            activeBonuses = aIEnemy.activeBonusList;
        }

        foreach (ChampionBonus b in activeBonuses)
        {
            d = b.ApplyOnGotHit(this, d);
        }

        currentHealth -= d;
        if (currentHealth <= 0)
        {
            this.gameObject.SetActive(false);
            isDead = true;
        }

        worldsCanvasController.AddDamageText(this.transform.position + new Vector3(0, 2.5f, 0), d);
        return isDead;
    }
    public void OnGotStun(float duration)
    {
        isStuned = true;
        stunTimer = duration;

        championAnimation.IsAnimated(false);
        navMeshAgent.isStopped = true;

    }
    public void OnGotHeal(float healValue)
    {
        currentHealth += healValue;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    public void AddEffect(GameObject effectPrefab, float duration)
    {

        if (effectPrefab == null) return;

        bool foundEffect = false;

        foreach (Effect e in effects)
        {
            if (e.effectPrefab == effectPrefab)
            {
                e.duration = duration;
                foundEffect = true;
            }
        }
        if (foundEffect == false)
        {
            Effect effect = this.gameObject.AddComponent<Effect>();
            effect.Init(effectPrefab, this.gameObject, duration);
            effects.Add(effect);
        }
    }
    public void RemoveEffect(Effect effect)
    {
        effects.Remove(effect);
        effect.Remove();
    }
    #endregion

}
