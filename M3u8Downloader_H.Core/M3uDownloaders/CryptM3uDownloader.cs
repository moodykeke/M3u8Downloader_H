﻿using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using M3u8Downloader_H.Common.M3u8Infos;
using M3u8Downloader_H.Common.Extensions;
using System;
using M3u8Downloader_H.Core.Utils.Extensions;

namespace M3u8Downloader_H.Core.M3uDownloaders
{
    internal class CryptM3uDownloader : M3u8Downloader
    {
        private readonly M3UFileInfo m3UFileInfo;
        public CryptM3uDownloader(M3UFileInfo m3UFileInfo) : base()
        {
            this.m3UFileInfo = m3UFileInfo;
        }

        public override async ValueTask Initialization(CancellationToken cancellationToken)
        {
            if (m3UFileInfo.Key is null)
                throw new InvalidDataException("没有可用的密钥信息");

            if(m3UFileInfo.Key.Uri != null && m3UFileInfo.Key.BKey == null)
            {
                try
                {
                    using var tokenSource = cancellationToken.CancelTimeOut(TimeOut);                    
                    byte[] data = m3UFileInfo.Key.Uri.IsFile
                        ? await File.ReadAllBytesAsync(m3UFileInfo.Key.Uri.OriginalString, tokenSource.Token)
                        : await HttpClient.GetByteArrayAsync(m3UFileInfo.Key.Uri, Headers, tokenSource.Token);

                    m3UFileInfo.Key.BKey = data.TryParseKey(m3UFileInfo.Key.Method);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    throw new HttpRequestException("密钥获取失败");
                }
                catch (HttpRequestException e) when(e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    throw new HttpRequestException("获取密钥失败，没有找到任何数据",e.InnerException,e.StatusCode);
                }
            }else
            {
                m3UFileInfo.Key.BKey = m3UFileInfo.Key.BKey != null
                    ? m3UFileInfo.Key.BKey.TryParseKey(m3UFileInfo.Key.Method)
                    : throw new InvalidDataException("密钥为空");
            }
        }

        protected override Stream DownloadAfter(Stream stream, string contentType, CancellationToken cancellationToken)
        {
            Stream Decryptstream = stream.AesDecrypt(m3UFileInfo.Key.BKey, m3UFileInfo.Key.IV);
            return base.DownloadAfter(Decryptstream, contentType, cancellationToken);
        }
    }
}
