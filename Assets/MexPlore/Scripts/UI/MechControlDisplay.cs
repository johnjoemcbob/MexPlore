using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechControlDisplay : MonoBehaviour
{
    void Update()
    {
		if ( LocalPlayer.Instance.Player == null ) return;

        var body = LocalPlayer.Instance.Player.GetComponentInParent<MechBody>();
        if ( body != null )
		{
			if ( body.GetComponentInChildren<IncrediMech>() != null )
			{
				ShowInstructions( 7 );
			}
			else if ( body.GetComponentInChildren<BoatController>() != null )
			{
				ShowInstructions( 6 );
			}
			else if ( body.GetComponentInChildren<BridgeExtender>() != null )
			{
				ShowInstructions( 5 );
			}
			else if ( body.GetComponentInChildren<WalkController>() != null )
			{
				ShowInstructions( 4 );
			}
			else if ( body.GetComponentInChildren<BallWheelController>() != null )
			{
				ShowInstructions( 3 );
			}
			else if ( body.GetComponentInChildren<SpringController>() != null )
			{
				ShowInstructions( 2 );
			}
			else if ( body.GetComponentInChildren<CrawlController>() != null )
			{
				ShowInstructions( 1 );
			}
		}
		else
		{
			// Heli cockpit
			ShowInstructions( 0 );
		}
    }

	void ShowInstructions( int index )
	{
		foreach ( Transform child in transform )
		{
			child.gameObject.SetActive( false );
		}
		transform.GetChild( index ).gameObject.SetActive( true );
	}
}
