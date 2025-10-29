using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PuzzleGraph))]
public class PuzzleGraphEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        
        PuzzleGraph puzzleGraph = (PuzzleGraph)target;
        if (GUILayout.Button("Generate Puzzle")) {
            puzzleGraph.GeneratePuzzle();
        }
    }
}