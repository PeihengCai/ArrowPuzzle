using UnityEngine;

public struct GenData
{
    public int MaxAttempts;
    public float Density;
    public int Width;
    public int Height;
    public int MinArrowLength;
    public int MaxArrowLength;
    public float MinTurnProb;
    public float MaxTurnProb;
    public float FromInsideProb;
}

public class ArrowPuzzleGame : MonoBehaviour
{
    public static ArrowPuzzleGame Instance;
    private GenData _genData;
    private PuzzleGraph _puzzleGraph;
    private GameObject _arrowPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        _puzzleGraph = FindObjectOfType<PuzzleGraph>();
        _arrowPrefab = Resources.Load<GameObject>("Prefabs/Arrow");
    }

    public void Generate(GenData genData)
    {
        _genData = genData;
        ClearArrows();

        Camera.main.transform.position = new Vector3(_genData.Width / 2f, _genData.Height / 2f, -10);
        Camera.main.orthographicSize = Mathf.Max(_genData.Width, _genData.Height) * 0.65f + 2.38f;


        _puzzleGraph.GenData = genData;
        _puzzleGraph.GeneratePuzzle();
    }

    public Arrow SprawnArrow(int arrowsID)
    {
        Arrow newArrow = Instantiate(_arrowPrefab, transform.Find("Arrows")).GetComponent<Arrow>();
        newArrow.InitializeArrow(arrowsID, _puzzleGraph);
        newArrow.name = $"Arrow_{arrowsID}";
        return newArrow;
    }

    public void DeleteArrow(Arrow arrow)
    {
        Destroy(arrow.gameObject);
    }

    private void ClearArrows()
    {
        var arr = transform.Find("Arrows");
        for (int i = arr.childCount - 1; i >= 0; i--)
        {
            Destroy(arr.GetChild(i).gameObject);
        }
    }
}
