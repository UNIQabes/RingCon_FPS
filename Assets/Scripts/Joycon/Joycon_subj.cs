using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Linq;
using Cysharp.Threading.Tasks;


//新しく割り当てられるjoyconを探す時、既に接続しているhid_deviceかどうかを判断できる?
//hid_enumerateで接続されているJoyConのシリアルナンバーをごとのJoyconConnectingのinstanceを作成する


public class Joycon_subj : MonoBehaviour
{
    static bool isInitialized = true;
    static Dictionary<string, JoyConConnection> _joyConConnections;
    const int JOYCON_R_PRODUCTID = 8199;
    const int JOYCON_L_PRODUCTID = 8198;


    public List<Joycon_obs> observers_R;
    IntPtr joyconR_dev;
    public List<Joycon_obs> observers_L;
    IntPtr joyconL_dev;


    bool joyconRIsConnectiong = false;
    bool joyconLIsConnectiong = false;
    string JoyconRSerialNum = "";
    string JoyconLSerialNum = "";


    Thread HidReadThreadR = null;
    Thread HidReadThreadL = null;
    Queue<byte[]> ReportQueue_R;
    Queue<byte[]> ReportQueue_L;

    static CancellationToken _cancellationToken;


    // Start is called before the first frame update
    void Start()
    {
        if (isInitialized)
        {
            HIDapi.hid_init();
            SceneManager.sceneLoaded += sceneLoaded;
            _joyConConnections = new Dictionary<string, JoyConConnection>();
            UpdateJoyConConnection();
            isInitialized = false;
            _cancellationToken = this.GetCancellationTokenOnDestroy();
        }
    }

    byte[] buf_update = null;
    uint replylen = 64;
    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<string, JoyConConnection> aPair in _joyConConnections)
        {
            if (aPair.Value.IsConnecting)
            {
                aPair.Value.PopInputReportToJoyconObs();
            }
        }
    }

    //辞書(_joycon_Connections)に登録されていないシリアルナンバーを持っているJoyconがhid_enummerateで見つかったら、それを辞書に登録する。
    //プログラム開始時にこの関数を使って、PCに接続されているそれぞれのJoyconに対応するJoyConConnectionインスタンスを作成する。
    public static bool UpdateJoyConConnection()
    {
        bool newJoyConIsFound = false;

        IntPtr device = HIDapi.hid_enumerate(0x0, 0x0);
        IntPtr topDevice = device;
        IntPtr joycon_Info_ptr = IntPtr.Zero;

        byte[] byteBuffer = new byte[200];


        while (device != IntPtr.Zero)
        {
            hid_device_info enInfo = (hid_device_info)Marshal.PtrToStructure(device, typeof(hid_device_info));
            Debug.Log($"{MyMarshal.intPtrToStrUtf32(enInfo.product_string, 30)} vendor_id:{enInfo.vendor_id} product_id:{enInfo.product_id}");
            if (enInfo.product_id == JOYCON_R_PRODUCTID | enInfo.product_id == JOYCON_L_PRODUCTID)
            {
                bool isJoyConR = (enInfo.product_id == JOYCON_R_PRODUCTID);
                joycon_Info_ptr = device;
                string serial_number = MyMarshal.intPtrToStrUtf32(enInfo.serial_number, 100);
                if (!_joyConConnections.ContainsKey(serial_number))
                {
                    _joyConConnections.Add(serial_number, new JoyConConnection(isJoyConR, serial_number, _cancellationToken));
                    newJoyConIsFound = true;
                }
            }
            device = enInfo.next;
        }

        HIDapi.hid_free_enumeration(topDevice);
        /*
        foreach (KeyValuePair<string, JoyConConnection> aPair in _joyConConnections)
        {
            aPair.Value.ConnectToJoyCon();
            aPair.Value.SendSubCmd(new byte[] { 0x03, 0x3F }, 2);
        }
        */
        return newJoyConIsFound;
    }

    public static List<string> GetJoyConSerialNumbers_R()
    {
        List<string> retValue = new List<string>();
        foreach (KeyValuePair<string, JoyConConnection> aKVP in _joyConConnections)
        {
            if (aKVP.Value.IsJoyconRight)
            {
                retValue.Add(aKVP.Key);
            }
        }
        return retValue;
    }
    public static List<string> GetJoyConSerialNumbers_L()
    {
        List<string> retValue = new List<string>();
        foreach (KeyValuePair<string, JoyConConnection> aKVP in _joyConConnections)
        {
            if (!aKVP.Value.IsJoyconRight)
            {
                retValue.Add(aKVP.Key);
            }
        }
        return retValue;
    }

    public static JoyConConnection GetJoyConConnection(string serial_number)
    {
        return _joyConConnections[serial_number];
    }

    byte globalPacketNumber = 0;
    /*
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
    */

    /*
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
    */

    



    private void OnApplicationQuit()
    {
        if (HidReadThreadR != null) {HidReadThreadR.Abort();}
        HIDapi.hid_close(joyconR_dev);
        if (HidReadThreadL != null) { HidReadThreadL.Abort(); }
        HIDapi.hid_close(joyconL_dev);
        Debug.Log(_joyConConnections==null);
        foreach (KeyValuePair<string, JoyConConnection> aPair in _joyConConnections)
        {
            aPair.Value.Disconnect();
        }
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

    

    
}
public class JoyConConnection
{
    const int JOYCON_R_PRODUCTID = 8199;
    const int JOYCON_L_PRODUCTID = 8198;

