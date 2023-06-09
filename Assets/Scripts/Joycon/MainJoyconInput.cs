using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
public class MainJoyconInput : Joycon_obs
{
    private CancellationToken _cancellationToken;

    public static Quaternion JoyconPose_R { get; private set; }
    public static Quaternion SmoothedPose_R { get; private set; }
    public static float ringconStrain { get; private set; }
    //以下の2つはJoyconに搭載されたIMUにおける座標系(Nintend Switch Reverse Engeneeringのimu_sensor_notes>Axes definitionに書いてある)で、自分が指定したい正面方向と下方向を指定する
    public static Vector3 FrontVector_R = new Vector3(0, 0, 1);
    public static Vector3 DownWardVector_R = new Vector3(0, 1, 0);
    
    public static string SerialNumber_R { get; private set; }

    private static JoyConConnection _joyconConnection_R;

    public static JoyConConnectInfo ConnectInfo= JoyConConnectInfo.JoyConIsNotFound;

    private List<byte[]> _inputReportsInThisFrame = null;//コルーチンでも実装できそう
    private bool _isReadyPolling = false;


    public override void OnReadReport(List<byte[]> reports)
    {
        base.OnReadReport(reports);
    }

    void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        List<string> joyconRKeys = Joycon_subj.GetJoyConSerialNumbers_R();
        if (joyconRKeys.Count > 0)
        {
            _joyconConnection_R = Joycon_subj.GetJoyConConnection(joyconRKeys[0]);
            _joyconConnection_R.AddObserver(this);
            Debug.Log("Joyconが見つかった！");

            joyConSetUp(_cancellationToken).Forget();
        }

        JoyconPose_R = Quaternion.identity;
        SmoothedPose_R = Quaternion.identity;
        ringconStrain = 0;
    }

    void Update()
    {
        SmoothedPose_R = Quaternion.Slerp(SmoothedPose_R, JoyconPose_R, 0.05f);

    }

    private async UniTask joyConSetUp(CancellationToken cancellationToken)
    {
        //実行コンテクスト(?というらしい)をPreUpdateに切り替える
        await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);

        // Enable vibration
        _joyconConnection_R.SendSubCmd(new byte[] { 0x48, 0x01 }, 2);
        await waitSubCommandReply(cancellationToken);

        // Enable IMU data
        _joyconConnection_R.SendSubCmd(new byte[] { 0x40, 0x01 }, 2);
        await waitSubCommandReply(cancellationToken);

        //Set input report mode to 0x30
        _joyconConnection_R.SendSubCmd(new byte[] { 0x03, 0x30 }, 2);
        await waitSubCommandReply(cancellationToken);

        // Enabling MCU data
        _joyconConnection_R.SendSubCmd(new byte[] { 0x22, 0x01 }, 2);
        await waitSubCommandReply(cancellationToken);

        //enabling_MCU_data_21_21_1_1
        _joyconConnection_R.SendSubCmd(new byte[39] { 0x21, 0x21, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3 }, 39);
        await waitSubCommandReply(cancellationToken);

        //get_ext_data_59
        _joyconConnection_R.SendSubCmd(new byte[] { 0x59, 0x0 }, 2);
        await waitSubCommandReply(cancellationToken);

        //get_ext_dev_in_format_config_5C
        _joyconConnection_R.SendSubCmd(new byte[] { 0x5C, 0x06, 0x03, 0x25, 0x06, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x16, 0xED, 0x34, 0x36, 0x00, 0x00, 0x00, 0x0A, 0x64, 0x0B, 0xE6, 0xA9, 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xA8, 0xE1, 0x34, 0x36 }, 38);
        await waitSubCommandReply(cancellationToken);

        //start_external_polling_5A
        _joyconConnection_R.SendSubCmd(new byte[] { 0x5A, 0x04, 0x01, 0x01, 0x02 }, 5);
        await waitSubCommandReply(cancellationToken);

        Debug.Log("おわり!!!");
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
