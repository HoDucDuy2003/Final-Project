using Unity.VisualScripting;
using UnityEngine;
/// Creates map grids where the player can move champions on
public class Map : MonoBehaviour
{
    //declare grid types
    public static int GRIDTYPE_OWN_INVENTORY = 0;
    public static int GRIDTYPE_OPONENT_INVENTORY = 1;
    public static int GRIDTYPE_HEXA_MAP = 2;

    public static int hexMapSizeX = 7;
    public static int hexMapSizeZ = 8;
    public static int inventorySize = 9;

    public Plane m_Plane;
    //start positions
    public Transform ownInventoryStartPosition;
    public Transform oponentInventoryStartPosition;
    public Transform mapStartPosition;

    //indicators that show where we place champions
    public GameObject squareIndicator;
    public GameObject hexaIndicator;

    public Color indicatorDefaultColor;
    public Color indicatorActiveColor;

    //store grid positions in list
    [HideInInspector]
    public Vector3[] ownInventoryGridPositions;
    [HideInInspector]
    public Vector3[] oponentInventoryGridPositions;
    [HideInInspector]
    public Vector3[,] mapGridPositions;




    //declare arrays to store indicators
    [HideInInspector]
    public GameObject[] ownIndicatorArray;
    [HideInInspector]
    public GameObject[] oponentIndicatorArray;
    [HideInInspector]
    public GameObject[,] mapIndicatorArray;

    [HideInInspector]
    public TriggerInfo[] ownTriggerArray;
    [HideInInspector]
    public TriggerInfo[,] mapGridTriggerArray;

    private GameObject indicatorContainer;


    private void Start()
    {
        CreateGridPosition();
        CreateIndicators();
        HideIndicators();

        m_Plane = new Plane(Vector3.up, Vector3.zero);

        this.SendMessage("OnMapReady", SendMessageOptions.DontRequireReceiver); // Notify that the map is ready
    }
    //Creates the positions for all the map grids
    private void CreateGridPosition()
    {
        //initialize position arrays
        ownInventoryGridPositions = new Vector3[inventorySize];
        oponentInventoryGridPositions = new Vector3[inventorySize];
        mapGridPositions = new Vector3[hexMapSizeX, hexMapSizeZ];

        //create own inventory grid positions
        for (int i = 0; i < inventorySize; i++)
        {
            //calculate position x offset for this slot
            float offsetX = i * -2.5f;

            //calculate and store the position
            Vector3 position = GetMapHitPoint(ownInventoryStartPosition.position + new Vector3(offsetX, 0, 0));
            //add position variable to array
            ownInventoryGridPositions[i] = position;
        }
        //create map position
        for (int x = 0; x < hexMapSizeX; x++)
        {
            for (int z = 0; z < hexMapSizeZ; z++)
            {
                //calculate even or add row
                int rowOffset = z % 2;

                //calculate position x and z
                float offsetX = x * -3f + rowOffset * 1.5f;
                float offsetZ = z * -2.5f;

                //calculate and store the position
                Vector3 position = GetMapHitPoint(mapStartPosition.position + new Vector3(offsetX, 0, offsetZ));

                //add position variable to array
                mapGridPositions[x, z] = position;
            }

        }
    }

