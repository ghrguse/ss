using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class TDRTools
{
    public static string byteToStr(byte[] bytes)
    {
        char[] array = Encoding.UTF8.GetChars(bytes);
        int len = 0;
        for (; len < array.Length; ++len)
        {
            if (array[len] == 0) break;
        }
        return new string(array, 0, len);
    }
    public static byte[] strToByte(string str)
    {
        if (str == null) return new byte[0];
        return Encoding.UTF8.GetBytes(str);
    }
}
