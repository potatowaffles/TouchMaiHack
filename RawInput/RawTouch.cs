using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace RawInput_dll
{
    public sealed class RawTouch
    {
        public delegate void DeviceEventHandler(object sender, TouchInfo e);
        public event DeviceEventHandler TouchActivated;       

        readonly object _padLock = new object();

        private TouchDevice touchDevice;

        public int PrevX { get; set; }

        // size of GESTURECONFIG structure
        private int _gestureConfigSize;
        // size of GESTUREINFO structure
        private int _gestureInfoSize;


        public RawTouch(IntPtr hwnd, bool captureOnlyInForeground)
        {

            var rid = new RawInputDevice[1];

            rid[0].UsagePage = HidUsagePage.Digitizer; //this.touchDevice.DeviceInfo.UsagePage;
            rid[0].Usage = HidUsage.Joystick;
            rid[0].Flags = (captureOnlyInForeground ? RawInputDeviceFlags.NONE : RawInputDeviceFlags.INPUTSINK) | RawInputDeviceFlags.DEVNOTIFY;
            rid[0].Target = hwnd;

            SetupStructSizes();

            if (!Win32.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0])))
            {
                throw new ApplicationException("Failed to register raw input device(s).");
            }
        }

        public RawTouch(IntPtr hwnd, bool captureOnlyInForeground, TouchDevice touchDevice) : this(hwnd, captureOnlyInForeground)
        {
            this.touchDevice = touchDevice;
        }

        public int GuestureConfigSize { get { return _gestureConfigSize; } }

        public string Guesture { get; private set; }

        [SecurityPermission(SecurityAction.Demand)]
        private void SetupStructSizes()
        {
            // Both GetGestureCommandInfo and GetTouchInputInfo need to be
            // passed the size of the structure they will be filling
            // we get the sizes upfront so they can be used later.
            _gestureConfigSize = Marshal.SizeOf(new GESTURECONFIG());
            _gestureInfoSize = Marshal.SizeOf(new GESTUREINFO());
        }

        public void ReadDeviceInfo(IntPtr hdevice)
        {
            uint size = 0;
            var ret = Win32.GetRawInputDeviceInfo(hdevice, RawInputDeviceInfo.PREPARSEDDATA, IntPtr.Zero, ref size);

            if(ret == -1)
            {
                Console.WriteLine("Err");
                return;
            }

            IntPtr pData = Marshal.AllocHGlobal((int)size);

            ret = Win32.GetRawInputDeviceInfo(hdevice, RawInputDeviceInfo.PREPARSEDDATA, pData, ref size);

            Win32Hid.HIDP_CAPS caps = new Win32Hid.HIDP_CAPS();
            Win32Hid.HidP_GetCaps(pData, ref caps);
            
        }

        // this works for *my* monitor - I dunno about other monitors...
        // monitor is HannsG HT273HPB, running portrait 1080x1920
        public struct TouchInfo
        {
            public struct SingleTouch
            {
                public SingleTouch(byte[] buffer, int idx)
                {
                    status = buffer[idx * 6 + 0];
                    down = buffer[idx * 6 + 1];
                    id = buffer[idx * 6 + 2];
                    xls = buffer[idx * 6 + 3];
                    xyn = buffer[idx * 6 + 4];
                    yms = buffer[idx * 6 + 5];
                    yyn = buffer[idx * 6 + 6];
                }

                byte status;
                byte down;
                byte id;
                byte xls;
                byte xyn;
                byte yms;
                byte yyn;

                public int X()
                {
                    return (((xyn & 0x0F) << 8) + xls);
                }
                public int Y()
                {
                    return (((yyn & 0x0F) << 8) + yms);
                }
            }

            SingleTouch[] touches;

            public TouchInfo(byte[] buffer)
            {
                // this tells us how many touches there are...
                numTouches = buffer[61];
                touches = new SingleTouch[10]; 
                // if it's a release event - no touches
                if(buffer[1] == 0)
                {
                    numTouches = 0;
                }
                for (int i = 0; i < numTouches; ++i)
                {
                    touches[i] = new SingleTouch(buffer, i);
                }
            }

            byte numTouches;

            public int GetNumTouches()
            {
                return numTouches;
            }

            public SingleTouch GetTouch(int idx)
            {
                return touches[idx];
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="hdevice"></param>
        public void ProcessRawInput(IntPtr hdevice)
        {            
            var size = 0;

            // Determine Size to be allocated
            //var dwSiz = 0;
            //int ret2 = Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSiz, Marshal.SizeOf(typeof(Rawinputheader)));
            var ret = Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero,  ref size, Marshal.SizeOf(typeof(Rawinputheader)));

            if (ret == -1)
            {
                Console.WriteLine("error");
                return;
            }

            int sizeToAllocate = Math.Max(size, Marshal.SizeOf(typeof(RawInput_Marshalling)));

            IntPtr pData = Marshal.AllocHGlobal(sizeToAllocate);
            try
            {
                //Populate alocated memory
                ret = Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, pData, ref sizeToAllocate, Marshal.SizeOf(typeof(Rawinputheader)));

                if (ret == -1)
                {
                    throw new System.ComponentModel.Win32Exception();
                }

                Rawinputheader header = (Rawinputheader) Marshal.PtrToStructure(pData, typeof(Rawinputheader));

                //RAWINPUT starts with RAWINPUTHEADER, so we can do this
                RawInputDeviceType type = (RawInputDeviceType)header.dwType;
                switch (type)
                {
                    case RawInputDeviceType.RIM_TYPEHID:
                        {
                            //As described on page of RAWHID, RAWHID needs special treatement
                            RawInput_Marshalling raw = (RawInput_Marshalling) Marshal.PtrToStructure(pData, typeof(RawInput_Marshalling));

                            //Do some copying
                            //raw2.header = raw.header;

                            ReadDeviceInfo(raw.header.hDevice);

                            var numBytes = raw.header.dwSize - (Marshal.SizeOf(typeof(Rawinputheader)) + Marshal.SizeOf(typeof(Rawhid_Marshalling)));// * raw.hid.dwSizHid;
                            byte[] data = new byte[numBytes];

                            // ERROR: Not supported in C#: ReDimStatement

                            //Allocate array
                            //Populate the array

                            // new pointer - offset by header sizes
                            IntPtr rawData =  new IntPtr(
                                pData.ToInt64() + Marshal.SizeOf(typeof(Rawinputheader)) + Marshal.SizeOf(typeof(Rawhid_Marshalling)));

                            Marshal.Copy(rawData, data, 0, (int)numBytes);

                            if (numBytes > 0)
                            {
                                for (int i = 0; i < numBytes; ++i)
                                {
                                    Console.Write(String.Format("{0:X2}", data[i]));
                                }
                                Console.WriteLine();

                                TouchInfo ti = new TouchInfo(data);
                                //Console.WriteLine(ti.X() + " " + ti.Y());

                                if (TouchActivated != null)
                                {
                                    TouchActivated(this, ti);
                                }
                            }
                            else
                            {
                                Console.WriteLine("no bytes...");
                            }
                            //return raw2;
                            break;
                        }
                    default:
                        {
                            //No additional processing is needed
                            var x = (RawInput_Marshalling)Marshal.PtrToStructure(pData, typeof(RawInput_Marshalling));
                            break;
                        }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pData);
            }
        }



        public bool DecodeGesture(ref Message m)
        {
            GESTUREINFO gi;

            try
            {
                gi = new GESTUREINFO();
            }
            catch (Exception excep)
            {
                Debug.Print("Could not allocate resources to decode gesture");
                Debug.Print(excep.ToString());

                return false;
            }

            gi.cbSize = _gestureInfoSize;

            // Load the gesture information.
            // We must p/invoke into user32 [winuser.h]
            if (!Win32.GetGestureInfo(m.LParam, ref gi))
            {
                return false;
            }

            switch (gi.dwID)
            {
                case Win32.GID_BEGIN:
                    {
                        Console.WriteLine("touch begin");
                        break;
                    }
                case Win32.GID_END:
                    {
                        Console.WriteLine("touch end");
                        break;
                    }

                case Win32.GID_PAN:

                    switch (gi.dwFlags)
                    {
                        case Win32.GF_BEGIN:

                            Console.WriteLine("PAN BEGIN: " + gi.ToString() + System.Environment.NewLine);
                            break;
                        case Win32.GF_INERTIA:
                            //In this case the ullArguments encodes direction and velocity
                            Console.WriteLine("PAN INERTIA: " + gi.ToString() + System.Environment.NewLine);
                            break;
                        case Win32.GF_END:
                            Console.WriteLine("PAN END: " + gi.ToString() + System.Environment.NewLine);
                            break;
                        case Win32.GF_END | Win32.GF_INERTIA:
                            Console.WriteLine("PAN END: " + gi.ToString() + System.Environment.NewLine);
                            break;
                        default:
                            Console.WriteLine("PAN: " + gi.ToString() + System.Environment.NewLine);
                            break;
                    }
                    break;


            }

            return true;
        }
    }

}


