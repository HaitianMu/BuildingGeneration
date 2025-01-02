using System;
using UnityEngine;
public class BuildingGeneratiion : MonoBehaviour
{

    // 输入的房间面积数组（已知数组）
    public float[] roomAreas;

    public int num = 0;//记录已经生成的房间数量
    // 总区域大小
    private float totalArea;
    public int totalWidth;//用于记录整个区域的宽
    public int totalHeight;//用于记录整个区域的高

    // 用于存储生成的房间对象
    private GameObject[] generatedRooms;

    // 创建一个矩形房间



    // 创建一个矩形房间
    /* public GameObject CreateRoom(float x, float z, float width, float height)
     {
         // Debug.Log($"即将生成房间的位置是： x: {x}, z: {z}, width: {width}, height: {height}");
         // 创建一个新的立方体表示房间
         GameObject room = GameObject.CreatePrimitive(PrimitiveType.Cube);

         // 设置房间的位置和大小
         room.transform.position = new Vector3(x + width / 2, 0f, z + height / 2); // 高度固定在 0
         room.transform.localScale = new Vector3(width, 1f, height); // 高度 1，适应 x-z 平面

         // 给房间随机上色
         room.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
         // 返回生成的房间对象
         return room;
     }*/
    //墙壁生成
    public GameObject CreateRoom(float x, float z, float width, float height)
    {
        // 创建房间的主体
        GameObject room = new GameObject("Room");

        // 生成房间的底部（如果需要的话，可以添加底部，作为房间地面）
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(x + width / 2, 0f, z + height / 2);
        floor.transform.localScale = new Vector3(width, 0.1f, height); // 底部宽度和高度

        // 给房间底部上色
        floor.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.2f); ; // 颜色可以自定义
        AddRoomToList(floor); // 将地面加入到房间列表中
                              // 添加四个墙壁
        CreateWall(x, z, width, height, room);
       /* // 给房间主体添加一个颜色，或者其他设置
        room.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);*/

