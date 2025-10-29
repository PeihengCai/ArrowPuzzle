using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PuzzleGraph : MonoBehaviour
{
    public GenData GenData { get { return _genData; } set { _genData = value; } }
    private GenData _genData;

    private int[,] graph;
    private Dictionary<int,Arrow> arrows;
    private Dictionary<int,List<int>> _arrowBlockGraph;
    private int _occupiedPoints;

    void Start()
    {
        arrows = new Dictionary<int, Arrow>();
        _arrowBlockGraph = new Dictionary<int, List<int>>();
    }

    /// <summary>
    /// 设置图中的点被某个箭头占据
    /// </summary>
    public void SetGraphPoint(Vector2 point, int arrowID)
    {
        graph[(int)point.x, (int)point.y] = arrowID;
        _occupiedPoints++;
    }

    /// <summary>
    /// 查询地图中的点是否已被占据
    /// </summary>
    public bool IsPointOccupied(Vector2 point)
    {
        return graph[(int)point.x, (int)point.y] != -1;
    }

    /// <summary>
    /// 查询箭头是否被其他箭头阻挡
    /// </summary>
    public bool IsArrowBlock(int arrowID)
    {
        //Debug.LogWarning($"Arrow {arrowID} is blocked by {_arrowBlockGraph[arrowID].Count} arrows.");
        return _arrowBlockGraph[arrowID].Count > 0;
    }

    /// <summary>
    /// 主体生成函数。共尝试attempts次数进行生成
    /// </summary>
    public PuzzleGraph GeneratePuzzle()
    {
        ClearArrows();

        graph = new int[_genData.Width, _genData.Height];
        for (int i = 0; i < _genData.Width; i++)
            for (int j = 0; j < _genData.Height; j++)
                graph[i, j] = -1;
        
        for (int i = 0; i < _genData.MaxAttempts; i++)
        {
            // 检查密度是否已经达标
            if (_occupiedPoints / (_genData.Width * _genData.Height * 1.0f) >= _genData.Density)
                break;

            Vector2 start = ChooseStartPoint();
            if (float.IsNegativeInfinity(start.x) || float.IsNegativeInfinity(start.y))
                break;
            int newID = GenerateArrow(start, i);
            if (newID == -1) continue;

            // 检查新箭头有效性（从该箭头出发是否形成环）-> 删除无效箭头或保留
            if (!Validate(newID))
            {
                //Debug.LogWarning($"Arrow {newID} has circle");
                DeleteArrow(newID);
            }
        }

        //Debug.Log("Finish Generating.");
        return this;
    }

    /// <summary>
    /// 根据图中未被占用的点，随机选择一个作为新箭头的起点。
    /// 若随机生成的点无效则重新生成，有效尝试次数为Height*Width次，超过次数被认为无效。
    /// </summary>
    /// 
    /// <returns>
    /// 有效的随机起点，无效则返回负无穷表示失败
    /// </returns>
    private Vector2 ChooseStartPoint()
    {
        /// ********************************* ///
        /// 法一：在有限次数内尝试生成有效起点法 ///
        /// ********************************* ///

        // if (_occupiedPoints >= _genData.Width * _genData.Height)
        // {
        //     //Debug.LogWarning("All points are occupied. Cannot choose a new start point.");
        //     return Vector2.negativeInfinity;
        // }
        // Vector2 start = new Vector2(Random.Range(0, _genData.Width), Random.Range(0, _genData.Height));
        // int attemp = 1;
        // while (graph[(int)start.x, (int)start.y] != -1 && attemp < _genData.Width * _genData.Height)
        // {
        //     start = new Vector2(Random.Range(0, _genData.Width), Random.Range(0, _genData.Height));
        //     attemp++;
        // }
        // if (attemp >= _genData.Width * _genData.Height) return Vector2.negativeInfinity;
        // return start;

        /// ************************** ///
        /// 法二：有概率地从内层往外生成 ///
        /// ************************** ///

        // 寻找未被占用的点
        List<Vector2> freePoints = new List<Vector2>();
        for (int i = 0; i < _genData.Width; i++)
        {
            for (int j = 0; j < _genData.Height; j++)
            {
                if (graph[i, j] == -1)
                {
                    freePoints.Add(new Vector2(i, j));
                }
            }
        }
        if (freePoints.Count == 0) return Vector2.negativeInfinity;

        // 根据距离中心点的远近进行排序
        Vector2 center = new Vector2(_genData.Width/2, _genData.Height/2);
        freePoints.Sort((a, b) => Vector2.Distance(a, center).CompareTo(Vector2.Distance(b, center)));

        // 决定本次是否从内到外生成，还是纯随机
        float roll = Random.Range(0f, 1f);
        if (roll <= _genData.FromInsideProb)
            return freePoints[Random.Range(0, Math.Min(20, freePoints.Count / 2))];
        else
            return freePoints[Random.Range(0, freePoints.Count)];
    }

    /// <summary>
    /// 根据随机生成的起点，生成一个折线箭头并加入PuzzleGraph中
    /// </summary>
    /// <param name="start"> ChooseStartPoint中生成的有效起点 </param>
    /// <returns> 有效键头的ID值，如果无效则为-1 </returns>
    private int GenerateArrow(Vector2 start, int attempt)
    {
        // 在新的起点处尝试摆放一个折线箭头
        if (graph[(int)start.x, (int)start.y] != -1)
        {
            //Debug.LogWarning($"Start point {start} already occupied by Arrow {graph[(int)start.x, (int)start.y]}");
            return -1;
        }

        // 生成箭头
        // Arrow newArrow = Instantiate(_arrowPrefab, transform.Find("Arrows")).GetComponent<Arrow>();
        // newArrow.InitializeArrow(arrows.Count, this);
        // newArrow.name = $"Arrow_{arrows.Count}";
        Arrow newArrow = ArrowPuzzleGame.Instance.SprawnArrow(attempt);
        newArrow.GenerateArrow(start, this);

        // 更新puzzle的箭头和阻挡图
        arrows[newArrow.arrowID] = newArrow;
        _arrowBlockGraph[newArrow.arrowID] = new List<int>();
        if (!newArrow.DrawArrow())
        {
            DeleteArrow(newArrow.arrowID);
            return -1;
        }

        return newArrow.arrowID;
    }

    /// <summary>
    /// 使用DFS检查从新加入的箭头出发是否会形成环
    /// </summary>
    private bool Validate(int newID)
    {
        // 更新所有箭头的阻挡关系
        foreach (var arr in arrows)
        {
            _arrowBlockGraph[arr.Key] = arr.Value.CheckBlock();
        }

        // 从新箭头出发检查是否有环
        Dictionary<int, int> visited = new Dictionary<int, int>();
        foreach (var arr in arrows){
            visited[arr.Key] = 0;
        }
        return dfs(visited, newID);
    }

    private bool dfs(Dictionary<int, int> visited, int currID)
    {
        if (visited[currID] == 1) return false;
        if (visited[currID] == 0) visited[currID] = 1;

        foreach (var blockID in _arrowBlockGraph[currID])
        {
            if (visited[blockID] == 2) continue;
            if (!dfs(visited, blockID)) return false;
        }
        visited[currID] = 2;
        return true;
    }

    /// <summary>
    /// 删除某一个箭头
    /// </summary>
    public void DeleteArrow(int id)
    {
        Arrow arrowToDelete = arrows[id];

        // 找到所有受该键头阻挡的箭头
        HashSet<int> affectedArrows = new HashSet<int>();
        foreach (var arr in arrows)
        {
            if (arr.Key == id) continue;
            if (_arrowBlockGraph[arr.Key]?.Contains(id) == true)
            {
                affectedArrows.Add(arr.Key);
            }
        }

        // 删除箭头占据的点
        foreach (var point in arrowToDelete.arrowPoints)
        {
            graph[(int)point.x, (int)point.y] = -1;
            _occupiedPoints--;
        }

        arrows.Remove(id);
        _arrowBlockGraph.Remove(id);

        ArrowPuzzleGame.Instance.DeleteArrow(arrowToDelete);
        // 更新受影响的箭头的阻挡关系
        foreach (var arrID in affectedArrows)
        {
            _arrowBlockGraph[arrID] = arrows[arrID].CheckBlock();
        }
    }

    /// <summary>
    /// 清除所有箭头
    /// </summary>
    private void ClearArrows()
    {
        arrows?.Clear();
        _arrowBlockGraph?.Clear();
        _occupiedPoints = 0;
    }
}
