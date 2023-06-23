using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
public class MainJoyconInput : Joycon_obs
{
    private CancellationToken _cancellationToken;//Application終了時にCancellされるToken

    public static Quaternion JoyconPose_R { get; private set; }
    public static Quaternion SmoothedPose_R { get; private set; }
    public static float ringconStrain { get; private set; }
    //以下の2つはJoyconに搭載されたIMUにおける座標系(Nintend Switch Reverse Engeneeringのimu_sensor_notes>Axes definitionに書いてある)で、自分が指定したい正面方向と下方向を指定する
    public static Vector3 FrontVector_R = new Vector3(0, 0, 1);
    public static Vector3 DownWardVector_R = new Vector3(0, 1, 0);

    //MainのJoyConのシリアルナンバー 接続しているJoyCon、もしくは接続していないときは優先して登録するJoyCon。空の文字列の時は好きに登録すれば良い。
    public static string SerialNumber_R { get; private set; } = "";
    //MainのJoyConのJoyConConnection nullでないなら、このJoyConConnectionに登録している
    private static JoyConConnection _joyconConnection_R;

    public static JoyConConnectInfo ConnectInfo= JoyConConnectInfo.JoyConIsNotFound;

    private List<byte[]> _inputReportsInThisFrame = null;//コルーチンでも実装できそう
    private bool _isReadyPolling = false;



    void Start()
    {
        _joyconConnection_R = null;
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        Joycon_subj.UpdateJoyConConnection();
        List<string> joyconRKeys = Joycon_subj.GetJoyConSerialNumbers_R();
        JoyConConnection newJoyConConnection=null;
        if (joyconRKeys.Count > 0)
        {
            SerialNumber_R = joyconRKeys[0];
            _joyconConnection_R = Joycon_subj.GetJoyConConnection(SerialNumber_R);
            _joyconConnection_R.AddObserver(this);
            _joyconConnection_R.ConnectToJoyCon();
            Debug.Log("Joyconが見つかった！");
            ConnectInfo = JoyConConnectInfo.SettingUpJoycon;
            

            joyConSetUp(_cancellationToken).Forget();
        }

        JoyconPose_R = Quaternion.identity;
        SmoothedPose_R = Quaternion.identity;
        ringconStrain = 0;
    }

    void Update()
    {
        SmoothedPose_R = Quaternion.Slerp(SmoothedPose_R, JoyconPose_R, 0.05f);
        if (ConnectInfo!= JoyConConnectInfo.JoyConIsNotFound && !_joyconConnection_R.IsConnecting)
        {
            ConnectInfo = JoyConConnectInfo.JoyConIsNotFound;
            Debug.Log("接続が切れたよ!!!");
        }
    }

