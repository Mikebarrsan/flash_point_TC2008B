// TC2008B Modelación de Sistemas Multiagentes con gráficas computacionales
// C# client to interact with Python server via POST
// Sergio Ruiz-Loza, Ph.D. March 2021

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class WebClient : MonoBehaviour
{
    public GameObject RoomPrefab;
    public GameObject DestroyedPrefab;
    public GameObject DoorClosedPrefab;
    public GameObject DoorOpenPrefab;
    public GameObject FirePrefab;
    public GameObject SmokePrefab;
    public GameObject PoiPrefab;
    public GameObject AgentPrefab;
    public GameObject VictimPrefab;
    public GameObject RoomHolder;
    public GameObject FireHolder;
    public GameObject PoiHolder;
    public GameObject AgentHolder;
    private string[] direction = {"Up", "Right", "Down","Left"};
    private bool moving = false;

    IEnumerator getMap()
    {
        string url = "http://localhost:8585/map";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            //www.SetRequestHeader("Content-Type", "text/html");
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();          // Talk to Python
            if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                Map mapa = JsonConvert.DeserializeObject<Map>(www.downloadHandler.text.Replace('\'', '\"'));
                for (int i = 0; i < mapa.walls.Length; i++)
                {
                    for (int j = 0; j < mapa.walls[i].Length; j++)
                    {
                        GameObject room = Instantiate(RoomPrefab, new Vector3(j * -1, 0, i), Quaternion.identity, RoomHolder.transform);
                        for (int k = 0; k < mapa.walls[i][j].Length; k++)
                        {
                            if (Convert.ToString(mapa.walls[i][j][k]) == "1" | Convert.ToString(mapa.walls[i][j][k]) == "2")
                            {
                                Transform wall = room.transform.Find(direction[k]);
                                wall.gameObject.SetActive(true);
                            }else if (Convert.ToString(mapa.walls[i][j][k]) == "door"){
                                ReplaceWall(k, room, DoorClosedPrefab);
                            }else if (Convert.ToString(mapa.walls[i][j][k]) == "entrance"){
                                ReplaceWall(k, room, DoorOpenPrefab);
                            }
                        }
                    }
                }
                
                for (int i = 0; i < mapa.fires.Length; i++)
                {
                    for (int j = 0; j < mapa.fires[i].Length; j++)
                    {
                        if(mapa.fires[i][j] == 2){
                            Instantiate(FirePrefab, new Vector3(j * -1, 0, i), Quaternion.Euler(-90,0,0), FireHolder.transform);
                        }
                    }
                }

                for (int i = 0; i < mapa.poi.Length; i++)
                {
                    Instantiate(PoiPrefab, new Vector3(mapa.poi[i][1] * -1, 0, mapa.poi[i][0]), Quaternion.identity, FireHolder.transform);
                }

                for (int i = 0; i < mapa.agents.Length; i++)
                {
                    GameObject agente = Instantiate(AgentPrefab, new Vector3(mapa.agents[i][2] * -1, 0, mapa.agents[i][1]), Quaternion.identity, AgentHolder.transform);
                    agente.name = mapa.agents[i][0].ToString();
                }
            }
        }
    }

    IEnumerator getStep()
    {
        moving = true;
        string url = "http://localhost:8585/step";
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();          // Talk to Python
            if ((www.result == UnityWebRequest.Result.ConnectionError) || (www.result == UnityWebRequest.Result.ProtocolError))
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log(www.downloadHandler.text);
                Moves resp = JsonConvert.DeserializeObject<Moves>(www.downloadHandler.text.Replace('\'', '\"'));
                bool delay = false;
                Transform agent = AgentHolder.transform.Find(resp.moves_agent[0][0]);

                for (int i = 1; i < resp.moves_agent.Length; i++)
                {
                    int x = int.Parse(resp.moves_agent[i][2]) * -1;
                    int z = int.Parse(resp.moves_agent[i][1]);

                    if (resp.moves_agent[i][0] == "move"){
                        Vector3 targetPos = new Vector3(x,0,z);
                        Vector3 currentPos = agent.position;
                        int rotation = 0;
                        float magX = agent.position.x - x;
                        float magZ = agent.position.z - z;

                        if (magX < 0 & Math.Abs(magX) > Math.Abs(magZ)){
                            rotation = 180;
                        }else if (magX > 0 & Math.Abs(magX) > Math.Abs(magZ)){
                            rotation = 0;
                        }else if (magZ > 0 & Math.Abs(magZ) > Math.Abs(magX)){
                            rotation = -90;
                        }else if (magZ < 0 & Math.Abs(magZ) > Math.Abs(magX)){
                            rotation = 90;
                        }

                        float timeElapsed = 0;
                        float timeToMove = 1;
                        
                        agent.rotation = Quaternion.Euler(0, rotation, 0);
                        
                        while(timeElapsed < timeToMove){
                            agent.position = Vector3.Lerp(currentPos,targetPos,timeElapsed/timeToMove);
                            timeElapsed += Time.deltaTime;
                            yield return null;
                        }
                    }else if (resp.moves_agent[i][0] == "extinguish"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "Fire(Clone)"){
                                Destroy(intersecting[j].gameObject);
                            }
                        }
                    }else if (resp.moves_agent[i][0] == "victim"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "POI(Clone)"){
                                Destroy(intersecting[j].gameObject);
                            }
                        }

                        Instantiate(VictimPrefab, agent.position, agent.rotation, agent);

                    }else if (resp.moves_agent[i][0] == "false"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "POI(Clone)"){
                                Destroy(intersecting[j].gameObject);
                            }
                        }
                    }else if (resp.moves_agent[i][0] == "entrance"){
                        Debug.Log("Llegó a entrada con un POI");
                        Transform Victim = agent.Find("Victim(Clone)");
                        Destroy(Victim.gameObject);
                    }
                }
                yield return new WaitForSeconds(1);

                for (int i = 0; i < resp.moves.Length; i++)
                {
                    int x = int.Parse(resp.moves[i][2]) * -1;
                    int z = int.Parse(resp.moves[i][1]);

                    if (resp.moves[i][0] == "fire"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "Room(Clone)"){
                                Instantiate(FirePrefab, new Vector3(x, 0, z), Quaternion.Euler(-90,0,0), FireHolder.transform);
                            }else if (intersecting[j].gameObject.name == "Smoke(Clone)"){
                                Destroy(intersecting[j].gameObject);
                            }
                        }
                    }else if (resp.moves[i][0] == "smoke"){
                        Instantiate(SmokePrefab, new Vector3(x, 0, z), Quaternion.Euler(-90,0,0), FireHolder.transform);
                    }else if (resp.moves[i][0] == "explosion"){

                    }else if (resp.moves[i][0] == "flashover"){
                        if (!delay){
                            yield return new WaitForSeconds(0.5f);
                            delay = true;
                        }
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "Smoke(Clone)"){
                                Destroy(intersecting[j].gameObject);
                                Instantiate(FirePrefab, new Vector3(x, 0, z), Quaternion.Euler(-90,0,0), FireHolder.transform);
                            }
                        }
                    }else if (resp.moves[i][0] == "door"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "Room(Clone)"){
                                Transform door = intersecting[j].transform.Find(direction[int.Parse(resp.moves[i][3])]);
                                Destroy(door.gameObject);
                            }
                        }
                    }else if (resp.moves[i][0] == "wall"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "Room(Clone)"){
                                if (int.Parse(resp.moves[i][4]) == 1){
                                    ReplaceWall(int.Parse(resp.moves[i][3]), intersecting[j].gameObject, DestroyedPrefab);
                                }else if (int.Parse(resp.moves[i][4]) == 0){
                                    Transform door = intersecting[j].transform.Find(direction[int.Parse(resp.moves[i][3])]);
                                    Destroy(door.gameObject);
                                }
                            }
                        }
                    }else if (resp.moves[i][0] == "poiD"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        for (int j = 0; j < intersecting.Length; j++) {
                            if (intersecting[j].gameObject.name == "POI(Clone)"){
                                Destroy(intersecting[j].gameObject);
                            }
                        }
                    }else if (resp.moves[i][0] == "poiC"){
                        Debug.Log("Crea POI en x:" + x + " z: " + z);
                        Instantiate(PoiPrefab, new Vector3(x, 0, z), Quaternion.identity, FireHolder.transform);
                    }
                    else if (resp.moves[i][0] == "teleport"){
                        Transform selected = AgentHolder.transform.Find(resp.moves[i][3]);
                        selected.position = new Vector3(x, 0, z);
                        Transform Victim = agent.Find("Victim(Clone)");
                        if (Victim != null){
                            Destroy(Victim.gameObject);
                        }
                    }
                }
                yield return new WaitForSeconds(1);
                moving = false;
            }
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(getMap());
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)){
        if (!moving){
            StartCoroutine(getStep());
        }else if (Input.GetKeyDown(KeyCode.Return)){
            SceneManager.LoadScene("Default");
        }
    }

    void ReplaceWall(int k, GameObject room, GameObject prefab){
        GameObject new_object = Instantiate(prefab,room.transform);
        Transform wall = room.transform.Find(direction[k]);
        new_object.transform.position = wall.position;
        new_object.transform.rotation = wall.rotation;
        new_object.name = wall.name;
        Destroy(wall.gameObject);
    }
}

