using System;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic;

namespace RawInput_dll
{
    public class Codes
    {
        public const ushort RIDEV_INPUTSINK = 0x00000100;
        public const ushort RIDEV_PAGEONLY = 0x00000020;
        public const ushort INPUTSINK = 0x00000100;
        public const ushort DEVNOTIFY = 0x00002000;
    }

    // ReSharper disable FieldCanBeMadeReadOnly.Global
    public enum RawInputDeviceInfoType : uint
    {
        RIDI_DEVICENAME = 0x20000007,
        RIDI_DEVICEINFO = 0x2000000b,
        RIDI_PREPARSEDDATA = 0x20000005,
    }

    public enum RawInputDeviceType : uint
    {
        RIM_TYPEMOUSE = 0,
        RIM_TYPEKEYBOARD = 1,
        RIM_TYPEHID = 2,
    }


    [StructLayout(LayoutKind.Explicit)]
    public struct DeviceInfo
    {
        [FieldOffset(0)]
        public int Size;
        [FieldOffset(4)]
        public int Type;

        [FieldOffset(8)]
        public DeviceInfoMouse MouseInfo;
        [FieldOffset(8)]
        public DeviceInfoKeyboard KeyboardInfo;
        [FieldOffset(8)]
        public DeviceInfoHid HIDInfo;

        public override string ToString()
        {
            return string.Format("DeviceInfo\n Size: {0}\n Type: {1}\n", Size, Type);
        }
    }

    public struct DeviceInfoMouse
    {
        // ReSharper disable MemberCanBePrivate.Global
        public uint Id;                         // Identifier of the mouse device
        public uint NumberOfButtons;            // Number of buttons for the mouse
        public uint SampleRate;                 // Number of data points per second.
        public bool HasHorizontalWheel;         // True is mouse has wheel for horizontal scrolling else false.
        // ReSharper restore MemberCanBePrivate.Global
        public override string ToString()
        {
            return string.Format("MouseInfo\n Id: {0}\n NumberOfButtons: {1}\n SampleRate: {2}\n HorizontalWheel: {3}\n", Id, NumberOfButtons, SampleRate, HasHorizontalWheel);
        }
    }

    public struct DeviceInfoKeyboard
    {
        public uint Type;                       // Type of the keyboard
        public uint SubType;                    // Subtype of the keyboard
        public uint KeyboardMode;               // The scan code mode
        public uint NumberOfFunctionKeys;       // Number of function keys on the keyboard
        public uint NumberOfIndicators;         // Number of LED indicators on the keyboard
        public uint NumberOfKeysTotal;          // Total number of keys on the keyboard

        public override string ToString()
        {
            return string.Format("DeviceInfoKeyboard\n Type: {0}\n SubType: {1}\n KeyboardMode: {2}\n NumberOfFunctionKeys: {3}\n NumberOfIndicators {4}\n NumberOfKeysTotal: {5}\n",
                                                             Type,
                                                             SubType,
                                                             KeyboardMode,
                                                             NumberOfFunctionKeys,
                                                             NumberOfIndicators,
                                                             NumberOfKeysTotal);
        }
    }

    public struct DeviceInfoHid
    {
        public uint VendorID;       // Vendor identifier for the HID
        public uint ProductID;      // Product identifier for the HID
        public uint VersionNumber;  // Version number for the device
        public ushort UsagePage;    // Top-level collection Usage page for the device
        public ushort Usage;        // Top-level collection Usage for the device

        public override string ToString()
        {
            return string.Format("HidInfo\n VendorID: {0}\n ProductID: {1}\n VersionNumber: {2}\n UsagePage: {3}\n Usage: {4}\n", VendorID, ProductID, VersionNumber, UsagePage, Usage);
        }
    }

    struct BroadcastDeviceInterface
    {
        // ReSharper disable NotAccessedField.Global
        // ReSharper disable UnusedField.Compiler
        public Int32 DbccSize;
        public BroadcastDeviceType BroadcastDeviceType;
        public Int32 DbccReserved;
        public Guid DbccClassguid;
        public char DbccName;
        // ReSharper restore NotAccessedField.Global
        // ReSharper restore UnusedField.Compiler
    }


    /****************************************/
    [StructLayout(LayoutKind.Explicit)]
    public struct RawInput_Marshalling
    {
        //ASAP: MSDN
        [FieldOffset(0)]
        public Rawinputheader header;
        [FieldOffset(Rawinputheader.Size)]
        public Rawmouse mouse;
        [FieldOffset(Rawinputheader.Size)]
        public Rawkeyboard keyboard;
        [FieldOffset(Rawinputheader.Size)]
        public Rawhid_Marshalling hid;
    }

    public struct RawInput_NonMarshalling
    {
        public Rawinputheader header;
        public Rawmouse mouse;
        public Rawkeyboard keyboard;
        public Rawhid_NonMarshalling hid;
    }
    /****************************************/

