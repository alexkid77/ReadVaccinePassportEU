using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using PeterO.Cbor;
using System;
using System.IO;
using System.Text;

namespace ReadVaccinePassportEU
{

    class Program
    {
        static void Main(string[] args)
        {
            //put your HC1 data
            string qrDataRaw = "HC1:....";

            if (qrDataRaw.StartsWith("HC1"))
            {
                qrDataRaw = qrDataRaw.Substring(3);
                if (qrDataRaw.StartsWith(":"))
                    qrDataRaw = qrDataRaw.Substring(1);
                else
                {
                    Console.WriteLine("No valid header");
                    return;
                }

            }
            else
            {
                Console.WriteLine("No valid header");
                return;
            }
            
            string qrDataDecoded = qrDataRaw.FromBase45();

            System.Text.Encoding iso_8859_1 = System.Text.Encoding.GetEncoding("iso-8859-1");
            byte[] bytes = iso_8859_1.GetBytes(qrDataDecoded);

            Stream unzippedStream = Decompress(bytes);
            using (var outputStream = new MemoryStream())
            {
                unzippedStream.CopyTo(outputStream);
                bytes = outputStream.ToArray();
            }


            var cbor = CBORObject.DecodeFromBytes(bytes);
            var cborItem = cbor[2];

            try
            {
                if (cborItem.Type == CBORType.ByteString)
                {
                    var valueBytes = cborItem.GetByteString();
                    var payload = CBORObject.DecodeFromBytes(valueBytes);
                    var json = payload.ToJSONString();
                    Console.WriteLine(json);
                }
                else { throw new InvalidOperationException("The second CBOR item is not a bytestring"); }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
         
        }

        public static Stream Decompress(byte[] data)
        {
            var outputStream = new MemoryStream();
            using (var compressedStream = new MemoryStream(data))
            using (var inputStream = new InflaterInputStream(compressedStream))
            {
                inputStream.CopyTo(outputStream);
                outputStream.Position = 0;
                return outputStream;
            }
        }
    }
}

