using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace RawInput_dll
{
    public class RawInput : NativeWindow
    {
        static RawTouch _touchDriver;

        readonly IntPtr _devNotifyHandle;
        static readonly Guid DeviceInterfaceHid = new Guid("4D1E55B2-F16F-11CF-88CB-001111000030");

        public  event RawTouch.DeviceEventHandler TouchActivated
        {
            add
            {
                if (_touchDriver != null)
                {
                    _touchDriver.TouchActivated += value;
                }
            }
            remove
            {
                if (_touchDriver != null)
                {
                    _touchDriver.TouchActivated -= value;
                }
            }
        }

  
        public RawInput(IntPtr parentHandle, bool captureOnlyInForeground)
        {
            AssignHandle(parentHandle);

            //_keyboardDriver = new RawKeyboard(parentHandle, captureOnlyInForeground);
            //_keyboardDriver.EnumerateDevices();

            _touchDriver = new RawTouch(parentHandle, captureOnlyInForeground, Win32.TouchDevice);
            //_touchDriver.EnumerateDevices();

           // _devNotifyHandle = RegisterForDeviceNotifications(parentHandle);
        }

        static IntPtr RegisterForDeviceNotifications(IntPtr parent)
        {
            var usbNotifyHandle = IntPtr.Zero;
            var bdi = new BroadcastDeviceInterface();
            bdi.DbccSize = Marshal.SizeOf(bdi);
            bdi.BroadcastDeviceType = BroadcastDeviceType.DBT_DEVTYP_DEVICEINTERFACE;
            bdi.DbccClassguid = DeviceInterfaceHid;

            var mem = IntPtr.Zero;
            try
            {
                mem = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(BroadcastDeviceInterface)));
                Marshal.StructureToPtr(bdi, mem, false);
                usbNotifyHandle = Win32.RegisterDeviceNotification(parent, mem, DeviceNotification.DEVICE_NOTIFY_WINDOW_HANDLE);
            }
            catch (Exception e)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
                Debug.Print(e.StackTrace);
            }
            finally
            {
                Marshal.FreeHGlobal(mem);
            }

            if (usbNotifyHandle == IntPtr.Zero)
            {
                Debug.Print("Registration for device notifications Failed. Error: {0}", Marshal.GetLastWin32Error());
            }

            return usbNotifyHandle;
        }

        protected override void WndProc(ref Message message)
        {
            //Console.WriteLine(message.Msg);
            switch (message.Msg)
            {           
                case Win32.WM_INPUT:
                    {
                       // Console.WriteLine("wm_input");
                        _touchDriver.ProcessRawInput(message.LParam);
                        break;
                    }
            }

            base.WndProc(ref message);
        }

        ~RawInput()
        {
            Win32.UnregisterDeviceNotification(_devNotifyHandle);
        }
    }
}


/*

    case Win32.WM_GESTURENOTIFY:
                    {
                        break;
                        Console.WriteLine("WM GUESture notify");
                        // This is the right place to define the list of gestures
                        // that this application will support. By populating
                        // GESTURECONFIG structure and calling SetGestureConfig
                        // function. We can choose gestures that we want to
                        // handle in our application. In this app we decide to
                        // handle all gestures.
                        GESTURECONFIG gc = new GESTURECONFIG();
                        gc.dwID = 0;                // gesture ID
                        gc.dwWant = Win32.GC_ALLGESTURES; // settings related to gesture
                                                    // ID that are to be turned on
                        gc.dwBlock = 0; // settings related to gesture ID that are
                                        // to be

                        // We must p/invoke into user32 [winuser.h]
                        bool bResult = Win32.SetGestureConfig(
                            Handle, // window for which configuration is specified
                            0,      // reserved, must be 0
                            1,      // count of GESTURECONFIG structures
                            ref gc, // array of GESTURECONFIG structures, dwIDs
                                    // will be processed in the order specified
                                    // and repeated occurances will overwrite
                                    // previous ones
                            _touchDriver.GuestureConfigSize // sizeof(GESTURECONFIG)
                        );

                        if (!bResult)
                        {
                            throw new Exception("Error in execution of SetGestureConfig");
                        }

                    break;
                    }

                case Win32.WM_GESTURE:
                {
                    Console.WriteLine("WM GUESTURE");
                    //// The gesture processing code is implemented in
                    //// the DecodeGesture method
                    bool handled = _touchDriver.DecodeGesture(ref message);
                    break;
                }


    */
