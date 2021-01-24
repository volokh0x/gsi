using System;

namespace gsi
{
    partial class X
    { 
        static string GetOctal6(uint x)
        {
            string fmt = "000000";
            string oct=Convert.ToString(x,8); 
            return Convert.ToInt32(oct).ToString(fmt);
        }
        static uint GetTimeStamp(DateTime dt)
        {
            var diff = dt.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToUInt32(Math.Floor(diff.TotalSeconds));
        }
    }
}
