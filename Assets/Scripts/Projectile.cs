using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float duration;
    [SerializeField] private GameObject target;
    [SerializeField] private bool isMoving = false;
    public void Init(GameObject _target)
    {
        target = _target;

        isMoving = true;
    }
    void Update()
    {
        if (isMoving)
        {
            if(target == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 relativePos = target.transform.position - transform.position;

            Quaternion rotaion = Quaternion.LookRotation(relativePos,Vector3.up);
            this.transform.rotation = rotaion;

            float step = speed * Time.deltaTime; // calculate distance to move

            transform.position = Vector3.MoveTowards(this.transform.position, target.transform.position, step);

            float distance = Vector3.Distance(this.transform.position, target.transform.position);

            if(distance < 0.2f)
            {
                Debug.Log("hit target");

                this.transform.parent = target.transform;

                Destroy(this.gameObject,duration);
            }
        }

    }
}
