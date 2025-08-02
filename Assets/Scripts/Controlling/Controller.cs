using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using Valve.VR;
using UnityEngine.InputSystem.XR;

public class Controller : MonoBehaviour
{
    public bool connectToServer;
    [SerializeField] private bool debuggingMode;
    [SerializeField] private bool useFixationScenes;
    [SerializeField] private bool useCyberSicknessScene;
    [SerializeField] private bool shuffleSetup;
    [SerializeField] private int repetitions;
    [SerializeField] private float maximumSceneDuration;
    public int seed = -1;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private List<string> scenes = new();
    [SerializeField] private Transform visionCatcherPrefabNone;
    [SerializeField] private List<Transform> visionCatcherPrefabs = new List<Transform>();
    [SerializeField] private GameObject QuestionsPrefab;
    [SerializeField] private GameObject SupportCrossPrefab;
    [SerializeField] private GameObject SupportCheckPrefab;
    [SerializeField] private GameObject EyeTrackingTransformPrefab;
    private GameObject currentQuestionObject;
    private GameObject currentSupportCross;
    private GameObject currentSupportCheck;
    private Transform currentEyeTrackingTransform;
    [HideInInspector] public DataCollector dataCollector;
    [HideInInspector] public FadeController fadeController;
    private PythonCommunicator pythonCommunicator;
    [HideInInspector] public Utilities utilities;
    private Environments currentEnvScript;
    [HideInInspector]
    public EyeTrackingRaycast currentEyeTrackingScript;
    private Transform searchTransform;
    private List<Combination> combinations = new();
    private List<string> usedLocations = new();
    int currentIndex = -1;
    string startScene = "StartupScene";
    string csQuestionnaireScene = "ArtificialEndScene";
    string endScene = "EndScene";
    string fixationScene = "FixationScene";
    Transform currentVisionCatcherPrefab;
    VisionCatcher currentVisionCatcherScript;
    private float supportObjectWaitTime = 1.5f;
    private bool waiting = false;
    [HideInInspector]
    public Transform environmentPosition;
    bool environmentPositionSet = false;
    bool objectFoundSuccessfully = false;
    int attempts = 0;
    //bool isCSQuestionnaireScene = false;

    private Coroutine sceneTimerCoroutine;
    private Coroutine correctnessCoroutine;

