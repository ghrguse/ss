/* This file is generated by tdr. */
/* No manual modification is permitted. */

/* creation time: Thu Aug 18 18:14:29 2016 */
/* tdr version: 2.7.12, build at 20151212 */


using System;
using System.Diagnostics;
using System.Text;

namespace tsf4g_tdr_csharp
{


public class TdrBufUtil
{
 public  static TdrError.ErrorType printMultiStr(ref TdrVisualBuf buf, string str, int times)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  for (int i = 0; i < times; i++)
  {
   ret = buf.sprintf("{0}", str);
   if (ret != TdrError.ErrorType.TDR_NO_ERROR)
   {
    break;
   }
  }
  return ret;
 }

 public static TdrError.ErrorType printVariable(ref TdrVisualBuf buf, int indent, char sep, string variable, bool withSep)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   if (withSep)
   {
    ret = buf.sprintf("{0}{1}", variable, sep);
   }
   else
   {
    ret = buf.sprintf("{0}: ", variable);
   }
  }

  return ret;
 }

 public static TdrError.ErrorType printVariable(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx, bool withSep)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   if (withSep)
   {
    ret = buf.sprintf("{0}[{1:d}]{2}", variable, arrIdx, sep);
   }
   else
   {
    ret = buf.sprintf("{0}[{1:d}]: ", variable, arrIdx);
   }
  }

  return ret;
 }

 public static TdrError.ErrorType printVariable(ref TdrVisualBuf buf, int indent, char sep, string variable, string format, params object[] args)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}: ", variable);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf(format, args);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}", sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printVariable(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx, string format, params object[] args)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}[{1:d}]: ", variable,arrIdx);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf(format, args);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}", sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printArray(ref TdrVisualBuf buf, int indent, char sep, string variable,Int64 count)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}[0:{1:d}]: ", variable, count);
  }

  return ret;
 }

 public static TdrError.ErrorType printString(ref TdrVisualBuf buf, int indent, char sep, string variable, byte[] bStr)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;
  string strUni = "";
  int count = TdrTypeUtil.cstrlen(bStr);

  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   ret = printMultiStr(ref buf, "    ", indent);
  }

  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   strUni = Encoding.ASCII.GetString(bStr, 0, count);
  }

  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   ret = buf.sprintf("{0}: {1}{2}", variable, strUni, sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printWString(ref TdrVisualBuf buf, int indent, char sep, string variable, Int16[] str)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;
  int count = TdrTypeUtil.wstrlen(str) + 1;
  ret = buf.sprintf("{0}:  ",variable);
  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   int len = TdrTypeUtil.wstrlen(str);
   for (int i = 0; i < len; i++)
   {
    ret = buf.sprintf("0x{0:X4}", str[i]);
    if (TdrError.ErrorType.TDR_NO_ERROR != ret)
    {
     break;
    }
   }
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}", sep);
  }

  return ret;
 }
 public static TdrError.ErrorType printString(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx,byte[] bStr)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;
  string strUni = "";
  int count = TdrTypeUtil.cstrlen(bStr);

  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   ret = printMultiStr(ref buf, "    ", indent);
  }

  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   strUni = Encoding.ASCII.GetString(bStr, 0, count);
  }

  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   ret = buf.sprintf("{0}[{1:d}]: {2}{3}", variable, arrIdx,strUni, sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printWString(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx,Int16[] str)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;
  int count = TdrTypeUtil.wstrlen(str) + 1;
  ret = buf.sprintf("{0}[{1:d}]",variable,arrIdx);
  if (ret == TdrError.ErrorType.TDR_NO_ERROR)
  {
   int len = TdrTypeUtil.wstrlen(str);
   for (int i = 0; i < len; i++)
   {
    ret = buf.sprintf("0x{0:X4}", str[i]);
    if (TdrError.ErrorType.TDR_NO_ERROR != ret)
    {
     break;
    }
   }
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}", sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrIP(ref TdrVisualBuf buf, int indent, char sep, string variable, UInt32 ip)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}: ", variable);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrIP2Str(ref buf,ip);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrIP(ref TdrVisualBuf buf, int indent, char sep, string variable,int arrIdx, UInt32 ip)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}[{1:d}]: ", variable, arrIdx);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrIP2Str(ref buf,ip);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrTime(ref TdrVisualBuf buf, int indent, char sep, string variable, UInt32 time)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}: ", variable);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrTime2Str(ref buf,time);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrTime(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx, UInt32 time)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}[{1:d}]: ", variable, arrIdx);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrTime2Str(ref buf,time);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrDate(ref TdrVisualBuf buf, int indent, char sep, string variable, UInt32 date)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}: ", variable);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrDate2Str(ref buf,date);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrDate(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx, UInt32 date)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}[{1:d}]: ", variable, arrIdx);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrDate2Str(ref buf,date);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrDateTime(ref TdrVisualBuf buf, int indent, char sep, string variable, UInt64 datetime)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}: ", variable);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrDateTime2Str(ref buf,datetime);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

 public static TdrError.ErrorType printTdrDateTime(ref TdrVisualBuf buf, int indent, char sep, string variable, int arrIdx, UInt64 datetime)
 {
  TdrError.ErrorType ret = TdrError.ErrorType.TDR_NO_ERROR;

  ret = printMultiStr(ref buf, "    ", indent);
  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}[{1:d}]: ", variable, arrIdx);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = TdrTypeUtil.tdrDateTime2Str(ref buf,datetime);
  }

  if (TdrError.ErrorType.TDR_NO_ERROR == ret)
  {
   ret = buf.sprintf("{0}",sep);
  }

  return ret;
 }

}

}
