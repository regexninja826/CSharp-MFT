using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp_mft_4_28_2014
{
    class timestamps
    {
        public timestamps() {}
        
        static public Int32 UnixTimeStampUTC()
        {
            Int32 unixTimeStamp;
            DateTime currentTime = DateTime.Now;
            DateTime zuluTime = currentTime.ToUniversalTime();
            DateTime unixEpoch = new DateTime(1970, 1, 1);
            unixTimeStamp = (Int32)(zuluTime.Subtract(unixEpoch)).TotalSeconds;
            return unixTimeStamp;

        }//end method unixtimestamp
    }//end class timestamps
}//end namespace
