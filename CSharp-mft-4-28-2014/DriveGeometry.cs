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

     

    class DriveGeometry
    {
        const int IOCTL_DISK_GET_DRIVE_GEOMETRY = unchecked((int)0x00070000);
        
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            int flags,
            IntPtr template);
        
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            [Out] IntPtr lpOutBuffer,
            uint nOutBufferSize,
            ref uint lpBytesReturned,
            IntPtr lpOverlapped);

        [StructLayout(LayoutKind.Sequential)]
        internal struct DiskGeometry
        {
            public long Cylinders;
            public int MediaType;
            public int TracksPerCylinder;
            public int SectorsPerTrack;
            public int BytesPerSector;
        }
                
        
        public DriveGeometry() { }


        public long GetBytesPerSector(string strdrive,SafeFileHandle drive) 
        {

            drive = CreateFile(fileName: strdrive,
                          fileAccess: FileAccess.Read,
                          fileShare: FileShare.Write | FileShare.Read | FileShare.Delete,
                          securityAttributes: IntPtr.Zero,
                          creationDisposition: FileMode.Open,
                          flags: 4, 
                          template: IntPtr.Zero);

            int geometrySize = Marshal.SizeOf(typeof(DiskGeometry));
            IntPtr geometryBlob = Marshal.AllocHGlobal(geometrySize);

            uint numBytesRead = 0;

            bool deviceio = DeviceIoControl(drive, IOCTL_DISK_GET_DRIVE_GEOMETRY, IntPtr.Zero, 0, geometryBlob, (uint)geometrySize, ref numBytesRead, IntPtr.Zero);

            DiskGeometry geometry = (DiskGeometry)Marshal.PtrToStructure(geometryBlob, typeof(DiskGeometry));
            Marshal.FreeHGlobal(geometryBlob);

            return (long)geometry.BytesPerSector;

        }


    }//end class
}//end namespace 
