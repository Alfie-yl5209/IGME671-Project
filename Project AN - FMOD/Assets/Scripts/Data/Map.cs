using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class Helpers
{
    public static int FindMaxValue(List<int> list)
    {
        int target = -1;

        if (list.Count < 1)
            return target;

        target = list[0];

        foreach (int i in list)
        {
            if (target < i)
                target = i;
        }

        return target;
    }

    public static int FindMinValue(List<int> list)
    {
        int target = -1;

        if (list.Count < 1)
            return target;

        target = list[0];

        foreach (int i in list)
        {
            if (target > i)
                target = i;
        }

        return target;
    }

    public static int GenerateSeed(string s)
    {
        long seed = 0;
        int final = 0;
        string seed_s = "";

        for (int i = 0; i < s.Length; i++)
        {
            seed_s += ((int)s[i] - 48).ToString();
        }

        seed = Int64.Parse(seed_s);

        final = (int)seed;

        Debug.Log(seed_s);
        Debug.Log(final);

        return final;
    }
}
public enum Direction
{
    Up,
    Down,
    Right,
    Left
}
public enum WallType
{
    None,
    Wall,
    Door,
    Broken
}
public enum RoomType
{
    None,
    Room,
    Hallway,
    Elevator,
    Upgrade,
    Key,
    Craft,
    Recycle,
    Treasure
}
public class Block
{
    public int room;
    public WallType[] walls;
    public Block[] neighbors;

    public Block()
    {
        room = 0;
        walls = new WallType[4] { WallType.None, WallType.None, WallType.None, WallType.None };
        neighbors = new Block[4] { null, null, null, null };
    }

    public void Reset()
    {
        room = 0;
        walls = new WallType[4] { WallType.None, WallType.None, WallType.None, WallType.None };
    }
}
public class Room
{
    public List<Block> blocks;
    public List<GameObject> objects;

    public int room_id;
    public int size;
    public RoomType type;

    private int exit_num;
    private int connectivity;

    public Room()
    {
        blocks = new List<Block>();
        objects = new List<GameObject>();

        room_id = 0;
        size = 0;
        connectivity = 0;
        exit_num = 0;
        type = RoomType.None;
    }

    public Room(int num)
    {
        blocks = new List<Block>();
        objects = new List<GameObject>();

        room_id = num;
        size = 0;
        connectivity = 0;
        exit_num = 0;
        type = RoomType.None;
    }

    public void addBlock(Block b)
    {
        if (blocks != null)
        {
            blocks.Add(b);
            b.room = room_id;
            size++;
        }
    }

    public void CalculateRoomInfo()
    {
        foreach (Block b in blocks)
        {
            foreach (Block n in b.neighbors)
            {
                if (b.room == n.room)
                {
                    connectivity++;
                }
            }

            foreach (WallType w in b.walls)
            {
                if (w == WallType.Door)
                    exit_num++;
            }
        }
    }

    public void GenerateRoomType()
    {
        //Only has one exit
        if (exit_num == 1)
        {
            type = RoomType.Room;
            return;
        }

        //Calculate connectivity and determine room size base on the formula
        int temp = 0;

        if (size == 2)
            temp = 2;
        else
            temp = 2 * (size - 1);

        if (temp == connectivity)
            type = RoomType.Hallway;
        else
            type = RoomType.Room;
    }
}
public class Map
{
    public Block[,] blocks;
    public List<Block> active_blocks;
    public List<Room> rooms;

    public int border_size;
    public int block_num;
    public int room_num;

