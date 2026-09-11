using System.Collections.Generic;
using UnityEngine;

public class GimmickManager : MonoBehaviour
{
    private readonly List<GimmickController> _gimmicks = new();

    public void Initialize()
    {
        GimmickController[] gimmicks = FindObjectsByType<GimmickController>(FindObjectsSortMode.None);

        _gimmicks.Clear();

        foreach (GimmickController gimmick in gimmicks)
        {
            Register(gimmick);
        }
    }

    public void Register(GimmickController gimmick)
    {
        if (_gimmicks.Contains(gimmick)) return;

        _gimmicks.Add(gimmick);

        gimmick.Initialize();
    }

    public void UnRegister(GimmickController gimmick)
    {
        _gimmicks.Remove(gimmick);
    }

    public void InvokeActiveEvent()
    {
        foreach (var gimmick in _gimmicks)
        {
            gimmick.ActiveEvent();
        }
    }
}