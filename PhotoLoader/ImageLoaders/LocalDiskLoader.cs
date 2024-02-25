using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace PhotoLoader.ImageLoaders
{
    internal class LocalDiskLoader: ILoader
    {
        // Original
        //public Stream Load(string source)
        //{
        //    //Thread.Sleep(1000);
        //    return File.OpenRead(source);
        //}

        public Stream Load(string source)
        {
            return new MemoryStream(File.ReadAllBytes(source));
        }
    }
}
