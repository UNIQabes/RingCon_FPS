using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine.SceneManagement;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.LowLevel;


//新しく割り当てられるjoyconを探す時、既に接続しているhid_deviceかどうかを判断できる?
//hid_enumerateで接続されているJoyConのシリアルナンバーをごとのJoyconConnectingのinstanceを作成する


public class Joycon_subj : MonoBehaviour
{
    //新しい実装
    

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        
        HIDapi.hid_init();
        //SceneManager.sceneLoaded += sceneLoaded;
        _joyConConnections = new Dictionary<string, JoyConConnection>();
        UpdateJoyConConnection();
        _cTokenSourceOnAppQuit= new CancellationTokenSource();
        _cancellationTokenOnAppQuit = _cTokenSourceOnAppQuit.Token;

        //Application終了時の処理を設定
        Application.quitting += OnApplicatioQuitStatic;

        UpdateStatic().Forget();


    }

    
    private static async UniTaskVoid UpdateStatic()
    {
        //アプリが動いている間は動いている。
        while (!_cancellationTokenOnAppQuit.IsCancellationRequested)
        {
            foreach (KeyValuePair<string, JoyConConnection> aPair in _joyConConnections)
            {
                if (aPair.Value.IsConnecting)
                {
                    aPair.Value.PopInputReportToJoyconObs();
                }
            }
            await UniTask.Yield(PlayerLoopTiming.PreUpdate, _cancellationTokenOnAppQuit);
        }
        Debug.Log("JoyconSubj.UpdateStatic stop");
    }

    private static void OnApplicatioQuitStatic()
    {
        Debug.Log("アプリ、終わったンゴねぇ…");
        foreach (KeyValuePair<string, JoyConConnection> aPair in _joyConConnections)
        {
            aPair.Value.Disconnect();
        }
        _cTokenSourceOnAppQuit.Cancel();
    }
    //新しい実装(終わり)


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

    static CancellationTokenSource _cTokenSourceOnAppQuit;
    static CancellationToken _cancellationTokenOnAppQuit;


    byte[] buf_update = null;
    uint replylen = 64;

   

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
                    _joyConConnections.Add(serial_number, new JoyConConnection(isJoyConR, serial_number, _cancellationTokenOnAppQuit));
                    newJoyConIsFound = true;
                }
            }
            device = enInfo.next;
        }

        HIDapi.hid_free_enumeration(topDevice);
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

    
    private CancellationTokenSource _cTokenSrcOnDisConnect;//JoyConとの接続が切れた時にキャンセルされるTokenのSource
    private CancellationTokenSource _cTokenSrcOnDisConnectOrAppQuit;//JoyConとの接続が切れた時かApplication終了時にキャンセルされるTokenのSource

    //private CancellationToken _cancellationToken;//Application終了時or接続切断時にCancellされるToken
    private CancellationToken _cTokenOnAppQuit;//Application終了時にCancellされるToken
    private CancellationToken _cTokenOnDisConnect;//JoyConとの接続が切れた時にキャンセルされるToken Application終了時
    private CancellationToken _cancellationTokenOnDisConnectOrAppQuit;//Application終了時or接続切断時にCancellされるToken



    public JoyConConnection(bool isJoyconRight, string serial_Number, CancellationToken cancellationTokenOnAppQuit)
    {
        IsJoyconRight = isJoyconRight;
        Serial_Number = serial_Number;
        IsConnecting = false;
        _hidReadThread = null;
        _reportQueue = new Queue<byte[]>();
        _joycon_dev = IntPtr.Zero;
        _observers = new List<Joycon_obs>();
        ThisFrameInputs = new List<byte[]>();
        subCmdQueue = new Queue<byte[]>();
        _subCmdReplysInThisFrame = new List<byte[]>();
        _cTokenOnAppQuit = cancellationTokenOnAppQuit;
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
        _subCmdReplysInThisFrame = sentReportInOneFrame;
    }

    //JoyConとの接続時、必ずこれが呼ばれる。この関数は外部の利用者から呼ばれる。
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
            //シリアルナンバーが一致し、かつJoyConの左右がIsJoyconRightと一致しているか
            if (enInfo.product_id == (IsJoyconRight ? JOYCON_R_PRODUCTID : JOYCON_L_PRODUCTID) & Serial_Number == serial_number)
            {
                joycon_Info_ptr = device;
                break;
            }

            device = enInfo.next;
        }

        if (joycon_Info_ptr == IntPtr.Zero)//前のwhileループでJoyconが見つからなかった
        {
            Debug.Log($"{Serial_Number} is not found!");
            return false;
        }
        hid_device_info joycon_info = (hid_device_info)Marshal.PtrToStructure(joycon_Info_ptr, typeof(hid_device_info));

        _joycon_dev = IntPtr.Zero;
        _joycon_dev = HIDapi.hid_open_path(joycon_info.path);//JoyConと通信開始

        if (_joycon_dev == IntPtr.Zero)//JoyconLと通信開始できなかった。
        {
            Debug.Log($"{Serial_Number} can't open!");
            return false;
        }

        IsConnecting = true;
        Debug.Log($"{Serial_Number} open!");
        _cTokenSrcOnDisConnect = new CancellationTokenSource();
        _cTokenOnDisConnect = _cTokenSrcOnDisConnect.Token;

        HIDapi.hid_set_nonblocking(_joycon_dev, 1);

        _hidReadThread = new Thread(HidReadRoop);
        _hidReadThread.Start();
        WaitSubCommandRoop().Forget();

        HIDapi.hid_free_enumeration(topDevice);

        
        return true;
    }

    //接続切断時には必ずこの関数が呼びだされる。この関数はHIDReadRoopか、外部の利用者から呼ばれる
    public void Disconnect()
    {

        
        if (IsConnecting)
        {
            HIDapi.hid_close(_joycon_dev);
            IsConnecting = false;
            _cTokenSrcOnDisConnect.Cancel();
            Debug.Log(_cTokenOnDisConnect.IsCancellationRequested);
            Debug.Log($"{Serial_Number} DisConnect");
        }
        if (_hidReadThread != null && _hidReadThread.IsAlive)
        {
            Debug.Log($"{Serial_Number} StopPolling");
            _hidReadThread.Abort();
            
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
    List<byte[]> _subCmdReplysInThisFrame;
    Queue<byte[]> subCmdQueue;
    byte[] replyGotSubCmd_ThisFrame;//replyがないフレームはnull
    byte[] subCmdReply_ThisFrame;//replyがないフレームはnull


    //Joyconと接続している(isConnecting=true)の時には動いている
    private async UniTaskVoid WaitSubCommandRoop()//基本的にここに渡されるcancellationTokenは接続切断時にcancellされるやつ
    {
        //CancellationToken(Appが実行中か)とIsConnecting
        Debug.Log("WaitSubCommandRoop_Start");
        while ((!_cTokenOnDisConnect.IsCancellationRequested)&IsConnecting)
        {
            if (subCmdQueue.Count > 0)
            {
                
                byte[] subCmd = subCmdQueue.Dequeue();
                Debug.Log($"pop {subCmd[0]} from subcmdQueue");
                SendSubCmdSimple(subCmd,subCmd.Length);
                byte[] subCmdReply = null;
                Debug.Log($"wait {subCmd[0]} reply");
                await UniTask.WaitUntil
                (
                    ()=>
                    {
                        //Debug.Log("チェックしてます");
                        foreach (byte[] aReply in _subCmdReplysInThisFrame)
                        {
                            if (aReply[0] == 0x21 & aReply[14] == subCmd[0])
                            {
                                subCmdReply = aReply;
                                return true;
                            }
                        }
                        return false;
                    }
                ,PlayerLoopTiming.PreUpdate, _cTokenOnDisConnect);
                Debug.Log($"get {subCmd[0]} reply");
                replyGotSubCmd_ThisFrame = subCmd;
                subCmdReply_ThisFrame = subCmdReply;
                Debug.Log($"sendSubcmdAndWaitReplyさん!subCmdReply_ThisFrame(ID:{replyGotSubCmd_ThisFrame[0]})を受け取ってください!");
                //このawait中にSendSubCmd_And_WaitReplyがsubCmdReply_ThisFrameを見てReplyを取得する
                await UniTask.DelayFrame(1,PlayerLoopTiming.PreUpdate, _cTokenOnDisConnect);
                replyGotSubCmd_ThisFrame = null;
                subCmdReply_ThisFrame = null;
                Debug.Log("subCmdReply_ThisFrameをクリアしました");
            }
            else
            {
                await UniTask.Yield(PlayerLoopTiming.PreUpdate, _cTokenOnDisConnect);//applicationが終了するとここで自動で止まるっぽい?updateメソッドが呼び出されなくなっているだけでこの関数は生きている気がする

            }
        }
        //基本的にはCancellationTokenのCancellによって停止するため、この部分に到達することはない
        Debug.Log($"{Serial_Number} WaitSubCommandRoop Stop");
        
    }
    

    public async UniTask SendSubCmd_And_WaitReply(byte[] subCmd,
        byte[] SubCmdReplyBuf, CancellationToken cancellationToken)//多分cancellationTokenはいらない気がするが、一応残していく
    {
        
        int cmdCpyLen =subCmd.Length;
        byte[] subCmdCpy = new byte[cmdCpyLen];
        Array.Copy(subCmd, subCmdCpy, cmdCpyLen);
        subCmdQueue.Enqueue(subCmdCpy);
        Debug.Log($"push {subCmdCpy[0]} to subcmdQueue QueueCnt:{subCmdQueue.Count}");
        //subcmdのreplyが来るまでawait
        await UniTask.WaitUntil(()=>((subCmdCpy== replyGotSubCmd_ThisFrame)), PlayerLoopTiming.EarlyUpdate, _cTokenOnDisConnect);
        //Replyを受け取る
        int replyCpyLen= Math.Min(subCmdReply_ThisFrame.Length, SubCmdReplyBuf.Length);
        Array.Copy(subCmdReply_ThisFrame, SubCmdReplyBuf, replyCpyLen);
        Debug.Log($"subcmdReply(ID:{replyGotSubCmd_ThisFrame[0]})を受け取りました!");
        Debug.Log($"get subcommand reply  {(subCmdReply_ThisFrame[13] >= 0x80 ? "ACK" : "NACK")}  ID:{subCmdReply_ThisFrame[14]}");

    }

    //JoyConが接続している時(isConnecting===true)は動いている
    private void HidReadRoop()
    {
        Debug.Log($"StartPolling {Serial_Number}");
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
                SendSubCmd_And_WaitReply(new byte[]{0x00}, ReplyBuf, _cTokenOnAppQuit).Forget();
                Debug.Log($"Check  {Serial_Number} Connection");
            }
            else if (failReadCounter > 1000)
            {
                Debug.Log($"{Serial_Number} ConnectionLost");
                Debug.Log($"{Serial_Number} HidReadRoop Stop");
                //IsConnecting = false;
                Disconnect();
                break;
            }

        }
        Debug.Log($"{Serial_Number} Polling Finish");
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
