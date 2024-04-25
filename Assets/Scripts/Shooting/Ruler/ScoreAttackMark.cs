using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class ScoreAttackMark : MonoBehaviour
{
    public ScoreAttackGameRuler scoreAttackGameRuler;
    public int DissapearTime;
    private float timer = 0;
    public float score;
    public GameObject ClockHand;
    
    
    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        //DissapearTimer().Forget();
    }
    
    private void FixedUpdate()
    {
        
        timer += Time.fixedDeltaTime;
        if (ClockHand)
        {
            ClockHand.transform.rotation = Quaternion.AngleAxis(360 * timer * 1000 / DissapearTime, new Vector3(0, 0, -1));
        }
        
        if (timer * 1000 > DissapearTime)
        {
            Destroy(this.gameObject);
        }

    }
    
    private async UniTaskVoid DissapearTimer()
    {
        await UniTask.Delay(DissapearTime,false,PlayerLoopTiming.FixedUpdate,gameObject.GetCancellationTokenOnDestroy());
        Destroy(this.gameObject);
    }
    
    public void OnMarkDestroyed()
    {
        scoreAttackGameRuler.Score += score;
        Destroy(this.gameObject);
    }
}
