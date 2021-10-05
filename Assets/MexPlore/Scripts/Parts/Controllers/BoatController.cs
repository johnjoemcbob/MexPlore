using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatController : BaseController
{
    [Header( "Variables" )]
    public float PropulsionForce = 5;
    public float PropulsionUpwardMultiplier = 0.5f;
    public float TurnForce = 2.5f;
    public float FloatForce = 5;
    public float FloatHeight = 3;
    public float ExtraGravityHeight = 5;
    public float GravityForce = 5;
    public float GravityPerEngineForce = 1;
    public float RightenForceX = 5;
    public float RightenForceZ = 5;
    public float RiverForce = 1;

    [Header( "References" )]
    public GameObject Body;
    public Transform Propulsion;
    public Transform[] Engines;

    [Header( "Assets" )]
    public AudioClip SoundThrust;
    public AudioClip SoundRotor;

    private Rigidbody Rigidbody;

    private bool Waterborne = false;
    private Vector3 RiverFlow = Vector3.zero;

    private void Awake()
	{
        Rigidbody = GetComponentInParent<Rigidbody>();
    }

	void FixedUpdate()
    {
        if ( IsMainController && LocalPlayer.CanInput() && Waterborne )
        {
            //// Forward
            //Vector3 dir = Input.GetAxis( MexPlore.GetControl( MexPlore.CONTROL.AXIS_FORWARD ) ) * transform.forward;
            //Vector3 target = dir * Time.deltaTime * Speed;

            //Body.GetComponent<MechBody>().SetTargetPos( Body.transform.position + target );

            //// Turn
            //Vector3 turn = Input.GetAxis( MexPlore.GetControl( MexPlore.CONTROL.AXIS_RIGHT ) ) * transform.right;
            //if ( turn != Vector3.zero )
            //{
            //    Vector3 targetang = turn;
            //    Quaternion targetquat = Quaternion.LookRotation( targetang, Vector3.up );
            //    Quaternion quat = Quaternion.Lerp( Body.transform.rotation, targetquat, Time.deltaTime * TurnSpeed ); // use progress?
            //    Vector3 turndir = quat * Vector3.forward;

            //    Body.GetComponent<MechBody>().SetTargetDirection( turndir );
            //}

            // Directional Input
            Vector3 forward = transform.TransformDirection( Vector3.forward );
            {
                forward.y = PropulsionUpwardMultiplier;
            }
            Rigidbody.AddForceAtPosition( Time.deltaTime * forward * Input.GetAxis( "Vertical" ) * PropulsionForce, Propulsion.transform.position );
            Rigidbody.AddTorque( Time.deltaTime * transform.TransformDirection( Vector3.up ) * Input.GetAxis( "Horizontal" ) * TurnForce );
        }

        // If has heli and heli isn't local, then kinematic and trust
        var player = GetComponentInChildren<MechCockpitDock>().GetComponentInChildren<Player>();
        if ( player != null && player != LocalPlayer.Instance.Player )
        {
            Rigidbody.isKinematic = true;
        }
		else
        {
            Rigidbody.isKinematic = false;
            UpdateVehiclePhysics();
        }
    }

    void UpdateVehiclePhysics()
    {
        // River
        Rigidbody.AddForce( Time.deltaTime * RiverFlow * RiverForce );

        // Float
        //Waterborne = true;
        Waterborne = false;
        foreach ( Transform engine in Engines )
        {
            RaycastHit hit;
            int layer = 1 << LayerMask.NameToLayer( "Water" );
            float upoff = 10;
            Vector3 up = transform.TransformDirection( Vector3.up ) * upoff;
            if ( Physics.Raycast( engine.position + up, transform.TransformDirection( Vector3.down ), out hit, FloatHeight + upoff, layer ) )
            {
                Rigidbody.AddForceAtPosition( Time.deltaTime * transform.TransformDirection( Vector3.up ) * Mathf.Pow( ( FloatHeight + upoff ) - hit.distance, 2 ) / 2f * FloatForce, engine.position );

                engine.localScale = Vector3.one;

                Waterborne = true;
            }
            else if ( !Physics.Raycast( engine.position + up, transform.TransformDirection( Vector3.down ), out hit, FloatHeight + upoff + ExtraGravityHeight, layer ) )
            {
                Rigidbody.AddForceAtPosition( Vector3.up * Time.deltaTime * GravityPerEngineForce, engine.position );

                //Waterborne = false;
            }

            engine.localScale = Vector3.Lerp( engine.localScale, Vector3.one * 0.1f, Time.deltaTime * 5 );
        }

        // ???
        Rigidbody.AddForce( -Time.deltaTime * transform.TransformVector( Vector3.right ) * transform.InverseTransformVector( Rigidbody.velocity ).x * 5f );

        // Extra gravity
        if ( !Waterborne )
		{
            Rigidbody.AddForce( Vector3.up * Time.deltaTime * GravityForce );
		}

        // Try to stay upright
        Rigidbody.AddTorque( Time.deltaTime * transform.TransformDirection( Vector3.forward ) * Body.transform.localEulerAngles.z * RightenForceZ );
        Rigidbody.AddTorque( Time.deltaTime * transform.TransformDirection( Vector3.right ) * Body.transform.localEulerAngles.x * RightenForceX );
    }

	private void OnTriggerEnter( Collider other )
	{
        var flow = other.GetComponentInParent<RiverFlow>();
        if ( flow != null )
        {
            RiverFlow = flow.Direction;
        }
    }

    private void OnTriggerExit( Collider other )
    {
        var flow = other.GetComponentInParent<RiverFlow>();
        if ( flow != null && other.name == "Last" )
        {
            RiverFlow = Vector3.zero;
        }
    }
}
