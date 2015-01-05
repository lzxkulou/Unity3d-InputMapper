//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.17929
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Runtime.InteropServices;
using UnityEngine;
using System.Collections.Generic;
using ws.winx.utils;

#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
using System;
using ws.winx.devices;
//using UnityEngine;


namespace ws.winx.platform.osx
{



	using CFString = System.IntPtr;

	using CFTypeRef = System.IntPtr;
	using IOHIDDeviceRef = System.IntPtr;
	using IOHIDElementRef = System.IntPtr;

	using IOHIDValueRef = System.IntPtr;
	using IOOptionBits = System.IntPtr;
	using IOReturn =Native.IOReturn;// System.IntPtr;

	using IOHIDElementType=Native.IOHIDElementType;





	sealed class OSXDriver:IDriver
		{

#region Fields

		public float dreadZone=0.3f;

		IHIDInterface _hidInterface;

		
	
		
        #endregion




#region Constructor
        public OSXDriver()
				{
				
					

				}
        #endregion


#region Private Members




     /// <summary>
     /// Devices the value received.
     /// </summary>
     /// <param name="device">Device.</param>
     /// <param name="type">Type.</param>
     /// <param name="uid">Uid.</param>
     /// <param name="value">Value.</param>
		internal void DeviceValueReceived(IDevice device,Native.IOHIDElementType type,uint uid,int value)
		{
	//		UnityEngine.Debug.Log ("OSXDriver>>DeviceValueReceived type="+type+" uid:"+uid+" value:"+value);
		

			//AXIS
			if(type==Native.IOHIDElementType.kIOHIDElementTypeInput_Misc
			   || type==Native.IOHIDElementType.kIOHIDElementTypeInput_Axis)
			{
				int numAxes=device.Axis.Count;
				AxisDetails axisDetails;

				//numAxes
				for (int axisIndex = 0; axisIndex < numAxes; axisIndex++) {

					axisDetails=device.Axis[axisIndex] as AxisDetails;

					//UnityEngine.Debug.Log ("OSXDriver>>DeviceValueReceived axisDetails="+axisDetails.uid);

					if (axisDetails!=null) 

					   if(axisDetails.uid== uid) {

						//UnityEngine.Debug.Log ("OSXDriver>>DeviceValueReceived Axis["+axisIndex+"] axisDetails.uid="+axisDetails.uid);
						
						//check hatch
						//Check if POV element.
						if(axisDetails.isHat)
						{

							//UnityEngine.Debug.Log ("OSXDriver>>DeviceValueReceived axisDetails.uid="+axisDetails.isHat);

									//Workaround for POV hat switches that do not have null states.
									if(!axisDetails.isNullable)
									{
										value = value < axisDetails.min ? axisDetails.max - axisDetails.min + 1 : value - 1;
									}


							float outX=0f;
							float outY=0f;

							if(value<15)
							hatValueToXY(value,(axisDetails.max - axisDetails.min)+1,out outX,out outY);

							device.Axis[JoystickAxis.AxisPovX].value=outX;
							device.Axis[JoystickAxis.AxisPovY].value=outY;

							axisIndex++;//cos 2 axis are handled

//							UnityEngine.Debug.Log("POVX:"+device.Axis[JoystickAxis.AxisPovX].value+" POVY:"+device.Axis[JoystickAxis.AxisPovY].value);	
						
						}else{
//							if(axisIndex==2){
//								UnityEngine.Debug.Log("ORG Axis"+axisIndex+" Counts:"+ (int)value+" min:"+axisDetails.min+" max:"+axisDetails.max);
//
//							}


							//Sanity check.
							if(value < axisDetails.min)
							{
								value = axisDetails.min;
							}
							if(value > axisDetails.max)
							{
								value = axisDetails.max;
							}

							//Calculate the -1 to 1 float from the min and max possible values.
							float analogValue=0f;

							//if trigger
							if(axisDetails.isTrigger)
								analogValue=(float)value/axisDetails.max;
							else
								analogValue=(value - axisDetails.min) / (float)(axisDetails.max - axisDetails.min) * 2.0f - 1.0f;





							axisDetails.value=analogValue;




						}




					}else{//cos update of values happen only on "interrupt" and its returning only one element value
						//the rest should take previous value which would update ButtonState
						//!!! side effect inputput might left to "hold" forever

						axisDetails.value=axisDetails.value;
					}

//					if(axisIndex==1){
//						//UnityEngine.Debug.Log("Axis"+axisIndex+" Counts:"+ value+" min:"+axisDetails.min+" max:"+axisDetails.max);
//						//UnityEngine.Debug.Log("Axis"+axisIndex+" Value:"+axisDetails.value);
//		}
					
				}//end for

			//BUTTONS
			}else if(type==Native.IOHIDElementType.kIOHIDElementTypeInput_Button){

				int numButtons=device.Buttons.Count;
				IButtonDetails buttonDetails;

				for (int buttonIndex = 0; buttonIndex < numButtons; buttonIndex++) {

					buttonDetails=device.Buttons[buttonIndex];

					if ( buttonDetails.uid== uid) {

						buttonDetails.value=value;

				//		UnityEngine.Debug.Log("Button "+buttonIndex+" value:"+value+" State:"+device.Buttons[buttonIndex].buttonState);
						
					
					}else{//cos update of values happen only on "interrupt" and its returning only one element value
						//the rest should take previous value which would update ButtonState

						buttonDetails.value=buttonDetails.value;
					}
				}
			}

		}

		 

//					   0                 
//					   |
//				3______|______1
//					   |
//					   |
//					   2



