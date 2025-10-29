using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class Arrow : MonoBehaviour, IPointerClickHandler
{
    public GameObject arrowPrefab;
    public List<Vector2> arrowPoints;
    public Vector2 dir;
    public int arrowID;
    public LayerMask layerMask;
    public bool canRemove = true;

    private LineRenderer _lineRenderer;
    private EdgeCollider2D _edgeCollider;
    private PuzzleGraph _puzzleGraph;
    private readonly Vector2[] CardinalDirs = {Vector2.up,Vector2.right,Vector2.down,Vector2.left};
    private int _length;
    private float _turnProb;

    public void InitializeArrow(int id, PuzzleGraph puzzleGraph)
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _edgeCollider = GetComponent<EdgeCollider2D>();

        arrowID = id;
        _puzzleGraph = puzzleGraph;
        arrowPoints = new List<Vector2>();

        // 随机生成长度和转向概率
        _length = Random.Range(_puzzleGraph.GenData.MinArrowLength, _puzzleGraph.GenData.MaxArrowLength + 1);
        _turnProb = Random.Range(_puzzleGraph.GenData.MinTurnProb, _puzzleGraph.GenData.MaxTurnProb);
        dir = CardinalDirs[Random.Range(0, CardinalDirs.Length)];
    }

    public void GenerateArrow(Vector2 start, PuzzleGraph puzzleGraph)
    {
        _puzzleGraph = puzzleGraph;
        arrowPoints.Add(start);
        _puzzleGraph.SetGraphPoint(arrowPoints[0], arrowID);
        for (int i = 1; i <= _length; i++)
        {
            // 检查可用方向
            List<Vector2> possibleDirs = new List<Vector2>();
            bool canGoStraight = false;
            foreach (var newDir in CardinalDirs)
            {
                Vector2 nextP = arrowPoints[i - 1] + newDir;
                if (nextP.x < 0 || nextP.y < 0 || nextP.x >= _puzzleGraph.GenData.Width || nextP.y >= _puzzleGraph.GenData.Height)
                    continue;
                if (_puzzleGraph.IsPointOccupied(nextP))
                    continue;
                if (newDir == dir)
                {
                    canGoStraight = true;
                    continue;
                }
                possibleDirs.Add(newDir);
            }

            // 如果没有可用方向，已经到头，停止生成
            if (possibleDirs.Count == 0 && !canGoStraight) break;

            // 检查转向概率
            float turnRoll = Random.Range(0f, 1f);
            // 如果小于转向概率或无法直行，则尝试转向，转向不成功则继续直行
            if ((turnRoll <= _turnProb || !canGoStraight) && possibleDirs.Count > 0)
            {
                dir = possibleDirs[Random.Range(0, possibleDirs.Count)];
            }
            arrowPoints.Add(arrowPoints[i - 1] + dir);
            _puzzleGraph.SetGraphPoint(arrowPoints[i], arrowID);
        }

    }

    public List<int> CheckBlock()
    {
        Vector2 origin = arrowPoints[arrowPoints.Count - 1] + 0.1f * dir;
        RaycastHit2D[] hit = Physics2D.RaycastAll(origin, dir, Math.Max(_puzzleGraph.GenData.Height, _puzzleGraph.GenData.Width), layerMask);
        // Debug.Log(origin + dir);

        Array.Sort(hit, (a, b) => Vector2.Distance(origin, a.point).CompareTo(Vector2.Distance(origin, b.point)));
        List<int> blockArrows = new List<int>();
        foreach (var h in hit)
        {
            if (h.collider != null)
                blockArrows.Add(h.collider.GetComponent<Arrow>().arrowID);
        }

        // Debug.Log($"Arrow {arrowID} with dir {dir} blocks arrows: {string.Join(", ", blockArrows)}");
        //_blockedArrows = blockArrows;
        return blockArrows;
    }
    
    public bool DrawArrow()
    {
        if (arrowPoints.Count <= 1) return false;

        _lineRenderer.positionCount = arrowPoints.Count;
        for (int i = 0; i < arrowPoints.Count; i++)
        {
            _lineRenderer.SetPosition(i, new Vector3(arrowPoints[i].x, arrowPoints[i].y, 0));
        }
        _edgeCollider.SetPoints(arrowPoints);

        arrowPrefab.SetActive(true);
        arrowPrefab.transform.position = arrowPoints[arrowPoints.Count - 1];

        // 上：不转
        // 右：-90
        // 下：180
        // 左：90
        arrowPrefab.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90);
        return true;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        // Debug.Log("click");
        if (_puzzleGraph.IsArrowBlock(arrowID)) return;

        _puzzleGraph.DeleteArrow(arrowID);
    }
    
}
