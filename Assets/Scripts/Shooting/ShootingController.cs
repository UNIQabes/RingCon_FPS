using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootingController : MonoBehaviour
{
    [SerializeField] private PlayerShooter _playerShooter;
    [SerializeField] private GameObject _reticleGobj;
    [SerializeField] private Image _chargeCircleImage;
    [SerializeField] private RectTransform _canvasRect;
    [SerializeField] private ReticleGraphic _reticleGraphic;

    [SerializeField] public float MaxXRot_xyOrder_deg;
    [SerializeField] public float MaxYRot_xyOrder_deg;
    [SerializeField] public float XRot_xyOrder_ResetYRot_deg = -75;
    [SerializeField] public float XRot_Offset_xyOrder_deg=0;

    [SerializeField] public float GyroDriftCalibration_YRot_xyOrder = 2.5f;


    [SerializeField] public float XSensetivity_KeyMouseMode=0;
    [SerializeField] public float YSensetivity_KeyMouseMode=0;

    [SerializeField] public ShootingControllerMode ControllerMode=ShootingControllerMode.JoyCon;


    // Start is called before the first frame update
    void Start()
    {
        arrowCharged = false;
    }

    void Update()
    {
    }

    bool arrowCharged;
    private float prevRingconStrain=0;
    void FixedUpdate()
    {
        float maxYRot_xyOrder = MaxYRot_xyOrder_deg * Mathf.PI/180f;
        float maxXRot_xyOrder = MaxXRot_xyOrder_deg * Mathf.PI / 180f;
        float XRot_xyOrder_ResetYRot= XRot_xyOrder_ResetYRot_deg * Mathf.PI / 180f;
        float XRot_Offset_xyOrder= XRot_Offset_xyOrder_deg * Mathf.PI / 180f;


        bool isReticleFix = false;
        RaycastHit hit;
        if (ControllerMode == ShootingControllerMode.JoyCon)
        {
            if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.JoyConIsReady)
            {
                MainJoyconInput.RotatZRot(GyroDriftCalibration_YRot_xyOrder * Time.fixedDeltaTime) ;
                DebugOnGUI.Log($"R:{MainJoyconInput.RButton} \nZR:{MainJoyconInput.ZRButton} \nA:{MainJoyconInput.AButton} \nRingconStrain:{MainJoyconInput.ringconStrain}", "JoyconButton");
                bool RingConAttached = MainJoyconInput.ringconStrain > 100;
                isReticleFix = !(Mathf.Abs(MainJoyconInput.ringconStrain - prevRingconStrain) < 20);
                Vector3 ringconFrontV = MainJoyconInput.SmoothedPose_R_Arrow * new Vector3(1, 0, 0);
                Vector3 ringconFrontV_InGame = new Vector3(-ringconFrontV.z, -ringconFrontV.y, ringconFrontV.x);

                float xRot_xyOrder = Mathf.Atan2(ringconFrontV_InGame.y, Mathf.Sqrt(Mathf.Pow(ringconFrontV_InGame.x, 2) + Mathf.Pow(ringconFrontV_InGame.z, 2)))+ XRot_Offset_xyOrder;
                float yRot_xyOrder = Mathf.Atan2(ringconFrontV_InGame.x, ringconFrontV_InGame.z);

                //_reticleGobj.transform.position = new Vector3(604 + 600 * (yRot_xyOrder), 320 + 400 * (xRot_xyOrder), 0);
                float minX_canvas, minY_canvas, maxX_canvas, maxY_canvas;
                (minX_canvas, minY_canvas, maxX_canvas, maxY_canvas) = RTransform_MyUtil.GetCanvasXYRange(_canvasRect);

                if (!isReticleFix)
                {
                    _reticleGobj.transform.position = new Vector3(Mathf.Clamp(yRot_xyOrder / maxYRot_xyOrder / 2 + 0.5f, 0, 1) * (maxX_canvas - minX_canvas) + minX_canvas,
                        Mathf.Clamp(xRot_xyOrder / maxXRot_xyOrder / 2 + 0.5f, 0, 1) * (maxY_canvas - minY_canvas) + minY_canvas, 0);
                }

                if (xRot_xyOrder < XRot_xyOrder_ResetYRot|MainJoyconInput.AButton)
                {
                    //Debug.Log(_XRot_xyOrder_ResetYRot);
                    //Debug.Log("リセットした~");
                    MainJoyconInput.ResetYRot_xyOrder();
                }
                Vector3 ReticleWorldPos = RTransform_MyUtil.OverLayRectToWorld(_reticleGobj.transform.position, _canvasRect, Camera.main, 20);
                Vector3 shotDirection = ReticleWorldPos - _playerShooter.transform.position;
                Vector3 shotPoint = ReticleWorldPos;

                Physics.Raycast(Camera.main.transform.position, ReticleWorldPos - Camera.main.transform.position, out hit, 500);




                if (hit.collider)
                {
                    shotDirection = hit.collider.transform.position - _playerShooter.transform.position;
                    shotPoint = hit.point;

                    //Debug.Log($"RayHit:{hit.collider.gameObject.name}");
                }

                if ((RingConAttached & MainJoyconInput.ringconStrain > 5000) | MainJoyconInput.RButton)
                {
                    _playerShooter.ShootBulletTo(shotPoint);
                }
                else if (RingConAttached & (MainJoyconInput.ringconStrain < 3500 || (arrowCharged & MainJoyconInput.ringconStrain < 4000)) | MainJoyconInput.ZRButton)
                {
                    arrowCharged = true;
                    //Debug.Log($"ひっぱてるよ〜{MainJoyconInput.ringconStrain}");
                    _playerShooter.ChargeArrow(1);
                }
                else
                {
                    arrowCharged = false;
                    _playerShooter.ShootArrowTo(shotPoint);
                }

                _reticleGraphic.SetfilledRate(_playerShooter.FillingRate);
                _reticleGraphic.SetIsRockedOn(hit.collider);
                _reticleGraphic.SetIsReticleFixed(isReticleFix);
                prevRingconStrain = MainJoyconInput.ringconStrain;

            }
        }
        else if (ControllerMode == ShootingControllerMode.KeyMouse)
        {
            Vector3 ReticleDelta=(Input.GetKey(KeyCode.W)?new Vector2(0,1):new Vector2(0,0))
                + (Input.GetKey(KeyCode.S) ? new Vector2(0, -1) : new Vector2(0, 0))
                + (Input.GetKey(KeyCode.A) ? new Vector2(-1, 0) : new Vector2(0, 0))
                + (Input.GetKey(KeyCode.D) ? new Vector2(1, 0) : new Vector2(0, 0))
                + (Input.GetKey(KeyCode.UpArrow) ? new Vector2(0, 0.5f) : new Vector2(0, 0))
                + (Input.GetKey(KeyCode.DownArrow) ? new Vector2(0, -0.5f) : new Vector2(0, 0))
                + (Input.GetKey(KeyCode.LeftArrow) ? new Vector2(-0.5f, 0) : new Vector2(0, 0))
                + (Input.GetKey(KeyCode.RightArrow) ? new Vector2(0.5f, 0) : new Vector2(0, 0));
            ReticleDelta.x = ReticleDelta.x * (10 + XSensetivity_KeyMouseMode) / 10;
            ReticleDelta.y = ReticleDelta.y * (10 + YSensetivity_KeyMouseMode) / 10;
            Vector3 reticlePos;
            reticlePos = _reticleGobj.transform.position + ReticleDelta*15;
            float minX_canvas, minY_canvas, maxX_canvas, maxY_canvas;
            (minX_canvas, minY_canvas, maxX_canvas, maxY_canvas) = RTransform_MyUtil.GetCanvasXYRange(_canvasRect);
            if (reticlePos.x < minX_canvas) { reticlePos.x = minX_canvas; }
            if (reticlePos.x > maxX_canvas) { reticlePos.x = maxX_canvas; }
            if (reticlePos.y < minY_canvas) { reticlePos.y = minY_canvas; }
            if (reticlePos.y > maxY_canvas) { reticlePos.y = maxY_canvas; }
            _reticleGobj.transform.position = reticlePos;
            //Debug.Log("うわぁぁぁぁ");

            //Vector2 reticlePos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect,Input.mousePosition,null,out reticlePos);
            //_reticleGobj.transform.localPosition = new Vector3(reticlePos.x, reticlePos.y,0);


            Vector3 ReticleWorldPos = RTransform_MyUtil.OverLayRectToWorld(_reticleGobj.transform.position, _canvasRect, Camera.main, 20);
            Vector3 shotDirection = ReticleWorldPos - _playerShooter.transform.position;
            Vector3 shotPoint = ReticleWorldPos;

            Physics.Raycast(Camera.main.transform.position, ReticleWorldPos - Camera.main.transform.position, out hit, 500);
            if (hit.collider)
            {
                shotDirection = hit.collider.transform.position - _playerShooter.transform.position;
                shotPoint = hit.point;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                _playerShooter.ShootBulletTo(shotPoint);
            }
            else if (Input.GetKey(KeyCode.C))
            {
                arrowCharged = true;
                //Debug.Log($"ひっぱてるよ〜{MainJoyconInput.ringconStrain}");
                _playerShooter.ChargeArrow(1);
            }
            else
            {
                arrowCharged = false;
                _playerShooter.ShootArrowTo(shotPoint);
            }

            _reticleGraphic.SetfilledRate(_playerShooter.FillingRate);
            _reticleGraphic.SetIsRockedOn(hit.collider);
            _reticleGraphic.SetIsReticleFixed(isReticleFix);
            prevRingconStrain = MainJoyconInput.ringconStrain;






            
        }








        //DebugOnGUI.Log(MainJoyconInput.ringconStrain);
    }

    private void OnGUI()
    {
        //DebugOnGUI.Log($"RingconStrain:{MainJoyconInput.ringconStrain}","a");
    }
}


public enum ShootingControllerMode
{
    KeyMouse,
    JoyCon
} 