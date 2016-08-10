using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Management; 



namespace CSharp_mft_4_28_2014
{
    class Program
    {
              

        static void Main(string[] args)
        {

            NTFS_Methods ntfs = new NTFS_Methods();
            List<StorageClass> pointers = new List<StorageClass>();
            pointers = ntfs.CreateList();
            
            ntfs.ProcessMftData();
            ntfs.writeMFT();
 
            Console.ReadKey();
            //Console.ReadKey();
            
          }//end main
    }//end class
}//endnamespace

