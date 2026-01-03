using Cinemachine;
using UnityEngine;

public class ThirdPersonBootstrap : MonoBehaviour
{
    [SerializeField] private string playerName = "Player";
    [SerializeField] private Vector3 playerSpawn = new Vector3(0f, 1f, 0f);
    [SerializeField] private Vector3 cameraOffset = new Vector3(0f, 3f, -6f);

    private void Awake()
    {
        EnsureGround();
        GameObject player = EnsurePlayer();
        EnsureCamera(player.transform);
    }

    private void EnsureGround()
    {
        if (GameObject.Find("Ground") != null)
        {
            return;
        }

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(5f, 1f, 5f);
    }

    private GameObject EnsurePlayer()
    {
        GameObject player = GameObject.Find(playerName);
        if (player == null)
        {
            player = new GameObject(playerName);
            player.transform.position = playerSpawn;

            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.35f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            player.AddComponent<ThirdPersonController>();

            GameObject visuals = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            visuals.name = "Visual";
            visuals.transform.SetParent(player.transform);
            visuals.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            visuals.transform.localRotation = Quaternion.identity;
            visuals.transform.localScale = new Vector3(1f, 1f, 1f);

            Collider capsuleCollider = visuals.GetComponent<Collider>();
            if (capsuleCollider != null)
            {
                Destroy(capsuleCollider);
            }
        }

        return player;
    }

    private void EnsureCamera(Transform followTarget)
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        if (mainCamera.GetComponent<CinemachineBrain>() == null)
        {
            mainCamera.gameObject.AddComponent<CinemachineBrain>();
        }

        GameObject vcamObject = GameObject.Find("ThirdPersonCamera");
        CinemachineVirtualCamera vcam;
        if (vcamObject == null)
        {
            vcamObject = new GameObject("ThirdPersonCamera");
            vcam = vcamObject.AddComponent<CinemachineVirtualCamera>();
        }
        else
        {
            vcam = vcamObject.GetComponent<CinemachineVirtualCamera>();
            if (vcam == null)
            {
                vcam = vcamObject.AddComponent<CinemachineVirtualCamera>();
            }
        }

        vcam.Follow = followTarget;
        vcam.LookAt = followTarget;

        CinemachineTransposer transposer = vcam.GetCinemachineComponent<CinemachineTransposer>();
        if (transposer == null)
        {
            transposer = vcam.AddCinemachineComponent<CinemachineTransposer>();
        }

        transposer.m_FollowOffset = cameraOffset;
        transposer.m_XDamping = 0.6f;
        transposer.m_YDamping = 0.8f;
        transposer.m_ZDamping = 0.9f;

        CinemachineComposer composer = vcam.GetCinemachineComponent<CinemachineComposer>();
        if (composer == null)
        {
            composer = vcam.AddCinemachineComponent<CinemachineComposer>();
        }

        composer.m_ScreenX = 0.5f;
        composer.m_ScreenY = 0.55f;
        composer.m_DeadZoneWidth = 0f;
        composer.m_DeadZoneHeight = 0f;
        composer.m_SoftZoneWidth = 0.8f;
        composer.m_SoftZoneHeight = 0.8f;
        composer.m_HorizontalDamping = 0.6f;
        composer.m_VerticalDamping = 0.6f;
    }
}
