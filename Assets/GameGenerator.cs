﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Random = System.Random;
using System.Linq;
using UnityEngine.UI;

[Serializable]
public class Platform
{
    public int x;
    public int y;
    public int xSize;
    public int ySize;

    // 2d Platform
    public static int z = 0;
    public static int zSize = 1;

    public Platform(int av1, int av2, int av3, int av4)
    {
        x = av1;
        y = av2;
        xSize = av3;
        ySize = av4;
    }

    public BoundsInt ToBounds()
    {
        return new BoundsInt(x, y, z, xSize, ySize, zSize);
    }

    public Platform xMirror()
    {
        int newX = -x - xSize;
        return new Platform(newX, y, xSize, ySize);
    }
}

[Serializable]
public class Platforms
{
    public List<Platform> platformList;
    public int player1x;
    public int player1y;
    public int player2x;
    public int player2y;

    public Platforms(List<Platform> l, int p1x, int p1y, int p2x, int p2y)
    {
        platformList = l;
        player1x = p1x;
        player1y = p1y;
        player2x = p2x;
        player2y = p2y;
    }
}

public class GameGenerator : MonoBehaviour
{

    public Player player;
    public Platforms platforms;
    private static string level_path = "Assets\\Game\\level.json";

    //UI components for each player
    public GameObject p1HUD;
    public GameObject p2HUD;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize RNG
        Random rand = new Random();

        // Generate / Load Map
        MapGenerator mapGen = new MapGenerator(2, 2, 3, 6, rand);
        platforms = mapGen.generate();
        //Debug.Log(platforms.platformList[0].x);

        // Write to file
        if (!File.Exists(level_path))
        {
            var level_string = JsonUtility.ToJson(platforms);
            Debug.Log(platforms);
            Debug.Log(level_string);
            File.Create(level_path).Dispose();
            File.WriteAllText(level_path, level_string);
        }
        // If the file exists, read from it
        else
        {
            var inputString = File.ReadAllText(level_path);
            //TODO: Temporarily disabled for ease of iteration.
            //platforms = JsonUtility.FromJson<Platforms>(inputString);
        }

        //TODO: Fetch from document - replace with constants set above during setup
        
        //Player 1 Instantiation
        Vector3 spawnLocationP1 = new Vector3(platforms.player1x, platforms.player1y, 0);
        Player player1 = Instantiate(player, spawnLocationP1, Quaternion.identity);
        //Player 1 Controls
        player1.leftKey = KeyCode.A;
        player1.rightKey = KeyCode.D;
        player1.jumpKey = KeyCode.W;
        player1.fallKey = KeyCode.S;
        player1.move1Key = KeyCode.Space;
        //player 1 Move 1 Definition
        player1.move1.center = player1.transform.position + new Vector3(-1, 0, 0);
        //TODO: Update UI to be dynamic, for now, hardcode
        player1.playerName = "Player 1";
        player1.playerDetails = p1HUD.GetComponent<Text>();
        player1.respawnLoc = new Vector2(platforms.player1x, platforms.player1y);
        

        //Player 2 Instantiation
        Vector3 spawnLocationP2 = new Vector3(platforms.player2x, platforms.player2y, 0);
        Player player2 = Instantiate(player, spawnLocationP2, Quaternion.identity);
        //Player 2 Controls
        player2.leftKey = KeyCode.J;
        player2.rightKey = KeyCode.L;
        player2.jumpKey = KeyCode.I;
        player2.fallKey = KeyCode.K;
        player2.move1Key = KeyCode.Return;

        player2.playerName = "Player 2";
        player2.playerDetails = p2HUD.GetComponent<Text>();
        player2.respawnLoc = new Vector2(platforms.player2x, platforms.player2y);
        
        // Generate / Load constants for each player

