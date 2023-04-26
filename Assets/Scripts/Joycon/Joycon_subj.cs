using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.SceneManagement;


//新しく割り当てられるjoyconを探す時、既に接続しているhid_deviceかどうかを判断できる?
//hid_enumerateで接続されているJoyConのシリアルナンバーをごとのJoyconConnectingのinstanceを作成する


public class Joycon_subj : MonoBehaviour
{
    //static Dictionary<string> f;
    const int JOYCON_R_PRODUCTID = 8199;
    const int JOYCON_L_PRODUCTID =8198;
    

    public List<Joycon_obs> observers_R;
    IntPtr joyconR_dev;
    public List<Joycon_obs> observers_L;
    IntPtr joyconL_dev;


    bool joyconRIsConnectiong = false;
    bool joyconLIsConnectiong = false;
    string JoyconRSerialNum ="";
    string JoyconLSerialNum="";

    Thread HidReadThreadR=null;
    Thread HidReadThreadL = null;
    Queue<byte[]> ReportQueue_R;
    Queue<byte[]> ReportQueue_L;



    // Start is called before the first frame update
    void Start()
    {
        SceneManager.sceneLoaded += sceneLoaded;

        ReportQueue_R = new Queue<byte[]>();
        ReportQueue_L = new Queue<byte[]>();
        //joy-conへパケットを送ってinputReportのモードを変更する
        HIDapi.hid_init();
        ConnectJoyConL();
        ConnectJoyConR();
    }

    byte[] buf_update = null;
    uint replylen = 64;
    // Update is called once per frame
    void Update()
    {
        if (joyconRIsConnectiong)
        {
            int reportCount = ReportQueue_R.Count;
            for (int i = 0; i < reportCount; i++)
            {
                byte[] inputReport = ReportQueue_R.Dequeue();
                foreach (Joycon_obs aObs in observers_R)
                {
                    aObs.onReadReport(inputReport, 50, Time.deltaTime / reportCount);
                    //aObs.onReadReport(inputReport, 50, 1.0f/ 120.0f);
                }
            }
        }
        

        if (joyconLIsConnectiong)
        {
            int reportCount = ReportQueue_L.Count;
            for (int i = 0; i < reportCount; i++)
            {
                byte[] inputReport = ReportQueue_L.Dequeue();
                foreach (Joycon_obs aObs in observers_L)
                {
                    aObs.onReadReport(inputReport, 50, Time.deltaTime / reportCount);
                    //aObs.onReadReport(inputReport, 50, 1.0f/ 120.0f);
                }
            }
        }
        
    }

