using System;
using System.Collections.Generic;
using UnityEngine;

public class Snapshot
{
    public string description;

    public List<SnapshotElement> elements = new List<SnapshotElement>();

    public void OnDrawGizmos()
    {
        foreach (SnapshotElement element in elements)
        {
            element.DrawGizmos();
        }
    }
}

public abstract class SnapshotElement
{
    public abstract void Instantiate();
    public abstract void DrawGizmos();
}

public class SnapshotGizmoWireCube : SnapshotElement
{
    private Vector3 _center;
    private Vector3 _size;

    private bool _changeColor = false;
    private Color _color;
    
    public SnapshotGizmoWireCube(Vector3 center, Vector3 size)
    {
        _center = center;
        _size = size;
    }
    
    public SnapshotGizmoWireCube(Vector3 center, Vector3 size, Color color)
    {
        _center = center;
        _size = size;

        _changeColor = true;
        _color = color;
    }
    
    public override void Instantiate()
    {
    }

    public override void DrawGizmos()
    {
        if (_changeColor)
        {
            Gizmos.color = _color;
        }
        Gizmos.DrawWireCube(_center, _size);
    }
}

public class SnapshotRecorder : MonoBehaviour
{
    private List<Snapshot> _snapshots = new List<Snapshot>();
    private int _recordingSnapshot;
    public int ActiveSnapshotIndex = 0;
    
    private bool _isPlaying = false;
    private float _timer = 0f;
    private float _playSpeed = 0.5f;

    private static SnapshotRecorder _instance;
    public static SnapshotRecorder Instance {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SnapshotRecorder");
                _instance = go.AddComponent<SnapshotRecorder>();
                _instance.Init();
            }
            
            return _instance;
        }
    }

    private void Init()
    {
        _snapshots = new List<Snapshot>();
        _recordingSnapshot = -1;
    }
    
    private void Update()
    {
        if (_isPlaying && _snapshots.Count > 0)
        {
            _timer += Time.deltaTime;
            
            if (_timer >= _playSpeed)
            {
                _timer = 0f;
                
                if (ActiveSnapshotIndex < _snapshots.Count - 1)
                {
                    ActiveSnapshotIndex++;
                }
                else
                {
                    _isPlaying = false;
                }
            }
        }
    }

    public void BeginNewSnapshot()
    {
        _recordingSnapshot++;
        _snapshots.Add(new Snapshot());
    }
    
    public void BeginNewSnapshot(string description)
    {
        _recordingSnapshot++;
        _snapshots.Add(new Snapshot());
        _snapshots[_recordingSnapshot].description = description;
    }

    public void AddSnapshotElement(SnapshotElement element)
    {
        if (_recordingSnapshot < 0 || _recordingSnapshot >= _snapshots.Count)
        {
            Debug.LogError("Error while adding snapshot element");
            return;
        }
        
        _snapshots[_recordingSnapshot].elements.Add(element);
    }

    public void OnDrawGizmos()
    {
        if (_snapshots.Count == 0) return;

        if (ActiveSnapshotIndex < 0)
        {
            ActiveSnapshotIndex = 0;
        }
        
        if(ActiveSnapshotIndex >= _snapshots.Count)
        {
            ActiveSnapshotIndex = _snapshots.Count - 1;
        }
        
        _snapshots[ActiveSnapshotIndex].OnDrawGizmos();
    }
    
    private void OnGUI()
    {
        if (_snapshots.Count == 0)
        {
            GUI.Label(new Rect(10, Screen.height - 30, 300, 20), "Snapshot Recorder: Aucune étape enregistrée.");
            return;
        }

        int maxIndex = _snapshots.Count - 1;

        // Bottom area
        float barHeight = 40f;
        float margin = 10f;
        Rect barRect = new Rect(margin, Screen.height - barHeight - margin, Screen.width - (2 * margin), barHeight);
        
        GUI.BeginGroup(barRect, GUI.skin.box); 

        float groupWidth = barRect.width;
        float playButtonWidth = 70f;
        float navButtonWidth = 60f;
        float labelWidth = 250f;
        float totalButtonSpacing = 40f; 
        float paddingLeft = 20f;
        
        float sliderWidth = groupWidth - playButtonWidth - (2 * navButtonWidth) - labelWidth - totalButtonSpacing; 
        
        // Play / Pause
        string playPauseText = _isPlaying ? "|| Pause" : "▶ Play";
        if (GUI.Button(new Rect(paddingLeft, 5, playButtonWidth, 30), playPauseText))
        {
            _isPlaying = !_isPlaying;
            
            if (_isPlaying && ActiveSnapshotIndex == maxIndex)
            {
                ActiveSnapshotIndex = 0;
            }
            _timer = 0f; 
        }
        
        // Next / Previous
        float navX = playButtonWidth + 5f + paddingLeft;

        if (GUI.Button(new Rect(navX, 5, navButtonWidth, 30), "<< "))
        {
            _isPlaying = false; 
            ActiveSnapshotIndex = Mathf.Max(0, ActiveSnapshotIndex - 1);
        }
        
        if (GUI.Button(new Rect(navX + navButtonWidth + 5f, 5, navButtonWidth, 30), ">>"))
        {
            _isPlaying = false;
            ActiveSnapshotIndex = Mathf.Min(maxIndex, ActiveSnapshotIndex + 1);
        }

        // Slider
        float sliderX = navX + (2 * navButtonWidth) + 10f;
        
        int oldIndex = ActiveSnapshotIndex;
        
        ActiveSnapshotIndex = (int)GUI.HorizontalSlider(
            new Rect(sliderX, 15, sliderWidth, 20), 
            ActiveSnapshotIndex, 
            0, 
            maxIndex
        );

        if (oldIndex != ActiveSnapshotIndex)
        {
             _isPlaying = false;
        }
        
        // Step + Description
        float labelX = sliderX + sliderWidth + 10f;
        string description = string.IsNullOrEmpty(_snapshots[ActiveSnapshotIndex].description) 
            ? "" 
            : ": "  + _snapshots[ActiveSnapshotIndex].description;
            
        string stepInfo = $"Step {ActiveSnapshotIndex + 1}/{_snapshots.Count} {description}";
        
        GUI.Label(new Rect(labelX, 5, labelWidth, 30), stepInfo);

        GUI.EndGroup();
    }
}
