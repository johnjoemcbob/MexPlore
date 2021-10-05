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

	[HideInInspector]
	public string CrossoverMech;
	[HideInInspector]
	public GameObject CrossoverMechInstance;
	[HideInInspector]
	public int CrossoverMaterial;

	private PhotonView PhotonView;

	#region MonoBehaviour
	void Awake()
    {
		PhotonView = GetComponent<PhotonView>();
	}

	private void Start()
	{
		// Request sync from other players on late join
		foreach ( var player in FindObjectsOfType<Player>() )
		{
			if ( player != this )
			{
				// Send every other player a request for their details from me
				player.photonView.RPC( "RequestSync", RpcTarget.All, photonView.Owner );
			}
		}
	}

	private void OnDestroy()
	{
		var body = GetComponentInParent<MechBody>();
		if ( body != null )
		{
			body.GetComponentInChildren<MechCockpitDock>().UnDock( true );
		}
	}
	#endregion

	#region RPCs
	// Callback for all current player to react to this new player
	// Called in this instance on all network player machines
	public void OnJoined()
	{
		// Request sync from other players on late join
		//Debug.Log( "I'm late!! " + name );
		//foreach ( var player in FindObjectsOfType<Player>() )
		//{
		//	if ( player != this )
		//	{
		//		// Send every other player a request for their details from me
		//		player.photonView.RPC( "RequestSync", RpcTarget.All, photonView.Owner );
		//		Debug.Log( "Requesting sync from player; " + player.name );
		//	}
		//}
	}

	[PunRPC]
	void RequestSync( Photon.Realtime.Player sendto )
	{
		PhotonView view = LocalPlayer.Instance.Player.photonView;

		// Create any new mechs as needed
		if ( CrossoverMech != "" )
		{
			view.RPC( "SendCrossoverMech", sendto, CrossoverMech, CrossoverMaterial, SystemInfo.deviceUniqueIdentifier );
		}

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

		// Fallen buildings are synced by all
		foreach ( var building in FindObjectsOfType<Building>() )
		{
			if ( building.Fallen )
			{
				view.RPC( "SendBuildingFallen", sendto, building.name, building.FallPos );
			}
		}
	}

	public void LeaveRoom()
	{
		var body = transform.GetComponentInParent<MechBody>();
		if ( body != null )
		{
			string mechname = body.name;
			PhotonView.RPC( "SendRemoveMech", RpcTarget.Others, mechname );
		}
	}

	[PunRPC]
	void SendRemoveMech( string mechname )
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
		Destroy( mech.gameObject );
	}


	[PunRPC]
	void SendCrossoverMech( string crossovermech, int material, string id )
	{
		Game.Instance.CreateCrossoverMech( crossovermech, material, id );
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
		StartCoroutine( TryDock( mechname ) );
	}

	IEnumerator TryDock( string mechname )
	{
		// Find local instance of the mech by name
		MechBody mech = MexPlore.FindMechByName( mechname );
		if ( mech != null )
		{
			var dock = mech.GetComponentInChildren<MechCockpitDock>();

			// Set this player as docked inside
			dock.Dock( GetComponent<HeliCockpit>(), true );
		}
		else
		{
			yield return new WaitForSeconds( 0.5f );

			StartCoroutine( TryDock( mechname ) );
		}
		yield break;
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
		dock.UnDock( false, true );
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

	public void KnockBuilding( string buildingname, Vector3 dmgpos )
	{
		PhotonView.RPC( "SendKnockBuilding", RpcTarget.Others, buildingname, dmgpos );
	}

	[PunRPC]
	void SendKnockBuilding( string buildingname, Vector3 dmgpos )
	{
		Building.FindByName( buildingname ).NetworkFall( dmgpos );
	}

	[PunRPC]
	void SendBuildingFallen( string buildingname, Vector3 dmgpos )
	{
		Building.FindByName( buildingname ).NetworkFall( dmgpos, true );
	}

	#endregion
}
