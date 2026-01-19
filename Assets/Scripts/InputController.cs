using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private InputReader reader;

    private bool inputEnabled;
    private System.Action<KeyCode> inputCallback;

    private void Awake()
    {
        if (reader != null)
            reader.OnKeyPressed += HandleKey;
    }

    private void OnDestroy()
    {
        if (reader != null)
            reader.OnKeyPressed -= HandleKey;
    }

    public void Bind(System.Action<KeyCode> callback)
    {
        inputCallback = callback;
    }

    public void Unbind()
    {
        inputCallback = null;
    }

    public void EnableInput(bool value)
    {
        inputEnabled = value;
    }

    private void HandleKey(KeyCode key)
    {
        if (!inputEnabled)
            return;

        inputCallback?.Invoke(key);
    }
}
