/** *****************************Description:地图数据采集工具*****************************
*Copyright(C) 2019 by DefaultCompany
*All rights reserved.
*ProductName:  Dxcb3
*Author:       futf-Tony
*Version:      1.0
*UnityVersion: 2018.4.0f1
*CreateTime:   2019/06/20 14:33:38
*Description:  地图数据采集工具
*/
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;//引用此命名空间是用于数据的写入与读取
using System.Text;//引用这个命名空间是用于接下来用可变的字符串的
using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

// /// <summary>
// /// name: =======获取map对象生成服务端需要的地图数据类=======
// /// </summary>
// [HelpURL("http://www.baidu.com")]
public class TEXTTILEMAP : MonoBehaviour
{
    #region ===================================变量==================================
    /*----------------------------------------------------------------------------------------地图数据采集*/
    // public GameObject m_Map;//地图预制拖到这里来 //需要遍历子物体的母体
    // public TextAsset fs;//这里需要一个json文本存放数据//自动载入=Resources.Load("Assets/Scripts/FutfScripts/Tilemap/test.json")as TextAsset
    // public List<Transform> m_butArray;//遍历的结果数组
    // private List<Vector2> m_blocksArray;//阻挡数组
    // private int m_maxI = 0;//用来比较保存的最大值
    // private int m_maxJ = 0;//用来比较保存的最大值
    // private MapC m_mc = new MapC();
    // public Maps map;//实例化json实体类
    [Tooltip("下面文本填写策划表名：")]
    [Header("文本填写策划表名：")]
    public string m_mapName;//地图名称，跟策划表名对应;
    [Tooltip("地图宽高：")]
    [Header("地图宽高：")]
    public Vector2Int m_width_Height = new Vector2Int(123, 123);//地图宽高;

    [Tooltip("随机区域绘制上去的颜色TileBase：")]
    [Header("随机区域绘制上去的颜色TileBase：")]
    public List<TileBase> m_huang;//随机区域绘制上去的颜色TileBase
    [System.NonSerialized]
    public Grid m_grid = null;

    private JObject _jObject;//json最外层对象

    FileStream fs;//10.读取.Json文件并转成对象
    byte[] bytes;//10.读取.Json文件并转成对象
    StreamReader sr;//10.读取.Json文件并转成对象
    JObject o;//把读取文件流转换成randArea.json数据对象//10.读取.Json文件并转成对象
    List<Vector2Int> areaList;
    [Tooltip("角色出生点：")]
    [Header("角色出生点：")]
    public Vector2Int m_birthPos = new Vector2Int(0, 0);

    [Tooltip("边界起点：")]
    [Header("边界起点：")]
    public Vector2Int m_BorderPos1 = new Vector2Int(0, 0);
    [Tooltip("边界终点：")]
    [Header("边界终点：")]
    public Vector2Int m_BorderPos2 = new Vector2Int(0, 0);
    // [Tooltip("是否保存静态阻挡：")]
    // [Header("是否保存静态阻挡：")]
    [System.NonSerialized]
    public bool isSaveBlock = true;
    // [Tooltip("是否保存随机区域：")]
    // [Header("是否保存随机区域：")]
    [System.NonSerialized]
    public bool isSaveArea = true;

    private bool isClientOrServer;//判断是否点击客户端数据导出或是服务端数据导出
    private GameObject eventObj;
    #endregion//===================================变量==================================

    /// <summary>
    /// 初始化
    /// </summary>
    // 初始化
    private void Iinit()
    {
        //=======类型判空undefined=======
        if (!gameObject.GetComponent<Grid>())
        {
            gameObject.AddComponent<Grid>().cellSize = new Vector3(0.2f, 0.2f, 0);
        }
        m_grid = gameObject.GetComponent<Grid>();
        #region /*------------------------------------------10.读取.Json文件并转成对象（JToken能转字符串和）*/
        if (fs == null)
        {
            // //FileMode.Open打开路径下的save.text文件
            // fs = new FileStream(Application.streamingAssetsPath + "/randArea.json", FileMode.Open);//10.读取.Json文件并转成对象
            // bytes = new byte[] { };//10.读取.Json文件并转成对象
            // fs.Read(bytes, 0, bytes.Length);//10.读取.Json文件并转成对象
            //                                 //将读取到的二进制转换成字符串
            // string s = new UTF8Encoding().GetString(bytes);//10.读取.Json文件并转成对象
            ////Debug.Log("读取本地randArea.json文件:" + s.ToString());//读取本地randArea.json文件
            // sr = new StreamReader(fs);//10.读取.Json文件并转成对象
            // o = (JObject)JToken.ReadFrom(new JsonTextReader(sr));//把读取文件流转换成randArea.json数据对象//10.读取.Json文件并转成对象
            ////Debug.Log("Intel//10.读取.Json文件并转成对象:" + o["Event"]["1"]["Name"]);//Intel//10.读取.Json文件并转成对象
        }
        #endregion  /*------------------------------------------10.读取.Json文件并转成对象（JToken能转字符串和）END*/

        eventObj = GameObject.Find("event");//不扫描event下的碰撞
    }

    /// <summary>
    /// 扫描静态阻挡
    /// </summary>
    /// <returns></returns>
    // 扫描静态阻挡
    public void ScanMap()
    {
        Iinit();
        gameObject.GetComponent<AstarPath>().Scan();

        // yield return new WaitForSeconds(5.0f);
    }

