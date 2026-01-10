using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    [SerializeField] private UIDocument _UIDocument;
    [SerializeField] private FirefliesManager _firefliesManager;
    
    // FPS Counter
    private Label _fpsValueLabel;
    
    // Firefly counter
    private TextField _fireflyCountTextField;
    
    List<float> _timedFPS = new (10);
    private int _currentTimedFPSIndex = 0;
    
    private void Start()
    {
        var rootElement = _UIDocument.rootVisualElement;
        
        // FPS Counter
        _fpsValueLabel = rootElement.Q<Label>("FPSValue");
        
        // Firefly counter
        _fireflyCountTextField = rootElement.Q<TextField>("FireflyCounterTextField");
        _fireflyCountTextField.SetValueWithoutNotify(_firefliesManager.GetFireflyCount().ToString());
        _fireflyCountTextField.RegisterValueChangedCallback(evt => UpdateFireflyCount(System.Convert.ToInt32(evt.newValue)));

        for (int i = 0; i < 10; i++)
        {
            _timedFPS.Add(0); 
        }
    }

    private void Update()
    {
        _timedFPS[_currentTimedFPSIndex] = Time.deltaTime;
        _currentTimedFPSIndex++;

        if (_currentTimedFPSIndex >= 10)
        {
            _currentTimedFPSIndex = 0;
        }
        
        float average = _timedFPS.Sum();

        average /= _timedFPS.Count;
        
        _fpsValueLabel.text = String.Format("{0:0.0}", 1.0f / average);
    }

    private void UpdateFireflyCount(int newValue)
    {
        _firefliesManager.SetFireflyCount(newValue);
    }
}
