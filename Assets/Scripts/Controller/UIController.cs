using JetBrains.Annotations;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class UIController : MonoBehaviour
{
    public GamePlayController gamePlayController;
    public ChampionShop championShop;

    public GameObject[] championsFrameArray;
    public GameObject[] bonusTraitPanel;

    public GameObject BonusContainer;
    public GameObject placementText;
    public GameObject winStreakUI;
    public GameObject loseStreakUI;


    public TextMeshProUGUI timerText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI championCountText;
    public TextMeshProUGUI streakWinText;
    public TextMeshProUGUI streakLoseText;


    public void OnChamponClicked()
    {
        string name = EventSystem.current.currentSelectedGameObject.transform.parent.name;

        string defaultName = "champion container ";
        int championFrameIndex = int.Parse(name.Substring(defaultName.Length, 1));

        championShop.OnChampionFrameClicked(championFrameIndex);

    }
    public void UpdateTimerText()
    {
        timerText.text = gamePlayController.timerDisplay.ToString();
    }
    public void UpdateStreakText()
    {
        if(gamePlayController.streak > 0)
        {
            streakWinText.text = gamePlayController.streak.ToString();
            winStreakUI.SetActive(true);
            loseStreakUI.SetActive(false);
        }
        else if(gamePlayController.loseStreak > 0)
        {
            streakLoseText.text = gamePlayController.streak.ToString();
            winStreakUI.SetActive(false);
            loseStreakUI.SetActive(true);
        }
    }
    public void SetTimerTextActive(bool b)
    {
        timerText.gameObject.SetActive(b);
        placementText.SetActive(b);
    }
    public void UpdateUI()
    {
        goldText.text = gamePlayController.currentGold.ToString();
        championCountText.text = gamePlayController.currentChampionCount.ToString() + " / " + gamePlayController.currentChampionLimit.ToString();
        UpdateStreakText();

        //hide all bonus trait panels
        foreach (GameObject panel in bonusTraitPanel)
        {
            panel.SetActive(false);
        }

        if(gamePlayController.championTypeCount != null)
        {
            int i = 0;
            foreach (KeyValuePair<ChampionType, int> m in gamePlayController.championTypeCount)
            {
                //Now you can access the key and value both separately from this attachStat as:
                GameObject bonusUI = bonusTraitPanel[i];
                bonusUI.transform.SetParent(BonusContainer.transform);
                TraitBonusUI traitBonusUI = bonusUI.GetComponent<TraitBonusUI>();

                traitBonusUI.icon.sprite = m.Key.icon;
                traitBonusUI.nameTrait.text = m.Key.displayName;
                traitBonusUI.count.text = m.Value.ToString();
                traitBonusUI.needToActivate.text = m.Value.ToString() + " / " + m.Key.championBonus.championCount.ToString();

                //bonusUI.transform.Find("border").Find("icon").GetComponent<Image>().sprite = m.Key.icon;
                //bonusUI.transform.Find("icon").GetComponent<Image>().sprite = m.Key.icon;
                //bonusUI.transform.Find("nameTrait").GetComponent<TextMeshProUGUI>().text = m.Key.displayName;
                //bonusUI.transform.Find("count").GetComponent<TextMeshProUGUI>().text = m.Value.ToString() + " / " + m.Key.championBonus.championCount.ToString();

                bonusUI.SetActive(true);

                i++;
            }
        }
    }
    public void LoadShop(Champion champion,int index)
    {
        Transform championUI = championsFrameArray[index].transform.Find("champion");
        Transform top = championUI.Find("top");
        Transform bottom = championUI.Find("bottom");
        Transform type1 = top.Find("type 1");
        Transform type2 = top.Find("type 2");
        Transform icon1 = top.Find("icon 1");
        Transform icon2 = top.Find("icon 2");
        Transform cost = bottom.Find("Cost");
        Transform name = bottom.Find("name");

        name.GetComponent<TextMeshProUGUI>().text = champion.ui_Name;
        cost.GetComponent<TextMeshProUGUI>().text = champion.cost.ToString();
        championUI.GetComponent<Image>().sprite = champion.display_cost;
        type1.GetComponent<TextMeshProUGUI>().text = champion.type1.displayName;
        type2.GetComponent<TextMeshProUGUI>().text = champion.type2.displayName;
        icon1.GetComponent<Image>().sprite = champion.type1.icon;
        icon2.GetComponent<Image>().sprite = champion.type2.icon;

    }
    public void ShowShopItems()
    {
        //unhide all champion frames
        for(int i=0; i < championsFrameArray.Length; i++)
        {
            championsFrameArray[i].transform.Find("champion").gameObject.SetActive(true);
        }
    }
    public void HideChampionFrame(int index)
    {
        championsFrameArray[index].transform.Find("champion").gameObject.SetActive(false);
    }
    public void Refresh_Click()
    {
        championShop.RefreshShop(false);
    }
    public void BuyLv_Click()
    {
        championShop.BuyLvl();
    }

}
