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
using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace EVAOK
{
	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class EVAOKMonoFlight : MonoBehaviour
	{
		//============================================================================================================================================
		//Define Variables
		//============================================================================================================================================

		//Settings Stuff
		private ConfigNode EVAOK_MFL_SystemSettings;
		public static KeyCode EVAOK_MFL_OverrideKeyCode = KeyCode.RightControl;
		private string EVAOK_MFL_OverrideKeyCodeString;

		//Logic Stuff
		Vessel EVAOK_MFL_OurVessel = new Vessel ();
		private bool EVAOK_MFL_SafeToEVA = false;
		private bool EVAOK_MFL_EVASafetyOverride = false;

		//============================================================================================================================================
		//Start Running Processes
		//============================================================================================================================================

		//Start - Called when the flight starts or in the editor. Start will be called before OnUpdate or OnFixedUpdate are ever called.
		//============================================================================================================================================
		private void Start ()
		{
			EVAOK_MFL_OurVessel = FlightGlobals.ActiveVessel;
			//Settings Information for Setting/Retrieving values from external file, we could reference the value from MonoSpaceCentre but we'll use the external file incase the Space Center is somehow bypassed during loading.
			EVAOK_MFL_SystemSettings = new ConfigNode ();
			EVAOK_MFL_SystemSettings = ConfigNode.Load ("GameData/EVA_OK/Config/EVAOK_PluginSettings.cfg");
			//If settings exist load those, otherwise use default of 'RightControl' button
			if (EVAOK_MFL_SystemSettings != null) {
				print ("EVA-OK! - Settings exist! Loading Values...");
				//Read Keycode from text file and convert it to a KeyCode
				EVAOK_MFL_OverrideKeyCodeString = System.IO.File.ReadAllText ("GameData/EVA_OK/Config/EVAOK_PluginSettings.cfg");
				EVAOK_MFL_OverrideKeyCode = (KeyCode)System.Enum.Parse (typeof(KeyCode), EVAOK_MFL_OverrideKeyCodeString);
			}
		}

		//FixedUpdate - This method runs every physics frame
		//============================================================================================================================================
		private void FixedUpdate ()
		{
			//Press and hold Key to bypass safety measures so you can EVA
			if (Input.GetKey (EVAOK_MFL_OverrideKeyCode)) {
				EVAOK_MFL_EVASafetyOverride = true;
			}
			if (Input.GetKeyUp (EVAOK_MFL_OverrideKeyCode)) {
				EVAOK_MFL_EVASafetyOverride = false;
			}

			//Make sure we are the current vessel
			if (EVAOK_MFL_OurVessel != FlightGlobals.ActiveVessel) {
				EVAOK_MFL_OurVessel = FlightGlobals.ActiveVessel;
			}

			//Only run if our craft can actually hold crew
			if (EVAOK_MFL_OurVessel.GetCrewCapacity () > 0) {
				if (EVAOK_MFL_EVASafetyOverride == true) {
					EVAOK_MFL_SafeToEVA = true;
				} else {
					//START ATMOSPHERE---------------------------------------------------------------------
					//If we are on a planet with an atmosphere...
					if (EVAOK_MFL_OurVessel.atmDensity > 0) {
						//...and we are landing on water/land...
						if (EVAOK_MFL_OurVessel.LandedOrSplashed == true) {
							//...and we are travelling less than 10m/s...
							if (EVAOK_MFL_OurVessel.srfSpeed < 10) {
								//...it is safe to EVA!
								EVAOK_MFL_SafeToEVA = true;
								//Otherwise...NOT SAFE TO EVA!
							} else {
								EVAOK_MFL_SafeToEVA = false;
							}
						} else {
							EVAOK_MFL_SafeToEVA = false;
						}
						//END ATMOSPHERE---------------------------------------------------------------------
					} else {
						//START VACUUM---------------------------------------------------------------------
						//If we are in a vacuum
						if (EVAOK_MFL_OurVessel.atmDensity == 0) {
							//...and our throttle is off...
							if (FlightInputHandler.state.mainThrottle == 0) {
								//...it is safe to EVA
								EVAOK_MFL_SafeToEVA = true;
							} else {
								//Otherwise...NOT SAFE TO EVA!
								EVAOK_MFL_SafeToEVA = false;
							}
						}
						//END VACUUM---------------------------------------------------------------------
					}
				}
			}
			//Set wether EVA is possible based on above parameters...
			if (EVAOK_MFL_SafeToEVA == true) {
				HighLogic.CurrentGame.Parameters.Flight.CanEVA = true;
			} else {
				HighLogic.CurrentGame.Parameters.Flight.CanEVA = false;
			}
		}
	}
}