    [StructLayout(LayoutKind.Sequential)]
    public struct Rawinputdevicelist
    {
        public IntPtr hDevice;
        public uint dwType;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RawData
    {
        [FieldOffset(0)]
        internal Rawmouse mouse;
        [FieldOffset(0)]
        internal Rawkeyboard keyboard;
        [FieldOffset(0)]
        internal Rawhid_NonMarshalling hid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct  InputData
    {
        public Rawinputheader header;           // 64 bit header size: 24  32 bit the header size: 16
        public RawData data;                    // Creating the rest in a struct allows the header size to align corRectly for 32/64 bit
    }

    [StructLayout(LayoutKind.Sequential, Size = Rawinputheader.Size)]
    public struct Rawinputheader
    {
        public uint dwType;                     // Type of raw input (RIM_TYPEHID 2, RIM_TYPEKEYBOARD 1, RIM_TYPEMOUSE 0)
        public uint dwSize;                     // Size in bytes of the entire input packet of data. This includes RAWINPUT plus possible extra input reports in the RAWHID variable length array.
        public IntPtr hDevice;                  // A handle to the device generating the raw input data.
        public IntPtr wParam;                   // RIM_INPUT 0 if input occurred while application was in the foreground else RIM_INPUTSINK 1 if it was not.
        public const int Size = 16;

        public override string ToString()
        {
            return string.Format("RawInputHeader\n dwType : {0}\n dwSize : {1}\n hDevice : {2}\n wParam : {3}", dwType, dwSize, hDevice, wParam);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rawhid_NonMarshalling
    {
        public uint dwSizHid;
        public uint dwCount;
        public byte bRawData;

        public override string ToString()
        {
            return string.Format("Rawhib\n dwSizeHid : {0}\n dwCount : {1}\n bRawData : {2}\n", dwSizHid, dwCount, bRawData);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rawhid_Marshalling
    {
        public uint dwSizHid;
        public uint dwCount;

        public override string ToString()
        {
            return string.Format("Rawhib\n dwSizeHid : {0}\n dwCount : {1}", dwSizHid, dwCount);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Rawmouse
    {
        [FieldOffset(0)]
        public ushort usFlags;
        [FieldOffset(4)]
        public uint ulButtons;
        [FieldOffset(4)]
        public ushort usButtonFlags;
        [FieldOffset(6)]
        public ushort usButtonData;
        [FieldOffset(8)]
        public uint ulRawButtons;
        [FieldOffset(12)]
        public int lLastX;
        [FieldOffset(16)]
        public int lLastY;
        [FieldOffset(20)]
        public uint ulExtraInformation;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rawkeyboard
    {
        public ushort Makecode;                 // Scan code from the key depression
        public ushort Flags;                    // One or more of RI_KEY_MAKE, RI_KEY_BREAK, RI_KEY_E0, RI_KEY_E1
        private readonly ushort Reserved;       // Always 0
        public ushort VKey;                     // Virtual Key Code
        public uint Message;                    // Corresponding Windows message for exmaple (WM_KEYDOWN, WM_SYASKEYDOWN etc)
        public uint ExtraInformation;           // The device-specific addition information for the event (seems to always be zero for keyboards)

        public override string ToString()
        {
            return string.Format("Rawkeyboard\n Makecode: {0}\n Makecode(hex) : {0:X}\n Flags: {1}\n Reserved: {2}\n VKeyName: {3}\n Message: {4}\n ExtraInformation {5}\n",
                                                Makecode, Flags, Reserved, VKey, Message, ExtraInformation);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RawInputDevice
    {
        internal HidUsagePage UsagePage;
        internal HidUsage Usage;
        internal RawInputDeviceFlags Flags;
        internal IntPtr Target;

        public override string ToString()
        {
            return string.Format("{0}/{1}, flags: {2}, target: {3}", UsagePage, Usage, Flags, Target);
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct RawInput2
    {
        [FieldOffset(0)]
        public Rawinputheader header;

        [FieldOffset(16 + 8)]
        public Rawmouse mouse;

        [FieldOffset(16 + 8)]
        public Rawkeyboard keyboard;

        [FieldOffset(16 + 8)]
        public Rawhid_NonMarshalling hid;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Rect
    {
        public int Left, Top, Right, Bottom;

        public Rect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public Rect(System.Drawing.Rectangle r) : this(r.Left, r.Top, r.Right, r.Bottom) { }

        public int X
        {
            get { return Left; }
            set { Right -= (Left - value); Left = value; }
        }

        public int Y
        {
            get { return Top; }
            set { Bottom -= (Top - value); Top = value; }
        }

        public int Height
        {
            get { return Bottom - Top; }
            set { Bottom = value + Top; }
        }

        public int Width
        {
            get { return Right - Left; }
            set { Right = value + Left; }
        }

        public System.Drawing.Point Location
        {
            get { return new System.Drawing.Point(Left, Top); }
            set { X = value.X; Y = value.Y; }
        }

        public System.Drawing.Size Size
        {
            get { return new System.Drawing.Size(Width, Height); }
            set { Width = value.Width; Height = value.Height; }
        }

        public static implicit operator System.Drawing.Rectangle(Rect r)
        {
            return new System.Drawing.Rectangle(r.Left, r.Top, r.Width, r.Height);
        }

        public static implicit operator Rect(System.Drawing.Rectangle r)
        {
            return new Rect(r);
        }

        public static bool operator ==(Rect r1, Rect r2)
        {
            return r1.Equals(r2);
        }

        public static bool operator !=(Rect r1, Rect r2)
        {
            return !r1.Equals(r2);
        }

        public bool Equals(Rect r)
        {
            return r.Left == Left && r.Top == Top && r.Right == Right && r.Bottom == Bottom;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rect)
                return Equals((Rect)obj);
            else if (obj is System.Drawing.Rectangle)
                return Equals(new Rect((System.Drawing.Rectangle)obj));
            return false;
        }

        public override int GetHashCode()
        {
            return ((System.Drawing.Rectangle)this).GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.CurrentCulture, "{{Left={0},Top={1},Right={2},Bottom={3}}}", Left, Top, Right, Bottom);
        }
    }
}
