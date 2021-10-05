using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLinePlotter : MonoBehaviour
{
    public Transform StartPos;
    public Transform FinishPos;

	private void Start()
	{
        StartPos.localPosition = FinishPos.localPosition;
    }

	void Update()
    {
        GetComponent<LineRenderer>().SetPosition( 0, StartPos.position );
        GetComponent<LineRenderer>().SetPosition( 1, FinishPos.position );
    }
}
