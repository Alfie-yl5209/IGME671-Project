using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Furniture
{
    [SerializeField]
    public string name;

    [SerializeField]
    public List<RoomType> type;

    [SerializeField]
    public Vector2 size;

    [SerializeField]
    public  bool moveable;

    [SerializeField]
    public bool canRotate;

    [SerializeField]
    public bool alongWall;

    [SerializeField]
    public bool mustCenter;

    [SerializeField]
    public GameObject obj;
}

public class FurnitureDatabase : MonoBehaviour
{
    public static FurnitureDatabase database;
    [SerializeField]
    public List<Furniture> data;
    public List<Furniture> loot;

    private void Awake()
    {
        database = this;
    }

    /// <summary>
    /// This method will return a list of furnitures with the specified room type.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public List<Furniture> GetByType(RoomType type)
    {
        List<Furniture> list = new List<Furniture>();
        foreach(Furniture furniture in data)
        {
            //If found curresponding type, add to list
            if(furniture.type.Find(item => item == type) != 0)
            {
                list.Add(furniture);
            }
        }

        if(list.Count <= 0)
        {
            throw new System.Exception("Database does not support '" + type + "' type");
        }

        return list;
    }

    public Furniture GetByName(string s)
    {
        foreach(Furniture furniture in data)
        {
            if (furniture.name == s)
                return furniture;
        }

        return null;
    }

    public Furniture GetRandomLootObj()
    {
        return loot[UnityEngine.Random.Range(0, loot.Count)];
    }
}
