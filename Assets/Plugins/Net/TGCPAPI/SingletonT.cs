using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//泛型单列类
public class SingletonT<T> where T:new()
{
    static readonly T instance = new T();
    static SingletonT() { }

    public static T Instance
    {
        get 
        {
            return instance;
        }
    }
}

