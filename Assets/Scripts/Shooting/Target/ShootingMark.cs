using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ShootingMark : ShootingTarget
{

    [SerializeField]SoundPoint _soundPoint;
    [SerializeField] AudioClip ClipOnDestroy;
    [SerializeField] AudioClip ClipOnHit;
    [SerializeField] float HP;
    [SerializeField] float _scale;
    [SerializeField] int _disappearingTime;
    [SerializeField] int _reappearingTime;
    [SerializeField] DamageDisp _damageDisp;
    [SerializeField] RectTransform _canvasRect;
    bool IsDestroyed=false;

    // Start is called before the first frame update
    void Start()
    {
        IsDestroyed = false;
        UpdateAsync().Forget();
    }

    async UniTaskVoid UpdateAsync()
    {
        CancellationToken cToken=this.GetCancellationTokenOnDestroy();
        while (true)
        {
            transform.localScale = new Vector3(1, 1, 1) * _scale;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate,cToken);
            while (!IsDestroyed)
            {
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cToken);
            }
            transform.localScale = new Vector3(0,0,0);
            await UniTask.Delay(_disappearingTime);

            float reappearingTimer = 0;
            while (true)
            {
                reappearingTimer += Time.fixedDeltaTime;
                int reappearingTimer_ms = (int)(reappearingTimer * 1000);
                transform.localScale = new Vector3(1, 1, 1)* _scale* (float)reappearingTimer_ms / (float)_reappearingTime;
                if (reappearingTimer_ms > _reappearingTime)
                {
                    //transform.localScale = new Vector3(1, 1, 1)* _scale;
                    IsDestroyed = false;
                    HP = 100;
                    break;
                }
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cToken);
            }
        }
    }
    
    public override void OnContact(ShootingContactData contactData)
    {
        
        
        base.OnContact(contactData);
        Vector3 damageDispPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(_canvasRect, Camera.main.WorldToScreenPoint(contactData.ContactPos), null,out damageDispPos);
        Instantiate(_damageDisp.gameObject, damageDispPos, Quaternion.identity,_canvasRect);
        HP += contactData.HPVariation;
        if (HP < 0)
        {
            if (_soundPoint && ClipOnDestroy)
            {
                _soundPoint.audioClip = ClipOnDestroy;
                Instantiate(_soundPoint.gameObject);
            }
            //Destroy(this.gameObject);
            IsDestroyed = true;
        }
        else
        {
            if (_soundPoint && ClipOnHit)
            {
                _soundPoint.audioClip = ClipOnHit;
                Instantiate(_soundPoint.gameObject);
            }
        }
        
    }

}
