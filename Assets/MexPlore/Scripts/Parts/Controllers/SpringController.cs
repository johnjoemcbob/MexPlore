using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringController : BaseController
{
    [Header( "Variables" )]
    public float LeanMax = 1;
    public float LeanHeightMultipier = 0.25f;
    public float SpringForce = 100;
    public float SpringUpForce = 100;
    public float BodyHeightOffset = 1;
    public float FootHoverHeightOffset = 0.5f;

    [Header( "References" )]
    public GameObject Body;
    public Transform Knee;
    public Transform Foot;

    private Vector3 FootPos;
    private float CurrentBodyHeight = 0;

    private void OnEnable()
    {
        FootPos = Foot.position;
    }

    private void Update()
    {
        if ( !IsMainController ) return;

        if ( Body.GetComponent<Rigidbody>().isKinematic )
        {
            Vector3 dir = MexPlore.GetCameraDirectionalInput();

            // Lean body towards forward dir based on camera
            CurrentBodyHeight = Mathf.Lerp( CurrentBodyHeight, BodyHeightOffset, Time.deltaTime * 5 );
            Body.GetComponent<MechBody>().SetTargetPos( FootPos + Vector3.up * ( CurrentBodyHeight - ( 1 - dir.magnitude ) * LeanHeightMultipier ) + dir * LeanMax );
            Foot.position = FootPos;
            if ( dir.magnitude != 0 )
            {
                Body.GetComponent<MechBody>().SetTargetDirection( dir.normalized );
            }

            // Knee should be in opposite direction of dir
            Knee.position = Body.transform.position - dir * 10;

            // Space launches in that direction, disables iskinematic
            if ( Input.GetButtonDown( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_MECH_JUMP ) ) )
            {
                Body.GetComponent<Rigidbody>().isKinematic = false;
                Body.GetComponent<Rigidbody>().AddForce( dir * SpringForce + Vector3.up * SpringUpForce, ForceMode.Impulse );

                // Fold in the spring
                Foot.position = Body.transform.position - Vector3.up * FootHoverHeightOffset;
                CurrentBodyHeight = 0;
            }
        }
        else
        {
            Knee.position = Body.transform.position - Body.GetComponent<Rigidbody>().velocity * 5;
        }
    }

    public override void OnCollisionEnter( Collision collision )
    {
        base.OnCollisionEnter( collision );

        var mech = GetComponentInParent<MechBody>();
        var othermech = collision.collider.GetComponentInParent<MechBody>();
        if ( othermech == null || othermech != mech )
        {
            // On collision, freeze again and hand extends from collision point to give mechbody proper height again
            FootPos = collision.GetContact( 0 ).point;
            Body.GetComponent<Rigidbody>().isKinematic = true;
            CurrentBodyHeight = 0;
        }
    }
}
