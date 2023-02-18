﻿using System.Runtime.InteropServices;
using System.Text;
using NETBinaryCookie.Types;
using NETBinaryCookie.Types.Meta;

namespace NETBinaryCookie;

internal static class BinaryCookieParser
{
    private const string FileSignature = "cook";
    private const uint FileSignatureHex = 0x_63_6F_6F_6B;

    private const uint PageMetaStartMarker = 0x_00_00_01_00;
    private const uint PageMetaEndMarker = 0x_00_00_00_00;

    private const uint CookieMetaEndMarker = 0x_00_00_00_00;

    internal static BinaryCookieJarMeta ImportFromFile(string fileName)
    {
        if (!File.Exists(fileName))
        {
            throw new FileNotFoundException("The binarycookie file does not exist");
        }
        
        using var stream = File.Open(fileName, FileMode.Open);
        using var reader = new BinaryReader(stream, Encoding.UTF8, false);

        var meta = new BinaryCookieJarMeta
        {
            JarDetails = BinaryCookieTranscoder.BytesToStruct<FileMeta>(reader.ReadBytes(Marshal.SizeOf<FileMeta>()))
        };

        // Start by checking the signature and getting the number of pages.
        if (meta.JarDetails.signature != FileSignatureHex)
        {
            throw new BinaryCookieException("Invalid binarycookie signature");
        }
        
        // Get each page's size and create the associated PageMeta object to use later.
        foreach (var _ in Enumerable.Range(0, (int)meta.JarDetails.numPages))
        {
            meta.JarPages.Add(new() { Size = reader.ReadBytes(4).ConvertBigEndianBytesToUInt32() });
        }
        
        // Get the position at the cursor right after it's done reading page offsets.
        //   This should always mark the first page (w/ header 0x00000100 BE)
        //var lastPageCursorPosition = stream.Position;
        foreach (var pageMeta in meta.JarPages)
        {
            // The first page header (and beyond) are always marked in this position as the start location.
            //   This is verified below by checking the page header signature indicates a page-start.
            pageMeta.StartPosition = (uint)stream.Position;

            pageMeta.PageProperties =
                BinaryCookieTranscoder.BytesToStruct<PageMeta>(reader.ReadBytes(Marshal.SizeOf<PageMeta>()));
    
            // Check the page header signature.
            if (pageMeta.PageProperties.pageStart != PageMetaStartMarker)
            {
                throw new BinaryCookieException("Invalid page header signature");
            }
            
            // Parse the cookie offsets.
            foreach (var _ in Enumerable.Range(0, (int)pageMeta.PageProperties.numCookies))
            {
                pageMeta.PageCookies.Add(new() { Size = reader.ReadUInt32() });
            }

            // At this point, the reader cursor should be at the pageEnd marker, after which the cookies begin.
            if (reader.ReadUInt32() != PageMetaEndMarker)
            {
                throw new BinaryCookieException("Missing or malformed page end marker");
            }
            
            // -----
            // GET THE COOKIES IN THIS PAGE.
            foreach (var pageCookie in pageMeta.PageCookies)
            {
                // Mark the cookie starting position.
                pageCookie.StartPosition = (uint)stream.Position;

                pageCookie.CookieProperties =
                    BinaryCookieTranscoder.BytesToStruct<BinaryCookieMeta>(
                        reader.ReadBytes(Marshal.SizeOf<BinaryCookieMeta>()));

                if (pageCookie.CookieProperties.endHeader != CookieMetaEndMarker)
                {
                    throw new BinaryCookieException("Missing or malformed cookie properties");
                }
                
                // Configure a quick string reader that just scrolls until a null character.
                Func<BinaryReader, string?> ReadStringToEnd = rdr =>
                {
                    var readByte = rdr.ReadByte();
                    var ret = new List<byte> { readByte };

                    while (readByte != 0x00)
                    {
                        readByte = rdr.ReadByte();
                        ret.Add(readByte);
                    }

                    return ret.Count < 1 ? null : Encoding.UTF8.GetString(ret.ToArray());
                };

                Func<BinaryReader, DateTime> MarshalToLongDateTime = rdr =>
                {
                    // The date longs here are BigEndian, so the Marshal should NOT reverse byte order.
                    var rawData = rdr.ReadBytes(8).ToArray();
                    var gcHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
                    
                    try
                    {
                        var rawDataPtr = gcHandle.AddrOfPinnedObject();
                        
                        var date = DateTime.FromBinary(978307200 + Marshal.ReadInt64(rawDataPtr));
                        return date;
                    }
                    finally
                    {
                        gcHandle.Free();
                    }
                };
                
                // Set all cookie properties here as they're read.
                pageCookie.Cookie = new()
                {
                    Expiration = MarshalToLongDateTime(reader),
                    Creation = MarshalToLongDateTime(reader),
                    Comment = pageCookie.CookieProperties.commentOffset > 0 ? ReadStringToEnd(reader) : null,
                    Domain = ReadStringToEnd(reader)!,
                    Name = ReadStringToEnd(reader)!,
                    Path = ReadStringToEnd(reader)!,
                    Value = ReadStringToEnd(reader)!
                };

                // Go to the end of the cookie in preparation to get the next one.
                stream.Seek(pageCookie.StartPosition + pageCookie.CookieProperties.cookieSize, SeekOrigin.Begin);
            }
            // -----

            // The next page should be located at the start of this page plus its size/offset/length.
            stream.Seek(pageMeta.StartPosition + pageMeta.Size, SeekOrigin.Begin);
        }
        
        return meta;
    }

    internal static void ExportToFile(BinaryCookieJarMeta meta, string fileName)
    {
        // TODO
    }
}