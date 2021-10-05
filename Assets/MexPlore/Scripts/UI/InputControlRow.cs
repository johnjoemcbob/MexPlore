using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputControlRow : MonoBehaviour
{
    [Header( "Variables" )]
    public string PrettyActionName = "";
    public MexPlore.CONTROL Action;

    [Header( "References" )]
    public Text ActionText;
    public Text ControlText;
    public Image ControlImage;

	private void FixedUpdate()
	{
        Apply();
	}

	public void Apply()
    {
        bool keyboard = MexPlore.Keyboard;

        ActionText.text = PrettyActionName;
        ControlText.text = MexPlore.GetControlText( Action, keyboard );

        bool hasimage = false;
        Sprite icon = MexPlore.GetControlSprite( Action, keyboard );
        if ( icon != null )
		{
            ControlImage.sprite = icon;
            hasimage = true;
		}
        // If has image, no text
        //ControlText.gameObject.SetActive( !hasimage );
        //ControlImage.gameObject.SetActive( hasimage );
    }
}
