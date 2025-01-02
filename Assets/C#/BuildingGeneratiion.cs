using System;
using UnityEngine;
public class BuildingGeneratiion : MonoBehaviour
{

    // ����ķ���������飨��֪���飩
    public float[] roomAreas;

    public int num = 0;//��¼�Ѿ����ɵķ�������
    // �������С
    private float totalArea;
    public int totalWidth;//���ڼ�¼��������Ŀ�
    public int totalHeight;//���ڼ�¼��������ĸ�

    // ���ڴ洢���ɵķ������
    private GameObject[] generatedRooms;

    // ����һ�����η���



    // ����һ�����η���
    /* public GameObject CreateRoom(float x, float z, float width, float height)
     {
         // Debug.Log($"�������ɷ����λ���ǣ� x: {x}, z: {z}, width: {width}, height: {height}");
         // ����һ���µ��������ʾ����
         GameObject room = GameObject.CreatePrimitive(PrimitiveType.Cube);

         // ���÷����λ�úʹ�С
         room.transform.position = new Vector3(x + width / 2, 0f, z + height / 2); // �߶ȹ̶��� 0
         room.transform.localScale = new Vector3(width, 1f, height); // �߶� 1����Ӧ x-z ƽ��

         // �����������ɫ
         room.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
         // �������ɵķ������
         return room;
     }*/
    //ǽ������
    public GameObject CreateRoom(float x, float z, float width, float height)
    {
        // �������������
        GameObject room = new GameObject("Room");

        // ���ɷ���ĵײ��������Ҫ�Ļ���������ӵײ�����Ϊ������棩
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.transform.parent = room.transform;
        floor.transform.position = new Vector3(x + width / 2, 0f, z + height / 2);
        floor.transform.localScale = new Vector3(width, 0.1f, height); // �ײ���Ⱥ͸߶�

        // ������ײ���ɫ
        floor.GetComponent<Renderer>().material.color = new Color(0.5f, 0.3f, 0.2f); ; // ��ɫ�����Զ���
        AddRoomToList(floor); // ��������뵽�����б���
                              // ����ĸ�ǽ��
        CreateWall(x, z, width, height, room);
       /* // �������������һ����ɫ��������������
        room.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);*/

        // �������ɵķ������
        return room;
    }

    void CreateWall(float x, float z, float width, float height, GameObject room)
    {
        // ǽ�ڵĺ�ȣ����Ե�����Խ��ǽ��Խ��
        float wallThickness = 0.1f;
        float y = 3.0f;//ǽ��ĸ߶�
        // ��������ǽ�壺�ĸ�����
        // 1. ��ǽ
        GameObject leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.transform.parent = room.transform;
        leftWall.transform.position = new Vector3(x + wallThickness / 2, y / 2, z + height / 2);
        leftWall.transform.localScale = new Vector3(wallThickness, y, height);
        leftWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // ǽ����ɫ


        // 2. ��ǽ
        GameObject rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.transform.parent = room.transform;
        rightWall.transform.position = new Vector3(x + width - wallThickness / 2, y / 2, z + height / 2);
        rightWall.transform.localScale = new Vector3(wallThickness, y, height);
        rightWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // ǽ����ɫ

        // 3. ǰǽ
        GameObject frontWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frontWall.transform.parent = room.transform;
        frontWall.transform.position = new Vector3(x + width / 2, y / 2, z + wallThickness / 2);
        frontWall.transform.localScale = new Vector3(width, y, wallThickness);
        frontWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // ǽ����ɫ

        // 4. ��ǽ
        GameObject backWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        backWall.transform.parent = room.transform;
        backWall.transform.position = new Vector3(x + width / 2, y / 2, z + height - wallThickness / 2);
        backWall.transform.localScale = new Vector3(width, y, wallThickness);
        backWall.GetComponent<Renderer>().material.color = new Color(0.5f, 0.7f, 1f); // ǽ����ɫ

    }



    // �����ɵķ�����ӵ������б�
    public void AddRoomToList(GameObject room)
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
        Debug.Log("Total Area: " + totalArea);
        Debug.Log("Total Width: " + totalWidth);
        Debug.Log("Total Height: " + totalHeight);

        // ʹ�÷�����ͼ���ɷ���
        CreateRoomRects(roomAreas, 0, roomAreas.Length, 0, 0, totalWidth, totalHeight, totalArea, (totalHeight / (float)totalWidth) > 1);
    }
    // ����ϴ����ɵķ���
    void ClearPreviousRooms()
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
    // �ҳ��������ӽ�1�Ŀ����ϣ����BestRatio>3��<1/3���򽫸���������Ϊ������
    void FindBestDimensions(float totalArea)
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

    // ������ͼ�����ַ���
    //areas ָ��Ҫ���ֵķ��䣬
    //start�ǵ�ǰ������������ʼ�㣬end������Ľ����㣬
    //x��z�ֱ��ʾ��ǰ������������½ǣ�width�ǵ�ǰ����ĳ���x���򣩣�height�ǵ�ǰ����ĸߣ�z���򣩣�totalArea�ǵ�ǰ������������С
    // isHorizontal �����1�����ʾ������ʹ�õ�����ֱ���֣���z/x>1;��֮��z/x<1������һ�����ֵ��в����෴�Ļ��ַ���
    void CreateRoomRects(float[] areas, int start, int end, float x, float z, float width, float height, float totalArea,bool isHorizontal)
    {

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
        for(int i=start;i<=splitIndex;i++)
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
                GameObject room = CreateRoom(currentX, z,roomWidth, splitHeight);
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
            CreateRoomRects(areas, splitIndex + 1, end, x + splitWidth, z, width - splitWidth, height, totalArea-splitArea, !isHorizontal);
        }
    }

    //����������
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
    }
}

  