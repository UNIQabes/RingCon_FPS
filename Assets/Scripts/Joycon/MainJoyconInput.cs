using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;



public class MainJoyconInput : Joycon_obs
{
    private static MainJoyconInput instance=null;
    private static MainJoyconInput getInstance
    {
        get
        {
            if (instance==null)
            {
                instance = new MainJoyconInput();
            }
            return instance;
        }
    }

    private static CancellationTokenSource cancellationTokenSourceOnAppQuit;
    private static CancellationToken cancellationTokenOnAppQuit;//Application終了時にCancellされるToken

    public static Quaternion JoyconPose_R { get; private set; }
    public static Quaternion SmoothedPose_R { get; private set; }

    public static Quaternion JoyconPose_R_Arrow { get; private set; }
    public static Quaternion SmoothedPose_R_Arrow { get; private set; }
    public static Quaternion JoyconPose_R_Ring { get; private set; }
    public static Quaternion SmoothedPose_R_Ring { get; private set; }
    public static Quaternion JoyconPose_R_Remote { get; private set; }
    public static Quaternion SmoothedPose_R_Remote { get; private set; }
    public static float ringconStrain { get; private set; }
    public static bool RButton { get; private set; }
    public static bool ZRButton { get; private set; }
    public static bool AButton { get; private set; }

    //以下の2つはJoyconに搭載されたIMUにおける座標系(Nintend Switch Reverse Engeneeringのimu_sensor_notes>Axes definitionに書いてある)で、自分が指定したい正面方向と下方向を指定する
    //public static Vector3 FrontVector_R = new Vector3(0, 0, 1);
    public static Vector3 FrontVector_R = new Vector3(1, 0, 0);
    public static Vector3 DownWardVector_R = new Vector3(0, 1, 0);

    public static Vector3 FrontVector_R_Arrow = new Vector3(1, 0, 0);
    public static Vector3 DownWardVector_R_Arrow = new Vector3(0, 1, 0);
    public static Vector3 FrontVector_R_Ring = new Vector3(0, 0, 1);
    public static Vector3 DownWardVector_R_Ring = new Vector3(0, 1, 0);
    public static Vector3 FrontVector_R_Remote = new Vector3(1, 0, 0);
    public static Vector3 DownWardVector_R_Remote = new Vector3(0, 0, 1);

    //MainのJoyConのシリアルナンバー 接続しているJoyCon、もしくは接続していないときは優先して登録するJoyCon。空の文字列の時は好きに登録すれば良い。
    public static string SerialNumber_R { get; private set; } = "";
    //MainのJoyConのJoyConConnection nullでないなら、このJoyConConnectionに登録している
    private static JoyConConnection _joyconConnection_R;

    public static JoyConConnectInfo ConnectInfo = JoyConConnectInfo.JoyConIsNotFound;

    private static List<byte[]> _inputReportsInThisFrame = null;//コルーチンでも実装できそう
    private static bool _isReadyPolling = false;


    private static float GyroXCalibration=0;
    private static float GyroYCalibration=0;
    private static float GyroZCalibration=0;
   
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        Joycon_subj.RegisterAfterInitCallback(AfterSubjInitCallback);

