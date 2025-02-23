using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;


namespace DaPlatform.RSA
{
    public class RSA
    {
        public static RSACryptoServiceProvider ImportPrivateKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }


        public static RSACryptoServiceProvider ImportPublicKey(string pem)
        {
            PemReader pr = new PemReader(new StringReader(pem));
            AsymmetricKeyParameter publicKey = (AsymmetricKeyParameter)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaKeyParameters)publicKey);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }


        public static void ExportPublicKey(RSACryptoServiceProvider csp, TextWriter outputStream)
        {
            var parameters = csp.ExportParameters(false);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    innerWriter.Write((byte)0x30); // SEQUENCE
                    EncodeLength(innerWriter, 13);
                    innerWriter.Write((byte)0x06); // OBJECT IDENTIFIER
                    var rsaEncryptionOid = new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 0x0d, 0x01, 0x01, 0x01 };
                    EncodeLength(innerWriter, rsaEncryptionOid.Length);
                    innerWriter.Write(rsaEncryptionOid);
                    innerWriter.Write((byte)0x05); // NULL
                    EncodeLength(innerWriter, 0);
                    innerWriter.Write((byte)0x03); // BIT STRING
                    using (var bitStringStream = new MemoryStream())
                    {
                        var bitStringWriter = new BinaryWriter(bitStringStream);
                        bitStringWriter.Write((byte)0x00); // # of unused bits
                        bitStringWriter.Write((byte)0x30); // SEQUENCE
                        using (var paramsStream = new MemoryStream())
                        {
                            var paramsWriter = new BinaryWriter(paramsStream);
                            EncodeIntegerBigEndian(paramsWriter, parameters.Modulus); // Modulus
                            EncodeIntegerBigEndian(paramsWriter, parameters.Exponent); // Exponent
                            var paramsLength = (int)paramsStream.Length;
                            EncodeLength(bitStringWriter, paramsLength);
                            bitStringWriter.Write(paramsStream.GetBuffer(), 0, paramsLength);
                        }
                        var bitStringLength = (int)bitStringStream.Length;
                        EncodeLength(innerWriter, bitStringLength);
                        innerWriter.Write(bitStringStream.GetBuffer(), 0, bitStringLength);
                    }
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                outputStream.WriteLine("-----BEGIN PUBLIC KEY-----");
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END PUBLIC KEY-----");
            }
        }


        public static void ExportPrivateKey(RSACryptoServiceProvider csp, TextWriter outputStream)
        {
            if (csp.PublicOnly) throw new ArgumentException("CSP does not contain a private key", "csp");
            var parameters = csp.ExportParameters(true);
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                writer.Write((byte)0x30); // SEQUENCE
                using (var innerStream = new MemoryStream())
                {
                    var innerWriter = new BinaryWriter(innerStream);
                    EncodeIntegerBigEndian(innerWriter, new byte[] { 0x00 }); // Version
                    EncodeIntegerBigEndian(innerWriter, parameters.Modulus);
                    EncodeIntegerBigEndian(innerWriter, parameters.Exponent);
                    EncodeIntegerBigEndian(innerWriter, parameters.D);
                    EncodeIntegerBigEndian(innerWriter, parameters.P);
                    EncodeIntegerBigEndian(innerWriter, parameters.Q);
                    EncodeIntegerBigEndian(innerWriter, parameters.DP);
                    EncodeIntegerBigEndian(innerWriter, parameters.DQ);
                    EncodeIntegerBigEndian(innerWriter, parameters.InverseQ);
                    var length = (int)innerStream.Length;
                    EncodeLength(writer, length);
                    writer.Write(innerStream.GetBuffer(), 0, length);
                }

                var base64 = Convert.ToBase64String(stream.GetBuffer(), 0, (int)stream.Length).ToCharArray();
                outputStream.WriteLine("-----BEGIN RSA PRIVATE KEY-----");
                // Output as Base64 with lines chopped at 64 characters
                for (var i = 0; i < base64.Length; i += 64)
                {
                    outputStream.WriteLine(base64, i, Math.Min(64, base64.Length - i));
                }
                outputStream.WriteLine("-----END RSA PRIVATE KEY-----");
            }
        }


        public static void StorePEMtoLocalFileDirectory(RSACryptoServiceProvider csp, string LocalFileDirectory, bool privateKey)
        {
            if (privateKey)
            {
                using (TextWriter writer = File.CreateText(LocalFileDirectory))
                {
                    ExportPrivateKey(csp, writer);
                }
            }
            else
            {
                using (TextWriter writer = File.CreateText(LocalFileDirectory))
                {
                    ExportPublicKey(csp, writer);
                }
            }
        }

        public static RSACryptoServiceProvider LoadRSAfromLocalPEMFile(string LocalprivatePEMFileDirectory)
        {
            /*
            //use this code for .NET 5+
            var rsa = RSA.Create();
            var RSAfromPEM = File.ReadAllText(LocalprivatePEMFileDirectory);
            rsa.ImportFromPem(RSAfromPEM.ToCharArray());
            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            csp.ImportParameters(rsa.ExportParameters(true));
            return csp;
            */
            var RSAfromPEM = File.ReadAllText(LocalprivatePEMFileDirectory);
            PemReader pr = new PemReader(new StringReader(RSAfromPEM));
            AsymmetricCipherKeyPair KeyPair = (AsymmetricCipherKeyPair)pr.ReadObject();
            RSAParameters rsaParams = DotNetUtilities.ToRSAParameters((RsaPrivateCrtKeyParameters)KeyPair.Private);

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();// cspParams);
            csp.ImportParameters(rsaParams);
            return csp;
        }

        public static string Encrypt(string plainText, string publicKey)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            PemReader pr = new PemReader(new StringReader(publicKey));
            RsaKeyParameters keys = (RsaKeyParameters)pr.ReadObject();

            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            OaepEncoding eng = new OaepEncoding(new RsaEngine());
            eng.Init(true, keys);

            int length = plainTextBytes.Length;
            int blockSize = eng.GetInputBlockSize();
            List<byte> cipherTextBytes = new List<byte>();
            for (int chunkPosition = 0;
                chunkPosition < length;
                chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, length - chunkPosition);
                cipherTextBytes.AddRange(eng.ProcessBlock(
                    plainTextBytes, chunkPosition, chunkSize
                ));
            }
            return Convert.ToBase64String(cipherTextBytes.ToArray());
        }

        public static string Decrypt(string cipherText, string privateKey)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);

            PemReader pr = new PemReader(new StringReader(privateKey));
            var temp = (RsaPrivateCrtKeyParameters)pr.ReadObject();
            var publicParameters = new RsaKeyParameters(false, temp.Modulus, temp.Exponent);
            var keys = new AsymmetricCipherKeyPair(publicParameters, temp);


            // Pure mathematical RSA implementation
            // RsaEngine eng = new RsaEngine();

            // PKCS1 v1.5 paddings
            // Pkcs1Encoding eng = new Pkcs1Encoding(new RsaEngine());

            // PKCS1 OAEP paddings
            OaepEncoding eng = new OaepEncoding(new RsaEngine());
            eng.Init(false, keys.Private);

            int length = cipherTextBytes.Length;
            int blockSize = eng.GetInputBlockSize();
            List<byte> plainTextBytes = new List<byte>();
            for (int chunkPosition = 0;
                chunkPosition < length;
                chunkPosition += blockSize)
            {
                int chunkSize = Math.Min(blockSize, length - chunkPosition);
                plainTextBytes.AddRange(eng.ProcessBlock(
                    cipherTextBytes, chunkPosition, chunkSize
                ));
            }
            return Encoding.UTF8.GetString(plainTextBytes.ToArray());
        }

        private static void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }


        private static void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }
    }
}


        //static void Main(string[] args)
        //{
        //    Console.WriteLine("Hello World!\n");

        //    /*
        //    var cryptoServiceProvider = new RSACryptoServiceProvider(384);
        //    StringBuilder sb = new StringBuilder();
        //    StringWriter sw = new StringWriter(sb);
        //    ExportPrivateKey(cryptoServiceProvider, sw);
        //    string privKeyString = sb.ToString();
        //    Console.WriteLine(privKeyString);
        //    Console.ReadLine();
        //    */

        //    var csp2 = LoadRSAfromLocalPEMFile(@"D:\privateKey.pem");
        //    StringBuilder sb = new StringBuilder();
        //    StringWriter sw = new StringWriter(sb);
        //    ExportPrivateKey(csp2, sw);
        //    string privKeyString = sb.ToString();


        //    var csp = new RSACryptoServiceProvider(3584);
        //    string encryptedText = Convert.ToBase64String(csp.Encrypt
        //        (Encoding.UTF8.GetBytes(privKeyString), true));
        //    Console.WriteLine(encryptedText);
        //    Console.ReadLine();

        //    string decryptedText = Encoding.UTF8.GetString(csp.Decrypt
        //        (Convert.FromBase64String(encryptedText), true));
        //    Console.WriteLine(decryptedText);
        //    Console.ReadLine();


        //    //StorePEMtoLocalFileDirectory(cryptoServiceProvider, @"D:\privateKey.pem", true);
        //    /*
        //    var csp2 = LoadRSAfromLocalPEMFile(@"D:\privateKey.pem");
        //    //File.Delete(@"D:\privateKey.pem");
        //    string DecryptedText = Encoding.UTF8.GetString(csp2.Decrypt
        //        (Convert.FromBase64String(encryptedText), true));
        //    Console.WriteLine(DecryptedText + '\n');
        //    */

        //    Console.WriteLine("Bye World!\n");
        //}   
    

