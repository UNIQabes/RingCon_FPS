using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSettingSaveAndLoader : MonoBehaviour
{
    [SerializeField] string SettingName;
    [SerializeField] bool _enable=true;
    [SerializeField] bool _loadSettingOnStart=true;
    [SerializeField] bool _saveSettingOnDestroy= true;
    // Start is called before the first frame update
    void Start()
    {
        if (_loadSettingOnStart)
        {
            LoadSetting();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SaveSetting()
    {
        if (!_enable)
        {
            return;
        }
        PlayerDatas playerDatas = Resources.Load<PlayerDatas>($"UserSettings/{SettingName}");
        if (!playerDatas)
        {
            Debug.Log("Settingが存在しません");
        }
        playerDatas.SaveToPlayerPrefs();
        
    }

    void LoadSetting()
    {
        if (!_enable)
        {
            return;
        }
        PlayerDatas playerDatas = Resources.Load<PlayerDatas>($"UserSettings/{SettingName}");
        if (!playerDatas)
        {
            Debug.Log("Settingが存在しません");
        }
        playerDatas.LoadFromPlayerPrefs();
    }
    private void OnDestroy()
    {
        if (_saveSettingOnDestroy)
        {
            SaveSetting();
        }
    }
}
