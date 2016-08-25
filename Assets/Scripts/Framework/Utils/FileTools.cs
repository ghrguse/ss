using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System;

///************* 本地缓存，用法范例 ************
//void init()
//{
//    print("--------------------------------");
//    DataTest a = new DataTest();
//    a.aaa = "new test2222222";
//    FileTools.SaveDataToBin<DataTest>(a, "testData");

//    //
//    FileTools.GetDataFromPath<DataTest>("testData", onLoaded);
//}
//private void onLoaded(object p)
//{
//    DataTest t = (DataTest)p;
//    print(t.aaa);
//}
//[Serializable]
//struct DataTest
//{
//    public string aaa;
//    public string b;
//    public int c;
//}
///************* 本地缓存，用法范例 ************
///
public class FileTools
{
    //验证路径目录是否存在,不存在就创建目录
    public static void existPathDirectory(string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    //生成二进制文件
    public static void SaveDataToBin<T>(T data, string absolutePath)
    {
        //Log.info("dataSave:");
        Log.info(data == null);
        if (string.IsNullOrEmpty(absolutePath)) return;
        //Log.info("absolutePath: " + absolutePath);
        if (null == data) return;
        ObjectData2BinSerializer<T> serializer = new ObjectData2BinSerializer<T>();
        serializer.Serialize(data, absolutePath);
        //Log.info("save Success！");
    }

    //解析指定路径的二进制文件 
    public static void GetDataFromPath<T>(string absolutePath, DelegateEnums.DataParam fn)
    {
        ObjectData2BinSerializer<T> serializer = new ObjectData2BinSerializer<T>();
        serializer.DeserializeFromPath(absolutePath, fn);
    }

    //解析二进制数据
    public static T GetDataFromBin<T>(byte[] bytes)
    {
        if (null == bytes || 0 == bytes.Length) return default(T);
        ObjectData2BinSerializer<T> serializer = new ObjectData2BinSerializer<T>();
        return serializer.DeserializeBinary(bytes);
    }

    //生成xml
    public static void GenerateXml<T>(T data, string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath)) return;
        if (null == data) return;
        ObjectData2XmlSerializer<T> serializer = new ObjectData2XmlSerializer<T>();
        serializer.Serialize(data, absolutePath);
    }

    //解析xml
    public static T ParseXml<T>(string xml)
    {
        if (string.IsNullOrEmpty(xml)) return default(T);
        ObjectData2XmlSerializer<T> serializer = new ObjectData2XmlSerializer<T>();
        return serializer.Deserialize(xml);
    }
}

/************************************************************************/
/* 类数据转xml转换                                                        */
/************************************************************************/
public class ObjectData2XmlSerializer<T>
{
    public void Serialize(T data, string absolutePath)
    {
        try
        {

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
            else
            {
                FileTools.existPathDirectory(absolutePath);
            }
            FileStream fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
            XmlSerializer s = new XmlSerializer(typeof(T));
            s.Serialize(fileStream, data);
            fileStream.Close();
        }
        catch (System.Exception ex)
        {
            Log.infoError(ex.ToString());
            //throw (ex);
        }
    }

    public T Deserialize(string xml)
    {
        if (!File.Exists(xml))
        {
            return default(T);
        }
        try
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            T data = (T)s.Deserialize(File.OpenRead(xml));
            return data;
        }
        catch (System.Exception ex)
        {
            Log.infoError(ex.ToString());
            return default(T);
            //throw (ex);
        }
    }
}

/************************************************************************/
/* 类数据和binary转换                                                     */
/************************************************************************/
public class ObjectData2BinSerializer<T>
{
    public void Serialize(T data, string absolutePath)
    {
        try
        {
            if (null == data) return;
            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
            else
            {
                FileTools.existPathDirectory(absolutePath);
            }
            FileStream fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);
            BinaryFormatter b = new BinaryFormatter();
            b.Serialize(fileStream, data);
            fileStream.Close();
        }
        catch (System.Exception ex)
        {
            Log.infoError(ex.ToString());  
            //Log.info(ex.ToString());
            //throw (ex);
        }
    }

    private DelegateEnums.DataParam fn;
    public void DeserializeFromPath(string absolutePath, DelegateEnums.DataParam fn)
    {
        this.fn = fn;
        //Log.info("loadBinStart "+absolutePath);
        ResLoaderManager.instance.Loader(new StResPath(absolutePath), onFileLoaded);
    }
    public T Deserialize(string absolutePath)
    {
        if (!File.Exists(absolutePath))
        {
            return default(T);
        }
        try
        {
            FileStream fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryFormatter b = new BinaryFormatter();
            T data;
            data = (T)b.Deserialize(fileStream);
            fileStream.Close();
            return data;
        }
        catch (System.Exception ex)
        {
            Log.infoError(ex.ToString());
            return default(T);
            //throw (ex);
        }
    }
    private void onFileLoaded(string path, object param = null)
    {
        Byte[] bytes = ResDataManager.instance.GetDataBytes(path);
        if (bytes == null)
        {
            fn(null);
        }
        else
        {
            T data = DeserializeBinary(bytes);
            fn(data);
            ResDataManager.instance.RemoveAssetBundle(path);
        }
        fn = null;
    }

    public T DeserializeBinary(byte[] bytes)
    {
        if (null == bytes)
        {
            return default(T);
        }

        try
        {
            Stream steam = new MemoryStream(bytes);
            BinaryFormatter b = new BinaryFormatter();
            T data = (T)b.Deserialize(steam);
            steam.Close();
            return data;
        }
        catch (System.Exception ex)
        {
            Log.info(ex.ToString());
            return default(T);
            //throw (ex);
        }
    }
}