    #region ===================================json创建和地图信息写入==================================
    // [EasyButtons.Button]
    /// <summary>
    /// json创建和地图信息写入
    /// </summary>
    /// <returns></returns>
    // json创建和地图信息写入
    public void GetData(bool isBlock, bool isArea)
    {
        ScanMap();
        isSaveBlock = isBlock;
        isSaveArea = isArea;

        #region /*------------------------------------------使用文本创建json和linq查询*/
        //  "{'Map':{'Name':'Level10','Width':123,'Height':123,'Blocks':[],'RandArea':{'objname1':{'pos':[[0,2],[0,2],[0,2],[0,2]]},'objname2':{'pos':[[0,2],[0,2],[0,2],[0,2]]},'objname3':{'pos':[[0,2],[0,2],[0,2],[0,2]]}},'Objects':{'objname':{'pos':[1,2],'pos2':[],'area':[[1,2],[1,2],[1,2],[1,2]]}},'NodeSize':0.2}}"

        _jObject = JObject.Parse(
       "{" + m_mapName + ":{'Name':'Level','Width':" + m_width_Height.x + ",'Height':" + m_width_Height.y + ",'Blocks':[],'RandArea':{},'Objects':{},'Transition':{},'NodeSize':0,'BirthPos':[],'BorderPos':[]}}"
       );//1、创建json雏形(可以通过解析字符串方式给对象赋值所有数据，包括数组)
         //Debug.Log("_value------:" + _jObject[m_mapName]["Name"].ToString());//2、查询取值 结果 : Level10
         //Debug.Log(_jObject.SelectToken(m_mapName + ".Name"));//3、查询取值 结果 : Level10
        #endregion /*------------------------------------------使用文本创建json和linq查询*/

        #region /*------------------------------------------linq修改json*/
        JObject map_JObject = (JObject)_jObject[m_mapName];//4、获得Map对象

        // map_JObject["Name"] = ((string)map_JObject["Name"]).ToUpper();//5、转换成大写后给Name对象

        map_JObject.Property("NodeSize").Remove();//6、移除对象

        map_JObject.Property("Transition").AddAfterSelf(new JProperty("NodeSize", 0.2));//7、动态添加同级对象和内容

        map_JObject["Name"] = m_mapName;//8、修改json对象Name的值为地图名称

        #endregion /*------------------------------------------linq修改json END*/

        #region /*------------------------------------------"Blocks"*/
        if (isSaveBlock)
        {
            //阻挡数据采集
            AstarData data = AstarPath.active.data;
            GridGraph grid = data.gridGraph;
            grid.GetNodes(node =>
            {
                // see GraphNode doc
                if (node.Walkable == false)
                {
                    ////Debug.Log("node pos:" + (Vector3)node.position + ";walkable:" + node.Walkable + ";index:" + node.NodeIndex + ",m_grid.LocalToCell" + (m_grid.LocalToCell((Vector3)node.position).x, m_grid.LocalToCell((Vector3)node.position).y));
                    int blockX = m_grid.LocalToCell((Vector3)node.position).x;
                    int blockY = m_grid.LocalToCell((Vector3)node.position).y;

                    JArray Blocks = (JArray)map_JObject["Blocks"];//动态添加数组内容
                    JArray ar = new JArray() { blockX, blockY };//定义可以添加到json内的数组，（所以可以动态添加数组值的方法只有这种，还有对象用JObject）
                    Blocks.Add(ar);//阻挡json赋值
                }
            });
        }
        #endregion/*------------------------------------------"Blocks"END*/

        #region /*------------------------------------------"RandArea"*/
        if (isSaveArea)
        {

            // map_JObject["RandArea"] = JObject.Parse("{'objname1':{'pos':[[0,2],[0,2],[0,2],[0,2]]},'objname2':{'pos':[[0,2],[0,2],[0,2],[0,2]]},'objname3':{'pos':[[0,2],[0,2],[0,2],[0,2]]}}");
            JObject RandArea = new JObject();//9、动态添加子对象和内容
            JObject objname1 = new JObject();//9、动态添加子对象和内容

            // #region /*------------------------------------------10.读取.Json文件并转成对象*/
            // JObject oEvent = new JObject();//提取randArea.json内的EVENT对象
            // oEvent = o["Event"] as JObject;
            // #endregion  /*------------------------------------------10.读取.Json文件并转成对象END*/

            #region /*------------------------------------------"编辑器内找到预制对象并采集随机范围坐标范围"*/
            //从场景中找到Event对象，遍历对象下的子对象，子对象中如果名字是带tilemap（做字符串分割以_为字符），抓到的这些对象给他们上面抓到的tilebase进行判断，找到四个最边缘的点，记录到randArea中；
            List<List<Vector2Int>> m_RandAreaAllArray = new List<List<Vector2Int>> { };//所有事件随机区域数组
            m_RandAreaAllArray.Clear();

            GameObject mapData = GameObject.Find("MapData");
            GameObject gameRandArea = mapData.transform.GetChild(2).gameObject;
            gameRandArea.name = "RandArea";
            Transform lvObj = gameRandArea.transform.GetChild(0);
            lvObj.name = m_mapName;
            for (int i = 0; i < lvObj.childCount; i++)
            {
                Transform eventChild = lvObj.GetChild(i);
                // eventChild.name//这里是wolf对象的名字
                List<Vector2Int> m_RandAreaArray = new List<Vector2Int> { };//随机区域数组
                m_RandAreaArray.Clear();
                //Debug.Log(eventChild.name);
                Tilemap tm = eventChild.GetChild(1).GetComponent<Tilemap>();//tilemap中的Tile

                //k=网格长宽
                //j=网格长宽
                //m_width_Height=网格长宽
                //h=颜色序号
                for (int k = 0; k <= (m_width_Height.x - 1); k++)//遍历网格x，m_width_Height0.2大小的网格
                {
                    for (int j = 0; j <= (m_width_Height.y - 1); j++)//遍历网格y
                    {
                        for (int h = 0; h < m_huang.Count; h++)//遍历所有颜色，也就是有涂过的就抓取。
                        {
                            if (tm.GetTile(new Vector3Int(k, j, 0)) == m_huang[h])//如果tile被填充过huang
                            {
                                //Debug.Log("m_huang[h]:" + m_huang[h]);
                                m_RandAreaArray.Add(new Vector2Int(k, j));//存到数组中
                                //Debug.Log("<color=#FF3A00>随机范围名称（有绘制的）：</color>" + eventChild.name);
                            }
                        }
                        if ((j == (m_width_Height.y - 1)) && (k == (m_width_Height.x - 1)))//遍历完毕,显示总数
                        {
                            //Debug.Log("随机范围坐标k,j:" + "(" + k + "," + j + ")");
                            //Debug.Log("生成完毕" + "随机范围坐标总数:" + m_RandAreaArray.Count);
                            if (m_RandAreaArray.Count > 0)
                            {
                                m_RandAreaAllArray.Add(m_RandAreaArray);//把这个类型的事件坐标添加到集合内，后面和其他事件区分开来
                            }
                        }
                    }
                }
            }

            for (int RandAreaId = 0; RandAreaId < m_RandAreaAllArray.Count; RandAreaId++)
            {
                #region /*------------------------------------------停用获取四点方式,最外围坐标判断*/
                // List<Vector2Int> posRandArea1 = new List<Vector2Int>() { };//1最左
                // List<Vector2Int> posRandArea2 = new List<Vector2Int>() { };//2最右
                // List<Vector2Int> posRandArea3 = new List<Vector2Int>() { };//3最下
                // List<Vector2Int> posRandArea4 = new List<Vector2Int>() { };//4最上
                // //初始化四个点，所以初始化时候存什么都没事。
                // Vector2Int RandArea1 = new Vector2Int(m_RandAreaAllArray[RandAreaId][0].x, m_RandAreaAllArray[RandAreaId][0].y);//1小小
                // Vector2Int RandArea2 = new Vector2Int(m_RandAreaAllArray[RandAreaId][0].x, m_RandAreaAllArray[RandAreaId][0].y);//2大小
                // Vector2Int RandArea3 = new Vector2Int(m_RandAreaAllArray[RandAreaId][0].x, m_RandAreaAllArray[RandAreaId][0].y);//3小大
                // Vector2Int RandArea4 = new Vector2Int(m_RandAreaAllArray[RandAreaId][0].x, m_RandAreaAllArray[RandAreaId][0].y);//4大大
                // int posXmax = m_RandAreaAllArray[RandAreaId][0].x;//找到x最大值
                // int posXmin = m_RandAreaAllArray[RandAreaId][0].x;//找到x小大值
                // int posYmax = m_RandAreaAllArray[RandAreaId][0].y;//找到y最大值
                // int posYmin = m_RandAreaAllArray[RandAreaId][0].y;//找到y小大值
                // if (m_RandAreaAllArray[RandAreaId].Count > 1)
                // {
                //     posXmax = m_RandAreaAllArray[RandAreaId][0].x;//找到x最大值
                //     posXmin = m_RandAreaAllArray[RandAreaId][0].x;//找到x小大值
                //     posYmax = m_RandAreaAllArray[RandAreaId][1].y;//找到y最大值
                //     posYmin = m_RandAreaAllArray[RandAreaId][1].y;//找到y小大值

                // }
                // else
                // {
                //     posXmax = m_RandAreaAllArray[RandAreaId][0].x;//找到x最大值
                //     posXmin = m_RandAreaAllArray[RandAreaId][0].x;//找到x小大值
                //     posYmax = m_RandAreaAllArray[RandAreaId][0].y;//找到y最大值
                //     posYmin = m_RandAreaAllArray[RandAreaId][0].y;//找到y小大值
                // }
                // //获取最外面的x和y值
                // for (int Q = 0; Q < m_RandAreaAllArray[RandAreaId].Count; Q++)
                // {
                //     if (m_RandAreaAllArray[RandAreaId][Q].x >= posXmax)
                //     {
                //         posXmax = m_RandAreaAllArray[RandAreaId][Q].x;
                //     }
                //     if (posXmin >= m_RandAreaAllArray[RandAreaId][Q].x)
                //     {
                //         posXmin = m_RandAreaAllArray[RandAreaId][Q].x;
                //     }
                //     if (m_RandAreaAllArray[RandAreaId][Q].y >= posYmax)
                //     {
                //         posYmax = m_RandAreaAllArray[RandAreaId][Q].y;
                //     }
                //     if (posYmin >= m_RandAreaAllArray[RandAreaId][Q].y)
                //     {
                //         posYmin = m_RandAreaAllArray[RandAreaId][Q].y;
                //     }
                // }
                // //Debug.Log("posXmax:" + posXmax + "posXmin:" + posXmin + "posYmax:" + posYmax + "posYmin:" + posYmin);
                // //获取最外面x和y值总共那些点
                // foreach (var item in m_RandAreaAllArray[RandAreaId])
                // {
                //     if (item.x == posXmax)
                //     {
                //         //Debug.Log("最右边点是：" + item);
                //         posRandArea2.Add(item);
                //     }
                //     if (item.x == posXmin)
                //     {
                //         //Debug.Log("最左边点是：" + item);
                //         posRandArea1.Add(item);
                //     }
                //     if (item.y == posYmax)
                //     {
                //         //Debug.Log("最上边点是：" + item);
                //         posRandArea4.Add(item);
                //     }
                //     if (item.y == posYmin)
                //     {
                //         //Debug.Log("最下边点是：" + item);
                //         posRandArea3.Add(item);
                //     }
                // }
                // //比较这些点确定四个点的值
                // foreach (var item1 in posRandArea1)
                // {
                //     if (item1.y < RandArea1.y)
                //     {
                //         RandArea1 = item1;
                //     }
                //     else if (item1.y > RandArea3.y)
                //     {
                //         RandArea3 = item1;
                //     }
                // }
                // foreach (var item2 in posRandArea2)
                // {
                //     if (item2.y < RandArea2.y)
                //     {
                //         RandArea2 = item2;
                //     }
                //     else if (item2.y > RandArea2.y)
                //     {
                //         RandArea4 = item2;
                //     }
                // }
                // foreach (var item3 in posRandArea3)
                // {
                //     if (item3.x < RandArea1.x)
                //     {
                //         RandArea1 = item3;
                //     }
                //     else if (item3.x > RandArea2.x)
                //     {
                //         RandArea2 = item3;
                //     }
                // }
                // foreach (var item4 in posRandArea4)
                // {
                //     if (item4.x < RandArea3.x)
                //     {
                //         RandArea3 = item4;
                //     }
                //     else if (item4.x > RandArea4.x)
                //     {
                //         RandArea4 = item4;
                //     }
                // }
                // //Debug.Log("RandArea1:" + RandArea1 + "RandArea2:" + RandArea2 + "RandArea3:" + RandArea3 + "RandArea4:" + RandArea4);
                // /*------------------------------------------最外围坐标判断END*/
                // objname1 = JObject.Parse("{'objname1':{'pos':[[" + RandArea1.x + "," + RandArea1.y + "],[" + RandArea2.x + "," + RandArea2.y + "],[" + RandArea3.x + "," + RandArea3.y + "],[" + RandArea4.x + "," + RandArea4.y + "]]}}");//9、动态添加子对象和内容
                // RandArea.Add(lvObj.GetChild(RandAreaId).name, objname1.SelectToken("objname1"));//9、动态添加子对象和内容
                #endregion
                /*------------------------------------------"编辑器内找到预制对象并采集随机范围坐标范围END"*/
                #endregion

                JArray i_posAllJArray = new JArray();
                JObject i_posAllJObject = new JObject();
                for (int i_pos = 0; i_pos < m_RandAreaAllArray[RandAreaId].Count; i_pos++)
                {
                    // Debug.Log("");
                    JArray i_posJArray = new JArray() { m_RandAreaAllArray[RandAreaId][i_pos].x, m_RandAreaAllArray[RandAreaId][i_pos].y };
                    i_posAllJArray.Add(i_posJArray);
                }
                i_posAllJObject.Add("pos", i_posAllJArray);

                RandArea.Add(lvObj.GetChild(RandAreaId).name, i_posAllJObject);//9、动态添加子对象和内容

                Debug.Log("<color=#88FF00>随机名称(有效的):</color>" + lvObj.GetChild(RandAreaId).name);
            }


            map_JObject["RandArea"] = RandArea;//9、动态添加子对象和内容

        }
        #endregion /*------------------------------------------"RandArea"*/

        #region /*------------------------------------------"Objects"*/
        #region 注释代码
        //     if (isClientOrServer)//客户端数据导出
        //     {
        //       JObject Objects = new JObject();
        //       JObject objname = new JObject();
        //       List<List<Vector2Int>> areaListAll = new List<List<Vector2Int>> { new List<Vector2Int> { } };
        //       // areaListAll.Clear();

        //       Transform but = GameObject.Find("Objects").transform;
        //       // for (int gi = 0; gi < but.childCount; gi++)
        //       // {
        //       //   Transform dynamicParent = but.GetChild(gi);
        //       // }

        //       /**=======foreach遍历所有的子物体以及孙物体，并且遍历包含本身=======
        //       * @param m_list:列表数组
        //       *
        // */
        //       // foreach (Transform child in but.GetComponentsInChildren<Transform>(true))
        //       Transform[] bArray = but.GetComponentsInChildren<Transform>(true);

        //       Transform eventObj = null;
        //       GameObject[] obj = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[]; //关键代码，获取所有gameobject元素给数组obj
        //       foreach (GameObject child in obj)    //遍历所有gameobject
        //       {
        //         //Debug.Log(child.gameObject.name);  //可以在unity控制台测试一下是否成功获取所有元素
        //         if (child.transform.parent != null)
        //         {
        //           if (child.transform.parent.name == "sprite")
        //           {
        //            //Debug.Log("找到event对象了");
        //             eventObj = child.transform;
        //           }
        //           else
        //           {
        //            //Debug.Log("没有正确的event对象");
        //           }
        //         }
        //       }


        //       int dynamicBlocksCount = 0;
        //       for (int bi = 0; bi < bArray.Length; bi++)
        //       {
        //         #region 注释代码

        //         /**=======GetComponentsInChildren只遍历所有的子物体，没有孙物体，遍历不包含本身=======
        //         * @param m_list:列表数组
        //         *
        //         */
        //         // for (int bii = 0; bii < eventObj.childCount; bii++)
        //         // {
        //         //   if (eventObj.GetChild(bii).name == bArray[bi].name)//sprite下的event下的所有子对象.name有bArray.name，就拿sprite下的event下的所有子对象.transform.position.x和y
        //         //   {
        //         //    //Debug.Log("相同的几个:" + bArray[bi].name);
        //         //     // eventObj.GetChild(bii).position.x// eventObj.GetChild(bii).position.y

        //         //     if (bArray[bi].GetComponent<DynamicBlocks>() && bArray[bi].gameObject.active == true && bArray[bi].GetComponent<SpriteRenderer>())
        //         //     {
        //         //       dynamicBlocksCount++;
        //         //       DynamicBlocks dynamicBlocks = bArray[bi].GetComponent<DynamicBlocks>();

        //         //       #region //采集单个对象上的两个area坐标，并进行转换成坐标群存到列表里面
        //         //       // int blockX = dynamicBlocks.grid.LocalToCell((Vector3)transform.position).x;
        //         //       // int blockY = dynamicBlocks.grid.LocalToCell((Vector3)transform.position).y;
        //         //       // int getAreaX = dynamicBlocks.m_parentVector2[1].x - dynamicBlocks.m_parentVector2[0].x + 1;
        //         //       // int getAreaY = dynamicBlocks.m_parentVector2[1].y - dynamicBlocks.m_parentVector2[0].y + 1;
        //         //       // int arrayCount = 0;
        //         //       // areaList = new List<Vector2Int> { new Vector2Int() };
        //         //       // areaList.Clear();
        //         //       // for (int getAreaXI = 0; getAreaXI < getAreaX; getAreaXI++)
        //         //       // {
        //         //       //   for (int getAreaYJ = 0; getAreaYJ < getAreaY; getAreaYJ++)
        //         //       //   {
        //         //       //     if (getAreaXI == (getAreaX - 1) && getAreaYJ == (getAreaY - 1))
        //         //       //     {

        //         //       //     }
        //         //       //     else
        //         //       //     {

        //         //       //     }

        //         //       //     arrayCount++;
        //         //       //     areaList.Add(new Vector2Int(getAreaXI, getAreaYJ));
        //         //       //   }
        //         //       // }
        //         //       #endregion //采集单个对象上的两个area坐标，并进行转换成坐标群存到列表里面

        //         //       //将父母对象下的坐标存到json要遍历的数组中
        //         //       areaList = new List<Vector2Int> { new Vector2Int() };
        //         //       areaList.Clear();
        //         //       for (int m_dpi = 0; m_dpi < dynamicBlocks.m_parentVector2.Count; m_dpi++)
        //         //       {
        //         //         areaList.Add(new Vector2Int(dynamicBlocks.m_parentVector2[m_dpi].x, dynamicBlocks.m_parentVector2[m_dpi].y));
        //         //       }

        //         //       areaListAll.Add(areaList);

        //         //       string sb = "{'objname':{'pos':[" + eventObj.GetChild(bii).position.x + "," + eventObj.GetChild(bii).position.y + "],'area':[";//数据拼接

        //         //       for (int pi = 0; pi < areaListAll[dynamicBlocksCount].Count; pi++)
        //         //       {
        //         //        //Debug.Log("areaListAll[dynamicBlocksCount]-----------//**:" + areaListAll[dynamicBlocksCount].Count + "," + pi);
        //         //         if (pi == (areaListAll[dynamicBlocksCount].Count - 1))
        //         //         {
        //         //           sb = sb + "[" + areaListAll[dynamicBlocksCount][pi].x + "," + areaListAll[dynamicBlocksCount][pi].y + "]";
        //         //         }
        //         //         else
        //         //         {
        //         //           sb = sb + "[" + areaListAll[dynamicBlocksCount][pi].x + "," + areaListAll[dynamicBlocksCount][pi].y + "],";
        //         //         }
        //         //       }

        //         //       sb = sb + "]}}";
        //         //      //Debug.Log("sb///////****----:" + sb);
        //         //       objname = JObject.Parse(sb);
        //         //       Objects.Add(bArray[bi].name, (JObject)objname.SelectToken("objname"));
        //         //     }



        //         //    //Debug.Log("跳出");
        //         //     goto GOTHIS;
        //         //   }
        //         // }
        //         ////Debug.Log("没跳出:" + bArray[bi].name);

        //         #endregion
        //         if (bArray[bi].GetComponent<DynamicBlocks>() && bArray[bi].gameObject.active == true && bArray[bi].GetComponent<SpriteRenderer>())
        //         {
        //           dynamicBlocksCount++;
        //           DynamicBlocks dynamicBlocks = bArray[bi].GetComponent<DynamicBlocks>();
        //          //Debug.Log("子物体Name:" + bArray[bi].name);
        //          //Debug.Log("area：" + dynamicBlocks.m_parentVector2.Count);
        //          //Debug.Log("pos:" + dynamicBlocks.pos.x + "," + dynamicBlocks.pos.y);


        //           #region //采集单个对象上的两个area坐标，并进行转换成坐标群存到列表里面
        //           // int blockX = dynamicBlocks.grid.LocalToCell((Vector3)transform.position).x;
        //           // int blockY = dynamicBlocks.grid.LocalToCell((Vector3)transform.position).y;
        //           // int getAreaX = dynamicBlocks.m_parentVector2[1].x - dynamicBlocks.m_parentVector2[0].x + 1;
        //           // int getAreaY = dynamicBlocks.m_parentVector2[1].y - dynamicBlocks.m_parentVector2[0].y + 1;
        //           // int arrayCount = 0;
        //           // areaList = new List<Vector2Int> { new Vector2Int() };
        //           // areaList.Clear();
        //           // for (int getAreaXI = 0; getAreaXI < getAreaX; getAreaXI++)
        //           // {
        //           //   for (int getAreaYJ = 0; getAreaYJ < getAreaY; getAreaYJ++)
        //           //   {
        //           //     if (getAreaXI == (getAreaX - 1) && getAreaYJ == (getAreaY - 1))
        //           //     {

        //           //     }
        //           //     else
        //           //     {

        //           //     }

        //           //     arrayCount++;
        //           //     areaList.Add(new Vector2Int(getAreaXI, getAreaYJ));
        //           //   }
        //           // }
        //           #endregion //采集单个对象上的两个area坐标，并进行转换成坐标群存到列表里面

        //           //将父母对象下的坐标存到json要遍历的数组中
        //           areaList = new List<Vector2Int> { new Vector2Int() };
        //           areaList.Clear();
        //           for (int m_dpi = 0; m_dpi < dynamicBlocks.m_parentVector2.Count; m_dpi++)
        //           {
        //             areaList.Add(new Vector2Int(dynamicBlocks.m_parentVector2[m_dpi].x, dynamicBlocks.m_parentVector2[m_dpi].y));
        //           }

        //           areaListAll.Add(areaList);

        //           string sb = "{'objname':{'pos':[" + dynamicBlocks.pos.x + "," + dynamicBlocks.pos.y + "],'area':[";//数据拼接

        //           for (int pi = 0; pi < areaListAll[dynamicBlocksCount].Count; pi++)
        //           {
        //            //Debug.Log("areaListAll[dynamicBlocksCount]-----------//**:" + areaListAll[dynamicBlocksCount].Count + "," + pi);
        //             if (pi == (areaListAll[dynamicBlocksCount].Count - 1))
        //             {
        //               sb = sb + "[" + areaListAll[dynamicBlocksCount][pi].x + "," + areaListAll[dynamicBlocksCount][pi].y + "]";
        //             }
        //             else
        //             {
        //               sb = sb + "[" + areaListAll[dynamicBlocksCount][pi].x + "," + areaListAll[dynamicBlocksCount][pi].y + "],";
        //             }
        //           }

        //           sb = sb + "]}}";
        //          //Debug.Log("sb///////****----:" + sb);
        //           objname = JObject.Parse(sb);
        //           Objects.Add(bArray[bi].name, (JObject)objname.SelectToken("objname"));
        //         }

        //         // GOTHIS:
        //         //  //Debug.Log("跳出2");
        //       }

        //       map_JObject["Objects"] = Objects;
        //     }
        //     else//服务端数据导出
        #endregion
        {

            JObject Objects = new JObject();
            JObject objname = new JObject();
            List<List<Vector2Int>> areaListAll = new List<List<Vector2Int>> { new List<Vector2Int> { } };

            Transform but = GameObject.Find("Objects").transform;

            /**=======foreach遍历所有的子物体以及孙物体，并且遍历包含本身=======
            * @param m_list:列表数组
            *
*/
            // foreach (Transform child in but.GetComponentsInChildren<Transform>(true))
            int dynamicBlocksCount = 0;
            string sb = null;
            for (int bi = 0; bi < but.childCount; bi++)
            {
                if (but.GetChild(bi).GetComponent<DynamicBlocks>() && but.GetChild(bi).gameObject.active == true && but.GetChild(bi).GetComponent<SpriteRenderer>())
                {
                    dynamicBlocksCount++;
                    DynamicBlocks dynamicBlocks = but.GetChild(bi).GetComponent<DynamicBlocks>();

                    #region //采集单个对象上的两个area坐标，并进行转换成坐标群存到列表里面
                    // int blockX = dynamicBlocks.grid.LocalToCell((Vector3)transform.position).x;
                    // int blockY = dynamicBlocks.grid.LocalToCell((Vector3)transform.position).y;
                    // int getAreaX = dynamicBlocks.m_parentVector2[1].x - dynamicBlocks.m_parentVector2[0].x + 1;
                    // int getAreaY = dynamicBlocks.m_parentVector2[1].y - dynamicBlocks.m_parentVector2[0].y + 1;
                    // int arrayCount = 0;
                    // areaList = new List<Vector2Int> { new Vector2Int() };
                    // areaList.Clear();
                    // for (int getAreaXI = 0; getAreaXI < getAreaX; getAreaXI++)
                    // {
                    //   for (int getAreaYJ = 0; getAreaYJ < getAreaY; getAreaYJ++)
                    //   {
                    //     if (getAreaXI == (getAreaX - 1) && getAreaYJ == (getAreaY - 1))
                    //     {

                    //     }
                    //     else
                    //     {

                    //     }

                    //     arrayCount++;
                    //     areaList.Add(new Vector2Int(getAreaXI, getAreaYJ));
                    //   }
                    // }
                    #endregion //采集单个对象上的两个area坐标，并进行转换成坐标群存到列表里面

                    //将父母对象下的坐标存到json要遍历的数组中
                    areaList = new List<Vector2Int> { new Vector2Int() };
                    areaList.Clear();

                    for (int m_dpi = 0; m_dpi < dynamicBlocks.m_parentVector2.Count; m_dpi++)
                    {
                        areaList.Add(new Vector2Int(dynamicBlocks.m_parentVector2[m_dpi].x, dynamicBlocks.m_parentVector2[m_dpi].y));
                    }

                    areaListAll.Add(areaList);

                    //如果是地标，就从event里面抓取位置
                    {
                        // foreach (Transform child5 in GameObject.Find("sprite").transform.GetComponentsInChildren<Transform>())
                        // {
                        //     if (child5.name == but.GetChild(bi).name)
                        //     {//如果是地标，就从event里面抓取位置
                        //         Vector2 pos3;
                        //         Vector3Int gridpos = GameObject.Find("MapData").GetComponent<Grid>().LocalToCell((Vector3)child5.position);
                        //         pos3.x = gridpos.x;
                        //         pos3.y = gridpos.y;

                        //         sb = "{'objname':{'pos':[" + pos3.x + "," + pos3.y + "],'pos2':[" + child5.position.x + "," + child5.position.y + "],'area':[";//数据拼接

                        //         goto GOTHIS;
                        //     }
                        // }
                    }
                    //如果是不地标，就从Objects里面抓取位置
                    {
                        sb = "{'objname':{'pos':[" + dynamicBlocks.pos.x + "," + dynamicBlocks.pos.y + "],'pos2':[" + but.GetChild(bi).position.x + "," + but.GetChild(bi).position.y + "],'area':[";//数据拼接
                    }

                //动态阻挡范围
                GOTHIS:
                    for (int pi = 0; pi < areaListAll[dynamicBlocksCount].Count; pi++)
                    {
                        //Debug.Log("areaListAll[dynamicBlocksCount]-----------//**:" + areaListAll[dynamicBlocksCount].Count + "," + pi);
                        if (pi == (areaListAll[dynamicBlocksCount].Count - 1))
                        {
                            sb = sb + "[" + areaListAll[dynamicBlocksCount][pi].x + "," + areaListAll[dynamicBlocksCount][pi].y + "]";
                        }
                        else
                        {
                            sb = sb + "[" + areaListAll[dynamicBlocksCount][pi].x + "," + areaListAll[dynamicBlocksCount][pi].y + "],";
                        }
                    }

                    sb = sb + "]}}";

                    objname = JObject.Parse(sb);
                    Objects.Add(but.GetChild(bi).name, (JObject)objname.SelectToken("objname"));
                }
            }

            map_JObject["Objects"] = Objects;
        }

        #endregion /*------------------------------------------"Objects"*/

        #region /*------------------------------------------"Transition"*/
        {
            JObject Transition = new JObject();
            JObject Transitionname = new JObject();

            Transform but = GameObject.Find("Transition").transform;
            Transform[] bArray2 = but.GetComponentsInChildren<Transform>(true);
            string sb2 = null;//json文本化存储
            //遍历Transition子对象个数
            for (int bi2 = 1; bi2 < bArray2.Length; bi2++)
            {
                //如果Transition的子对象有绑定组件且已经激活
                if (bArray2[bi2].GetComponent<TransitionData>() && bArray2[bi2].gameObject.active == true)
                {
                    TransitionData transitionData = bArray2[bi2].GetComponent<TransitionData>();
                    //如果是不地标，就从Objects里面抓取位置
                    sb2 = "{'ID':{'pos':[" + bArray2[bi2].position.x + "," + bArray2[bi2].position.y + "]," + "'radius':" + transitionData.m_Radius + "," + "'pos2':[" + 0 + "," + 0 + "],'pos3':[" + 0 + "," + 0 + "]}}";//数据拼接
                    Transitionname = JObject.Parse(sb2);
                    Transition.Add(transitionData.m_Id.ToString(), (JObject)Transitionname.SelectToken("ID"));
                }
            }
            map_JObject["Transition"] = Transition;
        }

        #endregion /*------------------------------------------"Transition"*/

        #region /*------------------------------------------"NodeSize"*/
        map_JObject["NodeSize"] = 0.2f;//网格缩放倍数
        #endregion /*------------------------------------------"NodeSize"*/

        #region /*------------------------------------------"BirthPos"*/
        JArray BirthPosJArray = (JArray)map_JObject["BirthPos"];//
        BirthPosJArray.Add(m_birthPos.x);
        BirthPosJArray.Add(m_birthPos.y);
        #endregion /*------------------------------------------"BirthPos"*/

        #region /*------------------------------------------"BorderPos"*/
        JArray BorderPosJArray = (JArray)map_JObject["BorderPos"];//

        JArray bar = new JArray() { m_BorderPos1.x, m_BorderPos1.y };//定义可以添加到json内的数组，（所以可以动态添加数组值的方法只有这种，还有对象用JObject）
        JArray bar2 = new JArray() { m_BorderPos2.x, m_BorderPos2.y };
        BorderPosJArray.Add(bar);//阻挡json赋值
        BorderPosJArray.Add(bar2);//阻挡json赋值

        #endregion /*------------------------------------------"BorderPos"*/
        // yield return new WaitForSeconds(5.0f);
    }

