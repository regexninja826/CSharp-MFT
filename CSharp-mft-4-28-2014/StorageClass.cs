using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharp_mft_4_28_2014
{
    class StorageClass
    {
        public string strDrive;
        public long ulNTFSRelativeSector;
        public int intBytesPerSector;
        public int intSectorsPerCluster;
        public int intBytesPerCluster;
        public long ulNTFSAbsoluteSector;
        public UInt64 mtfRelativeSector;
        public long mftAbsoluteSector;
        public List<Tuple<long,long>> fragments;
        
        public StorageClass() { }

    }
}
