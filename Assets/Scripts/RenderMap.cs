using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.IO;

[Serializable]
public class MapTile
{

    public string Id;
    public string Type;
    public float Width;
    public float Height;
    public float X = 0.0f;
    public float Y = 0.0f;

}

[Serializable]
public class Map
{
    public List<MapTile> List;
    public float leftBorder = +Mathf.Infinity;
    public float rightBorder = -Mathf.Infinity;
    public float topBorder = -Mathf.Infinity;
    public float bottomBorder = +Mathf.Infinity;
}

public class XComparer : IComparer<MapTile>
{
    public int Compare(MapTile t1, MapTile t2)
    {
        if (t1 == null)
        {
            if (t2 == null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (t2 == null)
            {
                return 1;
            }
            else
            {
                return t1.X.CompareTo(t2.X);
            }
        }
    }
}

public class YComparer : IComparer<MapTile>
{
    public int Compare(MapTile t1, MapTile t2)
    {
        if (t1 == null)
        {
            if (t2 == null)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        else
        {
            if (t2 == null)
            {
                return 1;
            }
            else
            {
                return t1.Y.CompareTo(t2.Y);
            }
        }
    }
}

//Node of the k-d tree
public class Node
{
    public MapTile tile;
    public Node leftChild;
    public Node rightChild;

    public Node(MapTile tile, Node leftChild, Node rightChild)
    {
        this.tile = tile;
        this.leftChild = leftChild;
        this.rightChild = rightChild;
    }
}

public class KDTree
{
    public int k = 2;
    public Node root;

    //Recursive construction of the tree from the list of tiles
    private Node ConstructTree(List<MapTile> tiles, int depth, int leftIdx, int rightIdx)
    {
        //Base case of the recursion
        if (leftIdx >= rightIdx)
        {
            return null;
        }
        //Alternate coordinates at each level
        if (depth % k == 1)
        {
            tiles.Sort(leftIdx, rightIdx - leftIdx, new YComparer());
        }
        else
        {
            tiles.Sort(leftIdx, rightIdx - leftIdx, new XComparer());
        }

        //Find the median
        var medIdx = Mathf.FloorToInt(leftIdx + (rightIdx - leftIdx) / 2);
        var median = tiles[medIdx];

        //Split the data by the median and move to the next level
        Node node = new Node(median, ConstructTree(tiles, depth + 1, leftIdx, medIdx),
            ConstructTree(tiles, depth + 1, medIdx + 1, rightIdx));

        return node;
    }

    public KDTree(List<MapTile> tiles)
    {
        root = ConstructTree(tiles, 0, 0, tiles.Count);
    }

    private float distSquared(float x1, float y1, float x2, float y2)
    {
        return (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
    }

    //Compare two nodes and return the closest one to the target point
    private Node closest(Node i, Node j, float targetX, float targetY)
    {
        if (i == null)
        {
            return j;
        }
        if (j == null) {
            return i;
        }
        float distI = distSquared(i.tile.X, i.tile.Y, targetX, targetY);
        float distJ = distSquared(j.tile.X, j.tile.Y, targetX, targetY);
        if (distI < distJ)
        {
            return i;
        }
        else
        {
            return j;
        }
    }

    //Search the tree for the nearest tile to the target location
    //Takes logarithmic time O(log n), where n is the number of tiles

    public Node NearestNeighbor(Node root, float targetX, float targetY, int depth)
    {
        //Base case of the recursion
        if (root == null)
        {
            return null;
        }
        //Alternate coordinates at each level
        float current;
        float target;
        if (depth % k == 1)
        {
            target = targetY;
            current = root.tile.Y;
        }
        else
        {
            target = targetX;
            current = root.tile.X;
        }
        //Determine the branch to search
        Node nextBranch;
        Node otherBranch;
        if (target < current)
        {
            nextBranch = root.leftChild;
            otherBranch = root.rightChild;
        }
        else
        {
            nextBranch = root.rightChild;
            otherBranch = root.leftChild;
        }
        //Update the closest node so far
        Node candidate = NearestNeighbor(nextBranch, targetX, targetY, depth + 1);
        Node best = closest(candidate, root, targetX, targetY);

        float radiusSquared = distSquared(targetX, targetY, best.tile.X, best.tile.Y);
        float altDistSquared = (target - current) * (target - current);

        //Check is the distance to the discarded branch is less than the current best result
        //If so search the discarded branch for a possibly better candidate

        if (radiusSquared >= altDistSquared)
        {
            candidate = NearestNeighbor(otherBranch, targetX, targetY, depth + 1);
            best = closest(candidate, best, targetX, targetY);
        }

        return best;
    }

}
    

public class RenderMap : MonoBehaviour
{
    public string settingsPath;
    public Map settings;
    public KDTree tree;
    private Dictionary<string, Sprite> sprites;

    public Vector3 RestrictCamera(Vector3 newPosition, float width, float height)
    {
        return new Vector3(
                Mathf.Clamp(newPosition.x, settings.leftBorder + width / 2,
                            settings.rightBorder - width / 2),
                Mathf.Clamp(newPosition.y, settings.bottomBorder + height / 2,
                            settings.topBorder - height / 2),
                newPosition.z);
    }

    void Awake()
    {
        string settingsJson = File.ReadAllText(settingsPath);
        settings = JsonUtility.FromJson<Map>(settingsJson);

        sprites = new Dictionary<string, Sprite>();

        foreach (var t in settings.List)
        {
            var tex = Resources.Load<Texture2D>(t.Id);
            var sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
            sprites[t.Id] = sprite;

            //Compute the borders of the map
            settings.leftBorder = Mathf.Min(t.X - t.Width/2, settings.leftBorder);
            settings.rightBorder = Mathf.Max(t.X + t.Width / 2, settings.rightBorder);
            settings.topBorder = Mathf.Max(t.Y + t.Height / 2, settings.topBorder);
            settings.bottomBorder = Mathf.Min(t.Y - t.Height / 2, settings.bottomBorder);
        }

        tree = new KDTree(settings.List);
    }

    void Start()
    {
        foreach (var t in settings.List)
        {
            var tileToSpawn = new GameObject(t.Id);
            var sr = tileToSpawn.AddComponent<SpriteRenderer>();
            tileToSpawn.transform.position = new Vector2(t.X, t.Y);
            sr.size = new Vector2(t.Width, t.Height);
            sr.sprite = sprites[t.Id];
        }
    }

}
