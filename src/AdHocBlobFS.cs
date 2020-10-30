using System;
using System.Collections.Generic;
using System.Linq;

namespace gvaduha.JsonWithBlobs
{
    /// <summary>
    /// Ad-hoc FS from blob record
    /// Designed for supply primarily read only files to big to be placed in json.
    /// Contains up to 255 files. Works on x86, x64 archs (little endian)
    /// 
    /// Structure:
    /// 3byte |1byte   |8byte|4byte              |
    /// 24b   |8b      |64b  |32b    dscr#n:96b  |file#1     file#n
    /// [sign][# files][offs][size]..[    ][    ][ .... ] .. [ .... ]
    /// </summary>
    internal class AdHocBlobFS
    {
        const UInt32 Signature = 0x56410100;
        const UInt32 ReversedSignature = 0x00014156;

        public static byte[] EmptyFS = BitConverter.GetBytes(Signature);
        public const Int32 NewFile = -1; 

        private byte[] _rofs;
        private byte _rofsFileCnt = 0;
        private Dictionary<Int32, byte[]> _newFiles = new Dictionary<Int32, byte[]>();
        private byte _newFileCnt = 0; // *new* files not overwritten

        public AdHocBlobFS()
        {
            _rofs = EmptyFS;
        }

        /// <summary>
        /// Construct ad-hoc fs with readonly fs file
        /// </summary>
        /// <param name="rofs">Read only fs file</param>
        public unsafe AdHocBlobFS(byte[] rofs)
        {
            if (rofs == null)
                throw new ArgumentNullException(nameof(rofs));
            if (rofs.Length < 4) // sig+len(4) == EmptyFS
                throw new ArgumentOutOfRangeException("FS file should not be less than 17 bytes");

            fixed (byte* pfs = rofs)
            {
                UInt32* header = (UInt32*)pfs;
                if ((*header & 0x00FFFFFF) != ReversedSignature)
                    throw new ApplicationException("Missing signature of adhoc fs file");

                _rofsFileCnt = (byte)(*header >> 24);
            }

            _rofs = rofs;
        }

        /// <summary>
        /// Returns distinct file count in fs
        /// </summary>
        public byte GetFileCount()
        {
            return (byte)(_rofsFileCnt + _newFileCnt);
        }

        /// <summary>
        /// Reads file from ad-hoc fs
        /// </summary>
        /// <param name="num">0 based number of the file</param>
        /// <returns>file stream</returns>
        public byte[] ReadFile(Int32 num)
        {
            if (num < 0)
                throw new ArgumentOutOfRangeException(nameof(num));

            if (_newFiles.ContainsKey(num))
                return _newFiles[num];
            else
            {
                if (_rofsFileCnt == 0 || _rofsFileCnt <= num)
                    throw new ApplicationException($"Requested file #{num} from ROFS, but there is only {_rofsFileCnt} files");

                return ROFSReadFile(num);
            }
        }

        protected unsafe byte[] ROFSReadFile(Int32 num)
        {
            fixed (byte* pfs = _rofs)
            {
                byte* descrArea = pfs + 4;
                byte* fileArea = descrArea + _rofsFileCnt * 12;

                UInt64 fileOffs = *(UInt64*)(descrArea + num * 12);
                Int32 fileSize = *(Int32*)(descrArea + num * 12 + 8);

                byte[] file = new byte[fileSize];
                fixed (byte* dstfile = file)
                {
                    Buffer.MemoryCopy(fileArea + fileOffs, dstfile, fileSize, fileSize);
                }

                return file;
            }
        }

        /// <summary>
        /// Writes new file or "rewrite" file to fs
        /// </summary>
        /// <param name="file"></param>
        /// <param name="num">File number for rewrite or -1 for new file</param>
        /// <returns>file number</returns>
        public Int32 WriteFile(byte[] file, Int32 num = NewFile)
        {
            if (file == null)
                throw new ArgumentNullException(nameof(file));

            var lastNum = _rofsFileCnt + _newFileCnt;

            if (lastNum == 255)
                throw new ApplicationException("FS is full");

            if (num != NewFile && num > lastNum)
                throw new ApplicationException($"Requsted to write file #{num} while fs store {_rofsFileCnt} ro and {_newFileCnt} new files");
            else if (num == NewFile)
            {
                num = lastNum;
            }

            if (num >= _rofsFileCnt)
                _newFileCnt++;

            _newFiles[num] = file;

            return num;
        }

        /// <summary>
        /// Return file size
        /// </summary>
        /// <param name="num">File number</param>
        /// <returns>File size</returns>
        public Int32 FileSize(Int32 num)
        {
            if (_newFiles.ContainsKey(num))
                return _newFiles[num].Length;
            else
                return ROFSFileSize(num);
        }

        protected unsafe Int32 ROFSFileSize(Int32 num)
        {
            fixed (byte* pfs = _rofs)
            {
                byte* fileSize = pfs + 4 + num * 12 + 8;
                return *(Int32*)fileSize;
            }
        }

        /// <summary>
        /// Return current state of fs blob file
        /// </summary>
        /// <returns>Combined array of all files in fs</returns>
        public unsafe byte[] GetCurrentBlobFile()
        {
            byte totalFileCnt = (byte)(_newFileCnt + _rofsFileCnt);
            var fileSizes = Enumerable.Range(0, totalFileCnt).Select(x => (num:x, size:FileSize(x)));
            Int32 totalFilesSize = fileSizes.Select(x => x.size).Sum();
            var fs = new byte[4 + 12 * totalFileCnt + totalFilesSize];

            fixed(byte* pfs = fs)
            {
                UInt32 header = (Signature & 0xFFFFFF00) | totalFileCnt;
                UInt32 hdr = BitConverter.ToUInt32(BitConverter.GetBytes(header).Reverse().ToArray(), 0);
                Buffer.MemoryCopy(&hdr, pfs, 4, 4);
                byte* curDescrOffs = pfs + 4;
                byte* fileArea = pfs + 4 + totalFileCnt * 12;
                UInt64 curFileOffs = 0;

                foreach (var (num, size) in fileSizes)
                {
                    //UInt64 fo = BitConverter.ToUInt64(BitConverter.GetBytes(curFileOffs).Reverse().ToArray());
                    //Buffer.MemoryCopy(&fo, curDescrOffs, 8, 8);
                    Buffer.MemoryCopy(&curFileOffs, curDescrOffs, 8, 8);
                    //Int32 sz = BitConverter.ToInt32(BitConverter.GetBytes((Int32)size).Reverse().ToArray());
                    Int32 sz = size;
                    Buffer.MemoryCopy(&sz, curDescrOffs+8, 4, 4);
                    fixed (byte* srcfile = ReadFile(num))
                    {
                        Buffer.MemoryCopy(srcfile, fileArea + curFileOffs, size, size);
                    }
                    curDescrOffs += 12;
                    curFileOffs += (UInt64)size;
                }
            }

            return fs;
        }
    }
}
