using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static DifficultySettings;
public enum GameStage
{
    Preparation,
    Combat,
    Lose
}
public class GamePlayController : MonoBehaviour
{
    public Map map;
    public InputController inputController;
    public GameData gameData;
    public UIController uiController;
    public AIEnemy aIEnemy;
    public ChampionShop championShop;


    [HideInInspector]
    public GameObject[] ownChampionInventoryArray;
    [HideInInspector]
    public GameObject[,] gridChampionsArray;

    [HideInInspector]public GameStage currentGameStage;
    [SerializeField] private float timer;

    [SerializeField] private int PreparationStageDuration;
    [SerializeField] private int CombatStageDuration;
   
    [HideInInspector] public bool isWinBattle;
    public int streak = 0;
    public int loseStreak = 0;

    [HideInInspector]
    public int timerDisplay = 0;
    public int currentChampionLimit;
    [HideInInspector]
    public int currentChampionCount = 0;
  
    [HideInInspector] public int currentGold;
    public int baseGoldIncome = 5;
    public int Player_HP = 100; // HP player

    public Dictionary<ChampionType, int> championTypeCount;
    public List<ChampionBonus> activeBonusList;

    [HideInInspector]public GameObject draggedChampion = null;
    private TriggerInfo dragStartTrigger = null;

    private DifficultySettings.DifficultyLevel currentDifficulty;
    private void Awake()
    {
        string difficultyString = PlayerPrefs.GetString("Difficulty", DifficultySettings.DifficultyLevel.Normal.ToString());
        if (!System.Enum.TryParse(difficultyString, out currentDifficulty))
        {
            currentDifficulty = DifficultySettings.DifficultyLevel.Normal; // Mặc định
            Debug.LogWarning("Invalid difficulty in PlayerPrefs, defaulting to Normal.");
        }
    }
    private void Start()
    {
        currentGameStage = GameStage.Preparation;

        ownChampionInventoryArray = new GameObject[Map.inventorySize];
        gridChampionsArray = new GameObject[Map.hexMapSizeX, Map.hexMapSizeZ / 2];


        DifficultyConfig config = DifficultySettings.instance.GetConfig(currentDifficulty);
        currentGold = config.startingGold;


        uiController.SetShopInitialState();
        uiController.UpdateUI();
    }
    private void Update()
    {
        if (currentGameStage == GameStage.Preparation)
        {
            timerDisplay = Mathf.CeilToInt(PreparationStageDuration - timer);
            timer += Time.deltaTime;
            uiController.UpdateTimerText();
            if (timer >= PreparationStageDuration)
            {
                if (GetChampionCountOnHexGrid() < currentChampionLimit) AutoPlaceChampionsOnGrid();
                timer = 0;
                OnGameStageComplete();
            }
        }
        else if (currentGameStage == GameStage.Combat)
        {
            timer += Time.deltaTime;
            timerDisplay = (int)(timer);
            if (timer >= CombatStageDuration)
            {
                timer = 0;

                OnGameStageComplete();
            }
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            championShop.RefreshShop(false); 
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            BuyLvl();
        }
    }


    public void StartDrag()
    {
        if (currentGameStage != GameStage.Preparation)
            return;
        TriggerInfo triggerinfo = inputController.triggerInfo;
        if (triggerinfo != null)
        {
            dragStartTrigger = triggerinfo;

            GameObject championGo = GetChampionFromTriggerInfo(triggerinfo);

            if (championGo != null)
            {
                map.ShowIndicators();
                draggedChampion = championGo;

                championGo.GetComponent<ChampionController>().IsDragged = true;
                uiController.SetShopSellState();
            }

        }
    }
    public void StopDrag()
    {
        map.HideIndicators();

        int championOnField = GetChampionCountOnHexGrid();

        if (draggedChampion != null)
        {
            draggedChampion.GetComponent<ChampionController>().IsDragged = false;
            PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };
            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);

            Debug.Log("Raycast Results Count: " + raycastResults.Count);

