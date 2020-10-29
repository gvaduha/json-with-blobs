using gvaduha.JsonWithBlobs;

namespace json_with_blobs_test
{
    class TwoBlobsObj
    {
        public string Name { get; set; }
        [BlobField(FileNumber = 0)]
        public byte[] File0 { get; set; }
        [BlobField(FileNumber = 1)]
        public byte[] File1 { get; set; }
    }

    class ThreeBlobsObj : TwoBlobsObj
    {
        [BlobField(FileNumber = 2)]
        public byte[] File2 { get; set; }
    }

    class TestSamples
    {
        public static byte[] TwoFileFS = new byte[]
                           { 0x56, 0x41, 0x01, 0x02, // 3byte sign + filesNumber
                             0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //#0 offset
                             0x05, 0x00, 0x00, 0x00,                         //#0 size
                             0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //#1 offset
                             0x08, 0x00, 0x00, 0x00,                         //#1 size
                             0x11, 0x22, //unused
                             0x33, 0x44, 0x55, 0x66, 0x77, //#0 
                             0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF //#1
                           };

        public static byte[] FileLen6Sig11 = new byte[]
            { 0x11, 0x88, 0x88, 0x88, 0x88, 0x11 };

        public static byte[] FileLen3Sig22 = new byte[]
            { 0x22, 0x88, 0x22 };

        public static byte[] FileLen8Sig88 = new byte[]
            { 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF };

        public static byte[] ThreeFileFS = new byte[]
                           { 0x56, 0x41, 0x01, 0x03, // 3byte sign + filesNumber
                             0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //#0 offset
                             0x06, 0x00, 0x00, 0x00,                         //#0 size
                             0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //#1 offset
                             0x08, 0x00, 0x00, 0x00,                         //#1 size
                             0x0E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //#2 offset
                             0x03, 0x00, 0x00, 0x00,                         //#2 size
                             0x11, 0x88, 0x88, 0x88, 0x88, 0x11, //#0 
                             0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, //#1
                             0x22, 0x88, 0x22 //#2
                           };

        public static string JsonSample1 = "{ 'Name': 'Sample #1' }";
    }
}
