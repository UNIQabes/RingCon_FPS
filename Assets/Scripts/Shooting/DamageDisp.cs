using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Threading;
using Cysharp.Threading.Tasks;

public class DamageDisp : MonoBehaviour
{
    public string DispText;
    private TextMeshProUGUI _tmp;
    public Vector3 InitialPos;

    [SerializeField] float _upTime;
    [SerializeField] float _stayTime;
    [SerializeField] float _fadeTime;
    [SerializeField] float _upOffset;


    // Start is called before the first frame update
    void Start()
    {
        InitialPos = this.transform.position;
        this.transform.position = InitialPos;
        _tmp = this.gameObject.GetComponent<TextMeshProUGUI>();
        _tmp.alpha = 1;
        FixedUpdateUnitask().Forget();
    }

    

    private async UniTaskVoid FixedUpdateUnitask()
    {

        float uptimer = 0;
        while (uptimer < _upTime)
        {
            uptimer += Time.fixedDeltaTime;
            this.transform.position= InitialPos+new Vector3(0,_upOffset*uptimer / _upTime,0);
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate,this.gameObject.GetCancellationTokenOnDestroy());
        }
        await UniTask.Delay((int)(_stayTime*1000f),false, PlayerLoopTiming.FixedUpdate, this.gameObject.GetCancellationTokenOnDestroy());
        float fadeTimer = 0;
        while (fadeTimer < _fadeTime)
        {
            fadeTimer += Time.fixedDeltaTime;
            _tmp.alpha = 1-fadeTimer / _fadeTime;
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, this.gameObject.GetCancellationTokenOnDestroy());
        }
        Destroy(this.gameObject);


    }

    private void setRectPosWithWorldPos(Vector3 WorldPos, RectTransform canvasRect)
    {
        InitialPos=RTransform_MyUtil.OverLayRectToScreen(WorldPos, canvasRect);

    } 

    
}
