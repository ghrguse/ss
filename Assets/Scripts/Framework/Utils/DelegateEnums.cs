using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class DelegateEnums
{
    public delegate void NoneParam();
    public delegate void DataParam(object data);

    public delegate bool NoneParamWithBoolResult();
    public delegate bool DataParamWithBoolResult(object data);
}