    public Map(int size)
    {
        border_size = size;
        blocks = new Block[border_size, border_size];
        rooms = new List<Room>();
        active_blocks = new List<Block>();
        room_num = 0;
        block_num = 0;

        //Generate 2D array
        for (int i = 0; i < border_size; i++)
        {
            for (int j = 0; j < border_size; j++)
            {
                blocks[i, j] = new Block();
            }
        }

        //Make it a 2D graph
        for (int i = 0; i < border_size; i++)
        {
            for (int j = 0; j < border_size; j++)
            {
                //up
                if (j + 1 < border_size)
                {
                    blocks[i, j].neighbors[0] = blocks[i, j + 1];
                }

                //down
                if (j - 1 > -1)
                {
                    blocks[i, j].neighbors[2] = blocks[i, j - 1];
                }

                //right
                if (i + 1 < border_size)
                {
                    blocks[i, j].neighbors[1] = blocks[i + 1, j];
                }

                //left
                if (i - 1 > -1)
                {
                    blocks[i, j].neighbors[3] = blocks[i - 1, j];
                }
            }
        }
    }

    //Generate dungeon information
    public void Generate(int size, int min_room_size, int max_room_size, int min_hallway_size, int max_hallway_size, float connectivity, bool isDynamic)
    {
        //First room
        DrawRoom(border_size / 2, border_size / 2, 3, 3);

        //Draw other rooms
        while (block_num < size)
        {
            DrawHallway(FindDrawPoint(), UnityEngine.Random.Range(min_hallway_size, max_hallway_size));
            DrawRoom(FindDrawPoint(), UnityEngine.Random.Range(min_room_size, max_room_size));
        }

        //Draw walls and doors
        DrawWalls();
        DrawDoors();

        //Improve connection
        if (isDynamic)
            ImproveConnection_dynamic(connectivity);
        else
            ImproveConnection_Static(connectivity);

        //Improve door
        RemoveUselessDoors();

        //Update room information and calculate basic room type
        foreach (Room r in rooms)
        {
            r.CalculateRoomInfo();
            r.GenerateRoomType();
        }

        //Spawn key rooms before adding more rooms
        TransformRandomRoom(RoomType.Room, RoomType.Key, 2, room_num / 3 * 2);

        //Transform some dead ends into a room using basic room type
        TransformAllDeadEndToRoom();

        //Set first room to elevator room
        SetRoomTypeTo(1, RoomType.Elevator);

        Debug.Log("Generation Completed!");
        Debug.Log(room_num + " rooms was generated");

        Debug.Log("----------------------------------------------------------");

    }

    //Reset all data to default state
    public void Reset()
    {
        room_num = 0;
        block_num = 0;
        active_blocks.Clear();
        rooms = new List<Room>();

        for (int i = 0; i < border_size; i++)
        {
            for (int j = 0; j < border_size; j++)
            {
                blocks[i, j].Reset();
            }
        }
    }

    //Set room type
    public void SetRoomTypeTo(int room_id, RoomType type)
    {
        if (room_id < 1)
            return;

        rooms[room_id - 1].type = type;
    }

    //Transform some room to other types
    public void TransformRandomRoom(RoomType origin, RoomType target, int num)
    {
        List<Room> targets = new List<Room>();

        //Add all specified room into a list
        foreach (Room r in rooms)
        {
            if (r.type == origin)
                targets.Add(r);
        }

        //Randomly transform them into target room type
        for (int i = 0; i < num; i++)
        {
            if (targets.Count < 1)
                return;

            int pos = UnityEngine.Random.Range(0, targets.Count - 1);
            targets[pos].type = target;
            targets.RemoveAt(pos);
        }
    }

    public void TransformRandomRoom(RoomType origin, RoomType target, int num, int min_room_id)
    {
        List<Room> targets = new List<Room>();

        //Add all specified room into a list
        foreach (Room r in rooms)
        {
            if (r.type == origin && r.room_id >= min_room_id)
                targets.Add(r);
        }

        //Randomly transform them into target room type
        for (int i = 0; i < num; i++)
        {
            if (targets.Count < 1)
                return;

            int pos = UnityEngine.Random.Range(0, targets.Count - 1);
            targets[pos].type = target;
            targets.RemoveAt(pos);
        }
    }

    public List<Room> GetRoom(RoomType type)
    {
        List<Room> list = new List<Room>();
        foreach(Room room in rooms)
        {
            if (room.type == type)
                list.Add(room);
        }
        return list;
    }

