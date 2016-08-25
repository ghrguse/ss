using GSProtocal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tsf4g_tdr_csharp;

public class MsgData
{
    public MsgData()
    {
        stGSPkg = new SMSG();
    }

    public void Set(byte[] data,int len)
    {
        Reset();
        pData = data;
        datalen = len;
    }

    /// <summary>
    /// 请求序列号，由服务器返回的包赋值
    /// </summary>
    public static Int32 ISeq = 0;

    public UInt16 Cmd
    {
        get 
        {
            return cmd;
        }
        set 
        {
            if (value > 0)
            {
                cmd = value;
            }
        }

    }

    /// <summary>
    /// 错误码
    /// </summary>
    public Int32 Eno
    {
        get { return nEno; }
        set { nEno = value; }
    }

    public UInt16 ListerCMD
    {
        get 
        {
            return ListenCmd;
        }

        set 
        {
            ListenCmd = value;
        }
    }

    public SData Body
    {
        get 
        {
            return stGSPkg.stBody;
        }
    }

    //解包
    public int UnPack()
    {
        int iRet = 0;
        TdrReadBuf unpackBuf = new TdrReadBuf(ref pData, datalen);
        TdrError.ErrorType ret = stGSPkg.unpack(ref unpackBuf,0);
        if (ret != TdrError.ErrorType.TDR_NO_ERROR)
        {
            System.Console.WriteLine(TdrError.getErrorString(ret));
            return (int)ret;
        }
        Cmd = stGSPkg.wCmdId;
        ISeq = stGSPkg.iSeq;
        Eno = stGSPkg.iEno;
        return iRet;
    }

    //压包
    public int Pack()
    {
        int iRet = 0;
        return iRet;
    }

    public void Reset()
    {
        pData = null;
        datalen = 0;
        cmd = 0;
    }

    public byte[] packData
    {
        get
        {
            return pData;
        }
    }

    public int packDataLen
    {
        get
        {
            return datalen;
        }
    }

#region 仅供Editor编辑器中使用
	#if UNITY_EDITOR
    public SMSG pkg
	{
		get { return stGSPkg; }
	}
	#endif
#endregion

    private byte[] pData;
    private int datalen;
    //协议id
    private UInt16 cmd;
    private Int32 iSeq;
    private Int32 nEno;
    //响应的回调CMD 
    private UInt16 ListenCmd;
    private SMSG stGSPkg;
}
