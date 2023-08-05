using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventMark : ShootingMark
{
    [SerializeField] UnityEvent OnDestroy = new UnityEvent();
    bool prevIsDestroyed = false;
    void Update()
    {
        prevIsDestroyed = IsDestroyed;
    }
    public override void OnContact(ShootingContactData contactData)
    {
        base.OnContact(contactData);
        if (!prevIsDestroyed & IsDestroyed)
        {
            OnDestroy.Invoke();
            //Debug.Log("うわぁぁぁぁl");
        }
        
    }
}
