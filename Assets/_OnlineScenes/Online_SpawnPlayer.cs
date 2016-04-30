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

        //StartCoroutine(NamingRoutine());

        BaseGameManager.startGame();
        //Destroy(gameObject);
    }

    IEnumerator NamingRoutine()
    {
        var p1Go = GameObject.Find("Online_HumanPlayer1(Clone)");
        while (p1Go == null)
            yield return null;

        var p2Go = GameObject.Find("Online_HumanPlayer2(Clone)");
        while (p2Go == null)
            yield return null;

        var p1 = p1Go.GetComponent<HumanPlayer>();
        var p2 = p2Go.GetComponent<HumanPlayer>();

        int otherId = TNManager.players[0].id;
        if (TNManager.playerID < otherId)
        {
            p1.playerName = TNManager.playerName;
            p2.playerName = TNManager.players[0].name;
        }
        else
        {
            p2.playerName = TNManager.playerName;
            p1.playerName = TNManager.players[0].name;
        }
    }
}
