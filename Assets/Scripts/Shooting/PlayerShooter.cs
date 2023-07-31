using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public GameObject ArrowGObject;
    public GameObject BulletObject;
    public GameObject ReticleObject;
    public GameObject CanvasObj;
    private RectTransform _canvasRect;
    private ArrowAttacker _arrowAttacker;

    public GameObject TestObject;
    // Start is called before the first frame update
    void Start()
    {
        _canvasRect = CanvasObj.GetComponent<RectTransform>();
        _arrowAttacker = ArrowGObject.GetComponent<ArrowAttacker>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        Vector3 ReticleWorldPos = RTransform_MyUtil.OverLayRectToWorld(ReticleObject.transform.position, _canvasRect, Camera.main, 20);
        //TestObject.transform.position = ReticleWorldPos;
        if (Input.GetKeyDown(KeyCode.A))
        {
            
            Quaternion bulletPose=V3_MyUtil.RotateV2V(Vector3.forward, ReticleWorldPos-this.transform.position);
            RaycastHit hit;
            Physics.Raycast(Camera.main.transform.position, ReticleWorldPos - Camera.main.transform.position, out hit,500);
            if (hit.collider)
            {
                bulletPose= V3_MyUtil.RotateV2V(Vector3.forward, hit.collider.transform.position - this.transform.position);
                Debug.Log("RayHit");
            }

            Instantiate(BulletObject,this.transform.position+ this.transform.rotation*new Vector3(0,0,1), bulletPose);
        }
        */
        shotCoolTime -= Time.deltaTime;



    }

    float _shotRate = 4f/30f;
    float shotCoolTime=0;

    public void ShootBullet(Vector3 shotDirection)
    {
        if (shotCoolTime <= 0)
        {
            Quaternion bulletPose = V3_MyUtil.RotateV2V(Vector3.forward, shotDirection);
            Instantiate(BulletObject, this.transform.position + this.transform.rotation * new Vector3(0, 0, 1), bulletPose);
        }
        
    }
    public void ShootBulletTo(Vector3 aimTo)
    {

        if (shotCoolTime <= 0)
        {
            Vector3 muzzlePos = this.transform.position + this.transform.rotation * new Vector3(0, 0, 1);
            Quaternion bulletPose = V3_MyUtil.RotateV2V(Vector3.forward, aimTo - muzzlePos);
            Instantiate(BulletObject, muzzlePos, bulletPose);
            shotCoolTime = _shotRate;
        }
        
    }

    public float FillingRate { get; private set; } = 0;
    [SerializeField] float maxChargeRate=1f/30f;
    [SerializeField] float _arrowMinDamage;
    [SerializeField] float _arrowLinearMaxDamage;
    [SerializeField] float _arrowFullChargedDamage;
    public void ChargeArrow(float chargeRate)
    {
        FillingRate = Mathf.Min(1, FillingRate + Mathf.Min(chargeRate,maxChargeRate));
        //Debug.Log($"FillingRate:{FillingRate}");
    } 
    public void ShootArrowTo(Vector3 aimTo)
    {
        if (FillingRate > 0)
        {
            Vector3 muzzlePos = this.transform.position + this.transform.rotation * new Vector3(0, 0, 1);
            Quaternion bulletPose = V3_MyUtil.RotateV2V(Vector3.forward, aimTo - muzzlePos);
            _arrowAttacker.ChargeRate = FillingRate;
            Instantiate(ArrowGObject, muzzlePos, bulletPose);
            FillingRate = 0;
        }
        
    }

    
    
}
