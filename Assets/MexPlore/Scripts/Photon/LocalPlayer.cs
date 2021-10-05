using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalPlayer : MonoBehaviour
{
	public static LocalPlayer Instance;

	public enum State
	{
		Movement,
		UI
	}

	public OrbitCamera Camera;

	public Player Player;

	[HideInInspector]
	public State CurrentState;

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

		StartState( State.Movement );
	}

	void Update()
    {
		if ( Player == null ) return;

		// Constantly find camera in case on new scene load
		if ( Camera == null )
		{
			Camera = FindObjectOfType<OrbitCamera>();
			Camera.focus = Player.transform;
		}

		if ( LastPos != transform.position )
		{
			Direction = LastPos - transform.position;
			LastPos = transform.position;
		}

		if ( Input.GetButtonDown( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_RESET ) ) )
		{
			var body = Player.GetComponentInParent<MechBody>();
			if ( body == null )
			{
				// If not in a mech, reset heli
				Player.transform.position = Vector3.zero;
			}
			else
			{
				// Otherwise reset mech
				body.Reset();
			}

			// If it brought a crossover mech and that mech is currently empty then reset it also
			if ( Player.CrossoverMechInstance != null )
			{
				if ( Player.CrossoverMechInstance.GetComponentInChildren<HeliCockpit>() == null )
				{
					Player.CrossoverMechInstance.GetComponent<MechBody>().Reset();
				}
			}
		}
	}

	public void OnSpawn()
	{
		//Player.transform.parent = transform.parent;
	}

	public bool SwitchState( State state )
	{
		if ( CurrentState == state ) return false;

		FinishState( CurrentState );
		CurrentState = state;
		StartState( CurrentState );

		return true;
	}

	void StartState( State state )
	{
		switch ( state )
		{
			case State.Movement:
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				break;
			case State.UI:
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				break;
			default:
				break;
		}
	}

	void FinishState( State state )
	{
		switch ( state )
		{
			case State.Movement:
				break;
			case State.UI:
				break;
			default:
				break;
		}
	}

	public static bool CanInput()
	{
		return Instance != null && Instance.CurrentState == State.Movement;
	}
}
