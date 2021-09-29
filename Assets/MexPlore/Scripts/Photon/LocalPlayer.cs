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

	private void Awake()
	{
		Instance = this;
	}

	void Start()
    {
		LastPos = transform.position;

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
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

		if ( LastPos != transform.position )
		{
			Direction = LastPos - transform.position;
			LastPos = transform.position;
		}

		if ( Input.GetButtonDown( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_CURSOR_RELEASE ) ) )
		{
			if ( Cursor.visible )
			{
				Cursor.visible = false;
				Cursor.lockState = CursorLockMode.Locked;
			}
			else
			{
				Cursor.visible = true;
				Cursor.lockState = CursorLockMode.None;
			}
		}
    }
}