/*
public static string GetKeyString(RSAParameters rsaKey)
{

    var stringWriter = new System.IO.StringWriter();
    var xmlSerializer = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
    xmlSerializer.Serialize(stringWriter, rsaKey);
    return stringWriter.ToString();
}

public static string Encrypt(string textToEncrypt, RSACryptoServiceProvider csp)
{
    var bytesToEncrypt = Encoding.UTF8.GetBytes(textToEncrypt);

    using (var rsa = new RSACryptoServiceProvider(2048))
    {
        try
        {
            //rsa.FromXmlString(publicKeyString.ToString());
            var encryptedData = rsa.Encrypt(bytesToEncrypt, true);
            var base64Encrypted = Convert.ToBase64String(encryptedData);
            return base64Encrypted;
        }
        finally
        {
            rsa.PersistKeyInCsp = false;
        }
    }
}

public static string Decrypt(string textToDecrypt, string privateKeyString)
{
    //var bytesToDescrypt = Encoding.UTF8.GetBytes(textToDecrypt);

    using (var rsa = new RSACryptoServiceProvider(2048))
    {
        try
        {

            // server decrypting data with private key                    
            rsa.FromXmlString(privateKeyString);

            var resultBytes = Convert.FromBase64String(textToDecrypt);
            var decryptedBytes = rsa.Decrypt(resultBytes, true);
            var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
            return decryptedData.ToString();
        }
        finally
        {
            rsa.PersistKeyInCsp = false;
        }
    }

public static string GenerateTestString()
{
    Guid opportinityId = Guid.NewGuid();
    Guid systemUserId = Guid.NewGuid();
    string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    StringBuilder sb = new StringBuilder();
    sb.AppendFormat("opportunityid={0}", opportinityId.ToString());
    sb.AppendFormat("&systemuserid={0}", systemUserId.ToString());
    sb.AppendFormat("&currenttime={0}", currentTime);

    return sb.ToString();
}
        
public static byte[] Zip(string str)
{
    var bytes = System.Text.Encoding.UTF8.GetBytes(str);
    using (var msi = new MemoryStream(bytes))
    using (var mso = new MemoryStream())
    {
        using (var gs = new GZipStream(mso, CompressionMode.Compress))
        {
            CopyTo(msi, gs);
        }
        return mso.ToArray();
    }
}
public static string Unzip(byte[] bytes)
{
    using (var msi = new MemoryStream(bytes))
    using (var mso = new MemoryStream())
    {
        using (var gs = new GZipStream(msi, CompressionMode.Decompress))
        {
            CopyTo(gs, mso);
        }
        return System.Text.Encoding.UTF8.GetString(mso.ToArray());
    }
}

public static void CopyTo(Stream src, Stream dest)
{
    byte[] bytes = new byte[4096];

    int cnt;

    while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
    {
        dest.Write(bytes, 0, cnt);
    }
}
*/
