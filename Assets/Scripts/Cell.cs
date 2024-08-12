using UnityEngine;

public struct Cell
{
    //表示格子的状态
    public enum Type
    {
        Invalid,
        Empty,  //空
        Mine,   //一个地雷
        Number  //一个数字，表示周围各自有多少地雷
    }

    public Vector3Int position; //格子的位置
    public Type type;           //type类型
    public int number;          //数字
    public bool revealed;       //是否被显示
    public bool flagged;        //是否被标记
    public bool exploded;       //是炸弹，是否要爆炸
}
