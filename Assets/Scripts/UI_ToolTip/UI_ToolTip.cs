using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_ToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [SerializeField] private ChampionType championType; 
    [SerializeField] private GameObject tooltip; 
    [SerializeField] private TextMeshProUGUI tooltipText; 

    [SerializeField] private bool isTooltipEnabled = true; 
    public void OnPointerEnter(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }
}
