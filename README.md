# Json with blobs
  
[![License](http://img.shields.io/badge/license-mit-blue.svg?style=flat-square)](https://raw.githubusercontent.com/json-iterator/go/master/LICENSE)
[![Build Status](https://travis-ci.org/gvaduha/json-with-blobs.svg?branch=master)](https://travis-ci.org/gvaduha/json-with-blobs)

## Brief
When you have an object with byte[] fields that can be large to base64 it and ugly to mix in Json you think about to store them separately. This helps to make it possible.

## Examples
```
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
```
```
    var obj = JsonBlobConvert
        .DeserializeObject<TwoBlobsObj>(TestSamples.JsonSample1, TestSamples.TwoFileFS);

    var (json, blobfs) = JsonBlobConvert.SerializeObject<TwoBlobsObj>(obj);
```
