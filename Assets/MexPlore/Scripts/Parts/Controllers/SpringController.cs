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
        if ( Body.GetComponent<Rigidbody>().isKinematic )
        {
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            Vector3 right = Camera.main.transform.right;
            right.y = 0;
            var hor = Input.GetAxis( "Horizontal" );
            var ver = Input.GetAxis( "Vertical" );
            Vector3 dir = forward * ver + right * hor;
            if ( dir.magnitude > 1 )
            {
                dir /= dir.magnitude;
            }

            // Lean body towards forward dir based on camera
            CurrentBodyHeight = Mathf.Lerp( CurrentBodyHeight, BodyHeightOffset, Time.deltaTime * 5 );
            Body.transform.position = FootPos + Vector3.up * ( CurrentBodyHeight - ( 1 - dir.magnitude ) * LeanHeightMultipier ) + dir * LeanMax;
            Foot.position = FootPos;

            // Knee should be in opposite direction of dir
            Knee.position = Body.transform.position - dir * 10;

            // Space launches in that direction, disables iskinematic
            if ( Input.GetButtonDown( "Jump" ) )
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

        // On collision, freeze again and hand extends from collision point to give mechbody proper height again
        FootPos = collision.GetContact( 0 ).point;
        Body.GetComponent<Rigidbody>().isKinematic = true;
        CurrentBodyHeight = 0;
    }
}