    public void ConnectJoyConR()
    {
        if (joyconRIsConnectiong)
        {
            return;
        }
        IntPtr device = HIDapi.hid_enumerate(0x0, 0x0);
        IntPtr topDevice = device;
        IntPtr joyconR_Info_ptr = IntPtr.Zero;

        byte[] byteBuffer = new byte[200];
        while (device != IntPtr.Zero)
        {
            hid_device_info enInfo = (hid_device_info)Marshal.PtrToStructure(device, typeof(hid_device_info));
            Debug.Log($"{intPtrToStrUtf32(enInfo.product_string, 30)} vendor_id:{enInfo.vendor_id} product_id:{enInfo.product_id}");
            if (enInfo.product_id == JOYCON_R_PRODUCTID)
            {
                joyconR_Info_ptr = device;
                string serial_number = intPtrToStrUtf32(enInfo.serial_number, 100);
                if (JoyconRSerialNum == "" | JoyconRSerialNum == serial_number)//まだ一度もjoyconLに接続していないか、以前接続したものと同一のものならば、それ以上JoyconLを探さない
                {
                    break;
                }
            }

            device = enInfo.next;
        }

        if (joyconR_Info_ptr == IntPtr.Zero)//前のwhileループでJoyconLが見つからなかった
        {
            Debug.Log("joy-conR is not found!");
            return;
        }
        hid_device_info joyconR_info = (hid_device_info)Marshal.PtrToStructure(joyconR_Info_ptr, typeof(hid_device_info));

        joyconR_dev = IntPtr.Zero;
        joyconR_dev = HIDapi.hid_open_path(joyconR_info.path);

        if (joyconR_dev == IntPtr.Zero)//JoyconLと通信開始できなかった。
        {
            Debug.Log("joy-conR can't open!");
            return;
        }
        joyconRIsConnectiong = true;
        JoyconRSerialNum = intPtrToStrUtf32(joyconR_info.serial_number, 100);
        Debug.Log($"JoyconRSerialNum:{JoyconRSerialNum}");

        int subCmdSize = 49;
        byte[] sendData = new byte[subCmdSize];
        int bufSize = 64;
        byte[] databuf = new byte[bufSize];
        HIDapi.hid_set_nonblocking(joyconR_dev, 1);
        // Enable vibration
        sendData[0] = 0x48;
        sendData[1] = 0x01;
        SendSubCmd(joyconR_dev, sendData, 2);
        getSubCmdReply(joyconR_dev, databuf, 49);

        // Enable IMU data
        sendData[0] = 0x40;
        sendData[1] = 0x01;
        SendSubCmd(joyconR_dev, sendData, 2);
        getSubCmdReply(joyconR_dev, databuf, 49);

        //Set input report mode to 0x30
        sendData[0] = 0x03;
        sendData[1] = 0x30;
        SendSubCmd(joyconR_dev, sendData, 2);
        getSubCmdReply(joyconR_dev, databuf, 49);

        // Enabling MCU data
        sendData[0] = 0x22;
        sendData[1] = 0x01;
        SendSubCmd(joyconR_dev, sendData, 2);
        getSubCmdReply(joyconR_dev, databuf, 49);


        //-----------------
        //enabling_MCU_data_21_21_1_1
        byte[] subcommand_21_21_1_1 = new byte[39] { 0x21, 0x21, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF3 };
        SendSubCmd(joyconR_dev, subcommand_21_21_1_1, 39);
        getSubCmdReply(joyconR_dev, databuf, 49);

        //get_ext_data_59
        sendData[0] = 0x59;
        sendData[1] = 0;
        SendSubCmd(joyconR_dev, sendData, 2);
        getSubCmdReply(joyconR_dev, databuf, 49);

        //get_ext_dev_in_format_config_5C
        byte[] subcommand_get_ext_dev_in_format_config_5C = new byte[38] { 0x5C, 0x06, 0x03, 0x25, 0x06, 0x00, 0x00, 0x00, 0x00, 0x1C, 0x16, 0xED, 0x34, 0x36, 0x00, 0x00, 0x00, 0x0A, 0x64, 0x0B, 0xE6, 0xA9, 0x22, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x90, 0xA8, 0xE1, 0x34, 0x36 };
        SendSubCmd(joyconR_dev, subcommand_get_ext_dev_in_format_config_5C, 38);
        getSubCmdReply(joyconR_dev, databuf, 49);

        //start_external_polling_5A
        sendData[0] = 0x5A;
        sendData[1] = 0x04;
        sendData[2] = 0x01;
        sendData[3] = 0x01;
        sendData[4] = 0x02;
        SendSubCmd(joyconR_dev, sendData, 5);
        getSubCmdReply(joyconR_dev, databuf, 49);



        HidReadThreadR = new Thread(
        () =>
        {
            Debug.Log($"StartPolling");
            int failReadCounter = 0;
            int i = 0;
            while (true)
            {
                i++;
                if (i % 1000 == 0)
                {
                    Debug.Log($"poll Count:{i}");
                }

                byte[] inputReport = new byte[50];
                int ret_read = HIDapi.hid_read_timeout(joyconR_dev, inputReport, 50, 10);
                if (ret_read != 0)
                {
                    ReportQueue_R.Enqueue(inputReport);
                    failReadCounter = 0;
                }
                else
                {
                    failReadCounter++;
                }
                if (failReadCounter > 500)
                {
                    Debug.Log($"JoyconR ConnectionLost StopPolling");
                    joyconRIsConnectiong = false;
                    break;
                }
            }
        });
        HidReadThreadR.Start();
        HIDapi.hid_free_enumeration(topDevice);
    }