        // 返回生成的房间对象
        return room;
    }

    void CreateWall(float x, float z, float width, float height, GameObject room)
    {
        // 墙壁的厚度（可以调整，越大墙壁越厚）
        float wallThickness = 0.1f;
        float y = 3.0f;//墙体的高度
        // 创建四面墙体：四个方向
        // 1. 左墙
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.transform.parent = room.transform;
        leftWall.transform.position = new Vector3(x + wallThickness / 2, y / 2, z + height / 2);
        leftWall.transform.localScale = new Vector3(wallThickness, y, height);
        leftWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // 墙壁颜色


        // 2. 右墙
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.transform.parent = room.transform;
        rightWall.transform.position = new Vector3(x + width - wallThickness / 2, y / 2, z + height / 2);
        rightWall.transform.localScale = new Vector3(wallThickness, y, height);
        rightWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // 墙壁颜色

        // 3. 前墙
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.transform.parent = room.transform;
        frontWall.transform.position = new Vector3(x + width / 2, y / 2, z + wallThickness / 2);
        frontWall.transform.localScale = new Vector3(width, y, wallThickness);
        frontWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // 墙壁颜色

        // 4. 后墙
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.transform.parent = room.transform;
        backWall.transform.position = new Vector3(x + width / 2, y / 2, z + height - wallThickness / 2);
        backWall.transform.localScale = new Vector3(width, y, wallThickness);
        backWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // 墙壁颜色

    }



    // 将生成的房间添加到房间列表
    public void AddRoomToList(GameObject room)
    {
        // 将新生成的房间加入到房间列表中
        Array.Resize(ref generatedRooms, generatedRooms.Length + 1);
        generatedRooms[generatedRooms.Length - 1] = room;
    }
    public void GenerateRooms()
    {
        foreach (float part in roomAreas)
        {
            Debug.Log("（生成器接受到的数据）" + part);
        }
        // 清除上次生成的房间
        ClearPreviousRooms();
        // 计算总面积
        totalArea = 0f;
        foreach (var area in roomAreas)
        {
            totalArea += area;
        }

        // 找出最接近正方形的长宽组合
        FindBestDimensions(totalArea);
        // 输出计算的总区域宽高
        Debug.Log("Total Area: " + totalArea);
        Debug.Log("Total Width: " + totalWidth);
        Debug.Log("Total Height: " + totalHeight);

        // 使用方形树图生成房间
        CreateRoomRects(roomAreas, 0, roomAreas.Length, 0, 0, totalWidth, totalHeight, totalArea, (totalHeight / (float)totalWidth) > 1);
    }
    // 清除上次生成的房间
    void ClearPreviousRooms()
    {
        // 检查 generatedRooms 是否为 null，如果是，初始化为一个空数组
        if (generatedRooms == null)
        {
            generatedRooms = new GameObject[0];
        }

        // 查找所有已经生成的房间对象并销毁
        foreach (var room in generatedRooms)
        {
            if (room != null)
            {
                Destroy(room);
            }
        }

        // 清空房间列表
        generatedRooms = new GameObject[0];
    }
    // 找出长宽比最接近1的宽高组合，如果BestRatio>3或<1/3，则将该区域设置为正方形
    void FindBestDimensions(float totalArea)
    {
        int bestWidth = 0;
        int bestHeight = 0;
        float bestRatio = float.MaxValue;  // 最接近1的比值

        // 遍历所有可能的宽度值
        for (int width = 1; width <= Mathf.FloorToInt(Mathf.Sqrt(totalArea)); width++)
        {
            if (totalArea % width == 0)  // 如果可以整除，得到对应的高度
            {
                int height = Mathf.FloorToInt(totalArea / width);

                // 计算当前的长宽比
                float ratio;
                if ((float)width / height > 1)
                {
                    // 长宽比大于1，直接计算差值
                    ratio = Mathf.Abs((float)width / height - 1);
                }
                else
                {
                    // 长宽比小于1，取倒数后计算差值
                    ratio = Mathf.Abs((float)height / width - 1);
                }

                // 如果当前的长宽比更接近1，则更新最佳结果
                if (ratio < bestRatio)
                {
                    bestRatio = ratio;
                    bestWidth = width;
                    bestHeight = height;
                }
            }
        }

        // 计算最终的长宽比
        float finalRatio = (float)bestWidth / bestHeight;

        //如果最佳长宽比大于3或小于1/3。 设置为正方形
        if (finalRatio > 3 || finalRatio < 1 / 3f)
        {
            // 设置为正方形（宽高相等）
            bestWidth = Mathf.FloorToInt(Mathf.Sqrt(totalArea));
            bestHeight = bestWidth;
        }


        // 设置最佳的宽度和高度
        totalWidth = bestWidth;
        totalHeight = bestHeight;
    }

    // 方形树图法划分房间
    //areas 指需要划分的房间，
    //start是当前区域的数组的起始点，end是数组的结束点，
    //x，z分别表示当前划分区域的左下角，width是当前区域的长（x方向），height是当前区域的高（z方向），totalArea是当前区域的总面积大小
    // isHorizontal 如果是1，则表示该区域使用的是竖直划分，即z/x>1;反之则z/x<1，在下一步划分当中采用相反的划分方法
    void CreateRoomRects(float[] areas, int start, int end, float x, float z, float width, float height, float totalArea,bool isHorizontal)
    {

        // 输出当前递归的参数信息到 Unity 控制台
       /* Debug.Log("........................................");
        Debug.Log("Calling CreateRoomRects:");
        Debug.Log($"  start: {start}, end: {end}");
        Debug.Log($"  x: {x}, z: {z}");
        Debug.Log($"  width: {width}, height: {height}");
        Debug.Log($"  totalArea: {totalArea}, isHorizontal: {isHorizontal}");
        Debug.Log("........................................");*/


        if (start >= end) return;
        // 如果只有一个房间，直接生成矩形，不再递归
      if (end - start == 1)
        {
            GameObject room = CreateRoom(x, z, width, height);
            AddRoomToList(room);
            return;
        }

        // 找到最佳划分点，使两部分面积最接近
        // 我们想要找到一个划分点，使得从 start 到 splitIndex 的面积总和尽可能接近 currentArea / 2。
        // 这里最初是采用的方形树图方法，但发现单纯的方形树图会使小房间全部生成在一个角落，这并不符合我们的预期，因此进行了一定的改进
        // 我们依据
        int splitIndex = start;
        float splitArea = 0; //划分后的面积


        float targetArea = totalArea / 5; //目标面积,
                                           //这里暂时将划分点改为总面积的1/10，以便于后面连通图的产生。连通图大致会最多划分为10个区域，所以产生的房间数最少应为10个，2025.1.1 目前还在考虑

        float minDifference = float.MaxValue; // 用来记录与目标面积的最小差距
        float currentArea = 0;//当前划分的面积


        for (int i = start; i < end; i++)//求出划分点
        {
            currentArea += areas[i];  // 累加当前部分的面积
            float currentDifference = Math.Abs(currentArea - targetArea); // 计算与目标面积的差距

            // 如果当前差距小于最小差距，则更新最佳划分点
            if (currentDifference < minDifference)
            {
                minDifference = currentDifference;
                splitIndex = i;
            }
            // 如果当前划分的面积已经超过了目标面积，可以提前终止
            if (currentArea >= targetArea)
            {
                break;
            }
        }

        //求出划分点之前的所有区域面积
        for(int i=start;i<=splitIndex;i++)
        {
            splitArea += areas[i];
        }



        //splitArea划分出来区域的面积
        // 当前方向的划分
        if (isHorizontal)
        {
            // 水平划分：划分高度
            float splitHeight = (splitArea / totalArea) * height;
            // 下半部分区域（从start到splitIndex）
            float currentX = x;
            // 生成每个小区域
            for (int i = start; i <= splitIndex; i++)
            {
                float roomWidth = (areas[i] / splitArea) * width; // 当前小区域的宽度
                GameObject room = CreateRoom(currentX, z,roomWidth, splitHeight);
                AddRoomToList(room);
                currentX += roomWidth; // 更新下一个区域的y坐标
            }
            //递归生成上半部分
            CreateRoomRects(areas, splitIndex + 1, end, x, z + splitHeight, width, height - splitHeight, totalArea - splitArea, !isHorizontal);
        }
        else

        {
            // 垂直划分：划分宽度
            float splitWidth = (splitArea / totalArea) * width;
            // 左半部分区域（从start到splitIndex）
            float currentZ = z;
            // 生成每个小区域，
            for (int i = start; i <= splitIndex; i++)
            {
                float roomHeight = (areas[i] / splitArea) * height; // 当前小区域的高度
                GameObject room = CreateRoom(x, currentZ, splitWidth, roomHeight);
                AddRoomToList(room);
                currentZ += roomHeight; // 更新下一个区域的x坐标
            }
            //递归生成右半部分
            CreateRoomRects(areas, splitIndex + 1, end, x + splitWidth, z, width - splitWidth, height, totalArea-splitArea, !isHorizontal);
        }
    }

    //程序的主入口
    void Start()
    {
        // 检查是否已经通过 UImanager 设置了房间面积
        if (roomAreas != null && roomAreas.Length > 0)
        {
            GenerateRooms();
        }
        else
        {
            Debug.LogError("房间面积数组为空，无法生成房间！");
        }
    }
}

  