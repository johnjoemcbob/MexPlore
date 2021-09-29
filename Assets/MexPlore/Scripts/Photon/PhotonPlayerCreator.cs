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
        GameObject newPlayerObject = PhotonNetwork.Instantiate( "NetworkPlayer", Spawn, Quaternion.identity, 0 );
		if ( LocalPlayer.Instance.Player == null )
		{
			LocalPlayer.Instance.Player = newPlayerObject.GetComponentInChildren<Player>();
			LocalPlayer.Instance.Player.OnJoined();
			LocalPlayer.Instance.OnSpawn();

			if ( !Application.isEditor )
			{
				StartCoroutine( SendDiscord() );
			}
		}

		//Camera.Target = newPlayerObject.transform;
		//var character = FindObjectOfType<NaughtyCharacter.Character>( true );
		//character._characterController = newPlayerObject.GetComponentInChildren<CharacterController>( true );
		//character._characterAnimator = newPlayerObject.GetComponentInChildren<NaughtyCharacter.CharacterAnimator>( true );
		//newPlayerObject.GetComponentInChildren<NaughtyCharacter.CharacterAnimator>( true )._character = character;
		//character.transform.SetParent( newPlayerObject.transform );
		//character.transform.localPosition = Vector3.zero;
	}

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
