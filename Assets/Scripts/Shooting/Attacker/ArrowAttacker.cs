using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowAttacker : MonoBehaviour
{
    [SerializeField]float FullChargedDamage;
    [SerializeField] float MiddleChargeMaxDamage;
    [SerializeField] float MiddleChargeMinDamage;
    public float ChargeRate=0;

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
            target.OnContact(new ShootingContactData(-damage));

        }
    }
}