		//				  7    0     1            
		//				   \   |   /
		//				6 _____|______2
		// 					  /|\
		//					/  |  \
		//				   5   4    3


		/// <summary>
		/// Hats the value to X.
		/// </summary>
		/// <param name="value">Value.</param>
		/// <param name="range">Range.</param>
		/// <param name="outX">Out x.</param>
		/// <param name="outY">Out y.</param>
		 void hatValueToXY(long value, int range,out float outX,out float outY) {

				outX = outY = 0f;
				int rangeHalf=range>>1;
				int rangeQuat=range>>2;
				
				if (value > 0 && value < rangeHalf) {
					outX = 1f;
					
				} else if (value > rangeHalf) {
					outX = -1f;					
				} 
				
				if (value > rangeQuat * 3 || value < rangeQuat) {
					outY = 1f;
					
				} else if (value > rangeQuat && value < rangeQuat * 3) {
					outY = -1f;					
				} 

		}




        #endregion








#region IJoystickDriver implementation
		/// <summary>
		/// Update the specified device.
		/// </summary>
		/// <param name="device">Joystick.</param>
        public void Update(IDevice device)	
		{
			if (device != null && _hidInterface != null && _hidInterface.Contains(device.ID)) {
								
				HIDReport report = _hidInterface.ReadDefault (device.ID);

								//Debug.Log (report.Status);

								if (report.Status == HIDReport.ReadStatus.Success || report.Status == HIDReport.ReadStatus.Buffered) {

										//Debug.Log ("update value");
										DeviceValueReceived (device, (Native.IOHIDElementType)BitConverter.ToUInt32 (report.Data, 0), BitConverter.ToUInt32 (report.Data, 4), BitConverter.ToInt32 (report.Data, 8));
								}

			}


		}

