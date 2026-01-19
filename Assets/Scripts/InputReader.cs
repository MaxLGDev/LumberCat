using UnityEngine;

public class InputReader : MonoBehaviour
{
    public System.Action<KeyCode> OnKeyPressed;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            OnKeyPressed?.Invoke(KeyCode.Q);

        if(Input.GetKeyDown(KeyCode.D))
            OnKeyPressed?.Invoke(KeyCode.D);

        if (Input.GetKeyDown(KeyCode.Space))
            OnKeyPressed?.Invoke(KeyCode.Space);

        if(Input.GetKeyDown(KeyCode.Return))
            OnKeyPressed?.Invoke(KeyCode.Return);
    }
}
