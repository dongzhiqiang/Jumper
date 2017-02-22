using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using LuaInterface;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Util
{
    public const float One_Frame = 1 / 30f;//一帧的时间
    public static float time
    {
        get
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                return Time.time;
#if UNITY_EDITOR
            }
            else
            {
                System.TimeSpan span = System.DateTime.Now.TimeOfDay;//这里怕totalsceonds转成float的时候超出float的最大值，所以先减下
                return (float)span.TotalSeconds;
            }
#endif
        }
    }

  

    public static void DoAllChild(Transform t, System.Action<Transform> a)
    {
        if (t == null) return;
        a(t);
        for (int i = 0, imax = t.childCount; i < imax; ++i)
            DoAllChild(t.GetChild(i), a);
    }

    public static void DoAllChild<T>(Transform t, System.Action<T> a) where T : Component
    {
        if (t == null) return;
        T component = t.GetComponent<T>();
        if (component != null)
            a(component);
        for (int i = 0, imax = t.childCount; i < imax; ++i)
            DoAllChild<T>(t.GetChild(i), a);
    }

    public static Transform GetRoot(Transform t)
    {
        if (t == null) return null;
        do
        {
            if (t.parent != null)
                t = t.parent;
            else
                return t;
        } while (true);
    }

    public static T GetRoot<T>(Transform t) where T : Component
    {
        do
        {
            T component = t.GetComponent<T>();
            if (component != null)
                return component;
            if (t.parent != null)
                t = t.parent;
            else
                return null;
        } while (true);
    }

    public static string GetGameObjectPath(GameObject obj)
    {
        string path = "/" + obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = "/" + obj.name + path;
        }
        return path;
    }


    //和Mathf.Clamp不同，比如[0,4)如果填-1会返回3，填4会返回0。注意这里是开区间，因为多用在枚举的计算上
    public static int Clamp(int value, int a, int b)
    {
        int tem;
        if (a == b)
            return a;
        else if (a > b)
        {
            tem = a;
            a = b;
            b = tem;
        }

        tem = (value - a) % (b - a);
        if (tem < 0)
            tem = (b - a) + tem;

        //Debuger.Log(string.Format("value:{0} clamp:{1}", value, a + tem));
        return a + tem;
    }

    //返回某小数的倍数
    public static float FloorOf(float value, float unit)
    {
        return value - value % unit;
    }

    //返回某小数的倍数
    public static float ClosestOf(float value, float unit)
    {
        float sign = Mathf.Sign(value);
        float f = (value % unit) * sign;
        if (Mathf.Approximately(f, 0))
            return value;
        else if (f >= unit / 2)
            return value - sign * f + sign * unit;
        else
            return value - sign * f;
    }

    //获取某个目录下的文件名
    static public List<string> GetAllFileList(string path, string prefix = null)
    {
        int startIndex = path.Length + 1;
        if (!System.IO.Directory.Exists(path))
            return new List<string>();

        string[] files = System.IO.Directory.GetFiles(path, "*.*", System.IO.SearchOption.AllDirectories);
        List<string> resList = new List<string>();
        string tmp;
        string s;
        for (int i = 0; i < files.Length; ++i)
        {
            s = files[i].Replace('\\', '/').Substring(startIndex);
            tmp = s.ToLower();

            if (tmp.EndsWith(".meta") ||
                tmp.Contains("__copy__/") ||
                tmp.Contains(".svn/") ||
                tmp.EndsWith(".dll") ||
                tmp.EndsWith(".js") ||
                tmp.EndsWith(".cs"))
                continue;

            if (string.IsNullOrEmpty(prefix))
                resList.Add(s);
            else
                resList.Add(prefix + s);

        }

        return resList;
    }


    //用这个接口创建线程，会添加异常处理
    public static Thread SafeCreateThread(ThreadStart fun)
    {
        var thread = new System.Threading.Thread(() =>
        {
            try
            {
                fun();
            }
            catch (System.Exception e)
            {
                SafeLogError("error:{0} StackTrace:{1}", e.Message, e.StackTrace);
            }
        });
        thread.IsBackground = true;//设置为后台线程，主线程退出后这个线程自动退出
        return thread;
    }

    [System.Obsolete("不建议以这种方式关闭线程，建议让线程自行关闭，参考见LogUtil.Close")]
    public static void SafeAbortThread(Thread t)
    {
        if (t != null && t.IsAlive)
        {
            try
            {
                t.Abort();
                //t.Join();//阻止调用线程直到线程终止，同时继续执行标准的 COM 和 SendMessage 传送。如果不调用这一行有可能会导致unity卡死(一直while的时候),TMD,调用了这一行unity也会卡死(有sleep的时候)

            }
            catch (System.Exception e)
            {
                Debuger.LogError("error:{0} StackTrace:{1}", e.Message, e.StackTrace);
            }
        }
    }

    //其他线程要打印信息的时候
    public static void SafeLogError(string format, params object[] args)
    {
        Debuger.LogError(string.Format(format, args));
    }

    //其他线程希望在主线程做一件事的时候
    public static void SafeDo(System.Action a)
    {
        //TimeMgr.instance.AddTimer(0f, a);
    }

    public static long SecToTick(float sce)
    {
        return ((long)(sce * 1000f)) * 10000;//System.TimeSpan.TicksPerSecond
    }

    public static byte[] StringToBytes(string src)
    {
        return System.Text.Encoding.UTF8.GetBytes(src);
    }

    public static string BytesToString(byte[] src)
    {
        return System.Text.Encoding.UTF8.GetString(src);
    }



    public static float XZSqrMagnitude(Vector3 a, Vector3 b)
    {
        float dx = b.x - a.x;
        float dz = b.z - a.z;
        return dx * dx + dz * dz;
    }
    public static float XZMagnitude(Vector3 a, Vector3 b)
    {
        float dx = b.x - a.x;
        float dz = b.z - a.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }
    public static float SqrMagnitude(Vector3 a, Vector3 b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        float dz = b.z - a.z;
        return dx * dx + dz * dz + dy * dy;
    }

    //可以传进来Action或者Func
    public static string GetDelegateName(System.Delegate d)
    {
        if (d == null) return "";
        return GetMethodName(d.Method);
    }

    public static string GetMethodName(MethodInfo info)
    {
        if (info == null) return "";

        return string.Format("{0}.{1}", info.ReflectedType.Name, info.Name);//类名.方法名
    }
    public static void Copy(object from, object to, params object[] ingoreFields)
    {
        Copy(from, to, BindingFlags.Public | BindingFlags.Instance, ingoreFields);
    }
    //复制属性,只复制值类型和枚举
    public static void Copy(object from, object to, BindingFlags bindingAttr, params object[] ingoreFields)
    {
        System.Type toType = to.GetType();
        System.Type fromType = from.GetType();
        if (toType != fromType && !toType.IsSubclassOf(fromType))
        {
            Debuger.LogError("同一个类或者子类才能复制属性.from:{0} to:{1}", fromType.Name, toType.Name);
            return;
        }

        FieldInfo[] fromFields = from.GetType().GetFields(bindingAttr);
        FieldInfo[] toFields = toType.GetFields(bindingAttr);

        HashSet<string> set = new HashSet<string>();
        Dictionary<string, FieldInfo> dict = new Dictionary<string, FieldInfo>();

        foreach (object ingoreFiled in ingoreFields)
            set.Add((string)ingoreFiled);
        foreach (FieldInfo f in toFields)
        {
            if (set.Contains(f.Name))
                continue;
            if (!f.FieldType.IsPrimitive && f.FieldType != typeof(string) && !f.FieldType.IsEnum && !f.FieldType.IsValueType)
                continue;
            dict.Add(f.Name, f);
        }

        foreach (FieldInfo f in fromFields)
        {
            FieldInfo toF = dict.Get(f.Name);
            if (toF == null) continue;
            toF.SetValue(to, f.GetValue(from));
        }
    }

    
    //投影
    public static Vector2 Project(Vector2 vector, Vector2 onNormal)
    {
        if (onNormal == Vector2.zero)
            return Vector2.zero;


        //return (Vector2.Dot(vector,onNormal)/onNormal.magnitude)*onNormal.normalized;
        float num = Vector2.Dot(onNormal, onNormal);
        if (num < Mathf.Epsilon)
            return Vector2.zero;
        return onNormal * Vector2.Dot(vector, onNormal) / num;
    }

    public static int guidNumberSeed = 0;

    public static string GenerateGUID()
    {
        guidNumberSeed = guidNumberSeed > 16777216 ? 0 : guidNumberSeed + 1;
        string svrIdPart = System.Convert.ToString(0, 16);  //0表示非服务端了
        string timePart = System.Convert.ToString(TimeMgr.instance.GetTrueTimestampMS(), 16);
        string numPart = System.Convert.ToString(guidNumberSeed, 16);
        return svrIdPart + "_" + timePart + "_" + numPart;
    }

    public static string GetNetworkType()
    {
        NetworkReachability r = Application.internetReachability;
        string n = "";
        switch (r)
        {
            case NetworkReachability.ReachableViaLocalAreaNetwork:
                {
                    n = "WIFI";
                }
                break;
            case NetworkReachability.ReachableViaCarrierDataNetwork:
                {
                    n = "MOBILE";
                }
                break;
            case NetworkReachability.NotReachable:
                {
                    n = "NONE";
                }
                break;
        }
        return n;
    }

    public static Dictionary<T, T> ArrayToDictionary<T>(T[] arr)
    {
        var d = new Dictionary<T, T>();
        if (arr == null || arr.Length == 0)
            return d;
        for (int i = 0, len = arr.Length / 2; i < len; ++i)
        {
            T n1 = arr[i * 2 + 0];
            T n2 = arr[i * 2 + 1];
            d[n1] = n2;
        }
        return d;
    }

    public static HashSet<T> ArrayToSet<T>(T[] arr)
    {
        var d = new HashSet<T>();
        if (arr == null)
            return d;
        for (int i = 0, len = arr.Length; i < len; ++i)
        {
            d.Add(arr[i]);
        }
        return d;
    }

    //在一个list里随机出几个不重复的元素
    public static List<T> GetRandomElements<T>(List<T> ls, int num) where T : new()
    {
        List<T> retLs = new List<T>();
        if (num >= ls.Count)
        {
            return ls;
        }

        int randomIndex = 0, count = ls.Count;
        T temp;
        for (int i = 0; i < num; i++)
        {
            randomIndex = Random(0, count);
            retLs.Add(ls[randomIndex]);

            //进行交换
            if (randomIndex != count - 1)
            {
                temp = ls[randomIndex];
                ls[randomIndex] = ls[count - 1];
                ls[count - 1] = temp;
            }
            count--;
        }
        return retLs;
    }

    public static T LoadJsonFile<T>(string filePath)
    {
        TextAsset ta = Resources.Load<TextAsset>(filePath);
        if (ta != null)
        {
            try
            {
                return JsonMapper.ToObject<T>(ta.text);
            }
            catch(System.Exception e)
            {
                Debug.LogError("解析json文件 " + filePath + "出错 可能冲突");
                return default(T);
            }
        }

        return default(T);
    }

    public static void SaveJsonFile(string filePath, object obj)
    {
        string path = Application.dataPath + "/Config/Resources/" + filePath + ".json";
        System.IO.File.WriteAllText(path, JsonMapper.ToJson(obj), System.Text.Encoding.UTF8);
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    public static void RemoveJsonFile(string filePath)
    {
        string path = Application.dataPath + "/Config/Resources/" + filePath + ".json";
        if (!System.IO.File.Exists(path))
            return;
        System.IO.File.Delete(path);
        while (System.IO.File.Exists(path));
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        UnityEditor.AssetDatabase.SaveAssets();
#endif
    }

    public static bool IsDigitsOnly(string str)
    {
        for (var i = 0; i < str.Length; ++i)
        {
            var c = str[i];
            if (c < '0' || c > '9')
                return false;
        }
        return true;
    }


    public static void PutTextToClipboard(string str)
    {
        var te = new UnityEngine.TextEditor();
        te.text = str;
        te.SelectAll();
        te.Copy();
    }

    public static string GetTextFromClipboard()
    {
        var te = new UnityEngine.TextEditor();
        te.Paste();
        return te.text;
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T t = a;
        a = b;
        b = t;
    }


    public static string GetEnumDesc(System.Enum obj)
    {
        string objName = obj.ToString();
        System.Type t = obj.GetType();
        FieldInfo fi = t.GetField(objName);
        System.ComponentModel.DescriptionAttribute[] arrDesc = (System.ComponentModel.DescriptionAttribute[])fi.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
        return arrDesc != null && arrDesc.Length > 0 ? arrDesc[0].Description : "";
    }

    public static string[] GetEnumAllDesc<T>()
    {
        System.Type t = typeof(T);

        if (!t.IsEnum)
        {
            Debuger.LogError("模板参数必须是枚举");
            return new string[] { };
        }

        var names = System.Enum.GetNames(t);
        var descs = new string[names.Length];

        for (int i = 0, len = names.Length; i < len; ++i)
        {
            var name = names[i];
            FieldInfo fi = t.GetField(name);
            System.ComponentModel.DescriptionAttribute[] arrDesc = (System.ComponentModel.DescriptionAttribute[])fi.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
            descs[i] = arrDesc != null && arrDesc.Length > 0 ? arrDesc[0].Description : "";
        }

        return descs;
    }

	private static List<string> luaPaths = new List<string>();

	public static int Int(object o)
	{
		return Convert.ToInt32(o);
	}

	public static float Float(object o)
	{
		return (float)Math.Round(Convert.ToSingle(o), 2);
	}

	public static long Long(object o)
	{
		return Convert.ToInt64(o);
	}

	public static int Random(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static float Random(float min, float max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	public static string Uid(string uid)
	{
		int position = uid.LastIndexOf('_');
		return uid.Remove(0, position + 1);
	}

	public static long GetTime()
	{
		TimeSpan ts = new TimeSpan(DateTime.UtcNow.Ticks - new DateTime(1970, 1, 1, 0, 0, 0).Ticks);
		return (long)ts.TotalMilliseconds;
	}

	/// <summary>
	/// 搜索子物体组件-GameObject版
	/// </summary>
	public static T Get<T>(GameObject go, string subnode) where T : Component
	{
		if (go != null)
		{
			Transform sub = go.transform.FindChild(subnode);
			if (sub != null) return sub.GetComponent<T>();
		}
		return null;
	}

	/// <summary>
	/// 搜索子物体组件-Transform版
	/// </summary>
	public static T Get<T>(Transform go, string subnode) where T : Component
	{
		if (go != null)
		{
			Transform sub = go.FindChild(subnode);
			if (sub != null) return sub.GetComponent<T>();
		}
		return null;
	}

	/// <summary>
	/// 搜索子物体组件-Component版
	/// </summary>
	public static T Get<T>(Component go, string subnode) where T : Component
	{
		return go.transform.FindChild(subnode).GetComponent<T>();
	}

	/// <summary>
	/// 添加组件
	/// </summary>
	public static T Add<T>(GameObject go) where T : Component
	{
		if (go != null)
		{
			T[] ts = go.GetComponents<T>();
			for (int i = 0; i < ts.Length; i++)
			{
				if (ts[i] != null) GameObject.Destroy(ts[i]);
			}
			return go.gameObject.AddComponent<T>();
		}
		return null;
	}

	/// <summary>
	/// 添加组件
	/// </summary>
	public static T Add<T>(Transform go) where T : Component
	{
		return Add<T>(go.gameObject);
	}

	/// <summary>
	/// 查找子对象
	/// </summary>
	public static GameObject Child(GameObject go, string subnode)
	{
		return Child(go.transform, subnode);
	}

	/// <summary>
	/// 查找子对象
	/// </summary>
	public static GameObject Child(Transform go, string subnode)
	{
		Transform tran = go.FindChild(subnode);
		if (tran == null) return null;
		return tran.gameObject;
	}

	/// <summary>
	/// 取平级对象
	/// </summary>
	public static GameObject Peer(GameObject go, string subnode)
	{
		return Peer(go.transform, subnode);
	}

	/// <summary>
	/// 取平级对象
	/// </summary>
	public static GameObject Peer(Transform go, string subnode)
	{
		Transform tran = go.parent.FindChild(subnode);
		if (tran == null) return null;
		return tran.gameObject;
	}

	/// <summary>
	/// 计算字符串的MD5值
	/// </summary>
	public static string md5(string source)
	{
		MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
		byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
		byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
		md5.Clear();

		string destString = "";
		for (int i = 0; i < md5Data.Length; i++)
		{
			destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
		}
		destString = destString.PadLeft(32, '0');
		return destString;
	}

	/// <summary>
	/// 计算文件的MD5值
	/// </summary>
	public static string md5file(string file)
	{
		try
		{
			FileStream fs = new FileStream(file, FileMode.Open);
			System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(fs);
			fs.Close();

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		}
		catch (Exception ex)
		{
			throw new Exception("md5file() fail, error:" + ex.Message);
		}
	}

	/// <summary>
	/// 清除所有子节点
	/// </summary>
	public static void ClearChild(Transform go)
	{
		if (go == null) return;
		for (int i = go.childCount - 1; i >= 0; i--)
		{
			GameObject.Destroy(go.GetChild(i).gameObject);
		}
	}

	/// <summary>
	/// 清理内存
	/// </summary>
	public static void ClearMemory()
	{
		GC.Collect(); Resources.UnloadUnusedAssets();
		LuaManager mgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
		if (mgr != null) mgr.LuaGC();
	}

	/// <summary>
	/// 取得数据存放目录
	/// </summary>
	public static string DataPath
	{
		get
		{
			string game = AppConst.AppName.ToLower();
			if (Application.isMobilePlatform)
			{
				return Application.persistentDataPath + "/" + game + "/";
			}
			if (AppConst.DebugMode)
			{
				return Application.dataPath + "/" + AppConst.AssetDir + "/";
			}
			if (Application.platform == RuntimePlatform.OSXEditor)
			{
				int i = Application.dataPath.LastIndexOf('/');
				return Application.dataPath.Substring(0, i + 1) + game + "/";
			}
			return "c:/" + game + "/";
		}
	}

	public static string GetRelativePath()
	{
		if (Application.isEditor)
			return "file://" + System.Environment.CurrentDirectory.Replace("\\", "/") + "/Assets/" + AppConst.AssetDir + "/";
		else if (Application.isMobilePlatform || Application.isConsolePlatform)
			return "file:///" + DataPath;
		else // For standalone player.
			return "file://" + Application.streamingAssetsPath + "/";
	}

	/// <summary>
	/// 取得行文本
	/// </summary>
	public static string GetFileText(string path)
	{
		return File.ReadAllText(path);
	}

	/// <summary>
	/// 网络可用
	/// </summary>
	public static bool NetAvailable
	{
		get
		{
			return Application.internetReachability != NetworkReachability.NotReachable;
		}
	}

	/// <summary>
	/// 是否是无线
	/// </summary>
	public static bool IsWifi
	{
		get
		{
			return Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;
		}
	}

	/// <summary>
	/// 应用程序内容路径
	/// </summary>
	public static string AppContentPath()
	{
		string path = string.Empty;
		switch (Application.platform)
		{
			case RuntimePlatform.Android:
				path = "jar:file://" + Application.dataPath + "!/assets/";
				break;
			case RuntimePlatform.IPhonePlayer:
				path = Application.dataPath + "/Raw/";
				break;
			default:
				path = Application.dataPath + "/" + AppConst.AssetDir + "/";
				break;
		}
		return path;
	}

	public static void Log(string str)
	{
		Debug.Log(str);
	}

	public static void LogWarning(string str)
	{
		Debug.LogWarning(str);
	}

	public static void LogError(string str)
	{
		Debug.LogError(str);
	}

	/// <summary>
	/// 防止初学者不按步骤来操作
	/// </summary>
	/// <returns></returns>
	public static int CheckRuntimeFile()
	{
		if (!Application.isEditor) return 0;
		string streamDir = Application.dataPath + "/StreamingAssets/";
		if (!Directory.Exists(streamDir))
		{
			return -1;
		}
		else
		{
			string[] files = Directory.GetFiles(streamDir);
			if (files.Length == 0) return -1;

			if (!File.Exists(streamDir + "files.txt"))
			{
				return -1;
			}
		}
		string sourceDir = Application.dataPath + "/ToLua/Source/Generate/";
		if (!Directory.Exists(sourceDir))
		{
			return -2;
		}
		else
		{
			string[] files = Directory.GetFiles(sourceDir);
			if (files.Length == 0) return -2;
		}
		return 0;
	}

	/// <summary>
	/// 执行Lua方法
	/// </summary>
	public static object[] CallMethod(string module, string func, params object[] args)
	{
		LuaManager luaMgr = AppFacade.Instance.GetManager<LuaManager>(ManagerName.Lua);
		if (luaMgr == null) return null;
		return luaMgr.CallFunction(module + "." + func, args);
	}

	/// <summary>
	/// 检查运行环境
	/// </summary>
	public static bool CheckEnvironment()
	{
#if UNITY_EDITOR
		int resultId = Util.CheckRuntimeFile();
		if (resultId == -1)
		{
			Debug.LogError("没有找到框架所需要的资源，单击Game菜单下Build xxx Resource生成！！");
			EditorApplication.isPlaying = false;
			return false;
		}
		else if (resultId == -2)
		{
			Debug.LogError("没有找到Wrap脚本缓存，单击Lua菜单下Gen Lua Wrap Files生成脚本！！");
			EditorApplication.isPlaying = false;
			return false;
		}
		if (Application.loadedLevelName == "Test" && !AppConst.DebugMode)
		{
			Debug.LogError("测试场景，必须打开调试模式，AppConst.DebugMode = true！！");
			EditorApplication.isPlaying = false;
			return false;
		}
#endif
		return true;
	}
}
