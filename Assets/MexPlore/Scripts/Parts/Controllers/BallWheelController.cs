using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallWheelController : BaseController
{
    [Header( "Variables" )]
    public float LeanMax = 1;
    public float LeanHeightMultipier = 0.25f;
    public float BodyHeightOffset = 4;
    public float RollForce = 10;

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

    private void FixedUpdate()
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
        Body.transform.position = FootPos + Vector3.up * ( CurrentBodyHeight - ( dir.magnitude ) * LeanHeightMultipier ) + dir * LeanMax;
        Foot.position = FootPos;
        if ( dir.magnitude != 0 )
        {
            Body.GetComponent<MechBody>().SetTargetDirection( dir.normalized );
        }

        // Knee should be in opposite direction of dir
        Knee.position = Body.transform.position - dir * 10;

        // Move in lean direction
        Body.GetComponent<Rigidbody>().AddForce( dir * RollForce, ForceMode.Force );
        FootPos += dir * RollForce;

        // Raycast on to ground
        RaycastHit hit;
        int mask = 1 << LayerMask.NameToLayer( "Ground" );
        float raydist = 10000;
        Vector3 start = FootPos + Vector3.up * raydist / 4;
        Vector3 raydir = -Vector3.up;
        if ( Physics.Raycast( start, raydir, out hit, raydist, mask ) )
        {
            FootPos = hit.point;
        }
    }
}
