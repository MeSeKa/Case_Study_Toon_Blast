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
        _boardManager = GetComponent<BoardManager>(); // Ayný obje üzerindelerse
    }

    private void Update()
    {
        // Board meþgulse veya input kapalýysa iþlem yapma
        if (!_isInputActive || _boardManager.IsProcessing) return;

        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            DetectClick();
        }
    }

    private void DetectClick()
    {
        Vector2 pointerPos = Pointer.current.position.ReadValue();
        Vector2 worldPos = _mainCamera.ScreenToWorldPoint(pointerPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null)
        {
            Tile clickedTile = hit.collider.GetComponent<Tile>();
            if (clickedTile != null)
            {
                // Input sadece "Týklandý" der, ne olacaðýna Board karar verir.
                _boardManager.OnTileClicked(clickedTile);
            }
        }
    }

    public void SetInputActive(bool isActive) => _isInputActive = isActive;
}