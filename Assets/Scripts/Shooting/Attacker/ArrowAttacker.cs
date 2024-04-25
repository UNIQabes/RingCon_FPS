using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

//delegate void OnHitCallback(HitData h);

public class ArrowAttacker : MonoBehaviour
{
    [SerializeField]float FullChargedDamage;
    [SerializeField] float MiddleChargeMaxDamage;
    [SerializeField] float MiddleChargeMinDamage;
    public float ChargeRate=0;
    public Action<HitData> OnHitShootingTarget=(h)=> { Debug.Log("にゃ〜ん3"); };
    public UnityEvent<HitData> OnHitShootingTargetEvent=new UnityEvent<HitData>();



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
            
            float damage= ChargeRate >= 1 ? FullChargedDamage : Mathf.Clamp(ChargeRate, 0, 1) * (MiddleChargeMaxDamage - MiddleChargeMinDamage) + MiddleChargeMinDamage;
            
            target.OnContact(new ShootingContactData(-damage,contactPos: other.ClosestPoint(this.transform.position),
                contactType: ChargeRate >= 1 ? ShootingContactType.FullChargeShot : ShootingContactType.NormalShot));
            OnHitShootingTarget(new HitData());
            OnHitShootingTargetEvent.Invoke(new HitData());

        }
    }
}

public class HitData
{
}
