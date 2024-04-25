using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCloseButton : MonoBehaviour
{
    [SerializeField] GameObject _menuPanelGObj;
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
        _menuPanelGObj.SetActive(false);
        Time.timeScale = 1;
    }
}
