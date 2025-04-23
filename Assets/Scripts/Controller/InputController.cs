using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    public Map map;
    public LayerMask triggerLayer;
    public GamePlayController gamePlayController;

    [HideInInspector]
    public TriggerInfo triggerInfo = null;

    private Vector3 rayCastStartPos;

    private Vector3 mousePos;
    void Start()
    {
        rayCastStartPos = new Vector3(0, 20, 0);
    }

    // Update is called once per frame
    void Update()
    {
        triggerInfo = null;
        map.resetIndicators();

        RaycastHit hit;

        //convert mouse screen position to ray
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out hit, 100f, triggerLayer,QueryTriggerInteraction.Collide))
        {
            triggerInfo = hit.collider.gameObject.GetComponent<TriggerInfo>();
            if (triggerInfo != null)
            {
                //get indicator
                GameObject indicator = map.GetIndicatorFromTriggerInfo(triggerInfo);

                //set indicator color to active
                indicator.GetComponent<MeshRenderer>().material.color = map.indicatorActiveColor;
            }
            else
            {
                map.resetIndicators();
            }
        }
        if(Input.GetMouseButtonDown(0))
        {
            gamePlayController.StartDrag();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gamePlayController.StopDrag();
        }

        mousePos = Input.mousePosition;
    }
}
