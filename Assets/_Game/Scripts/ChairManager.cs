using System;
using System.Collections.Generic;
using UnityEngine;

public class ChairManager : MonoBehaviour
{
    [SerializeField] List<Chair> listChair = new List<Chair>();
    [SerializeField] HashSet<Chair> hsChair = new HashSet<Chair>();

    private void Awake()
    {
        ConvertSlotToHashSet();
    }

    private void ConvertSlotToHashSet()
    {
        hsChair.Clear();
        foreach (var slot in listChair)
        {
            hsChair.Add(slot);
        }
    }

    public void SetChairStatus(Chair slot, Chair.SlotStatus status)
    {
        slot.ChangeStatus(status);
    }
    public Chair GetEmptyChair()
    {
        foreach (var slot in hsChair)
        {
            if (slot.Status == Chair.SlotStatus.Empty)
            {
                return slot;
            }
        }
        Debug.Log("No empty slot");
        return null;
    }

}
