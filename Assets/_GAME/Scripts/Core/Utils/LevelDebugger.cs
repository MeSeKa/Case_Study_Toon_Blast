using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LevelDebugger : MonoBehaviour
{
#if UNITY_EDITOR 

    [Header("Test Edilecek Leveller")]
    public List<LevelData> levels;
    private int _currentIndex = 0;

    private void Update()
    {
        // Klavyeden Space tuþu
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            LoadNextLevel();
        }
    }

    private void OnGUI()
    {
        // Sol üst köþeye bilgilendirme
        GUI.color = Color.red; // Dikkat çeksin diye kýrmýzý
        GUI.Label(new Rect(10, 10, 300, 20), "DEBUG MODE: Press Space to Switch Level");
        GUI.Label(new Rect(10, 30, 300, 20), $"Current: {BoardManager.Instance.currentLevel.name}");
    }

    private void LoadNextLevel()
    {
        if (levels == null || levels.Count == 0) return;
        _currentIndex = (_currentIndex + 1) % levels.Count;
        BoardManager.Instance.SwitchLevel(levels[_currentIndex]);
    }

#endif 
}