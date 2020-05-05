using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Solria.SAFT.Desktop.Services
{
    public class RSAKeys
    {
        public const string PEM_PRIV_HEADER = "-----BEGIN RSA PRIVATE KEY-----";
        public const string PEM_PRIV_FOOTER = "-----END RSA PRIVATE KEY-----";
        public const string PEM_PUB_HEADER = "-----BEGIN PUBLIC KEY-----";
        public const string PEM_PUB_FOOTER = "-----END PUBLIC KEY-----";

        /// <summary>
        /// RSA public key
        /// </summary>
        public string PublicKey { get; set; }

        /// <summary>
        /// RSA private key
        /// </summary>
        public string PrivateKey { get; set; }

        /// <summary>
        /// Decode PEM pubic or private key
        /// </summary>
        /// <param name="fileContent">File content</param>
        public void DecodePEMKey(string fileContent)
        {
            byte[] pempublickey;
            byte[] pemprivatekey;

            fileContent = fileContent.Trim();

            //pem public key file
            if (fileContent.StartsWith(PEM_PUB_HEADER) && fileContent.EndsWith(PEM_PUB_FOOTER))
            {
                pempublickey = DecodeOpenSSL(fileContent);
                if (pempublickey != null)
                {
                    var rsa = DecodeX509PublicKey(pempublickey);
                    string xmlpublickey = rsa.ToXmlString(false);

                    PublicKey = xmlpublickey;
                }
            } //pem private key file
            else if (fileContent.StartsWith(PEM_PRIV_HEADER) && fileContent.EndsWith(PEM_PRIV_FOOTER))
            {
                pemprivatekey = DecodeOpenSSL(fileContent);
                if (pemprivatekey != null)
                {
                    var rsa = DecodeRSAPrivateKey(pemprivatekey);
                    string xmlprivatekey = rsa.ToXmlString(true);

                    PrivateKey = xmlprivatekey;
                }
            }
        }

        /// <summary>
        /// Get the binary RSA PUBLIC key
        /// </summary>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        private byte[] DecodeOpenSSL(string fileContent)
        {
            StringBuilder sb = new StringBuilder(fileContent);
            //remove headers/footers, if present
            sb.Replace(PEM_PUB_HEADER, string.Empty);
            sb.Replace(PEM_PUB_FOOTER, string.Empty);

            sb.Replace(PEM_PRIV_HEADER, string.Empty);
            sb.Replace(PEM_PRIV_FOOTER, string.Empty);

            try
            {
                return Convert.FromBase64String(sb.ToString());
            }
            catch (FormatException)
            {
                //if can't b64 decode, data is not valid
                return null;
            }
        }

        /// <summary>
        /// Parses binary asn.1 X509 SubjectPublicKeyInfo; returns RSACryptoServiceProvider
        /// </summary>
        /// <param name="x509key">Pem file content decoded.</param>
        /// <returns>RSACryptoServiceProvider instance of the public pem file.</returns>
        private RSACryptoServiceProvider DecodeX509PublicKey(byte[] x509key)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            // Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob
            MemoryStream mem = new MemoryStream(x509key);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            
            try
            {
                var twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                //read the Sequence OID
                var seq = binr.ReadBytes(15);
                //make sure Sequence for OID is correct
                if (!CompareByteArrays(seq, SeqOID))
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8103)	//data read as little endian order (actual data order for Bit String is 03 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8203)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                var bt = binr.ReadByte();
                //expect null byte next
                if (bt != 0x00)
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                byte lowbyte = 0x00;
                byte highbyte = 0x00;

                if (twobytes == 0x8102)	//data read as little endian order (actual data order for Integer is 02 81)
                    lowbyte = binr.ReadByte();	// read next bytes which is bytes in modulus
                else if (twobytes == 0x8202)
                {
                    highbyte = binr.ReadByte();	//advance 2 bytes
                    lowbyte = binr.ReadByte();
                }
                else
                    return null;
                //reverse byte order since asn.1 key uses big endian order
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                int modsize = BitConverter.ToInt32(modint, 0);

                byte firstbyte = binr.ReadByte();
                binr.BaseStream.Seek(-1, SeekOrigin.Current);

                if (firstbyte == 0x00)
                {	//if first byte (highest order) of modulus is zero, don't include it
                    //skip this null byte
                    binr.ReadByte();
                    //reduce modulus buffer size by 1
                    modsize -= 1;
                }

                //read the modulus bytes
                byte[] modulus = binr.ReadBytes(modsize);

                //expect an Integer for the exponent data
                if (binr.ReadByte() != 0x02)
                    return null;
                // should only need one byte for actual exponent data (for all useful values)
                int expbytes = (int)binr.ReadByte();
                byte[] exponent = binr.ReadBytes(expbytes);

                // create RSACryptoServiceProvider instance and initialize with public key
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAKeyInfo = new RSAParameters
                {
                    Modulus = modulus,
                    Exponent = exponent
                };
                RSA.ImportParameters(RSAKeyInfo);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Close(); }
        }

        /// <summary>
        /// Parses binary ans.1 RSA private key; returns RSACryptoServiceProvider
        /// </summary>
        /// <param name="privkey">>Pem file content decoded.</param>
        /// <returns>RSACryptoServiceProvider instance of the private pem file.</returns>
        private RSACryptoServiceProvider DecodeRSAPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // Set up stream to decode the asn.1 encoded RSA private key  ------
            MemoryStream mem = new MemoryStream(privkey);
            BinaryReader binr = new BinaryReader(mem);    //wrap Memory Stream with BinaryReader for easy reading
            
            try
            {
                var twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)	//data read as little endian order (actual data order for Sequence is 30 81)
                    binr.ReadByte();	//advance 1 byte
                else if (twobytes == 0x8230)
                    binr.ReadInt16();	//advance 2 bytes
                else
                    return null;

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)	//version number
                    return null;
                var bt = binr.ReadByte();
                if (bt != 0x00)
                    return null;

                // all private key components are Integer sequences
                var elems = GetIntegerSize(binr);
                MODULUS = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                E = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                D = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                P = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                Q = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DP = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                DQ = binr.ReadBytes(elems);

                elems = GetIntegerSize(binr);
                IQ = binr.ReadBytes(elems);

                // create RSACryptoServiceProvider instance and initialize with public key 
                RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                RSAParameters RSAparams = new RSAParameters
                {
                    Modulus = MODULUS,
                    Exponent = E,
                    D = D,
                    P = P,
                    Q = Q,
                    DP = DP,
                    DQ = DQ,
                    InverseQ = IQ
                };
                RSA.ImportParameters(RSAparams);
                return RSA;
            }
            catch (Exception)
            {
                return null;
            }
            finally { binr.Close(); }
        }

        private bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            return a.Where((c, i) => c != b[i]).Count() == 0;
        }

        private int GetIntegerSize(BinaryReader binr)
        {
            var bt = binr.ReadByte();
            if (bt != 0x02)		//expect integer
                return 0;
            bt = binr.ReadByte();

            int count;
            if (bt == 0x81)
                count = binr.ReadByte();	// data size in next byte
            else
            {
                if (bt == 0x82)
                {
                    var highbyte = binr.ReadByte();	// data size in next 2 bytes
                    var lowbyte = binr.ReadByte();
                    byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                    count = BitConverter.ToInt32(modint, 0);
                }
                else
                    count = bt;		// we already have the data size
            }

            while (binr.ReadByte() == 0x00)
            {	//remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);		//last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }
    }
}
