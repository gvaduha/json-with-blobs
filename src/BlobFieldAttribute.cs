using System;
using System.Collections.Generic;
using System.Text;

namespace gvaduha.JsonWithBlobs
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class BlobFieldAttribute : Attribute
    {
        public byte FileNumber { get; set; }
    }
}
