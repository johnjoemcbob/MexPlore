using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    public bool RightArm = false;
    public float ExtendLength = 1;
    public float AboveHeadHeight = 6;
    public float EngineHeight = 2;

    private bool HasControl = true;
    private bool Extended = false;
    private Vector3 DefaultHandPos;
    private GameObject HeldObject;
    private int AnimStep = 0;

    void Start()
    {
        DefaultHandPos = GetComponentInChildren<InverseKinematics>().TargetTarget.localPosition;
    }

    void Update()
    {
        var body = GetComponentInParent<MechBody>();
        if ( !body.IsMainController ) return;

        var ik = GetComponentInChildren<InverseKinematics>();
        if ( HasControl )
        {
            // If control is down, extend
            var button = MexPlore.CONTROL.BUTTON_ARM_LEFT;
            if ( RightArm )
            {
                button = MexPlore.CONTROL.BUTTON_ARM_RIGHT;
            }
            Extended = Input.GetButton( MexPlore.GetControl( button ) );

            float extend = Extended ? ExtendLength : 0;
            ik.TargetTarget.localPosition = DefaultHandPos + Vector3.right * extend;
        }
		else
        {
            // Emergency break
            if ( HeldObject == null && AnimStep < 1 )
            {
                HasControl = true;
            }

            // Set target depending on AnimStep
            switch ( AnimStep )
            {
                case 0:
                    // Lerp over head
                    ik.TargetTarget.position = body.transform.position + body.transform.up * AboveHeadHeight;
                    break;
                case 1:
                    // Lerp to engine and destroy
                    ik.TargetTarget.position = body.GetComponentInChildren<Engine>().transform.position + body.transform.up * EngineHeight;
                    break;
                case 2:
                    // Lerp over head
                    ik.TargetTarget.position = body.transform.position + body.transform.up * AboveHeadHeight;
                    break;
                default:
                    break;
            }

            bool reached = ( Vector3.Distance( ik.TargetTarget.position, ik.target.position ) < 0.1f );
            if ( reached )
			{
				// If reached logic depending on AnimStep
				switch ( AnimStep )
				{
                    case 0:

                        break;
                    case 1:
                        Destroy( HeldObject );
                        body.GetComponentInChildren<Engine>().OnFuelConsume();
                        break;
                    case 2:
                        HasControl = true;
                        break;
					default:
						break;
				}
                AnimStep++;
			}
		}
    }

    // Get messages from grabhand
    public void OnPickup( GameObject obj )
	{
        if ( obj.GetComponent<FuelSource>() )
        {
            // If picked up fuelsource, take away control and feed into engine
            HasControl = false;
            HeldObject = obj;
            AnimStep = 0;
        }
	}

    public void OnDrop( GameObject obj )
	{
        HasControl = true;
        HeldObject = null;
    }
}
