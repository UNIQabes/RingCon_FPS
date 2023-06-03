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


    public void SendSubCmdSimple(byte[] subCmdIDAndArgs, int subCmdLen)
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

    byte[] _popedSubCmd_temp = null;
    Queue<byte[]> _subcmdQueue_temp = new Queue<byte[]>();
    void pushToSubCmdQueue_temp(byte[] subCmd)
    {
        if (_popedSubCmd_temp == null)
        {
            _popedSubCmd_temp = subCmd;
        }
        else
        {
            _subcmdQueue_temp.Enqueue(subCmd);
        }
    }
    void popSubCmdQueue_temp()
    {
        if (_subcmdQueue_temp.Count == 0)
        {
            _popedSubCmd_temp = null;
        }
        else
        {
            _popedSubCmd_temp=_subcmdQueue_temp.Dequeue();
        }
    }

    List<byte[]> _subCmdReplys_temp = new List<byte[]>();
    public async UniTask<bool> SendSubCommandAsync(byte[] subCmd, int subCmdLen,
        byte[] SubCmdReplyBuf, int replyLen, CancellationToken cancellationToken)
    {
        
        byte[] subCmdCpy = new byte[subCmdLen];
        Array.Copy(subCmd, subCmdCpy, subCmdLen);
        pushToSubCmdQueue_temp(subCmdCpy);
        while (_popedSubCmd_temp == subCmdCpy & !cancellationToken.IsCancellationRequested) { }
        SendSubCmdSimple(subCmdCpy, subCmdLen);
        byte[] cmdReply = new byte[0];

        while (!cancellationToken.IsCancellationRequested)
        {
            bool isbreak = false;
            foreach (byte[] aReply in _subCmdReplys_temp)
            { if (aReply[0] == 0x21 & aReply[14] == subCmdCpy[10]) { isbreak = true; cmdReply = aReply; break; } }
            _subCmdReplys_temp.Clear();
            if (isbreak) { break; }
        }
        await UniTask.WaitUntil(()=>true);
        Array.Copy(cmdReply, SubCmdReplyBuf, Math.Min(replyLen, cmdReply.Length));
        popSubCmdQueue_temp();
        return true;
    }
    //新しい実装(終)------------------------

    //新しい実装2-----------------------後々playerLoopにこの処理を入れるつもり
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
    /*
    public void SubCmdQueing(byte[] subCmd, int subCmdLen)
    {
        
        int cmdCpyLen = Math.Min(subCmdLen, subCmd.Length);
        byte[] subCmdCpy = new byte[cmdCpyLen];
        Array.Copy(subCmd, subCmdCpy, cmdCpyLen);
        subCmdQueue.Enqueue(subCmdCpy);
        Debug.Log($"QueueCount:{subCmdQueue.Count}");
    }
    */

    public async UniTask SendSubCmd_And_WaitReply(byte[] subCmd,
        byte[] SubCmdReplyBuf, CancellationToken cancellationToken)
    {
        int cmdCpyLen =subCmd.Length;
        byte[] subCmdCpy = new byte[cmdCpyLen];
        Array.Copy(subCmd, subCmdCpy, cmdCpyLen);
        subCmdQueue.Enqueue(subCmdCpy);
        Debug.Log($"push{subCmdCpy[0]} QueueCnt:{subCmdQueue.Count}");
        await UniTask.WaitUntil(()=>(subCmdCpy== replyGotSubCmd_ThisFrame), PlayerLoopTiming.EarlyUpdate);
        Debug.Log("終わったンゴねぇ…");
        //Replyを受け取る
        Debug.Log(subCmdReply_ThisFrame==null);
        Debug.Log(SubCmdReplyBuf == null);
        int replyCpyLen= Math.Min(subCmdReply_ThisFrame.Length, SubCmdReplyBuf.Length);
        Debug.Log(subCmdReply_ThisFrame.Length);
        Debug.Log(SubCmdReplyBuf.Length);

        Array.Copy(subCmdReply_ThisFrame, SubCmdReplyBuf, replyCpyLen);

    }
    //新しい実装2(終)-----------------------


    /*実装するには、HidReadRoop内でprivateなイベントリスナーを呼び出すコードを書く必要がありそう
    public bool Update_IsConnecting()
    {
        
        return IsConnecting;
    }
    */

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
                SendSubCmd(new byte[]{0x00},1);
                Debug.Log($"Check  {Serial_Number} Connection");
            }
            else if (failReadCounter > 1000)
            {
                Debug.Log($"{Serial_Number} ConnectionLost StopPolling");
                IsConnecting = false;
                break;
            }

        }
    }

    private byte globalPacketNumber = 0;
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
    //特定の連続したサブコマンドを送り続ける場合、途中でサブコマンドの返信を待つ必要がある。そのため、Ring-Conの入力を受け取るための一連のパケットを送る関数は非同期である必要がある。

    public void AddObserver(Joycon_obs joycon_Obs)
    {
        _observers.Add(joycon_Obs);
    }
    public void DelObserver(Joycon_obs joycon_Obs)
    {
        _observers.Remove(joycon_Obs);
    }


}
