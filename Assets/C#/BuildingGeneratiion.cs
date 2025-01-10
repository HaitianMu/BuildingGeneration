using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static BuildingGeneratiion;
public class BuildingGeneratiion : MonoBehaviour
{

    /*........................һ�����������õ������ݽṹ.....................................*/
    public float[] roomAreas;// ����ķ���������飨��֪���飩
    public int num = 1;//��¼�Ѿ����ɵķ�������,���ڸ�������  
    private float totalArea; // �������С
    public int totalWidth;//���ڼ�¼��������Ŀ�
    public int totalHeight;//���ڼ�¼��������ĸ�
    float y = 3.0f;//ǽ��ĸ߶� ,
    private GameObject[] generatedRooms; //�洢���ɵķ����������������ڶ�����ɻ���ʱ��ɾ��֮ǰ���ɵ�object


    /*.............................��������֮���������õ������ݽṹ................................*/
    public class Room
    {
        //Ϊ��������һЩ���ԣ����緿������ꡢ��С���Լ������ھ��б����ڼ�¼���ڷ��䣩��
        public GameObject roomObject;  // �������Ϸ����
        public Vector3 position;       // ��������½�����
        public float width;            // ����Ŀ��
        public float height;           // ����ĸ߶�
        public List<Room> neighbors;   // �ھ��б��洢���ڵķ���
        public bool isConnected;       // ��Ǹ÷����Ƿ�������������ͨ 

        public Room(GameObject roomObject, Vector3 position, float width, float height)
        {
            this.roomObject = roomObject;
            this.position = position;
            this.width = width;
            this.height = height;
            this.neighbors = new List<Room>();
            this.isConnected = false;
        }
    }
    private List<Room> roomList = new List<Room>();  // �洢���ɵ����з���

