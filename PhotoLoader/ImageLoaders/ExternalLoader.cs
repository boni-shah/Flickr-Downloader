﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PhotoLoader.ImageLoaders
{
    class ExternalLoader : ILoader
    {
        #region ILoader Members

        public Stream Load(string source)
        {
            var webClient = new WebClient();
            byte[] html = webClient.DownloadData(source);

            if (html == null || html.Count() == 0) return null;

            return new MemoryStream(html);
        }

        #endregion
    }
}