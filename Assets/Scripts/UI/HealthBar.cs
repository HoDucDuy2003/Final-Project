using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private GameObject championGo;
    private ChampionController championController;
    public Image fillImage;

    private CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (championGo != null)
        {
            this.transform.position = championGo.transform.position + new Vector3(0, 1.5f + 1.5f * championGo.transform.localScale.x, 0);
            fillImage.fillAmount = championController.currentHealth / championController.maxHealth;

            if (championController.currentHealth <= 0)
            {
                canvasGroup.alpha = 0;
            }
            else
            {
                canvasGroup.alpha = 1;
            }
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    public void Init(GameObject _championGO)
    {
        championGo = _championGO;
        championController = championGo.GetComponent<ChampionController>();
    }
}
