using System.Linq;
using Newtonsoft.Json;

namespace gvaduha.JsonWithBlobs
{
    /// <summary>
    /// Json with blob fields loader
    /// </summary>
    public static class JsonBlobConvert
    {
        /// <summary>
        /// Deserialize json objects and loads fields with BlobAttribute from blob fs
        /// </summary>
        /// <typeparam name="T">Type of object to load</typeparam>
        /// <param name="json">String json object representation in Newtonsoft format</param>
        /// <param name="blobfs">Byte array in the form of AdHocBlobFS</param>
        /// <returns>Deserialized objects</returns>
        public static T DeserializeObject<T>(string json, byte[] blobFile)
        {
            var obj = JsonConvert.DeserializeObject<T>(json);

            var blobfs = new AdHocBlobFS(blobFile);

            var blobFileds = obj.GetType().GetProperties()
                .Select(x => (p:x, atrrs:x.GetCustomAttributes(typeof(BlobFieldAttribute), true)))
                .Select(x => (p:x.p, a:x.atrrs.FirstOrDefault()))
                .Where(x => x.a != null);

            foreach(var bf in blobFileds)
            {
                var bfa = (BlobFieldAttribute)bf.a;
                bf.p.SetValue(obj, blobfs.ReadFile(bfa.FileNumber));
            }

            return obj;
        }

        /// <summary>
        /// Serialize object. Important than object blob fields are nullified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Object to serialize. NOTE: All blob field will be nullified!</param>
        /// <returns>json: all fields exept marked BlobFieldAttribute, blobfs: AdHocBlockFS with blob fields</returns>
        public static (string json, byte[] blobfs) SerializeObject<T>(T obj)
        {
            var blobfs = new AdHocBlobFS();

            var blobFileds = obj.GetType().GetProperties()
                .Select(x => (p: x, atrrs: x.GetCustomAttributes(typeof(BlobFieldAttribute), true)))
                .Select(x => (p: x.p, a: x.atrrs.FirstOrDefault()))
                .Where(x => x.a != null)
                .Select(x => (p: x.p, fn: ((BlobFieldAttribute)x.a).FileNumber))
                .OrderBy(x => x.fn);

            foreach (var bf in blobFileds)
            {
                var file = (byte[])bf.p.GetValue(obj);
                blobfs.WriteFile(file, bf.fn);
                bf.p.SetValue(obj, null);
            }

            // Important to serialize here when fields are cleared
            string json = JsonConvert.SerializeObject(obj);

            return (json, blobfs.GetCurrentBlobFile());
        }
    }
}