    #endregion //===================================json创建和地图信息写入==================================

    #region ===================================文件流的存取==================================
    /// <summary>
    /// 保存服务端需要的json数据
    /// </summary>
    // 保存服务端需要的json数据
    [EasyButtons.Button]
    public void SaveFileServer()
    {
        isClientOrServer = false;
        // StartCoroutine(GetData());
        GetData(true, true);

        //  m_json.text//存储位置
        StringBuilder sb = new StringBuilder();//声明一个可变字符串//这里放经过json序列化后的地图采集数据
        for (int i = 0; i < 10; i++)
        {
            //循环给字符串拼接字符
            sb.Append(i + '|');
        }


        //m_mapName
        //写文件 文件名为save.text
        //这里的FileMode.create是创建这个文件,如果文件名存在则覆盖重新创建
#if NETFX_CORE
		throw new System.NotSupportedException("Cannot save to file on this platform");
#else
        using (var fs2 = new FileStream(Application.dataPath + "/Data/" + m_mapName + ".json", FileMode.Create))
        {
            //压缩json，序列化map_JObject ，可以删除空格和换行减少json文件的大小
            string st = JsonConvert.SerializeObject(_jObject);//_jObject可以是JObject
            //存储时时二进制,所以这里需要把我们的字符串转成二进制
            byte[] bytes = new UTF8Encoding().GetBytes(st);
            fs2.Write(bytes, 0, bytes.Length);
            //每次读取文件后都要记得关闭文件
            fs2.Close();
        }

        UnityEditor.AssetDatabase.Refresh();//刷新项目文件夹
#endif


        // eventObj.SetActive(true);
    }

