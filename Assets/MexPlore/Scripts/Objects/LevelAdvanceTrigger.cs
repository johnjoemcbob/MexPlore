using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAdvanceTrigger : MonoBehaviour
{
	public int LevelToLoad = 1;

	private void OnTriggerEnter( Collider other )
	{
		var player = other.GetComponentInParent<Player>();
		if ( player != null && player == LocalPlayer.Instance.Player )
		{
			Game.Instance.LoadLevel( LevelToLoad );
		}
	}
}