		/// <summary>
		/// Resolves the device.
		/// </summary>
		/// <returns>returns JoystickDevice if driver is for this device or null</returns>
		/// <param name="info">Info.</param>
		/// <param name="hidDevice">Hid device.</param>
		public IDevice ResolveDevice (IHIDDevice hidDevice)
		{
	
			this._hidInterface = hidDevice.hidInterface;

			IntPtr deviceRef=hidDevice.deviceHandle;

			JoystickDevice device;
			int axisIndex=0;
			int buttonIndex=0;

			Native.CFArray elements=new Native.CFArray();
			IOHIDElementRef element;
			IOHIDElementType type;

			//copy all matched 
			elements.typeRef=Native.IOHIDDeviceCopyMatchingElements(deviceRef, IntPtr.Zero,(int)Native.IOHIDOptionsType.kIOHIDOptionsTypeNone );
			
			int numButtons=0;
		
			int numPov=0;

			int numElements=elements.Length;
			int HIDElementType=Native.IOHIDElementGetTypeID();

			//check for profile
			DeviceProfile profile = null;
			
			if (hidDevice.hidInterface.Profiles.ContainsKey (hidDevice.Name)) {
				
				
				
				profile = hidDevice.hidInterface.LoadProfile (hidDevice.hidInterface.Profiles [hidDevice.Name]);
			}
			


			IAxisDetails axisDetailsPovX=null;
			IAxisDetails axisDetailsPovY=null;

			List<IAxisDetails> axisDetailsList = new List <IAxisDetails>();
		

			IAxisDetails axisDetails;
			
							for (int elementIndex = 0; elementIndex < numElements; elementIndex++){

									element =  elements[elementIndex].typeRef;


								if(element!=IntPtr.Zero && Native.CFGetTypeID(element) == HIDElementType){
												
												type = Native.IOHIDElementGetType(element);
							
							
												// All of the axis elements I've ever detected have been kIOHIDElementTypeInput_Misc. kIOHIDElementTypeInput_Axis is only included for good faith...
												if (type == IOHIDElementType.kIOHIDElementTypeInput_Misc ||
												    type == IOHIDElementType.kIOHIDElementTypeInput_Axis) {
												


													axisDetails=new AxisDetails();
													
													axisDetails.uid=Native.IOHIDElementGetCookie(element);
													axisDetails.min=(int)Native.IOHIDElementGetLogicalMin(element);
													axisDetails.max=(int)Native.IOHIDElementGetLogicalMax(element);
													axisDetails.isNullable=Native.IOHIDElementHasNullState(element);

													

					

										            
													if(Native.IOHIDElementGetUsage(element)==(uint)Native.HIDUsageGD.Hatswitch){
												
														axisDetails.isHat=true;
														axisDetailsPovX=axisDetails;
		
														axisDetailsPovY=new AxisDetails();
														
														axisDetailsPovY.uid=Native.IOHIDElementGetCookie(element);
														axisDetailsPovY.min=(int)Native.IOHIDElementGetLogicalMin(element);
														axisDetailsPovY.max=(int)Native.IOHIDElementGetLogicalMax(element);
														axisDetailsPovY.isNullable=Native.IOHIDElementHasNullState(element);
														axisDetailsPovY.isHat=true;

							numPov++;


													}else{
							if(axisDetails.min==0) axisDetails.isTrigger=true;
							axisDetailsList.Add(axisDetails);

						}

										          
											
												} else if (type == IOHIDElementType.kIOHIDElementTypeInput_Button) {
													numButtons++;
												}
								}
			
						}


			if (axisDetailsPovX != null) {
				int diff;

				diff=axisDetailsList.Count -8;
				//need 2 slots for Pov X,Y
				if(diff>=0){
					//just insert them
					axisDetailsList.Insert((int)JoystickAxis.AxisPovX,axisDetailsPovX);
					axisDetailsList.Insert((int)JoystickAxis.AxisPovY,axisDetailsPovY);
				}else if(diff<-1){
					diff=diff+2;
					while(diff<0){
						axisDetailsList.Add(null);
						diff+=1;
					}

					axisDetailsList.Add(axisDetailsPovX);
					axisDetailsList.Add(axisDetailsPovY);

				}else{//=-1
					axisDetailsList.Resize (9);
					axisDetailsList[(int)JoystickAxis.AxisPovX]=axisDetailsPovX;
					axisDetailsList[(int)JoystickAxis.AxisPovY]=axisDetailsPovY;
				}

								
								
			}


			device=new JoystickDevice(hidDevice.index,hidDevice.PID,hidDevice.VID,hidDevice.ID,axisDetailsList.Count,numButtons,this);
			device.Name = hidDevice.Name;
			device.numPOV = numPov;
			device.profile = profile;

			for(;axisIndex<device.Axis.Count;axisIndex++)
			{
				
				device.Axis[(JoystickAxis)axisIndex]=axisDetailsList[axisIndex];
				if (profile != null && profile.axisNaming.Length > axisIndex && device.Axis[axisIndex]!=null) {
					device.Axis[axisIndex].name = profile.axisNaming [axisIndex];
					
				}

			}
			

			
			
			
			for (int elementIndex = 0; elementIndex < numElements; elementIndex++){
				element = elements[elementIndex].typeRef;

				if(element!=IntPtr.Zero && Native.CFGetTypeID(element) == HIDElementType){
				type = Native.IOHIDElementGetType(element);
				 if (type == IOHIDElementType.kIOHIDElementTypeInput_Button) {
								//
								device.Buttons[buttonIndex]=new ButtonDetails(Native.IOHIDElementGetCookie(element));


						if (profile != null && profile.buttonNaming.Length > buttonIndex) {
							device.Buttons[buttonIndex].name = profile.buttonNaming [buttonIndex];
						}
						buttonIndex++;
							
				}
			}
				
		}







			//joystick.isReady = false;

						device.Extension=new OSXDefaultExtension();








             return device;

		}
		


#region ButtonDetails
		public sealed class ButtonDetails:IButtonDetails{
			
#region Fields
			
