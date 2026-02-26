using UnityEngine;

// 毎フレームまたは一定間隔で更新処理を行うオブジェクト用インターフェース
public interface ITickable
{
    // deltaTime: 前回更新からの経過時間
    void Tick(float deltaTime);
}