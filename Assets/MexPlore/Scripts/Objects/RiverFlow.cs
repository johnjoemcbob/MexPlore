using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverFlow : MonoBehaviour
{
	public Vector3 Direction;

	private void OnDrawGizmos()
	{
		if ( Direction != Vector3.zero )
		{
			Gizmos.color = Color.red;
			Gizmos.DrawRay( transform.position, Direction );
		}
	}
}