    //Draw a room with specified size and center
    private void DrawRoom(int x, int y, int sizeX, int sizeY)
    {

        if (sizeX < 1 || sizeY < 1)
            return;

        room_num++;

        rooms.Add(new Room());
        rooms[rooms.Count - 1].room_id = room_num;

        int xOffset, yOffset;

        if (sizeX == 1)
            xOffset = 0;
        else
            xOffset = (sizeX - 1) / 2;

        if (sizeX == 1)
            yOffset = 0;
        else
            yOffset = (sizeY - 1) / 2;

        for (int i = x - xOffset; i < x + sizeX - xOffset; i++)
        {
            for (int j = y - yOffset; j < y + sizeY - yOffset; j++)
            {
                AddBlock(blocks[i, j]);
                rooms[rooms.Count - 1].addBlock(blocks[i, j]);
            }
        }
    }

    //Draw a room
    private void DrawRoom(Block start, int size)
    {
        Block b = start;

        room_num++;
        rooms.Add(new Room(room_num));

        for (int i = 0; i < size; i++)
        {
            AddBlock(b);
            rooms[rooms.Count - 1].addBlock(b);

            List<Block> next = new List<Block>();
            List<int> connectivities = new List<int>();

            //calculate connectivities
            string s = "";

            //for each neighbors
            for (int j = 0; j < 4; j++)
            {
                //calculate connectivity
                connectivities.Add(GetConnectivity(b.neighbors[j], room_num));
                s += connectivities[connectivities.Count - 1].ToString();
            }

            int targetCon = Helpers.FindMaxValue(connectivities);

            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (connectivities[k] == targetCon && b.neighbors[k].room == 0)
                        next.Add(b.neighbors[k]);
                }

                if (next.Count > 0)
                    break;

                targetCon--;
            }

            if (next.Count == 0)
                return;

