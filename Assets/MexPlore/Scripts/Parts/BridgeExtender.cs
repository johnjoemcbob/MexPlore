using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class BridgeExtender : MonoBehaviour
{
    [Header( "Variables" )]
    public float Extension = 0;
    public float ExtensionMaxAngle = 180;
    public float ExtensionInputSpeed = 5;

    [Header( "References" )]
    public Transform[] Segments;

    void Update()
    {
        // Input if mech enabled
        var body = GetComponentInParent<MechBody>();
        if ( body.IsMainController )
        {
            if ( LocalPlayer.CanInput() )
            {
                bool extend = Input.GetButton( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_BRIDGE_EXTEND ) ) || Extension > 0.1f;
                body.GetComponentInChildren<WalkController>().IsMainController = !extend; // Don't move while extending/retracting bridge
                if ( extend )
                {
                    float forward = Input.GetAxis( MexPlore.GetControl( MexPlore.CONTROL.AXIS_FORWARD ) );
                    Extension = Mathf.Clamp( Extension + forward * Time.deltaTime * ExtensionInputSpeed, 0, 1 );
                }
            }
		}

        // Rotate outwards
        int segind = 0;
        int dir = 1;
        foreach ( var seg in Segments )
		{
            seg.localEulerAngles = new Vector3( 1, 0, 0 ) * dir * Mathf.Min( Extension * ExtensionMaxAngle, ExtensionMaxAngle );
            segind++;
            dir *= -1;
        }
    }
}
