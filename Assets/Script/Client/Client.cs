using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Client : MonoBehaviour
{
	public GameObject chatContainer;
	public GameObject messagePrefab;

	public string clientName;

	private bool socketReady;
	private TcpClient socket;
	private NetworkStream stream;
	private StreamWriter writer;
	private StreamReader reader;

	public void ConnectedToServer()
	{
		// If already connected, ignore this func;
		if (socketReady)
			return;

		// Default host / port values;
		string host = "127.0.0.1";
		int port = 6321;

		//Overwrite default host / port values, if there is something in those boxes;
		string h;
		int p;
		h = GameObject.Find("HostInput").GetComponent<TMP_InputField>().text;
		if (h != "")
			host = h;

		int.TryParse(GameObject.Find("PortInput").GetComponent<TMP_InputField>().text, out p);
		if(p != 0)
			port = p;

		// Create the Socket;
		try
		{
			socket = new TcpClient(host, port);
			stream = socket.GetStream();
			writer = new StreamWriter(stream);
			reader = new StreamReader(stream);
			socketReady = true;
		}
		catch (Exception e)
		{
			Debug.Log($"Socket Error : {e.Message}");
		}
	}

	private void Update()
	{
		if(socketReady)
		{
			if(stream.DataAvailable)
			{
				string data = reader.ReadLine();
				if(data != null)
				{
					OnIncomingData(data);
				}
			}
		}
	}

	private void OnIncomingData(String data)
	{
		if(data == "%Name")
		{
			Send("&Name|" + GameObject.Find("NameInput").GetComponent<TMP_InputField>().text);
			return;
		}
		GameObject go = Instantiate(messagePrefab, chatContainer.transform);
		go.GetComponentInChildren<TMP_Text>().text = data;
	}

	private void Send(String data)
	{
		if (!socketReady)
			return;

		writer.WriteLine(data);
		writer.Flush();
	}

	public void OnSendButton()
	{
		string message = GameObject.Find("SendInput").GetComponent<TMP_InputField>().text;
		Send(message);
	}

	private void CloseSocket()
	{
		if(!socketReady)
			return;
		writer.Close();
		reader.Close();
		socket.Close();
		socketReady = false;
	}

	private void OnApplicationQuit()
	{
		CloseSocket();
	}

	private void OnDisable()
	{
		CloseSocket();
	}
}