    //テスト
    public void SubCmdQueing()
    {
        Debug.Log("0");
        if (_joyconConnection_R == null)
        {
            Debug.Log("nullだ");
        }
        
    }
    public async UniTaskVoid SendSubCmd()
    {
        byte[] buf = new byte[50];
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x03, 0x30 }, buf, _cancellationToken);
    }
    public void asyncTrigger()
    {
        SendSubCmd().Forget();
    }
    public void ReconnectTrigger()
    {
        ReConnectJoyconAsync().Forget();
    }
    //テスト(終わり)

    //JoyConを接続し直す

    public async UniTask<bool> ReConnectJoyconAsync()
    {
        //以前登録していたJoyConConnectionへの登録を解除 二重にJoyconConnectionに登録するのを防ぐ
        if (_joyconConnection_R!=null)
        {
            _joyconConnection_R.DelObserver(this);
            _joyconConnection_R = null;
        }
        string newJoyConSerialNum="";
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


        if (newJoyConSerialNum!="")
        {
            JoyConConnection newJoyConConnection = Joycon_subj.GetJoyConConnection(newJoyConSerialNum);

            if (newJoyConConnection.ConnectToJoyCon())
            {
                ConnectInfo = JoyConConnectInfo.SettingUpJoycon;
                newJoyConConnection.AddObserver(this);
                _joyconConnection_R = newJoyConConnection;
                await joyConSetUp(_cancellationToken);
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

    

    private async UniTask joyConSetUp(CancellationToken cancellationToken)
    {
        
        //await UniTask.DelayFrame(100, cancellationToken: cancellationToken);
        //実行コンテクスト(?というらしい)をPreUpdateに切り替える
        await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);

        byte[] ReplyBuf=new byte[50];

        Debug.Log("セットアップします!");
        // Enable vibration
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x48, 0x01 }, ReplyBuf, _cancellationToken);


        // Enable IMU data
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x40, 0x01 }, ReplyBuf, _cancellationToken);

        //Set input report mode to 0x30
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x03, 0x30 }, ReplyBuf, _cancellationToken);

        // Enabling MCU data
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x22, 0x01 }, ReplyBuf, _cancellationToken);

        //enabling_MCU_data_21_21_1_1
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[39] { 0x21, 0x21, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3 }, ReplyBuf, _cancellationToken);

        //get_ext_data_59
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x59, 0x0 }, ReplyBuf, _cancellationToken);

        //get_ext_dev_in_format_config_5C
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x5C, 0x06, 0x03, 0x25, 0x06, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x16, 0xED, 0x34, 0x36, 0x00, 0x00, 0x00, 0x0A, 0x64, 0x0B, 0xE6, 0xA9, 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xA8, 0xE1, 0x34, 0x36 }, ReplyBuf, _cancellationToken);

        //start_external_polling_5A
        await _joyconConnection_R.SendSubCmd_And_WaitReply(new byte[] { 0x5A, 0x04, 0x01, 0x01, 0x02 }, ReplyBuf, _cancellationToken);

        ConnectInfo=JoyConConnectInfo.JoyConIsReady;
        Debug.Log("おわり!!!");
    }

    public override void OnReadReport(List<byte[]> reports)
    {
        int x30ReportNum = 0;
        foreach (byte[] report in reports)
        {
            if (report[0] == 0x30)
            {
                x30ReportNum++;
            }
        }
        if (x30ReportNum == 0)
        {
            return;
        }

        foreach (byte[] report in reports)
        {
            if (report[0] == 0x30)
            {
                float sec = Time.deltaTime / x30ReportNum;
                ringconStrain = BitConverter.ToInt16(report, 39);
                float gyro_x1 = 0.070f * (float)BitConverter.ToInt16(report, 19);
                float gyro_y1 = 0.070f * (float)BitConverter.ToInt16(report, 21);
                float gyro_z1 = 0.070f * (float)BitConverter.ToInt16(report, 23);
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
            }
        }


    }
    public void applyIMUData(Vector3 accV, Vector3 gyroV, float sec)
    {
        float gyro_x = gyroV.x;
        float gyro_y = gyroV.y;
        float gyro_z = gyroV.z;
        Quaternion acRoll = Quaternion.AngleAxis(Mathf.Sqrt(gyro_x * gyro_x + gyro_y * gyro_y + gyro_z * gyro_z) * sec, JoyconPose_R * new Vector3(gyro_x, gyro_y, gyro_z));
        //Quaternion acRoll1 = argJoyconPose * Quaternion.Euler(new Vector3(gyro_x1, gyro_y1, gyro_z1) * sec / 2);
        JoyconPose_R = acRoll * JoyconPose_R;
        SmoothedPose_R = acRoll * SmoothedPose_R;
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
            Quaternion accPose = V3_MyUtil.RotateV2V(gravityV, DownWardVector_R);
            Vector3 gyro_frontV = JoyconPose_R * FrontVector_R.normalized;
            Vector3 acc_frontV = accPose * FrontVector_R.normalized;
            //accPose = Quaternion.AngleAxis((Mathf.Atan2(gyro_frontV.x, gyro_frontV.z) - Mathf.Atan2(acc_frontV.x, acc_frontV.z)) * 180 / Mathf.PI, new Vector3(0, 1, 0)) * accPose;
            Vector3 gyroFront_DownVVertical = V3_MyUtil.GetVerticalComp(gyro_frontV, DownWardVector_R).normalized;
            Vector3 accFront_DownVVertical = V3_MyUtil.GetVerticalComp(acc_frontV, DownWardVector_R).normalized;
            accPose = V3_MyUtil.RotateV2V(accFront_DownVVertical, gyroFront_DownVVertical) * accPose;
            JoyconPose_R = accPose;
        }
    }


    private async UniTask waitSubCommandReply(CancellationToken cancellationToken)
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

    void OnGUI()
    {
        GUI.Label(new Rect(50, 50, 50, 50), "Hello.");
    }
}

public enum JoyConConnectInfo
{
    JoyConIsNotFound,
    SettingUpJoycon,
    JoyConIsReady
}