    public void ConnectJoyConL()
    {
        if (joyconLIsConnectiong)
        {
            return;
        }
        IntPtr device = HIDapi.hid_enumerate(0x0, 0x0);
        IntPtr topDevice = device;
        IntPtr joyconL_Info_ptr = IntPtr.Zero;

        byte[] byteBuffer = new byte[200];
        while (device != IntPtr.Zero)
        {
            hid_device_info enInfo = (hid_device_info)Marshal.PtrToStructure(device, typeof(hid_device_info));
            Debug.Log($"{intPtrToStrUtf32(enInfo.product_string, 30)} vendor_id:{enInfo.vendor_id} product_id:{enInfo.product_id}");
            if (enInfo.product_id == JOYCON_L_PRODUCTID)
            {
                joyconL_Info_ptr = device;
                string serial_number=intPtrToStrUtf32(enInfo.serial_number,100);
                if (JoyconLSerialNum==""| JoyconLSerialNum == serial_number)//まだ一度もjoyconLに接続していないか、以前接続したものと同一のものを見つけたならば、それ以上JoyconLを探さない
                {
                    break;
                }
            }
            
            device = enInfo.next;
        }

        if (joyconL_Info_ptr == IntPtr.Zero)//前のwhileループでJoyconLが見つからなかった
        {
            Debug.Log("joy-conL is not found!");
            return;
        }
        hid_device_info joyconL_info = (hid_device_info)Marshal.PtrToStructure(joyconL_Info_ptr, typeof(hid_device_info));

        joyconL_dev = IntPtr.Zero;
        joyconL_dev = HIDapi.hid_open_path(joyconL_info.path);

        if (joyconL_dev == IntPtr.Zero)//JoyconLと通信開始できなかった。
        {
            Debug.Log("joy-conL can't open!");
            return;
        }

        joyconLIsConnectiong = true;
        JoyconLSerialNum = intPtrToStrUtf32(joyconL_info.serial_number, 100);
        Debug.Log($"JoyconLSerialNum:{JoyconLSerialNum}");

        HIDapi.hid_set_nonblocking(joyconL_dev, 1);

        int subCmdSize = 49;
        byte[] sendData = new byte[subCmdSize];
        int bufSize = 64;
        byte[] databuf = new byte[bufSize];

        subCmdSize = 49;
        sendData = new byte[subCmdSize];
        bufSize = 64;
        databuf = new byte[bufSize];

        // Enable vibration
        sendData[0] = 0x48;
        sendData[1] = 0x01;
        SendSubCmd(joyconL_dev, sendData, 2);
        getSubCmdReply(joyconL_dev, databuf, 49);

        // Enable IMU data
        sendData[0] = 0x40;
        sendData[1] = 0x01;
        SendSubCmd(joyconL_dev, sendData, 2);
        getSubCmdReply(joyconL_dev, databuf, 49);

        //Set input report mode to 0x30
        sendData[0] = 0x03;
        sendData[1] = 0x30;
        SendSubCmd(joyconL_dev, sendData, 2);
        getSubCmdReply(joyconL_dev, databuf, 49);

        // Enabling MCU data
        sendData[0] = 0x22;
        sendData[1] = 0x01;
        SendSubCmd(joyconL_dev, sendData, 2);
        getSubCmdReply(joyconL_dev, databuf, 49);




        HidReadThreadL = new Thread(
        () =>
        {
            Debug.Log($"StartPolling");
            int i = 0;
            int failReadCounter = 0;
            while (true)
            {
                i++;
                if (i % 1000 == 0)
                {
                    Debug.Log($"poll Count:{i}");
                }

                byte[] inputReport = new byte[50];
                int ret_read = HIDapi.hid_read_timeout(joyconL_dev, inputReport, 50, 10);
                if (ret_read != 0)
                {
                    ReportQueue_L.Enqueue(inputReport);
                }
                else
                {
                    failReadCounter++;
                }
                if (failReadCounter > 500)
                {
                    Debug.Log($"JoyconL ConnectionLost StopPolling");
                    joyconLIsConnectiong = false;
                    break;
                }
            }
        });

        HidReadThreadL.Start();
        HIDapi.hid_free_enumeration(topDevice);


    }





