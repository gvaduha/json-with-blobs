using System.Linq;
using gvaduha.JsonWithBlobs;
using Xunit;

namespace json_with_blobs_test
{
    public class JsonBlobTest
    {
        [Fact]
        public void ReadJsonBlobObject()
        {
            var obj = JsonBlobConvert
                .DeserializeObject<TwoBlobsObj>(TestSamples.JsonSample1, TestSamples.TwoFileFS);

            Assert.Equal("Sample #1", obj.Name);
            Assert.Equal(5, obj.File0.Length);
            Assert.Equal(8, obj.File1.Length);
            Assert.Equal(0x33, obj.File0[0]);
            Assert.Equal(0x88, obj.File1[0]);
        }

        [Fact]
        public void WriteJsonBlobObject()
        {
            var obj = new ThreeBlobsObj
            {
                Name = "Write Sample",
                File0 = TestSamples.FileLen6Sig11,
                File1 = TestSamples.FileLen8Sig88,
                File2 = TestSamples.FileLen3Sig22
            };

            var (json, blobfs) = JsonBlobConvert.SerializeObject(obj);

            Assert.Contains("Write Sample", json);
            Assert.True(blobfs.SequenceEqual(TestSamples.ThreeFileFS));
        }
    }
}
