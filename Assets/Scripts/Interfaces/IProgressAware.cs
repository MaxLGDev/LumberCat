using UnityEngine;

public interface IProgressAware
{
    void OnProgressChanged(int current, int required);
}
