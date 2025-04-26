using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEnemy : MonoBehaviour
{
    public ChampionShop championShop;
    public Map map;
    public UIController uIController;
    public GamePlayController gamePlayController;

    public GameObject[,] gridChampionsArray;

    public Dictionary<ChampionType, int> championTypeCount;
    public List<ChampionBonus> activeBonusList;

    ///The damage that player takes when losing a round
    public int TakeDamageWhenLose = 2;

    public void OnMapReady()
    {
        gridChampionsArray = new GameObject[Map.hexMapSizeX, Map.hexMapSizeZ / 2];

        AddRandomChampion();
    }
    public void OnGameStageComplete(GameStage stage)
    {
        if(stage == GameStage.Preparation)
        {
            for(int x =0;x <Map.hexMapSizeX; x++)
            {
                for(int z =0;z<Map.hexMapSizeZ/2;z++)
                {
                    if (gridChampionsArray[x, z] != null)
                    {
                        ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                        championController.OnCombatStart();

                    }
                }
            }
        }
        if(stage == GameStage.Combat)
        {
            for(int x = 0; x < Map.hexMapSizeX; x++)
            {
                for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
                {
                    if (gridChampionsArray[x, z] != null)
                    {
                        ChampionController championController = gridChampionsArray[x, z].GetComponent<ChampionController>();

                        Debug.Log("Champion " + championController.champion.name + " is attacking");
                    }
                }
            }

            
            ResetChampions();
            AddRandomChampion();
        }
    }
    private void GetEmptySlot(out int emptyIndexX, out int emptyIndexZ)
    {
        emptyIndexX = -1;
        emptyIndexZ = -1;

        //get first empty inventory slot
        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                if (gridChampionsArray[x, z] == null)
                {
                    emptyIndexX = x;
                    emptyIndexZ = z;
                    break;
                }
            }
        }
    }
    public void AddRandomChampion()
    {
        int indexX, indexZ;
        GetEmptySlot(out indexX, out indexZ);

        //dont add champion if there is no empty slot
        if (indexX == -1 || indexZ == -1) return;

        Champion champion = championShop.GetRandomChampionInfo();
        GameObject championPrefab = Instantiate(champion.prefab);

        gridChampionsArray[indexX,indexZ] = championPrefab;
        ChampionController championController = championPrefab.GetComponent<ChampionController>();
        championController.Init(champion, 1);

        championController.SetGridPosition(Map.GRIDTYPE_HEXA_MAP,indexX, indexZ + 4);

        championController.SetWorldPosition();
        championController.SetWorldRotation();

        List<ChampionController> championList_lvl_1 = new List<ChampionController>();
        List<ChampionController> championList_lvl_2 = new List<ChampionController>();

        for (int x = 0; x < Map.hexMapSizeX; x++)
        {
            for (int z = 0; z < Map.hexMapSizeZ / 2; z++)
            {
                //there is a champion
                if (gridChampionsArray[x, z] != null)
                {
                    //get character
                    ChampionController cc = gridChampionsArray[x, z].GetComponent<ChampionController>();

                    //check if is the same type of champion that we are buying
                    if (cc.champion == champion)
                    {
                        if (cc.lvl == 1)
                            championList_lvl_1.Add(cc);
                        else if (cc.lvl == 2)
                            championList_lvl_2.Add(cc);
                    }
                }

            }
        }

        if (championList_lvl_1.Count == 3)
        {
            //upgrade
            championList_lvl_1[2].UpgradeLevel();

            //destroy gameobjects
            Destroy(championList_lvl_1[0].gameObject);
            Destroy(championList_lvl_1[1].gameObject);

            //we upgrade to lvl 3
            if (championList_lvl_2.Count == 2)
            {
                //upgrade
                championList_lvl_1[2].UpgradeLevel();

                //destroy gameobjects
                Destroy(championList_lvl_2[0].gameObject);
                Destroy(championList_lvl_2[1].gameObject);
            }
        }

        //CalculateBonuses();
    }
    public void Restart()
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

                    Destroy(championController.gameObject);
                    gridChampionsArray[x, z] = null;

                }

            }
        }

        AddRandomChampion();
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

                    //set position and rotation
                    championController.Reset();
                }

            }
        }
    }
    public void OnChampionDeath()
    {
        bool allDead = IsAllChampionDeath();
        if (allDead)
        {
            gamePlayController.isWinBattle = true;
            gamePlayController.EndRound();
        }  
    }
    private bool IsAllChampionDeath()
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
                }
            }
        }
        if (championDead == championCount++)
            return true;

        return false;

    }
}
