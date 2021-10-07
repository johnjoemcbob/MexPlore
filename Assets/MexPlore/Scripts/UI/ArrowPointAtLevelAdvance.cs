using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowPointAtLevelAdvance : MonoBehaviour
{
    public float AngleOffset = 0;

    private Transform LevelAdvance;
    private Vector3 InitialAng;
    private MechBody Body;

	private void Awake()
	{
        InitialAng = transform.localEulerAngles;
    }

	void Update()
    {
        Body = GetComponentInParent<MechBody>();
        if ( Body != null )
		{
            transform.position = Body.GetComponentInChildren<Torso>().transform.position;
            transform.GetChild( 0 ).gameObject.SetActive( true );
        }
        else
		{
            transform.GetChild( 0 ).gameObject.SetActive( false );
		}

        if ( LevelAdvance == null )
        {
            var obj = GameObject.FindGameObjectWithTag( "LevelAdvance" );
            if ( obj != null )
            {
                LevelAdvance = obj.transform;
            }
        }

        if ( LevelAdvance != null )
        {
            transform.LookAt( LevelAdvance );
            transform.eulerAngles = new Vector3( 0, transform.eulerAngles.y, 0 );
        }
    }
}
