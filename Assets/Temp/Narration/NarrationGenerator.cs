using System.Collections.Generic;
using TMPro;
using UnityEngine;

// 1. Créer un text à trou
// 2. Afficher ce texte à trou
// 3. Avoir une référence sur le texte dans le canvas
// 4. Mettre à jour le texte qui est affiché à l'écran
// 5. Créer des catégories de texte pour remplir les trous
// 6. Remplir les trous
// 7. Afficher le texte compléter
// 8. Faire appaître le canvas puis le faire disparaitre

public class NarrationGenerator : MonoBehaviour
{
    [SerializeField][TextArea(minLines: 3, maxLines: 10)][Tooltip("Use [Title] to indicate the person that has been killed")] 
    private string _textToFill;

    [SerializeField] private TextMeshProUGUI _textCanvas;
    [SerializeField] private GameObject _panel;

    // Lists 
    [SerializeField] private List<string> _titles; 
    [SerializeField] private List<string> _killerNames; 
    [SerializeField] private List<string> _locations; 
    [SerializeField] private List<string> _weaponNames;

    [SerializeField] private float _timeBeforeCanvasShowUpInSeconds = 2.0f;
    [SerializeField] private float _timeBeforeCanvasDisapearInSeconds = 10.0f;
    private float _timer = 0.0f;

    public int Age;
    
    private void Start()
    {
        // Titles
        string newText; // Declaration 
        string selectedTitle = _titles[Random.Range(0, _titles.Count)];
        newText = _textToFill.Replace("[Title]", selectedTitle); // Assign text
        
        // Killer Names
        newText = newText.Replace("[KillerName]", _killerNames[Random.Range(0, _killerNames.Count)]);
        
        // Locations
        newText = newText.Replace("[Location]", _locations[Random.Range(0, _locations.Count)]);
        
        // Weapon Names
        newText = newText.Replace("[WeaponName]", _weaponNames[Random.Range(0, _weaponNames.Count)]);
        
        _textCanvas.text = newText;

        Age = 19;
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= _timeBeforeCanvasShowUpInSeconds && _timer < _timeBeforeCanvasDisapearInSeconds)
        {
            _panel.SetActive(true);
        }
        else
        {
            _panel.SetActive(false);
        }
    }
}
