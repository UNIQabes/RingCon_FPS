using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;

public class SnipeStage2 : MonoBehaviour
{
    private bool _canTransNextWave;
    public ScoreAttackGameRuler scoreAttackGameRuler;
    public GameObject messageGObj;

    public List<GameObject> waves;
    public int currentMark;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject aWave in waves)
        {
            aWave.SetActive(false);
        }

        FixedUpdateAsync().Forget();
    }

    // Update is called once per frame
    void Update()
    {

    }

    async UniTaskVoid FixedUpdateAsync()
    {
        CancellationToken cToken = this.gameObject.GetCancellationTokenOnDestroy();
        messageGObj.SetActive(true);
        await UniTask.Delay(4000, false, PlayerLoopTiming.FixedUpdate, cancellationToken: cToken);
        messageGObj.SetActive(false);
        GameObject prevWave = null;
        foreach (GameObject aWave in waves)
        {
            if (prevWave) { prevWave.gameObject.SetActive(false); }
            aWave.SetActive(true);
            await UniTask.WaitUntil(()=> _canTransNextWave,PlayerLoopTiming.FixedUpdate,cToken);
            _canTransNextWave = false;
            prevWave = aWave;
        }
       
        scoreAttackGameRuler.isGameFinished = true;
    }
    public void TransNextWave()
    {
        _canTransNextWave = true;
    }
}
