using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;


public class SendSubCmdAsyncTest : MonoBehaviour
{
    private static JoyConConnection _joyconConnection_R;
    private CancellationToken _cancellationToken;


    // Start is called before the first frame update
    void Start()
    {
        _cancellationToken = this.GetCancellationTokenOnDestroy();
        HIDapi.hid_init();
        List<string> joyconRKeys = Joycon_subj.GetJoyConSerialNumbers_R();
        _joyconConnection_R = Joycon_subj.GetJoyConConnection(joyconRKeys[0]);
        //_joyconConnection_R.AddObserver(this);
        _joyconConnection_R.ConnectToJoyCon();
        Debug.Log("Joyconが見つかった！");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SendSubCommand0x00()
    {
        byte[] buf = new byte[20];
        _joyconConnection_R.SendSubCommandAsync(new byte[] { 0x00 }, 1, buf,20, _cancellationToken).Forget();
    }
}
