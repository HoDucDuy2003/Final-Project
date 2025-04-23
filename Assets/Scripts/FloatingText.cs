using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private Vector3 moveDirection;
    private float timer = 0;

    public float moveSpeed = 2f;

    public float fadeOutTime = 1f;

    // Update is called once per frame
    void Update()
    {
        this.transform.position = this.transform.position + moveDirection * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;
        float fade = (fadeOutTime - timer) / fadeOutTime;

        canvasGroup.alpha = fade;

        if(fade <= 0)
        {
            Destroy(this.gameObject);
        }
    }
    public void Init(Vector3 startPos,float v)
    {
        this.transform.position = startPos;
        canvasGroup =this. GetComponent<CanvasGroup>();

        this.GetComponent<TextMeshProUGUI>().text = Mathf.Round(v).ToString();
        moveDirection = new Vector3(Random.Range(-0.5f, 0.5f), 1, Random.Range(-0.5f, 0.5f)).normalized;
    }
}
