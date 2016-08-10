using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Management; 

namespace CSharp_mft_4_28_2014{
    class NTFS_Methods{
    
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            int flags,
            IntPtr template);

        private int partition_type_offset = 450;
        private int ntfs_start_sector = 454;
        private int drivecount;
        private string strdrive;
        private SafeFileHandle drive;
        private FileStream driveread;
        private byte[] buf;
        private int partition_type;
        private byte[] dataheader = {0x80,0x00,0x00,0x00};
        private List<StorageClass> Storedpointers;       
                
        public NTFS_Methods() {drivecount = 0;}

        public List<StorageClass> CreateList() {
            Storedpointers = new List<StorageClass>();
            DriveGeometry geometry = new DriveGeometry();
            while (drivecount < 15){
                strdrive = "\\\\.\\Physicaldrive" + drivecount;
                drive = CreateFile(fileName: strdrive,
                         fileAccess: FileAccess.Read,
                         fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                         securityAttributes: IntPtr.Zero,
                         creationDisposition: FileMode.Open,
                         flags: 4,
                         template: IntPtr.Zero);
                if (!drive.IsInvalid){
                    int partition_itterate = 0;
                    buf = new byte[512];
                    driveread = new FileStream(drive, FileAccess.Read);
                    driveread.Read(buf, 0, 512);
                    while (partition_itterate < 49){
                       partition_type = buf[partition_type_offset+partition_itterate];
                        if (partition_type == 7){
                            StorageClass pointerCollection = new StorageClass();
                            byte[] NTFSbuf = new byte[512];
                            pointerCollection.ulNTFSRelativeSector = BitConverter.ToUInt32(buf, ntfs_start_sector + partition_itterate);
                            pointerCollection.strDrive = strdrive;
                            pointerCollection.ulNTFSAbsoluteSector = (long)geometry.GetBytesPerSector(strdrive,drive) * pointerCollection.ulNTFSRelativeSector;
                            driveread = new FileStream(drive, FileAccess.Read);
                            driveread.Seek((long)pointerCollection.ulNTFSAbsoluteSector, SeekOrigin.Begin);
                            driveread.Read(NTFSbuf, 0, 512);
                            pointerCollection.mtfRelativeSector = BitConverter.ToUInt64(NTFSbuf, 0x30);
                            pointerCollection.intBytesPerSector = BitConverter.ToInt16(NTFSbuf, 0x0B);
                            pointerCollection.intSectorsPerCluster = BitConverter.ToInt16(NTFSbuf, 0x0D);
                            pointerCollection.intBytesPerCluster = pointerCollection.intBytesPerSector * pointerCollection.intSectorsPerCluster;
                            pointerCollection.mftAbsoluteSector = ((long)pointerCollection.mtfRelativeSector * (long)pointerCollection.intBytesPerCluster)+(long)pointerCollection.ulNTFSAbsoluteSector;
                            Storedpointers.Add(pointerCollection);
                        }//end if check partition type
                        partition_itterate += 16;
                    }//end while loop to scan thepartition table
                }//end if valid drive code block
                drivecount++;
                drive.Close();
            }//end while loop physical drive search

            return Storedpointers;
        }//end method to create list

        public void ProcessMftData() 
        {
            foreach (StorageClass s in Storedpointers)
            {
                drive = CreateFile(fileName: s.strDrive,
                            fileAccess: FileAccess.Read,
                            fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                            securityAttributes: IntPtr.Zero,
                            creationDisposition: FileMode.Open,
                            flags: 4,
                            template: IntPtr.Zero);
                                
                buf = new byte[512];
                driveread = new FileStream(drive, FileAccess.Read);
                driveread.Seek(s.mftAbsoluteSector, SeekOrigin.Begin);
                driveread.Read(buf, 0, 512);
                int f = ByteSearch(buf, dataheader);
                mftDatarun(buf, f,s);   
                drive.Close();
            }
        }//end method

