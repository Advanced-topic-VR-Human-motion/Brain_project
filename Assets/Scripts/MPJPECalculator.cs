using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

public class MPJPECalculator : MonoBehaviour{

    const string separator = " "; //tab separation string
    public string path; //path to data file
    public GameObject markerContainer; 
    

    private class JointDistance
    {
        public string startJointName;
        public string endJointName;
        public float distance;
    }

    private List<JointDistance> TrueJointDistances;
    private List<JointDistance> MeasuredJointDistances;

    void Start()
    {
        TrueJointDistances = new List<JointDistance>();
        MeasuredJointDistances = new List<JointDistance>();
        readStream(path);
    }
    
    void Update(){
        MeasureJointDistances();
        CalculateMPJPE();
    }

    private void readStream(string filepath)
    {
        // slider.onValueChanged.AddListener(delegate { ChangeSpeed(); });

        BetterStreamingAssets.Initialize();
        StreamReader sr = null;
        string streamPath = path.Split('/').Last();
        if (!BetterStreamingAssets.FileExists(streamPath) )
        {
            Debug.LogErrorFormat("Streaming asset not found: {0}", streamPath);
        }
        else{
            sr = BetterStreamingAssets.OpenText(streamPath); //read from file
        }


        //extract and distribute info
        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        ExtractTrueJointData(sr);

        //close reader
        sr.Close();
    }

      //format of lines:
      // startjoint endjoint x y z
    private void ExtractTrueJointData(StreamReader reader)
    {
        string line;
        line = reader.ReadLine(); //first line

        //extract info and distribute
        while (line != null && line != "") //interrupt at empty line or end of file
        {
            string[] temp = line.Split(separator.ToCharArray());
            if(temp.Length < 2)
                break;
            
            JointDistance jointDistance = new JointDistance();
            jointDistance.startJointName = temp[0];
            jointDistance.endJointName = temp[1];
            jointDistance.distance = float.Parse(temp[2]);
            TrueJointDistances.Add(jointDistance);
            line = reader.ReadLine();
        }

        if(line == null)
            Debug.Log("End of file reached");
    }

    private void MeasureJointDistances(){
        foreach (JointDistance trueDistance in TrueJointDistances){
            string startJointName = trueDistance.startJointName;
            string endJointName = trueDistance.endJointName;
            GameObject startMarker = GameObject.Find(startJointName);
            GameObject endMarker = GameObject.Find(endJointName);
            float distance = Vector3.Distance(startMarker.transform.position, endMarker.transform.position);
            if(!UpdateMeasuredDistance(startJointName, endJointName, distance)){
                JointDistance measuredJointDistance = new JointDistance();
                measuredJointDistance.startJointName = startJointName;
                measuredJointDistance.endJointName = endJointName;
                measuredJointDistance.distance = distance * 1000f;
                Debug.Log("Measured distance between " + startJointName + " and " + endJointName + ": " + measuredJointDistance.distance);
                MeasuredJointDistances.Add(measuredJointDistance);
            }
        }
    }

    private bool UpdateMeasuredDistance(string startJointName, string endJointName, float newDistance){
        foreach(JointDistance jointDistance in MeasuredJointDistances){
            if(jointDistance.startJointName == startJointName && jointDistance.endJointName == endJointName){
                jointDistance.distance = newDistance * 1000f;
                return true;
            }
        }
        return false;
    }


    private void CalculateMPJPE(){

        /*
        the MPJPE is calculated as the mean Euclidean distance between the predicted 3D joint locations and the 
        corresponding ground truth joint locations. This metric is used to evaluate how accurately the algorithm 
        is able to predict the 3D pose of a person in an image or video.
        */

        float MPJPE = 0f;
        float maxEuclidianDistance = -100f;
        string startMaxJointName = "", endMaxJointName = "";
        foreach(JointDistance trueDistance in TrueJointDistances){
            foreach(JointDistance measuredDistance in MeasuredJointDistances){
                if (trueDistance.startJointName == measuredDistance.startJointName){
                    float euclideanDistance = Mathf.Sqrt(Mathf.Pow(trueDistance.distance - measuredDistance.distance, 2));
                    if(euclideanDistance > maxEuclidianDistance){
                        maxEuclidianDistance = euclideanDistance;
                        startMaxJointName = trueDistance.startJointName;
                        endMaxJointName = trueDistance.endJointName;
                    }
                    MPJPE += euclideanDistance;
                }
            }
        }
        Debug.Log("Max Euclidian Distance: " + maxEuclidianDistance + " between " + startMaxJointName + " and " + endMaxJointName 
        +" (True distance: " + TrueJointDistances.Find(x => x.startJointName == startMaxJointName && x.endJointName == endMaxJointName).distance + ")" + 
        " (Measured distance: " + MeasuredJointDistances.Find(x => x.startJointName == startMaxJointName && x.endJointName == endMaxJointName).distance + ")");
        MPJPE = MPJPE / TrueJointDistances.Count;
        Debug.Log("MPJPE: " + MPJPE + " mm");
    }
}
    
    

