using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using Unity.VisualScripting;
using UnityEngine;

public class Server : MonoBehaviour
{
	private List<ServerClient> clients;
	private List<ServerClient> disconnectList;

	public int port = 6321;
	private TcpListener server;
	private bool serverStarted;

	private void Start()
	{
		clients = new List<ServerClient>();
		disconnectList = new List<ServerClient>();

		try
		{
			server = new TcpListener(IPAddress.Any, port);
			server.Start();

			StartListening();
			serverStarted = true;

			Debug.Log($"Server has been started on port : {port}");
		}
		catch (Exception e)
		{
			Debug.Log("Socket Error : " + e.Message);
		}
	}

	private void Update()
	{
		if (!serverStarted)
			return;

		foreach (ServerClient c in clients)
		{
			// is client still connected?
			if(!IsConnected(c.tcp))
			{
				c.tcp.Close();
				disconnectList.Add(c);
				continue;
			}
			// Check for message from the client;
			else
			{
				NetworkStream s = c.tcp.GetStream();
				if(s.DataAvailable)
				{
					StreamReader reader = new StreamReader(s, true);
					string data = reader.ReadLine();
					
					if(data != null)
					{
						OnIncomingData(c, data);
					}
				}
			}
		}
		for(int i = 0; i < disconnectList.Count - 1; i++)
		{
			BroadCast($"{disconnectList[i].clientName} disconnected!!!", clients);

			clients.Remove(disconnectList[i]);
			disconnectList.RemoveAt(i);
		}
	}

	private void StartListening()
	{
		server.BeginAcceptTcpClient(AcceptTcpClient, server);
	}

	private bool IsConnected(TcpClient c)
	{
		try
		{
			if (c != null && c.Client != null && c.Client.Connected)
			{
				if (c.Client.Poll(0, SelectMode.SelectRead))
				{
					return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
				}

				return true;
			}
			else
				return false;
		}
		catch (Exception)
		{
			return false;
		}
	}

	private void AcceptTcpClient(IAsyncResult ar)
	{
		TcpListener listen = (TcpListener) ar.AsyncState;

		clients.Add(new ServerClient(listen.EndAcceptTcpClient(ar)));
		StartListening();

		//Send a message to everyone, say someone has connected;
		BroadCast("%Name", new List<ServerClient>() { clients[clients.Count - 1] });
	}

	private void OnIncomingData(ServerClient c, string data)
	{
		if(data.Contains("&Name"))
		{
			c.clientName = data.Split('|')[1];
			BroadCast($"{c.clientName} has connected!!", clients);
			return;
		}
		BroadCast($"{c.clientName} : {data}", clients);
	}

	private void BroadCast(string data, List<ServerClient> cl)
	{
		foreach (ServerClient c in cl)
		{
			try
			{
				StreamWriter writer = new StreamWriter(c.tcp.GetStream());
				writer.WriteLine(data);
				writer.Flush();
			}
			catch (Exception e)
			{
				Debug.Log($"Write Error : {e.Message} to client {c.clientName}");
			}
		}
	}
}

public class ServerClient
{
	public TcpClient tcp;
	public string clientName;

	public ServerClient(TcpClient clientSocket)
	{
		clientName = "Guest";
		tcp = clientSocket;
	}
}