    byte globalPacketNumber=0;
    void SendSubCmd(IntPtr sentDev, byte[] subCmdIDAndArgs, int subCmdLen)
    {
        int reportlen = 10 + subCmdLen;
        byte[] sendData = new byte[reportlen];
        sendData[0] = 0x01; //Output report
        sendData[1] = (byte)((globalPacketNumber++) % 16);    //Global packet number 0x00から0x0fをループする
        //左のjoy-conの振動
        sendData[2] = 0x00; //HF周波数 4step 0x04-0xfc
        sendData[3] = 0x00; //HF振幅 2step 偶奇でHF周波数が変化 0x00-0xc8
        sendData[4] = 0x00; //LF周波数 80以上でLF振幅が変化 0x01-0x7f 0x81-0xff
        sendData[5] = 0x00; //LF振幅 0x40-0x72 変化後40-71
        //右のjoy-conの振動
        sendData[6] = 0x00;
        sendData[7] = 0x00;
        sendData[8] = 0x00;
        sendData[9] = 0x00;
        //サブコマンド
        sendData[10] = subCmdIDAndArgs[0];
        //サブコマンドの引数
        for (int i = 1; i < subCmdLen; i++)
        {
            sendData[10 + i] = subCmdIDAndArgs[i];
        }
        HIDapi.hid_write(sentDev, sendData, (uint)reportlen);
        Debug.Log($"send subcommand{subCmdIDAndArgs[0]}");
    }

    void getSubCmdReply(IntPtr sentDev, byte[] buf, int replylen)
    {
        int counter = 0;
        while (true)
        {
            int ret_read = 0;
            ret_read = HIDapi.hid_read_timeout(sentDev, buf, 50, 1000);
            if (ret_read != 0)
            {
                if (buf[0] == 0x21)
                {
                    Debug.Log($"get subcommand reply  {(buf[13] >= 0x80 ? "ACK" : "NACK")}  ID:{buf[14]}");
                    break;
                }
            }
            counter++;
            //if (counter > 100) { Debug.Log("AT~~~~"); break; }
        }
    }

    


    private void OnApplicationQuit()
    {
        if (HidReadThreadR != null) {HidReadThreadR.Abort();}
        HIDapi.hid_close(joyconR_dev);
        if (HidReadThreadL != null) { HidReadThreadL.Abort(); }
        HIDapi.hid_close(joyconL_dev);
    }

    void sceneLoaded(Scene nextScene, LoadSceneMode mode)
    {
        Debug.Log(nextScene.name);
        Debug.Log(mode);
        if (HidReadThreadR != null) { HidReadThreadR.Abort(); }
        HIDapi.hid_close(joyconR_dev);
        if (HidReadThreadL != null) { HidReadThreadL.Abort(); }
        HIDapi.hid_close(joyconL_dev);
        SceneManager.sceneLoaded-= sceneLoaded;
    }

    string bytesToStr(byte[] bytes)
    {
        string retV="";
        foreach (byte aByte in bytes)
        {
            retV += aByte.ToString()+" ";
        }
        return retV;
    }
    string bytesToStrUTF32(byte[] bytes)
    {
        string retV="";
        int lastCode = -1; 
        for (int i=0;i<=bytes.Length-4;i+=4)
        {
            
            int utf32Code = BitConverter.ToInt32(bytes, i);
            //0x000000 and 0x10ffff
            if (0x000000 <= utf32Code & utf32Code <= 0x10ffff& utf32Code!=0)
            {
                lastCode = utf32Code;
                retV += Char.ConvertFromUtf32(utf32Code);
            }
            else
            {
                break;
            }
        }
        //Debug.Log(lastCode);
        //Debug.Log(Char.ConvertFromUtf32(lastCode));
        return retV;
    }
    string intPtrToStrUtf32(IntPtr intPtr,int maxlen)
    {
        string retV = "";
        byte[] wcharBuf=new byte[4];
        IntPtr curPtr=intPtr;
        while (true)
        {
            int counter = 0;
            Marshal.Copy(curPtr, wcharBuf, 0, wcharBuf.Length);
            int utf32Code = BitConverter.ToInt32(wcharBuf, 0);
            //UTF32の文字コードの取り得る値の範囲に収まっているか? & utf32Codeがnull文字じゃないか? & 文字数の最大値に達していないか?
            if (0x000000 <= utf32Code & utf32Code <= 0x10ffff & utf32Code != 0& counter<= maxlen)
            {
                retV += Char.ConvertFromUtf32(utf32Code);
                counter++;
            }
            else
            {
                break;
            }
            curPtr = IntPtr.Add(curPtr, 4);
        }
        return retV;
    }

    
}
public class JoyConConnection
{
    public bool IsConnecting { get; private set; } = false;
    public bool IsJoyconRight { get; private set; } = false;
    public string Serial_Number { get; private set; }
    private List<Joycon_subj> _observers=null;
    private Thread _hidReadThread=null;
    private Queue<byte[]> _reportQueue=null;
    private IntPtr _joycon_dev=IntPtr.Zero;