    public List<byte[]> ThisFrameInputs;

    //IsConnectingがfalseなら
    //・_hid_Read_Threadが Null or 動いていない
    //・_joycon_devの参照しているhid_deviceが開いていない/有効でない/IntPtr.Zero

    //IsConnectingがtrueなら
    //・_hid_Read_Threadは基本動いてる(動いてなくても最悪エラーやクラッシュは起こらない)
    //・_joycon_devの参照しているhid_deviceが開いている/有効である

    public bool IsConnecting { get; private set; } = false;
    public bool IsJoyconRight { get; private set; } = false;
    public string Serial_Number { get; private set; }
    private List<Joycon_obs> _observers = null;
    private Thread _hidReadThread = null;
    private Queue<byte[]> _reportQueue = null;
    private IntPtr _joycon_dev = IntPtr.Zero;

    private CancellationToken _cancellationToken;


    public JoyConConnection(bool isJoyconRight, string serial_Number, CancellationToken cancellationToken)
    {
        IsJoyconRight = isJoyconRight;
        Serial_Number = serial_Number;
        IsConnecting = false;
        _hidReadThread = null;
        _reportQueue = new Queue<byte[]>();
        _joycon_dev = IntPtr.Zero;
        _observers = new List<Joycon_obs>();
        ThisFrameInputs = new List<byte[]>();
        _cancellationToken= cancellationToken;
        subCmdQueue = new Queue<byte[]>();
    }

    public void PopInputReportToJoyconObs()
    {
        int reportCount = _reportQueue.Count;
        List<byte[]> sentReportInOneFrame = new List<byte[]>();
        for (int i = 0; i < reportCount; i++)
        {
            byte[] inputReport = _reportQueue.Dequeue();
            sentReportInOneFrame.Add(inputReport);
        }
        ThisFrameInputs = sentReportInOneFrame;
        foreach (Joycon_obs aObs in _observers)
        {
            aObs.OnReadReport(sentReportInOneFrame);
        }
        _subCmdReplys_temp = sentReportInOneFrame;
    }

