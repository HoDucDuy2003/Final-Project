using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect : MonoBehaviour
{
    public GameObject effectPrefab;

    public float duration;
    private GameObject championGO;
    private GameObject effectGO;

    // Update is called once per frame
    void Update()
    {
        duration -= Time.deltaTime;

        if(duration < 0)
        {
            championGO.GetComponent<ChampionController>().RemoveEffect(this);
        }
    }
    public void Init(GameObject _effectPrefab, GameObject _championGO,float _duration)
    {
        effectPrefab = _effectPrefab;
        championGO = _championGO;
        duration = _duration;

        effectGO = Instantiate(effectPrefab);
        effectGO.transform.SetParent(championGO.transform);
        effectGO.transform.localPosition = Vector3.zero;
    }
    public void Remove()
    {
        Destroy(effectGO);
        Destroy(this);
    }
}
