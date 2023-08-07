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

    //CanvasがoverRayモードの時のみ使える
    protected void PrintDamageDisp(string dispStr,DamageDisp damageDispAsset,RectTransform canvasRect,Vector3 worldPos,int fontSize=24)
    {
        Debug.Log($"dispStr:{dispStr}");
        Vector3 damageDispPos;
        Debug.Log($"canvasRect==null:{canvasRect==null}");
        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRect, Camera.main.WorldToScreenPoint(worldPos), null, out damageDispPos);
        damageDispAsset.DispText = dispStr;
        damageDispAsset.FontSize = fontSize;
        Instantiate(damageDispAsset.gameObject, damageDispPos, Quaternion.identity, canvasRect);
    }
}

public class ShootingContactData
{
    public float HPVariation { get; private set; }
    public Vector3 ContactPos { get; private set; }
    public ShootingContactType ContactType { get; private set; }

    public ShootingContactData(float hpVariation,Vector3 contactPos=new Vector3(),ShootingContactType contactType=ShootingContactType.NormalShot)
    {
        HPVariation = hpVariation;
        ContactPos = contactPos;
        ContactType = contactType;
    }                       
    
}

public enum ShootingContactType
{
    NormalShot,
    FullChargeShot
}
