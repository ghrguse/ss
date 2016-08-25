using UnityEngine;
using System.Collections;

public enum PERFORMANCE_ENUM
{
    HIGH = 1,
    MIDDLE_HIGH,
    MIDDLE,
    LOW,
    PC
}

public class SystemInfoCheck
{
    private static string[] mi2GpuList = { "Adreno (TM) 320", "Adreno (TM) 220", "Adreno (TM) 203" };
    private static string[] lowCpuSet = { "mt", "hi", "k3", "v9" };

    public static int defaultScreenWidth = 128;
    public static int defaultScreenHeight = 128;



    public static void cacheDefaultResolution()
    {
        defaultScreenWidth = Screen.currentResolution.width;
        defaultScreenHeight = Screen.currentResolution.height;
        Log.info("SystemInfo.systemMemorySize : " + SystemInfo.systemMemorySize);
    }

#if UNITY_EDITOR
    public static PERFORMANCE_ENUM GetPerformanceType()
    {
        return PERFORMANCE_ENUM.PC;
    }
#elif UNITY_IPHONE
    public static PERFORMANCE_ENUM GetPerformanceType()
    {
        iPhoneGeneration iosGen = iPhone.generation;
        if (iosGen == iPhoneGeneration.iPhone4S ||
            iosGen == iPhoneGeneration.iPhone4)
        {
            return PERFORMANCE_ENUM.LOW;
        } else
            if (iosGen == iPhoneGeneration.iPhone6 ||
                iosGen == iPhoneGeneration.iPhone6Plus)
        {
            return PERFORMANCE_ENUM.MIDDLE;
        }
        
        // 其他统一为中配
        return  PERFORMANCE_ENUM.MIDDLE;
    }
#elif UNITY_ANDROID
    public static PERFORMANCE_ENUM GetPerformanceType()
    {
        int height = Screen.height;
        uint cpuCount = HWInfo.getCpuCoreNumber();
        long ram = long.Parse(PhoneInfo.getTotalMemory()) / 1024 / 1024;
        float frequency = (float) HWInfo.getCpuMaxFre() / 1024 / 1024;

        Log.info("height : " + height);
        Log.info("cpuCount : " + cpuCount);
        Log.info("ram : " + ram);
        Log.info("frequency : " + frequency);

        if (ram >= 2500)
        {
            // 大于3g全部为中
            return PERFORMANCE_ENUM.MIDDLE;
        } else
        if (ram < 1600)
        {
            // 小于2G 全部为低
            return PERFORMANCE_ENUM.LOW;
        }

        if (cpuCount > 4)
        {
            // 4核以上为中
            return PERFORMANCE_ENUM.MIDDLE;
        }
        
        // 获取硬件信息
        string hardware = PhoneInfo.getHardware();
        Log.info("hardware : " + hardware);
        foreach (string member in lowCpuSet)
        {
            if (hardware.ToLower().StartsWith(member))
                // 指定低端cpu的，全部为低
                return PERFORMANCE_ENUM.LOW;
        }

        // 其他为中
        return PERFORMANCE_ENUM.MIDDLE;
    }
#endif

    // 判断是否在需要优化的列表中
    public static bool CheckNeedFixMi2()
    {
        foreach (string gpuName in mi2GpuList)
        {
            if (SystemInfo.graphicsDeviceName.Contains(gpuName))
                return true;
        }

        return false;
    }

    // 判断是否内存512
    public static bool MemoryLow512()
    {
#if UNITY_IPHONE
        iPhoneGeneration iosGen = iPhone.generation;
        if (iosGen == iPhoneGeneration.iPhone4)
            return true;
        else
            return false;
#else
        if (SystemInfo.systemMemorySize < 512)
            return true;
        else
            return false;
#endif
    }

    // 判断是否是iPhone4s
    public static bool IsIphone4s()
    {
#if UNITY_IPHONE
        iPhoneGeneration iosGen = iPhone.generation;
        if (iosGen == iPhoneGeneration.iPhone4S)
            return true;
        else
            return false;
#else
        return false;
#endif
    }


    public static bool CheckCanCacheResource()
    {
#if UNITY_EDITOR
        return true;
#elif UNITY_IPHONE
        iPhoneGeneration iosGen = iPhone.generation;
        if (iosGen == iPhoneGeneration.iPhone4S ||
            iosGen == iPhoneGeneration.iPhone4)
            return true;
        else
            return false;
#elif UNITY_ANDROID
        long ram = long.Parse(PhoneInfo.getTotalMemory()) / 1024 / 1024;
        if (ram >= 1024)
            return false;
        else
            return true;
#else
        return true;
#endif
    }


}
