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

public class WebClient : MonoBehaviour
{
    public GameObject RoomPrefab;
    public GameObject FirePrefab;
    public GameObject SmokePrefab;
    public GameObject RoomHolder;
    public GameObject FireHolder;

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
                                if (k == 0)
                                {
                                    Transform wall = room.transform.Find("Up");
                                    wall.gameObject.SetActive(true);
                                }
                                else if (k == 1)
                                {
                                    Transform wall = room.transform.Find("Right");
                                    wall.gameObject.SetActive(true);
                                }
                                else if (k == 2)
                                {
                                    Transform wall = room.transform.Find("Down");
                                    wall.gameObject.SetActive(true);
                                }
                                else if (k == 3)
                                {
                                    Transform wall = room.transform.Find("Left");
                                    wall.gameObject.SetActive(true);
                                }
                            }else if (Convert.ToString(mapa.walls[i][j][k]) == "door"){
                                //Cambiar modelo de pared por puerta
                            }else if (Convert.ToString(mapa.walls[i][j][k]) == "entrance"){
                                //Cambiar el modelo de pared por puerta
                            }
                        }
                    }
                }
                
                for (int i = 0; i < mapa.fires.Length; i++)
                {
                    for (int j = 0; j < mapa.fires[i].Length; j++)
                    {
                        if(mapa.fires[i][j] == 2){
                            Instantiate(FirePrefab, new Vector3(j * -1, 0, i), Quaternion.identity, FireHolder.transform);
                        }
                    }
                }
            }
        }

    }

    IEnumerator getStep()
    {
        string url = "http://localhost:8585/step";
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
                Moves resp = JsonConvert.DeserializeObject<Moves>(www.downloadHandler.text.Replace('\'', '\"'));
                bool delay = false;
                for (int i = 0; i < resp.moves.Length; i++)
                {
                    int x = int.Parse(resp.moves[i][2]) * -1;
                    int z = int.Parse(resp.moves[i][1]);

                    if (resp.moves[i][0] == "fire"){
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        if (intersecting.Length == 0) {
                            Instantiate(FirePrefab, new Vector3(x, 0, z), Quaternion.Euler(-90,0,0), FireHolder.transform);
                        }else{
                            Destroy(intersecting[0].gameObject);
                            Instantiate(FirePrefab, new Vector3(x, 0, z), Quaternion.Euler(-90,0,0), FireHolder.transform);
                        }
                    }else if (resp.moves[i][0] == "smoke"){
                        Instantiate(SmokePrefab, new Vector3(x, 0, z), Quaternion.identity, FireHolder.transform);
                    }else if (resp.moves[i][0] == "explosion"){
                        Debug.Log("Explosión en " + resp.moves[i][1] + ", " + resp.moves[i][2]);
                    }else if (resp.moves[i][0] == "flashover"){
                        if (!delay){
                            yield return new WaitForSeconds(2.0f);
                            Debug.Log("Espero 2 segs");
                            delay = true;
                        }
                        Collider[] intersecting = Physics.OverlapSphere(new Vector3(x, 0, z), 0.01f);
                        Destroy(intersecting[0].gameObject);
                        Instantiate(FirePrefab, new Vector3(x, 0, z), Quaternion.Euler(-90,0,0), FireHolder.transform);
                    }
                }
            }
        }

    }


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(getMap());
        //string call = "What's up?";
        //Vector3 fakePos = new Vector3(3.44f, 0, -15.707f);
        //string json = EditorJsonUtility.ToJson(fakePos);
        //StartCoroutine(SendData(call));
        //StartCoroutine(SendData(json));
        // transform.localPosition

        //StartCoroutine(getMap());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(getStep());
        }else if (Input.GetKeyDown(KeyCode.Return)){
            SceneManager.LoadScene("Default");
        }
    }
}