    /// <summary>
    /// 保存客户端需要的json文件，并调用Astar.Save保存json到二进制zip中;
    /// </summary>
    // 保存客户端需要的json文件，并调用Astar.Save保存json到二进制zip中;
    [EasyButtons.Button]
    public void SaveFileClient()
    {
        isClientOrServer = true;
        GetData(false, false);

        UpdateExportExtJsonData();
        AstarPathEditor.path = Application.dataPath + "/Art/Map/Level/" + m_mapName;
        AstarPathEditor.SaveToFile();

        // eventObj.SetActive(true);
    }

    /// <summary>
    /// 抓取服务端的数据给客户端使用
    /// </summary>
    // 抓取服务端的数据给客户端使用
    public void UpdateExportExtJsonData()
    {
        JObject jobj = (JObject)_jObject[m_mapName];
        jobj.Remove("Blocks");
        jobj.Remove("RandArea");
        byte[] bytes = new UTF8Encoding().GetBytes(jobj.ToString());
        AstarPath.active.SerializeExtData = bytes;
    }
    #endregion //===================================文件流的存取==================================

    #region ===================================没用代码==================================
    /// <summary>
    /// 读取文本文件
    /// </summary>
    //读取文本文件
    public void 读取文本文件()
    {
        //FileMode.Open打开路径下的save.text文件
        FileStream fs = new FileStream(Application.streamingAssetsPath + "/test.json", FileMode.Open);
        byte[] bytes = new byte[10];
        fs.Read(bytes, 0, bytes.Length);
        //将读取到的二进制转换成字符串
        string s = new UTF8Encoding().GetString(bytes);
        //将字符串按照'|'进行分割得到字符串数组
        string[] itemIds = s.Split('|');
        for (int i = 0; i < itemIds.Length; i++)
        {
            //Debug.Log(itemIds[i]);
        }
    }

