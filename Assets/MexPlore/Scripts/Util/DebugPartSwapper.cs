using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPartSwapper : MonoBehaviour
{
    public GameObject[] Parts;

    void Start()
    {
        foreach ( var part in Parts )
        {
            part.SetActive( false );
        }
        Parts[0].SetActive( true );
    }

    void Update()
    {
        for ( int i = 0; i <= 9; i++ )
        {
            if ( Input.GetKey( KeyCode.Alpha1 + i ) )
            {
                foreach ( var part in Parts )
                {
                    part.SetActive( false );
                }
                Parts[i].SetActive( true );

                // Raycast on to ground
                RaycastHit hit;
                int mask = 1 << LayerMask.NameToLayer( "Ground" );
                float raydist = 10000;
                Vector3 start = transform.position + Vector3.up * raydist / 4;
                Vector3 raydir = -Vector3.up;
                if ( Physics.Raycast( start, raydir, out hit, raydist, mask ) )
                {
                    transform.position = hit.point;
                }
            }
        }
    }
}