    /*.....................................һ������UI�������������ͷ�����Ŀ��Ȼ����з���İڷźͷ���ǽ�ڵ�����.......................................*/
    public GameObject CreateRoom(float x, float z, float width, float height)
    {
        // �������������
        GameObject room = new GameObject("Room" + num);
        num++;

        // ���ɷ���ĵײ��������Ҫ�Ļ���������ӵײ�����Ϊ������棩
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "floor";
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(x + width / 2, 0f, z + height / 2);
        floor.transform.localScale = new Vector3(width, 0.1f, height); // �ײ���Ⱥ͸߶�

        // ������ײ���ɫ
        floor.GetComponent<Renderer>().material.color = new UnityEngine.Color(0.5f, 0.3f, 0.2f);  // ��ɫ�����Զ���
        AddRoomToList(floor); // ��������뵽�����б���
                              // �����ĸ�ǽ��
        CreateWall(x, z, width, height, room);

        // ���·�����뷿���б�,����¼ÿ�������λ�úʹ�С
        Room newRoom = new Room(room, new Vector3(x, 0f, z), width, height);
        roomList.Add(newRoom);  // ���·�����뷿���б�


        return room; // �������ɵķ������
    }
    void CreateWall(float x, float z, float width, float height, GameObject room)
    {
        // ǽ�ڵĺ�ȣ����Ե�����Խ��ǽ��Խ��
        float wallThickness = 0.1f;
        // ��������ǽ�壺�ĸ�����
        // 1. ��ǽ
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "leftWall";
        leftWall.transform.parent = room.transform;
        leftWall.transform.position = new Vector3(x + wallThickness / 2, y / 2, z + height / 2);
        leftWall.transform.localScale = new Vector3(wallThickness, y, height);
        leftWall.GetComponent<Renderer>().material.color = new UnityEngine.Color(0.5f, 0.7f, 1f); // ǽ����ɫ


        // 2. ��ǽ
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "rightWall";
        rightWall.transform.parent = room.transform;
        rightWall.transform.position = new Vector3(x + width - wallThickness / 2, y / 2, z + height / 2);
        rightWall.transform.localScale = new Vector3(wallThickness, y, height);
        rightWall.GetComponent<Renderer>().material.color = new UnityEngine.Color(0.5f, 0.7f, 1f); // ǽ����ɫ

        // 3. ��ǽ
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.name = "backWall";
        frontWall.transform.parent = room.transform;
        frontWall.transform.position = new Vector3(x + width / 2, y / 2, z + wallThickness / 2);
        frontWall.transform.localScale = new Vector3(width, y, wallThickness);
        frontWall.GetComponent<Renderer>().material.color = new UnityEngine.Color(0.5f, 0.7f, 1f); // ǽ����ɫ

        // 4. ǰǽ
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.name = "frontWall";
        backWall.transform.parent = room.transform;
        backWall.transform.position = new Vector3(x + width / 2, y / 2, z + height - wallThickness / 2);
        backWall.transform.localScale = new Vector3(width, y, wallThickness);
        backWall.GetComponent<Renderer>().material.color = new UnityEngine.Color(0.5f, 0.7f, 1f); // ǽ����ɫ

    }
    public void AddRoomToList(GameObject room)// �����ɵķ�����ӵ������б�
    {
        // �������ɵķ�����뵽�����б���
        Array.Resize(ref generatedRooms, generatedRooms.Length + 1);
        generatedRooms[generatedRooms.Length - 1] = room;
    }
    public void GenerateRooms()
    {
        foreach (float part in roomAreas)
        {
            Debug.Log("�����������ܵ������ݣ�" + part);
        }
        // ����ϴ����ɵķ���
        num = 1;  //���÷��������
        ClearPreviousRooms();
        // ���������
        totalArea = 0f;
        foreach (var area in roomAreas)
        {
            totalArea += area;
        }

        // �ҳ���ӽ������εĳ������
        FindBestDimensions(totalArea);
        // ����������������
        /* Debug.Log("Total Area: " + totalArea);
         Debug.Log("Total Width: " + totalWidth);
         Debug.Log("Total Height: " + totalHeight);*/

        // ʹ�÷�����ͼ���ɷ���
        CreateRoomRects(roomAreas, 0, roomAreas.Length, 0, 0, totalWidth, totalHeight, totalArea, (totalHeight / (float)totalWidth) > 1);
        // ��������֮����ţ�ȷ��������ͨ
       // CreateDoors();

        //�����һ��������������ǽ����������������Ϊ��������
        AddExitDoors(roomList[roomList.Count - 1]);
    }