			float _value;
			uint _uid;
			ButtonState _buttonState;
			string _name;

#region IDeviceDetails implementation



			public string name {
				get {
					return _name;
				}
				set {
					_name=value;
				}
			}


			public uint uid {
				get {
					return _uid;
				}
				set {
					_uid=value;
				}
			}




			public ButtonState buttonState{
				get{return _buttonState; }
			}



			public float value{
				get{
					return _value;
					//return (_buttonState==ButtonState.Hold || _buttonState==ButtonState.Down);
				}
				set{

					_value = value;
					
					//  UnityEngine.Debug.Log("Value:" + _value);
					
					//if pressed==TRUE
					//TODO check the code with triggers
					if (value > 0)
					{
						if (_buttonState == ButtonState.None
						    || _buttonState == ButtonState.Up)
						{
							
							_buttonState = ButtonState.Down;
							
							
							
						}
						else
						{
							//if (buttonState == ButtonState.Down)
							_buttonState = ButtonState.Hold;
							
						}
						
						
					}
					else
					{ //
						if (_buttonState == ButtonState.Down
						    || _buttonState == ButtonState.Hold)
						{
							_buttonState = ButtonState.Up;
						}
						else
						{//if(buttonState==ButtonState.Up){
							_buttonState = ButtonState.None;
						}
						
					}
				}//set
			}
#endregion
#endregion
			
#region Constructor
			public ButtonDetails(uint uid=0){this.uid=uid; }
#endregion
			
			
			
			
			
			
		}
		
#endregion
		
#region AxisDetails
		public sealed class AxisDetails:IAxisDetails{
			
#region Fields
			float _value;
			uint _uid;
			int _min;
			int _max;
			ButtonState _buttonState;
			bool _isNullable;
			bool _isHat;
			float sensitivityOffset=0.3f;
			bool _isTrigger;
			string _name;
			
#region IAxisDetails implementation

			public string name {
				get {
					return _name;
				}
				set {
					_name=value;
				}
			}
				
				
				
				public bool isTrigger {
					get {
						return _isTrigger;
					}
					set {
						_isTrigger=true;
					}
				}




			public int min {
				get {
					return _min;
				}
				set {
					_min=value;
				}
			}


			public int max {
				get {
					return _max;
				}
				set {
					_max=value;
				}
			}


			public bool isNullable {
				get {
					return _isNullable;
				}
				set {
					_isNullable=value;
				}
			}


			public bool isHat {
				get {
					return _isHat;
				}
				set {
					_isHat=value;
				}
			}


#endregion


#region IDeviceDetails implementation


			public uint uid {
				get {
					return _uid;
				}
				 set {
					_uid=value;
				}
			}


#endregion

			public ButtonState buttonState{
				get{return _buttonState; }
			}
			public float value
			{
				get { return _value; }
				set
				{

					//TODO maybe dreadzone should goes here
					//sometimes even if stick is pushed to the end gives 0.7 instead 1f
					//maybe if value>0.7 should be counted as one
					if ( -value> 1- sensitivityOffset || value>1-sensitivityOffset)
					{
						if (_buttonState == ButtonState.None)
						    //|| _buttonState == ButtonState.PosToUp || _buttonState==ButtonState.NegToUp)
						{
							
							_buttonState = ButtonState.Down;
							
							//Debug.Log("val:"+value+"_buttonState:"+_buttonState);
							//Debug.Log("_buttonState:"+_buttonState);
						}
						else
						{
							_buttonState = ButtonState.Hold;

							//Debug.Log("_buttonState:"+_buttonState);
						}
						
						
					}
					else
					{
						
						if (_buttonState == ButtonState.Down
						    || _buttonState == ButtonState.Hold)
						{
							
							//if previous value was >0 => PosToUp
							if (_value>0)
								_buttonState = ButtonState.PosToUp;
							else
								_buttonState = ButtonState.NegToUp;

//							Debug.Log("_buttonState:"+_buttonState);
							//Debug.Log("val:"+value+"_buttonState:"+_buttonState);
							
						}
						else
						{//if(buttonState==ButtonState.Up){
							_buttonState = ButtonState.None;
//							Debug.Log("_buttonState:"+_buttonState);
						}
						
						
					}
					
					
					_value = value;
					
					
					
				}//set
			}

			
#endregion
			
		}
		
#endregion
		
		
		
		
		
		
		
		public sealed class OSXDefaultExtension:IDeviceExtension{
		}
		
		
		
		
	}
}

#endregion
#endif