using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace RawInput_dll
{
    //
    // Gesture configuration structure
    //   - Used in SetGestureConfig and GetGestureConfig
    //   - Note that any setting not included in either GESTURECONFIG.dwWant
    //     or GESTURECONFIG.dwBlock will use the parent window's preferences
    //     or system defaults.
    //
    // Touch API defined structures [winuser.h]
    [StructLayout(LayoutKind.Sequential)]
    public struct GESTURECONFIG
    {
        public int dwID;    // gesture ID
        public int dwWant;  // settings related to gesture ID that are to be
                            // turned on
        public int dwBlock; // settings related to gesture ID that are to be
                            // turned off
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct POINTS
    {
        public short x;
        public short y;
    }

    //
    // Gesture information structure
    //   - Pass the HGESTUREINFO received in the WM_GESTURE message lParam 
    //     into the GetGestureInfo function to retrieve this information.
    //   - If cbExtraArgs is non-zero, pass the HGESTUREINFO received in 
    //     the WM_GESTURE message lParam into the GetGestureExtraArgs 
    //     function to retrieve extended argument information.
    //
    [StructLayout(LayoutKind.Sequential)]
    public struct GESTUREINFO
    {
        public int cbSize;           // size, in bytes, of this structure
                                     // (including variable length Args 
                                     // field)
        public int dwFlags;          // see GF_* flags
        public int dwID;             // gesture ID, see GID_* defines
        public IntPtr hwndTarget;    // handle to window targeted by this 
                                     // gesture
        [MarshalAs(UnmanagedType.Struct)]
        internal POINTS ptsLocation; // current location of this gesture
        public int dwInstanceID;     // internally used
        public int dwSequenceID;     // internally used
        public Int64 ullArguments;   // arguments for gestures whose 
                                     // arguments fit in 8 BYTES
        public int cbExtraArgs;      // size, in bytes, of extra arguments, 
                                     // if any, that accompany this gesture
        public override string ToString()
        {
            return "dwFlags=" + dwFlags + ";dwID=" + dwID + ";x=" + ptsLocation.x +
                ";y=" + ptsLocation.y + ";dwInstanceID=" + dwInstanceID + ";dwSequenceID=" + dwSequenceID +
                ";ullArgs=" + String.Format("0x{0}", ullArguments.ToString("X16")) +
                ";cpExtraArgs=" + cbExtraArgs;
        }
    }

}
