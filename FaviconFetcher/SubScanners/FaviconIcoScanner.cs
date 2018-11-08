﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FaviconFetcher.SubScanners
{
    class FaviconIcoScanner : SubScanner
    {

        public FaviconIcoScanner(ISource source, Uri uri) : base(source, uri)
        {
        }

        public override void Start()
        {
            Results.Add(new ScanResult
            {
                Location = new Uri(TargetUri, "/favicon.ico"),
                ExpectedSize = new Size(16, 16)
            });
        }

    }
}
