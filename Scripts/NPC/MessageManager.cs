using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager;
using UnityEngine;
using System.Linq;
using UnityEngine.Windows;
using System.Threading;
using UnityEngine.Assertions;


public class MessageManager : SingletonMonobehaviour<MessageManager>
{
    private int idx_all = 0;
    Dictionary<string, string> request_d;
    Dictionary<string, string> receive_d;
    List<string> npc_list;
    Dictionary<string, NPCDialogue> npc_d;

    public string ip = "127.0.0.1";
    public int port = 60000;
    private Socket client;

    // Start is called before the first frame update
    void Start()
    {
        request_d = new Dictionary<string, string>();
        receive_d = new Dictionary<string, string>();
        npc_list = new List<string>();
        npc_d = new Dictionary<string, NPCDialogue>();
        npc_list.Add("NPC_Butch"); //llm_list.Add("gpt3.5-turbo");
        npc_d.Add("NPC_Butch", GameObject.Find("NPC_Butch").GetComponent<NPCDialogue>());
        npc_list.Add("NPC_Butch2"); //llm_list.Add("gpt3.5-turbo");
        npc_d.Add("NPC_Butch2", GameObject.Find("NPC_Butch2").GetComponent<NPCDialogue>());

        //First ask the python server to clear its memory
        string mes_r = SendAndReceive("-1 CLEAR placeholder placeholder placeholder placeholder placeholder");
        Assert.AreEqual(mes_r.Substring(0, 8), "-1 CLEAR", "The strings do not match.");
    }

    // Update is called once per frame
    void Update()
    {
        if (request_d.Count > 0)
        {
            System.Random rand = new System.Random();
            KeyValuePair<string, string> s_now = request_d.ElementAt(rand.Next(request_d.Count));
            string idx_now = s_now.Key;
            string mes_s = s_now.Value;

            string mes_r = SendAndReceive(mes_s);

            Dictionary<string, string> res_d = MessageParse(mes_r);
            string message_update = "placeholder";
            if (res_d["ctrl"] == "WORKING")
            {
                message_update = res_d["llm_name"] + ": " + "WORKING";
            }
            if (res_d["ctrl"] == "DONE")
            {
                request_d.Remove(idx_now);
                receive_d.Add(idx_now, res_d["llm_out"]);
                message_update = res_d["llm_name"] + ": " + res_d["llm_out"];
            }
            npc_d[res_d["npc_name"]].UpdateMessage(message_update);
        }
    }

    public void UpdatePlayerInput(string text)
    {
        foreach (string npc_name in npc_list)
        {
            idx_all += 1; //we will generate one message for each npc
            string mes = idx_all.ToString() + " REQUEST " + npc_name + " " + npc_d[npc_name].llm_name + " TEXT: " + text;
            Debug.Log("MessageManager adding message: " + mes);
            request_d.Add(idx_all.ToString(), mes);
        }
    }

    public Dictionary<string, string> MessageParse(string text)
    {
        List<string> tt = new List<string>(text.Split(" "));
        //Debug.Log(tt.ToString()); UnityEditor.EditorApplication.isPlaying = false;
        Dictionary<string, string> res_d = new Dictionary<string, string>(){
            {"idx", tt[0]},
            {"ctrl", tt[1]},
            {"npc_name", tt[2]},
            {"llm_name", tt[3]},
            {"llm_out", String.Join(" ", tt.Skip(4))}
        };
        return res_d;
    }

    /// <summary> 
    /// Send data to port, receive data from port.
    /// </summary>
    /// <param name="dataOut">Data to send</param>
    /// <returns></returns>
    private string SendAndReceive(string dataOut_s)
    {
        try
        {
            //initialize socket
            float[] floatsReceived;
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(ip, port);
            if (!client.Connected)
            {
                Debug.LogError("Connection Failed");
                return null;
            }

            //convert floats to bytes, send to port
            //var byteArray = new byte[dataOut.Length * 4];
            //Buffer.BlockCopy(dataOut, 0, byteArray, 0, byteArray.Length);
            //client.Send(byteArray);

            //trying string
            byte[] byData = System.Text.Encoding.UTF8.GetBytes(dataOut_s);
            client.Send(byData);

            //allocate and receive bytes
            byte[] bytes_r = new byte[4000];
            int iRx = client.Receive(bytes_r);
            //print(idxUsedBytes + " new bytes received.");

            char[] chars = new char[iRx];
            System.Text.Decoder d = System.Text.Encoding.UTF8.GetDecoder();
            int charLen = d.GetChars(bytes_r, 0, iRx, chars, 0);
            string dataIn_s = new string(chars);

            //convert bytes to floats
            //floatsReceived = new float[idxUsedBytes / 4];
            //Buffer.BlockCopy(bytes, 0, floatsReceived, 0, idxUsedBytes);

            client.Close();
            return dataIn_s;
        }
        catch (Exception ex)
        {
            Debug.LogException(ex, this);
            Application.Quit();
            UnityEditor.EditorApplication.isPlaying = false;
            return null;
        }
    }


}
