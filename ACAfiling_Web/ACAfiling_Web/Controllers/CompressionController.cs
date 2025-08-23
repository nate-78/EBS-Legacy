using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.IO.Compression;

namespace ACAfiling_Web.Controllers
{
    public class CompressionController : Controller
    {
        // Compress file
        // got this function from http://toreaurstad.blogspot.com/2014/01/compressing-byte-array-in-c-with.html
        public static byte[] Compress(byte[] inputData) {
            if (inputData == null)
                throw new ArgumentNullException("inputData must be non-null");

            using (var compressIntoMs = new MemoryStream()) {
                using (var gzs = new BufferedStream(new GZipStream(compressIntoMs,
                 CompressionMode.Compress), inputData.Length)) { // original method used a BUFFER_SIZE variable, set before this function
                    gzs.Write(inputData, 0, inputData.Length);
                }
                return compressIntoMs.ToArray();
            }
        }
    }
}