    void ClearPreviousRooms()// ����ϴ����ɵķ���
        {
            // ��� generatedRooms �Ƿ�Ϊ null������ǣ���ʼ��Ϊһ��������
            if (generatedRooms == null)
            {
                generatedRooms = new GameObject[0];
            }

            // ���������Ѿ����ɵķ����������
            foreach (var room in generatedRooms)
            {
                if (room != null)
                {
                    Destroy(room);
                }
            }

            // ��շ����б�
            generatedRooms = new GameObject[0];
        }
    void FindBestDimensions(float totalArea)// �ҳ��������ӽ�1�Ŀ����ϣ����BestRatio>3��<1/3���򽫸���������Ϊ������
        {
            int bestWidth = 0;
            int bestHeight = 0;
            float bestRatio = float.MaxValue;  // ��ӽ�1�ı�ֵ

            // �������п��ܵĿ��ֵ
            for (int width = 1; width <= Mathf.FloorToInt(Mathf.Sqrt(totalArea)); width++)
            {
                if (totalArea % width == 0)  // ��������������õ���Ӧ�ĸ߶�
                {
                    int height = Mathf.FloorToInt(totalArea / width);

                    // ���㵱ǰ�ĳ����
                    float ratio;
                    if ((float)width / height > 1)
                    {
                        // ����ȴ���1��ֱ�Ӽ����ֵ
                        ratio = Mathf.Abs((float)width / height - 1);
                    }
                    else
                    {
                        // �����С��1��ȡ����������ֵ
                        ratio = Mathf.Abs((float)height / width - 1);
                    }

                    // �����ǰ�ĳ���ȸ��ӽ�1���������ѽ��
                    if (ratio < bestRatio)
                    {
                        bestRatio = ratio;
                        bestWidth = width;
                        bestHeight = height;
                    }
                }
            }

            // �������յĳ����
            float finalRatio = (float)bestWidth / bestHeight;

            //�����ѳ���ȴ���3��С��1/3�� ����Ϊ������
            if (finalRatio > 3 || finalRatio < 1 / 3f)
            {
                // ����Ϊ�����Σ������ȣ�
                bestWidth = Mathf.FloorToInt(Mathf.Sqrt(totalArea));
                bestHeight = bestWidth;
            }


            // ������ѵĿ�Ⱥ͸߶�
            totalWidth = bestWidth;
            totalHeight = bestHeight;
        }
    void CreateRoomRects(float[] areas, int start, int end, float x, float z, float width, float height, float totalArea, bool isHorizontal)
        {
            // ������ͼ�����ַ���
            //areas ָ��Ҫ���ֵķ��䣬
            //start�ǵ�ǰ������������ʼ�㣬end������Ľ����㣬
            //x��z�ֱ��ʾ��ǰ������������½ǣ�width�ǵ�ǰ����ĳ���x���򣩣�height�ǵ�ǰ����ĸߣ�z���򣩣�totalArea�ǵ�ǰ������������С
            // isHorizontal �����1�����ʾ������ʹ�õ�����ֱ���֣���z/x>1;��֮��z/x<1������һ�����ֵ��в����෴�Ļ��ַ���


            // �����ǰ�ݹ�Ĳ�����Ϣ�� Unity ����̨
            /* Debug.Log("........................................");
             Debug.Log("Calling CreateRoomRects:");
             Debug.Log($"  start: {start}, end: {end}");
             Debug.Log($"  x: {x}, z: {z}");
             Debug.Log($"  width: {width}, height: {height}");
             Debug.Log($"  totalArea: {totalArea}, isHorizontal: {isHorizontal}");
             Debug.Log("........................................");*/


            if (start >= end) return;
            // ���ֻ��һ�����䣬ֱ�����ɾ��Σ����ٵݹ�
            if (end - start == 1)
            {
                GameObject room = CreateRoom(x, z, width, height);
                AddRoomToList(room);
                return;
            }

            // �ҵ���ѻ��ֵ㣬ʹ�����������ӽ�
            // ������Ҫ�ҵ�һ�����ֵ㣬ʹ�ô� start �� splitIndex ������ܺ;����ܽӽ� currentArea / 2��
            // ��������ǲ��õķ�����ͼ�����������ֵ����ķ�����ͼ��ʹС����ȫ��������һ�����䣬�Ⲣ���������ǵ�Ԥ�ڣ���˽�����һ���ĸĽ�
            // ��������
            int splitIndex = start;
            float splitArea = 0; //���ֺ�����


            float targetArea = totalArea / 5; //Ŀ�����,
                                              //������ʱ�����ֵ��Ϊ�������1/10���Ա��ں�����ͨͼ�Ĳ�������ͨͼ���»���໮��Ϊ10���������Բ����ķ���������ӦΪ10����2025.1.1 Ŀǰ���ڿ���

            float minDifference = float.MaxValue; // ������¼��Ŀ���������С���
            float currentArea = 0;//��ǰ���ֵ����


            for (int i = start; i < end; i++)//������ֵ�
            {
                currentArea += areas[i];  // �ۼӵ�ǰ���ֵ����
                float currentDifference = Math.Abs(currentArea - targetArea); // ������Ŀ������Ĳ��

                // �����ǰ���С����С��࣬�������ѻ��ֵ�
                if (currentDifference < minDifference)
                {
                    minDifference = currentDifference;
                    splitIndex = i;
                }
                // �����ǰ���ֵ�����Ѿ�������Ŀ�������������ǰ��ֹ
                if (currentArea >= targetArea)
                {
                    break;
                }
            }

            //������ֵ�֮ǰ�������������
            for (int i = start; i <= splitIndex; i++)
            {
                splitArea += areas[i];
            }



            //splitArea���ֳ�����������
            // ��ǰ����Ļ���
            if (isHorizontal)
            {
                // ˮƽ���֣����ָ߶�
                float splitHeight = (splitArea / totalArea) * height;
                // �°벿�����򣨴�start��splitIndex��
                float currentX = x;
                // ����ÿ��С����
                for (int i = start; i <= splitIndex; i++)
                {
                    float roomWidth = (areas[i] / splitArea) * width; // ��ǰС����Ŀ��
                    GameObject room = CreateRoom(currentX, z, roomWidth, splitHeight);
                    AddRoomToList(room);
                    currentX += roomWidth; // ������һ�������y����
                }
                //�ݹ������ϰ벿��
                CreateRoomRects(areas, splitIndex + 1, end, x, z + splitHeight, width, height - splitHeight, totalArea - splitArea, !isHorizontal);
            }
            else

            {
                // ��ֱ���֣����ֿ��
                float splitWidth = (splitArea / totalArea) * width;
                // ��벿�����򣨴�start��splitIndex��
                float currentZ = z;
                // ����ÿ��С����
                for (int i = start; i <= splitIndex; i++)
                {
                    float roomHeight = (areas[i] / splitArea) * height; // ��ǰС����ĸ߶�
                    GameObject room = CreateRoom(x, currentZ, splitWidth, roomHeight);
                    AddRoomToList(room);
                    currentZ += roomHeight; // ������һ�������x����
                }
                //�ݹ������Ұ벿��
                CreateRoomRects(areas, splitIndex + 1, end, x + splitWidth, z, width - splitWidth, height, totalArea - splitArea, !isHorizontal);
            }
        }

