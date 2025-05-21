using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldsCanvasController : MonoBehaviour
{
    public GameObject worldCanvas;
    public GameObject floatingTextPrefabs;
    public GameObject healthBarPrefab;


    public void AddDamageText(Vector3 pos,float v)
    {
        GameObject go = Instantiate(floatingTextPrefabs);
        go.transform.SetParent(worldCanvas.transform);

        go.GetComponent<FloatingText>().Init(pos, v);
    }
    public void AddHealthBar(GameObject championGO)
    {
        GameObject go = Instantiate(healthBarPrefab);
        go.transform.SetParent(worldCanvas.transform);

        go.GetComponent<HealthBar>().Init(championGO);
    }
}
