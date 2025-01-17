﻿using System;
using System.Threading;
using System.Threading.Tasks;

namespace M3u8Downloader_H.Core.M3u8UriManagers
{
    internal interface IM3u8UriManager
    {
        Task<Uri> GetM3u8UriAsync(Uri uri,int timeout, CancellationToken cancellationToken);
    }
}
