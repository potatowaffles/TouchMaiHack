using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Keyboard
{
    class MaiMaiConnection
    {
        // these are the bits to set on the touch sensor presses...
        public enum TouchSensorPress
        {
            A1 = 1 << 0,
            B1 = 1 << 1,
            A2 = 1 << 2,
            B2 = 1 << 3,
            // gap 4
            A3 = 1 << 5,
            B3 = 1 << 6,
            A4 = 1 << 7,
            B4 = 1 << 8,
            // gap 9
            A5 = 1 << 10,
            B5 = 1 << 11,
            A6 = 1 << 12,
            B6 = 1 << 13,
            // gap 14
            A7 = 1 << 15,
            B7 = 1 << 16,
            A8 = 1 << 17,
            B8 = 1 << 18,
            C = 1 << 19,
        }

        const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);


        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rectangle);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
        }

        /// <summary>
        /// Retrieves the cursor's position, in screen coordinates.
        /// </summary>
        /// <see>See MSDN documentation for further information.</see>
        [DllImport("user32.dll")]
        static extern bool GetCursorPos(out POINT lpPoint);

        public static POINT GetCursorPosition()
        {
            POINT lpPoint;
            GetCursorPos(out lpPoint);
            //bool success = User32.GetCursorPos(out lpPoint);
            // if (!success)

            return lpPoint;
        }

        static int GetTouchedAreas(float xSRel, float ySRel)
        {
            // relative to centre
            int result = 0;
            // work out hwere on the screen we've touched... there's a bit of overlap - so it can be more than one place...
            float distNorm = (float)Math.Sqrt((xSRel * xSRel) + (ySRel * ySRel)) / 0.25f;
            if (distNorm < 1.1f)
            {
                if (distNorm < 0.3f)
                {
                    result |= (int)TouchSensorPress.C;
                }
                float deg = (float)(Math.Atan2(xSRel, ySRel) * (180.0 / Math.PI));
                if (deg > 180.0f)
                {
                    deg -= 360.0f;
                }
                if (deg < -180.0f)
                {
                    deg += 360.0f;
                }
                if (distNorm > 0.275f && distNorm < 0.7f)
                {
                    if (deg < 50.0f && deg >= -5.0f)
                    {
                        result |= (int)TouchSensorPress.B1;
                    }
                    if (deg < 95.0f && deg >= 40.0f)
                    {
                        result |= (int)TouchSensorPress.B2;
                    }
                    if (deg < 140.0f && deg >= 85.0f)
                    {
                        result |= (int)TouchSensorPress.B3;
                    }
                    if ((deg >= 130.0f && deg <= 185.0f) || (deg < -175.0f))
                    {
                        result |= (int)TouchSensorPress.B4;
                    }
                    if (deg > -50.0f && deg <= 5.0f)
                    {
                        result |= (int)TouchSensorPress.B8;
                    }
                    if (deg > -95.0f && deg <= -40.0f)
                    {
                        result |= (int)TouchSensorPress.B7;
                    }
                    if (deg > -140.0f && deg <= -85.0f)
                    {
                        result |= (int)TouchSensorPress.B6;
                    }
                    if ((deg <= -130.0f && deg >= -185.0f) || (deg > 175.0f))
                    {
                        result |= (int)TouchSensorPress.B5;
                    }
                }
                if (distNorm > 0.65f)
                {
                    if (deg < 50.0f && deg >= -5.0f)
                    {
                        result |= (int)TouchSensorPress.A1;
                    }
                    if (deg < 95.0f && deg >= 40.0f)
                    {
                        result |= (int)TouchSensorPress.A2;
                    }
                    if (deg < 140.0f && deg >= 85.0f)
                    {
                        result |= (int)TouchSensorPress.A3;
                    }
                    if ((deg >= 130.0f && deg <= 185.0f) || (deg < -175.0f))
                    {
                        result |= (int)TouchSensorPress.A4;
                    }
                    if (deg > -50.0f && deg <= 5.0f)
                    {
                        result |= (int)TouchSensorPress.A8;
                    }
                    if (deg > -95.0f && deg <= -40.0f)
                    {
                        result |= (int)TouchSensorPress.A7;
                    }
                    if (deg > -140.0f && deg <= -85.0f)
                    {
                        result |= (int)TouchSensorPress.A6;
                    }
                    if ((deg <= -130.0f && deg >= -185.0f) || (deg > 175.0f))
                    {
                        result |= (int)TouchSensorPress.A5;
                    }
                }
            }

            return result;
        }

        bool m_hasProcess = false;
        RawInput_dll.RawTouch.TouchInfo m_touchInfo = new RawInput_dll.RawTouch.TouchInfo();

        public void SetTouchInfo(RawInput_dll.RawTouch.TouchInfo ti)
        {
            m_touchInfo = ti;
        }

        public enum MaiMaiState
        {
            Waiting,
            Connecting,
            Setup,
            Running,
            Error
        }

        private MaiMaiState m_currentState;

        public MaiMaiState GetState()
        {
            return m_currentState;
        }

        public void RunThread()
        {
            m_currentState = MaiMaiState.Waiting;
            while (true)
            {
                Thread.Sleep(500);

                Process[] allMais = Process.GetProcessesByName("maimai_dump_");

                if (allMais.Length > 0 && allMais[0] != null)
                {
                    m_currentState = MaiMaiState.Connecting;
                    Thread.Sleep(10000);
                    Process process = allMais[0];
                    IntPtr processHandle = process.Handle;// OpenProcess(PROCESS_WM_READ, false, process.Id);

                    int bytesRead = 0;
                    byte[] buffer = new byte[4]; // ptr 4 bytes

                    // 0x0046A3B8 is the address where I found the string, replace it with what you found
                    Int32 baseAddress = process.MainModule.BaseAddress.ToInt32();
                    // 0x8DF9C0 is Reaver's magic address
                    ReadProcessMemory((int)processHandle, 0x8DF9C0, buffer, buffer.Length, ref bytesRead);

                    // get the pointer
                    int val = BitConverter.ToInt32(buffer, 0);
                    Console.WriteLine(string.Format("{0:X}", val));

                    m_currentState = MaiMaiState.Setup;

                    // get the window rect for MaiMai
                    RECT rect = new RECT();

                    try
                    {
                        GetWindowRect(process.MainWindowHandle, ref rect);
                        Console.WriteLine(rect.Bottom + " " + rect.Left + " " + rect.Top + " " + rect.Right);
                    }
                    catch
                    {
                        m_currentState = MaiMaiState.Error;
                    }

                    byte[] structBuf = new byte[8];

                    try
                    {
                        ReadProcessMemory((int)processHandle, val + 52, structBuf, structBuf.Length, ref bytesRead);

                        for (int i = 0; i < 8; ++i)
                        {
                            Console.Write(string.Format("{0:X}", structBuf[i]));
                        }
                        Console.WriteLine();
                    }
                    catch
                    {
                        m_currentState = MaiMaiState.Error;
                    }


                    byte[] toWrite = new byte[4];
                    while (m_currentState != MaiMaiState.Error)
                    {
                        m_currentState = MaiMaiState.Running;
                        RawInput_dll.RawTouch.TouchInfo touchCopy = m_touchInfo;
                        int j = 0;
                        for (int i = 0; i < m_touchInfo.GetNumTouches(); ++i)
                        {
                            GetWindowRect(process.MainWindowHandle, ref rect);
                            if(rect.Bottom == rect.Top)
                            {
                                rect.Bottom = Screen.PrimaryScreen.Bounds.Height;
                                rect.Top = 0;
                                rect.Left = 0;
                                rect.Right = Screen.PrimaryScreen.Bounds.Width;
                            }
                            RawInput_dll.RawTouch.TouchInfo.SingleTouch st = m_touchInfo.GetTouch(i);
                            // x and y flipped 'coz portrait
                            int xScreen = (int)(((float)st.Y() / (float)4096) * Screen.PrimaryScreen.Bounds.Width);
                            int yScreen = Screen.PrimaryScreen.Bounds.Height - ((int)(((float)st.X() / (float)4096) * Screen.PrimaryScreen.Bounds.Height));
                            float xS = (float)(xScreen - rect.Left) / (float)(rect.Right - rect.Left);
                            float yS = (float)(rect.Bottom - yScreen) / (float)(rect.Right - rect.Left);                            

                            // player 1
                            j = j | GetTouchedAreas(xS - 0.25f, yS - 0.25f);
                            // player 2 - not tested... might use a 2nd screen?
                            //j = GetTouchedAreas(xS - 0.75f, yS - 0.25f);
                            //Console.WriteLine(j);
                            //toWrite = BitConverter.GetBytes(j);
                            //WriteProcessMemory((int)processHandle, val + 56, toWrite, structBuf.Length, ref bytesRead);
                        }

                        toWrite = BitConverter.GetBytes(j);
                        if (!process.HasExited)
                        {
                            try
                            {
                                WriteProcessMemory((int)processHandle, val + 52, toWrite, structBuf.Length, ref bytesRead);
                            }
                            catch
                            {
                                m_currentState = MaiMaiState.Error;
                                break;
                            }
                        }
                        else
                        {
                            m_currentState = MaiMaiState.Error;
                            break;
                        }

                        Thread.Sleep(16);
                    }
                }
                m_currentState = MaiMaiState.Error;
                Thread.Sleep(2000);
            }
        }

    }
}
