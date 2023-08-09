using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingSetter : MonoBehaviour
{
    [SerializeField] ShootingController _shootingController;
    [SerializeField] string _settingName;
    [SerializeField] bool _enable = true;
    [SerializeField] bool _setOnStart = true;
    string _curSettingName="";
    PlayerDatas _playerSettingData;

    // Start is called before the first frame update
    void Start()
    {
        _curSettingName = "";
        _playerSettingData = null;
        if (_setOnStart)
        {
            SetSetting();
        }
    }

    

    private PlayerDatas getSettingFromBufOrResource(string settingName)
    {
        if (_curSettingName!= settingName)
        {
            _curSettingName = settingName;
            _playerSettingData= Resources.Load<PlayerDatas>($"UserSettings/{_settingName}");
        }
        return _playerSettingData;
    }

    public void SetSetting()
    {
        if (!_enable)
        {
            return;
        }
        PlayerDatas playerSettingData = getSettingFromBufOrResource(_settingName);
        if (!playerSettingData)
        {
            Debug.Log("セッティングが存在しません");
            return;
        }

        //PlayerDataをセット
        if (_shootingController)
        {
            _shootingController.MaxXRot_xyOrder_deg = playerSettingData.MaxXRot_xyOrder_deg;
            _shootingController.MaxYRot_xyOrder_deg = playerSettingData.MaxYRot_xyOrder_deg;
            _shootingController.XRot_xyOrder_ResetYRot_deg=-playerSettingData.MaxXRot_xyOrder_deg;
            _shootingController.GyroDriftCalibration_YRot_xyOrder = playerSettingData.GyroDriftCalibration_YRot_xyOrder;
            _shootingController.ControllerMode = playerSettingData.ControllerMode;
            _shootingController.XSensetivity_KeyMouseMode = playerSettingData.XSensitivity;
            _shootingController.YSensetivity_KeyMouseMode = playerSettingData.YSensitivity;

        }
    }

    public PlayerDatas GetSetting()
    {
        PlayerDatas playerSettingData = getSettingFromBufOrResource(_settingName);
        if (!playerSettingData)
        {
            Debug.Log("セッティングが存在しません");
        }
        return playerSettingData;

    }
}
