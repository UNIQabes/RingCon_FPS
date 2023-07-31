using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletAttacker : MonoBehaviour
{
    //public float LifeTime;
    public float BulletDamage;
    // Start is called before the first frame update
    void Start()
    {
        //LifeTime = 2;
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.gameObject.name}が当たった!!");
        ShootingTarget target = other.gameObject.GetComponent<ShootingTarget>();
        if (target)
        {
            target.OnContact(new ShootingContactData(-BulletDamage,other.ClosestPointOnBounds(this.transform.position))) ;
        }
    }
}