    public bool ConnectToJoyCon()
    {

        if (IsConnecting)
        {
            return true;
        }
        IntPtr device = HIDapi.hid_enumerate(0x0, 0x0);
        IntPtr topDevice = device;
        IntPtr joycon_Info_ptr = IntPtr.Zero;

        byte[] byteBuffer = new byte[200];
        while (device != IntPtr.Zero)
        {
            hid_device_info enInfo = (hid_device_info)Marshal.PtrToStructure(device, typeof(hid_device_info));
            //Debug.Log($"{MyMarshal.intPtrToStrUtf32(enInfo.product_string, 30)} vendor_id:{enInfo.vendor_id} product_id:{enInfo.product_id}");
            string serial_number = MyMarshal.intPtrToStrUtf32(enInfo.serial_number, 100);
            if (enInfo.product_id == (IsJoyconRight ? JOYCON_R_PRODUCTID : JOYCON_L_PRODUCTID) & Serial_Number == serial_number)
            {
                joycon_Info_ptr = device;
                break;
            }

            device = enInfo.next;
        }

        if (joycon_Info_ptr == IntPtr.Zero)//前のwhileループでJoyconLが見つからなかった
        {
            Debug.Log($"{Serial_Number} is not found!");
            return false;
        }
        hid_device_info joycon_info = (hid_device_info)Marshal.PtrToStructure(joycon_Info_ptr, typeof(hid_device_info));

        _joycon_dev = IntPtr.Zero;
        _joycon_dev = HIDapi.hid_open_path(joycon_info.path);

        if (_joycon_dev == IntPtr.Zero)//JoyconLと通信開始できなかった。
        {
            Debug.Log($"{Serial_Number} can't open!");
            return false;
        }
        IsConnecting = true;
        Debug.Log($"{Serial_Number} open!");



        HIDapi.hid_set_nonblocking(_joycon_dev, 1);
        _hidReadThread = new Thread(HidReadRoop);
        _hidReadThread.Start();
        WaitSubCommandRoop(_cancellationToken).Forget();
        HIDapi.hid_free_enumeration(topDevice);
        return true;
    }

    //接続切断時の処理
    public void Disconnect()
    {

        if (_hidReadThread != null && _hidReadThread.IsAlive)
        {
            _hidReadThread.Abort();
            Debug.Log($"{Serial_Number} StopPolling");
        }
        if (IsConnecting)
        {
            HIDapi.hid_close(_joycon_dev);
            IsConnecting = false;
            Debug.Log($"{Serial_Number} DisConnect");
        }

    }


    //新しい実装------------------------

    private byte globalPacketNumber = 0;
    private void SendSubCmdSimple(byte[] subCmdIDAndArgs, int subCmdLen)
    {
        if (!IsConnecting)
        {
            Debug.Log($"{Serial_Number} is not connecting!");
            return;
        }
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
        HIDapi.hid_write(_joycon_dev, sendData, (uint)reportlen);
        Debug.Log($"send subcommand{subCmdIDAndArgs[0]} to {Serial_Number}");
    }

    
    //新しい実装(終)------------------------

