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
using System.IO;

namespace EVAOK
{
	[KSPAddon (KSPAddon.Startup.Flight, false)]
	public class EVAOK : MonoBehaviour
	{
		//============================================================================================================================================
		//Define Variables
		//============================================================================================================================================
		Vessel OurVessel = new Vessel ();
		private bool SafeToEVA = false;
		private bool EVASafetyOverride = false;

		//============================================================================================================================================
		//Start Running Processes
		//============================================================================================================================================

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
			if (Input.GetKey (KeyCode.RightControl)) {
				EVASafetyOverride = true;
			}
			if (Input.GetKeyUp (KeyCode.RightControl)) {
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
					//START-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
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
						//END-AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
					} else {
						//START-VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
						if (OurVessel.atmDensity == 0) {
							//...and our throttle is off...
							if (FlightInputHandler.state.mainThrottle == 0) {
								SafeToEVA = true;
							} else {
								SafeToEVA = false;
							}
						}
						//END-VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV
					}
				}
			}
			if (SafeToEVA == true) {
				HighLogic.CurrentGame.Parameters.Flight.CanEVA = true;
			} else {
				HighLogic.CurrentGame.Parameters.Flight.CanEVA = false;
			}
		}
	}
}

