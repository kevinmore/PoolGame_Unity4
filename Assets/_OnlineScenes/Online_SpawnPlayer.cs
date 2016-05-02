using UnityEngine;
using TNet;
using System.Collections;
using PoolKit;

/// <summary>
/// Instantiate the specified prefab at the game object's position.
/// </summary>

public class Online_SpawnPlayer : MonoBehaviour
{
    /// <summary>
    /// Prefab to instantiate.
    /// </summary>

    public GameObject player1;
    public GameObject player2;

    /// <summary>
    /// Whether the instantiated object will remain in the game when the player that created it leaves.
    /// Set this to 'false' for the player's avatar.
    /// </summary>

    bool persistent = false;

    IEnumerator Start()
    {
        while (TNManager.isJoiningChannel) yield return null;

        // get the other player
        int otherId = TNManager.players[0].id;
        TNManager.Create(TNManager.playerID < otherId ? player1 : player2, persistent);

        BaseGameManager.startGame();
        Destroy(gameObject);
    }
}
