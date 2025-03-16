using System;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
public partial class BuildingControl : MonoBehaviour
{

    /*........................һ�����������õ������ݽṹ.....................................*/
    private float totalArea; // �������С
    public int roomNum=10; //��Ҫ���ɵķ�������
    public int totalWidth;//���ڼ�¼��������Ŀ�
    public int totalHeight;//���ڼ�¼��������ĸ�


    public float[] roomAreas;// ����ķ���������飨��֪���飩
    public int RoomNum = 0;//��¼�Ѿ����ɵķ�������,���ڸ�������  
    public int doorNum = 1;//��¼�Ѿ����ɵ�������,���ڸ��ű��
    float y = 3.0f;//ǽ��ĸ߶� ,
    float doorWidth = 1.5f;//�ŵĿ��

    private GameObject[] AllObjects; //�洢�������ɵ��������壬����������ڶ�����ɻ���ʱ�����֮ǰ�ĳ���

    public Material Floor;
    public Material Door;
    public Material Exit;
    public Material Wall;
    /*.............................��������֮���������õ������ݽṹ................................*/

    public float minWidth = 2.5f;
    public float minheight = 2.5f;
    public class Room
    {
        //Ϊ��������һЩ���ԣ����緿������ꡢ��С���Լ������ھ��б����ڼ�¼���ڷ��䣩��
        public GameObject roomObject;  // �������Ϸ����
        public Vector3 XZposition;       // ��������½�����
        public float width;            // ����Ŀ��
        public float height;           // ����ĸ߶�

        /*������������������������������������������������������������������������3.12������*/
        public float roomSize; //���ڴ�ŷ���������С
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
        // ���㷿��ĶԽ��߳��ȣ�Ȩ�أ�
        
        // �������������һ�������Ƿ����ڣ����ڷ���λ�úʹ�С��
        public bool IsAdjacentTo(Room other)  //�жϸ÷����Ƿ�������һ����������,�������������ڵĳ��ȴ���Distanceʱ�����ǲ���Ϊ��������������
        {
            // ���跿���Ǿ��εģ����Ǽ���Ƿ���һ�����ڵ���
            float Distance = 5.0f; 
            bool isAdjacent = false;

            // ����������ڣ�����xΪˮƽ����yΪ��ֱ����zΪ��ȷ���
            if (Mathf.Abs(this.XZposition.x + this.width - other.XZposition.x) < 0.1f || Mathf.Abs(other.XZposition.x + other.width - this.XZposition.x) < 0.1f)
            {
                // ������������� x �᷽��������,��������������ڲ�����z�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
                //����ɷ�Ϊ�������
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
            // �����������
            else if (Mathf.Abs(this.XZposition.z + this.height - other.XZposition.z) < 0.1f || Mathf.Abs(other.XZposition.z + other.height - this.XZposition.z) < 0.1f)
            {
                // ������������� z �᷽�������ڣ���������������ڲ�����x�᷽��Ĳ�ֵ.���<2����ô���ǲ���Ϊ���������������ڵ�,��Ϊ�� 1 �ľ���Ҫ��������
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


    public List<Room> roomList = new List<Room>();  // �洢���ɵ����з������

    /*.............................�������ɵ���ʹ�õ����ݽṹ................................*/

    public GameObject ParentObject;

    }
