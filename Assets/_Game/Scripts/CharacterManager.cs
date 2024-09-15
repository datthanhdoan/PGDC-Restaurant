using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterManager : MonoBehaviour
{
    /// <summary>
    /// Reference to the ChairManager component.
    /// </summary>
    [Header("[References]")]
    [SerializeField] private ChairManager chairManager;

    /// <summary>
    /// The interval at which characters are spawned.
    /// </summary>
    [Header("[Settings]")]
    [SerializeField] private float spawnInterval = 5f;
    [SerializeField] private Transform spawnPoint;

    /// <summary>
    /// The prefab to be used for creating characters.
    /// </summary>
    [Header("[Pool Settings]")]
    [SerializeField] private Character characterPrefab;
    [SerializeField] private int maxCharacters = 5;
    [SerializeField] private int initialPoolSize = 10;
    /// <summary>
    /// Flag to control the spawning of characters.
    /// </summary>
    [Header("[Debug]")]
    [SerializeField] private bool isSpawning = true;

    private List<Character> activeCharacters = new List<Character>();
    public IReadOnlyList<Character> ActiveCharacters => activeCharacters;

    private GenericObjectPooler<Character> characterPool;

    private void Awake()
    {
        InitializePool();
    }

    private void InitializePool()
    {
        characterPool = new GenericObjectPooler<Character>(
            characterPrefab,
            initialPoolSize,
            transform,
            OnCreateCharacter,
            OnGetCharacter,
            OnReleaseCharacter
        );
    }

    private void OnCreateCharacter(Character character)
    {
        // Initialization logic for newly created characters
    }

    private void OnGetCharacter(Character character)
    {
        character.gameObject.SetActive(true);
    }

    private void OnReleaseCharacter(Character character)
    {
        character.gameObject.SetActive(false);
        character.ResetForPooling();
    }

    private void Start()
    {
        StartCoroutine(SpawnCharacterLoop());
    }

    private IEnumerator SpawnCharacterLoop()
    {
        while (isSpawning)
        {
            if (CanSpawnCharacter())
            {
                yield return SpawnCharacter();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private bool CanSpawnCharacter()
    {
        return activeCharacters.Count < maxCharacters && chairManager.HasEmptyChair();
    }

    private IEnumerator SpawnCharacter()
    {
        if (chairManager == null)
        {
            Debug.LogError("ChairManager is null!");
            yield break;
        }

        Chair emptyChair = chairManager.GetEmptyChair();
        if (emptyChair == null)
        {
            Debug.LogWarning("No empty chair available for spawning.");
            yield break;
        }

        Character character = characterPool.Get();

        character.transform.position = spawnPoint.position;
        character.Initialize(emptyChair, spawnPoint.position, this);

        chairManager.SetChairStatus(emptyChair, Chair.SlotStatus.Occupied);

        AddCharacter(character);
        character.StartJourney();

        yield return null;
    }

    public void RemoveCharacter(Character character)
    {
        character.CurrentChair.ChangeStatus(Chair.SlotStatus.Empty);
        activeCharacters.Remove(character);
        characterPool.Release(character);
    }

    public void AddCharacter(Character character)
    {
        activeCharacters.Add(character);
    }

    [ContextMenu("Test Spawn Character")]
    private void TestSpawnCharacter()
    {
        StartCoroutine(SpawnCharacter());
    }
}