        /*
        _joyconConnection_R = null;
        cancellationTokenSourceOnAppQuit = new CancellationTokenSource();
        cancellationTokenOnAppQuit = cancellationTokenSourceOnAppQuit.Token;
        Joycon_subj.UpdateJoyConConnection();
        IsTryingReconnectJoycon = true;

        JoyconPose_R = Quaternion.identity;
        SmoothedPose_R = Quaternion.identity;
        JoyconPose_R_Arrow = Quaternion.identity;
        SmoothedPose_R_Arrow = Quaternion.identity;
        JoyconPose_R_Ring = Quaternion.identity;
        SmoothedPose_R_Ring = Quaternion.identity;
        JoyconPose_R_Remote= Quaternion.identity;
        SmoothedPose_R_Remote = Quaternion.identity;
        ringconStrain = 0;
        Application.quitting += OnApplicatioQuitStatic;
        updatestatic().Forget();
        reconnectTask().Forget();
        fixedupdatestatic().Forget();
        */
    }

    static void AfterSubjInitCallback()
    {
        _joyconConnection_R = null;
        cancellationTokenSourceOnAppQuit = new CancellationTokenSource();
        cancellationTokenOnAppQuit = cancellationTokenSourceOnAppQuit.Token;
        Joycon_subj.UpdateJoyConConnection();
        IsTryingReconnectJoycon = true;
        /*
        List<string> joyconRKeys = Joycon_subj.GetJoyConSerialNumbers_R();
        JoyConConnection newJoyConConnection = null;
        
        if (joyconRKeys.Count > 0)
        {
            SerialNumber_R = joyconRKeys[0];
            _joyconConnection_R = Joycon_subj.GetJoyConConnection(SerialNumber_R);
            _joyconConnection_R.AddObserver(getInstance);
            _joyconConnection_R.ConnectToJoyCon();
            Debug.Log("Joyconが見つかった！");
            ConnectInfo = JoyConConnectInfo.SettingUpJoycon;


            joyConSetUp(cancellationTokenOnAppQuit).Forget();
        }
        */

        JoyconPose_R = Quaternion.identity;
        SmoothedPose_R = Quaternion.identity;
        JoyconPose_R_Arrow = Quaternion.identity;
        SmoothedPose_R_Arrow = Quaternion.identity;
        JoyconPose_R_Ring = Quaternion.identity;
        SmoothedPose_R_Ring = Quaternion.identity;
        JoyconPose_R_Remote = Quaternion.identity;
        SmoothedPose_R_Remote = Quaternion.identity;
        ringconStrain = 0;
        Application.quitting += OnApplicatioQuitStatic;
        updatestatic().Forget();
        reconnectTask().Forget();
        fixedupdatestatic().Forget();
    }




    static async UniTaskVoid updatestatic()
    {
        while (!cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            /*
            SmoothedPose_R = Quaternion.Slerp(SmoothedPose_R, JoyconPose_R, 0.05f);
            SmoothedPose_R_Arrow = Quaternion.Slerp(SmoothedPose_R, JoyconPose_R_Arrow, 0.05f);
            SmoothedPose_R_Ring = Quaternion.Slerp(SmoothedPose_R, JoyconPose_R_Ring, 0.05f);
            */
            DebugOnGUI.Log($"{SerialNumber_R} {ConnectInfo}", "ConnectInfo");
            if (_joyconConnection_R!=null&&!_joyconConnection_R.IsConnecting)
            {
                if (ConnectInfo != JoyConConnectInfo.JoyConIsNotFound)
                {
                    DebugOnGUI.Log("接続が切れたよ", "Joycon");
                    Debug.Log("接続が切れたよ!!!");
                }
                ConnectInfo = JoyConConnectInfo.JoyConIsNotFound;
            }
            await UniTask.Yield(PlayerLoopTiming.EarlyUpdate,cancellationTokenOnAppQuit);
        }
        
    }

    static async UniTaskVoid fixedupdatestatic()
    {
        while (!cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            //ResetYRot_xyOrder();
            SmoothedPose_R = Quaternion.Slerp(SmoothedPose_R, JoyconPose_R, 0.05f);
            SmoothedPose_R_Arrow = Quaternion.Slerp(SmoothedPose_R_Arrow, JoyconPose_R_Arrow, 0.05f);
            SmoothedPose_R_Ring = Quaternion.Slerp(SmoothedPose_R_Ring, JoyconPose_R_Ring, 0.05f);
            
            
            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, cancellationTokenOnAppQuit);
        }
        
    }

    public static bool IsTryingReconnectJoycon;
    static async UniTaskVoid reconnectTask()
    {

        while (!cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            if (ConnectInfo!=JoyConConnectInfo.JoyConIsReady & IsTryingReconnectJoycon)
            {
                Debug.Log("再接続を試行します");
                DebugOnGUI.Log("再接続を試行します", "Joycon");
                bool connectIsSuccess=await ReConnectJoyconAsync();
            }
            await UniTask.Delay(1000,false,PlayerLoopTiming.EarlyUpdate,cancellationTokenOnAppQuit);
        }
    }

    private static void OnApplicatioQuitStatic()
    {

        cancellationTokenSourceOnAppQuit.Cancel();
        
    }

    


    

    //テスト
    public static void SubCmdQueing()
    {
        Debug.Log("0");
        if (_joyconConnection_R == null)
        {
            Debug.Log("nullだ");
        }
    }

    public static async UniTaskVoid SendSubCmd()
    {
        byte[] buf = new byte[50];
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x03, 0x30 }, buf, cancellationTokenOnAppQuit);
    }

    public static void asyncTrigger()
    {
        SendSubCmd().Forget();
    }

    public static void ReconnectTrigger()
    {
        ReConnectJoyconAsync().Forget();
    }
    //テスト(終わり)

    //JoyConを接続し直す
    public static async UniTask<bool> ReConnectJoyconAsync()
    {
        //以前登録していたJoyConConnectionへの登録を解除 二重にJoyconConnectionに登録するのを防ぐ
        if (_joyconConnection_R != null)
        {
            _joyconConnection_R.DelObserver(getInstance);
            _joyconConnection_R = null;
        }
        string newJoyConSerialNum = "";
        Joycon_subj.UpdateJoyConConnection();
        List<string> joyconRKeys = Joycon_subj.GetJoyConSerialNumbers_R();
        foreach (string aJoyconSerialNum in joyconRKeys)
        {
            newJoyConSerialNum = aJoyconSerialNum;
            //以前接続していたJoyConに優先的に繋ぐ
            if (newJoyConSerialNum == SerialNumber_R)
            {
                break;
            }
        }


        if (newJoyConSerialNum != "")
        {
            JoyConConnection newJoyConConnection = Joycon_subj.GetJoyConConnection(newJoyConSerialNum);
            //DebugOnGUI.Log(newJoyConSerialNum, "dedwedsfef");
            if (newJoyConConnection.ConnectToJoyCon())
            {
                //ConnectInfo = JoyConConnectInfo.SettingUpJoycon;
                newJoyConConnection.AddObserver(getInstance);
                _joyconConnection_R = newJoyConConnection;
                SerialNumber_R = newJoyConSerialNum;
                await joyConSetUp(cancellationTokenOnAppQuit);
            }
            else
            {
                Debug.Log("接続できなかった!!");
            }

        }

        JoyconPose_R = Quaternion.identity;
        SmoothedPose_R = Quaternion.identity;
        ringconStrain = 0;
        return true;
    }

    



    private static async UniTask joyConSetUp(CancellationToken cancellationToken)
    {


        //await UniTask.DelayFrame(100, cancellationToken: cancellationToken);
        //実行コンテクスト(?というらしい)をPreUpdateに切り替える
        await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationTokenOnAppQuit);

        byte[] ReplyBuf = new byte[50];

        
        try
        {
            ConnectInfo = JoyConConnectInfo.SettingUpJoycon;
            Debug.Log("セットアップします!");
            DebugOnGUI.Log("セットアップ開始", "Joycon");
            // Enable vibration
            DebugOnGUI.Log($"{SerialNumber_R}:Enable vibration", "Joycon");
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x48, 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Enable IMU data", "Joycon");
            // Enable IMU data
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x40, 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Set input report mode to 0x30", "Joycon");
            //Set input report mode to 0x30
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x03, 0x30 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Enable MCU data", "Joycon");
            // Enabling MCU data
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x22, 0x01 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Enable MCU data", "Joycon");
            //enabling_MCU_data_21_21_1_1
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[39] { 0x21, 0x21, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Get external data", "Joycon");
            //get_ext_data_59
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x59, 0x0 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Get external device in format config 5C", "Joycon");
            //get_ext_dev_in_format_config_5C
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x5C, 0x06, 0x03, 0x25, 0x06, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x16, 0xED, 0x34, 0x36, 0x00, 0x00, 0x00, 0x0A, 0x64, 0x0B, 0xE6, 0xA9, 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xA8, 0xE1, 0x34, 0x36 }, ReplyBuf, cancellationTokenOnAppQuit);

            DebugOnGUI.Log($"{SerialNumber_R}:Start external polling 5A", "Joycon");
            //start_external_polling_5A
            await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x5A, 0x04, 0x01, 0x01, 0x02 }, ReplyBuf, cancellationTokenOnAppQuit);

            ConnectInfo = JoyConConnectInfo.JoyConIsReady;
            Debug.Log($"{SerialNumber_R}:Joyconのセットアップが完了しました");
            DebugOnGUI.Log("Joyconのセットアップが完了しました", "Joycon");
        }
        catch(OperationCanceledException e)
        {
            ConnectInfo = JoyConConnectInfo.JoyConIsNotFound;
            DebugOnGUI.Log("JoyConのセットアップに失敗しました", "Joycon");
            Debug.Log("JoyConのセットアップに失敗しました。");
        }

    }

    public static async UniTaskVoid SetupAgain()
    {
        if (ConnectInfo == JoyConConnectInfo.JoyConIsReady)
        {
            await joyConSetUp(cancellationTokenOnAppQuit);
        }
    }

    public override void OnReadReport(List<byte[]> reports)
    {
        _inputReportsInThisFrame = reports;
        int x30ReportNum = 0;
        foreach (byte[] report in reports)
        {
            if (report != null && report.Length >= 37)
            {
                if (report[0] == 0x30)
                {
                    x30ReportNum++;
                }
            }
        }
        if (x30ReportNum == 0)
        {
            return;
        }

        foreach (byte[] report in reports)
        {
            if (report != null && report.Length >= 37)
            {
                if (report[0] == 0x30)
                {
                    float sec = Time.deltaTime / x30ReportNum;
                    ringconStrain = BitConverter.ToInt16(report, 39);
                    float gyro_x1 = 0.070f * (float)BitConverter.ToInt16(report, 19) ;
                    float gyro_y1 = 0.070f * (float)BitConverter.ToInt16(report, 21) ;
                    float gyro_z1 = 0.070f * (float)BitConverter.ToInt16(report, 23) ;
                    float acc_x1 = 0.000244f * (float)BitConverter.ToInt16(report, 13);
                    float acc_y1 = 0.000244f * (float)BitConverter.ToInt16(report, 15);
                    float acc_z1 = 0.000244f * (float)BitConverter.ToInt16(report, 17);
                    applyIMUData(new Vector3(acc_x1, acc_y1, acc_z1), new Vector3(gyro_x1, gyro_y1, gyro_z1), sec / 2);
                    

                    float gyro_x2 = 0.070f * (float)BitConverter.ToInt16(report, 31);
                    float gyro_y2 = 0.070f * (float)BitConverter.ToInt16(report, 33);
                    float gyro_z2 = 0.070f * (float)BitConverter.ToInt16(report, 35);
                    float acc_x2 = 0.000244f * (float)BitConverter.ToInt16(report, 25);
                    float acc_y2 = 0.000244f * (float)BitConverter.ToInt16(report, 27);
                    float acc_z2 = 0.000244f * (float)BitConverter.ToInt16(report, 29);
                    applyIMUData(new Vector3(acc_x2, acc_y2, acc_z2), new Vector3(gyro_x2, gyro_y2, gyro_z2), sec / 2);
                    byte buttonStatus = report[3];
                    RButton = (buttonStatus & 0b01000000) > 0;
                    ZRButton = (buttonStatus & 0b10000000) > 0;
                    AButton = (buttonStatus & 0b00001000) > 0;
                }
            }
            
        }


    }

    


    public static void applyIMUData(Vector3 accV, Vector3 gyroV, float sec)
    {
        (JoyconPose_R_Arrow,SmoothedPose_R_Arrow) =applyIMUData(accV,gyroV,sec,DownWardVector_R_Arrow,FrontVector_R_Arrow, JoyconPose_R_Arrow, SmoothedPose_R_Arrow);
        (JoyconPose_R_Ring, SmoothedPose_R_Ring) = applyIMUData(accV, gyroV, sec, DownWardVector_R_Ring, FrontVector_R_Ring, JoyconPose_R_Ring, SmoothedPose_R_Ring);
        (JoyconPose_R, SmoothedPose_R) = applyIMUData(accV, gyroV, sec, DownWardVector_R, FrontVector_R, JoyconPose_R, SmoothedPose_R);
        (JoyconPose_R_Remote, SmoothedPose_R_Remote) = applyIMUData(accV, gyroV, sec, DownWardVector_R_Remote, FrontVector_R_Remote, JoyconPose_R_Remote, SmoothedPose_R_Remote);
    }

    private static (Quaternion pose,Quaternion smoothedPose) applyIMUData(Vector3 accV, Vector3 gyroV, float sec,
        Vector3 downwardVector,Vector3 frontVector, Quaternion prevPose, Quaternion prevSmoothedPose)
    {
        Quaternion retPose = prevPose;
        Quaternion retSmoothedPose = prevSmoothedPose;
        float gyro_x = gyroV.x+GyroXCalibration;
        float gyro_y = gyroV.y + GyroYCalibration;
        float gyro_z = gyroV.z + GyroZCalibration;
        Quaternion acRoll = Quaternion.AngleAxis(Mathf.Sqrt(gyro_x * gyro_x + gyro_y * gyro_y + gyro_z * gyro_z) * sec, retPose * new Vector3(gyro_x, gyro_y, gyro_z));
        //Quaternion acRoll1 = argJoyconPose * Quaternion.Euler(new Vector3(gyro_x1, gyro_y1, gyro_z1) * sec / 2);
        retPose = acRoll * retPose;
        retSmoothedPose = acRoll * retSmoothedPose;
        /*
        float acc_x = accV.x;
        float acc_y = accV.y;
        float acc_z = accV.z;
        */

        float acc_mag1 = accV.magnitude;
        if (Mathf.Abs(1 - (acc_mag1)) < 0.001f)
        {
            Vector3 gravityV = -accV;
            //Debug.Log($"補正あり G:{acc_mag1}");
            //Vector3 acc_angle = new Vector3(Mathf.PI - Mathf.Atan2(acc_z, -Mathf.Sqrt(acc_x * acc_x + acc_y * acc_y)), 0, -Mathf.PI / 2 - Mathf.Atan2(acc_y, acc_x));
            //Quaternion accPose = Quaternion.Euler(acc_angle * 180 / Mathf.PI);
            Quaternion accPose = V3_MyUtil.RotateV2V(gravityV, downwardVector);
            Vector3 gyro_frontV = retPose * frontVector.normalized;
            Vector3 acc_frontV = accPose * frontVector.normalized;
            //accPose = Quaternion.AngleAxis((Mathf.Atan2(gyro_frontV.x, gyro_frontV.z) - Mathf.Atan2(acc_frontV.x, acc_frontV.z)) * 180 / Mathf.PI, new Vector3(0, 1, 0)) * accPose;
            Vector3 gyroFront_DownVVertical = V3_MyUtil.GetVerticalComp(gyro_frontV, downwardVector).normalized;
            Vector3 accFront_DownVVertical = V3_MyUtil.GetVerticalComp(acc_frontV, downwardVector).normalized;
            accPose = V3_MyUtil.RotateV2V(accFront_DownVVertical, gyroFront_DownVVertical) * accPose;
            retPose = accPose;
        }
        return (retPose, retSmoothedPose);
    }

    


    private static async UniTask waitSubCommandReply(CancellationToken cancellationToken)
    {
        bool isSentReply = false;
        while (!isSentReply)
        {
            if (_inputReportsInThisFrame != null)
            {
                foreach (byte[] aInputReport in _inputReportsInThisFrame)
                {
                    if (aInputReport[0] == 0x21)
                    {
                        Debug.Log($"get subcommand reply  {(aInputReport[13] >= 0x80 ? "ACK" : "NACK")}  ID:{aInputReport[14]}");
                        isSentReply = true;
                        break;
                    }
                }
            }

            await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);
        }

    }

    public static void RotatZRot(float RotDeg)
    {
        Vector3 axis= new Vector3(0, 1, 0);
        JoyconPose_R_Arrow = Quaternion.AngleAxis(RotDeg, axis) * JoyconPose_R_Arrow;
        SmoothedPose_R_Arrow = Quaternion.AngleAxis(RotDeg, axis) * SmoothedPose_R_Arrow;
    }

    public static void ResetYRot_xyOrder()
    {
        
        Vector3 joycon_frontV = JoyconPose_R_Arrow * FrontVector_R_Arrow.normalized;
        Vector3 Smoothed_frontV = SmoothedPose_R_Arrow * FrontVector_R_Arrow.normalized;
        //accPose = Quaternion.AngleAxis((Mathf.Atan2(gyro_frontV.x, gyro_frontV.z) - Mathf.Atan2(acc_frontV.x, acc_frontV.z)) * 180 / Mathf.PI, new Vector3(0, 1, 0)) * accPose;
        Vector3 joyconFront_DownVVertical = V3_MyUtil.GetVerticalComp(joycon_frontV, DownWardVector_R_Arrow).normalized;
        Vector3 smoothedFront_DownVVertical = V3_MyUtil.GetVerticalComp(Smoothed_frontV, DownWardVector_R_Arrow).normalized;
        Vector3 FrontVector = new Vector3(1,0,0);
        JoyconPose_R_Arrow=V3_MyUtil.RotateV2V(joyconFront_DownVVertical, FrontVector_R_Arrow) * JoyconPose_R_Arrow;
        SmoothedPose_R_Arrow = V3_MyUtil.RotateV2V(smoothedFront_DownVVertical, FrontVector_R_Arrow) * SmoothedPose_R_Arrow;


        Vector3 joycon_frontV_Remote = JoyconPose_R_Remote * FrontVector_R_Remote.normalized;
        Vector3 Smoothed_frontV_Remote = SmoothedPose_R_Remote * FrontVector_R_Remote.normalized;
        //accPose = Quaternion.AngleAxis((Mathf.Atan2(gyro_frontV.x, gyro_frontV.z) - Mathf.Atan2(acc_frontV.x, acc_frontV.z)) * 180 / Mathf.PI, new Vector3(0, 1, 0)) * accPose;
        Vector3 joyconFront_DownVVertical_Remote = V3_MyUtil.GetVerticalComp(joycon_frontV_Remote, DownWardVector_R_Remote).normalized;
        Vector3 smoothedFront_DownVVertical_Remote = V3_MyUtil.GetVerticalComp(Smoothed_frontV_Remote, DownWardVector_R_Remote).normalized;
        Vector3 FrontVector_Remote = new Vector3(1, 0, 0);
        JoyconPose_R_Remote = V3_MyUtil.RotateV2V(joyconFront_DownVVertical_Remote, FrontVector_R_Remote) * JoyconPose_R_Remote;
        SmoothedPose_R_Remote = V3_MyUtil.RotateV2V(smoothedFront_DownVVertical_Remote, FrontVector_R_Remote) * SmoothedPose_R_Remote;
    }
    public static async  UniTaskVoid SetCalibrationWhenStaticCondition()
    {
        Debug.Log("キャリブレーション開始");

        int counter = 0;
        Vector3 gyroVInStaticCondition = new Vector3(0,0,0);
        float threshold = 0.5f;
        while (counter <= 6)
        {
            if (ConnectInfo != JoyConConnectInfo.JoyConIsReady)
            {
                Debug.Log("キャリブレーション設定中止");
                return;
            }
            if (_inputReportsInThisFrame != null && _inputReportsInThisFrame.Count > 0)
            {

                foreach (byte[] aInputReport in _inputReportsInThisFrame)
                {
                    float gyro_x1 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 19);
                    float gyro_y1 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 21);
                    float gyro_z1 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 23);
                    Vector3 gyroV1 = new Vector3(gyro_x1, gyro_y1, gyro_z1);
                    if ((gyroVInStaticCondition - gyroV1).magnitude < threshold) { counter++; }
                    else { counter = 0; gyroVInStaticCondition = gyroV1; /*Debug.Log("リセット!");*/ }

                    float gyro_x2 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 31);
                    float gyro_y2 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 33);
                    float gyro_z2 = 0.070f * (float)BitConverter.ToInt16(aInputReport, 35);
                    Vector3 gyroV2 = new Vector3(gyro_x2, gyro_y2, gyro_z2);
                    if ((gyroVInStaticCondition - gyroV2).magnitude < threshold) { counter++; }
                    else { counter = 0; gyroVInStaticCondition = gyroV2; /*Debug.Log("リセット!");*/ }
                }
            }
            else
            {
                //Debug.Log("ぬるぬる");
            }
            await UniTask.Yield(cancellationTokenOnAppQuit);
        }
        
        GyroXCalibration = -gyroVInStaticCondition.x;
        GyroYCalibration = -gyroVInStaticCondition.y;
        GyroZCalibration = -gyroVInStaticCondition.z;
        Debug.Log($"キャリブレーション設定完了:{new Vector3(GyroXCalibration, GyroYCalibration, GyroZCalibration)}(deg/s)") ;
    }

    

    

}

public enum JoyConConnectInfo
{
    JoyConIsNotFound,
    SettingUpJoycon,
    JoyConIsReady
}