            bool droppedOnUI = false;
            foreach (var result in raycastResults)
            {
                Debug.Log("Hit GameObject: " + result.gameObject.name);
                if (result.gameObject.GetComponent<ShopSlotDropHandler>() != null)
                {
                    droppedOnUI = true;
                    Debug.Log("Dropped on SellSlot detected!");
                    if (draggedChampion != null)
                    {
                        SellChampion(draggedChampion);
                    }
                    else
                    {
                        Debug.LogWarning("draggedChampion is null when trying to sell!");
                    }
                    break;
                }
            }
            if (!droppedOnUI)
            {
                TriggerInfo triggerinfo = inputController.triggerInfo;

                if (triggerinfo != null)
                {
                    GameObject currentTriggerChampion = GetChampionFromTriggerInfo(triggerinfo);

                    //there is another champion in the way
                    if (currentTriggerChampion != null)
                    {
                        //store this champion to start position
                        StoreChampionInArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ, currentTriggerChampion);

                        //store this champion to dragged position
                        StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);
                    }
                    else
                    {
                        //we are adding to combat field
                        if (triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
                        {
                            //only add if there is a free spot or we adding from combatfield
                            if (championOnField < currentChampionLimit || dragStartTrigger.gridType == Map.GRIDTYPE_HEXA_MAP)
                            {
                                // remove champion from dragged position
                                RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);

                                //add this champion to dragged position
                                StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);

                                if (dragStartTrigger.gridType != Map.GRIDTYPE_HEXA_MAP) championOnField++;
                            }
                        }
                        else if (triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
                        {
                            // remove champion from dragged position
                            RemoveChampionFromArray(dragStartTrigger.gridType, dragStartTrigger.gridX, dragStartTrigger.gridZ);

                            //add this champion to dragged position
                            StoreChampionInArray(triggerinfo.gridType, triggerinfo.gridX, triggerinfo.gridZ, draggedChampion);

                            if (dragStartTrigger.gridType == Map.GRIDTYPE_HEXA_MAP) championOnField--;
                        }
                    }
                }
                CalculateTraitBonuses();
                currentChampionCount = GetChampionCountOnHexGrid();
                uiController.UpdateUI();
            }
            draggedChampion = null;
        }
        uiController.SetShopBuyState();
    }
    public void BuyLvl()
    {
        if (currentGold < 4) return;

        if (currentChampionLimit < 9)
        {
            currentChampionLimit++;
            currentGold -= 4;
            uiController.UpdateUI();
        }
    }
    public void EndRound()
    {
        timer = CombatStageDuration - 3;//reduce timer so game ends fast
    }
    public bool BuyChampionFromShop(Champion champion)
    {
        int emptyIndex = -1;
        for (int i = 0; i < ownChampionInventoryArray.Length; i++)
        {
            if (ownChampionInventoryArray[i] == null)
            {
                emptyIndex = i;
                break;
            }
        }

        //return if no slot to add champion
        if (emptyIndex == -1) return false;

        //return if not enough gold
        if (currentGold < champion.cost) return false;

        GameObject championPrefab = Instantiate(champion.prefab);

        ChampionController championController = championPrefab.GetComponent<ChampionController>();
        championController.Init(champion, 0); // 0 = player teamID

        championController.SetGridPosition(Map.GRIDTYPE_OWN_INVENTORY, emptyIndex, -1);

        championController.SetWorldPosition();
        championController.SetWorldRotation();

        StoreChampionInArray(Map.GRIDTYPE_OWN_INVENTORY, map.ownTriggerArray[emptyIndex].gridX, -1, championPrefab);

        if (currentGameStage == GameStage.Preparation) TryUpgradeChampion(champion);

        currentGold -= champion.cost;
        uiController.UpdateUI();

        return true;

    }
    public void SellChampion(GameObject champion)
    {
        Debug.Log("SellChampion called for: " + (champion != null ? champion.name : "null"));
       if (currentGameStage != GameStage.Preparation) return;

        if (champion == null)
        {
            Debug.LogWarning("championObject is null!");
            return;
        }
        ChampionController championController = champion.GetComponent<ChampionController>();
        if (championController == null || championController.isDead)
        {
            Debug.LogWarning("ChampionController is null: " + (championController == null) + " or champion is dead: " + (championController?.isDead));
            return;
        }

        int CurrentStar = championController.currentStar;
        int refundGold = championController.champion.starLevelStats[CurrentStar-1].sellPrice;

        currentGold += refundGold;
        uiController.UpdateUI();

        // Trả tướng về pool
        //championShop.ReturnChampionToPool(championController.champion);

        if (championController.gridType == Map.GRIDTYPE_HEXA_MAP)
        {
           RemoveChampionFromArray(championController.gridType, championController.gridPositionX, championController.gridPositionZ);
        }
        else if (championController.gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            RemoveChampionFromArray(championController.gridType,championController.gridPositionX,championController.gridPositionZ);
        }

       foreach (Effect e in championController.effects)
       {
            championController.RemoveEffect(e);
       }
       
       

        Destroy(champion);

        Debug.Log("Sell champion: " + championController.champion.name + " for " + refundGold + " gold.");
    }
    private GameObject GetChampionFromTriggerInfo(TriggerInfo _triggerinfo)
    {
        GameObject championGO = null;
        if (_triggerinfo.gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            championGO = ownChampionInventoryArray[_triggerinfo.gridX];
        }
        else if (_triggerinfo.gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            championGO = gridChampionsArray[_triggerinfo.gridX, _triggerinfo.gridZ];
        }
        return championGO;
    }
    private void StoreChampionInArray(int gridType, int gridX, int gridZ, GameObject champion)
    {
        ChampionController championController = champion.GetComponent<ChampionController>();
        championController.SetGridPosition(gridType, gridX, gridZ);

        if (gridType == Map.GRIDTYPE_OWN_INVENTORY)
        {
            ownChampionInventoryArray[gridX] = champion;
        }
        else if (gridType == Map.GRIDTYPE_HEXA_MAP)
        {
            gridChampionsArray[gridX, gridZ] = champion;
        }
    }
    private void RemoveChampionFromArray(int type, int gridX, int gridZ)
    {
        if (type == Map.GRIDTYPE_OWN_INVENTORY)
        {
            ownChampionInventoryArray[gridX] = null;
        }
        else if (type == Map.GRIDTYPE_HEXA_MAP)
        {
            gridChampionsArray[gridX, gridZ] = null;
        }
    }
    private int GetChampionCountOnHexGrid()
    {
        int count = 0;
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] != null)
                {
                    count++;
                }
            }
        }

        return count;
    }
    private void CalculateTraitBonuses()
    {
        championTypeCount = new Dictionary<ChampionType, int>();

        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] != null)
                {
                    Champion c = gridChampionsArray[x, z].GetComponent<ChampionController>().champion;

                    if (championTypeCount.ContainsKey(c.type1))
                    {
                        int cCount = 0;
                        championTypeCount.TryGetValue(c.type1, out cCount);

                        cCount++;

                        championTypeCount[c.type1] = cCount;
                    }
                    else
                    {
                        championTypeCount.Add(c.type1, 1);
                    }


                    if (championTypeCount.ContainsKey(c.type2))
                    {
                        int cCount = 0;
                        championTypeCount.TryGetValue(c.type2, out cCount);

                        cCount++;

                        championTypeCount[c.type2] = cCount;
                    }
                    else
                    {
                        championTypeCount.Add(c.type2, 1);
                    }
                }
            }

            activeBonusList = new List<ChampionBonus>();

            foreach (KeyValuePair<ChampionType, int> m in championTypeCount)
            {
                ChampionBonus championBonus = m.Key.championBonus;

                if (m.Value >= championBonus.championCount)
                {
                    activeBonusList.Add(championBonus);
                }
            }
        }
    }

    private void TryUpgradeChampion(Champion champion)
    {
        //check for champion upgrade
        List<ChampionController> championList_lvl1 = new List<ChampionController>();
        List<ChampionController> championList_lvl2 = new List<ChampionController>();

        for (int i = 0; i < ownChampionInventoryArray.Length; i++)
        {
            if (ownChampionInventoryArray[i] != null)
            {
                ChampionController championController = ownChampionInventoryArray[i].GetComponent<ChampionController>();

                //check if is the same type of champion that we are buying
                if (championController.champion == champion)
                {
                    if (championController.currentStar == 1)
                    {
                        championList_lvl1.Add(championController);
                    }
                    else if (championController.currentStar == 2)
                    {
                        championList_lvl2.Add(championController);
                    }
                }
            }
        }


        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] != null)
                {
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    //check if is the same type of champion that we are buying
                    if (championController.champion == champion)
                    {
                        if (championController.currentStar == 1)
                        {
                            championList_lvl1.Add(championController);
                        }
                        else if (championController.currentStar == 2)
                        {
                            championList_lvl2.Add(championController);
                        }
                    }
                }
            }
        }



        //if we have 3 we upgrade a champion and delete rest
        if (championList_lvl1.Count > 2) //upgrade to lvl 2
        {
            championList_lvl1[2].UpgradeLevel();

            //remove champion from inventory
            RemoveChampionFromArray(championList_lvl1[0].gridType, championList_lvl1[0].gridPositionX, championList_lvl1[0].gridPositionZ);
            RemoveChampionFromArray(championList_lvl1[1].gridType, championList_lvl1[1].gridPositionX, championList_lvl1[1].gridPositionZ);

            Destroy(championList_lvl1[0].gameObject);
            Destroy(championList_lvl1[1].gameObject);

            //upgrade champion to lvl 3
            if (championList_lvl2.Count > 1)
            {
                championList_lvl1[2].UpgradeLevel();

                //remove champion from inventory
                RemoveChampionFromArray(championList_lvl2[0].gridType, championList_lvl2[0].gridPositionX, championList_lvl2[0].gridPositionZ);
                RemoveChampionFromArray(championList_lvl2[1].gridType, championList_lvl2[1].gridPositionX, championList_lvl2[1].gridPositionZ);

                Destroy(championList_lvl2[0].gameObject);
                Destroy(championList_lvl2[1].gameObject);
            }
        }

        currentChampionCount = GetChampionCountOnHexGrid();

        uiController.UpdateUI();
    }
    // Future: will optimize this function
    private void AutoPlaceChampionsOnGrid()
    {
        try
        {
            int currentChampionOnGrid = GetChampionCountOnHexGrid();
            int championsToPlace = currentChampionLimit - currentChampionOnGrid;

            if (championsToPlace <= 0)
            {
                Debug.Log("No need to auto place champions");
                return;
            }

            // Lấy danh sách các slot không trống trong kho
            List<int> availableChampions = new List<int>();
            for (int i = 0; i < ownChampionInventoryArray.Length; i++)
            {
                if (ownChampionInventoryArray[i] != null)
                {
                    availableChampions.Add(i);
                }
            }

            if (availableChampions.Count == 0)
            {
                Debug.LogWarning("No champions available in inventory to place");
                return;
            }

            // Chọn ngẫu nhiên tối đa championsToPlace tướng
            championsToPlace = Mathf.Min(championsToPlace, availableChampions.Count);
            List<int> selectedIndices = new List<int>();
            while (selectedIndices.Count < championsToPlace && availableChampions.Count > 0)
            {
                int randomIndex = Random.Range(0, availableChampions.Count);
                selectedIndices.Add(availableChampions[randomIndex]);
                availableChampions.RemoveAt(randomIndex);
            }

            // Tìm các ô trống trên sàn đấu (chỉ nửa dưới, z < hexMapSizeZ / 2)
            List<Vector2Int> emptyGridPositions = new List<Vector2Int>();
            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    if (gridChampionsArray[x, z] == null)
                    {
                        emptyGridPositions.Add(new Vector2Int(x, z));
                    }
                }
            }

            if (emptyGridPositions.Count < championsToPlace)
            {
                Debug.LogWarning($"Not enough empty grid positions. Can only place {emptyGridPositions.Count} champions");
                championsToPlace = emptyGridPositions.Count;
            }

            // Đặt tướng lên sàn đấu
            for (int i = 0; i < championsToPlace; i++)
            {
                int inventoryIndex = selectedIndices[i];
                Vector2Int gridPos = emptyGridPositions[i];

                GameObject championObj = ownChampionInventoryArray[inventoryIndex];
                if (championObj == null)
                {
                    Debug.LogWarning($"Champion at inventory index {inventoryIndex} is null");
                    continue;
                }

                ChampionController championController = championObj.GetComponent<ChampionController>();
                if (championController == null)
                {
                    Debug.LogWarning($"ChampionController missing at inventory index {inventoryIndex}");
                    continue;
                }

                // Lưu vào sàn đấu
                StoreChampionInArray(Map.GRIDTYPE_HEXA_MAP, gridPos.x, gridPos.y, championObj);

                // Xóa khỏi kho
                //RemoveChampionFromArray(Map.GRIDTYPE_OWN_INVENTORY, inventoryIndex, -1);
                ownChampionInventoryArray[inventoryIndex] = null;

                // Cập nhật vị trí và góc quay
                championController.SetWorldPosition();
                championController.SetWorldRotation();
            }

            // Cập nhật số lượng tướng, bonus đặc tính và UI
            currentChampionCount = GetChampionCountOnHexGrid();
            CalculateTraitBonuses();
            uiController.UpdateUI();

            Debug.Log($"Auto placed {championsToPlace} champions on grid");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in AutoPlaceChampionsOnGrid: {ex.Message}");
        }
    }
    private void OnGameStageComplete()
    {
        aIEnemy.OnGameStageComplete(currentGameStage);

        if (currentGameStage == GameStage.Preparation)
        {
            currentGameStage = GameStage.Combat;
            map.HideIndicators();

            uiController.SetActive(false);

            if (draggedChampion != null)
            {
                draggedChampion.GetComponent<ChampionController>().IsDragged = false;
                draggedChampion = null;
            }

            for (int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    //there is a champion
                    if (gridChampionsArray[x, z] != null)
                    {
                        //get character
                        ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                        //start combat
                        championController.OnCombatStart();
                    }

                }
            }

            if (IsAllChampionDead()) // losing if dont buy any champion to place into grid
                EndRound();

        }
        else if (currentGameStage == GameStage.Combat)
        {
            currentGameStage = GameStage.Preparation;

            uiController.SetActive(true);

            ResetChampions();

            for (int i = 0; i < gameData.championArray.Length; i++)
            {
                TryUpgradeChampion(gameData.championArray[i]);
            }


            UpdateStreak();
            currentGold += CaculateIncome();

            uiController.UpdateUI();
            championShop.RefreshShop(true);

            if (Player_HP <= 0)
            {
                currentGameStage = GameStage.Lose;
                uiController.ShowLoseScreen();
            }
        }
    }
    private void ResetChampions()
    {
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    //reset
                    championController.Reset();
                }

            }
        }
    }
    public void RestartGame()
    {
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] != null)
                {
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    Destroy(championController.gameObject);
                    gridChampionsArray[x, z] = null;

                }
            }
        }
        for (int i = 0; i < Map.inventorySize; i++)
        {
            if (ownChampionInventoryArray[i] != null)
            {
                ChampionController championController = ownChampionInventoryArray[i].GetComponent<ChampionController>();

                Destroy(championController.gameObject);
                ownChampionInventoryArray[i] = null;
            }
        }
        Player_HP = 100;
        DifficultyConfig config = DifficultySettings.instance.GetConfig(currentDifficulty);
        currentGold = config.startingGold;
        streak = 0; 
        loseStreak = 0;
        currentGameStage = GameStage.Preparation;
        
        currentChampionLimit = 1;
        currentChampionCount = GetChampionCountOnHexGrid();

        uiController.UpdateUI();


        aIEnemy.Restart();

        uiController.ShowGameScreen();

    }
    public void OnPlayerChampionDeath()
    {
        bool allDead = IsAllChampionDead();
        if (allDead)
        {
            isWinBattle = false;
            EndRound();
        }

    }
    private bool IsAllChampionDead()
    {
        int championDead = 0;
        int championCount = 0;
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] != null)
                {
                    ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();
                    championCount++;
                    if (championController.isDead) championDead++;
                    //if (!championController.isDead) return false;
                    //else championDead++;

                }
            }
        }
        if (championDead == championCount++)
            return true;

        return false;

    }
    private int CaculateIncome()
    {
        int income = 0;
        //int bank = currentGold / 10;

        income += baseGoldIncome;
        //income += bank;

        if (streak >= 2) income += Mathf.Min(streak - 1, 3);  // +1 gold at 2 wins, +2 at 3, +3 at 4+
        if (loseStreak >= 2) income += Mathf.Min(loseStreak - 1, 3); // +1 gold at 2 losses, +2 at 3, +3 at 4+

        return income;
    }
    private void UpdateStreak()
    {
        if (isWinBattle)
        {
            streak++;
            loseStreak = 0; // Reset lose streak on win
        }
        else
        {
            streak = 0; // Reset win streak on loss
            loseStreak++;
        }
    }
    public void TakeDamage(int damage)
    {
        Player_HP -= damage;
        uiController.UpdateUI();
    }
    
}