    //   bool active = false;
    //   [EasyButtons.Button]
    //   public void 可视化物件上的动态阻挡()
    //   {
    //     Transform but = GameObject.Find("Objects").transform;
    //     /**=======foreach遍历所有的子物体以及孙物体，并且遍历包含本身=======
    //     * @param m_list:列表数组
    //     *
    // */
    //     foreach (Transform child in but.GetComponentsInChildren<Transform>(true))
    //     {
    //       if (child.GetComponent<DynamicBlocks>() && child.gameObject.active == true)
    //       {
    //         if (active)
    //         {
    //           child.GetComponent<DynamicBlocks>().DrawTile();
    //         }
    //         else
    //         {
    //           child.GetComponent<DynamicBlocks>().ClearTile();
    //         }
    //       }
    //     }
    //     active = !active;
    //   }


    /// <summary>
    /// json序列化和反序列化
    /// </summary>
    public void JsonSerializeObject()
    {
        // map = new Maps() { Map = m_mc };//实例化json实体类
        // map.Map.Name = m_mapName;
        // map.Map.Width = m_width_Height.x;
        // map.Map.Height = m_width_Height.y;
        // /*------------------------------------------Blocks.Add*/
        // //数据初始化
        // List<List<int>> list = new List<List<int>> { new List<int> { } };
        // list.Clear();
        // //阻挡数据采集
        // AstarData data = AstarPath.active.data;
        // GridGraph grid = data.gridGraph;
        // grid.GetNodes(node =>
        // {
        //   // see GraphNode doc
        //   if (node.Walkable == false)
        //   {
        //     ////Debug.Log("node pos:" + (Vector3)node.position + ";walkable:" + node.Walkable + ";index:" + node.NodeIndex + ",m_grid.LocalToCell" + (m_grid.LocalToCell((Vector3)node.position).x, m_grid.LocalToCell((Vector3)node.position).y));
        //     int blockX = m_grid.LocalToCell((Vector3)node.position).x;
        //     int blockY = m_grid.LocalToCell((Vector3)node.position).y;
        //     list.Add(new List<int> { blockX, blockY });
        //   }
        // });
        // //阻挡json赋值
        // map.Map.Blocks = list;
        // /*------------------------------------------randArea.Add*/
        // List<List<int>> pos4 = new List<List<int>> { };
        // List<int> l = new List<int> { };
        // l.Add(0);
        // l.Add(2);
        // for (int i = 0; i < 4; i++)
        // {
        //   pos4.Add(l);
        // }

        // map.Map.RandArea = new RandAreas()
        // {
        //   objname1 = new Objnames()
        //   {
        //     pos = pos4
        //   },
        //   objname2 = new Objnames()
        //   {
        //     pos = pos4
        //   },
        //   objname3 = new Objnames()
        //   {
        //     pos = pos4
        //   }
        // };
        // List<int> listt = new List<int> { 1, 2 };


        // //从场景中找到Event对象，遍历对象下的子对象，子对象中如果名字是带tilemap（做字符串分割以_为字符），抓到的这些对象给他们上面抓到的tilebase进行判断，找到四个最边缘的点，记录到randArea中；
        // List<Vector2> m_RandAreaArray = new List<Vector2> { };//阻挡数组
        // m_RandAreaArray.Clear();

        // GameObject gameRandArea = GameObject.Find("Event");
        // for (int i = 0; i < gameRandArea.GetComponentsInChildren<Transform>(true).Length; i++)
        // {
        //   Transform eventChild = gameRandArea.GetComponentsInChildren<Transform>(true)[i];
        //  //Debug.Log(eventChild.name);
        //   if (eventChild.name == "woofTilemap")
        //   {
        //     Tilemap tm = eventChild.GetComponent<Tilemap>();//tilemap中的Tile
        //     for (int k = 0; k <= m_width_Height.x; k++)//遍历网格x，m_width_Height0.2大小的网格
        //     {
        //       for (int j = 0; j <= m_width_Height.y; j++)//遍历网格y
        //       {
        //         for (int h = 0; h < m_huang.Count; h++)//遍历所有颜色，也就是有涂过的就抓取。
        //         {
        //           if (tm.GetTile(new Vector3Int(k, j, 0)) == m_huang[h])//如果tile被填充过huang
        //           {
        //             m_RandAreaArray.Add(new Vector2(k, j));//存到数组中
        //           }
        //           if ((j == m_width_Height.y) && (k == m_width_Height.x))//遍历完毕
        //           {
        //            //Debug.Log("k,j:" + "(" + k + "," + j + ")");
        //            //Debug.Log("生成完毕" + "，总共生成阻挡:" + m_RandAreaArray.Count);
        //           }
        //         }

        //       }
        //     }
        //   }
        // }

        // /*------------------------------------------Objects.Add*/
        // List<int> listi = new List<int> { 1, 2 };
        // List<List<int>> list4 = new List<List<int>> { new List<int> { } };
        // list4.Clear();
        // for (int i = 0; i < 4; i++)
        // {
        //   list4.Add(new List<int> { 1, 2 });
        // }
        // Objname oname = new Objname() { pos = listi, area = list4 };
        // map.Map.Objects = new MapObjects() { objname = oname };
        // /*------------------------------------------网格缩放倍数*/
        // map.Map.NodeSize = 0.2f;//网格缩放倍数
        // /*------------------------------------------实体序列化(Map)和反序列化*/
        // string json1 = JsonHelper.SerializeObject(map);
        // Maps map1 = JsonHelper.DeserializeJsonToObject<Maps>(json1);
        ////Debug.Log("json1:" + json1);
        // ////Debug.Log(";objname:" + map1.Objects.objname);

    }

