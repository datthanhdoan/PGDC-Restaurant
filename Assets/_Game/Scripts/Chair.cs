using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
public interface IChair
{
    public void ChangeStatus(Chair.SlotStatus status);
}


public class Chair : MonoBehaviour, IChair
{
    public enum SlotStatus
    {
        Empty,
        Occupied,
    }
    public SlotStatus Status { get; private set; } = SlotStatus.Empty;
    [field: SerializeField] public Transform Trans { get; private set; }
    [SerializeField] private SpriteRenderer _spriteRenderer;


    public void ChangeStatus(SlotStatus status)
    {
        ChangeSpriteColor(status == SlotStatus.Empty ? Color.white : Color.red);
        this.Status = status;
    }

    public void ChangeSpriteColor(Color color)
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = color;
        }
        else
        {
            Debug.LogError("SpriteRenderer is null");
        }
    }
}