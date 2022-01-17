using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ATA_Tools
{
    public unsafe class IOPortAccess
    {
        [DllImport("kernel32.dll")]
        private extern static IntPtr LoadLibrary(String DllName);

        [DllImport("kernel32.dll")]
        private extern static IntPtr GetProcAddress(IntPtr hModule, String ProcName);

        [DllImport("kernel32")]
        private extern static bool FreeLibrary(IntPtr hModule);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool InitializeWinIo();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetPhysLong(IntPtr PhysAddr, UInt32* pPhysVal);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SetPhysLong(uint PhysAddr, UInt32 PhysVal);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool GetPortVal(uint wPortAddr, UInt32* pdwPortVal, byte bSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SetPortVal(uint wPortAddr, UInt32 pdwPortVal, byte bSize);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool ShutdownWinIo();

        InitializeWinIo InitWinIo = null;
        GetPortVal GetPVal = null;
        SetPortVal SetPVal = null;
        ShutdownWinIo ShtWinIo = null;
        IntPtr hMod;

        public IOPortAccess()
        {
            hMod = LoadLibrary("WinIo.dll");

            if (hMod == IntPtr.Zero)
            {
                string msg = "Can't find WinIo.dll. Make sure the WinIo.dll library files are located in the same directory as your executable file.";
                Console.WriteLine(msg);
                // throw new Exception(msg);
            }

            IntPtr pFunc = GetProcAddress(hMod, "InitializeWinIo");
            if (pFunc != IntPtr.Zero)
            {
                InitWinIo = (InitializeWinIo)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(InitializeWinIo));
                bool result = InitWinIo();

                if (!result)
                {
                    FreeLibrary(hMod);
                    hMod = IntPtr.Zero;

                    string msg = "Error returned from InitializeWinIo. Make sure you are running with administrative privilages are that WinIo library files are located in the same directory as your executable file.";
                    Console.WriteLine(msg);
                    // throw new Exception(msg);
                }
            }
        }

        ~IOPortAccess()
        {
            if (hMod != IntPtr.Zero)
            {
                IntPtr pFunc = GetProcAddress(hMod, "ShutdownWinIo");
                if (pFunc != IntPtr.Zero)
                {
                    this.GetPVal = null;
                    this.SetPVal = null;

                    ShtWinIo = (ShutdownWinIo)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(ShutdownWinIo));
                    ShtWinIo();

                    FreeLibrary(hMod);
                    hMod = IntPtr.Zero;
                }
            }
        }

        public void Close()
        {
            if (hMod != IntPtr.Zero)
            {
                IntPtr pFunc = GetProcAddress(hMod, "ShutdownWinIo");
                if (pFunc != IntPtr.Zero)
                {
                    this.GetPVal = null;
                    this.SetPVal = null;

                    ShtWinIo = (ShutdownWinIo)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(ShutdownWinIo));
                    ShtWinIo();

                    FreeLibrary(hMod);
                    hMod = IntPtr.Zero;
                }
            }
        }

        public bool GetPortValue(UInt16 portAddress, UInt32* dddd, byte bt)
        {
            //UInt32 portValue = 0;

            if (hMod != IntPtr.Zero)
            {
                if (this.GetPVal == null)
                {
                    IntPtr pFunc = GetProcAddress(hMod, "GetPortVal");
                    if (pFunc != IntPtr.Zero)
                    {
                        this.GetPVal = (GetPortVal)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(GetPortVal));
                    }
                    else
                    {
                        return false;
                    }
                }

                if (!this.GetPVal(portAddress, dddd, bt))
                {
                    return false;
                }
            }

            return true;
        }

        public bool SetPortValue(UInt16 portAddress, UInt32 portValue, byte bt)
        {
            if (hMod != IntPtr.Zero)
            {
                if (this.SetPVal == null)
                {
                    IntPtr pFunc = GetProcAddress(this.hMod, "SetPortVal");
                    if (pFunc != IntPtr.Zero)
                    {
                        this.SetPVal = (SetPortVal)Marshal.GetDelegateForFunctionPointer(pFunc, typeof(SetPortVal));
                    }
                    else
                    {
                        return false;
                    }
                }

                bool result = this.SetPVal(portAddress, portValue, bt);
                if (!result)
                {
                    return false;
                }
            }
            return true;
        }

        public bool GetBit(UInt32 data, int index)
        {
            if ((index < 0) || (index > 31))
            {
                string msg = "Bit index must be between 0 and 31.";
                Console.WriteLine(msg);
                // throw new ArgumentOutOfRangeException("index", index, msg);
            }
            UInt32 mask = (UInt32)1 << index;
            UInt32 result = data & mask;
            return (result > 0);
        }

        public UInt32 SetBit(UInt32 data, int index, bool value)
        {
            if ((index < 0) || (index > 31))
            {
                string msg = "Bit index must be between 0 and 31.";
                Console.WriteLine(msg);
                // throw new ArgumentOutOfRangeException("index", index, msg);
            }
            UInt32 result;
            UInt32 mask = (UInt32)1 << index;

            if (value)
            {
                result = data | mask;
            }
            else
            {
                result = data & ~mask;
            }
            return result;
        }
    }
}