    public SteamVR_Action_Boolean triggerAction;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        SetupController();
        SetupStudy();
        if (connectToServer)
        {
            // data can only be collected if connected to server
            SetupDataCollection();
            pythonCommunicator.SetupServerConnection();
            pythonCommunicator.SendMessageToPython("start");
        }
    }

    private void SetupStudy()
    {
        Random.InitState(seed);
        List<Combination> tmpCombinations = new();
        visionCatcherPrefabs.Insert(0, visionCatcherPrefabNone);
        Application.targetFrameRate = 90;
        for (int i = 0; i < scenes.Count; i++)
        {
            if (!scenes[i].Contains("TestDemo"))
            {
                for (int j = 0; j < visionCatcherPrefabs.Count; j++)
                {
                    tmpCombinations.Add(new Combination(scenes[i], visionCatcherPrefabs[j]));
                }
                List<Combination> tmp = tmpCombinations;
                for (int k = 0; k < repetitions; k++)
                {
                    tmpCombinations.AddRange(tmp);
                }
                if (shuffleSetup)
                {
                    // need different shuffles (seeds)
                    tmpCombinations = utilities.ShuffleList(tmpCombinations, seed + i);
                }
                combinations.AddRange(tmpCombinations);
                tmpCombinations = new();
            }
            else
            {
                // no vision catcher in demo scene
                combinations.Add(new Combination(scenes[i], visionCatcherPrefabNone));
            }

        }
        Debug.Log("Total of " + combinations.Count + " experiments...");
        if (useFixationScenes)
        {
            for (int i = 0; i < combinations.Count; i += 2)
            {
                combinations.Insert(i, new Combination(fixationScene, visionCatcherPrefabNone));
            }
        }
        if (useCyberSicknessScene)
        {
            for (int j = 0; j < visionCatcherPrefabs.Count; j++)
            {
                combinations.Add(new Combination(csQuestionnaireScene, visionCatcherPrefabs[j]));
            }
        }
    }

    private void SetupController()
    {
        dataCollector = GetComponent<DataCollector>();
        fadeController = GetComponent<FadeController>();
        utilities = GetComponent<Utilities>();
        pythonCommunicator = GetComponent<PythonCommunicator>();
        currentEyeTrackingScript = GetComponent<EyeTrackingRaycast>();
        currentEyeTrackingScript.mainCamera = mainCamera.transform;
        pythonCommunicator.SetController(this);
        if (seed < 0)
        {
            seed = Random.Range(0, 30000);
        }
    }

    private void Update()
    {
        if (debuggingMode && !waiting && Input.GetKeyUp(KeyCode.L))
        {
            LoadNextScene();
            return;
        }
        // object seen
        if (!waiting && (Input.GetKeyUp(KeyCode.P)
            ||
            triggerAction.GetStateDown(SteamVR_Input_Sources.RightHand))
            )
        {
            if (SceneManager.GetActiveScene().name == startScene)
            {
                if (connectToServer)
                {
                    return;
                }
                LoadNextScene();
                return;
            }
            correctnessCoroutine = StartCoroutine(CheckCorrectness());
        }

        if (Input.GetKeyUp(KeyCode.O))
        {
            InitialSetEnvironmentPosAndRot();
        }
    }

    private IEnumerator CheckCorrectness()
    {
        bool correct = currentEyeTrackingScript.lookingAtSearchObject;
        waiting = true;
        Transform gameObjectTransform;
        if (correct)
        {
            if (sceneTimerCoroutine != null)
            {
                StopCoroutine(sceneTimerCoroutine);
            }
            if (connectToServer)
            {
                dataCollector.SetDataCollectingState(false);
            }
            gameObjectTransform = currentSupportCheck.transform;
            gameObjectTransform.transform.position = mainCamera.transform.position + ((searchTransform.position - mainCamera.transform.position) / 1.1f);
        }
        else
        {
            attempts += 1;
            gameObjectTransform = currentSupportCross.transform;
            gameObjectTransform.transform.position = mainCamera.transform.position + (currentEyeTrackingScript.gazeRay.direction * currentEnvScript.maxSearchDistance);
        }
        gameObjectTransform.transform.LookAt(mainCamera.transform);
        float distance = Vector3.Distance(gameObjectTransform.position, mainCamera.transform.position);
        float scaleFactor = distance / mainCamera.orthographicSize;
        float fixedScale = 1f;
        gameObjectTransform.localScale = Vector3.one * scaleFactor * fixedScale;
        gameObjectTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        gameObjectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObjectTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(supportObjectWaitTime);
        gameObjectTransform.gameObject.SetActive(false);
        if (correct)
        {
            objectFoundSuccessfully = true;
            SearchObjectFound();
        }
        else
        {
            waiting = false;
        }
    }

    private void SearchObjectFound()
    {
        // message python
        currentVisionCatcherScript.StopVisionCatcher();
        DisplayQuestions();
        if (connectToServer)
        {
            NotifyServer();
        }
        else
        {
            ResumeScene();
        }
    }

    private void DisplayQuestions()
    {
        Transform location = currentEnvScript.GetQuestionTransform();
        currentQuestionObject = Instantiate(QuestionsPrefab, currentEnvScript.environmentTransform);
        currentQuestionObject.transform.position = location.position;
        currentQuestionObject.transform.rotation = location.rotation;
    }

    private void NotifyServer()
    {
        pythonCommunicator.SendMessageToPython("paused");
    }

    public void ResumeScene()
    {
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (currentVisionCatcherScript)
        {
            currentVisionCatcherScript.StopVisionCatcher();
            currentEyeTrackingScript.eyeTrackingActive = false;
            currentEyeTrackingTransform = null;
        }
        if (currentEyeTrackingTransform)
        {
            currentEyeTrackingTransform = null;
        }
        currentIndex++;
        if (SceneManager.GetActiveScene().name == endScene) return;
        if (currentIndex > combinations.Count - 1)
        {
            Debug.Log("Done.");
            LoadScene(endScene);
            return;
        }
        Debug.Log(currentIndex.ToString() + ": " + combinations[currentIndex].ToString());
        currentVisionCatcherPrefab = combinations[currentIndex].visionCatcherPrefab;
        LoadScene(currentIndex);
    }

    public void LoadScene(string name)
    {
        ClearEnvironment();
        StartCoroutine(LoadSceneWithFade(name));
    }

    public void LoadScene(int index)
    {
        ClearEnvironment();
        if (index >= 0 && index < combinations.Count)
        {
            StartCoroutine(LoadSceneWithFade(index));
        }
        else
        {
            Debug.LogError("index error");
        }
    }

    private IEnumerator LoadSceneWithFade(string name)
    {
        fadeController.FadeOut();
        yield return new WaitForSeconds(fadeController.fadeDuration);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(name);
    }

    private IEnumerator LoadSceneWithFade(int index)
    {
        fadeController.FadeOut();
        yield return new WaitForSeconds(fadeController.fadeDuration);
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(combinations[index].sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        // fixationcross between scenes to adjust alignment
        if (scene.name == fixationScene)
        {
            FindSceneEnvironment();
            SetupEnvironment();
            fadeController.FadeInNoDelay();
            return;
        }
        // last scene reached
        if (scene.name == endScene)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            return;
        }
        if (scene.name == csQuestionnaireScene)
        {
            FindSceneEnvironment();
            SetupEnvironment();
            SetupVisionCatcher();
            pythonCommunicator.SendMessageToPython("endq_" + dataCollector.currentStudyID + "_" + currentVisionCatcherScript.visionCatcherName);
            fadeController.FadeIn();
            return;
        }
        FindSceneEnvironment();
        SetupEnvironment();
        SetupVisionCatcher();
        if (connectToServer)
        {
            UpdateDataCollector();
        }
        sceneTimerCoroutine = StartCoroutine(SceneTimer());
        fadeController.FadeIn();
        attempts = 0;
        waiting = false;
    }

    IEnumerator SceneTimer()
    {
        yield return new WaitForSeconds(maximumSceneDuration);
        if (correctnessCoroutine != null)
        {
            StopCoroutine(correctnessCoroutine);
        }
        {
            dataCollector.SetDataCollectingState(false);
        }
        waiting = true;
        objectFoundSuccessfully = false;
        Transform gameObjectTransform;
        gameObjectTransform = currentSupportCross.transform;
        gameObjectTransform.transform.position = mainCamera.transform.position + (currentEyeTrackingScript.gazeRay.direction * currentEnvScript.maxSearchDistance);
        gameObjectTransform.transform.LookAt(mainCamera.transform);
        float distance = Vector3.Distance(gameObjectTransform.position, mainCamera.transform.position);
        float scaleFactor = distance / mainCamera.orthographicSize;
        float fixedScale = 1f;
        gameObjectTransform.localScale = Vector3.one * scaleFactor * fixedScale;
        gameObjectTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        gameObjectTransform.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.2f);
        gameObjectTransform.gameObject.SetActive(true);
        yield return new WaitForSeconds(supportObjectWaitTime);
        gameObjectTransform.gameObject.SetActive(false);
        SearchObjectFound();
    }

    private void UpdateDataCollector()
    {
        dataCollector.eyeTrackingTransform = currentEyeTrackingTransform;
        SetEnvAndCatcherData();
        dataCollector.SetDataCollectingState(true);
    }

    private void SetupVisionCatcher()
    {
        var newVisionCatcher = Instantiate(currentVisionCatcherPrefab);
        currentVisionCatcherScript = newVisionCatcher.GetComponent<VisionCatcher>();
        currentVisionCatcherScript.SetupVisionCatcher(searchTransform, mainCamera, this);
        currentVisionCatcherScript.StartVisionCatcher();
    }

    public void FindSceneEnvironment()
    {
        currentEnvScript = FindObjectOfType<Environments>();
        if (currentEnvScript == null)
        {
            Debug.LogError("current environment script not found");
        }
    }

    private void SetupEnvironment()
    {
        currentSupportCross = Instantiate(SupportCrossPrefab);
        currentSupportCheck = Instantiate(SupportCheckPrefab);
        currentSupportCheck.SetActive(false);
        currentSupportCross.SetActive(false);
        currentEyeTrackingTransform = Instantiate(EyeTrackingTransformPrefab).transform;
        currentEyeTrackingScript.eyetrackingTransform = currentEyeTrackingTransform;
        currentEyeTrackingScript.eyeTrackingActive = true;
        currentEnvScript.controller = this;
        currentEnvScript.SetupEnvironment();
        SetEnvironmentPosAndRot();
        searchTransform = currentEnvScript.GetSearchObject();
    }

    private void InitialSetEnvironmentPosAndRot()
    {
        environmentPosition = transform.GetChild(0);
        currentEnvScript.environmentTransform.position = mainCamera.transform.position + new Vector3(0, -2, 0);
        Vector3 targetDirection = new Vector3(mainCamera.transform.forward.x, 0, mainCamera.transform.forward.z);
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        currentEnvScript.environmentTransform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        environmentPosition.rotation = currentEnvScript.environmentTransform.rotation;
        environmentPosition.position = currentEnvScript.environmentTransform.position;
        environmentPositionSet = true;
    }

    private void SetEnvironmentPosAndRot()
    {
        if (!environmentPositionSet)
        {
            InitialSetEnvironmentPosAndRot();
        }
        currentEnvScript.environmentTransform.rotation = environmentPosition.rotation;
        currentEnvScript.environmentTransform.position = environmentPosition.position;
    }

    private void ClearEnvironment()
    {
        currentEnvScript = null;
        searchTransform = null;
    }

    public Transform GetCamera()
    {
        return mainCamera.transform;
    }

    private void SetupDataCollection()
    {
        dataCollector.controller = this;
        Debug.Log(seed);
        dataCollector.Setup("Setup", "Data", seed);
    }

    public void SetEnvAndCatcherData()
    {
        dataCollector.SetEnvAndCatcher(combinations[currentIndex].sceneName, combinations[currentIndex].visionCatcherPrefab.GetComponent<VisionCatcher>().visionCatcherName, searchTransform);
    }

    public void SetSetupData(string part1, string part2)
    {
        dataCollector.SetSetupData(part1, part2, objectFoundSuccessfully, attempts);
    }

    public void SearchLocationUsed(string location)
    {
        usedLocations.Add(location);
    }

    public List<string> GetAllUsedSearchLocations()
    {
        return usedLocations;
    }
}

class Combination
{
    public string sceneName;
    public Transform visionCatcherPrefab;

    public Combination(string sceneName, Transform visionCatcherPrefab)
    {
        this.sceneName = sceneName;
        this.visionCatcherPrefab = visionCatcherPrefab;
    }

    public override string ToString()
    {
        return sceneName + " / " + visionCatcherPrefab.GetComponent<VisionCatcher>().visionCatcherName;
    }
}