        private static int ByteSearch(byte[] searchIn, byte[] searchBytes, int start = 0){
            int found = -1;
            bool matched = false;
            if (searchIn.Length > 0 && searchBytes.Length > 0 && start <= (searchIn.Length - searchBytes.Length) && searchIn.Length >= searchBytes.Length){
                for (int i = start; i <= searchIn.Length - searchBytes.Length; i++){
                     if (searchIn[i] == searchBytes[0]){
                        if (searchIn.Length > 1){
                            matched = true;
                            for (int y = 1; y <= searchBytes.Length - 1; y++){
                                if (searchIn[i + y] != searchBytes[y]){
                                    matched = false;
                                    break;
                                }
                            }
                            if (matched){
                                found = i;
                                break;
                            }//end if matched
                        }
                        else{
                            found = i;
                            break; 
                        }//end else
                    }
                }
            }
            return found;
        }//end method search


        public void mftDatarun(byte[] arr1, int nextbyte, StorageClass s)
        {
            
            byte[] Size;
            byte[] Offset;
            int nibble_offset;
            int nibble_size;
            ulong datarunLegnth;
            ulong datarunOffset;
            List<Tuple<long,long>> runlist = new List<Tuple<long,long>>();
            nextbyte += 0x40;
            while ((arr1[nextbyte] != 0xFF) && (arr1[nextbyte] != 0x00)){
                nibble_size = (byte)(arr1[nextbyte] & 0x0F);
                nibble_offset = (byte)(arr1[nextbyte] & 0xF0) >> 4;
                Offset = new byte[(int)nibble_offset];
                Size = new byte[(int)nibble_size];
                Buffer.BlockCopy(arr1, nextbyte + 1, Size, 0, nibble_size);
                Buffer.BlockCopy(arr1, nextbyte + nibble_size + 1, Offset, 0, nibble_offset);
                Bitconvert convert = new Bitconvert();
                datarunLegnth = convert.ConvertLittleEndian(Size);
                datarunOffset = convert.ConvertLittleEndian(Offset);
                runlist.Add(new Tuple<long,long>((long)datarunLegnth,(long)datarunOffset));
                nextbyte += (int)nibble_offset + (int)nibble_size;
                nextbyte++;
            }//end while loop search datarun list
            s.fragments = runlist;
        }//end method

        public void writeMFT()
        {   
            long offset;
            long endofrun;
            foreach (StorageClass s in Storedpointers){
                string timestamp = timestamps.UnixTimeStampUTC().ToString().Substring(3);
                bool firsttuple = true;
                foreach(Tuple<long,long> t in s.fragments){
                    if (firsttuple){
                        offset = t.Item2 * s.intBytesPerCluster + s.ulNTFSAbsoluteSector;
                    }//end of if firsttuple
                    else{
                        offset = t.Item2 * s.intBytesPerCluster + s.mftAbsoluteSector;
                    }//end else !first tuple
                    endofrun = t.Item1 * s.intBytesPerCluster;
                    writeBytes(endofrun, offset, s.strDrive,timestamp);
                }//end foreach tuple
            }//end foreach storageclass
        }//end method writemft

        private void writeBytes(long endofrun, long offset, string filename, string tstamp){
            FileStream write;
            string path = @"C:\Testing" + filename.Substring(4) + "_" + tstamp ;
            if(!File.Exists(path)) {
                write = new FileStream(path, FileMode.OpenOrCreate);
                write.Close();
            }
            //long endofrun = pointers[0].fragments[0].Item1 * pointers[0].intBytesPerCluster;
            //long offset = (pointers[0].fragments[0].Item2 * pointers[0].intBytesPerCluster) + pointers[0].ulNTFSAbsoluteSector;
            byte[] buf = new byte[1024];
            long end = offset + endofrun;
            while(offset != end){
                drive = CreateFile(fileName: filename,
                            fileAccess: FileAccess.Read,
                            fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                            securityAttributes: IntPtr.Zero,
                            creationDisposition: FileMode.Open,
                            flags: 4,
                            template: IntPtr.Zero);
                FileStream driveread = new FileStream(drive, FileAccess.Read);
                using (write = new FileStream(path, FileMode.Append)){
                    driveread.Seek(offset, SeekOrigin.Begin);
                    driveread.Read(buf, 0, 1024);
                    write.Write(buf, 0, 1024);
                    drive.Close();
                }//end using filestream
                offset += 1024;
            }//endwhile != end
        }//end method write file
    }//end class
}//end namespace
























