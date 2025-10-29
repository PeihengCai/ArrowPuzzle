using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleGenUI : MonoBehaviour
{
    public TMP_InputField InputAttempts;
    public Slider SliderDensity;
    public TextMeshProUGUI Textdensity;
    public Slider SliderFromInside;
    public TextMeshProUGUI TextFromInside;
    public TMP_InputField InputWidth;
    public TMP_InputField InputHeight;
    public TMP_InputField InputMinLen;
    public TMP_InputField InputMaxLen;
    public Slider SliderMinProb;
    public TextMeshProUGUI TextminTurnProb;
    public Slider SliderMaxProb;
    public TextMeshProUGUI TextmaxTurnProb;
    public Button ButtonGenerate;
    public Button ButtonSetDefault;

    private GenData _genData;
    // Start is called before the first frame update
    void Start()
    {
        SetToDefault();
    }

    void OnEnable()
    {
        AddListeners();
    }

    void OnDisable()
    {
        RemoveListeners();
    }

    void AddListeners()
    {
        InputAttempts.onEndEdit.AddListener(SetAttempt);
        SliderDensity.onValueChanged.AddListener(SetDensity);
        SliderFromInside.onValueChanged.AddListener(SetFromInside);
        InputWidth.onEndEdit.AddListener(SetWidth);
        InputHeight.onEndEdit.AddListener(SetHeight);
        InputMinLen.onEndEdit.AddListener(SetMinArrowLength);
        InputMaxLen.onEndEdit.AddListener(SetMaxArrowLength);
        SliderMinProb.onValueChanged.AddListener(SetMinTurnProb);
        SliderMaxProb.onValueChanged.AddListener(SetMaxTurnProb);
        ButtonGenerate.onClick.AddListener(GeneratePuzzle);
        ButtonSetDefault.onClick.AddListener(SetToDefault);
    }

    void RemoveListeners()
    {
        InputAttempts.onEndEdit.RemoveListener(SetAttempt);
        SliderDensity.onValueChanged.RemoveListener(SetDensity);
        SliderFromInside.onValueChanged.RemoveListener(SetFromInside);
        InputWidth.onEndEdit.RemoveListener(SetWidth);
        InputHeight.onEndEdit.RemoveListener(SetHeight);
        InputMinLen.onEndEdit.RemoveListener(SetMinArrowLength);
        InputMaxLen.onEndEdit.RemoveListener(SetMaxArrowLength);
        SliderMinProb.onValueChanged.RemoveListener(SetMinTurnProb);
        SliderMaxProb.onValueChanged.RemoveListener(SetMaxTurnProb);
        ButtonGenerate.onClick.RemoveListener(GeneratePuzzle);
        ButtonSetDefault.onClick.RemoveListener(SetToDefault);
    }

    void SetToDefault()
    {
        _genData.MaxAttempts = 300;
        _genData.Density = 1;
        _genData.Width = 10;
        _genData.Height = 10;
        _genData.MinArrowLength = 1;
        _genData.MaxArrowLength = 7;
        _genData.MinTurnProb = 0.1f;
        _genData.MaxTurnProb = 0.9f;
        _genData.FromInsideProb = 0.5f;

        SliderDensity.value = _genData.Density;
        SliderMinProb.value = _genData.MinTurnProb;
        SliderMaxProb.value = _genData.MaxTurnProb;
        SliderFromInside.value = _genData.FromInsideProb;
        UpdateGenUI();
    }

    void UpdateGenUI()
    {
        InputAttempts.text = _genData.MaxAttempts.ToString();
        Textdensity.text = _genData.Density.ToString("F2");
        TextFromInside.text = _genData.FromInsideProb.ToString("F2");

        InputWidth.text = _genData.Width.ToString();
        InputHeight.text = _genData.Height.ToString();
        InputMinLen.text = _genData.MinArrowLength.ToString();
        InputMaxLen.text = _genData.MaxArrowLength.ToString();

        TextminTurnProb.text = _genData.MinTurnProb.ToString("F2");
        TextmaxTurnProb.text = _genData.MaxTurnProb.ToString("F2");
    }

    public void SetAttempt(string attempts)
    {
        if (string.IsNullOrWhiteSpace(attempts)) { UpdateGenUI(); return; }
        if (int.TryParse(attempts.Trim(), out int v) && v > 0 && v <= 1500)
            _genData.MaxAttempts = v;
        UpdateGenUI();
    }

    public void SetDensity(float d)
    {
        if (d > 0 && d <= 1)
            _genData.Density = d;
        UpdateGenUI();
    }

    public void SetFromInside(float d)
    {
        if (d > 0 && d <= 1)
            _genData.FromInsideProb = d;
        UpdateGenUI();
    }

    public void SetWidth(string w)
    {
        if (string.IsNullOrWhiteSpace(w)) { UpdateGenUI(); return; }
        if (int.TryParse(w.Trim(), out int v) && v > 0 && v <= 50)
            _genData.Width = v;
        UpdateGenUI();
    }

    public void SetHeight(string h)
    {
        if (string.IsNullOrWhiteSpace(h)) { UpdateGenUI(); return; }
        if (int.TryParse(h.Trim(), out int v) && v > 0 && v <= 50)
            _genData.Height = v;
        UpdateGenUI();
    }

    public void SetMinArrowLength(string len)
    {
        if (int.TryParse(len?.Trim(), out int l) && l >= 1 && l <= _genData.MaxArrowLength)
        {
            _genData.MinArrowLength = l;
        }
        UpdateGenUI();
    }

    public void SetMaxArrowLength(string len)
    {
        if (int.TryParse(len?.Trim(), out int l) && l >= _genData.MinArrowLength)
        {
            _genData.MaxArrowLength = l;
        }
        UpdateGenUI();
    }

    public void SetMinTurnProb(float p)
    {
        if (p >= 0f && p <= _genData.MaxTurnProb)
            _genData.MinTurnProb = p;
        else SliderMinProb.value = _genData.MinTurnProb;
        UpdateGenUI();
    }

    public void SetMaxTurnProb(float p)
    {
        if (p >= _genData.MinTurnProb && p <= 1f)
            _genData.MaxTurnProb = p;
        else SliderMaxProb.value = _genData.MaxTurnProb;
        UpdateGenUI();
    }

    public void GeneratePuzzle()
    {
        ArrowPuzzleGame.Instance.Generate(_genData);
    }
}
