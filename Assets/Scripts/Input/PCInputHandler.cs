using UniRx;
using UnityEngine;

public class PCInputHandler : IInputHandler
{
    private Subject<Vector3> _moveSubject;
    private Subject<Vector2> _lookSubject;
    private Subject<Unit> _shootSubject;
    private Subject<Unit> _menuSubject;
    
    private bool _cursorLocked = true;
    private bool _gameplayEnabled = true;
    
    private float _lastToggleTime = 0f;
    private const float TOGGLE_COOLDOWN = 0.2f;
    private bool _hasInitializedCursor = false;

    public void Initialize(Subject<Vector3> move, Subject<Vector2> look, Subject<Unit> shoot, Subject<Unit> menu)
    {
        _moveSubject = move;
        _lookSubject = look;
        _shootSubject = shoot;
        _menuSubject = menu;
        
        ForceEnterGameplayMode();
    }
    
    private void ForceEnterGameplayMode()
    {
        _gameplayEnabled = true;
        _cursorLocked = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _hasInitializedCursor = true;
    }

    public void Update()
    {
        if (!_hasInitializedCursor && Input.GetMouseButtonDown(0))
        {
            ForceEnterGameplayMode();
            _hasInitializedCursor = true;
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.time - _lastToggleTime < TOGGLE_COOLDOWN)
                return;
            
            _lastToggleTime = Time.time;
            
            if (_gameplayEnabled)
                EnterUIMode();
            else
                EnterGameplayMode();
        }
        
        if (!_gameplayEnabled)
        {
            _moveSubject.OnNext(Vector3.zero);
            _lookSubject.OnNext(Vector2.zero);
            return;
        }
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        _moveSubject.OnNext(moveDirection);
        
        if (_cursorLocked)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            _lookSubject.OnNext(new Vector2(mouseX, mouseY));
        }

        if (Input.GetMouseButtonDown(0) && _cursorLocked)
        {
            _shootSubject.OnNext(Unit.Default);
        }
    }
    
    public void EnterGameplayMode()
    {
        if (_gameplayEnabled && _cursorLocked)
            return;
        
        _gameplayEnabled = true;
        _cursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        _moveSubject.OnNext(Vector3.zero);
        
        Debug.Log("Entered Gameplay Mode - Controls ENABLED");
    }
    
    public void EnterUIMode()
    {
        if (!_gameplayEnabled && !_cursorLocked)
            return;
        
        _gameplayEnabled = false;
        _cursorLocked = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        _moveSubject.OnNext(Vector3.zero);
        _lookSubject.OnNext(Vector2.zero);
        
        Debug.Log("Entered UI Mode - Controls DISABLED");
    }

    public void Dispose()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}