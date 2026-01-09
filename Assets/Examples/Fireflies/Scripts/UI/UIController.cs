using System.Collections;
using System.Collections.Generic;
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
    
    private void Start()
    {
        var rootElement = _UIDocument.rootVisualElement;
        
        // FPS Counter
        _fpsValueLabel = rootElement.Q<Label>("FPSValue");
        
        // Firefly counter
        _fireflyCountTextField = rootElement.Q<TextField>("FireflyCounterTextField");
        _fireflyCountTextField.SetValueWithoutNotify(_firefliesManager.GetFireflyCount().ToString());
        _fireflyCountTextField.RegisterValueChangedCallback(evt => UpdateFireflyCount(System.Convert.ToInt32(evt.newValue)));
    }

    private void Update()
    {
        _fpsValueLabel.text = (1.0f / Time.deltaTime).ToString();
    }

    private void UpdateFireflyCount(int newValue)
    {
        Debug.Log("ICI");
        _firefliesManager.SetFireflyCount(newValue);
    }
}