    /*----------------------------------------------------------------------------------------生成地图数据按钮*/
    // [EasyButtons.Button]
    public void 生成地图数据按钮()
    {
        ////Debug.Log("生成地图数据按钮");
        // if (m_Map)//地图预制判空
        // {
        //  //Debug.Log("/*------------------------------------------------找到Sprite对象*/");
        //  //Debug.Log(m_Map.transform.GetChild(0).name);//找到Sprite对象
        //  //Debug.Log(m_Map.transform.GetChild(0).GetChild(0).name);//找到Sprite子对象

        //  //Debug.Log("/*------------------------------------------------找到美术的Grid对象*/");
        //  //Debug.Log(m_Map.transform.GetChild(1).name);//找到美术的Grid对象
        //  //Debug.Log("/*------------------------------------------------遍历所有的子物体以及孙物体，并且遍历包含本身,判断图层是第八层*/");
        //   m_butArray.Clear();//遍历的结果数组初始化
        //                      //遍历所有的子物体以及孙物体，并且遍历包含本身
        //   foreach (Transform child in m_Map.GetComponentsInChildren<Transform>(true))
        //   {
        //     if (child.gameObject.layer == 8)//layer==PostProcessing
        //     {
        //       m_butArray.Add(child);
        //      //Debug.Log("子物体Name:" + child.name);
        //     }
        //   }
        //  //Debug.Log("/*------------------------------------------------只遍历所有的子物体，没有孙物体  ，遍历不包含本身*/");
        //   ////只遍历所有的子物体，没有孙物体  ，遍历不包含本身
        //   foreach (Transform child in m_Map.transform)
        //   {
        //     m_butArray.Add(child);
        //    //Debug.Log("子物体Name:" + child.name);
        //   }
        //  //Debug.Log("/*------------------------------------------------地图名称*/");
        //   ////Debug.Log(m_Map.name);//地图名称
        //   // //split分割字符串
        //   // string s = m_Map.name.Split('V')[0];
        //   ////Debug.Log(s);
        //   // /**split分割字符串
        //   // * @param :字符串
        //   // * @param :分割符号
        //   // * @param :返回第个数组
        //   // */
        //  //Debug.Log(m_mapName);//地图名称
        //  //Debug.Log("/*------------------------------------------------地图宽高*/");
        //  //Debug.Log(m_width_Height.x);//地图宽
        //  //Debug.Log(m_width_Height.y);//地图高
        //  //Debug.Log("/*------------------------------------------------Blocks阻挡块*/");
        //   m_blocksArray.Clear();//初始化阻挡存储数组
        //   Tilemap tc = m_Map.transform.GetChild(1).GetChild(4).GetComponent<Tilemap>();

        //   TileBase bl = tc.GetTile(new Vector3Int(26, 60, 0));//tilemap中的Tile

        //   for (int i = 0; i <= m_width_Height.x; i++)//遍历网格x
        //   {
        //     for (int j = 0; j <= m_width_Height.y; j++)//遍历网格y
        //     {
        //       if (tc.GetTile(new Vector3Int(i, j, 0)) == m_huang)//如果tile被填充过huang
        //       {
        //         ////Debug.Log("i,j:" + "(" + i + "," + j + ")");
        //         m_blocksArray.Add(new Vector2(i, j));//存到数组中
        //       }
        //       if ((j == m_width_Height.y) && (i == m_width_Height.x))//遍历完毕
        //       {
        //        //Debug.Log("i,j:" + "(" + i + "," + j + ")");
        //        //Debug.Log("生成完毕" + "，总共生成阻挡:" + m_blocksArray.Count);
        //       }
        //     }
        //   }
        //   //tc.GetSprite();//tilemap中的sprite
        //   // Tilemap.HasTile//返回位置是否有图块。
        //   ////Debug.Log();//Blocks阻挡块【【x，y】，】


        // }
    }
    /*----------------------------------------------------------------------------------------A星阻挡解析*/
    // [EasyButtons.Button]
    public void 解析二进制()
    {
        // TextAsset asset = Resources.Load("enemy_seq_bin") as TextAsset;
        // TextAsset fs = Resources.Load(Application.streamingAssetsPath + "/graph_references.txt") as TextAsset;
        // byte[] bytes = new byte[10];
        // fs.Read(bytes, 0, bytes.Length);
        //将读取到的二进制转换成字符串
        // string s = new UTF8Encoding().GetString(fs.bytes);
        //     Stream s2 = new MemoryStream(fs.bytes);
        //     BinaryReader br = new BinaryReader(s2);
        //     string text=null;
        // for (int i = 0; i < fs.bytes.Length; i++)
        // {
        //     text=text+fs.bytes[i];
        // }

        //    //Debug.Log("text:" + text);

        //    //Debug.Log("s2:" + s2.ToString());
        //    //Debug.Log("br:" + br.ToString());
        // AstarData.
    }

