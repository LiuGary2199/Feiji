using UnityEngine;
using System;

public static class GameEventManager
{
    // 定义加分事件
    public static event Action<int> OnScoreAdded;
    public static event Action<int> OnGoldAdded;

    // 定义生命值减少事件
    public static event Action OnLifeLost;
    // 定义金币时间开始事件
    public static event Action OnGoldTimeStart;
    // 定义金币时间结束事件
    public static event Action OnGoldTimeEnd;
    // 定义游戏结束事件
    public static event Action OnGameOver;
    // 定义游戏重新开始事件
    public static event Action OnGameRestart;
    // 定义游戏继续事件
    public static event Action OnGameContinue;

    // 触发加分事件的方法
    public static void TriggerScoreAdded(int score)
    {
        OnScoreAdded?.Invoke(score);
    }

    public static void TriggerGoldAdded(int gold)
    {
        OnGoldAdded?.Invoke(gold);
    }

    // 触发生命值减少事件的方法
    public static void TriggerLifeLost()
    {
        OnLifeLost?.Invoke();
    }

    // 触发金币时间开始事件
    public static void TriggerGoldTimeStart()
    {
        OnGoldTimeStart?.Invoke();
    }

    // 触发金币时间结束事件
    public static void TriggerGoldTimeEnd()
    {
        OnGoldTimeEnd?.Invoke();
    }

    // 触发游戏结束事件
    public static void TriggerGameOver()
    {
        OnGameOver?.Invoke();
    }

    // 触发游戏重新开始事件
    public static void TriggerGameRestart()
    {
        OnGameRestart?.Invoke();
    }

    // 触发游戏继续事件
    public static void TriggerGameContinue()
    {
        OnGameContinue?.Invoke();
    }
} 