            b = next[UnityEngine.Random.Range(0, next.Count)];
        }
    }

    //Draw a hallway
    private void DrawHallway(Block start, int size)
    {
        Block current = start;

        //Create a room
        room_num++;
        rooms.Add(new Room(room_num));

        for (int i = 0; i < size; i++)
        {
            AddBlock(current);
            rooms[rooms.Count - 1].addBlock(current);

            List<Block> next = new List<Block>();
            List<int> connectivities = new List<int>();

            //calculate connectivities
            string s = "";

            //for each neighbors
            for (int j = 0; j < 4; j++)
            {
                //calculate connectivity
                connectivities.Add(GetConnectivity(current.neighbors[j], room_num));
                s += connectivities[connectivities.Count - 1].ToString();
            }

            int targetCon = Helpers.FindMinValue(connectivities);

            for (int j = 0; j < 4; j++)
            {
                for (int k = 0; k < 4; k++)
                {
                    if (connectivities[k] == targetCon && current.neighbors[k].room == 0)
                        next.Add(current.neighbors[k]);
                }

                if (next.Count > 0)
                    break;

                targetCon--;
            }

            if (next.Count == 0)
                return;

            current = next[UnityEngine.Random.Range(0, next.Count)];
        }
    }

    //Draw a room using a random walker
    private void DrawRandom(Block start, int size)
    {
        Block b = start;

        //Create a room
        room_num++;
        rooms.Add(new Room(room_num));

        for (int i = 0; i < size; i++)
        {
            AddBlock(b);
            rooms[rooms.Count - 1].addBlock(b);

            //List of next block
            List<Block> next = new List<Block>();

            //Add all valid nearby neighbors
            for (int j = 0; j < 4; j++)
            {
                if (b.neighbors[j].room == 0)
                {
                    next.Add(b.neighbors[j]);
                }
            }

            if (next.Count == 0)
                break;

            //Go to next block
            b = next[UnityEngine.Random.Range(0, next.Count)];
        }
    }

    //Change walls to doors using a random depth-first search
    private void DrawDoors()
    {
        Block current = blocks[border_size / 2, border_size / 2];
        List<Block> visited = new List<Block>();
        Stack<Block> stack = new Stack<Block>();

        visited.Add(current);
        stack.Push(current);

        while (stack.Count != 0)
        {
            List<Block> next = new List<Block>();
            Block target = null;

            //Add all unvisited openings
            for (int i = 0; i < 4; i++)
            {
                if (!visited.Contains(current.neighbors[i]) && current.walls[i] == WallType.None && current.neighbors[i].room != 0)
                    next.Add(current.neighbors[i]);
            }

            //Select a random location
            if (next.Count > 0)
                target = next[UnityEngine.Random.Range(0, next.Count)];

            //Update and move to next
            if (target != null)
            {
                visited.Add(target);
                stack.Push(target);
                current = target;
                continue;
            }

            //target index
            int index = -1;

            //Add all unvisited directions
            for (int i = 0; i < 4; i++)
            {
                if (!visited.Contains(current.neighbors[i]) && current.walls[i] == WallType.Wall && current.neighbors[i].room != 0)
                    next.Add(current.neighbors[i]);
            }

            //Select a random location
            if (next.Count > 0)
            {
                index = UnityEngine.Random.Range(0, next.Count);
                target = next[index];
            }

            //Add door
            if (target != null)
            {
                AddDoor(current, target);

                visited.Add(target);
                stack.Push(target);
                current = target;

                continue;
            }

            if (stack.Count > 0)
            {
                stack.Pop();
                if (stack.Count > 0)
                {
                    current = stack.Peek();
                }
            }
        }

        return;

    }

    //Update wall information based on room_id
    private void DrawWalls()
    {
        foreach (Block b in active_blocks)
        {
            for (int i = 0; i < 4; i++)
            {
                if (b.neighbors[i].room != b.room)
                {
                    b.walls[i] = WallType.Wall;
                }
            }
        }
    }

    //Turn wall between block a and block b to a door
    private void AddDoor(Block a, Block b)
    {
        for (int i = 0; i < 4; i++)
        {
            if (a.neighbors[i].Equals(b))
            {
                a.walls[i] = WallType.Door;
                if (i < 2)
                    b.walls[i + 2] = WallType.Door;
                else
                    b.walls[i - 2] = WallType.Door;
            }
        }
    }

    //Add a block to the active block pool
    private void AddBlock(Block b)
    {
        //overlap code
        //b.room = room_num;
        active_blocks.Add(b);
        block_num++;
    }

    //Return a random valid block to draw
    private Block FindDrawPoint()
    {
        if (active_blocks.Count == 0)
            return null;

        List<Block> next = new List<Block>();

        PopulateValidNeighboringBlocks(next);

        if (next.Count == 0)
            return null;

        return next[UnityEngine.Random.Range(0, next.Count)];
    }

    //Return a random valid block around specified room
    private Block FindDrawPoint(int target)
    {
        if (active_blocks.Count == 0)
            return null;

        List<Block> next = new List<Block>();

        PopulateValidNeighboringBlocks(next, target);

        if (next.Count == 0)
            return null;

        return next[UnityEngine.Random.Range(0, next.Count)];
    }

    #region Improvment

    private void ImproveConnection_dynamic(float strength)
    {
        int total_valid_door = 0;
        int debug_V = 0;
        int debug_H = 0;
        int debug_total_valid_blocks = 0;
        int debug_total_denied_doors = 0;

        strength = Mathf.Clamp(strength, 0f, 1f);
        List<Block> valid_blocks = new List<Block>();

        //Generate a list of valid blocks and calculate all potential positions
        foreach (Block b in blocks)
        {
            bool canSpawn = false;

            //Total door count
            int doorCount = 0;

            //vertical door and horizontal door counts
            int v_num, h_num;

            bool vertical_only, horizontal_only;

            //is there wall around with active block behind? (can I add doors?)
            for (int i = 0; i < 4; i++)
            {
                if (b.walls[i] == WallType.Wall && b.neighbors[i].room != 0)
                    canSpawn = true;
            }

            //if yes, count doors
            if (canSpawn)
            {
                v_num = h_num = 0;

                vertical_only = horizontal_only = false;

                //Check how many surrounding doors
                for (int i = 0; i < 4; i++)
                {
                    //Check surrounding walls
                    if (b.walls[i] == WallType.Door)
                    {
                        doorCount++;
                        switch (i)
                        {
                            case 0:
                            case 2:
                                h_num++;
                                break;
                            case 1:
                            case 3:
                                v_num++;
                                break;
                            default:
                                break;
                        }
                    }

                    //Check neighbor's walls
                    for (int j = 0; j < 4; j++)
                    {
                        if (b.neighbors[i].walls[j] == WallType.Door)
                        {
                            //ignore the outer door
                            if (j != i)
                            {
                                doorCount++;
                                switch (j)
                                {
                                    case 0:
                                    case 2:
                                        h_num++;
                                        break;
                                    case 1:
                                    case 3:
                                        v_num++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                continue;
            }

            //If there's door, check variaty
            if (doorCount > 0 && doorCount < 4)
            {
                if (v_num == 0)
                    horizontal_only = true;

                if (h_num == 0)
                    vertical_only = true;

                //With bad variety, we need to add more doors
                if (horizontal_only)
                {
                    bool added = false;

                    if ((b.walls[1] == WallType.Wall && b.neighbors[1].room != 0))
                    {
                        valid_blocks.Add(b);
                        added = true;

                        if (CanSpawnDoor(b, 1))
                            total_valid_door++;
                    }

                    if (b.walls[3] == WallType.Wall && b.neighbors[3].room != 0)
                    {
                        if (!added)
                            valid_blocks.Add(b);

                        if (CanSpawnDoor(b, 3))
                            total_valid_door++;
                    }
                }
                else if (vertical_only)
                {
                    bool added = false;

                    if ((b.walls[0] == WallType.Wall && b.neighbors[0].room != 0))
                    {
                        valid_blocks.Add(b);
                        added = true;

                        if (CanSpawnDoor(b, 0))
                            total_valid_door++;
                    }

                    if (b.walls[2] == WallType.Wall && b.neighbors[2].room != 0)
                    {
                        if (!added)
                            valid_blocks.Add(b);

                        if (CanSpawnDoor(b, 2))
                            total_valid_door++;
                    }
                }
            }
            else
            {
                bool added = false;

                for (int i = 0; i < 4; i++)
                {
                    if (b.walls[i] == WallType.Wall && b.neighbors[i].room != 0)
                    {
                        if (!added)
                        {
                            valid_blocks.Add(b);
                            added = true;
                        }

                        total_valid_door++;
                    }
                }
            }
        }

        if (total_valid_door < 2)
            return;

        //Calculate our target doors base on strength
        int target_door_num = (int)(total_valid_door * strength);
        int added_door = 0;
        debug_total_valid_blocks = valid_blocks.Count;

        while (added_door < target_door_num && valid_blocks.Count > 0)
        {
            Block b = valid_blocks[UnityEngine.Random.Range(0, valid_blocks.Count)];
            valid_blocks.Remove(b);

            int doorCount = 0;
            //vertical door and horizontal door counts
            int v_num, h_num;
            bool vertical_only, horizontal_only;

            v_num = h_num = 0;
            vertical_only = horizontal_only = false;

            //Check how many surrounding doors
            for (int i = 0; i < 4; i++)
            {
                //Check surrounding walls
                if (b.walls[i] == WallType.Door)
                {
                    doorCount++;
                    switch (i)
                    {
                        case 0:
                        case 2:
                            h_num++;
                            break;
                        case 1:
                        case 3:
                            v_num++;
                            break;
                        default:
                            break;
                    }
                }

                //Check neighbor's walls
                for (int j = 0; j < 4; j++)
                {
                    if (b.neighbors[i].walls[j] == WallType.Door)
                    {
                        //ignore the outer door
                        if (j != i)
                        {
                            doorCount++;
                            switch (j)
                            {
                                case 0:
                                case 2:
                                    h_num++;
                                    break;
                                case 1:
                                case 3:
                                    v_num++;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }

            //If there's door, check variaty
            if (doorCount > 0)
            {
                if (v_num == 0)
                    horizontal_only = true;

                if (h_num == 0)
                    vertical_only = true;

                //With bad variety, we need to add more doors
                if (horizontal_only)
                {
                    if (added_door < target_door_num)
                    {
                        if ((b.walls[1] == WallType.Wall && b.neighbors[1].room != 0))
                        {
                            if (CanSpawnDoor(b, 1))
                            {
                                AddDoor(b, b.neighbors[1]);
                                added_door++;
                                debug_V++;
                            }
                            else
                                debug_total_denied_doors++;
                        }
                    }

                    if (added_door < target_door_num)
                    {
                        if (b.walls[3] == WallType.Wall && b.neighbors[3].room != 0)
                        {
                            if (CanSpawnDoor(b, 3))
                            {
                                AddDoor(b, b.neighbors[3]);
                                added_door++;
                                debug_V++;
                            }
                            else
                                debug_total_denied_doors++;
                        }
                    }
                }
                else if (vertical_only)
                {
                    if (added_door < target_door_num)
                    {
                        if ((b.walls[0] == WallType.Wall && b.neighbors[0].room != 0))
                        {
                            if (CanSpawnDoor(b, 0))
                            {
                                AddDoor(b, b.neighbors[0]);
                                added_door++;
                                debug_H++;
                            }
                            else
                                debug_total_denied_doors++;
                        }
                    }

                    if (added_door < target_door_num)
                    {
                        if (b.walls[2] == WallType.Wall && b.neighbors[2].room != 0)
                        {
                            if (CanSpawnDoor(b, 2))
                            {
                                AddDoor(b, b.neighbors[2]);
                                added_door++;
                                debug_H++;
                            }
                            else
                                debug_total_denied_doors++;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (added_door < target_door_num)
                    {
                        if (b.walls[i] == WallType.Wall && b.neighbors[i].room != 0)
                        {
                            AddDoor(b, b.neighbors[i]);
                            added_door++;
                        }
                    }
                }
            }

        }

        Debug.Log("Blocks: " + debug_total_valid_blocks);
        Debug.Log("Total Potential: " + total_valid_door + " Potential Target: " + target_door_num + " Connectivity: " + strength);
        Debug.Log("Total Added: " + added_door + " V: " + debug_V + " H: " + debug_H);
        Debug.Log("Denied: " + debug_total_denied_doors);
    }

    private void ImproveConnection_Static(float strength)
    {
        int total_valid_door = 0;
        int debug_V = 0;
        int debug_H = 0;
        int debug_total_denied_doors = 0;

        strength = Mathf.Clamp(strength, 0f, 1f);
        List<Pair<Block, int>> potential_pos = new List<Pair<Block, int>>();

        //Generate a list of valid blocks and calculate all potential positions
        foreach (Block b in blocks)
        {
            bool canSpawn = false;

            //Total door count
            int doorCount = 0;

            //vertical door and horizontal door counts
            int v_num, h_num;

            bool vertical_only, horizontal_only;

            //is there wall around with active block behind? (can I add doors?)
            for (int i = 0; i < 4; i++)
            {
                if (b.walls[i] == WallType.Wall && b.neighbors[i].room != 0)
                    canSpawn = true;
            }

            //if yes, count doors
            if (canSpawn)
            {
                v_num = h_num = 0;

                vertical_only = horizontal_only = false;

                //Check how many surrounding doors
                for (int i = 0; i < 4; i++)
                {
                    //Check surrounding walls
                    if (b.walls[i] == WallType.Door)
                    {
                        doorCount++;
                        switch (i)
                        {
                            case 0:
                            case 2:
                                h_num++;
                                break;
                            case 1:
                            case 3:
                                v_num++;
                                break;
                            default:
                                break;
                        }
                    }

                    //Check neighbor's walls
                    for (int j = 0; j < 4; j++)
                    {
                        if (b.neighbors[i].walls[j] == WallType.Door)
                        {
                            //ignore the outer door
                            if (j != i)
                            {
                                doorCount++;
                                switch (j)
                                {
                                    case 0:
                                    case 2:
                                        h_num++;
                                        break;
                                    case 1:
                                    case 3:
                                        v_num++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                continue;
            }

            //If there's door, check variaty
            if (doorCount > 0 && doorCount < 4)
            {
                if (v_num == 0)
                    horizontal_only = true;

                if (h_num == 0)
                    vertical_only = true;

                //With bad variety, we need to add more doors
                if (horizontal_only)
                {

                    if ((b.walls[1] == WallType.Wall && b.neighbors[1].room != 0))
                    {
                        if (CanSpawnDoor(b, 1))
                        {
                            potential_pos.Add(new Pair<Block, int>(b, 1));
                            debug_V++;
                            total_valid_door++;
                        }
                    }

                    if (b.walls[3] == WallType.Wall && b.neighbors[3].room != 0)
                    {

                        if (CanSpawnDoor(b, 3))
                        {
                            total_valid_door++;
                            debug_V++;
                            potential_pos.Add(new Pair<Block, int>(b, 3));
                        }
                    }
                }
                else if (vertical_only)
                {

                    if ((b.walls[0] == WallType.Wall && b.neighbors[0].room != 0))
                    {
                        if (CanSpawnDoor(b, 0))
                        {
                            potential_pos.Add(new Pair<Block, int>(b, 0));
                            debug_H++;
                            total_valid_door++;
                        }

                    }

                    if (b.walls[2] == WallType.Wall && b.neighbors[2].room != 0)
                    {

                        if (CanSpawnDoor(b, 2))
                        {
                            potential_pos.Add(new Pair<Block, int>(b, 2));
                            debug_H++;
                            total_valid_door++;
                        }
                    }
                }
            }
            else
            {

                for (int i = 0; i < 4; i++)
                {
                    if (b.walls[i] == WallType.Wall && b.neighbors[i].room != 0)
                    {
                        potential_pos.Add(new Pair<Block, int>(b, i));
                        total_valid_door++;
                    }
                }
            }
        }

        //Calculate our target doors base on strength
        int target_door_num = (int)(total_valid_door * strength);
        int added_door = 0;

        while (added_door < target_door_num && potential_pos.Count > 0)
        {
            Pair<Block, int> pos = potential_pos[UnityEngine.Random.Range(0, potential_pos.Count)];
            potential_pos.Remove(pos);

            AddDoor(pos.First, pos.First.neighbors[pos.Second]);
            added_door++;
        }

        Debug.Log("Total Potential: " + total_valid_door + " Target: " + target_door_num);
        Debug.Log("V: " + debug_V + " H: " + debug_H);
        Debug.Log("Added: " + added_door);

        return;

    }

    private void TransformAllDeadEndToRoom()
    {
        int debug_transformed_dead_ends = 0;

        foreach (Block b in active_blocks)
        {
            int count = 0;

            RoomType type = rooms[b.room - 1].type;

            foreach (WallType w in b.walls)
            {
                if (w == WallType.Wall)
                    count++;

                if (count == 3 && type == RoomType.Hallway)
                {
                    if (TransformDeadEnd(b))
                    {
                        debug_transformed_dead_ends++;
                        break;
                    }
                }
            }
        }

        Debug.Log("Transformed Dead Ends: " + debug_transformed_dead_ends);
    }

    private bool TransformDeadEnd(Block b)
    {
        //Calculate max number of tranformation
        int transformed = 0;
        int transform_num = rooms[b.room - 1].size / 2;
        int originalRoom = b.room;

        //Create a new room
        room_num++;
        rooms.Add(new Room(room_num));

        Room target = rooms[rooms.Count - 1];
        Block current = b;
        //target.addBlock(b);

        bool flag = true;

        //Track back and add eligible blocks to the new room until max number is reached or next block is not eligible
        do
        {
            int count = 0;


            //check amount of wall
            foreach (WallType t in current.walls)
            {
                if (t == WallType.Wall)
                    count++;
            }

            //check amount of useless block
            foreach (Block t in current.neighbors)
            {
                if (t.room == room_num)
                    count++;
            }

            //if count equal 3, which means there is only 1 eligible way to go
            if (count >= 3)
            {
                //add the current block to the new room and increment counter
                target.addBlock(current);
                transformed++;
            }

            //check the next block
            foreach (Block t in current.neighbors)
            {
                if (t.room == originalRoom)
                {
                    //if we reached our goal, add door and end the algorithm
                    if (transformed == transform_num)
                    {
                        AddDoor(current, t);
                        flag = false;
                        break;
                    }

                    //if the next block has doors, add door and end the algorithm
                    foreach (WallType w in t.walls)
                    {
                        if (w == WallType.Door)
                        {
                            AddDoor(current, t);
                            flag = false;
                            break;
                        }
                    }
                }
            }

            //else move to the next block
            foreach (Block t in current.neighbors)
            {
                if (t.room == originalRoom)
                    current = t;
            }

        } while (transformed < transform_num && flag);

        target.CalculateRoomInfo();
        target.GenerateRoomType();

        return true;

    }

    private void RemoveUselessDoors()
    {
        int debug_total_removed_doors = 0;

        foreach (Block b in active_blocks)
        {
            //Count doors
            int count = 0;

            foreach (WallType w in b.walls)
            {
                if (w == WallType.Door)
                    count++;
            }

            //if door is less than 2
            if (count < 2)
                continue;

            //construct an array, storing room number on relative directions
            int[] room = new int[4];

            for (int i = 0; i < 4; i++)
            {
                if (b.walls[i] == WallType.Door)
                    room[i] = b.neighbors[i].room;
                else
                    room[i] = 0;
            }

            for (int i = 0; i < 3; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (room[i] == 0 || room[j] == 0)
                        continue;

                    if (room[i] == room[j])
                    {
                        int target = -1;

                        if (UnityEngine.Random.Range(0, 2) == 0)
                        {
                            room[i] = 0;
                            target = i;
                        }
                        else
                        {
                            room[j] = 0;
                            target = j;
                        }

                        b.walls[target] = WallType.Wall;
                        if (target < 2)
                            b.neighbors[target].walls[target + 2] = WallType.Wall;
                        else
                            b.neighbors[target].walls[target - 2] = WallType.Wall;

                        debug_total_removed_doors++;
                    }
                }
            }
        }

        Debug.Log("Removed Useless Doors: " + debug_total_removed_doors);
    }

    #endregion

    #region Helpers

    //Helper method - return connectivity of a block
    private int GetConnectivity(Block b, int room)
    {
        int connectivity = 0;

        for (int i = 0; i < 4; i++)
        {
            if (b.neighbors[i].room == room)
                connectivity++;
        }

        return connectivity;
    }

    //Helper method
    private void PopulateValidNeighboringBlocks(List<Block> l)
    {
        foreach (Block b in active_blocks)
        {
            AddValidBlock(b, l);
        }
    }

    //Helper method - Need Optimization
    private void PopulateValidNeighboringBlocks(List<Block> l, int target)
    {
        foreach (Block b in active_blocks)
        {
            if (b.room <= target)
            {
                AddValidBlock(b, l);
            }
        }
    }

    //Helper method
    private void AddValidBlock(Block b, List<Block> l)
    {
        if (b == null || l == null)
            return;

        if (b.neighbors[0].room == 0)
            l.Add(b.neighbors[0]);

        if (b.neighbors[2].room == 0)
            l.Add(b.neighbors[2]);

        if (b.neighbors[1].room == 0)
            l.Add(b.neighbors[1]);

        if (b.neighbors[3].room == 0)
            l.Add(b.neighbors[3]);
    }

    //Helper method
    private bool CanSpawnDoor(Block b, int dir)
    {
        int temp = Mathf.Clamp(dir, 0, 3);
        bool flag = true;

        for (int i = 0; i < 4; i++)
        {
            if (i != temp)
            {
                if (b.neighbors[i].room == b.neighbors[dir].room)
                    flag = false;
            }
        }

        return flag;
    }

    #endregion
}