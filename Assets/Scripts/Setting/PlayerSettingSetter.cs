using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingSetter : MonoBehaviour
{
    [SerializeField] ShootingController _shootingController;
    [SerializeField] TimeAttackRecordDisp timeAttackLv1RecordDisp;
    [SerializeField] TimeAttackRecordDisp timeAttackLv2RecordDisp;
    [SerializeField] ScoreAttackRecordDisp scoreAttackLv1RecordDisp;
    [SerializeField] ScoreAttackRecordDisp scoreAttackLv2RecordDisp;
    [SerializeField] ScoreRanking scoreRanking1;
    [SerializeField] ScoreRanking scoreRanking2;
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

        MainJoyconInput.GyroXCalibration = playerSettingData.GyroXCalibration;
        MainJoyconInput.GyroYCalibration = playerSettingData.GyroYCalibration;
        MainJoyconInput.GyroZCalibration = playerSettingData.GyroZCalibration;


        //PlayerDataをセット
        if (_shootingController)
        {
            _shootingController.MaxXRot_xyOrder_deg = playerSettingData.MaxXRot_xyOrder_deg;
            _shootingController.MaxYRot_xyOrder_deg = playerSettingData.MaxYRot_xyOrder_deg;
            _shootingController.XRot_xyOrder_ResetYRot_deg=-playerSettingData.MaxXRot_xyOrder_deg;
            _shootingController.XRot_Offset_xyOrder_deg = playerSettingData.XRotOffset_xyOrder_deg;
            _shootingController.GyroDriftCalibration_YRot_xyOrder = playerSettingData.GyroDriftCalibration_YRot_xyOrder;
            _shootingController.RingconPushThreshold = playerSettingData.RingconThreshold_Push;
            _shootingController.RingconModerateStarin = playerSettingData.RingconStrain_mod;
            _shootingController.RingconPullThreshold = playerSettingData.RingconThreshold_Pull;
            _shootingController.ControllerMode = playerSettingData.ControllerMode;

            _shootingController.XSensetivity_KeyMouseMode = playerSettingData.XSensitivity;
            _shootingController.YSensetivity_KeyMouseMode = playerSettingData.YSensitivity;




        }
        if (timeAttackLv1RecordDisp)
        {
            timeAttackLv1RecordDisp.RecordTime = playerSettingData.Time_TimeAttack1;
            timeAttackLv1RecordDisp.RecordRank = playerSettingData.Rank_TimeAttack1;
        }
        if (timeAttackLv2RecordDisp)
        {
            timeAttackLv2RecordDisp.RecordTime = playerSettingData.Time_TimeAttack2;
            timeAttackLv2RecordDisp.RecordRank = playerSettingData.Rank_TimeAttack2;
        }
        if (scoreAttackLv1RecordDisp)
        {
            scoreAttackLv1RecordDisp.RecordScore = playerSettingData.Score_ScoreAttack1;
            scoreAttackLv1RecordDisp.RecordRank = playerSettingData.Rank_ScoreAttack1;
        }
        if (scoreAttackLv2RecordDisp)
        {
            scoreAttackLv2RecordDisp.RecordScore = playerSettingData.Score_ScoreAttack2;
            scoreAttackLv2RecordDisp.RecordRank = playerSettingData.Rank_ScoreAttack2;
        }
        if (scoreRanking1)
        {
            scoreRanking1.ScoreList = new List<int>(playerSettingData.ScoreRanking1);
        }
        if (scoreRanking2)
        {
            scoreRanking2.ScoreList = new List<int>(playerSettingData.ScoreRanking2);
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
