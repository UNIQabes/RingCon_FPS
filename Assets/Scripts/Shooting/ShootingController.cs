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
    

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
    }


    private float prevRingconStrain=0;
    void FixedUpdate()
    {
        float maxYRot_xyOrder = _maxYRot_xyOrder_deg*Mathf.PI/180f;
        float maxXRot_xyOrder = _maxXRot_xyOrder_deg *Mathf.PI / 180f;


        bool isReticleFix = !(Mathf.Abs(MainJoyconInput.ringconStrain - prevRingconStrain) < 20);
        if (MainJoyconInput.ConnectInfo == JoyConConnectInfo.JoyConIsReady)
        {
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
            
        }



        Vector3 ReticleWorldPos = RTransform_MyUtil.OverLayRectToWorld(_reticleGobj.transform.position, _canvasRect, Camera.main, 20);
        Vector3 shotDirection = ReticleWorldPos - _playerShooter.transform.position;
        Vector3 shotPoint = ReticleWorldPos;
        RaycastHit hit;
        Physics.Raycast(Camera.main.transform.position, ReticleWorldPos - Camera.main.transform.position, out hit, 500);



        
        if (hit.collider)
        {
            shotDirection = hit.collider.transform.position - _playerShooter.transform.position;
            shotPoint = hit.point;
            
            //Debug.Log($"RayHit:{hit.collider.gameObject.name}");
        }

        if (MainJoyconInput.ringconStrain > 5000)
        {
            _playerShooter.ShootBulletTo(shotPoint);
        }
        else if (MainJoyconInput.ringconStrain < 3500)
        {
            //Debug.Log($"ひっぱてるよ〜{MainJoyconInput.ringconStrain}");
            _playerShooter.ChargeArrow(1);
        }
        else
        {
            _playerShooter.ShootArrowTo(shotPoint);
        }

        _reticleGraphic.SetfilledRate(_playerShooter.FillingRate);
        _reticleGraphic.SetIsRockedOn(hit.collider);
        _reticleGraphic.SetIsReticleFixed(isReticleFix);
        prevRingconStrain = MainJoyconInput.ringconStrain;
        
        //DebugOnGUI.Log(MainJoyconInput.ringconStrain);
    }

    private void OnGUI()
    {
        DebugOnGUI.Log($"RingconStrain:{MainJoyconInput.ringconStrain}","a");
    }
}
