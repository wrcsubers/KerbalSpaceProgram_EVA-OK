//=====================================================================================
// The MIT License (MIT)
// 
// EVA OK! - Copyright (c) 2015 Cameron Woods/WRCsubeRS
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
//Version 1.1 - Released 12.12.15
//Version 1.0 - Initial Release 12.03.15
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace EVAOK
{
	[KSPAddon (KSPAddon.Startup.SpaceCentre, false)]
	public class EVAOKMonoSpaceCentre : MonoBehaviour
	{
		//============================================================================================================================================
		//Define Variables
		//============================================================================================================================================

		//Settings Stuff
		private ConfigNode EVAOK_MSC_SystemSettings;
		//Default Override Key if there is a problem with external file - default is: RightControl
		public static KeyCode EVAOK_MSC_OverrideKeyCode = KeyCode.RightControl;
		private string EVAOK_MSC_OverrideKeyCodeString;

		//GUI Stuff
		private ApplicationLauncherButton EVAOK_MSC_ToolbarButton = null;
		private Texture2D EVAOK_MSC_ToolbarButtonTexture = new Texture2D (38, 38, TextureFormat.ARGB32, false);
		private bool EVAOK_MSC_ShowSettingsGUI = false;
		private static Rect EVAOK_MSC_SettingsGUIWindow = new Rect (Screen.width - 172, 50, 150, 110);
		private string EVAOK_MSC_KeyMapButtonText = "<b><color=white>Set New Key</color></b>";
		private bool EVAOK_MSC_DetectNewKeyMap = false;

		//Timer for Blinking Text
		private static Timer EVAOK_MSC_Timer100 = new Timer (100);
		private float EVAOK_MSC_TimerTime = 0.0f;
		private bool EVAOK_MSC_BlinkText = false;

		//============================================================================================================================================
		//Start Running Processes
		//============================================================================================================================================

		//This function gets called only once, during the KSP loading screen.
		//============================================================================================================================================
		private void Awake ()
		{
			//Set Toolbar Textures and tell us when it's alive
			if (GameDatabase.Instance.ExistsTexture ("EVA_OK/Textures/ToolbarButtonDefault")) {
				EVAOK_MSC_ToolbarButtonTexture = GameDatabase.Instance.GetTexture ("EVA_OK/Textures/ToolbarButtonDefault", false);
			}

			//Add Hook to GameEvents & Run AppLaucnher Method
			GameEvents.onGUIApplicationLauncherReady.Add (EVAOK_MSC_OnGUIApplicationLauncherReady);
			EVAOK_MSC_OnGUIApplicationLauncherReady ();

			//Settings Information for Setting/Retrieving values from external file
			EVAOK_MSC_SystemSettings = new ConfigNode ();
			EVAOK_MSC_SystemSettings = ConfigNode.Load ("GameData/EVA_OK/Config/EVAOK_PluginSettings.cfg");

			//If settings exist load those, otherwise create new settings in SaveSettings method
			if (EVAOK_MSC_SystemSettings != null) {
				print ("EVA-OK! - Settings exist! Loading Values...");
				//Read Keycode from text file and convert it to a KeyCode
				EVAOK_MSC_OverrideKeyCodeString = System.IO.File.ReadAllText ("GameData/EVA_OK/Config/EVAOK_PluginSettings.cfg");
				EVAOK_MSC_OverrideKeyCode = (KeyCode)System.Enum.Parse (typeof(KeyCode), EVAOK_MSC_OverrideKeyCodeString);
			} else {
				print ("EVA-OK! - Settings don't exist! Creating new file with built in defaults...");
				EVAOK_MSC_SaveSettings ();
			}
			//Setup Timer for Text Blinking
			EVAOK_MSC_Timer100.Elapsed += new ElapsedEventHandler (EVAOK_MSC_OnTimedEvent1);
		}

		//This is all GUI Stuff...
		//============================================================================================================================================
		//Create Toolbar Button if one doesn't already exist
		private void EVAOK_MSC_OnGUIApplicationLauncherReady ()
		{
			if (EVAOK_MSC_ToolbarButton == null) {
				EVAOK_MSC_ToolbarButton = ApplicationLauncher.Instance.AddModApplication (
					EVAOK_MSC_GUISwitch, EVAOK_MSC_GUISwitch,
					null, null,
					null, null,
					ApplicationLauncher.AppScenes.SPACECENTER,
					EVAOK_MSC_ToolbarButtonTexture
				);
			}
		}

		//This is the Switch to turn show/hide the main GUI
		//============================================================================================================================================
		public void EVAOK_MSC_GUISwitch ()
		{
			//When Toolbar Button is Pressed...
			//Show The Settings Window
			if (EVAOK_MSC_ShowSettingsGUI == false) {
				RenderingManager.AddToPostDrawQueue (0, EVAOK_MSC_OnDraw);
				EVAOK_MSC_ShowSettingsGUI = true;
			} else {
				//Hide The Settings Window and Save Settings
				RenderingManager.RemoveFromPostDrawQueue (0, EVAOK_MSC_OnDraw);
				EVAOK_MSC_DetectNewKeyMap = false;
				EVAOK_MSC_KeyMapButtonText = "<b><color=white>Set New Key</color></b>";
				//Save Settings Automatically when window is closed
				EVAOK_MSC_SaveSettings ();
				EVAOK_MSC_ShowSettingsGUI = false;
			}
		}

		//EVAOK_MSC_OnDraw Method
		//============================================================================================================================================
		private void EVAOK_MSC_OnDraw ()
		{
			GUI.skin.window.richText = true;
			if (EVAOK_MSC_ShowSettingsGUI == true) {
				EVAOK_MSC_SettingsGUIWindow = GUI.Window (151844, EVAOK_MSC_SettingsGUIWindow, EVAOK_MSC_GUI, "<b>EVA OK!</b>");
			} 
		}

		//GUI Windows Setup
		//============================================================================================================================================
		private void EVAOK_MSC_GUI (int WindowID)
		{
			//Current KeyMap display
			GUI.skin.label.fontStyle = FontStyle.Bold;
			GUI.skin.label.alignment = TextAnchor.MiddleCenter;
			GUI.skin.label.fontSize = 13;
			GUI.Label (new Rect (3, 13, 144, 25), "Override Key: ");
			GUI.skin.box.fontSize = 14;
			GUI.Box (new Rect (20, 34, 110, 25), EVAOK_MSC_OverrideKeyCode.ToString ());
			GUI.skin.button.fontStyle = FontStyle.Bold;
			//KeyMap Button
			GUI.skin.button.fontSize = 13;
			GUIContent ButtonMapSwitch = new GUIContent (EVAOK_MSC_KeyMapButtonText);
			if (GUI.Button (new Rect (15, 63, 120, 22), ButtonMapSwitch) == true) {
				EVAOK_MSC_DetectNewKeyMap = true;
			}
			//Okay Button
			GUI.skin.button.fontSize = 10;
			GUIContent EVAOK_MSC_OKButton = new GUIContent ("<b><color=#bfbfbf>Okay</color></b>");
			if (GUI.Button (new Rect (45, 88, 60, 18), EVAOK_MSC_OKButton) == true) {
				//Close Window and Save Settings when 'Okay' is pressed
				EVAOK_MSC_GUISwitch ();
			}
			GUI.DragWindow (new Rect (0, 0, 150, 17));
		}

		//OnGui Method - Key Detection must go here!
		//============================================================================================================================================
		void OnGUI ()
		{
			if (EVAOK_MSC_DetectNewKeyMap == true) {
				//Start Timer for Text Blinking
				EVAOK_MSC_Timer100.Enabled = true;
				if (EVAOK_MSC_BlinkText == true) {
					EVAOK_MSC_KeyMapButtonText = "<b><color=white>Press Any Key...</color></b>";
				} else {
					EVAOK_MSC_KeyMapButtonText = "<b><color=grey>Press Any Key...</color></b>";
				}
				//Detect Key Press
				Event EVAOK_MSC_NewKeyCode = Event.current;
				if (EVAOK_MSC_NewKeyCode.isKey) {
					EVAOK_MSC_OverrideKeyCode = EVAOK_MSC_NewKeyCode.keyCode;
					//Once Key Code is detected exit this detection sequence
					EVAOK_MSC_DetectNewKeyMap = false;
					EVAOK_MSC_KeyMapButtonText = "<b><color=white>Set New Key</color></b>";
					EVAOK_MSC_Timer100.Stop ();
				}
			}
		}

		//Timer Events for Module, these are for various things...
		//============================================================================================================================================
		private void EVAOK_MSC_OnTimedEvent1 (object source, ElapsedEventArgs e)
		{
			//EVAOK_MSC_TimerTime for Blink Text - This runs continuously
			EVAOK_MSC_TimerTime += 0.1f;
			//Blink Text every 0.4 seconds
			if (EVAOK_MSC_TimerTime == 0.4f) {
				EVAOK_MSC_TimerTime = 0.0f;
				EVAOK_MSC_BlinkText = !EVAOK_MSC_BlinkText;
			}
		}

		//Save Settings to external File
		//============================================================================================================================================
		private void EVAOK_MSC_SaveSettings ()
		{
			System.IO.File.WriteAllText ("GameData/EVA_OK/Config/EVAOK_PluginSettings.cfg", EVAOK_MSC_OverrideKeyCode.ToString ());
		}

		//OnDisable Method - This is called when the toolbar button is destroyed, make sure we save settings...
		//============================================================================================================================================
		private void OnDisable ()
		{
			EVAOK_MSC_SaveSettings ();
			ApplicationLauncher.Instance.RemoveModApplication (EVAOK_MSC_ToolbarButton);
			GameEvents.onGUIApplicationLauncherReady.Remove (EVAOK_MSC_OnGUIApplicationLauncherReady);
			EVAOK_MSC_Timer100.Dispose ();
		}
	}
}

