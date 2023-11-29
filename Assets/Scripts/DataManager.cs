using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;

public class DataManager : MonoBehaviour
{
    float prevPos; //prevPos of catheter tip to calculate movement direction
    List<dataEntry> data;
    List<dataEntry> handMotionData = new List<dataEntry>(); //smoothed
    [HideInInspector]
    public string startTime;
    bool started;
    struct dataEntry
    {
        public string timestamp;

        public dataEntry(string timestamp)
        {
            this.timestamp = timestamp;
        }
        public string getAsString()
        {
            return " ;;" + timestamp;
        }
    }

    void WriteData()
    {
        StringBuilder sb = new StringBuilder();
        for (int index = 0; index < handMotionData.Count; index++)
            sb.AppendLine(handMotionData[index].getAsString());
        string filePath = Application.persistentDataPath + "/results" + startTime + ".csv";
        StreamWriter outStream = File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();
        Debug.Log("data saved at: "+Application.persistentDataPath);
    }
}