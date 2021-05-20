# Json with blobs
  
[![License](http://img.shields.io/badge/license-mit-blue.svg?style=flat-square)](https://raw.githubusercontent.com/json-iterator/go/master/LICENSE)
[![Build Status](https://travis-ci.org/gvaduha/json-with-blobs.svg?branch=master)](https://travis-ci.org/gvaduha/json-with-blobs)
![.NET Core](https://github.com/gvaduha/json-with-blobs/workflows/.NET%20Core/badge.svg)
[![NuGet version](https://img.shields.io/nuget/v/json-with-blobs.svg?style=flat-square)](https://www.nuget.org/packages/json-with-blobs/1.0.0)
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
Deserialize
```
  var obj = JsonBlobConvert
      .DeserializeObject<TwoBlobsObj>(TestSamples.JsonSample1, TestSamples.TwoFileFS);
```
Serialize
```
  var obj = new ThreeBlobsObj
  {
    Name = "Write Sample",
    File0 = TestSamples.FileLen6Sig11,
    File1 = TestSamples.FileLen8Sig88,
    File2 = TestSamples.FileLen3Sig22
  };

  var (json, blobfs) = JsonBlobConvert.SerializeObject(obj);
```

## blobfs-tool
Basic tool for reading writing blobfs files
```
Options:
  -b | --bfs      blob fs file
  -n | --new      create new blob fs file
  -l | --list     list files
  -x | --extract  extract all files to disk
  -w | --write    write one or multiple files

Example:
	-b blobfs.blob -x
	-b newbfs.blob -n -w test1 -w test2
```
