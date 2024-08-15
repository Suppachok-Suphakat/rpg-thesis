using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OnScreenMessage
{
    public GameObject go;
    public float timeToFade;

    public OnScreenMessage(GameObject go)
    {
        this.go = go;
    }
}

public class OnScreenMessageSystem : MonoBehaviour
{
    public static OnScreenMessageSystem instance;

    [SerializeField] GameObject textPrefab;

    [SerializeField] float horizontalScatter = 0.25f;
    [SerializeField] float verticalScatter = 0.5f;
    [SerializeField] float timeToFade = 0.5f;

    List<OnScreenMessage> onScreenMessageList;
    List<OnScreenMessage> openList;

    private void Awake()
    {
        instance = this;

        onScreenMessageList = new List<OnScreenMessage>();
        openList = new List<OnScreenMessage>();
    }

    private void Update()
    {
        for(int i = onScreenMessageList.Count -1; i >= 0; i--)
        {
            onScreenMessageList[i].timeToFade -= Time.deltaTime;
            if (onScreenMessageList[i].timeToFade < 0)
            {
                onScreenMessageList[i].go.SetActive(false);
                openList.Add(onScreenMessageList[i]);
                onScreenMessageList.RemoveAt(i);
            }
        }
    }

    public void PostMessage(Vector3 worldposition, string message)
    {
        worldposition.z = -1;
        worldposition.x += Random.Range(-horizontalScatter, horizontalScatter);
        worldposition.y += Random.Range(-verticalScatter, verticalScatter);

        if (openList.Count > 0)
        {
            ReuseObjectFromOpenList(worldposition, message);
        }
        else
        {
            CreateNewOnScreenMessageObject(worldposition, message);

        }
    }

    private void ReuseObjectFromOpenList(Vector3 worldposition, string message)
    {
        OnScreenMessage osm = openList[0];
        osm.go.SetActive(true);
        osm.timeToFade = timeToFade;
        osm.go.GetComponent<TextMeshPro>().text = message;
        osm.go.transform.position = worldposition;
        openList.RemoveAt(0);
        onScreenMessageList.Add(osm);
    }

    private void CreateNewOnScreenMessageObject(Vector3 worldposition, string message)
    {
        GameObject textGO = Instantiate(textPrefab, transform);
        textGO.transform.position = worldposition;

        TextMeshPro tmp = textGO.GetComponent<TextMeshPro>();
        tmp.text = message;

        OnScreenMessage onScreenMessage = new OnScreenMessage(textGO);
        onScreenMessage.timeToFade = timeToFade;
        onScreenMessageList.Add(onScreenMessage);
    }
}
