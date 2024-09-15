using System;
using System.Collections.Generic;
using UnityEngine;

public class ChairManager : MonoBehaviour
{
    [SerializeField] private List<Chair> chairs;

    public bool HasEmptyChair()
    {
        return chairs.Exists(chair => chair.Status == Chair.SlotStatus.Empty);
    }

    public Chair GetEmptyChair()
    {
        return chairs.Find(chair => chair.Status == Chair.SlotStatus.Empty);
    }

    public void SetChairStatus(Chair chair, Chair.SlotStatus status)
    {
        chair.ChangeStatus(status);
    }
}