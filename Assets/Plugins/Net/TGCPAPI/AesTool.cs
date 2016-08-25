using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

    public class AesTool
    {
        /// <summary>
        /// 设置Key
        /// </summary>
        /// <param name="key"></param>
        public static void SetKey(long key)
        {
            keyByte = System.BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(key));
            keyByte = keyByte.Concat(keyByte).ToArray();
        }
        //static Rijndael des = Rijndael.Create();
        /// <summary>
        /// Key
        /// </summary>
        private static byte[] keyByte;
        

        private static byte[] bytel = new byte[16] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

        //验证Key
        private static byte[] value = new byte[4] {77,76,89,83};
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="inputdata">输入值</param>
        /// <returns></returns>
        public static byte[] AESEncrypt(byte[] inputdata)
        {
            //分组加密算法   
            Rijndael des = Rijndael.Create();
            inputdata = value.Concat(inputdata).ToArray();
            inputdata = inputdata.Concat(value).ToArray();
            byte[] inputByteArray = inputdata;// Encoding.ASCII.GetBytes("MLYS" + inputdata + "MLYS");//得到需要加密的字节数组      

            //设置密钥及密钥向量
            des.Key = keyByte;
          
           
            //Log.info("AESEncrypt31");
            des.IV = bytel;// des.Key;
            //Log.info("AESEncrypt32");
            des.Mode = CipherMode.CBC;
            //Log.info("AESEncrypt33");
            des.FeedbackSize = 128;
            //Log.info("AESEncrypt34");
            des.Padding = PaddingMode.PKCS7;
            //Log.info("AESEncrypt4");
            using (MemoryStream ms = new MemoryStream())
            {
                //Log.info("AESEncrypt5");
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    //Log.info("AESEncrypt6");
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    //Log.info("AESEncrypt7");
                    byte[] cipherBytes = ms.ToArray();//得到加密后的字节数组   
                    cs.Close();
                    ms.Close();
                    return cipherBytes;
                }
            }
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="inputdata">输入的数据</param>
        /// <param name="strKey">key</param>
        /// <returns></returns>
        public static byte[] AESDecrypt(byte[] inputdata)
        {
            //Log.info("AESDecrypt1");
            if (inputdata.Length % 16 != 0) return null;
            Rijndael des = Rijndael.Create();
            //Log.info("AESDecrypt2");
            //SymmetricAlgorithm des = Rijndael.Create();
            //Log.info("AESDecrypt3");
            //byte[] keyByte = System.BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(keyByte));
            des.Key = keyByte;// keyByte.Concat(keyByte).ToArray();
            des.IV = bytel;
            byte[] decryptBytes = new byte[inputdata.Length];
            int len = 0;
            //Log.info("AESDecrypt4");
            using (MemoryStream ms = new MemoryStream(inputdata))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    len=cs.Read(decryptBytes, 0, decryptBytes.Length);
                    cs.Close();
                    ms.Close();
                }
            }
            //Log.info("AESDecrypt8");
            for (int i = 0; i < 4; i++)
            {
                if (decryptBytes[i] != value[i]) 
                {
                    return null;
                }
            }
            //Log.info("AESDecrypt9");
            if (len < 8)
                return null;
            for (int i = len - 4; i < len; i++)
            {
                if (decryptBytes[i] != value[i - len + 4])
                {
                    return null;
                }
            }
            //Log.info("AESDecrypt10");

            byte[] tempByte = new byte[len - 8];
            for (int i = 4; i < tempByte.Length+4; i++)
            {
                tempByte[i-4] = decryptBytes[i];
            }
            //Log.info("AESDecrypt11");
            return tempByte;
        }
    }
