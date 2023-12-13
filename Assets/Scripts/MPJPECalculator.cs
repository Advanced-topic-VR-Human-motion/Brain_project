using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

public class MPJPECalculator : MonoBehaviour{

    public string path; //path to data file
    public GameObject markerContainer;
    

    private class JointOffset
    {
        public string startJointName;
        public string endJointName;
        public Vector3 offset;
    }

    private JointOffset[] RealJointOffsets;
    private JointOffset[] MeasuredJointOffsets;

    
    private StreamReader ReadFile(string path)
    {
        StreamReader reader = new StreamReader(path);
        string line = reader.ReadLine(); //first line = headers
        return reader;
    }

        //function to find the total number of lines in the file being read
    private int FindSize(StreamReader reader)
    {
        int i = 1;
        string line = reader.ReadLine();
        while (line != null)
        {
            i++;
            line = reader.ReadLine();
        }
        return i;
    }

      //format of lines:
      // startjoint endjoint x y z
    private void ExtractRealJointData(StreamReader reader)
    {
        string line;
        line = reader.ReadLine(); //first line

        //extract info and distribute
        while (line != null && line != "") //interrupt at empty line or end of file
        {
            string[] temp = line.Split(separator.ToCharArray());
            if(temp.Length < 2)
                break;
            
            JointOffset jointOffset = new JointOffset();
            jointOffset.startJointName = temp[0];
            jointOffset.endJointName = temp[1];
            jointOffset.offset = new Vector3(float.Parse(temp[2]), float.Parse(temp[3]), float.Parse(temp[4]));
            RealJointOffsets.Add(jointOffset);

            line = reader.ReadLine();
        }

        if(line == null)
            Debug.Log("End of file reached");
    }

    private void CalculateJointOffsets(){
        foreach (GameObject marker in markerContainer){
            string markerName = marker.name;
            foreach (JointOffset jointOffset in RealJointOffsets){
                if (markerName == jointOffset.startJointName){
                    string endMarkerName = jointOffset.endJointName;
                    GameObject endMarker = markerContainer.Find(endMarkerName);
                }
            }
        }
    }


    private void CalculateMPJPE(){

    }
}
    
    

