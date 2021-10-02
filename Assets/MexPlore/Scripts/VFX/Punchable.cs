using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Punchable : MonoBehaviour
{
	public Vector3 PunchScale;
	protected Vector3 TargetScale;

	virtual public void Start()
	{
		TargetScale = transform.localScale;
	}

    virtual public void Update()
	{
		transform.localScale = Vector3.Lerp( transform.localScale, TargetScale, Time.deltaTime * 5 );
	}

	public void Punch()
	{
		transform.localScale = PunchScale;
	}
}
