using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayer : MonoBehaviour
{
	public static LocalPlayer Instance;

	public GameObject Camera;

	public Player Player;

	[HideInInspector]
	public Vector3 Direction;
	private Vector3 LastPos;
	private bool grounded = true;

	private void Awake()
	{
		Instance = this;
	}

	void Start()
    {
		LastPos = transform.position;
	}

	public void OnSpawn()
	{
		//Player.SetAnimal( Player.Animal.Chick );
		Camera.GetComponent<OrbitCamera>().focus = Player.transform;
		Player.transform.parent = transform.parent;
	}

	void Update()
    {
		if ( Player == null ) return;

		//var current =  GetComponent<NaughtyCharacter.Character>().IsGrounded;
		//if ( !grounded && current )
		//{
		//	StaticHelpers.EmitParticleDust( transform.position );
		//}
		//grounded = current;

		if ( LastPos != transform.position )
		{
			Direction = LastPos - transform.position;
			LastPos = transform.position;
		}
    }
}
