using UnityEngine;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;

public class PythonCommunicator : MonoBehaviour
{
    private TcpClient client;
    private NetworkStream stream;
    private Process pythonProcess;
    private Thread receiveThread;
    private bool isRunning = true;
    private Queue<string> logQueue = new Queue<string>(); // Queue to hold log messages
    private Controller controller;
    private string messagePrefix = "tolog:";
    private string startPrefix = "tostart:";
    private string csqPrefix = "resumeendq";

    public void SetController(Controller controller)
    {
        this.controller = controller;
    }

    public void SetupServerConnection()
    {
        ConnectToServer();
    }

    void Update()
    {
        lock (logQueue)
        {
            while (logQueue.Count > 0)
            {
                string message = logQueue.Dequeue();
                if (message.StartsWith(messagePrefix))
                {
                    string messageTmp = message.Substring(messagePrefix.Length);
                    string[] parts = messageTmp.Split(';');
                    if (parts.Length != 2)
                    {
                        UnityEngine.Debug.Log("error in log message length: " + messageTmp);
                        return;
                    };
                    controller.SetSetupData(parts[0], parts[1]);
                    controller.ResumeScene();
                }
                if (message.StartsWith(startPrefix))
                {
                    string messageTmp = message.Substring(startPrefix.Length);
                    string[] parts = messageTmp.Split(';');
                    if (parts.Length != 4)
                    {
                        UnityEngine.Debug.Log("error in start message length: " + messageTmp);
                        return;
                    };
                    controller.dataCollector.SetStudyStart(parts[0], parts[1], parts[2], parts[3]);
                    controller.ResumeScene();
                }
                if (message.StartsWith(csqPrefix))
                {
                    controller.ResumeScene();
                }
            }
        }
    }

    void LogToUnityConsole(string message)
    {
        lock (logQueue)
        {
            logQueue.Enqueue(message);
        }
    }

    void LogPythonOutput(string output)
    {
        if (!string.IsNullOrEmpty(output))
        {
            lock (logQueue)
            {
                logQueue.Enqueue(output);
            }
        }
    }

    void LogPythonError(string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            lock (logQueue)
            {
                logQueue.Enqueue(error);
            }
        }
    }

    void ConnectToServer()
    {
        try
        {
            client = new TcpClient("localhost", 9999);
            stream = client.GetStream();
            LogToUnityConsole("Connected to server");

            receiveThread = new Thread(new ThreadStart(ReceiveMessages));
            receiveThread.Start();
        }
        catch (Exception e)
        {
            LogToUnityConsole("Socket error: " + e.Message);
        }
    }

    public void SendMessageToPython(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            stream.Write(data, 0, data.Length);
            LogToUnityConsole("Sent message to server: " + message);
        }
        catch (Exception e)
        {
            LogToUnityConsole("Error sending message: " + e.Message);
        }
    }

    public void ReceiveMessages()
    {
        byte[] buffer = new byte[1024];
        while (isRunning)
        {
            try
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    LogToUnityConsole(message);

                }
            }
            catch (Exception e)
            {
                LogToUnityConsole("Error receiving message: " + e.Message);
                break;
            }
        }
    }

    void OnApplicationQuit()
    {
        isRunning = false;
        if (stream != null)
        {
            stream.Close();
        }
        if (client != null)
        {
            client.Close();
        }
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill();
        }
    }
}
