using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseController : MonoBehaviour
{
	[Header( "Base - Variables" )]
	public bool IsMainController = false;

	public virtual void OnCollisionEnter( Collision collision )
	{

	}
}
