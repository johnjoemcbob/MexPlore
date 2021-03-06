using System.Collections;
using NewResolutionDialog.Scripts;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable 0649

/// <summary>
///     <para>
///         Default input handler that provides basic support for the legacy <see cref="Input" /> system.
///     </para>
///     <para>
///         This is used only if the <see cref="ResolutionDialogStyle" /> is set to
///         <see cref="ResolutionDialogStyle.PopupDialog" />.
///     </para>
///     <para>
///         When the <see cref="popupKeyCode" /> is pressed, the popup will be shown/hidden.
///     </para>
/// </summary>
/// <remarks>
///     You can create your own Inputs Handler, notably to add support for the new InputSystem.
///     In that case, remove this script from the Resolution Dialog prefab, and add your new script.
/// </remarks>
/// <seealso cref="PopupHandler" />
public class NewResolutionDialogInputHandler : MonoBehaviour
{
#if ENABLE_LEGACY_INPUT_MANAGER
	[SerializeField]
	private Settings settings;

	[SerializeField]
	private Canvas dialogCanvas;
	public GameObject Dialog;
	public GameObject QuitButton;
	public Slider VolumeSlider;
	public Slider SensitivitySlider;
	public Toggle NetworkToggle;
	public Toggle KeyboardToggle;
	public GameObject InfoPanel;

	[SerializeField]
	private KeyCode popupKeyCode = KeyCode.Escape;

	private bool HasOpenedBefore = false;
	[HideInInspector]
	public bool DontOpen = false;

	private void Awake()
	{
		if ( settings == null ) Debug.LogError( $"Serialized Field {nameof( settings )} is missing!" );
	}

	private void Start()
	{
		if ( settings.dialogStyle == ResolutionDialogStyle.PopupDialog )
			StartCoroutine( WaitForActivation() );
		else
		{
			ToggleCanvas();
			ToggleCanvas();
		}

		InfoPanel.SetActive( false );
	}

	private IEnumerator WaitForActivation()
	{
		while ( true )
		{
			yield return new WaitUntil( () =>
				Input.GetKeyDown( popupKeyCode ) ||
				(
					//(
					//	Application.platform == RuntimePlatform.WindowsPlayer ||
					//	Application.platform == RuntimePlatform.WindowsEditor
					//) &&
					Input.GetButtonUp( MexPlore.GetControl( MexPlore.CONTROL.BUTTON_CURSOR_RELEASE ) )
				)
			);

			if ( !DontOpen && ( LocalPlayer.Instance.CurrentState == LocalPlayer.State.Movement || dialogCanvas.enabled ) )
			{
				if ( !HasOpenedBefore )
				{
					ToggleCanvas();
					ToggleCanvas();
					HasOpenedBefore = true;
				}
				ToggleCanvas();
			}
			DontOpen = false;

			// wait twice (into next frame) to prevent the hotkey from being recognized again in the same frame
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
		}
	}

	private void ToggleCanvas()
	{
		if ( !dialogCanvas.enabled && ( LocalPlayer.Instance == null || LocalPlayer.Instance.CurrentState == LocalPlayer.State.Movement ) )
		{
			dialogCanvas.enabled = true;
			QuitButton.SetActive( Application.platform != RuntimePlatform.WebGLPlayer );
			VolumeSlider.value = MexPlore.GlobalVolume;
			SensitivitySlider.value = MexPlore.MouseSensitivity;
			NetworkToggle.isOn = MexPlore.OnlineMode;
			KeyboardToggle.isOn = MexPlore.Keyboard;
			if ( LocalPlayer.Instance != null )
			{
				LocalPlayer.Instance.SwitchState( LocalPlayer.State.UI );
			}
		}
		else
		{
			dialogCanvas.enabled = false;
			if ( LocalPlayer.Instance != null )
			{
				LocalPlayer.Instance.SwitchState( LocalPlayer.State.Movement );
			}
		}
		Dialog.SetActive( dialogCanvas.enabled );
	}

	public void ButtonInfo()
	{
		InfoPanel.SetActive( !InfoPanel.activeSelf );
	}

	public void ButtonClose()
	{
		dialogCanvas.enabled = false;
		LocalPlayer.Instance.SwitchState( LocalPlayer.State.Movement );

		Dialog.SetActive( dialogCanvas.enabled );
	}

	public void UpdateVolumeSlider( float value )
	{
		AudioListener.volume = value;
		MexPlore.GlobalVolume = value;
	}

	public void UpdateSensitivitySlider( float value )
	{
		MexPlore.MouseSensitivity = value;
	}

	public void UpdateNetworkToggle( bool toggle )
	{
		MexPlore.OnlineMode = toggle;
	}

	public void UpdateKeyboardToggle( bool toggle )
	{
		MexPlore.Keyboard = toggle;

		foreach ( var input in FindObjectsOfType<InputControlRow>( true ) )
		{
			input.Apply();
		}
	}

#elif ENABLE_INPUT_SYSTEM
private void Awake()
{
    Debug.LogError(
        "The new InputSystem is not supported out of the box. " +
        "If you want to use the popup mode, you must create your own InputsHandler and remove this one from the prefab. " +
        $"Otherwise, just remove this {nameof(DefaultInputsHandler)} component for the prefab.");
}
#endif
}