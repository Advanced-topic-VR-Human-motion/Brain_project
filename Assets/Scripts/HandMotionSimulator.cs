using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.IO;
using System;

public class HandMotionSimulator : MonoBehaviour
{
    float timeToCall;
    float timeDelay = 0.00005f;
    const string separator = "\t"; //tab separation string
    public string path; //path to tsv file
    int index, fileSize; //index to cycle through arrays
    bool readyToUpdate;
    public Button start;
    bool simulationStarted = false;
    bool pause = false; // if the playing of the recording is to be paused.

    public GameObject[] handMarkers;
    public Slider slider;
    public GameObject player;

    //arrays with data from each row
    private float[] time;
    private static string[] markerNames = { 
        "RThumb1", "RThumbTip", "RIndex2", "RIndexTip", "RMiddle2", "RMiddleTip", "RRing2", "RRingTip", "RPinky2", "RPinkyTip",
        "LThumb1", "LThumbTip", "LIndex2", "LIndexTip", "LMiddle2", "LMiddleTip", "LRing2", "LRingTip", "LPinky2", "LPinkyTip"
    };

    private Dictionary<string, float[,]> markersToData = new Dictionary<string, float[,]>();
    private  Dictionary<string, Coordinate> markers; // when data is parsed, it is stored in this array

    struct Coordinate{
        public float x,y,z;
    }

    void Start()
    {
        start.onClick.AddListener(() => { 
            simulationStarted = true;
            //start motion
        });

    }

    void Update()
    {
        //DEBUG
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(!simulationStarted)
                simulationStarted = true;
        }
    }

    private void startStream(string filepath)
    {
        //initialize indexes
        index = fileSize = 0;
        readyToUpdate = false;

        // slider.onValueChanged.AddListener(delegate { ChangeSpeed(); });

        timeToCall = Time.fixedTime + timeDelay;
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

        fileSize = FindSize(sr); //find size of file


        //initialize arrays
        time = new float[fileSize];
        foreach (string markername in markerNames)
        {
            if(!markersToData.ContainsKey(markername))
                markersToData.Add(markername, new float[fileSize, 3]);
        }
        //initialize markers
        markers = new Dictionary<string, Coordinate>();
        foreach (string markername in markerNames)
        {
            markers.Add(markername, new Coordinate());
        }

        //extract and distribute info
        sr.DiscardBufferedData();
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        Extract(sr);
        readyToUpdate = true;

        //close reader
        sr.Close();
    }

    void FixedUpdate()
    {
        // to pause and restart the process
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (pause == false) { pause = true; }
            else { pause = false; }
        }

        //start the simulation
        // if ((OVRInput.GetDown(OVRInput.Button.Three) || Input.GetKeyDown(KeyCode.Space)) && simulationStarted)
        if(simulationStarted)
        {
                startStream(path);
                simulationStarted = false;
        }

        if (Time.fixedTime >= timeToCall && fileSize > 0 && readyToUpdate && pause == false)
        {
            //normalize positions
            if (index >= fileSize)
                index = 0;
            Normalize();
            index++;

            //update markers GOs positions
            foreach (string markername in markerNames)
            {
                GameObject markerGO = GameObject.Find(markername);
                if(!(markers[markername].x == 0 && markers[markername].y == 0 && markers[markername].z == 0)){
                    markerGO.GetComponent<Renderer>().enabled = true;
                    markerGO.transform.localPosition = new Vector3(markers[markername].x, markers[markername].y, markers[markername].z) + 1.5f * Vector3.up;
                }
                else{
                    //make marker invisible
                    markerGO.GetComponent<Renderer>().enabled = false;
                }
            }
            timeToCall = Time.fixedTime + timeDelay;
        }
    }

     //method to normalize coordinates in Unity scene
    private void Normalize()
    {
        foreach(string markername in markerNames)
        {
            Coordinate coord = new Coordinate();
            coord.x = markersToData[markername][index, 0] / 1000.0f;
            coord.z = markersToData[markername][index, 1] / 1000.0f; //z and y are swapped in unity
            coord.y = markersToData[markername][index, 2] / 1000.0f;
            markers[markername] = coord;

        }
    }

     //function to read the file with recorded MoCap data
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

      //method to extract coordinates from the file being read
    private void Extract(StreamReader reader)
    {
        string line;
        for (int i = 0; i < 5; i++)
            line = reader.ReadLine(); //skip headers
        line = reader.ReadLine(); //first line

        //extract info and distribute
        while (line != null && line != "") //interrupt at empty line or end of file
        {
            string[] temp = line.Split(separator.ToCharArray());
            if(temp.Length < 2)
                break;
            int runtimeField = Int32.Parse(temp[0]); //current array id

            //populate arrays
            time[runtimeField] = runtimeField / 100.0f;
            foreach(string markername in markerNames)
            {
                int i= Array.IndexOf(markerNames, markername);
                markersToData[markername][runtimeField, 0] = float.Parse(temp[3 * i + 1]);
                markersToData[markername][runtimeField, 1] = float.Parse(temp[3 * i + 2]);
                markersToData[markername][runtimeField, 2] = float.Parse(temp[3 * i + 3]);
            }

            line = reader.ReadLine();
        }

        if(line == null)
            Debug.Log("End of file reached");
    }

    private void ChangeSpeed()
    {
        timeDelay = slider.value;
    }


}
