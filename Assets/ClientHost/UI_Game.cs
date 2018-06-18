using BeardedManStudios.Forge.Networking;
using BeardedManStudios.Forge.Networking.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : MonoBehaviour
{
    [SerializeField]
    private GameLogic _GameLogic;

    [SerializeField]
    private RectTransform _WaitingToStartPanel;

    [SerializeField]
    private RectTransform _GameOverPanel;

    [SerializeField]
    private RectTransform _RankingBoard;

    [SerializeField]
    private RectTransform _RankingEntry;

    [SerializeField]
    private Text _Time;

    [SerializeField]
    private NetworkingPlayer _Server;

    // Use this for initialization
    void Start ()
    {
        _GameLogic.OnStartGame += _GameLogic_OnStartGame;
        _GameLogic.OnGameOver += _GameLogic_OnGameOver;
        _GameOverPanel.gameObject.SetActive(false);
    }

    private void _GameLogic_OnGameOver()
    {
        _GameOverPanel.gameObject.SetActive(true);

        List<PlayerCharacter> players = new List<PlayerCharacter>(_GameLogic.Players);
        players.Sort((x, y) => x.Kills.CompareTo(y.Kills));

        for(int i = 0;i<players.Count;i++)
        {
            var player = players[i];
            var entry = _RankingEntry;
            if(i != players.Count - 1)
            {
                entry = Instantiate(_RankingEntry, _RankingBoard);
            }

            entry.Find("Name").GetComponent<Text>().text = player.name;
            entry.Find("Kills").GetComponent<Text>().text = player.Kills.ToString();
        }
    }

    private void _GameLogic_OnStartGame()
    {
        _WaitingToStartPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Networker != null)
        {
            string players_lat = string.Empty;
            NetworkManager.Instance.Networker.IteratePlayers(player =>
            {
                players_lat += string.Format("{0} : {1}\n\r", player.NetworkId, player.RoundTripLatency);
            });

            _Time.text = string.Format("{0} {1}\r\n{2}", NetworkManager.Instance.Networker.Time.Timestep.ToString(), _GameLogic.RoundTripLatency.ToString("F1"), players_lat);
        }
    }
}
