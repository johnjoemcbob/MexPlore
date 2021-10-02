using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class Player : MonoBehaviourPun
{
	[Header( "Variables" )]
	public string Name;
	public float Scale = 1;

	[Header( "References" )]
	public Text NameTag;

	private PhotonView PhotonView;

	void Awake()
    {
		PhotonView = GetComponent<PhotonView>();
	}

	private void OnDestroy()
	{
		var body = GetComponentInParent<MechBody>();
		if ( body != null )
		{
			body.GetComponentInChildren<MechCockpitDock>().UnDock( true );
		}
	}

	public void OnJoined()
	{
		// Request sync from other players on late join
		PhotonView.RPC( "RequestSync", RpcTarget.Others, photonView.Owner );
	}

	public void SetName( string name )
	{
		Name = name;
		NameTag.text = "";

		PhotonView.RPC( "SendName", RpcTarget.Others, name );
	}

	[PunRPC]
	void SendName( string name )
	{
		Name = name;
	}

	[PunRPC]
	void RequestSync( Photon.Realtime.Player sendto )
	{
		PhotonView view = LocalPlayer.Instance.Player.photonView;

		// Send my name directly back to the player who requested the info
		view.RPC( "SendName", sendto, Name );
		// Sync self mech in inside one
		var body = LocalPlayer.Instance.Player.GetComponentInParent<MechBody>();
		if ( body != null )
		{
			view.RPC( "SendDock", sendto, body.name );
		}

		// Empty mechs are synced by all
		foreach ( var mech in FindObjectsOfType<MechCockpitDock>() )
		{
			if ( mech.CanDock )
			{
				body = mech.GetComponentInParent<MechBody>();
				if ( body != null )
				{
					view.RPC( "SendEmptyMech", sendto, body.name, body.transform.position, body.transform.rotation );
				}
			}
		}
	}

	[PunRPC]
	void SendEmptyMech( string mechname, Vector3 pos, Quaternion rot )
	{
		// Find local instance of the mech by name
		MechBody mech = null;
		foreach ( var trymech in FindObjectsOfType<MechBody>() )
		{
			if ( trymech.name == mechname )
			{
				mech = trymech;
			}
		}
		mech.SetTargetPos( pos );
		mech.transform.rotation = rot;
	}

	public void Dock( MechCockpitDock dock )
	{
		string mechname = dock.GetComponentInParent<MechBody>().name;
		PhotonView.RPC( "SendDock", RpcTarget.Others, mechname );
	}

	[PunRPC]
	void SendDock( string mechname )
	{
		// Find local instance of the mech by name
		MechBody mech = MexPlore.FindMechByName( mechname );
		var dock = mech.GetComponentInChildren<MechCockpitDock>();

		// Set this player as docked inside
		dock.Dock( GetComponent<HeliCockpit>() );
	}

	public void UnDock( MechCockpitDock dock )
	{
		string mechname = dock.GetComponentInParent<MechBody>().name;
		PhotonView.RPC( "SendUnDock", RpcTarget.Others, mechname );
	}

	[PunRPC]
	void SendUnDock( string mechname )
	{
		// Find local instance of the mech by name
		MechBody mech = MexPlore.FindMechByName( mechname );
		var dock = mech.GetComponentInChildren<MechCockpitDock>();

		// Set this player free
		dock.UnDock();
	}

	public void Voice( MechVoice.VoiceInfo info )
	{
		string mechname = GetComponentInParent<MechBody>().name;

		PhotonView.RPC( "SendVoice", RpcTarget.Others, mechname, info.Pitches, info.Delays );
	}

	[PunRPC]
	void SendVoice( string mechname, float[] pitches, float[] delays )
	{
		var body = MexPlore.FindMechByName( mechname );
		MechVoice.VoiceInfo info = new MechVoice.VoiceInfo();
		{
			info.Pitches = pitches;
			info.Delays = delays;
		}
		body.GetComponentInChildren<MechVoice>().PlayVoice( info );
	}
}