    private void CreateIndicators()
    {
        //create a container for indicators
        indicatorContainer = new GameObject();
        indicatorContainer.name = "IndicatorContainer";

        //create a container for triggers
        GameObject triggerContainer = new GameObject();
        triggerContainer.name = "TriggerContainer";

        ownIndicatorArray = new GameObject[inventorySize];
        mapIndicatorArray = new GameObject[hexMapSizeX, hexMapSizeZ /2];

        ownTriggerArray = new TriggerInfo[inventorySize];
        mapGridTriggerArray = new TriggerInfo[hexMapSizeX, hexMapSizeZ / 2];
        //iterate own grid position
        for (int i = 0; i < inventorySize; i++)
        {
            //create indicator gameobject
            GameObject indicatorGO = Instantiate(squareIndicator);

            
            indicatorGO.transform.position = ownInventoryGridPositions[i];

           
            indicatorGO.transform.parent = indicatorContainer.transform;

            //store indicator gameobject in array
            ownIndicatorArray[i] = indicatorGO;

            //create trigger gameobject
            GameObject trigger = CreateBoxTrigger(GRIDTYPE_OWN_INVENTORY, i);

            //set trigger parent
             trigger.transform.parent = triggerContainer.transform;

            //set trigger gameobject position
             trigger.transform.position = ownInventoryGridPositions[i];

            //store triggerinfo
            ownTriggerArray[i] = trigger.GetComponent<TriggerInfo>();
        }

        //iterate map grid position
        for (int x = 0; x < hexMapSizeX; x++)
        {
            for (int z = 0; z < hexMapSizeZ / 2; z++)
            {
                // create indicator gameobject
                GameObject indicatorGO = Instantiate(hexaIndicator);    
                // set indicator gameobject position
                indicatorGO.transform.position = mapGridPositions[x, z];
                // set indicator parent
                indicatorGO.transform.parent = indicatorContainer.transform;
                // store indicator gameobject in array
                mapIndicatorArray[x, z] = indicatorGO;

                
                GameObject trigger = CreateSphereTrigger(GRIDTYPE_HEXA_MAP,x,z);

                trigger.transform.parent = triggerContainer.transform;

                trigger.transform.position = mapGridPositions[x,z];

                //store triggerinfo
                mapGridTriggerArray[x,z] = trigger.GetComponent<TriggerInfo>();
            }
        }
    }

    // Get a point with accurate y axis
    public Vector3 GetMapHitPoint(Vector3 p)
    {
        Vector3 newPos = p;

        RaycastHit hit;

        //Debug.DrawRay(newPos + new Vector3(0, 10, 0), Vector3.down * 15, Color.red,100f);
        if (Physics.Raycast(newPos + new Vector3(0, 10, 0), Vector3.down, out hit, 15))
        {
            newPos = hit.point;
        }
        return newPos;
    }

    // Creates a trigger collider gameobject and returns it
    public GameObject CreateBoxTrigger(int type,int x)
    {
        GameObject trigger = new GameObject();

        // add collider component
        BoxCollider collider = trigger.AddComponent<BoxCollider>();

        //set collider size
        collider.size = new Vector3(2, 0.5f, 2);

        //set collider to trigger 
        collider.isTrigger = true;

        //add and store trigger info
        TriggerInfo trigerInfo = trigger.AddComponent<TriggerInfo>();
        trigerInfo.gridType = type;
        trigerInfo.gridX = x;

        trigger.layer = LayerMask.NameToLayer("Triggers");

        return trigger;
    }

    public GameObject CreateSphereTrigger(int type, int x,int z)
    {
        GameObject trigger = new GameObject();

        // add collider component
        SphereCollider collider = trigger.AddComponent<SphereCollider>();

        //set collider size
        collider.radius = 1.4f;

        //set collider to trigger 
        collider.isTrigger = true;

        //add and store trigger info
        TriggerInfo trigerInfo = trigger.AddComponent<TriggerInfo>();
        trigerInfo.gridType = type;
        trigerInfo.gridX = x;
        trigerInfo.gridZ = z;

        trigger.layer = LayerMask.NameToLayer("Triggers");

        return trigger;
    }

    public GameObject GetIndicatorFromTriggerInfo(TriggerInfo triggerinfo)
    {
        GameObject triggerGo = null;
        if(triggerinfo.gridType == GRIDTYPE_OWN_INVENTORY)
        {
            triggerGo = ownIndicatorArray[triggerinfo.gridX];
        }
        else if(triggerinfo.gridType == GRIDTYPE_HEXA_MAP)
        {
            triggerGo = mapIndicatorArray[triggerinfo.gridX, triggerinfo.gridZ];
        }

        return triggerGo;
    }

    public void resetIndicators()
    {
        for(int x = 0; x < hexMapSizeX; x++)
        {
            for(int z = 0; z < hexMapSizeZ / 2; z++)
            {
                mapIndicatorArray[x, z].GetComponent<MeshRenderer>().material.color = indicatorDefaultColor;
            }
        }
        for(int i = 0; i < inventorySize; i++)
        {
            ownIndicatorArray[i].GetComponent<MeshRenderer>().material.color = indicatorDefaultColor;
        }
    }

    // Make all map indicators visible
    public void ShowIndicators()
    {
        indicatorContainer.SetActive(true);
    }

    // Make all map indicators invisible
    public void HideIndicators()
    {
        indicatorContainer.SetActive(false);
    }
}
