using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using TMPro;

public class server : MonoBehaviour
{
    private WebSocket ws;

    public GameObject player;
    public GameObject playerPrefab;
    public List<GameObject> players;

    private List<string> otherIds;
    private string myId = "";
    private bool haveIds;
    delegate void ChangeThings();
    List<ChangeThings> changeThingsList = new List<ChangeThings>();
    List<Vector3> otherPlayerPositions;
    List<int> otherPlayerHealth;

    private TextMeshProUGUI info;
    public static string infoText;

    // Updating your or other player position
    struct Position {
        public string type;
        public string who;
        public Vector3 position;
        public Vector3 rotation;
    }

    struct Health {
        public string type;
        public string who;
        public int health;
    }

    // In the start you get all the information
    struct Lobby {
        public string type;
        public string myId;
        public Vector3 myPosition;
        public List<string> otherIds;
        public List<Vector3> otherPositions;
        public List<int> health;
    }

    // Inplument those:
    struct NewPlayerJoinedLobby {
        public string type;
        public string id;
        public Vector3 position;
        public int health;
    }

    struct PlayerLeftLobby {
        public string type;
        public string id;
    }

    struct Ask {
        public string type;
    }

    // Don't even ask...
    class GetData<T> {
        public string type;
        public T data;
    }

    void Start()
    {
        info = FindFirstObjectByType<TextMeshProUGUI>();

        ws = new WebSocket("ws://localhost:3000");

        ws.OnMessage += (sender, e) =>
        {
            GetData<object> data = JsonUtility.FromJson<GetData<object>>(e.Data);
            switch (data.type)
            {
                case "position":
                    Position positionData = JsonUtility.FromJson<Position>(e.Data);
                    UpdatePosition(positionData);
                    break;
                case "health":
                    Health healthData = JsonUtility.FromJson<Health>(e.Data);
                    changeThingsList.Add(() => {
                        UpdateHealth(healthData);
                    });
                    break;
                case "lobby":
                    Lobby id = JsonUtility.FromJson<Lobby>(e.Data);
                    myId = id.myId;
                    changeThingsList.Add(() => {
                        player.transform.position = id.myPosition;
                        player.GetComponent<Player>().health = 100;
                    });
                    otherIds = id.otherIds;
                    otherPlayerPositions = id.otherPositions;
                    otherPlayerHealth = id.health;
                    for(int i = 0; i < otherIds.Count; i++) {
                        int currentIndex = i;
                        changeThingsList.Add(() => {
                            SpawnNewPlayer(otherIds[currentIndex], otherPlayerPositions[currentIndex], otherPlayerHealth[currentIndex]);
                        });
                    }
                    break;
                case "newPlayerJoinedLobby":
                    NewPlayerJoinedLobby newPlayerData = JsonUtility.FromJson<NewPlayerJoinedLobby>(e.Data);
                    changeThingsList.Add(() => {
                        SpawnNewPlayer(newPlayerData.id, newPlayerData.position, 100);
                    });
                    break;
                case "playerLeftLobby":
                    PlayerLeftLobby leftedPlayer = JsonUtility.FromJson<PlayerLeftLobby>(e.Data);
                    changeThingsList.Add(() => {
                        RemovePlayer(leftedPlayer.id);
                    });
                    break;
                default:
                    Debug.Log("What is this!?");
                    break;
            }
        };

        ws.Connect();
    }

    void Update() {
        if(changeThingsList.Count > 0) {
            foreach(var action in changeThingsList) {
                action();
            }

            changeThingsList = new List<ChangeThings>();
        }

        if(!haveIds && ws != null && ws.IsAlive) {
            ws.Send(JsonUtility.ToJson(new Ask{type = "lobby"}));
            haveIds = true;
        }

        info.text = infoText;
    }

    void UpdatePosition(Position data) {
        if (data.who == myId) {
            changeThingsList.Add(() => {
                player.transform.position = data.position;
            });
        } else {
            for(int i = 0; i < players.Count; i++) {
                if(data.who == players[i].GetComponent<PlayerSync>().id) {
                    int currentOtherIndex = i;
                    changeThingsList.Add(() => {
                        players[currentOtherIndex].GetComponent<PlayerSync>().ChangePosition(data.position, data.rotation);
                    });
                }
            }
        }
    }

    public void SendPosition(Vector3 newPosition, Vector3 newRotation) {
        if(myId != "") {
            Position dataObj = new Position{type = "position", who = myId, position = newPosition, rotation = newRotation};
            string dataStr = JsonUtility.ToJson(dataObj);
            ws.Send(dataStr);
        }
    }

    public void SendDemage(string id, int newHealth) {
        Health dataObj = new Health{type = "health", who = id, health = newHealth};
        string dataStr = JsonUtility.ToJson(dataObj);
        ws.Send(dataStr);
    }

    void UpdateHealth(Health data) {
        if(myId == data.who) {
            player.GetComponent<Player>().health = data.health;
        } else {
            for(int i = 0; i < players.Count; i++) {
                if(data.who == players[i].GetComponent<PlayerSync>().id) {
                    players[i].GetComponent<PlayerSync>().SetHealth(data.health);
                }
            }
        }
    }

    void SpawnNewPlayer(string newId, Vector3 position, int health) {
        players.Add(Instantiate(playerPrefab, position, Quaternion.identity));
        players[players.Count - 1].GetComponent<PlayerSync>().SetId(newId);
        players[players.Count - 1].GetComponent<PlayerSync>().SetHealth(health);
    }

    void RemovePlayer(string removeId) {
        for(int i = 0; i < players.Count; i++) {
            if(players[i].GetComponent<PlayerSync>().id == removeId) {
                Destroy(players[i]);
                players.RemoveAt(i);
                i = players.Count + 1000;
            }
        }

        return;
    }

    private void OnDestroy() {
        if (ws != null && ws.IsAlive) {
            ws.Close();
        }
    }
}