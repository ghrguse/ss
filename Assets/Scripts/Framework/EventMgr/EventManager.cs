using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using System.Collections;

namespace Utils.Event
{
    public class EventManager
    {

        public delegate void EventCallBack(GEvent e);

        //event对象池
        private static ObjectPool _pool = new ObjectPool(typeof(GEvent), null, 120, 1000);

        //回调列表字典
        private static Dictionary<string, List<EventCallBack>> _callbackDict = new Dictionary<string, List<EventCallBack>>();

        /// <summary>
        /// 从对象池里获取GEvent
        /// </summary>
        /// <param name="type"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static GEvent CreateEvent(string type, object data = null)
        {
            GEvent evt = _pool.GetObject() as GEvent;//使用对象池，避免频繁实例,触发GCloc.
            if (evt == null)
            {
                Log.info("事件管理器[EventManager] 当前事件个数:" + _pool.ToString());
            }
            evt.type = type;
            evt.data = data;
            return evt;
        }

        //派发事件
        public static void Send(string type, object data = null)
        {
            SendEvent(CreateEvent(type, data));
        }

        //添加监听事件
        public static void AddEventListener(string type, EventCallBack fn)
        {
            if (!_callbackDict.ContainsKey(type))
            {
                _callbackDict.Add(type, new List<EventCallBack>());
            }

            if (_callbackDict[type].Contains(fn))
            {
                return;
            }

            _callbackDict[type].Add(fn);
        }

        //移除监听事件
        public static void RemoveEventListener(string type, EventCallBack fn)
        {
            if (_callbackDict.ContainsKey(type) && _callbackDict[type].Contains(fn))
            {
                _callbackDict[type].Remove(fn);
                if (_callbackDict[type].Count <= 0)
                {
                    _callbackDict.Remove(type);
                }
            }
        }

        //移除整个类型的监听事件
        public static void RemoveEventListenerByType(string type)
        {
            if (_callbackDict.ContainsKey(type))
            {
                _callbackDict.Remove(type);
            }
        }

        //此监听事件是否存在
        public static bool HasEventListener(string type, EventCallBack fn)
        {
            if (_callbackDict.ContainsKey(type))
            {
                if (_callbackDict[type].Contains(fn))
                {
                    return true;
                }
                return false;
            }
            return false;
        }

        //此类型监听是否存在
        public static bool HasEventListenerByType(string type)
        {
            if (_callbackDict.ContainsKey(type))
            {
                return true;
            }
            return false;
        }


        //派发事件
        private static void SendEvent(GEvent evt)
        {
            if (_callbackDict.ContainsKey(evt.type))
            {
                List<EventCallBack> list = (_callbackDict[evt.type] as List<EventCallBack>).ToList();
                foreach (EventCallBack fn in list)
                {
                    try
                    {
                        fn(evt);
                    }
                    catch (Exception ex)
                    {
                        Log.infoError(string.Format("SendEvent Exception {0}, {1}", evt.type, ex.ToString()));
                        //CLogSys.Log(ELogLevel.Fatal, ELogTag.Event, string.Format("SendEvent Exception {0}, {1}", evt.type, ex.ToString()));
                    }
                }
            }

            //CLogSys.Log(ELogLevel.Verbose, ELogTag.Event, string.Format("SendEvent {0}", evt.type));

            _pool.FreeObject(evt.GetHashCode());//扔回对象池

        }
    }
}//end namespace