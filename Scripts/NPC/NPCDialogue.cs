using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Net.Sockets;
using System;

public class NPCDialogue : MonoBehaviour
{
    private GameObject text_dialogue;
    private TextMeshProUGUI textMesh;
    // Start is called before the first frame update

    //private int update_co = 0;

    [SerializeField] private LLMOptions llm_name_enum;
    public string llm_name;
    //public string llm_name { get { return _llm_name; } private set { _llm_name = value; } }
    //[SerializeField] private float[] dataOut, dataIn; //debugging

    void Start()
    {
        text_dialogue = transform.Find("Canvas").Find("Text-Dialogue").gameObject;
        textMesh = text_dialogue.GetComponent<TextMeshProUGUI>();
        llm_name = llm_name_enum.ToString();
    }

    /// <summary>
    /// Helper function for sending and receiving.
    /// </summary>
    /// <param name="dataOut">Data to send</param>
    /// <returns></returns>
    /*
    protected float[] ServerRequest(float[] dataOut)
    {
        //print("request");
        this.dataOut = dataOut; //debugging
        this.dataIn = SendAndReceive(dataOut); //debugging
        return this.dataIn;
    } */

    public void UpdateMessage(string text)
    {
        textMesh.text = text;
    }

    // Update is called once per frame
    void Update()
    {
        //string data_r = "none";
        //Debug.Log("dataIn: + dataIn[0].ToString());
        //textMesh.text = "Update: " + update_co.ToString() + "\nData: " + data_r;
        //update_co += 1;

        // Debug.Log("Text:" + textMesh.text);
    }
}
