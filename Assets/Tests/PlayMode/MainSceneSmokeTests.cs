using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class MainSceneSmokeTests
{
    private const string MainSceneName = "Main";
    private const string PlayerName = "Player";
    private const string GroundName = "Ground";
    private const string CameraName = "Main Camera";

    [UnityTest]
    public IEnumerator MainScene_Loads_AndHasCoreRuntimeScaffolding()
    {
        yield return SceneManager.LoadSceneAsync(MainSceneName, LoadSceneMode.Single);
        yield return null;

        // If the scene doesn't include the runtime bootstrapper, add one so the smoke test
        // still verifies the project's core promise: it can stand itself up without authoring.
        if (Object.FindObjectOfType<ThirdPersonBootstrap>() == null)
        {
            new GameObject("ThirdPersonBootstrap_Test").AddComponent<ThirdPersonBootstrap>();
            yield return null;
        }

        Assert.IsNotNull(GameObject.Find(PlayerName), $"Expected '{PlayerName}' to exist after bootstrap.");
        Assert.IsNotNull(GameObject.Find(GroundName), $"Expected '{GroundName}' to exist after bootstrap.");

        Camera mainCamera = Camera.main;
        Assert.IsNotNull(mainCamera, "Expected Camera.main to exist after bootstrap.");
        Assert.IsTrue(mainCamera.gameObject.name == CameraName || mainCamera.CompareTag("MainCamera"),
            "Expected the main camera to be tagged 'MainCamera' (or named 'Main Camera').");
    }
}


