using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeliCockpit : MonoBehaviour
{
    [Header( "Variables" )]
    public float RotorLerpSpeed = 5;
    public float RotorLookMultiplier = 0.5f;
    public float RotorLookY = -0.5f;
    public float BladeLerpSpeed = 1;
    public float BladeMaxSpeed = 1;
    public float BladeVisualSpeedMultiplier = 1;
    public float BladeVisualOffsetMultiplier = 1;
    public float BladeMaxForce = 5;
    public float BladeUpwardForce = 5;

    [Header( "References" )]
    public Transform Rotor;
    public Transform[] Blades;

    private Vector3 LastDirection = Vector3.zero;
    private float CurrentBladeSpeed = 0;

	private void Start()
	{
        AddRigidbody();
	}

	void FixedUpdate()
    {
        // Get input
        bool thrust = Input.GetButton( "Jump" );
        Vector3 dir = MexPlore.GetCameraDirectionalInput().normalized;
        if ( dir != Vector3.zero )
		{
            //dir = LastDirection;
            dir = dir * RotorLookMultiplier + new Vector3( 0, RotorLookY, 0 );
        }
        //LastDirection = dir;

        // Turn rotor
        Quaternion target = Quaternion.LookRotation( dir, Vector3.up );
        Rotor.rotation = Quaternion.Lerp( Rotor.rotation, target, Time.deltaTime * RotorLerpSpeed );

        // Add blade spin speed with input
        float targetspeed = 0;
        if ( thrust )
		{
            targetspeed = BladeMaxSpeed;
		}
        CurrentBladeSpeed = Mathf.Lerp( CurrentBladeSpeed, targetspeed, Time.deltaTime * BladeLerpSpeed );

        // Spin blades by speed
        int i = 0;
		foreach ( var blade in Blades )
		{
            blade.localEulerAngles += new Vector3( 0, 0, 1 ) * Time.deltaTime * CurrentBladeSpeed * BladeVisualSpeedMultiplier * ( ( i + 1 ) * BladeVisualOffsetMultiplier );
            i++;
		}

        // Apply force towards rotor direction * space bar
        GetComponent<Rigidbody>().AddForce( Rotor.up * CurrentBladeSpeed * BladeMaxForce + Vector3.up * CurrentBladeSpeed * BladeUpwardForce, ForceMode.Acceleration );
    }

    public void AddRigidbody()
	{
        var body = gameObject.AddComponent<Rigidbody>();
        {
            body.mass = 1;
            body.drag = 0;
            body.angularDrag = 0.05f;
            body.useGravity = true;
            body.isKinematic = false;
            body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }
    }
}
