using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveGameManager : MonoBehaviour
{
    private const string SaveFileName = "vestige_save.json";

    [SerializeField, Min(5f)] private float autosaveInterval = 45f;

    public static SaveGameManager Instance { get; private set; }

    private SaveGameData cachedData = new SaveGameData();
    private Coroutine autosaveRoutine;
    private bool isApplyingLoad;
    private ArchiveManager archiveManager;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (Instance != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("Save Game Manager");
        managerObject.AddComponent<SaveGameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadFromDisk();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
        CreatureArchiveProgress.OnKnowledgeChanged += HandleKnowledgeChanged;
        ScanProgressTracker.OnScanCountChanged += HandleScanCountChanged;
        EncounterFailureTracker.OnFailureLogged += HandleEncounterFailure;
        HookArchiveManager();
        StartAutosave();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        CreatureArchiveProgress.OnKnowledgeChanged -= HandleKnowledgeChanged;
        ScanProgressTracker.OnScanCountChanged -= HandleScanCountChanged;
        EncounterFailureTracker.OnFailureLogged -= HandleEncounterFailure;
        UnhookArchiveManager();
        StopAutosave();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveNow();
        }
    }

    private void OnApplicationQuit()
    {
        SaveNow();
    }

    public void SaveNow()
    {
        if (isApplyingLoad)
        {
            return;
        }

        SaveGameData data = new SaveGameData
        {
            Version = cachedData.Version,
            ArchiveState = GetArchiveState(),
            ScanCount = ScanProgressTracker.ScanCount,
            CreatureKnowledge = CreatureArchiveProgress.CreateRecords(),
            EncounterFailures = EncounterFailureTracker.CreateRecords(),
            ElusiveRecords = CreatureElusiveTracker.CreateRecords()
        };

        Transform playerTransform = FindPlayerTransform();
        if (playerTransform != null)
        {
            data.HasPlayerTransform = true;
            data.PlayerPosition = playerTransform.position;
            data.PlayerRotation = playerTransform.rotation;
        }
        else
        {
            data.HasPlayerTransform = cachedData.HasPlayerTransform;
            data.PlayerPosition = cachedData.PlayerPosition;
            data.PlayerRotation = cachedData.PlayerRotation;
        }

        cachedData = data;

        string json = JsonUtility.ToJson(data, true);
        string path = GetSavePath();
        try
        {
            File.WriteAllText(path, json);
        }
        catch (IOException exception)
        {
            Debug.LogWarning($"Failed to write save data to {path}. {exception.Message}");
        }
    }

    public void LoadFromDisk()
    {
        isApplyingLoad = true;
        SaveGameData data = new SaveGameData();
        string path = GetSavePath();
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    data = JsonUtility.FromJson<SaveGameData>(json);
                }
            }
            catch (IOException exception)
            {
                Debug.LogWarning($"Failed to load save data from {path}. {exception.Message}");
            }
        }

        cachedData = data ?? new SaveGameData();
        ApplyLoadedData();
        isApplyingLoad = false;
    }

    private void ApplyLoadedData()
    {
        CreatureArchiveProgress.LoadRecords(cachedData.CreatureKnowledge);
        ScanProgressTracker.SetCount(cachedData.ScanCount);
        EncounterFailureTracker.LoadRecords(cachedData.EncounterFailures);
        CreatureElusiveTracker.LoadRecords(cachedData.ElusiveRecords);
        ApplyPlayerTransform();
        ApplyArchiveState();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HookArchiveManager();
        ApplyPlayerTransform();
        ApplyArchiveState();
    }

    private void HandleKnowledgeChanged(int total)
    {
        SaveNow();
    }

    private void HandleScanCountChanged(int count)
    {
        SaveNow();
    }

    private void HandleEncounterFailure(string creatureId, int count)
    {
        SaveNow();
    }

    private void HandleArchiveStateChanged(ArchiveManager.ArchiveState state)
    {
        SaveNow();
    }

    private void StartAutosave()
    {
        if (autosaveRoutine != null)
        {
            StopCoroutine(autosaveRoutine);
        }

        autosaveRoutine = StartCoroutine(AutosaveLoop());
    }

    private void StopAutosave()
    {
        if (autosaveRoutine != null)
        {
            StopCoroutine(autosaveRoutine);
            autosaveRoutine = null;
        }
    }

    private IEnumerator AutosaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autosaveInterval);
            SaveNow();
        }
    }

    private string GetSavePath()
    {
        return Path.Combine(Application.persistentDataPath, SaveFileName);
    }

    private Transform FindPlayerTransform()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }

        return player != null ? player.transform : null;
    }

    private void ApplyPlayerTransform()
    {
        if (!cachedData.HasPlayerTransform)
        {
            return;
        }

        Transform playerTransform = FindPlayerTransform();
        if (playerTransform == null)
        {
            return;
        }

        playerTransform.position = cachedData.PlayerPosition;
        playerTransform.rotation = cachedData.PlayerRotation;
    }

    private ArchiveManager.ArchiveState GetArchiveState()
    {
        if (archiveManager == null)
        {
            archiveManager = FindObjectOfType<ArchiveManager>();
        }

        return archiveManager != null ? archiveManager.CurrentState : cachedData.ArchiveState;
    }

    private void ApplyArchiveState()
    {
        if (archiveManager == null)
        {
            archiveManager = FindObjectOfType<ArchiveManager>();
        }

        if (archiveManager == null)
        {
            return;
        }

        archiveManager.LoadState(cachedData.ArchiveState);
    }

    private void HookArchiveManager()
    {
        ArchiveManager found = FindObjectOfType<ArchiveManager>();
        if (found == archiveManager)
        {
            return;
        }

        UnhookArchiveManager();
        archiveManager = found;
        if (archiveManager != null)
        {
            archiveManager.OnStateChanged += HandleArchiveStateChanged;
        }
    }

    private void UnhookArchiveManager()
    {
        if (archiveManager != null)
        {
            archiveManager.OnStateChanged -= HandleArchiveStateChanged;
        }
    }
}
