using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp_mft_4_28_2014{
    class Bitconvert{
        
        public Bitconvert() {}
 
        public ulong ConvertLittleEndian(byte[] array){
            
            int pos = 0;
            ulong result = 0;
            foreach (byte by in array){

                result |= (ulong)(by << pos);
                pos += 8;
            }

            return result;
        }//end function custom bitconvert
    }//end class
}//end namespace
