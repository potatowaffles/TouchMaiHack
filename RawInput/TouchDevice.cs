using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RawInput_dll
{
    public class TouchDevice
    {
        public uint VendorId { get; set; }
        public uint ProductId { get; set; }
        public DeviceInfoHid DeviceInfo { get; internal set; }
        public int Width { get; internal set; }
    }
}
