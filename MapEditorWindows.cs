/** *****************************Description:地图编辑器界面*****************************
*Copyright(C) 2019 by DefaultCompany
*All rights reserved.
*ProductName:  Dxcb3
*Author:       futf-Tony
*Version:      1.0
*UnityVersion: 2018.4.0f1
*CreateTime:   2019/08/02 09:42:07
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;//引用这个命名空间是用于接下来用可变的字符串的
using System.Text;//引用这个命名空间是用于接下来用可变的字符串的
using UnityEngine.UI;
using System;
using System.Threading.Tasks;

// using Bolt;
// using Ludiq;
// using Ludiq.Bolt;
// using Bolt.Generated;
public class MapEditorWindows : EditorWindow
{
    #region 变量
    // static Component[] copiedComponents;
    // public Rect windowRect = new Rect(0, 0, 200, 200);//子窗口的大小和位置
    string m_input = "Level1";
    Vector2Int m_width_hight;
    Vector2Int m_BorderPos1;
    Vector2Int m_BorderPos2;
    GameObject m_obj;
    static MapEditorWindows myWindow;
    bool showBtn = true;//单选框
    float nowX = 0;
    float nowY = 0;
    Grid grid;
    Vector3Int v3I = new Vector3Int(0, 0, 0);
    Vector2Int pos;
    bool m_snap = false;//开关网格捕捉
    #endregion
    [MenuItem("⚙事件编辑器/⚙编辑器窗口/🔍打开窗口 &F1", false, 1)] // & alt  #shift %ctrl
    static void Copy()
    {
        myWindow = (MapEditorWindows)EditorWindow.GetWindow(typeof(MapEditorWindows), false, "地图编辑器", true);//创建窗口
        myWindow.Show();//展示

        SelectHierarchyObj();
    }

    private void Update()
    {
        //     Selection.selectionChanged = delegate
        // {
        //     if (Selection.gameObjects.Length > 0)
        //     {
        //         Debug.Log(Selection.gameObjects[0].name);
        //         //这里我做了改动
        //     }
        // };

        if (Selection.gameObjects.Length > 0 && m_snap)
        {
            // Debug.Log(Selection.transforms[0].name);
            if (nowX != Selection.transforms[0].position.x || nowY != Selection.transforms[0].position.y)
            {
                Grid grid = GameObject.Find("MapData").GetComponent<Grid>();
                Vector3Int gridpos = grid.LocalToCell((Vector3)Selection.transforms[0].position);
                pos.x = gridpos.x;
                pos.y = gridpos.y;
                v3I.x = gridpos.x;
                v3I.y = gridpos.y;
                v3I.z = 0;
                Selection.transforms[0].position = grid.CellToLocal(v3I);
            }
            //上一帧的坐标和这帧的坐标不同的话
            nowX = Selection.transforms[0].position.x;
            nowY = Selection.transforms[0].position.y;
        }
    }
    void OnGUI()
    {
        //标题
        {
            GUILayout.Space(10);
            GUI.skin.label.fontSize = 24;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("地图导出工具");
        }
        //地图数据相关:
        {
            GUI.skin.label.fontSize = 16;
            GUI.skin.label.alignment = TextAnchor.UpperLeft;
            GUILayout.Label("地图数据相关:");

            GUILayout.Space(5);
            m_input = EditorGUILayout.TextField("关卡名：", m_input);//文本输入
            GUILayout.Space(5);
            m_width_hight = EditorGUILayout.Vector2IntField("地图宽高：", m_width_hight);//文本输入
            GUILayout.Space(5);
            m_BorderPos1 = EditorGUILayout.Vector2IntField("边界起点：", m_BorderPos1);//文本输入
            GUILayout.Space(5);
            m_BorderPos2 = EditorGUILayout.Vector2IntField("边界终点：", m_BorderPos2);//文本输入

            GUILayout.Space(5);
            if (GUILayout.Button("确认，设置地图信息"))
            {
                //把m_width_hight传给a星寻路的格子大小内
                {
                    ChangeLevelName();
                }
                OnTip("设置地图信息-完成");
            }
            GUILayout.Space(5);

            GUILayout.Space(5);
            if (GUILayout.Button("Objects渲染动态对象的位置"))
            {
                Transform but = GameObject.Find("Objects").transform;
                //       /**=======foreach遍历所有的子物体以及孙物体，并且遍历包含本身=======
                //       * @param m_list:列表数组
                //       *
                //   */
                foreach (Transform child in but.GetComponentsInChildren<Transform>(true))
                {
                    if (child.GetComponent<DynamicBlocks>() && child.gameObject.active == true)
                    {
                        child.GetComponent<DynamicBlocks>().CallDraw();
                    }
                }
                OnTip("渲染-完成");
            }

            {
                GUILayout.Space(5);
                if (GUILayout.Button("保存服务端地图数据"))
                {
                    ChangeLevelName();//修改关卡名
                    SelectHierarchyObj();//选择实例面板对象
                    #region /*------------------------------------------延迟功能Start*/
                    //命名空间：System.Threading.Tasks;
                    System.Func<Task> func = async () =>
                    {
                        await Task.Delay(System.TimeSpan.FromSeconds(2));
                        //Debug.Log("延时-------+++++");
                        //需要延迟执行的方法体...
                        SaveFileServer();
                        OnTip("保存服务端数据-完成");
                    };
                    func();
                    #endregion/*------------------------------------------延迟功能End*/
                };
                GUILayout.Space(5);
                if (GUILayout.Button("保存客户端地图数据"))
                {
                    ChangeLevelName();//修改关卡名
                    SelectHierarchyObj();//选择实例面板对象
                                         //命名空间：System.Threading.Tasks;
                    System.Func<Task> func = async () =>
                    {
                        await Task.Delay(System.TimeSpan.FromSeconds(2));
                        //Debug.Log("延时-------+++++");
                        //需要延迟执行的方法体...
                        SaveFileClient();
                        OnTip("保存客户端数据-完成");
                    };
                    func();

                };
            }

            {
                GUILayout.Space(5);

                if (GUILayout.Button("ServerMerge合并服务端地图数据"))
                {
                    //Debug.Log("ServerMerge合并服务端地图数据");
                    JObject JObject3 = new JObject();

                    DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/Data/");
                    FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories); //包括子目录
                    for (int i = 0; i < files.Length; i++)//遍历所有关卡json文件，读取文件数据，合并json对象
                    {
                        if (files[i].Name.EndsWith(".json"))
                        {
                            #region /*------------------------------------------7、读取.Json文件并转成对象（JToken能转字符串和）*/
                            //FileMode.Open打开路径下的save.text文件
                            FileStream fs3 = files[i].Open(FileMode.Open);//7、读取.Json文件并转成对象
                            byte[] bytes3 = new byte[fs3.Length];//7、读取.Json文件并转成对象
                            fs3.Read(bytes3, 0, bytes3.Length);//7、读取.Json文件并转成对象
                                                               //将读取到的二进制转换成字符串
                            string s = new UTF8Encoding().GetString(bytes3);//7、读取.Json文件并转成对象
                                                                            //1、创建json雏形(可以通过解析字符串方式给对象赋值所有数据，包括数组(“单引号表示对象名称“表示被Parse读取的信息，”要改成英文格式)
                            JObject _jObject = JObject.Parse(s);
                            //Split分割字符串
                            string lvname = (files[i].Name).Split('.')[0];
                            JObject3.Add(lvname, JObject.Parse(s)[lvname]);
                            #endregion  /*------------------------------------------7、读取.Json文件并转成对象（JToken能转字符串和）END*/    
                        }
                    }

                    //这里的FileMode.create是创建这个文件,如果文件名存在则覆盖重新创建
#if NETFX_CORE
		throw new System.NotSupportedException("Cannot save to file on this platform");
#else
                    using (var fs2 = new FileStream(Application.streamingAssetsPath + "/test.json", FileMode.Create))
                    {
                        //存储时时二进制,所以这里需要把我们的字符串转成二进制
                        //压缩json，序列化map_JObject ，可以删除空格和换行减少json文件的大小
                        string st = JsonConvert.SerializeObject(JObject3);//_jObject可以是JObject
                        byte[] bytes = new UTF8Encoding().GetBytes(st);
                        fs2.Write(bytes, 0, bytes.Length);
                        //每次读取文件后都要记得关闭文件
                        fs2.Close();
                    }
                    UnityEditor.AssetDatabase.Refresh();//刷新项目文件夹
#endif
                    OnTip("合并服务端数据-完成");
                }
            }
            {
                GUILayout.Space(5);
                if (GUILayout.Button("一键导出"))
                {
                    OneKeyExp();
                }

            }
        }

        //美术资源给客户端:
        {
            GUILayout.Space(10);
            GUILayout.Label("美术资源给客户端:");

            GUILayout.Space(5);
            if (GUILayout.Button("自动规范DynamicObj文件夹内的预制"))
            {
                //Objects自动规范spine对象改名添加碰撞
                //Debug.Log("这里查找文件夹内的预制，并批量修改预制件保存");
                //https://blog.csdn.net/qq_30585525/article/details/78865802

                string fullPath = Application.dataPath + "/Art/Map/DynamicObj/";
                //Debug.Log(fullPath);
                //获得指定路径下面的所有资源文件

                if (Directory.Exists(fullPath))
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(fullPath);
                    FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories); //包括子目录
                                                                                           //Debug.Log(files.Length);
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (files[i].Name.EndsWith(".prefab"))
                        {
                            //Debug.Log("预制体名字" + files[i].Name);
                            string path = "Assets/Art/Map/DynamicObj/" + files[i].Name;
                            //Debug.Log("预制体路径" + path);
                            GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                            OnTip("导出-完成，路径：" + path);
                            if (obj.transform.GetComponentInChildren<MeshRenderer>())
                            {
                                //Debug.Log("name:" + obj.transform.name);
                                MeshRenderer[] labels = obj.transform.GetComponentsInChildren<MeshRenderer>(true);
                                foreach (MeshRenderer lab in labels)
                                {
                                    //Debug.Log("LAB:" + lab.gameObject.name);
                                    if (lab.gameObject.GetComponent<PolygonCollider2D>() == null)
                                    {
                                        lab.gameObject.AddComponent<PolygonCollider2D>();
                                    }
                                    lab.name = "ani";
                                }

                            }
                            else
                            {
                                //Debug.Log("name:" + obj.transform.name);
                                SpriteRenderer[] labels2 = obj.transform.GetComponentsInChildren<SpriteRenderer>(true);
                                foreach (SpriteRenderer lab in labels2)
                                {
                                    //Debug.Log("LAB:" + lab.gameObject.name);
                                    if (lab.gameObject.GetComponent<PolygonCollider2D>() == null)
                                    { lab.gameObject.AddComponent<PolygonCollider2D>(); }
                                    lab.name = "sprite";
                                }
                            }
                            //通知你的编辑器 obj 改变了
                            EditorUtility.SetDirty(obj);
                        }
                    }
                    //保存修改
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                else
                {
                    //Debug.Log("资源路径不存在");
                }
                OnTip("自动规范-完成");
            }

            GUILayout.Space(5);
            if (GUILayout.Button("导出给程序的scene预制"))
            {
                // string[] str = prefabName.Split('(');
                string path = "Assets/Art/Map/Level/" + m_input + "/" + "scene.prefab";
                GameObject scene;
                if (GameObject.Find("scene"))
                {
                    scene = GameObject.Find("scene");
                }
                else
                {
                    scene = new GameObject("scene");
                }
                Transform sceneT = scene.transform;
                if (GameObject.Find("sprite") && GameObject.Find("tilemap"))
                {

                    GameObject.Find("sprite").transform.parent = sceneT;
                    GameObject.Find("tilemap").transform.parent = sceneT;
                }

                Directory.CreateDirectory(Application.dataPath + "/Art/Map/Level/" + m_input);//动态创建文件夹

                //参数1 创建路径，参数2 需要创建的对象， 如果路径下已经存在该名字的prefab，则覆盖
                PrefabUtility.CreatePrefab(path, scene);
                //原文链接：https://blog.csdn.net/LIQIANGEASTSUN/article/details/42124469
                UnityEditor.AssetDatabase.Refresh();//刷新项目文件夹

                // 1、实例化Assets/Art/Map/Level/Level1/scene.prefab 并返回Obj
                // 2、解耦Obj中的预制
                // 3、再保存Obj预制到 string path2 = "Assets/Art/Map/Level/" + m_input + "/scene.prefab";
                // 4、删除实例Obj
                {
                    //1、
                    var newPrefab = Instantiate(AssetDatabase.LoadAssetAtPath(path, typeof(GameObject))) as GameObject;

                    //2、
                    if (PrefabUtility.IsOutermostPrefabInstanceRoot(newPrefab.transform.Find("sprite/event").gameObject))
                    {
                        PrefabUtility.UnpackPrefabInstance(newPrefab.transform.Find("sprite/event").gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                        PrefabUtility.UnpackPrefabInstance(newPrefab.transform.Find("sprite/tilemap").gameObject, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                    }
                    //3、
                    PrefabUtility.SaveAsPrefabAsset(newPrefab, path);
                    //4、
                    GameObject.DestroyImmediate(newPrefab, true);
                }

                //按规范删除Assets/Art/Map/Level/Level1/scene.prefab下sprite下的event内的对象
                {
                    string path2 = "Assets/Art/Map/Level/" + m_input + "/scene.prefab";
                    //Debug.Log("预制体路径" + path2);
                    GameObject obj3 = AssetDatabase.LoadAssetAtPath<GameObject>(path2);

                    //按规范删除sprite下的event内的对象
                    Transform eventObjs = obj3.transform.Find("sprite/event");

                    for (int iiii = 0; iiii < 99; iiii++)
                    {
                        if (eventObjs.childCount > 0)
                        {
                            GameObject.DestroyImmediate(eventObjs.GetChild(0).gameObject, true);
                        }
                        else
                        {
                            break;
                        }
                    }
                    EditorUtility.SetDirty(obj3);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
                OnTip("导出-完成，路径：" + path);
            };

            GUILayout.Space(5);
            // showBtn = EditorGUILayout.ToggleLeft("开关网格捕捉功能", showBtn);
            if (GUILayout.Button("开关网格捕捉功能"))
            {
                // GameObject[] obj = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
                // foreach (GameObject child in obj)    //遍历所有gameobject
                // {
                //     //=======类型判空undefined=======
                //     if (child.GetComponent<DynamicBlocks>())
                //     {
                //         child.GetComponent<DynamicBlocks>().m_snap = !child.GetComponent<DynamicBlocks>().m_snap; ;
                //         if (child.GetComponent<DynamicBlocks>().m_snap)
                //         {
                //             OnTip("捕捉-开");
                //         }
                //         else
                //         {
                //             OnTip("捕捉-关");
                //         }

                //     }
                // }
                m_snap = !m_snap;
                if (m_snap)
                {
                    OnTip("捕捉-开");
                }
                else
                {
                    OnTip("捕捉-关");
                }

            }


            #region 注释
            // if (GUILayout.Button("删除scene/sprite/event内的物体"))
            // {
            // {
            //   // #region /*------------------------------------------.prefab这里查找文件夹内的预制，并批量修改预制件保存*/
            //   //https://blog.csdn.net/qq_30585525/article/details/78865802
            //   string fullPath2 = Application.dataPath + "/Art/Map/Level/" + m_input + "/";
            //  //Debug.Log(fullPath2);
            //   //获得指定路径下面的所有资源文件
            //   if (Directory.Exists(fullPath2))
            //   {
            //     DirectoryInfo dirInfo2 = new DirectoryInfo(fullPath2);
            //     FileInfo[] files3 = dirInfo2.GetFiles("*", SearchOption.AllDirectories); //包括子目录
            //    //Debug.Log(files3.Length);
            //     for (int ii = 0; ii < files3.Length; ii++)
            //     {
            //       if (files3[ii].Name.EndsWith(".prefab"))
            //       {
            //        //Debug.Log("预制体名字" + files3[ii].Name);
            //         string path2 = "Assets/Art/Map/Level/" + m_input + "/" + files3[ii].Name;
            //        //Debug.Log("预制体路径" + path2);
            //         GameObject obj3 = AssetDatabase.LoadAssetAtPath(path2, typeof(GameObject)) as GameObject;

            //         //按规范删除sprite下的event内的对象
            //         Transform eventObjs = obj3.transform.Find("sprite/event");
            //         for (int iiii = 0; iiii < eventObjs.childCount; iiii++)
            //         {
            //           GameObject.DestroyImmediate(eventObjs.GetChild(iiii).gameObject, true);
            //         }

            //         // GameObject event3 = new GameObject("event");
            //         // event3.transform.parent = sceneT.Find("sprite");

            //         //通知你的编辑器 obj 改变了
            //         EditorUtility.SetDirty(obj3);
            //         //保存修改
            //         // AssetDatabase.SaveAssets();
            //         // AssetDatabase.Refresh();
            //       }
            //     }
            //   }
            //   else
            //   {
            //    //Debug.Log("资源路径不存在");
            //   }
            //   // #endregion  /*------------------------------------------.prefab这里查找文件夹内的预制，并批量修改预制件保存END*/
            // }
            // };

            #endregion
        }
        //制作中
        {
            GUILayout.Space(10);
            GUILayout.Label("制作中的功能:");

            GUILayout.Space(5);
            if (GUILayout.Button("创建地图关卡"))
            {
                OnTip("正在研发中");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("载入地图关卡"))
            {
                OnTip("正在研发中");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("保存地图关卡"))
            {
                OnTip("正在研发中");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("管理地图关卡"))
            {
                OnTip("正在研发中");
            }
            GUILayout.Space(5);
            if (GUILayout.Button("动态阻挡红色块和轴心不对齐"))
            {
                OnTip("正在研发中");
            }
        }

        //进度条测试
        {
            //终点就两句
            //EditorUtility.DisplayProgressBar("22", "啊啊", 0.2f);
            //EditorUtility.ClearProgressBar(); //清空进度条的值， 基本没什么用
            ////////////////////////////////////////////////////////////////////
            // float secs = 10.0f;
            // double startVal = 0;
            // double progress = 0;
            // bool isShow = false;
            // secs = EditorGUILayout.FloatField("Time to wait:", secs);
            // if (GUILayout.Button("Display bar"))
            // {
            //     startVal = EditorApplication.timeSinceStartup; //开始编译到现在的时间
            //     isShow = !isShow;
            // }

            // if (GUILayout.Button("Clear bar"))
            // {
            //     EditorUtility.ClearProgressBar(); //清空进度条的值， 基本没什么用
            // }
            // if (GUILayout.Button("50"))
            // {

            //     EditorUtility.DisplayProgressBar("22", "啊啊", 10.0f);
            // }
            // if (progress < secs && isShow == true)
            // {
            //     //使用这句代码，在进度条后边会有一个 关闭按钮，但是用这句话会直接卡死，切记不要用
            //     // EditorUtility.DisplayCancelableProgressBar("Simple Progress Bar", "Show a progress bar for the given seconds", (float)(progress / secs));
            //     //使用这句创建一个进度条，  参数1 为标题，参数2为提示，参数3为 进度百分比 0~1 之间
            //     EditorUtility.DisplayProgressBar("Simple Progress Bar", "Show a progress bar for the given seconds", (float)(progress / secs));
            // }
            // else
            // {
            //     startVal = EditorApplication.timeSinceStartup;
            //     progress = 0.0f;
            //     return;
            // }
            // progress = EditorApplication.timeSinceStartup - startVal;



        }
        #region 注释代码：
        {
            // m_obj = (GameObject)EditorGUILayout.ObjectField("对象传入", m_obj, typeof(GameObject), true);//对象传入
            // if (GUILayout.Button("显示消息"))//消息
            // {
            // OnTip("string text");
            // }

            // BeginWindows();//标记开始区域所有弹出式窗口
            // windowRect = GUILayout.Window(1, windowRect, DoWindow, "子窗口");//创建内联窗口,参数分别为id,大小位置，创建子窗口的组件的函数，标题
            // EndWindows();//标记结束
        }
        #endregion
    }

    /// <summary>
    /// 调用导出客户端数据
    /// </summary>
    // 调用导出客户端数据
    private void SaveFileClient()
    {
        GameObject[] obj3 = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        foreach (GameObject child in obj3)    //遍历所有gameobject
        {
            //Debug.Log(child.gameObject.name);  //可以在unity控制台测试一下是否成功获取所有元素
            if (child.GetComponent<TEXTTILEMAP>() && child.activeSelf)
            {
                child.GetComponent<TEXTTILEMAP>().SaveFileClient();
            }
        }
    }

    /// <summary>
    /// 调用导出服务端数据
    /// </summary>
    // 调用导出服务端数据
    private void SaveFileServer()
    {
        GameObject[] obj2 = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        foreach (GameObject child in obj2)    //遍历所有gameobject
        {
            //Debug.Log(child.gameObject.name);  //可以在unity控制台测试一下是否成功获取所有元素
            if (child.GetComponent<TEXTTILEMAP>() && child.activeSelf)
            {
                child.GetComponent<TEXTTILEMAP>().SaveFileServer();
            }
        }
    }


    /// <summary>
    /// 修改关卡名
    /// </summary>
    // 修改关卡名
    private void ChangeLevelName()
    {
        GameObject[] obj = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        foreach (GameObject child in obj)    //遍历所有gameobject
        {
            //Debug.Log(child.gameObject.name);  //可以在unity控制台测试一下是否成功获取所有元素
            if (child.GetComponent<TEXTTILEMAP>())
            {
                child.GetComponent<TEXTTILEMAP>().m_mapName = m_input;
                child.GetComponent<TEXTTILEMAP>().m_width_Height = m_width_hight;
                child.GetComponent<TEXTTILEMAP>().m_BorderPos1 = m_BorderPos1;
                child.GetComponent<TEXTTILEMAP>().m_BorderPos2 = m_BorderPos2;

                SelectHierarchyObj();//选择实例面板对象
                                     //命名空间：System.Threading.Tasks;
                System.Func<Task> func = async () =>
                {
                    await Task.Delay(System.TimeSpan.FromSeconds(2));
                    //Debug.Log("延时-------+++++");
                    //需要延迟执行的方法体...

                    //设置astar组件上的数据宽高和网格倍数
                    child.GetComponent<AstarPath>().astarData.gridGraph.SetDimensions(m_width_hight.x, m_width_hight.y, 0.2f);
                    child.GetComponent<AstarPath>().astarData.gridGraph.center = new Vector3(m_width_hight.x * 0.1F, m_width_hight.y * 0.1F, 0);

                    #region /*------------------------------------------.绘制边界*/
                    Grid cGrid = child.GetComponent<Grid>();
                    #region /*------------------------------------------.Grid格子坐标转position*/
                    Vector3 posLocal1 = cGrid.CellToLocalInterpolated(new Vector3(m_BorderPos1.x, m_BorderPos1.y, 0));
                    Vector3 posLocal2 = cGrid.CellToLocalInterpolated(new Vector3(m_BorderPos2.x, m_BorderPos2.y, 0));
                    #endregion  /*------------------------------------------.Grid格子坐标转position*/

                    if (child.GetComponent<LineRenderer>())
                    {
                        LineRenderer LR = child.GetComponent<LineRenderer>();
                        Material mat = new Material(Shader.Find("UI/Default"));
                        mat.color = new Color32(0, 212, 8, 255);
                        LR.material = mat;
                        LR.sortingLayerName = "策划备注";
                        LR.sortingOrder = 9999;
                        LR.loop = true;
                        LR.SetWidth(0.2f, 0.2f);

                        LR.positionCount = 5;
                        LR.SetPosition(0, new Vector3(posLocal1.x, posLocal1.y, 0));
                        LR.SetPosition(1, new Vector3(posLocal2.x, posLocal1.y, 0));
                        LR.SetPosition(2, new Vector3(posLocal2.x, posLocal2.y, 0));
                        LR.SetPosition(3, new Vector3(posLocal1.x, posLocal2.y, 0));
                        LR.SetPosition(4, new Vector3(posLocal1.x, posLocal1.y, 0));
                    }
                    else
                    {
                        LineRenderer LR = child.AddComponent<LineRenderer>();
                        Material mat = new Material(Shader.Find("UI/Default"));
                        mat.color = new Color32(0, 212, 8, 255);
                        LR.material = mat;
                        LR.sortingLayerName = "策划备注";
                        LR.sortingOrder = 9999;
                        LR.loop = true;
                        LR.SetWidth(0.2f, 0.2f);

                        LR.positionCount = 5;
                        LR.SetPosition(0, new Vector3(posLocal1.x, posLocal1.y, 0));
                        LR.SetPosition(1, new Vector3(posLocal2.x, posLocal1.y, 0));
                        LR.SetPosition(2, new Vector3(posLocal2.x, posLocal2.y, 0));
                        LR.SetPosition(3, new Vector3(posLocal1.x, posLocal2.y, 0));
                        LR.SetPosition(4, new Vector3(posLocal1.x, posLocal1.y, 0));
                    }
                    #endregion  /*------------------------------------------.绘制边界End*/


                    #region /*------------------------------------------.绘制边界*/
                    #region /*------------------------------------------.Grid格子坐标转position*/
                    Vector3 posLocal = cGrid.CellToLocalInterpolated(new Vector3(m_width_hight.x, m_width_hight.y, 0));
                    #endregion  /*------------------------------------------.Grid格子坐标转position*/

                    if (child.transform.parent.gameObject.GetComponent<LineRenderer>())
                    {
                        LineRenderer LR = child.transform.parent.gameObject.GetComponent<LineRenderer>();
                        Material mat = new Material(Shader.Find("UI/Default"));
                        mat.color = new Color32(135, 2, 0, 255);
                        LR.material = mat;
                        LR.sortingLayerName = "策划备注";
                        LR.sortingOrder = 9999;
                        LR.loop = true;
                        LR.SetWidth(0.2f, 0.2f);

                        LR.positionCount = 5;
                        LR.SetPosition(0, new Vector3(0, 0, 0));
                        LR.SetPosition(1, new Vector3(posLocal.x, 0, 0));
                        LR.SetPosition(2, new Vector3(posLocal.x, posLocal.y, 0));
                        LR.SetPosition(3, new Vector3(0, posLocal.y, 0));
                        LR.SetPosition(4, new Vector3(0, 0, 0));
                    }
                    else
                    {
                        LineRenderer LR = child.transform.parent.gameObject.AddComponent<LineRenderer>();
                        Material mat = new Material(Shader.Find("UI/Default"));
                        mat.color = new Color32(135, 2, 0, 255);
                        LR.material = mat;
                        LR.sortingLayerName = "策划备注";
                        LR.sortingOrder = 9999;
                        LR.loop = true;
                        LR.SetWidth(0.2f, 0.2f);

                        LR.positionCount = 5;
                        LR.SetPosition(0, new Vector3(0, 0, 0));
                        LR.SetPosition(1, new Vector3(posLocal.x, 0, 0));
                        LR.SetPosition(2, new Vector3(posLocal.x, posLocal.y, 0));
                        LR.SetPosition(3, new Vector3(0, posLocal.y, 0));
                        LR.SetPosition(4, new Vector3(0, 0, 0));
                    }
                    #endregion  /*------------------------------------------.绘制边界End*/
                };
                func();

            }
        }

        //自动设置event的层级，不让被静态阻挡扫描
        GameObject eventObj = GameObject.Find("event");//不扫描event下的碰撞
                                                       /**=======GetComponentsInChildren只遍历所有的子物体，没有孙物体，遍历不包含本身=======
                                                       * @param m_list:列表数组
                                                       *
                                                       */
        foreach (Transform child in eventObj.transform.GetComponentsInChildren<Transform>(true))
        {
            child.gameObject.layer = 0;
        }
    }

    private void OnHierarchyChange()
    {
        //Debug.Log("对象面板事件");
        SaveExpConfig();//自动保存上一次设置的导出配置
    }
    private void OnProjectChange()
    {
        //Debug.Log("文件事件");
    }
    private void OnSelectionChange()
    {
        //Debug.Log("选择事件");
    }
    /// <summary>
    /// 重新绘制改窗口 心跳函数
    /// </summary>
    void OnInspectorUpdate()
    {
        Repaint();

    }

    /// <summary>
    ///自动选择实例化面板的这个物体
    /// </summary>
    public static void SelectHierarchyObj()
    {
        GameObject[] obj5 = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        foreach (GameObject child in obj5)    //遍历所有gameobject
        {
            if (child.GetComponent<AstarPath>())
            {
                EditorGUIUtility.PingObject(child);
                Selection.activeGameObject = child;
            }
        }
    }
    /// <summary>
    /// 延时没用到（运行时能用）
    /// </summary>
    private void SelectHierarchy()
    {
        GameObject[] obj5 = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        foreach (GameObject child in obj5)    //遍历所有gameobject
        {
            if (child.GetComponent<AstarPath>())
            {
                EditorGUIUtility.PingObject(child);
                Selection.activeGameObject = child;
            }
        }

        //延时
        // TimerDelayGraphScript timerDelay = new TimerDelayGraphScript(new TimerDelayMachineScript());
        // timerDelay.TimerStart();
        // GameObject g=GameObject.Find("Tool");
        // CustomEvent.Trigger(g, "testf",null);
    }

    /// <summary>
    /// 自动保存上一次设置的导出配置
    /// </summary>
    /// <param name="parameter"></param>
    private void SaveExpConfig()
    {
        GameObject[] obj = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        foreach (GameObject child in obj)    //遍历所有gameobject
        {
            if (child.GetComponent<TEXTTILEMAP>())
            {
                m_input = child.GetComponent<TEXTTILEMAP>().m_mapName;
                m_width_hight = child.GetComponent<TEXTTILEMAP>().m_width_Height;
                m_BorderPos1 = child.GetComponent<TEXTTILEMAP>().m_BorderPos1;
                m_BorderPos2 = child.GetComponent<TEXTTILEMAP>().m_BorderPos2;
                // SelectHierarchyObj();//选择实例面板对象
                //命名空间：System.Threading.Tasks;
            }
        }
    }


    /// <summary>
    /// 一键导出功能
    /// </summary>
    private void OneKeyExp()
    {
        //1、把m_width_hight传给a星寻路的格子大小内
        {
            ChangeLevelName();
            EditorUtility.DisplayProgressBar("一键导出", "设置地图信息......", 0.2f);
        }
        //2、渲染动态对象暂时不加

        //3、保存服务端数据
        {
            ChangeLevelName();//修改关卡名
            SelectHierarchyObj();//选择实例面板对象
            #region /*------------------------------------------延迟功能Start*/
            //命名空间：System.Threading.Tasks;
            System.Func<Task> func1 = async () =>
            {
                await Task.Delay(System.TimeSpan.FromSeconds(2));
                //Debug.Log("延时-------+++++");
                //需要延迟执行的方法体...
                SaveFileServer();
                EditorUtility.DisplayProgressBar("一键导出", "保存服务端数据......", 0.6f);
            };
            func1();
            #endregion/*------------------------------------------延迟功能End*/
        }

        //4、ServerMerge合并服务端地图数据
        #region /*------------------------------------------延迟功能Start*/
        //命名空间：System.Threading.Tasks;
        System.Func<Task> func3 = async () =>
        {
            await Task.Delay(System.TimeSpan.FromSeconds(12));
            //Debug.Log("延时-------+++++");
            //需要延迟执行的方法体...

            {
                //Debug.Log("ServerMerge合并服务端地图数据");
                JObject JObject3 = new JObject();

                DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath + "/Data/");
                FileInfo[] files = dirInfo.GetFiles("*", SearchOption.AllDirectories); //包括子目录
                for (int i = 0; i < files.Length; i++)//遍历所有关卡json文件，读取文件数据，合并json对象
                {
                    if (files[i].Name.EndsWith(".json"))
                    {
                        #region /*------------------------------------------7、读取.Json文件并转成对象（JToken能转字符串和）*/
                        //FileMode.Open打开路径下的save.text文件
                        FileStream fs3 = files[i].Open(FileMode.Open);//7、读取.Json文件并转成对象
                        byte[] bytes3 = new byte[fs3.Length];//7、读取.Json文件并转成对象
                        fs3.Read(bytes3, 0, bytes3.Length);//7、读取.Json文件并转成对象
                                                           //将读取到的二进制转换成字符串
                        string s = new UTF8Encoding().GetString(bytes3);//7、读取.Json文件并转成对象
                                                                        //1、创建json雏形(可以通过解析字符串方式给对象赋值所有数据，包括数组(“单引号表示对象名称“表示被Parse读取的信息，”要改成英文格式)
                        JObject _jObject = JObject.Parse(s);
                        //Split分割字符串
                        string lvname = (files[i].Name).Split('.')[0];
                        JObject3.Add(lvname, JObject.Parse(s)[lvname]);
                        #endregion  /*------------------------------------------7、读取.Json文件并转成对象（JToken能转字符串和）END*/    
                    }
                }

                //这里的FileMode.create是创建这个文件,如果文件名存在则覆盖重新创建
#if NETFX_CORE
		throw new System.NotSupportedException("Cannot save to file on this platform");
#else
                using (var fs2 = new FileStream(Application.streamingAssetsPath + "/test.json", FileMode.Create))
                {
                    //存储时时二进制,所以这里需要把我们的字符串转成二进制
                    byte[] bytes = new UTF8Encoding().GetBytes(JObject3.ToString());
                    fs2.Write(bytes, 0, bytes.Length);
                    //每次读取文件后都要记得关闭文件
                    fs2.Close();
                }
                UnityEditor.AssetDatabase.Refresh();//刷新项目文件夹
#endif
                OnTip("合并服务端数据-完成");
            }
            EditorUtility.DisplayProgressBar("一键导出", "合并服务端关卡地图json......", 0.8f);
        };
        func3();
        #endregion/*------------------------------------------延迟功能End*/


        //5、保存客户端数据
        {
            ChangeLevelName();//修改关卡名
            SelectHierarchyObj();//选择实例面板对象
                                 //命名空间：System.Threading.Tasks;
            System.Func<Task> func2 = async () =>
            {
                await Task.Delay(System.TimeSpan.FromSeconds(22));
                //Debug.Log("延时-------+++++");
                //需要延迟执行的方法体...
                SaveFileClient();
                EditorUtility.DisplayProgressBar("一键导出", "保存客户端数据......", 1.0f);
                EditorUtility.ClearProgressBar(); //清空进度条的值， 基本没什么用
            };
            func2();
        }


    }

    /// <summary>
    /// 弹出吐司提示框
    /// </summary>
    /// <param name="text">要显示的文本</param>
    private void OnTip(string text)
    {
        myWindow = (MapEditorWindows)EditorWindow.GetWindow(typeof(MapEditorWindows), false, "开", true);//创建窗口
        myWindow.Show();//展示
        myWindow.ShowNotification(new GUIContent(text));
    }

    #region 注释代码

    // private void OnDestroy()
    // {

    // }
    // private void Awake()
    // {

    // }
    // private void Update() {

    // }

    // void DoWindow(int unusedWindowID)
    // {
    //   GUILayout.Button("按钮");//创建button
    //   GUI.DragWindow();//画出子窗口
    // }

    #endregion
}
