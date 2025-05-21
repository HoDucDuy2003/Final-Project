using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public GamePlayController gamePlayController;
    public ChampionShop championShop;

    public GameObject[] championsFrameArray;
    public GameObject[] bonusTraitPanel;

    public GameObject bonusContainer;
    [SerializeField] private GameObject placementText;
    [SerializeField] private GameObject winStreakUI;
    [SerializeField] private GameObject loseStreakUI;
    [SerializeField] private GameObject shop;
    [SerializeField] private GameObject gold;
    public GameObject restartButton;
    [SerializeField] private  Button btnModeGame;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject sellPanel;


    public TextMeshProUGUI timerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI championCountText;
    public TextMeshProUGUI streakWinText;
    public TextMeshProUGUI streakLoseText;



    public void UpdateTimerText()
    {
        timerText.text = gamePlayController.timerDisplay.ToString();
    }
    public void UpdateStreakText()
    {
        if (gamePlayController.streak > 0)
        {
            streakWinText.text = gamePlayController.streak.ToString();
            winStreakUI.SetActive(true);
            loseStreakUI.SetActive(false);
        }
        else if (gamePlayController.loseStreak > 0)
        {
            streakLoseText.text = gamePlayController.loseStreak.ToString();
            winStreakUI.SetActive(false);
            loseStreakUI.SetActive(true);
        }
    }
    public void SetActive(bool b)
    {
        timerText.gameObject.SetActive(b);
        placementText.SetActive(b);
        btnModeGame.interactable = b;
    }
    private void SetHPText()
    {
        hpText.text = "HP " + gamePlayController.Player_HP.ToString();
        if (gamePlayController.Player_HP <= 0)
        {
            hpText.text = "HP 0";
        }
    }
    public void UpdateUI()
    {
        goldText.text = gamePlayController.currentGold.ToString();
        SetHPText();
        championCountText.text = gamePlayController.currentChampionCount.ToString() + " / " + gamePlayController.currentChampionLimit.ToString();
        UpdateStreakText();

        //hide all bonus trait panels
        foreach (GameObject panel in bonusTraitPanel)
        {
            panel.SetActive(false);
        }

        ShowBonusTrait();
    }
    private void ShowBonusTrait()
    {
        if (gamePlayController.championTypeCount != null)
        {
            int i = 0;
            foreach (KeyValuePair<ChampionType, int> m in gamePlayController.championTypeCount)
            {
                GameObject bonusUI = bonusTraitPanel[i];
                bonusUI.transform.SetParent(bonusContainer.transform);
                TraitBonusUI traitBonusUI = bonusUI.GetComponent<TraitBonusUI>();

                traitBonusUI.icon.sprite = m.Key.icon;
                traitBonusUI.nameTrait.text = m.Key.displayName;
                traitBonusUI.count.text = m.Value.ToString();
                traitBonusUI.needToActivate.text = m.Value.ToString() + " / " + m.Key.championBonus.championCount.ToString();

                bonusUI.SetActive(true);


                i++;
            }
        }
    }
    public void LoadShop(Champion champion, int index)
    {
        Transform child = championsFrameArray[4 - index].transform.GetChild(0);
        if (!child.gameObject.activeSelf)
            child.gameObject.SetActive(true);

        ChampionShopUI championShopUI = championsFrameArray[4 - index].GetComponentInChildren<ChampionShopUI>();
        championShopUI.gameObject.SetActive(true);

        championShopUI.championImage.sprite = champion.display_cost;
        championShopUI.nameText.text = champion.ui_Name;
        championShopUI.costText.text = champion.cost.ToString();
        championShopUI.type1.text = champion.type1.displayName;
        championShopUI.type2.text = champion.type2.displayName;
        championShopUI.Icon1.sprite = champion.type1.icon;
        championShopUI.Icon2.sprite = champion.type2.icon;

        
    }
    public void ShowChampionFrame(int index)
    {
       championsFrameArray[4 - index].transform.Find("champion").gameObject.SetActive(true);
    }
    public void HideChampionFrame(int index)
    {
        championsFrameArray[4 - index].transform.Find("champion").gameObject.SetActive(false);
    }
    public void Refresh_Click()
    {
        championShop.RefreshShop(false);
    }
    public void BuyLv_Click()
    {
        championShop.BuyLvl();
    }
    public void Restart_Click()
    {
        gamePlayController.RestartGame();
    }
    public void BackToModeGame_Click()
    {
        SceneManager.LoadScene("ModeGame");
    }
    public void OnChampionClicked()
    {
        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        string defaultName = "champion container ";
        int championFrameIndex = int.Parse(name.Substring(defaultName.Length, 1));
        championShop.OnChampionFrameClicked(championFrameIndex);

    }
    public void ShowGameScreen()
    {
        SetActive(true);
        shop.SetActive(true);
        gold.SetActive(true);

        restartButton.SetActive(false);

    }
    public void ShowLoseScreen()
    {
        SetActive(false);
        shop.SetActive(false);
        gold.SetActive(false);
        loseStreakUI.SetActive(false);
        winStreakUI.SetActive(false);

        restartButton.SetActive(true);
    }

    public void SetShopInitialState()
    {
        shopPanel.SetActive(true);
        sellPanel.SetActive(false);
    }

    
    public void SetShopSellState()
    {
        shopPanel.SetActive(false);
        sellPanel.SetActive(true);
    }
    
    public void SetShopBuyState()
    {
        shopPanel.SetActive(true);
        sellPanel.SetActive(false);
    }
}
