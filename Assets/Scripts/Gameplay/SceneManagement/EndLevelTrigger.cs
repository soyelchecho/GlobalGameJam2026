using System;
using UnityEngine;
using Gameplay.Player;
using Gameplay.SceneManagement;


/// <summary>
/// Trigger collider that ends the level when the player enters it.
/// Place this on a GameObject with a Collider2D set to "Is Trigger".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EndLevelTrigger : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool loadNextLevel = true;
    [SerializeField] private string specificSceneName;
    [SerializeField] private int specificSceneIndex = -1;

    [Header("Level Progress")]
    [Tooltip("Index of the level to unlock when this trigger fires. Set to the NEXT level index (e.g. Level 1 completes → set 1 to unlock Level 2, Level 2 → set 2 to unlock Level 3). -1 = nothing.")]
    [SerializeField] private int levelIndexToUnlock = -1;

    [Header("Optional Effects")]
    [SerializeField] private float delayBeforeTransition = 0f;
    [SerializeField] private bool freezePlayerOnTrigger = true;

    public event Action OnLevelEnd;

    private bool hasTriggered = false;

    private void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (!col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning("EndLevelTrigger: Collider was not set to trigger. Fixed automatically.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.CompareTag(playerTag))
        {
            hasTriggered = true;
            TriggerLevelEnd(other.gameObject);
        }
    }

    private void TriggerLevelEnd(GameObject player)
    {
        if (levelIndexToUnlock >= 0)
            LevelProgressManager.UnlockLevel(levelIndexToUnlock);

        OnLevelEnd?.Invoke();

        if (freezePlayerOnTrigger)
        {
            FreezePlayer(player);
        }

        if (delayBeforeTransition > 0)
        {
            Invoke(nameof(ExecuteTransition), delayBeforeTransition);
        }
        else
        {
            ExecuteTransition();
        }
    }

    private void FreezePlayer(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false;
        }
    }

    private void ExecuteTransition()
    {
        if (GameSceneManager.Instance == null)
        {
            Debug.LogError("GameSceneManager not found! Make sure it exists in the scene.");
            return;
        }

        if (!string.IsNullOrEmpty(specificSceneName))
        {
            GameSceneManager.Instance.LoadScene(specificSceneName);
        }
        else if (specificSceneIndex >= 0)
        {
            GameSceneManager.Instance.LoadScene(specificSceneIndex);
        }
        else if (loadNextLevel)
        {
            GameSceneManager.Instance.LoadNextLevel();
        }
    }

    private void OnDrawGizmos()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
        }
    }
}