    // [EasyButtons.Button]
    public void exportBlockInfo()
    {
        AstarData data = AstarPath.active.data;
        GridGraph grid = data.gridGraph;

        grid.GetNodes(node =>
        {
            // see GraphNode doc
            if (node.Walkable == false)
            {
                //Debug.Log("node pos:" + (Vector3)node.position + ";walkable:" + node.Walkable + ";index:" + node.NodeIndex + ",m_grid.LocalToCell" + (m_grid.LocalToCell((Vector3)node.position).x, m_grid.LocalToCell((Vector3)node.position).y));
            }
        });
    }



    #endregion
}

#region ===================================不重要==================================
/// <summary>
/// 单个地图对象实体类
public class MapC
{
    public string Name { get; set; }//地图名称，策划配表对应
    public int Width { get; set; }//宽
    public int Height { get; set; }//高
    public List<List<int>> Blocks { get; set; }//阻挡块
    public RandAreas RandArea { get; set; }//随机范围集合
    public MapObjects Objects { get; set; }//物件集合
    public float NodeSize { get; set; }//物件集合
}

/// <summary>
/// 物件集合实体类
/// </summary>
public class MapObjects
{
    public Objname objname { get; set; }

}
/// <summary>
/// 物件对象实体类
/// </summary>
public class Objname
{
    public List<int> pos { get; set; }
    public List<List<int>> area { get; set; }
}