    //新しい実装2-----------------------後々playerLoopにこの処理を入れるつもり
    List<byte[]> _subCmdReplys_temp = null;
    Queue<byte[]> subCmdQueue;
    byte[] replyGotSubCmd_ThisFrame;
    byte[] subCmdReply_ThisFrame;
    private async UniTaskVoid WaitSubCommandRoop(CancellationToken cancellationToken)//基本的にここに渡されるcancellationTokenはJoycon_subjのメンバ変数のcancellationToken
    {
        Debug.Log("WaitSubCommandRoop_Start");
        while ((!cancellationToken.IsCancellationRequested)&IsConnecting)
        {
            
            
            if (subCmdQueue.Count > 0)
            {
                
                byte[] subCmd = subCmdQueue.Dequeue();
                Debug.Log($"Pop:{subCmd[0]}");
                SendSubCmdSimple(subCmd,subCmd.Length);
                byte[] subCmdReply = null;
                await UniTask.WaitUntil
                (
                    ()=>
                    {
                        foreach (byte[] aReply in _subCmdReplys_temp)
                        {
                            if (aReply[0] == 0x21 & aReply[14] == subCmd[0])
                            {
                                subCmdReply = aReply;
                                return true;
                            }
                        }
                        return false;
                    }
                ,PlayerLoopTiming.PreUpdate, cancellationToken);
                replyGotSubCmd_ThisFrame = subCmd;
                subCmdReply_ThisFrame = subCmdReply;
                await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);//このawait中にSendSubCmd_And_WaitReplyがReplyを取得する
                replyGotSubCmd_ThisFrame = null;
                subCmdReply_ThisFrame = null;
            }
            else
            {
                await UniTask.Yield(PlayerLoopTiming.PreUpdate, cancellationToken);
                //Debug.Log("まってます");

            }
        }
        Debug.Log("CancellationRequested");
        
    }
    

    public async UniTask SendSubCmd_And_WaitReply(byte[] subCmd,
        byte[] SubCmdReplyBuf, CancellationToken cancellationToken)
    {
        int cmdCpyLen =subCmd.Length;
        byte[] subCmdCpy = new byte[cmdCpyLen];
        Array.Copy(subCmd, subCmdCpy, cmdCpyLen);
        subCmdQueue.Enqueue(subCmdCpy);
        Debug.Log($"push{subCmdCpy[0]} QueueCnt:{subCmdQueue.Count}");
        await UniTask.WaitUntil(()=>(subCmdCpy== replyGotSubCmd_ThisFrame), PlayerLoopTiming.EarlyUpdate);
        //Replyを受け取る
        int replyCpyLen= Math.Min(subCmdReply_ThisFrame.Length, SubCmdReplyBuf.Length);
        Array.Copy(subCmdReply_ThisFrame, SubCmdReplyBuf, replyCpyLen);
        Debug.Log($"get subcommand reply  {(subCmdReply_ThisFrame[13] >= 0x80 ? "ACK" : "NACK")}  ID:{subCmdReply_ThisFrame[14]}");

    }
    
    private void HidReadRoop()
    {
        Debug.Log($"StartPolling");
        int failReadCounter = 0;
        int i = 0;
        while (IsConnecting)
        {
            byte[] inputReport = new byte[50];
            inputReport[0] = 0x00;
            int ret_read = HIDapi.hid_read_timeout(_joycon_dev, inputReport, 50, 10);
            if (ret_read != 0 && inputReport[0]!=0x00)
            {
                _reportQueue.Enqueue(inputReport);
                failReadCounter = 0;

                i++;
                if (i % 1000 == 0)
                {
                    Debug.Log($" {Serial_Number} poll Count:{i}");
                }
            }
            else
            {
                failReadCounter++;
            }
            if (failReadCounter == 500)
            {
                byte[] ReplyBuf = new byte[1];
                SendSubCmd_And_WaitReply(new byte[]{0x00}, ReplyBuf,_cancellationToken).Forget();
                Debug.Log($"Check  {Serial_Number} Connection");
            }
            else if (failReadCounter > 1000)
            {
                Debug.Log($"{Serial_Number} ConnectionLost StopPolling");
                //IsConnecting = false;
                Disconnect();
                Debug.Log("fff");
                break;
            }

        }
    }

    
    public void SendSubCmd(byte[] subCmdIDAndArgs, int subCmdLen)
    {
        if (!IsConnecting)
        {
            Debug.Log($"{Serial_Number} is not connecting!");
            return;
        }
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
        HIDapi.hid_write(_joycon_dev, sendData, (uint)reportlen);
        Debug.Log($"send subcommand{subCmdIDAndArgs[0]} to {Serial_Number}");
    }
    


    public void AddObserver(Joycon_obs joycon_Obs)
    {
        _observers.Add(joycon_Obs);
    }
    public void DelObserver(Joycon_obs joycon_Obs)
    {
        _observers.Remove(joycon_Obs);
    }


}