/*

          case Win32.GID_ZOOM:
                    switch (gi.dwFlags)
                    {
                        //case Win32.GF_BEGIN:
                        //    _iArguments = (int)(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK);
                        //    _ptFirst.X = gi.ptsLocation.x;
                        //    _ptFirst.Y = gi.ptsLocation.y;
                        //    _ptFirst = PointToClient(_ptFirst);
                        //    this.textBoxStatus.Clear();
                        //    this.textBoxStatus.AppendText("ZOOM BEGIN: (" + gi.ptsLocation.x + "," + gi.ptsLocation.y + ")" + System.Environment.NewLine);
                        //    break;

                        //default:
                        //    // We read here the second point of the gesture. This
                        //    // is middle point between fingers in this new
                        //    // position.
                        //    _ptSecond.X = gi.ptsLocation.x;
                        //    _ptSecond.Y = gi.ptsLocation.y;
                        //    _ptSecond = PointToClient(_ptSecond);
                        //    this.textBoxStatus.AppendText("ZOOM: (" + gi.ptsLocation.x + "," + gi.ptsLocation.y + ")" + System.Environment.NewLine);

                        //    // We have to calculate zoom center point
                        //    Point ptZoomCenter = new Point((_ptFirst.X + _ptSecond.X) / 2,
                        //                                (_ptFirst.Y + _ptSecond.Y) / 2);

                        //    // The zoom factor is the ratio of the new
                        //    // and the old distance. The new distance
                        //    // between two fingers is stored in
                        //    // gi.ullArguments (lower 4 bytes) and the old
                        //    // distance is stored in _iArguments.
                        //    double k = (double)(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK) /
                        //                (double)(_iArguments);


                        //    // Now we have to store new information as a starting
                        //    // information for the next step in this gesture.
                        //    _ptFirst = _ptSecond;
                        //    _iArguments = (int)(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK);
                        //    break;
                    }
                    break;

                case Win32.GID_ROTATE:
                    //switch (gi.dwFlags)
                    //{
                    //    case GF_BEGIN:
                    //        _iArguments = 0;
                    //        this.textBoxStatus.Clear();
                    //        this.textBoxStatus.AppendText("ROTATE BEGIN: (" + gi.ptsLocation.x + "," + gi.ptsLocation.y + ")" + System.Environment.NewLine);

                    //        break;

                    //    default:
                    //        _ptFirst.X = gi.ptsLocation.x;
                    //        _ptFirst.Y = gi.ptsLocation.y;
                    //        _ptFirst = PointToClient(_ptFirst);
                    //        this.textBoxStatus.AppendText("ROTATE: (" + gi.ptsLocation.x + "," + gi.ptsLocation.y + ")" + System.Environment.NewLine);

                    //        // Gesture handler returns cumulative rotation angle.

                    //        _iArguments = (int)(gi.ullArguments & ULL_ARGUMENTS_BIT_MASK);
                    //        break;
                    //}
                    break;

                case Win32.GID_TWOFINGERTAP:
                    //this.textBoxStatus.Clear();
                    //this.textBoxStatus.AppendText("TWOFINGERTAP: (" + gi.ptsLocation.x + "," + gi.ptsLocation.y + ")" + System.Environment.NewLine);

                    break;

                case Win32.GID_PRESSANDTAP:
                    //if (gi.dwFlags == GF_BEGIN)
                    //{
                    //    this.textBoxStatus.Clear();
                    //}
                    //this.textBoxStatus.AppendText("PRESSANDTAP: (" + gi.ptsLocation.x + "," + gi.ptsLocation.y + ")" + System.Environment.NewLine);

                    break;



            var dwSize = 0;
            Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, IntPtr.Zero, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader)));


            if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader))))
            {
                Debug.WriteLine("Error getting the rawinput buffer");
                return;
            }

            if (_rawBuffer.header.dwType == (uint) RawInputDeviceType.RIM_TYPEHID)
            {
                List<byte> data = new List<byte>();
                do
                {
                    Rawhid_NonMarshalling hid = _rawBuffer.data.hid;
                    data.Add(hid.bRawData);

                    dwSize = (int)hid.dwSizHid;

                    Console.WriteLine($"DWSize:{dwSize} dwSizHid:{hid.dwSizHid} dwCount:{hid.dwCount} dwRawData:{hid.bRawData}");

                    if (dwSize != Win32.GetRawInputData(hdevice, DataCommand.RID_INPUT, out _rawBuffer, ref dwSize, Marshal.SizeOf(typeof(Rawinputheader))))
                    {

                    }

                }
                while (dwSize > 0);

                int x =  _rawBuffer.data.mouse.lLastX;
                //Console.WriteLine($"data: dwcount- {_rawBuffer.data.hid.dwCount}, dwsize- {_rawBuffer.data.hid.dwSizHid}, byte {_rawBuffer.data.hid.bRawData.ToString()} ");
                //Console.WriteLine($"{x}");
                if (x < this.PrevX)
                {
                    this.Guesture = "swipe left";
                }
                else
                {
                    this.Guesture = "unknown";
                }

                this.PrevX = x;

                Console.WriteLine($"{this.Guesture} : {x} < {Win32.TouchDevice.Width}");
            }

    */
