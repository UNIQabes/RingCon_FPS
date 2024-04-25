using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class JoyconAlert : MonoBehaviour
{
    private TextMeshProUGUI _tmp;
    // Start is called before the first frame update
    void Start()
    {
        _tmp = GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.JoyConIsNotFound)
        {
            _tmp.text ="connect Joycon with bluetooth";
        }
        else if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.SettingUpJoycon)
        {
            _tmp.text = "Sending Output Report";
        }
        else if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.JoyConIsReady)
        {
            _tmp.text = "connection establishment,";
        }
    }
}
