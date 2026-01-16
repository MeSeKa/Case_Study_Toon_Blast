using UnityEngine;
using UnityEngine.InputSystem;

public class BoardInputHandler : MonoBehaviour
{
    private Camera _mainCamera;
    private BoardManager _boardManager;
    private bool _isInputActive = true;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _boardManager = GetComponent<BoardManager>();
    }

    private void Update()
    {
        // Ignore input if disabled or if the board is busy (animating/shuffling)
        if (!_isInputActive || _boardManager.IsProcessing) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            DetectClick();
        }
    }

    // Casts a ray from the screen click position to detect which tile was clicked
    private void DetectClick()
    {
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(pointerPos);

        // Raycast against 2D colliders
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null)
            {
                // Delegate the logic to the BoardManager
                _boardManager.OnTileClicked(clickedTile);
            }
        }
    }

    public void SetInputActive(bool isActive) => _isInputActive = isActive;
}