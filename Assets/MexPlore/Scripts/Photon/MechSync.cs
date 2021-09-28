using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MechSync : MonoBehaviourPun, IPunObservable
{
	private string mechName = "";
	private Vector3 realPosition = Vector3.zero;
	private Quaternion realRotation = Quaternion.identity;

	private Transform Mech;

	void Update()
	{
		if ( photonView.IsMine )
		{

		}
		else
		{
			if ( Mech != null )
			{
				// Just teleport if super far away (especially if looping)
				Mech.GetComponent<MechBody>().SetTargetPos( this.realPosition );
				Mech.rotation = Quaternion.Lerp( Mech.rotation, this.realRotation, 0.1f );
			}
		}
	}

	public void OnPhotonSerializeView( PhotonStream stream, PhotonMessageInfo info )
	{
		if ( stream.IsWriting )
		{
			// Only send when parented to a mech
			var mech = GetComponentInParent<MechBody>();
			if ( mech != null )
			{
				Mech = mech.transform;
				// Basic Info
				stream.SendNext( Mech.name );
				stream.SendNext( Mech.position );
				stream.SendNext( Mech.rotation );

				// Torso
				foreach ( var torso in Mech.GetComponentsInChildren<Torso>() )
				{
					stream.SendNext( torso.CurrentLean );
				}

				// Arms & Legs
				foreach ( var ik in Mech.GetComponentsInChildren<InverseKinematics>() )
				{
					stream.SendNext( ik.TargetTarget.position );
				}

				// Bridges
				foreach ( var bridge in Mech.GetComponentsInChildren<BridgeExtender>() )
				{
					stream.SendNext( bridge.Extension );
				}

				// Incredimech(s?) - This could be a virtual/override MechBody.SyncStatus function!
				foreach ( var incredi in Mech.GetComponentsInChildren<IncrediMech>() )
				{
					stream.SendNext( incredi.GetWalkerState() );
				}
			}
			else
			{
				stream.SendNext( "NULL" );
			}
		}
		else
		{
			this.mechName = (string) stream.ReceiveNext();
			if ( this.mechName != "NULL" )
			{
				// Find local instance here
				Mech = GameObject.Find( this.mechName ).transform;

				// Basic Info
				this.realPosition = (Vector3) stream.ReceiveNext();
				this.realRotation = (Quaternion) stream.ReceiveNext();

				// Torso
				foreach ( var torso in Mech.GetComponentsInChildren<Torso>() )
				{
					torso.CurrentLean = (float) stream.ReceiveNext();
				}

				// Arms & Legs
				foreach ( var ik in Mech.GetComponentsInChildren<InverseKinematics>() )
				{
					ik.TargetTarget.position = (Vector3) stream.ReceiveNext();
				}

				// Bridges
				foreach ( var bridge in Mech.GetComponentsInChildren<BridgeExtender>() )
				{
					bridge.Extension = (float) stream.ReceiveNext();
				}

				// Incredimech(s?) - This could be a virtual/override MechBody.SyncStatus function!
				foreach ( var incredi in Mech.GetComponentsInChildren<IncrediMech>() )
				{
					incredi.SetWalkerState( (bool) stream.ReceiveNext() );
				}
			}
		}
	}
}
