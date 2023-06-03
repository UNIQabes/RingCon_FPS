using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;


public class RingConPoseEst : Joycon_obs
{
    private CancellationToken _cTokenOnDestroy;

    public Quaternion joyconPose { get; private set; }
    public Quaternion smoothedPose { get; private set; }
    public float ringconStrain { get; private set; }
    //以下の2つはJoyconに搭載されたIMUにおける座標系(Nintend Switch Reverse Engeneeringのimu_sensor_notes>Axes definitionに書いてある)で、自分が指定したい正面方向と下方向を指定する
    public Vector3 FrontVector = new Vector3(0, 0, 1);
    public Vector3 DownWardVector = new Vector3(0, 1, 0);

    public JoyConConnection joyconRConnection;

    private List<byte[]> _inoutReportsInThisFrame = null;//コルーチンでも実装できそう
    private bool _isReadyPolling = false;


    // Start is called before the first frame update
    void Start()
    {
        _cTokenOnDestroy = this.GetCancellationTokenOnDestroy();
        List<string> joyconRKeys=Joycon_subj.GetJoyConSerialNumbers_R();
        if (joyconRKeys.Count > 0)
        {
            joyconRConnection = Joycon_subj.GetJoyConConnection(joyconRKeys[0]);
            joyconRConnection.AddObserver(this);
            Debug.Log("Joyconが見つかった！");

            joyConSetUp(_cTokenOnDestroy).Forget();
        }

        joyconPose = Quaternion.identity;
        smoothedPose = Quaternion.identity;
        ringconStrain = 0;

        /*
        Vector3 v1 = new Vector3(1, 1, 1).normalized;
        Vector3 v2 = new Vector3(2, 3, 5).normalized;
        Debug.Log(v1);
        Debug.Log(v2);
        Debug.Log(V3_MyUtil.GetHorizontalComp(v1, v2));
        Debug.Log(V3_MyUtil.GetVerticalComp(v1, v2));
        Debug.Log(V3_MyUtil.GetVerticalComp(v1, v2) + V3_MyUtil.GetHorizontalComp(v1, v2));
        */
        /*
        Debug.Log(RotateV2V(v1,v2)*v1);
        Debug.Log(RotateV2V(v2, v1) * v2);
        */
    }

    // Update is called once per frame
    void Update()
    {
        smoothedPose = Quaternion.Slerp(smoothedPose, joyconPose, 0.05f);
        this.transform.rotation = smoothedPose;
        
    }
    public override void OnReadReport(List<byte[]> reports)
    {
        _inoutReportsInThisFrame = reports;
        int x30ReportNum = 0;
        foreach (byte[] report in reports)
        {
            if (report[0] == 0x30)
            {
                x30ReportNum++;
            }
        }
        if (x30ReportNum==0)
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
        Quaternion acRoll = Quaternion.AngleAxis(Mathf.Sqrt(gyro_x * gyro_x + gyro_y * gyro_y + gyro_z * gyro_z) * sec, joyconPose * new Vector3(gyro_x, gyro_y, gyro_z));
        //Quaternion acRoll1 = argJoyconPose * Quaternion.Euler(new Vector3(gyro_x1, gyro_y1, gyro_z1) * sec / 2);
        joyconPose = acRoll * joyconPose;
        smoothedPose = acRoll * smoothedPose;
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
            Quaternion accPose = V3_MyUtil.RotateV2V(gravityV, DownWardVector);
            Vector3 gyro_frontV = joyconPose * FrontVector.normalized;
            Vector3 acc_frontV = accPose * FrontVector.normalized;
            //accPose = Quaternion.AngleAxis((Mathf.Atan2(gyro_frontV.x, gyro_frontV.z) - Mathf.Atan2(acc_frontV.x, acc_frontV.z)) * 180 / Mathf.PI, new Vector3(0, 1, 0)) * accPose;
            Vector3 gyroFront_DownVVertical = V3_MyUtil.GetVerticalComp(gyro_frontV, DownWardVector).normalized;
            Vector3 accFront_DownVVertical = V3_MyUtil.GetVerticalComp(acc_frontV, DownWardVector).normalized;
            accPose = V3_MyUtil.RotateV2V(accFront_DownVVertical, gyroFront_DownVVertical) * accPose;
            joyconPose = accPose;
        }
    }

    private async UniTask joyConSetUp(CancellationToken cancellationToken)
    {

        // Enable vibration
        joyconRConnection.SendSubCmd(new byte[] { 0x48, 0x01 }, 2);
        await waitSubCommandReply(cancellationToken);

        // Enable IMU data
        joyconRConnection.SendSubCmd(new byte[] { 0x40, 0x01 }, 2);
        await waitSubCommandReply(cancellationToken);

        //Set input report mode to 0x30
        joyconRConnection.SendSubCmd(new byte[] { 0x03, 0x30 }, 2);
        await waitSubCommandReply(cancellationToken);

        // Enabling MCU data
        joyconRConnection.SendSubCmd(new byte[] { 0x22, 0x01 }, 2);
        await waitSubCommandReply(cancellationToken);

        //enabling_MCU_data_21_21_1_1
        joyconRConnection.SendSubCmd(new byte[39] { 0x21, 0x21, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3 }, 39);
        await waitSubCommandReply(cancellationToken);

        //get_ext_data_59
        joyconRConnection.SendSubCmd(new byte[] { 0x59, 0x0 }, 2);
        await waitSubCommandReply(cancellationToken);

        //get_ext_dev_in_format_config_5C
        joyconRConnection.SendSubCmd(new byte[] { 0x5C, 0x06, 0x03, 0x25, 0x06, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x16, 0xED, 0x34, 0x36, 0x00, 0x00, 0x00, 0x0A, 0x64, 0x0B, 0xE6, 0xA9, 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xA8, 0xE1, 0x34, 0x36 }, 38);
        await waitSubCommandReply(cancellationToken);

        //start_external_polling_5A
        joyconRConnection.SendSubCmd(new byte[] { 0x5A, 0x04, 0x01, 0x01, 0x02 }, 5);
        await waitSubCommandReply(cancellationToken);

        Debug.Log("おわり!!!");
    }
    private async UniTask waitSubCommandReply(CancellationToken cancellationToken)
    {
        bool isSentReply=false;
        while (!isSentReply)
        {
            if (_inoutReportsInThisFrame!=null)
            {
                foreach (byte[] aInputReport in _inoutReportsInThisFrame)
                {
                    if (aInputReport[0] == 0x21)
                    {
                        
                        isSentReply = true;
                        break;
                    }
                }
            }
            
            await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);
        }
        
    }
}
