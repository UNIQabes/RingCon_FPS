using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HittingGameMark : MonoBehaviour
{
    [SerializeField]HittingGameRuler _hittingGameRuler;

    private void Start()
    {
        if (!_hittingGameRuler)
        {
            _hittingGameRuler = GameObject.Find("GM").GetComponent<HittingGameRuler>();
        }
    }
    // Start is called before the first frame update
    public void OnMarkDestroyed()
    {
        //Debug.Log("破壊！破壊 ！");
        _hittingGameRuler.OnMarkDestroyed(this.gameObject);
        Destroy(this.gameObject);
    }
}
