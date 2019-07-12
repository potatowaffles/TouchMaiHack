using System;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace RawInput_dll
{
	public sealed class RawKeyboard
	{
		private readonly Dictionary<IntPtr,KeyPressEvent> _deviceList = new Dictionary<IntPtr,KeyPressEvent>();
		public delegate void DeviceEventHandler(object sender, RawInputEventArg e);
		public event DeviceEventHandler KeyPressed;
		readonly object _padLock = new object();
		public int NumberOfKeyboards { get; private set; }
        public uint ProductID { get; private set; }
        public uint VendorID { get; private set; }

        static InputData _rawBuffer;

		public RawKeyboard(IntPtr hwnd, bool captureOnlyInForeground)
		{
            //initTouch();
			var rid = new RawInputDevice[1];

			rid[0].UsagePage = HidUsagePage.GENERIC;
			rid[0].Usage = HidUsage.Keyboard;
            rid[0].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
			rid[0].Target = hwnd;

			if(!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
			{
				throw new ApplicationException("Failed to register raw input device(s).");
			}
		}

        //private void initTouch()
        //{

        //    bool res = false;
        //    Rawinputdevicelist[] pRawInputDeviceList = null;
        //    uint puiNumDevices = 0;
        //    uint returnCode = Win32.GetRawInputDeviceList(pRawInputDeviceList, ref puiNumDevices, (uint)Marshal.SizeOf(new Rawinputdevicelist()));
        //    res = (0xFFFFFFFF != returnCode);
        //    if (res)
        //    {
        //        //alloc array
        //        pRawInputDeviceList = new Rawinputdevicelist[puiNumDevices];

        //        //get devices
        //        returnCode = Win32.GetRawInputDeviceList(pRawInputDeviceList, ref puiNumDevices, (uint)Marshal.SizeOf((typeof(Rawinputdevicelist))));
        //        res = (0xFFFFFFFF != returnCode);
        //        if (res)
        //        {
        //            //look for the touchscreen.
        //            bool foundTouchScreen = false;
        //            foreach (Rawinputdevicelist rawInputDevice in pRawInputDeviceList)
        //            {


        //                uint structsize = (uint)Marshal.SizeOf(typeof(DeviceInfo));
        //                DeviceInfo di = new DeviceInfo();
        //                di.Size = (int)structsize;
        //                IntPtr pData = Marshal.AllocHGlobal((int)structsize);
        //                returnCode = Win32.GetRawInputDeviceInfo(rawInputDevice.hDevice, RawInputDeviceInfoType.RIDI_DEVICEINFO, pData, ref structsize);
        //                if (0xFFFFFFF != returnCode && 0 != returnCode)
        //                {
        //                    di = (DeviceInfo)Marshal.PtrToStructure(pData, typeof(DeviceInfo));
        //                    //Console.WriteLine("di.dwType = " + Enum.GetName(typeof(RawInputDeviceType), di.dwType));

        //                    uint comparor = (uint)di.Type;

        //                    if (comparor == (uint)RawInputDeviceType.RIM_TYPEHID)
        //                    {
        //                        /* Console.WriteLine("di.hid.dwVendorId = " + di.hid.dwVendorId);
        //                           Console.WriteLine("di.hid.dwProductId = " + di.hid.dwProductId);
        //                           Console.WriteLine("di.hid.dwVersionNumber = " + di.hid.dwVersionNumber);
        //                           Console.WriteLine("di.hid.usUsagePage = " + di.hid.usUsagePage);
        //                           Console.WriteLine("di.hid.usUsage = " + di.hid.usUsage);*/
        //                        if (0x0D == di.HIDInfo.UsagePage && 0x04 == di.HIDInfo.Usage)
        //                        {
        //                            VendorID = di.HIDInfo.VendorID;
        //                            ProductID = di.HIDInfo.ProductID;
        //                            foundTouchScreen = true;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
                    //        if (foundTouchScreen)
                    //        {
                    //            RAWINPUTDEVICE[] rawInputDevicesToMonitor = new RAWINPUTDEVICE[1];
                    //            RAWINPUTDEVICE device = new RAWINPUTDEVICE();
                    //            device.dwFlags = INPUTSINK | DEVNOTIFY;

                    //            //RIDEV_INPUTSINK;
                    //            device.hwndTarget = Process.GetCurrentProcess().MainWindowHandle;
                    //            device.usUsage = di.hid.usUsage;
                    //            device.usUsagePage = di.hid.usUsagePage;
                    //            rawInputDevicesToMonitor[0] = device;

                    //            if (!RegisterRawInputDevices(rawInputDevicesToMonitor, (uint)1, (uint)Marshal.SizeOf(new RAWINPUTDEVICE())))
                    //            {
                    //                Console.WriteLine("Registration of device --> NOK (error: " + Marshal.GetLastWin32Error() + ")");
                    //                RAWINPUTDEVICE[] pRegisteredRawInputDeviceList = null;
                    //                uint puiNumRegDevices = 0;
                    //                returnCode = GetRegisteredRawInputDevices(pRegisteredRawInputDeviceList, ref puiNumRegDevices, (uint)Marshal.SizeOf(new RAWINPUTDEVICE()));
                    //                res = (0xFFFFFFFF != returnCode);
                    //                if (res)
                    //                {
                    //                    //alloc array
                    //                    pRegisteredRawInputDeviceList = new RAWINPUTDEVICE[puiNumRegDevices];

                    //                    //get devices
                    //                    returnCode = GetRegisteredRawInputDevices(pRegisteredRawInputDeviceList, ref puiNumRegDevices, (uint)Marshal.SizeOf((typeof(RAWINPUTDEVICE))));

                    //                    Console.WriteLine("Registered devices nb : " + returnCode);
                    //                    //}
                    //                }
                    //                break;
                    //            }
                    //        }
                    //    }
                    //}


        public void EnumerateDevices()
		{
			lock (_padLock)
			{
				_deviceList.Clear();

				var keyboardNumber = 0;

				var globalDevice = new KeyPressEvent
				{
					DeviceName = "Global Keyboard",
					DeviceHandle = IntPtr.Zero,
					DeviceType = Win32.GetDeviceType(DeviceType.RimTypekeyboard),
					Name = "Fake Keyboard. Some keys (ZOOM, MUTE, VOLUMEUP, VOLUMEDOWN) are sent to rawinput with a handle of zero.",
					Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
				};

				_deviceList.Add(globalDevice.DeviceHandle, globalDevice);

                var numberOfDevices = 0;
                uint deviceCount = 0;
                var dwSize = (Marshal.SizeOf(typeof(Rawinputdevicelist)));

                if (Win32.GetRawInputDeviceList(IntPtr.Zero, ref deviceCount, (uint)dwSize) == 0)
                {
                    var pRawInputDeviceList = Marshal.AllocHGlobal((int)(dwSize * deviceCount));
                    Win32.GetRawInputDeviceList(pRawInputDeviceList, ref deviceCount, (uint)dwSize);

                    for (var i = 0; i < deviceCount; i++)
                    {
                        uint pcbSize = 0;

                        // On Window 8 64bit when compiling against .Net > 3.5 using .ToInt32 you will generate an arithmetic overflow. Leave as it is for 32bit/64bit applications
                        var rid = (Rawinputdevicelist)Marshal.PtrToStructure(new IntPtr((pRawInputDeviceList.ToInt64() + (dwSize * i))), typeof(Rawinputdevicelist));

                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, IntPtr.Zero, ref pcbSize);

                        if (pcbSize <= 0) continue;

                        var pData = Marshal.AllocHGlobal((int)pcbSize);
                        Win32.GetRawInputDeviceInfo(rid.hDevice, RawInputDeviceInfo.RIDI_DEVICENAME, pData, ref pcbSize);
                        var deviceName = Marshal.PtrToStringAnsi(pData);

                        if (rid.dwType == DeviceType.RimTypekeyboard || rid.dwType == DeviceType.RimTypeHid)
                        {
                            var deviceDesc = Win32.GetDeviceDescription(deviceName);

                            var dInfo = new KeyPressEvent
                            {
                                DeviceName = Marshal.PtrToStringAnsi(pData),
                                DeviceHandle = rid.hDevice,
                                DeviceType = Win32.GetDeviceType(rid.dwType),
                                Name = deviceDesc,
                                Source = keyboardNumber++.ToString(CultureInfo.InvariantCulture)
                            };

                            if (!_deviceList.ContainsKey(rid.hDevice))
                            {
                                numberOfDevices++;
                                _deviceList.Add(rid.hDevice, dInfo);
                            }
                        }

                        Marshal.FreeHGlobal(pData);
                    }

                    Marshal.FreeHGlobal(pRawInputDeviceList);

                    NumberOfKeyboards = numberOfDevices;
                    Debug.WriteLine("EnumerateDevices() found {0} Keyboard(s)", NumberOfKeyboards);
                    return;
                }
                return;
			}

			throw new Win32Exception(Marshal.GetLastWin32Error());
		}


  //      public void ProcessRawInput(IntPtr hdevice)
		//{
		//	//Debug.WriteLine(_rawBuffer.data.keyboard.ToString());
		//	//Debug.WriteLine(_rawBuffer.data.hid.ToString());
		//	//Debug.WriteLine(_rawBuffer.header.ToString());

		//	if (_deviceList.Count == 0) return;

		//	var dwSize = 0;
		//	Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader)));

		//	if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof (Rawinputheader))))
		//	{
		//		Debug.WriteLine("Error getting the rawinput buffer");
		//		return;
		//	}

		//	int virtualKey = _rawBuffer.data.keyboard.VKey;
		//	int makeCode = _rawBuffer.data.keyboard.Makecode;
		//	int flags = _rawBuffer.data.keyboard.Flags;

		//	if (virtualKey == Win32.KEYBOARD_OVERRUN_MAKE_CODE) return;

		//	var isE0BitSet = ((flags & Win32.RI_KEY_E0) != 0);

		//	KeyPressEvent keyPressEvent;

		//	if (_deviceList.ContainsKey(_rawBuffer.header.hDevice))
		//	{
		//		lock (_padLock)
		//		{
		//			keyPressEvent = _deviceList[_rawBuffer.header.hDevice];
		//		}
		//	}
		//	else
		//	{
		//		Debug.WriteLine("Handle: {0} was not in the device list.", _rawBuffer.header.hDevice);
		//		return;
		//	}

		//	var isBreakBitSet = ((flags & Win32.RI_KEY_BREAK) != 0);

		//	keyPressEvent.KeyPressState = isBreakBitSet ? "BREAK" : "MAKE";
		//	keyPressEvent.Message = _rawBuffer.data.keyboard.Message;
		//	keyPressEvent.VKeyName = KeyMapper.GetKeyName(VirtualKeyCorrection(virtualKey, isE0BitSet, makeCode)).ToUpper();
		//	keyPressEvent.VKey = virtualKey;

		//	if (KeyPressed != null)
		//	{
		//		KeyPressed(this, new RawInputEventArg(keyPressEvent));
		//	}
		//}

		private static int VirtualKeyCorrection(int virtualKey, bool isE0BitSet, int makeCode)
		{
			var correctedVKey = virtualKey;

			if (_rawBuffer.header.hDevice == IntPtr.Zero)
			{
				// When hDevice is 0 and the vkey is VK_CONTROL indicates the ZOOM key
				if (_rawBuffer.data.keyboard.VKey == Win32.VK_CONTROL)
				{
					correctedVKey = Win32.VK_ZOOM;
				}
			}
			else
			{
				switch (virtualKey)
				{
					// Right-hand CTRL and ALT have their e0 bit set
					case Win32.VK_CONTROL:
						correctedVKey = isE0BitSet ? Win32.VK_RCONTROL : Win32.VK_LCONTROL;
						break;
					case Win32.VK_MENU:
						correctedVKey = isE0BitSet ? Win32.VK_RMENU : Win32.VK_LMENU;
						break;
					case Win32.VK_SHIFT:
						correctedVKey = makeCode == Win32.SC_SHIFT_R ? Win32.VK_RSHIFT : Win32.VK_LSHIFT;
						break;
					default:
						correctedVKey = virtualKey;
						break;
				}
			}

			return correctedVKey;
		}
	}
}
