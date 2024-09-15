using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    [Header("[References]")]
    [SerializeField] private ChairManager chairManager;
    /// <summary>
    /// Settings
    /// </summary>
    [Header("[Settings]")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private int maxCharacter = 5;
    [SerializeField] private int currentCharacter = 0;
    [SerializeField] private Character characterPrefab;
    [SerializeField] private Transform spawnPoint;
    [Header("[List Serialized]")]
    [SerializeField] private List<Character> listCurrentCharacter = new List<Character>();
    public List<Character> ListCurrentCharacter => listCurrentCharacter;
    /// <summary>
    /// Bool to check if the character is still spawning
    /// </summary>
    [SerializeField] private bool isSpawning = true;
    private void Start()
    {
        StartCoroutine(SpawnCharacterLoop());
    }

    [ContextMenu("Test Spawn Character")]
    private void TestSpawnCharacter()
    {
        StartCoroutine(SpawnCharacter());
    }


    private IEnumerator SpawnCharacterLoop()
    {
        while (isSpawning)
        {
            yield return StartCoroutine(SpawnCharacter());
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator SpawnCharacter()
    {
        // Get pos from SlotManager
        Chair emptyChair = chairManager.GetEmptyChair();
        if (emptyChair == null)
        {
            Debug.Log("No empty slot");
            yield break; // No empty slots available
        }


        // Spawn Character
        Character character = Instantiate(characterPrefab, spawnPoint.position, Quaternion.identity);

        // Set Character Info 
        character.SetInfo(emptyChair, this, spawnPoint.position);

        // Change Slot Status
        chairManager.SetChairStatus(emptyChair, Chair.SlotStatus.Occupied);

        // Move to Slot
        character.Go();

        listCurrentCharacter.Add(character);
    }

    public void RemoveCharacter(Character character)
    {
        listCurrentCharacter.Remove(character);
    }
}