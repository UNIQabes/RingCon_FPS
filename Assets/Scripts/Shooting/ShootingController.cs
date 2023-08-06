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

    [SerializeField] private float _maxXRot_xyOrder_deg;
    [SerializeField] private float _maxYRot_xyOrder_deg;

    [SerializeField] private float _XRot_xyOrder_ResetYRot_deg = -75;

    public ShootingControllerMode ControllerMode=ShootingControllerMode.JoyCon;


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
        float maxYRot_xyOrder = _maxYRot_xyOrder_deg*Mathf.PI/180f;
        float maxXRot_xyOrder = _maxXRot_xyOrder_deg *Mathf.PI / 180f;
        float _XRot_xyOrder_ResetYRot= _XRot_xyOrder_ResetYRot_deg * Mathf.PI / 180f;
        bool isReticleFix = false;
        RaycastHit hit;
        if (ControllerMode == ShootingControllerMode.JoyCon)
        {
            if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.JoyConIsReady)
            {
                DebugOnGUI.Log($"R:{MainJoyconInput.RButton} ZR:{MainJoyconInput.ZRButton}", "ddd");
                bool RingConAttached = MainJoyconInput.ringconStrain > 100;
                isReticleFix = !(Mathf.Abs(MainJoyconInput.ringconStrain - prevRingconStrain) < 20);
                Vector3 ringconFrontV = MainJoyconInput.SmoothedPose_R_Arrow * new Vector3(1, 0, 0);
                Vector3 ringconFrontV_InGame = new Vector3(-ringconFrontV.z, -ringconFrontV.y, ringconFrontV.x);

                float xRot_xyOrder = Mathf.Atan2(ringconFrontV_InGame.y, Mathf.Sqrt(Mathf.Pow(ringconFrontV_InGame.x, 2) + Mathf.Pow(ringconFrontV_InGame.z, 2)));
                float yRot_xyOrder = Mathf.Atan2(ringconFrontV_InGame.x, ringconFrontV_InGame.z);

                //_reticleGobj.transform.position = new Vector3(604 + 600 * (yRot_xyOrder), 320 + 400 * (xRot_xyOrder), 0);
                float minX_canvas, minY_canvas, maxX_canvas, maxY_canvas;
                (minX_canvas, minY_canvas, maxX_canvas, maxY_canvas) = RTransform_MyUtil.GetCanvasXYRange(_canvasRect);

                if (!isReticleFix)
                {
                    _reticleGobj.transform.position = new Vector3(Mathf.Clamp(yRot_xyOrder / maxYRot_xyOrder / 2 + 0.5f, 0, 1) * (maxX_canvas - minX_canvas) + minX_canvas,
                        Mathf.Clamp(xRot_xyOrder / maxXRot_xyOrder / 2 + 0.5f, 0, 1) * (maxY_canvas - minY_canvas) + minY_canvas, 0);
                }

                if (xRot_xyOrder < _XRot_xyOrder_ResetYRot)
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
                else if (RingConAttached & (MainJoyconInput.ringconStrain < 3500 || (arrowCharged & MainJoyconInput.ringconStrain < 3200)) | MainJoyconInput.ZRButton)
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
            //bool RingConAttached = MainJoyconInput.ringconStrain > 100;
            //isReticleFix = !(Mathf.Abs(MainJoyconInput.ringconStrain - prevRingconStrain) < 20);
            //Vector3 ringconFrontV = MainJoyconInput.SmoothedPose_R_Arrow * new Vector3(1, 0, 0);
            //Vector3 ringconFrontV_InGame = new Vector3(-ringconFrontV.z, -ringconFrontV.y, ringconFrontV.x);

            //float xRot_xyOrder = Mathf.Atan2(ringconFrontV_InGame.y, Mathf.Sqrt(Mathf.Pow(ringconFrontV_InGame.x, 2) + Mathf.Pow(ringconFrontV_InGame.z, 2)));
            //float yRot_xyOrder = Mathf.Atan2(ringconFrontV_InGame.x, ringconFrontV_InGame.z);

            ////_reticleGobj.transform.position = new Vector3(604 + 600 * (yRot_xyOrder), 320 + 400 * (xRot_xyOrder), 0);
            //float minX_canvas, minY_canvas, maxX_canvas, maxY_canvas;
            //(minX_canvas, minY_canvas, maxX_canvas, maxY_canvas) = RTransform_MyUtil.GetCanvasXYRange(_canvasRect);

            //if (!isReticleFix)
            //{
            //    _reticleGobj.transform.position = new Vector3(Mathf.Clamp(yRot_xyOrder / maxYRot_xyOrder / 2 + 0.5f, 0, 1) * (maxX_canvas - minX_canvas) + minX_canvas,
            //        Mathf.Clamp(xRot_xyOrder / maxXRot_xyOrder / 2 + 0.5f, 0, 1) * (maxY_canvas - minY_canvas) + minY_canvas, 0);
            //}
            Vector2 reticlePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect,Input.mousePosition,null,out reticlePos);
            _reticleGobj.transform.localPosition = new Vector3(reticlePos.x, reticlePos.y,0);


            Vector3 ReticleWorldPos = RTransform_MyUtil.OverLayRectToWorld(_reticleGobj.transform.position, _canvasRect, Camera.main, 20);
            Vector3 shotDirection = ReticleWorldPos - _playerShooter.transform.position;
            Vector3 shotPoint = ReticleWorldPos;

            Physics.Raycast(Camera.main.transform.position, ReticleWorldPos - Camera.main.transform.position, out hit, 500);
            if (hit.collider)
            {
                shotDirection = hit.collider.transform.position - _playerShooter.transform.position;
                shotPoint = hit.point;
            }

            if (Input.GetMouseButton(0))
            {
                _playerShooter.ShootBulletTo(shotPoint);
            }
            else if (Input.GetMouseButton(1))
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






            //    if (xRot_xyOrder < _XRot_xyOrder_ResetYRot)
            //    {
            //        //Debug.Log(_XRot_xyOrder_ResetYRot);
            //        //Debug.Log("リセットした~");
            //        MainJoyconInput.ResetYRot_xyOrder();
            //    }
            //    Vector3 ReticleWorldPos = RTransform_MyUtil.OverLayRectToWorld(_reticleGobj.transform.position, _canvasRect, Camera.main, 20);
            //    Vector3 shotDirection = ReticleWorldPos - _playerShooter.transform.position;
            //    Vector3 shotPoint = ReticleWorldPos;

            //    Physics.Raycast(Camera.main.transform.position, ReticleWorldPos - Camera.main.transform.position, out hit, 500);




            //    if (hit.collider)
            //    {
            //        shotDirection = hit.collider.transform.position - _playerShooter.transform.position;
            //        shotPoint = hit.point;

            //        //Debug.Log($"RayHit:{hit.collider.gameObject.name}");
            //    }

            //    if ((RingConAttached & MainJoyconInput.ringconStrain > 5000) | MainJoyconInput.RButton)
            //    {
            //        _playerShooter.ShootBulletTo(shotPoint);
            //    }
            //    else if (RingConAttached & (MainJoyconInput.ringconStrain < 3500 || (arrowCharged & MainJoyconInput.ringconStrain < 3200)) | MainJoyconInput.ZRButton)
            //    {
            //        arrowCharged = true;
            //        //Debug.Log($"ひっぱてるよ〜{MainJoyconInput.ringconStrain}");
            //        _playerShooter.ChargeArrow(1);
            //    }
            //    else
            //    {
            //        arrowCharged = false;
            //        _playerShooter.ShootArrowTo(shotPoint);
            //    }

            //    _reticleGraphic.SetfilledRate(_playerShooter.FillingRate);
            //    _reticleGraphic.SetIsRockedOn(hit.collider);
            //    _reticleGraphic.SetIsReticleFixed(isReticleFix);
            //    prevRingconStrain = MainJoyconInput.ringconStrain;
        }








        //DebugOnGUI.Log(MainJoyconInput.ringconStrain);
    }

    private void OnGUI()
    {
        DebugOnGUI.Log($"RingconStrain:{MainJoyconInput.ringconStrain}","a");
    }
}


public enum ShootingControllerMode
{
    KeyMouse,
    RingCon,
    JoyCon
} 