/// <summary>
/// 随机范围集合实体类
/// </summary>
public class RandAreas
{

    public Objnames objname1 { get; set; }//随机区域1数组
    public Objnames objname2 { get; set; }//随机区域2数组
    public Objnames objname3 { get; set; }//随机区域3数组
                                          // public List<List<int>> objname3 { get; set; }//随机区域3数组

}

/// <summary>
/// 物件对象实体类
/// </summary>
public class Objnames
{
    public List<List<int>> pos { get; set; }
}

#endregion

#region ===================================无用代码==================================
/// <summary>
/// 地图实体类
/// </summary>
public class Maps
{
    public MapC Map { get; set; }//地图名称，策划配表对应

}
/// <summary>
/// Json帮助类
/// </summary>
public class JsonHelper
{
    // /// <summary>
    // /// 将对象序列化为JSON格式
    // /// </summary>
    // /// <param name="o">对象</param>
    // /// <returns>json字符串</returns>
    // public static string SerializeObject(object o)
    // {
    //   string json = Newtonsoft.Json.JsonConvert.SerializeObject(o);
    //   return json;
    // }

    // /// <summary>
    // /// 解析JSON字符串生成对象实体
    // /// </summary>
    // /// <typeparam name="T">对象类型</typeparam>
    // /// <param name="json">json字符串(eg.{"ID":"112","Name":"石子儿"})</param>
    // /// <returns>对象实体</returns>
    // public static T DeserializeJsonToObject<T>(string json) where T : class
    // {
    //   Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
    //   StringReader sr = new StringReader(json);
    //   object o = serializer.Deserialize(new Newtonsoft.Json.JsonTextReader(sr), typeof(T));
    //   T t = o as T;
    //   return t;
    // }

    // /// <summary>
    // /// 解析JSON数组生成对象实体集合
    // /// </summary>
    // /// <typeparam name="T">对象类型</typeparam>
    // /// <param name="json">json数组字符串(eg.[{"ID":"112","Name":"石子儿"}])</param>
    // /// <returns>对象实体集合</returns>
    // public static List<T> DeserializeJsonToList<T>(string json) where T : class
    // {
    //   Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();
    //   StringReader sr = new StringReader(json);
    //   object o = serializer.Deserialize(new Newtonsoft.Json.JsonTextReader(sr), typeof(List<T>));
    //   List<T> list = o as List<T>;
    //   return list;
    // }

    // /// <summary>
    // /// 反序列化JSON到给定的匿名对象.
    // /// </summary>
    // /// <typeparam name="T">匿名对象类型</typeparam>
    // /// <param name="json">json字符串</param>
    // /// <param name="anonymousTypeObject">匿名对象</param>
    // /// <returns>匿名对象</returns>
    // public static T DeserializeAnonymousType<T>(string json, T anonymousTypeObject)
    // {
    //   T t = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, anonymousTypeObject);
    //   return t;
    // }
}

#endregion

#region ===================================注释代码==================================
/*----------------------------------------------------------------------------------------------------------------------------*/
/// <summary>
/// name: =======绘制代码tilemap类=======
/// </summary>
// public class TEXTTILEMAP : MonoBehaviour
// {
//    public Tilemap TM;
//    public TileBase tb;

//     // Start is called before the first frame update
//     void Start()
//     {
//         TM.BoxFill(Vector3Int.zero,tb,-2,-2,2,2);
//     }
// }
/*----------------------------------------------------------------------------------------------------------------------------*/

// /// <summary>
// /// name: =======橡皮擦代码tilemap类=======
// /// </summary>
// public class TEXTTILEMAP : MonoBehaviour
// {
//    public Tilemap TM;
//    public TileBase tb;

//     // Start is called before the first frame update
//     void Start()
//     {
//         TM.SetTile(new Vector3Int(-2,2,0),null);
//     }
// }
/*----------------------------------------------------------------------------------------------------------------------------*/

// /// <summary>
// /// name: =======查找tilemap中的一种笔刷内容，然后替换成另一种笔刷内容类=======
// /// </summary>
// public class TEXTTILEMAP : MonoBehaviour
// {
//    public Tilemap TM;
//    public TileBase oldTileBase;
//    public TileBase newTileBase;

//     // Start is called before the first frame update
//     void Start()
//     {
//         TM.SwapTile(oldTileBase,newTileBase);
//     }
// }
/*----------------------------------------------------------------------------------------------------------------------------*/

// /// <summary>
// /// name: =======油漆桶能自动识别填充能清空类=======
// /// </summary>
// public class TEXTTILEMAP : MonoBehaviour
// {
//     public Tilemap TM;
//     public TileBase tb;

//     // Start is called before the first frame update
//     void Start()
//     {
//         TM.FloodFill(new Vector3Int(-1, -1, 0), null);
//     }
// }

/*----------------------------------------------------------------------------------------------------------------------------*/

// /// <summary>
// /// name: =======根据给定边界批量获得Grid中的Tile类=======
// /// </summary>
// public class TEXTTILEMAP : MonoBehaviour
// {
//     public Tilemap TM;
//     public TileBase oldTileBase;
//     public TileBase newTileBase;

//     // Start is called before the first frame update
//     void Start()
//     {
//         TileBase[] TileBase = TM.GetTilesBlock(new BoundsInt(-3, -3, 0, 3, 3, 1));//BoundsInt(-3, -3, 0 ,     3, 3, 1)三个值表示一个坐标里面的一个点，1表示z轴上的一个单位

//     foreach (var item in TileBase)
//     {
//        //Debug.Log(item);
//     }
//     }
// }

/*----------------------------------------------------------------------------------------------------------------------------*/

// // /// <summary>
// // /// name: =======grid坐标转换类=======
// // /// </summary>
// public class TEXTTILEMAP : MonoBehaviour
// {
// public Grid g;
// public Transform t;
//     void Start()
//     {
//         // print("g.cellGap"+g.cellGap);//0,0,0
//         // print("g.cellSize"+g.cellSize);//(0.2, 0.2, 1.0)
//         //print("g.CellToLocal"+g.CellToLocal(new Vector3Int(19,17,0)));//传入格子的位置返回transform的坐标,可以通过123,123去遍历出他们的坐标
//         //print("g.LocalToCellInterpolated"+g.LocalToCellInterpolated(new Vector3(t.localPosition.x,t.localPosition.y,0)));//传入本地transform的坐标返回格子位置
//         //print("GetInstanceID"+t.GetInstanceID());//生成固定id具有不可变和唯一性
//     }
// }
/*----------------------------------------------------------------------------------------------------------------------------*/

#endregion
