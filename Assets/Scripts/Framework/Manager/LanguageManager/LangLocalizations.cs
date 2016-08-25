using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// 语言类型
/// </summary>
public enum LanguageTypeEnum
{
    CN = 0, //简体
    TW = 1, //繁体
    EN = 2, //英文
    KOR = 3  //韩语
}

/// <summary>
/// 本地化工具
/// </summary>
public class LangLocalizations
{

    private static Dictionary<string, string> _langDict = new Dictionary<string, string>();

    private static LanguageTypeEnum _lang = LanguageTypeEnum.CN;

    /// <summary>
    /// 获取语言版本
    /// </summary>
    public static LanguageTypeEnum Language
    {
        get { return _lang; }
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init(LanguageTypeEnum lang, Dictionary<string, string> data)
    {
        _lang = lang;
        if (data == null) return;
        _langDict = data;
    }

    /// <summary>
    /// 获取语言
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string GetWord(string key)
    {
        if (_lang == LanguageTypeEnum.CN) return key;

        if (_langDict.ContainsKey(key))
        {
            return _langDict[key];
        }
        return key;
    }

    /// <summary>
    /// 获取组合文本  txt = "掉落物品{0}：{1}个","xx之剑","5"；
    /// </summary>
    /// <param name="key"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    public static string GetWord(string key, params object[] args)
    {
        string txt = GetWord(key);
        return string.Format(txt, args);
    }
}
