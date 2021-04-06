using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ProcDungeon : MonoBehaviour
{

    [Header("Prefabs")]
    public GameObject floor;
    public GameObject T;
    public GameObject B;
    public GameObject L;
    public GameObject R;
    public GameObject D_TB;
    public GameObject D_LR;

    [Header("Containers")]
    public List<GameObject> floors;
    public List<GameObject> walls;
    public List<GameObject> furnitures;

    [Header("Specs")]
    public float floorSize;
    public float wallThickness;
    public float wallHeight;

    [Header("Params")]
    public int mapSize;
    [Range(0f, 0.1f)]
    public float loot_spawn_rate;
    [Range(1f, 10f)]
    public float difficulty;

    [Header("-Layout")]
    [SerializeField]
    [Range(0, 1f)]
    public float connectivity;
    public bool dynamic_Improvement;

    [Range(1, 7)]
    public int min_hallway_size;
    [Range(2, 8)]
    public int max_hallway_size;
    [Range(1, 7)]
    public int min_room_size;
    [Range(2, 8)]
    public int max_room_size;

    [Header("-Furniture")]
    [Range(1, 10)]
    public int room_draw_count;
    [Range(1, 10)]
    public int hallway_draw_count;
    [Range(1, 10)]
    public int special_draw_count;
    [Range(0.1f, 5f)]
    public float furniture_density;
    [Range(0, 180)]
    public float furniture_max_rotation_offset;

    [Header("Debug")]
    public bool random_seed;
    public bool show_debug_numbers;
    public bool show_debug_room_type;

    [Header("Components")]
    public InputField seed_input;

    [Header("Masks")]
    public LayerMask OverlapMask;

    //Data
    private int borderSize;
    private Vector3 toCenter;
    private Map map;

    private LayerMask WallMask;
    private LayerMask DoorMask;
    private LayerMask FurnitureMask;
    private LayerMask InteractableMask;

    private GameObject root;
    private string seed;

    // Start is called before the first frame update
    void Start()
    {
        //UnityEngine.Random.InitState(System.Random r);

        seed = "00000";

        WallMask = LayerMask.GetMask("Wall");
        DoorMask = LayerMask.GetMask("Block");
        FurnitureMask = LayerMask.GetMask("Furniture");
        InteractableMask = LayerMask.GetMask("Interactable");

        borderSize = (int)Mathf.Sqrt(mapSize) + 30;
        toCenter = new Vector3(-floorSize * (borderSize / 2), 0, -floorSize * (borderSize / 2));

        map = new Map(borderSize);
        root = new GameObject("Dungeon");

        if (min_room_size > max_room_size)
            max_room_size = min_room_size + 1;

        Generate();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reset());
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleDebugNumbers();
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleDebugRoomType();
        }
    }

    IEnumerator Reset()
    {

        if (random_seed)
            UnityEngine.Random.InitState((int)Time.time);
        else
            UnityEngine.Random.InitState(Helpers.GenerateSeed(seed));

        Clear();

        yield return new WaitForSeconds(0.5f);

        Generate();
    }

    public void Clear()
    {
        foreach (GameObject g in floors)
        {
            Destroy(g);
        }

        floors = new List<GameObject>();

        foreach (GameObject g in walls)
        {
            Destroy(g);
        }

        walls = new List<GameObject>();

        foreach (GameObject g in furnitures)
        {
            Destroy(g);
        }

        furnitures = new List<GameObject>();

        map.Reset();
    }

    public void Generate()
    {
        //Generae basic layout
        map.Generate(mapSize, min_room_size, max_room_size + 1, min_hallway_size, max_hallway_size + 1, connectivity, dynamic_Improvement);

        //Add speical rooms
        map.TransformRandomRoom(RoomType.Room, RoomType.Upgrade, 1, map.room_num / 2);
        map.TransformRandomRoom(RoomType.Room, RoomType.Craft, 1, map.room_num / 2);
        map.TransformRandomRoom(RoomType.Room, RoomType.Recycle, 1, map.room_num / 2);
        map.TransformRandomRoom(RoomType.Room, RoomType.Treasure, 2, map.room_num / 2);

        float h_floorSize = floorSize / 2;
        float h_wallThickness = wallThickness / 2;

        //Generate Floors, Walls and Doors
        for (int i = 0; i < borderSize; i++)
        {
            for (int j = 0; j < borderSize; j++)
            {
                //Spawn floor
                if (map.blocks[i, j].room != 0)
                {
                    floors.Add(Instantiate(floor, new Vector3(i * floorSize, 0, j * floorSize) + toCenter, Quaternion.identity));
                    floors[floors.Count - 1].transform.parent = root.transform;
                    map.rooms[map.blocks[i, j].room - 1].objects.Add(floors[floors.Count - 1]);

                    floors[floors.Count - 1].transform.GetChild(4).GetComponent<TextMesh>().text = map.blocks[i, j].room.ToString();
                    floors[floors.Count - 1].transform.GetChild(4).gameObject.SetActive(show_debug_numbers);

                    if (map.rooms[map.blocks[i, j].room - 1].type == RoomType.Hallway)
                        floors[floors.Count - 1].transform.GetChild(5).GetComponent<TextMesh>().text = "H";

                    if (map.rooms[map.blocks[i, j].room - 1].type == RoomType.Room)
                        floors[floors.Count - 1].transform.GetChild(5).GetComponent<TextMesh>().text = "R";

                    TextMesh t = floors[floors.Count - 1].transform.GetChild(5).GetComponent<TextMesh>();

                    switch (map.rooms[map.blocks[i, j].room - 1].type)
                    {
                        case RoomType.None:
                            t.text = "N";
                            break;

                        case RoomType.Room:
                            t.text = "R";
                            break;

                        case RoomType.Hallway:
                            t.text = "H";
                            break;

                        case RoomType.Elevator:
                            t.text = "E";
                            break;

                        case RoomType.Upgrade:
                            t.text = "U";
                            break;

                        case RoomType.Craft:
                            t.text = "C";
                            break;

                        case RoomType.Recycle:
                            t.text = "Y";
                            break;

                        case RoomType.Treasure:
                            t.text = "T";
                            break;

                        case RoomType.Key:
                            t.text = "K";
                            break;
                    }

                    floors[floors.Count - 1].transform.GetChild(5).gameObject.SetActive(show_debug_room_type);
                }
                else
                    continue;

                for (int k = 0; k < 4; k++)
                {
                    switch (map.blocks[i, j].walls[k])
                    {
                        case WallType.Wall:
                            switch (k)
                            {
                                case 0:
                                    walls.Add(Instantiate(T, new Vector3(i * floorSize, wallHeight, j * floorSize + h_floorSize - h_wallThickness) + toCenter, Quaternion.identity));
                                    break;
                                case 1:
                                    walls.Add(Instantiate(R, new Vector3(i * floorSize + h_floorSize - h_wallThickness, wallHeight, j * floorSize) + toCenter, Quaternion.identity));
                                    break;
                                case 2:
                                    walls.Add(Instantiate(B, new Vector3(i * floorSize, wallHeight, j * floorSize - h_floorSize + h_wallThickness) + toCenter, Quaternion.identity));
                                    break;
                                case 3:
                                    walls.Add(Instantiate(L, new Vector3(i * floorSize - h_floorSize + h_wallThickness, wallHeight, j * floorSize) + toCenter, Quaternion.identity));
                                    break;
                            }
                            walls[walls.Count - 1].transform.parent = root.transform;
                            floors[floors.Count - 1].transform.GetChild(k).GetComponent<TextMesh>().text = "1";
                            break;
                        case WallType.Door:
                            Vector3 v3 = Vector3.zero;
                            switch (k)
                            {
                                case 0:
                                    v3 = new Vector3(i * floorSize, wallHeight, j * floorSize + h_floorSize) + toCenter;
                                    if (Physics.OverlapSphere(v3, 2f, DoorMask).Length == 0)
                                        walls.Add(Instantiate(D_TB, v3, Quaternion.identity));
                                    break;
                                case 1:
                                    v3 = new Vector3(i * floorSize + h_floorSize, wallHeight, j * floorSize) + toCenter;
                                    if (Physics.OverlapSphere(v3, 2f, DoorMask).Length == 0)
                                        walls.Add(Instantiate(D_LR, v3, Quaternion.identity));
                                    break;
                                case 2:
                                    v3 = new Vector3(i * floorSize, wallHeight, j * floorSize - h_floorSize) + toCenter;
                                    if (Physics.OverlapSphere(v3, 2f, DoorMask).Length == 0)
                                        walls.Add(Instantiate(D_TB, v3, Quaternion.identity));
                                    break;
                                case 3:
                                    v3 = new Vector3(i * floorSize - h_floorSize, wallHeight, j * floorSize) + toCenter;
                                    if (Physics.OverlapSphere(v3, 2f, DoorMask).Length == 0)
                                        walls.Add(Instantiate(D_LR, v3, Quaternion.identity));
                                    break;
                            }
                            walls[walls.Count - 1].transform.parent = root.transform;
                            floors[floors.Count - 1].transform.GetChild(k).GetComponent<TextMesh>().text = "2";
                            break;
                        default:
                            floors[floors.Count - 1].transform.GetChild(k).GetComponent<TextMesh>().text = "0";
                            break;
                    }
                    floors[floors.Count - 1].transform.GetChild(k).gameObject.SetActive(show_debug_numbers);
                }

            }
        }

        foreach (Room r in map.rooms)
        {
            PopulateInteractables(r);
            PopulateLoot(r);
            PopulateFurniture(r);
        }
    }

    //Helper - Spawn a furniture in a room, return true if successful
    public bool SpawnFurniture(Room room, Furniture furniture)
    {
        //Select a random floor to spawn the furniture
        GameObject floor = room.objects[UnityEngine.Random.Range(0, room.objects.Count)];

        //A random rotation
        float rotation = 0;
        Quaternion quaternion = Quaternion.identity;

        bool alongWall = false;
        Vector3 pos = Vector3.zero;

        float hFloorSize = floorSize / 2;
        float x = 0, y = 0;
        float sizeX = furniture.size.x;
        float sizeY = furniture.size.y;
        float sizeXh = furniture.size.x / 2;
        float sizeYh = furniture.size.y / 2;
        float l;

        bool flag = false;

        //Furniture must be place along walls
        if (furniture.alongWall)
        {
            //Debug.Log("Along Wall");
            alongWall = true;
        }

        //If not, it has 1/3 chance to be place along walls
        if (UnityEngine.Random.Range(0, 3) == 0 && !furniture.mustCenter)
            alongWall = true;

        if (alongWall)
        {
            List<Direction> wall = new List<Direction>();

            if (Physics.Raycast(floor.transform.position + new Vector3(0, 2f, 0), Vector3.forward, 4f, WallMask))
            {
                wall.Add(Direction.Up);
            }
            if (Physics.Raycast(floor.transform.position + new Vector3(0, 2f, 0), -Vector3.forward, 4f, WallMask))
            {
                wall.Add(Direction.Down);
            }
            if (Physics.Raycast(floor.transform.position + new Vector3(0, 2f, 0), Vector3.right, 4f, WallMask))
            {
                wall.Add(Direction.Right);
            }
            if (Physics.Raycast(floor.transform.position + new Vector3(0, 2f, 0), -Vector3.right, 4f, WallMask))
            {
                wall.Add(Direction.Left);
            }

            if (wall.Count == 0)
            {
                //Debug.Log("No Wall");
                return false;
                //return;
            }

            Direction dir = wall[UnityEngine.Random.Range(0, wall.Count - 1)];

            switch (dir)
            {
                case Direction.Up:
                    l = hFloorSize - sizeX * 0.5f;
                    x = UnityEngine.Random.Range(-l, l);
                    y = hFloorSize - sizeY * 0.5f - 0.25f;
                    rotation = 180;
                    break;
                case Direction.Down:
                    l = hFloorSize - sizeX * 0.5f;
                    x = UnityEngine.Random.Range(-l, l);
                    y = -hFloorSize + sizeY * 0.5f + 0.25f;
                    rotation = 0;
                    break;
                case Direction.Left:
                    l = hFloorSize - sizeX * 0.5f;
                    x = -hFloorSize + sizeY * 0.5f + 0.25f;
                    y = UnityEngine.Random.Range(-l, l);
                    rotation = 90;
                    break;
                case Direction.Right:
                    l = hFloorSize - sizeX * 0.5f;
                    x = hFloorSize - sizeY * 0.5f - 0.25f;
                    y = UnityEngine.Random.Range(-l, l);
                    rotation = -90;
                    break;
                default:
                    break;
            }

            pos = floor.transform.position;
            pos += new Vector3(x, furniture.obj.transform.position.y, y);
            //Debug.Log("X: " + x + " Y: " + y);

            quaternion = Quaternion.Euler(0, rotation, 0);


            if (Physics.OverlapBox(pos, new Vector3(sizeXh, 1f, sizeYh), quaternion, FurnitureMask | InteractableMask).Length == 0)
            {
                furnitures.Add(Instantiate(furniture.obj, pos, quaternion));
                furnitures[furnitures.Count - 1].name += room.room_id;
                furnitures[furnitures.Count - 1].transform.parent = root.transform;
                flag = true;
                //Debug.Log("Spawned Furniture!");
            }
            else
            {
                int hit = Physics.OverlapBox(pos, new Vector3(sizeXh, 1f, sizeYh), quaternion).Length;
                //Debug.Log("Detected " + hit + " objects.");
            }
        }
        else
        {
            float r = 0;
            float deg = UnityEngine.Random.Range(0, 360f);

            //r = Mathf.Sqrt(target.size.x * target.size.x + target.size.y * target.size.y);
            float value = sizeX > sizeY ? sizeX : sizeY;
            r = 3f - value * 0.5f;
            float finalR = UnityEngine.Random.Range(0, r);
            //Debug.Log(finalR + " < " + r);

            float xPos = finalR * Mathf.Cos(deg * Mathf.Deg2Rad);
            float zPos = finalR * Mathf.Sin(deg * Mathf.Deg2Rad);

            pos = floor.transform.position + new Vector3(xPos, furniture.obj.transform.position.y, zPos);

            if (furniture.canRotate)
                rotation = UnityEngine.Random.Range(-furniture_max_rotation_offset, furniture_max_rotation_offset);

            quaternion = Quaternion.Euler(0, rotation, 0);

            if (Physics.OverlapBox(pos, new Vector3(sizeXh, 1f, sizeYh), quaternion, OverlapMask).Length == 0)
            {
                furnitures.Add(Instantiate(furniture.obj, pos, quaternion));
                furnitures[furnitures.Count - 1].name += room.room_id;
                furnitures[furnitures.Count - 1].transform.parent = root.transform;
                if (furniture.moveable)
                {
                    furnitures[furnitures.Count - 1].AddComponent<Rigidbody>();
                    Rigidbody rig = furnitures[furnitures.Count - 1].GetComponent<Rigidbody>();
                    rig.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
                    rig.mass = sizeX * sizeY / 2;
                    rig.drag = 3f;
                    rig.interpolation = RigidbodyInterpolation.Interpolate;
                }
                furnitures[furnitures.Count - 1].name += room.room_id;
                flag = true;
                //Debug.Log("Spawned Furniture!");
            }
            else
            {
                int hit = Physics.OverlapBox(pos, new Vector3(sizeXh, 1f, sizeYh), quaternion, OverlapMask).Length;
                //Debug.Log("Detected " + hit + " objects.");
            }
        }

        return flag;
        //return;

    }

    //Helper - Spawn a specified furniture in a room, will try 10 times
    public void SpawnFurniture(Room room, string name)
    {
        bool flag = false;
        int count = 10;

        while (!flag || count < 0)
        {
            flag = SpawnFurniture(room, FurnitureDatabase.database.GetByName(name));
            count--;
        }
    }

    //Spawn all loot furniture of a room
    private void PopulateLoot(Room room)
    {
        if (room.type == RoomType.Treasure)
        {
            List<Furniture> list = FurnitureDatabase.database.GetByType(RoomType.Treasure);
            for (int i = 0; i < 2; i++)
            {
                SpawnFurniture(room, list[UnityEngine.Random.Range(0,list.Count)]);
            }
        }
        else
        {
            foreach(Block b in room.blocks)
            {
                float random = UnityEngine.Random.Range(0, 1f);
                if (random < loot_spawn_rate / difficulty)
                {
                    SpawnFurniture(room, FurnitureDatabase.database.GetRandomLootObj());
                }
            }
        }
    }

    //Spawn all speical furniture of a room
    private void PopulateInteractables(Room room)
    {
        switch (room.type)
        {
            case RoomType.Craft:
                SpawnFurniture(room, "Crafting Table");
                break;
            case RoomType.Upgrade:
                SpawnFurniture(room, "Mutation Bench");
                SpawnFurniture(room, "Tech Bench");
                break;
            case RoomType.Recycle:
                SpawnFurniture(room, "Recycle Machine");
                break;
            case RoomType.Key:
                SpawnFurniture(room, "Key");
                break;
            case RoomType.Elevator:
                //SpawnFurniture(room, "Elevator");
                break;
            default:
                break;
        }
    }

    //Spawn all furniture
    private void PopulateFurniture(Room room)
    {
        //Get a list of furniture based to current type
        List<Furniture> list = FurnitureDatabase.database.GetByType(room.type);

        int drawCount = 0;

        //Calculate draw count based on room type
        switch (room.type)
        {
            case RoomType.None:
                drawCount = 0;
                break;
            case RoomType.Hallway:
                drawCount = (int)((float)hallway_draw_count * furniture_density * room.size);
                break;
            case RoomType.Room:
                drawCount = (int)((float)room_draw_count * furniture_density * room.size);
                break;
            case RoomType.Craft:
            case RoomType.Upgrade:
            case RoomType.Recycle:
            case RoomType.Key:
            case RoomType.Treasure:
            case RoomType.Elevator:
                drawCount = (int)((float)special_draw_count * furniture_density * room.size);
                break;
            default:
                drawCount = 0;
                break;
        }

        drawCount = UnityEngine.Random.Range(drawCount / 2, drawCount);

        //Draw
        while (drawCount > 0)
        {
            //Debug.Log("Room " + room.room_id + " Start.");

            //Select a random furniture from the list
            Furniture target = list[UnityEngine.Random.Range(0, list.Count)];

            SpawnFurniture(room, target);
            drawCount--;
        }
        //Debug.Log("Room " + room.room_id + " Done.");
    }

    public void SetSeed()
    {
        while (seed_input.text.Length < 5)
        {
            seed_input.text += "0";
        }

        seed = seed_input.text;
    }

    public void ToggleDebugNumbers()
    {
        show_debug_numbers = !show_debug_numbers;

        foreach (GameObject g in floors)
        {
            g.transform.GetChild(0).gameObject.SetActive(show_debug_numbers);
            g.transform.GetChild(1).gameObject.SetActive(show_debug_numbers);
            g.transform.GetChild(2).gameObject.SetActive(show_debug_numbers);
            g.transform.GetChild(3).gameObject.SetActive(show_debug_numbers);
            g.transform.GetChild(4).gameObject.SetActive(show_debug_numbers);
        }
    }

    public void ToggleDebugRoomType()
    {
        show_debug_room_type = !show_debug_room_type;

        foreach (GameObject g in floors)
        {
            g.transform.GetChild(5).gameObject.SetActive(show_debug_room_type);
        }
    }


}
