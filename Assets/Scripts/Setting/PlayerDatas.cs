using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "PlayerSetting", menuName = "ScriptableObjects/PlayerDatas")]
public class PlayerDatas : ScriptableObject
{
    //Scores
    public string Rank_HittingStage1 = "C";
    public float Score_HittingStage1 = 0;
    public string Rank_HittingStage2 = "C";
    public float Score_HittingStage2 = 0;
    public string Rank_HittingStage3 = "C";
    public float Score_HittingStage3 = 0;
    public string Rank_ScoreAttack1 = "C";
    public float Score_ScoreAttack1 = 0;
    public string Rank_ScoreAttack2 = "C";
    public float Score_ScoreAttack2 = 0;
    public string Rank_TimeAttack1 = "C";
    public float Time_TimeAttack1 = 999;
    public string Rank_TimeAttack2 = "C";
    public float Time_TimeAttack2 = 999;


    public ShootingControllerMode ControllerMode = ShootingControllerMode.KeyMouse;

    //JoyConSettings
    public int MaxXRot_xyOrder_deg = 30;
    public int MaxYRot_xyOrder_deg = 45;
    public int XRotOffset_xyOrder_deg = 0;
    public float GyroDriftCalibration_YRot_xyOrder=0;
    public float GyroXCalibration=0;
    public float GyroYCalibration=0;
    public float GyroZCalibration=0;
    public int RingconThreshold_Pull = 0;
    public int RingconStrain_mod = 0;
    public int RingconThreshold_Push = 0;

    //KeyMouseSettings
    public int XSensitivity=0;
    public int YSensitivity=0;

    [ContextMenu("LoadFromPlayerPrefs")]
    public void LoadFromPlayerPrefs()
    {
        Rank_HittingStage1=PlayerPrefs.GetString("Rank_HittingStage1","C");
        Rank_HittingStage2 = PlayerPrefs.GetString("Rank_HittingStage2", "C");
        Rank_HittingStage3 = PlayerPrefs.GetString("Rank_HittingStage3", "C");
        Rank_ScoreAttack1=PlayerPrefs.GetString("Rank_ScoreAttack1", "C");
        Rank_ScoreAttack2=PlayerPrefs.GetString("Rank_ScoreAttack2", "C");
        Rank_TimeAttack1=PlayerPrefs.GetString("Rank_TimeAttack1", "C");
        Rank_TimeAttack2=PlayerPrefs.GetString("Rank_TimeAttack2", "C");

        Score_HittingStage1 = PlayerPrefs.GetFloat("Score_HittingStage1", 0);
        Score_HittingStage2 = PlayerPrefs.GetFloat("Score_HittingStage2", 0);
        Score_HittingStage3 = PlayerPrefs.GetFloat("Score_HittingStage3", 0);
        Score_ScoreAttack1=PlayerPrefs.GetFloat("Score_ScoreAttack1", 0);
        Score_ScoreAttack2=PlayerPrefs.GetFloat("Score_ScoreAttack2", 0);
        Time_TimeAttack1=PlayerPrefs.GetFloat("Time_TimeAttack1", 999);
        Time_TimeAttack2=PlayerPrefs.GetFloat("Time_TimeAttack2", 999);

        MaxXRot_xyOrder_deg = PlayerPrefs.GetInt("MaxXRot_xyOrder_deg", 30);
        MaxYRot_xyOrder_deg = PlayerPrefs.GetInt("MaxYRot_xyOrder_deg", 45);
        XRotOffset_xyOrder_deg = PlayerPrefs.GetInt("XRotOffset_xyOrder_deg", 0);
        GyroDriftCalibration_YRot_xyOrder= PlayerPrefs.GetFloat("GyroDriftCalibration_YRot_xyOrder", 0);
        GyroXCalibration=PlayerPrefs.GetFloat("GyroXCalibration",0);
        GyroYCalibration=PlayerPrefs.GetFloat("GyroYCalibration", 0);
        GyroZCalibration=PlayerPrefs.GetFloat("GyroZCalibration", 0);
        RingconThreshold_Pull = PlayerPrefs.GetInt("RingconThreshold_Pull", 3800);
        RingconStrain_mod = PlayerPrefs.GetInt("RingconStrain_mod", 4330);
        RingconThreshold_Push = PlayerPrefs.GetInt("RingconThreshold_Push", 5000);

        ControllerMode = (ShootingControllerMode)PlayerPrefs.GetInt("ControllerMode", 0);

        XSensitivity = PlayerPrefs.GetInt("XSensitivty", 0);
        YSensitivity = PlayerPrefs.GetInt("YSensitivity", 0);
    }

    [ContextMenu("SaveToPlayerPrefs")]
    public void SaveToPlayerPrefs()
    {
        PlayerPrefs.SetString("Rank_HittingStage1", Rank_HittingStage1);
        PlayerPrefs.SetString("Rank_HittingStage2", Rank_HittingStage2);
        PlayerPrefs.SetString("Rank_HittingStage3", Rank_HittingStage3);
        PlayerPrefs.SetString("Rank_ScoreAttack1", Rank_ScoreAttack1);
        PlayerPrefs.SetString("Rank_ScoreAttack2", Rank_ScoreAttack2);
        PlayerPrefs.SetString("Rank_TimeAttack1", Rank_TimeAttack1);
        PlayerPrefs.SetString("Rank_TimeAttack2", Rank_TimeAttack2);


        PlayerPrefs.SetFloat("Score_HittingStage1", Score_HittingStage1);
        PlayerPrefs.SetFloat("Score_HittingStage2", Score_HittingStage2);
        PlayerPrefs.SetFloat("Score_HittingStage3", Score_HittingStage3);
        PlayerPrefs.SetFloat("Score_ScoreAttack1", Score_ScoreAttack1);
        PlayerPrefs.SetFloat("Score_ScoreAttack2", Score_ScoreAttack2);
        PlayerPrefs.SetFloat("Time_TimeAttack1", Time_TimeAttack1);
        PlayerPrefs.SetFloat("Time_TimeAttack2", Time_TimeAttack2);

        PlayerPrefs.SetInt("MaxXRot_xyOrder_deg", MaxXRot_xyOrder_deg);
        PlayerPrefs.SetInt("MaxYRot_xyOrder_deg", MaxYRot_xyOrder_deg);
        PlayerPrefs.SetInt("XRotOffset_xyOrder_deg", XRotOffset_xyOrder_deg);
        PlayerPrefs.SetFloat("GyroDriftCalibration_YRot_xyOrder", GyroDriftCalibration_YRot_xyOrder);
        PlayerPrefs.SetFloat("GyroXCalibration", GyroXCalibration);
        PlayerPrefs.SetFloat("GyroYCalibration", GyroYCalibration);
        PlayerPrefs.SetFloat("GyroZCalibration", GyroZCalibration);
        PlayerPrefs.SetInt("RingconThreshold_Pull", 3800);
        PlayerPrefs.SetInt("RingconStrain_mod", 4330);
        PlayerPrefs.SetInt("RingconThreshold_Push", 5000);


        PlayerPrefs.SetInt("XSensitivty", XSensitivity);
        PlayerPrefs.SetInt("YSensitivity", YSensitivity);

        PlayerPrefs.SetInt("ControllerMode", (int)ControllerMode);

        PlayerPrefs.Save();
    }

    

    public void OnValidate()
    {
        //SaveToPlayerPrefs();
    }

    
}


