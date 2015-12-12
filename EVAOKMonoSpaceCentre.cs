//=====================================================================================
// The MIT License (MIT)
// 
// EVA OK! - Copyright (c) 2015 WRCsubeRS
// 
// EVA OK! - A Mod for Kerbal Space Program by Squad
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
// 
//=====================================================================================
//Version 1.0 - Initial Release 12.03.15
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace EVAOK
{
	[KSPAddon (KSPAddon.Startup.SpaceCentre, false)]
	public class EVAOK : MonoBehaviour
	{
		//============================================================================================================================================
		//Define Variables
		//============================================================================================================================================
		//Settings Stuff
		private ConfigNode EVAOK_SystemSettings;
		private KeyCode EVAOK_OverrideKeyCode = KeyCode.RightControl;
		private string EVAOK_OverrideKeyCodeString;

		//GUI Stuff
		private bool ShowEVAOKSettings = false;
		private static Rect EVAOK_SettingsGUI = new Rect (Screen.width / 2, Screen.height / 2, 150, 90);
		//set window position
		private string KeyMapButtonText = "Set New Key";
		private ApplicationLauncherButton EVAOK_ToolbarButton = null;
		private bool DetectNewKeyMap = false;
		private Texture2D EVAOK_Button = new Texture2D (38, 38, TextureFormat.ARGB32, false);

		//Logic Stuff
		Vessel OurVessel = new Vessel ();
		private bool SafeToEVA = false;
		private bool EVASafetyOverride = false;



		//============================================================================================================================================
		//Start Running Processes
		//============================================================================================================================================
		//This function gets called only once, during the KSP loading screen.
		private void Awake ()
		{
			//Set Toolbar Textures and tell us when it's alive
			if (GameDatabase.Instance.ExistsTexture ("EVAOK!/Textures/ToolbarButtonDefault")) {
				EVAOK_Button = GameDatabase.Instance.GetTexture ("EVAOK!/Textures/ToolbarButtonDefault", false);
			}

			//Add Hook to GameEvents & Run AppLaucnher Method
			GameEvents.onGUIApplicationLauncherReady.Add (OnGUIApplicationLauncherReady);
			OnGUIApplicationLauncherReady ();

			//Settings Information for Setting/Retrieving values from external file
			EVAOK_SystemSettings = new ConfigNode ();
			EVAOK_SystemSettings = ConfigNode.Load ("GameData/EVAOK!/Config/EVAOK_PluginSettings.cfg");

			//If settings exist load those, otherwise create new settings in SaveSettings method
			if (EVAOK_SystemSettings != null) {
				print ("EVA-OK! - Settings exist! Loading Values...");
				//Read Keycode from text file and convert it to a KeyCode
				EVAOK_OverrideKeyCodeString = System.IO.File.ReadAllText ("GameData/EVAOK!/Config/EVAOK_PluginSettings.cfg");
				EVAOK_OverrideKeyCode = (KeyCode)System.Enum.Parse (typeof(KeyCode), EVAOK_OverrideKeyCodeString);
			} else {
				print ("EVA-OK! - Settings don't exist! Creating new file with built in defaults...");
				EVAOK_SaveSettings ();
			}
		}

		//This is all GUI Stuff...
		//============================================================================================================================================

		//Create Toolbar Button if one doesn't already exist
		private void OnGUIApplicationLauncherReady ()
		{
			if (EVAOK_ToolbarButton == null) {
				EVAOK_ToolbarButton = ApplicationLauncher.Instance.AddModApplication (
					EVAOK_GUISwitch, EVAOK_GUISwitch,
					null, null,
					null, null,
					ApplicationLauncher.AppScenes.SPACECENTER,
					EVAOK_Button
				);
			}
		}

		//This is the Switch to turn show/hide the main GUI
		//============================================================================================================================================
		public void EVAOK_GUISwitch ()
		{
			//When Toolbar Button is Pressed...
			//Show The Settings Window
			if (ShowEVAOKSettings == false) {
				RenderingManager.AddToPostDrawQueue (0, OnDraw);
				ShowEVAOKSettings = true;
			} else {
				//Hide The Settings Window and Save Settings
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
				//Save Settings Automatically when window is closed
				EVAOK_SaveSettings ();
				ShowEVAOKSettings = false;
			}
			print (EVAOK_ToolbarButton.container.transform.localPosition.x);
		}

		//OnDraw Method
		//============================================================================================================================================
		private void OnDraw ()
		{
			GUI.skin.window.richText = true;
			if (ShowEVAOKSettings == true) {
				EVAOK_SettingsGUI = GUI.Window (151844, EVAOK_SettingsGUI, EVAOK_GUI, "<b>EVA OK!</b>");
			} 
		}

		//GUI Windows Setup
		//============================================================================================================================================
		private void EVAOK_GUI (int WindowID)
		{
			//Current KeyMap display
			GUI.skin.label.fontStyle = FontStyle.Bold;
			GUI.skin.label.fontSize = 14;
			GUI.Label (new Rect (3, 18, 144, 25), "Override Key: " + EVAOK_OverrideKeyCode.ToString ());
			GUI.skin.button.fontStyle = FontStyle.Bold;
			//KeyMap Button
			GUI.skin.button.fontSize = 13;
			GUIContent ButtonMapSwitch = new GUIContent ("<b><color=white>" + KeyMapButtonText + "</color></b>");
			if (GUI.Button (new Rect (15, 45, 120, 22), ButtonMapSwitch) == true) {
				DetectNewKeyMap = true;
			}
			//Okay Button
			GUI.skin.button.fontSize = 11;
			GUIContent EVAOK_OKButton = new GUIContent ("<b><color=white>Okay</color></b>");
			if (GUI.Button (new Rect (45, 70, 60, 16), EVAOK_OKButton) == true) {
				//Close Window and Save Settings when 'Okay' is pressed
				EVAOK_GUISwitch ();
			}
			GUI.DragWindow (new Rect (0, 0, 150, 17));
		}

		void OnGUI ()
		{
			if (DetectNewKeyMap == true) {
				KeyMapButtonText = "Press Any Key...";
				Event EVAOK_NewKeyCode = Event.current;
				if (EVAOK_NewKeyCode.isKey) {
					EVAOK_OverrideKeyCode = EVAOK_NewKeyCode.keyCode;
					Debug.Log ("EVA-OK! - Detected key code: " + EVAOK_NewKeyCode.keyCode);
					DetectNewKeyMap = false;
					KeyMapButtonText = "Set New Key";
				}
			}
		}

		//Save Settings to external File
		//============================================================================================================================================
		private void EVAOK_SaveSettings ()
		{
			System.IO.File.WriteAllText ("GameData/EVAOK!/Config/EVAOK_PluginSettings.cfg", EVAOK_OverrideKeyCode.ToString ());
		}

		private void OnDisable ()
		{
			ApplicationLauncher.Instance.RemoveModApplication (EVAOK_ToolbarButton);
			GameEvents.onGUIApplicationLauncherReady.Remove (OnGUIApplicationLauncherReady);
		}

		//Called when the flight starts or in the editor. OnStart will be called before OnUpdate or OnFixedUpdate are ever called.
		//============================================================================================================================================
		private void Start ()
		{
			OurVessel = FlightGlobals.ActiveVessel;
		}

		//This method runs every physics frame
		//============================================================================================================================================
		private void FixedUpdate ()
		{
			//Press and hold Key to bypass safety measures so you can EVA
			if (Input.GetKey (EVAOK_OverrideKeyCode)) {
				EVASafetyOverride = true;
			}
			if (Input.GetKeyUp (EVAOK_OverrideKeyCode)) {
				EVASafetyOverride = false;
			}

			//Make sure we are the current vessel
			if (OurVessel != FlightGlobals.ActiveVessel) {
				OurVessel = FlightGlobals.ActiveVessel;
			}

			//Only run if our craft can actually hold crew
			if (OurVessel.GetCrewCapacity () > 0) {
				if (EVASafetyOverride == true) {
					SafeToEVA = true;
				} else {
					//START ATMOSPHERE---------------------------------------------------------------------
					//If we are on a planet with an atmosphere...
					if (OurVessel.atmDensity > 0) {
						//...and we are landing on water/land...
						if (OurVessel.LandedOrSplashed == true) {
							//...and we are travelling less than 10m/s...
							if (OurVessel.srfSpeed < 10) {
								//...it is safe to EVA!
								SafeToEVA = true;
								//Otherwise...NOT SAFE TO EVA!
							} else {
								SafeToEVA = false;
							}
						} else {
							SafeToEVA = false;
						}
						//END ATMOSPHERE---------------------------------------------------------------------
					} else {
						//START VACUUM---------------------------------------------------------------------
						//If we are in a vacuum
						if (OurVessel.atmDensity == 0) {
							//...and our throttle is off...
							if (FlightInputHandler.state.mainThrottle == 0) {
								//...it is safe to EVA
								SafeToEVA = true;
							} else {
								//Otherwise...NOT SAFE TO EVA!
								SafeToEVA = false;
							}
						}
						//END VACUUM---------------------------------------------------------------------
					}
				}
			}
			//Set wether EVA is possible based on above parameters...
			if (SafeToEVA == true) {
				HighLogic.CurrentGame.Parameters.Flight.CanEVA = true;
			} else {
				HighLogic.CurrentGame.Parameters.Flight.CanEVA = false;
			}
		}
	}
}