        // Generate / Load constants for the player's moves 

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class MapGenerator
{
    public int jumpHeight;
    public int jumpLength;
    public int nPlatforms;
    public int maxPlatformSize;
    Random rand;

    public static int minWidth = 1;
    public static int maxWidth = 2;
    public static int initialY = -3;

    public MapGenerator(int _jumpHeight, int _jumpLength, int _nPlatforms, int _maxPlatformSize, Random _rand)
    {
        jumpHeight = _jumpHeight;
        jumpLength = _jumpLength;
        nPlatforms = _nPlatforms;
        maxPlatformSize = _maxPlatformSize;
        rand = _rand;
    }

    public Platforms generate()
    {
        List<Platform> allPlatforms = new List<Platform>();
        Stack<Platform> stack = new Stack<Platform>();
        // Create initial platform
        Platform initialPlatform = Initial();
        allPlatforms.Add(initialPlatform);
        stack.Push(initialPlatform);
        // For each platform, create 0-2 children
        // Stop when the length reaches nPlatforms
        while (allPlatforms.Count < nPlatforms)
        {
            if (stack.Count == 0)
            {
                break;
            }
            Platform top = stack.Pop();
            if (rand.Next(0, 2) == 1)
            {
                Platform leftPlatform = Left(top);
                stack.Push(leftPlatform);
                allPlatforms.Add(leftPlatform);
            }
            if (rand.Next(0, 2) == 1)
            {
                Platform abovePlatform = Above(top);
                stack.Push(abovePlatform);
                allPlatforms.Add(abovePlatform);
            }
        }
        // Mirror everything around y = 0
        List<Platform> mirrorPlatforms = new List<Platform>();
        foreach (Platform platform in allPlatforms)
        {
            mirrorPlatforms.Add(platform.xMirror());
        }
        allPlatforms = allPlatforms.Concat(mirrorPlatforms).ToList();
        // Player 1 spawns on the initial platform
        int p1x = rand.Next(initialPlatform.x, initialPlatform.x + initialPlatform.xSize + 1);
        int p1y = initialY + initialPlatform.ySize + 1;
        // Mirror Player 2's spawn relative to Player 1's
        int p2x = -p1x;
        int p2y = p1y;
        return new Platforms(allPlatforms, p1x, p1y, p2x, p2y);
    }

    public Platform Initial()
    {
        int y = initialY;
        int ySize = rand.Next(minWidth, maxWidth + 1);
        int x = rand.Next(-maxPlatformSize - 1, -2);
        int midGap = jumpLength / 2;
        int xSize = -x + rand.Next(-midGap, 0);
        Debug.Log("initial");
        Debug.Log(x);
        Debug.Log(y);
        Debug.Log(xSize);
        Debug.Log(ySize);
        return new Platform(x, y, xSize, ySize);
    }

    public Platform Above(Platform platform)
    {
        // Generate y values
        int yMin = 2;
        int yMax = jumpHeight;
        int platformTop = platform.y + platform.ySize;
        int y = platformTop + rand.Next(yMin, yMax + 1);
        Debug.Log(minWidth);
        Debug.Log(maxWidth + 1);
        int ySize = rand.Next(minWidth, Math.Min(maxWidth, y - platformTop));
        //Math.Min(rand.Next(minWidth, maxWidth + 1), y - platform.y);
        // Generate x values
        int xMin = platform.x + 1;
        int xMax = platform.x + platform.xSize;
        int x = rand.Next(xMin, xMax);
        int xSize = rand.Next(2, platform.xSize - x + 1);
        Debug.Log("above");
        Debug.Log(yMin);
        Debug.Log(yMax);
        Debug.Log(x);
        Debug.Log(y);
        Debug.Log(xSize);
        Debug.Log(ySize);
        return new Platform(x, y, xSize, ySize);
    }

    public Platform Left(Platform platform)
    {
        // Generate y values
        int yMin = platform.y - jumpHeight;
        int yMax = platform.y + jumpHeight;
        int y = rand.Next(yMin, yMax + 1);
        int ySize = rand.Next(minWidth, maxWidth + 1);
        // Generate x values
        int xSize = rand.Next(2, maxPlatformSize);
        int xRight = rand.Next(1, jumpLength);
        int x = platform.x - xRight - xSize;
        Debug.Log("left");
        Debug.Log(x);
        Debug.Log(y);
        Debug.Log(xSize);
        Debug.Log(ySize);
        return new Platform(x, y, xSize, ySize);
    }
}
