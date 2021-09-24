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
            }
        }
    }
}
