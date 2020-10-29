using System;
using System.Linq;
using gvaduha.JsonWithBlobs;
using Xunit;

namespace json_with_blobs_test
{
    public class AdHocBlobFsTest
    {
        public AdHocBlobFsTest()
        {
        }

        [Fact]
        public void CreateAdHocFsException()
        {
            Assert.Throws<ArgumentNullException>(() => new AdHocBlobFS(null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new AdHocBlobFS(new byte[3]));
            Assert.Throws<ApplicationException>(() => new AdHocBlobFS(new byte[5]));
        }

        [Fact]
        public void ReadFilesFromROFS()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);

            var file0 = sut.ReadFile(0);
            Assert.Equal(5, file0.Length);
            Assert.Equal(0x33, file0[0]);
            Assert.Equal(0x77, file0[4]);

            var file1 = sut.ReadFile(1);
            Assert.Equal(8, file1.Length);
            Assert.Equal(0x88, file1[0]);
            Assert.Equal(0xFF, file1[7]);
        }

        [Fact]
        public void ReadBadFile()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);

            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ReadFile(AdHocBlobFS.NewFile));

            var ex = Assert.Throws<ApplicationException>(() => sut.ReadFile(2));
            Assert.StartsWith("Requested file", ex.Message);
        }

        [Fact]
        public void WriteFileInsteadExisting()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);

            sut.WriteFile(TestSamples.FileLen3Sig22, 1);

            var file1 = sut.ReadFile(1);
            Assert.Equal(3, file1.Length);
            Assert.Equal(0x22, file1[0]);
            Assert.Equal(0x22, file1[2]);

            var file0 = sut.ReadFile(0);
            Assert.Equal(5, file0.Length);
            Assert.Equal(0x33, file0[0]);
            Assert.Equal(0x77, file0[4]);

            sut.WriteFile(TestSamples.FileLen6Sig11, 0);

            file0 = sut.ReadFile(0);
            Assert.Equal(6, file0.Length);
            Assert.Equal(0x11, file0[0]);
            Assert.Equal(0x11, file0[5]);
        }

        [Fact]
        public void WriteNewFile()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);

            var newFile = sut.WriteFile(TestSamples.FileLen3Sig22);
            Assert.Equal(2, newFile);

            var file0 = sut.ReadFile(0);
            Assert.Equal(5, file0.Length);
            Assert.Equal(0x33, file0[0]);
            Assert.Equal(0x77, file0[4]);

            var file2 = sut.ReadFile(newFile);
            Assert.Equal(3, file2.Length);
            Assert.Equal(0x22, file2[0]);
            Assert.Equal(0x22, file2[2]);

            newFile = sut.WriteFile(TestSamples.FileLen6Sig11, 3);
            Assert.Equal(3, newFile);
            var file3 = sut.ReadFile(newFile);
            Assert.Equal(6, file3.Length);
        }

        [Fact]
        public void WriteBadFile()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);
            Assert.Throws<ArgumentNullException>(() => sut.WriteFile(null));

            Assert.Throws<ApplicationException>(() => sut.WriteFile(TestSamples.FileLen3Sig22, 3));
        }

        [Fact]
        public void WriteAndGetNewAdhocFs()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);

            var newFile = sut.WriteFile(TestSamples.FileLen3Sig22);
            Assert.Equal(2, newFile);

            newFile = sut.WriteFile(TestSamples.FileLen6Sig11, 0);
            Assert.Equal(0, newFile);

            var blobfs = sut.GetCurrentBlobFile();

            sut = new AdHocBlobFS(blobfs);

            var file0 = sut.ReadFile(0);
            Assert.Equal(6, file0.Length);
            Assert.Equal(0x11, file0[0]);
            Assert.Equal(0x11, file0[5]);

            var file1 = sut.ReadFile(1);
            Assert.Equal(8, file1.Length);
            Assert.Equal(0x88, file1[0]);
            Assert.Equal(0xFF, file1[7]);

            var file2 = sut.ReadFile(2);
            Assert.Equal(3, file2.Length);
            Assert.Equal(0x22, file2[0]);
            Assert.Equal(0x22, file2[2]);

            Assert.True(blobfs.SequenceEqual(TestSamples.ThreeFileFS));
        }

        [Fact]
        public void FileCount()
        {
            var sut = new AdHocBlobFS(TestSamples.TwoFileFS);
            Assert.Equal(2, sut.GetFileCount());

            for(var i=0; i<255; ++i)
            {
                var filen = sut.WriteFile(TestSamples.FileLen6Sig11, i);
                Assert.Equal(i, filen);

                if (i<2)
                    Assert.Equal(2, sut.GetFileCount());
                else
                    Assert.Equal(i+1, sut.GetFileCount());
            }

            Assert.Throws<ApplicationException>(() => sut.WriteFile(TestSamples.FileLen6Sig11));
        }
    }
}
