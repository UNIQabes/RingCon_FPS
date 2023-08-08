using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingCloseButton : MonoBehaviour
{

    [SerializeField] GameObject SettingPanelGObj;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClicked()
    {
        SettingPanelGObj.SetActive(false);
    }
}
