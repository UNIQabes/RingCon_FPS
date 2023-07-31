using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingTarget : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public virtual void OnContact(ShootingContactData contactData)
    {
        Debug.Log($"HPVariation:{contactData.HPVariation}");
    
    }
}

public class ShootingContactData
{
    public float HPVariation { get; private set; }
    public Vector3 ContactPos { get; private set; }

    public ShootingContactData(float hpVariation,Vector3 contactPos=new Vector3())
    {
        HPVariation = hpVariation;
        ContactPos = contactPos;
    }                       
    
}