    public JoyConConnection()
    {
        
    }

    /*
    public void SetJoyconDev(IntPtr newJoyconDev)
    {
        if (!IsConnecting)
        {
            _joycon_dev = newJoyconDev;
            IsConnecting = true;
        }

    }
    */

    public void ConnectToJoyCon()
    {
        const int JOYCON_R_PRODUCTID = 8199;
        const int JOYCON_L_PRODUCTID = 8198;

        if (IsConnecting)
        {
            return;
        }
        IntPtr device = HIDapi.hid_enumerate(0x0, 0x0);
        IntPtr topDevice = device;
        IntPtr joyconR_Info_ptr = IntPtr.Zero;

        byte[] byteBuffer = new byte[200];
        while (device != IntPtr.Zero)
        {
            hid_device_info enInfo = (hid_device_info)Marshal.PtrToStructure(device, typeof(hid_device_info));
            Debug.Log($"{MyMarshal.intPtrToStrUtf32(enInfo.product_string, 30)} vendor_id:{enInfo.vendor_id} product_id:{enInfo.product_id}");
            if (enInfo.product_id == JOYCON_R_PRODUCTID)
            {
                joyconR_Info_ptr = device;
                string serial_number = MyMarshal.intPtrToStrUtf32(enInfo.serial_number, 100);
                if (Serial_Number == serial_number)//instance作成時に指定したシリアルナンバーと一致した場合のみ接続
                {
                    break;
                }
            }

            device = enInfo.next;
        }

        if (joyconR_Info_ptr == IntPtr.Zero)//前のwhileループでJoyconLが見つからなかった
        {
            Debug.Log($"{Serial_Number} is not found!");
            return;
        }
        hid_device_info joyconR_info = (hid_device_info)Marshal.PtrToStructure(joyconR_Info_ptr, typeof(hid_device_info));

        _joycon_dev = IntPtr.Zero;
        _joycon_dev = HIDapi.hid_open_path(joyconR_info.path);

        if (_joycon_dev == IntPtr.Zero)//JoyconLと通信開始できなかった。
        {
            Debug.Log($"{Serial_Number} can't open!");
            return;
        }
        IsConnecting = true;
        Debug.Log($"{Serial_Number} open!");



        HIDapi.hid_set_nonblocking(_joycon_dev, 1);
        _hidReadThread = new Thread(
        () =>
        {
            Debug.Log($"StartPolling");
            int failReadCounter = 0;
            int i = 0;
            while (true)
            {
                i++;
                if (i % 1000 == 0)
                {
                    Debug.Log($"poll Count:{i}");
                }

                byte[] inputReport = new byte[50];
                int ret_read = HIDapi.hid_read_timeout(_joycon_dev, inputReport, 50, 10);
                if (ret_read != 0)
                {
                    _reportQueue.Enqueue(inputReport);
                    failReadCounter = 0;
                }
                else
                {
                    failReadCounter++;
                }
                if (failReadCounter > 500)
                {
                    Debug.Log($"{Serial_Number} ConnectionLost StopPolling");
                    IsConnecting = false;
                    break;
                }
            }
        });
        _hidReadThread.Start();
        HIDapi.hid_free_enumeration(topDevice);
    }
    

    private void HidReadRoop()
    {
        int i = 0;
        while (true)
        {
            i++;
            if (i % 1000 == 0)
            {
                Debug.Log($"poll Count:{i}");
            }

            byte[] inputReport = new byte[50];
            int ret_read = HIDapi.hid_read_timeout(_joycon_dev, inputReport, 50, 10);
            if (ret_read != 0)
            {
                _reportQueue.Enqueue(inputReport);
            }

        }
    }

}
