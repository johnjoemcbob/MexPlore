using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MexPlore
{
    public static Vector3 GetCameraDirectionalInput()
    {
        // Camera
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        Vector3 right = Camera.main.transform.right;
        right.y = 0;

        // Input
        var hor = Input.GetAxis( "Horizontal" );
        var ver = Input.GetAxis( "Vertical" );

        // Together
        Vector3 dir = forward * ver + right * hor;
        {
            // Weird and bad normalisation because < 1 is valid for directional force? or something?
            if ( dir.magnitude > 1 )
            {
                dir /= dir.magnitude;
            }
        }
        return dir;
    }

    public static Vector3 RaycastToGround( Vector3 pos )
    {
        // Raycast on to ground
        RaycastHit hit;
        int mask = 1 << LayerMask.NameToLayer( "Ground" );
        float raydist = 10;
        float updist = 10;
        Vector3 start = pos + Vector3.up * updist;
        Vector3 raydir = -Vector3.up;
        if ( Physics.Raycast( start, raydir, out hit, updist + raydist, mask ) )
        {
            pos = hit.point;
        }
        return pos;
    }
}
