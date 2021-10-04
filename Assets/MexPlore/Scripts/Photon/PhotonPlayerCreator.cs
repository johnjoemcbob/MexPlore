using UnityEngine;
using System.Collections;
using Photon.Pun;

public class PhotonPlayerCreator : MonoBehaviourPun
{
	public static Vector3 Spawn = new Vector3( 0, 0, 0 );

	public void OnJoinedRoom()
    {
        CreatePlayerObject();
	}

    void CreatePlayerObject()
    {
		// Find or default spawn position
		Vector3 spawn = GetSpawn();

		// Create
        GameObject newPlayerObject = PhotonNetwork.Instantiate( "NetworkPlayer", spawn, Quaternion.identity, 0 );
		if ( LocalPlayer.Instance.Player == null )
		{
			LocalPlayer.Instance.Player = newPlayerObject.GetComponentInChildren<Player>();
			LocalPlayer.Instance.OnSpawn();

			Game.Instance.OnPlayerSpawnLoadCrossoverMech();

			if ( !Application.isEditor )
			{
				StartCoroutine( SendDiscord() );
			}
		}

		// Callback for all current player to react to this new player
		newPlayerObject.GetComponentInChildren<Player>().OnJoined();
	}

	public static Vector3 GetSpawn()
	{
		var spawn = Spawn;
		var point = GameObject.Find( "SpawnPoint" );
		if ( point != null )
		{
			spawn = point.transform.position;
		}
		return Spawn;
	}

	// Join alert notification
	IEnumerator SendDiscord()
	{
		WWWForm form = new WWWForm();
		var headers = form.headers;
		headers["X-Parse-Application-Id"] = "AppId";
		headers["X-Parse-REST-API-Key"] = "RestKey";
		headers["Content-Type"] = "application/json";
		WWW www = new WWW("https://discord-queer-horizon.glitch.me/mexplore/" + PhotonNetwork.CloudRegion + "/", null, headers);
		while ( !www.isDone )
			yield return 1;
	}
}