        /*.......................................���������ɵķ���֮��������.....................................*/
    bool AreRoomsAdjacent(Room roomA, Room roomB)
        {
            // ������������� X ��� Z �������ڣ���û�м�϶
            bool isAdjacentX = Mathf.Abs(roomA.position.x + roomA.width - roomB.position.x) < 0.1f || Mathf.Abs(roomB.position.x + roomB.width - roomA.position.x) < 0.1f;
            bool isAdjacentZ = Mathf.Abs(roomA.position.z + roomA.height - roomB.position.z) < 0.1f || Mathf.Abs(roomB.position.z + roomB.height - roomA.position.z) < 0.1f;

            return isAdjacentX || isAdjacentZ;
        }
    void CreateDoorBetweenRooms(Room roomA, Room roomB) // �����ŵ��߼��������ڷ���֮��������
        {
           
        }
    void AddExitDoors(Room EscapeRoom) //�����һ��������Ҳ���Ϸ�������
      {         
            float wallHeight = y;//�ŵĸ߶�
            // �ҷ��ŵ�λ��
            Vector3 RightDoorPosition = new Vector3(EscapeRoom.position.x + EscapeRoom.width, wallHeight / 2, EscapeRoom.position.z+ EscapeRoom.height / 2);  
            // �Ϸ��ŵ�λ��
            Vector3 FrontDoorPosition = new Vector3(EscapeRoom.position.x + EscapeRoom.width/2, wallHeight / 2, EscapeRoom.position.z + EscapeRoom.height);

        // �����ҷ���������
        DivideWall(EscapeRoom.roomObject.transform.Find("rightWall"), "rightWall");
        CreateDoor(RightDoorPosition, 0.1f, 2, true, "Exit");
        //�����Ϸ���������
        DivideWall(EscapeRoom.roomObject.transform.Find("frontWall"), "frontWall");
        CreateDoor(FrontDoorPosition, 0.1f, 2, false, "Exit");
            
           //Transform child = transform.Find("ChildName");
    }
    void CreateDoor(Vector3 position, float width, float height, bool isHorizontal, String Tag)
        {
            // ����һ���µ������壨�ţ�
            GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);

            // �����ŵ����ƺͱ�ǩ
            door.name = Tag;
            door.tag = Tag;

