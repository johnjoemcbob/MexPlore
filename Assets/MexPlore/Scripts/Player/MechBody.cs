using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechBody : MonoBehaviour
{
	private void OnCollisionEnter( Collision collision )
	{
		foreach ( var controller in GetComponentsInChildren<BaseController>() )
		{
			controller.OnCollisionEnter( collision );
		}
	}
}
