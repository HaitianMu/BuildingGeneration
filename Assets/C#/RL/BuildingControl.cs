using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public partial class BuildingControl : MonoBehaviour
{

    /*........................一、房间生成用到的数据结构.....................................*/
    private float totalArea; // 总区域大小
    public int roomNum=10; //需要生成的房间数量
    public int totalWidth;//用于记录整个区域的宽
    public int totalHeight;//用于记录整个区域的高


    public float[] roomAreas;// 输入的房间面积数组（已知数组）
    public int RoomNum = 0;//记录已经生成的房间数量,用于给房间编号  
    public int doorNum = 1;//记录已经生成的门数量,用于给门编号
    float y = 3.0f;//墙体的高度 ,
    float doorWidth = 1.5f;//门的宽度

    private GameObject[] AllObjects; //存储单次生成的所有物体，这个数据用于多次生成环境时，清空之前的场景

    public Material Floor;
    public Material Door;
    public Material Exit;
    public Material Wall;
    /*.............................二、房间之间生成门用到的数据结构................................*/

    public float minWidth = 2.5f;
    public float minheight = 2.5f;
    public class Room
    {
        //为房间增加一些属性，比如房间的坐标、大小，以及它的邻居列表（用于记录相邻房间）。
        public GameObject roomObject;  // 房间的游戏对象
        public Vector3 XZposition;       // 房间的左下角坐标
        public float width;            // 房间的宽度
        public float height;           // 房间的高度

        /*！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！！3.12日新增*/
        public float roomSize; //用于存放房间的面积大小
        public Room() { }
        public Room(GameObject roomObject, Vector3 position, float width, float height)
        {
            this.roomObject = roomObject;
            this.XZposition = position;
            this.width = width;
            this.height = height;
        }

        public Room( Vector3 position, float width, float height,float roomsize) {
            this.XZposition = position;
            this.width = width;
            this.height = height;
            roomSize = roomsize;
        }
        // 计算房间的对角线长度（权重）
        
        // 方法：检查与另一个房间是否相邻（基于房间位置和大小）
        public bool IsAdjacentTo(Room other)  //判断该房间是否与另外一个房间相邻,当两个房间相邻的长度大于Distance时，我们才认为这两个房间相邻
        {
            // 假设房间是矩形的，我们检查是否有一个相邻的面
            float Distance = 5.0f; 
            bool isAdjacent = false;

            // 检查左右相邻（假设x为水平方向，y为垂直方向，z为深度方向）
            if (Mathf.Abs(this.XZposition.x + this.width - other.XZposition.x) < 0.1f || Mathf.Abs(other.XZposition.x + other.width - this.XZposition.x) < 0.1f)
            {
                // 如果两个房间在 x 轴方向上相邻,检查两个房间相邻部分在z轴方向的差值.如果<2，那么我们不认为这两个房间是相邻的,因为有 1 的距离要用来放门
                //总体可分为三种情况
                if (other.XZposition.z <= this.XZposition.z && other.XZposition.z+other.height>=this.XZposition.z && other.XZposition.z+other.height-this.XZposition.z >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if(other.XZposition.z <= this.XZposition.z+height && other.XZposition.z + other.height >= this.XZposition.z+height && this.XZposition.z + this.height - other.XZposition.z >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if(other.XZposition.z >= this.XZposition.z && other.XZposition.z + other.height <= this.XZposition.z + this.height && other.height >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                if (other.XZposition.z <= XZposition.z || other.XZposition.z + other.height >= XZposition.z + height || height >= 0.2)
                {

                }
                else { return isAdjacent; }

            }
            // 检查上下相邻
            else if (Mathf.Abs(this.XZposition.z + this.height - other.XZposition.z) < 0.1f || Mathf.Abs(other.XZposition.z + other.height - this.XZposition.z) < 0.1f)
            {
                // 如果两个房间在 z 轴方向上相邻，检查两个房间相邻部分在x轴方向的差值.如果<2，那么我们不认为这两个房间是相邻的,因为有 1 的距离要用来放门
                if (other.XZposition.x<= this.XZposition.x && other.XZposition.x+other.width>=this.XZposition.x && other.XZposition.x + other.width-this.XZposition.x>=Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if (other.XZposition.x <= this.XZposition.x+this.width && other.XZposition.x + other.width >= this.XZposition.x+this.width && XZposition.x + width - other.XZposition.x >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if (other.XZposition.x >= this.XZposition.x && other.XZposition.x + other.width <= this.XZposition.x+width && other.width  >= Distance)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }
                else if (other.XZposition.x <= XZposition.x && other.XZposition.x + other.width >= XZposition.x + width && width >= 0.2)
                {
                    isAdjacent = true;
                    return isAdjacent;
                }

                else { return isAdjacent; }
            }
            return isAdjacent;
        }

    }


    public List<Room> roomList = new List<Room>();  // 存储生成的所有房间对象

    /*.............................三、生成导航使用的数据结构................................*/

    public GameObject ParentObject;

    }