            // �����ŵ�λ��
            door.transform.position = position;

            // �ж��Ƿ��Ǻ�����
            if (isHorizontal)
            {
                // ����Ǻ����ţ������ŵĴ�С
                // 0.3f ���ŵĺ�ȣ�y-1 ���ŵĸ߶ȣ�1f ���ŵĿ��
                door.transform.localScale = new Vector3(0.3f, y , 1f);

                // �����ŵ���ɫΪ��ɫ
                door.GetComponent<Renderer>().material.color = new UnityEngine.Color(0, 0, 0);  // ��ɫ
            }
            else
            {
                // ����������ţ������ŵĴ�С
                // 1f ���ŵĿ�ȣ�y-1 ���ŵĸ߶ȣ�0.3f ���ŵĺ��
                door.transform.localScale = new Vector3(1f, y, 0.3f);

                // �����ŵ���ɫΪ��ɫ
                door.GetComponent<Renderer>().material.color = new UnityEngine.Color(0, 0, 0);  // ��ɫ
            }

            // ������ӵ������б���
            AddRoomToList(door);
        }
    void DivideWall(Transform wall, String WallName)
    { //����ǽ�ڣ���ǽ�ڷ�Ϊ�����֣����ŵĲ�λ������������ǽ�ڵ�������������λ���
     
      // ǽ�ڵĲ���
        float doorWidth = 1.0f;  // �ŵĿ��
 
        // ����ǽ�����ƻ��ֲ�ͬ��ǽ
        if (WallName == "leftWall" || WallName == "rightWall")
        {
            // �����ϰ벿��ǽ��
            GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frontWall.transform.parent = wall.transform.parent;
            frontWall.name = WallName;
            frontWall.transform.position = new Vector3(wall.position.x, wall.position.y, wall.position.z + (doorWidth + wall.localScale.z) / 4);
            frontWall.transform.localScale = new Vector3(wall.localScale.x, wall.localScale.y, (wall.localScale.z - doorWidth) / 2);

            // �����°벿��ǽ��
            GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backWall.transform.parent = wall.transform.parent;
            backWall.name = WallName;
            backWall.transform.position = new Vector3(wall.position.x, wall.position.y, wall.position.z - (doorWidth + wall.localScale.z) / 4);
            backWall.transform.localScale = new Vector3(wall.localScale.x, wall.localScale.y, (wall.localScale.z - doorWidth) / 2);

            // ����ԭǽ
            Destroy(wall.gameObject);
        }
        else if (WallName == "frontWall" || WallName == "backWall")
        {
            // ������벿��ǽ��
            GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWall.transform.parent = wall.transform.parent;
            leftWall.name = WallName;
            leftWall.transform.position = new Vector3(wall.position.x - (doorWidth+wall.localScale.x) / 4, wall.position.y, wall.position.z);
            leftWall.transform.localScale = new Vector3((wall.localScale.x - doorWidth) / 2, wall.localScale.y, wall.localScale.z);

            // �����Ұ벿��ǽ��
            GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWall.transform.parent = wall.transform.parent;
            rightWall.name = WallName;
            rightWall.transform.position = new Vector3(wall.position.x + (doorWidth + wall.localScale.x) / 4, wall.position.y, wall.position.z);
            rightWall.transform.localScale = new Vector3((wall.localScale.x - doorWidth) / 2, wall.localScale.y, wall.localScale.z);
            // ����ԭǽ
            Destroy(wall.gameObject);
        }
    }
        /*  //����������,��UImanage���洴����һ��BuildingGeneration�ֱ࣬�ӵ����˸����GenerateRoom����
          void Start()
          {
              // ����Ƿ��Ѿ�ͨ�� UImanager �����˷������
              if (roomAreas != null && roomAreas.Length > 0)
              {
                  GenerateRooms();
              }
              else
              {
                  Debug.LogError("�����������Ϊ�գ��޷����ɷ��䣡");
              }
          }*/
    }




