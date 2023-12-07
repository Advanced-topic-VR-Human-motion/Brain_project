using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FABRIKHand: FABRIK
{
    public Transform [] FingerTips;
    public GameObject ConnectorPrefab;

    private float connectorScale = 40f;
    //NOTE: constraint is applied with respect to the parent, if the root joint has constraints it doesn't do anything anyways!

    /*
    NOTES FOR PRESENTATION / SPACE FOR IMPROVEMENT:
    - no distinction between global and local constraints (meaning, constraints relative to one another), only local constraints relative to the parent
    - constraint is the whole amplitude of rotation -> need to divide in half to get per direction
    - rotations of root joints need to be updated manually as the mocap data does not include those
    */


    void Start()
    {
        InstantiateConnectors();
    }
    public override void OnFABRIK ()
    {
        int numChains = FingerTips.Length;
        FABRIKChain [] ends = new FABRIKChain[numChains];
        for(int i = 0 ; i < numChains ; i++){
            string endEffectorName = "fingertip" + (i+1) + "_end_effector";
            ends[i] = GetEndChain(endEffectorName);
            ends[i].Target = FingerTips[i].position;
        }


        UpdateJointRotation();   
    }
    private void UpdateJointRotation(){
        Vector3 positionSum = Vector3.zero;
        
        foreach(Transform fingertip in FingerTips){
            positionSum += fingertip.position;
        }
        Vector3 averagePosition = positionSum / FingerTips.Length;
        Debug.Log("Average: " + averagePosition);

        Vector3 averagePositionCorrected = CalculateAveragePositionCorrected(averagePosition);

        Vector3 direction = averagePositionCorrected - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;

    }
    private Vector3 CalculateAveragePositionCorrected(Vector3 averagePosition){
        Vector3 averagePositionCorrected = Vector3.zero;
        int numFingerTipsToConsider = FingerTips.Length;
        foreach(Transform fingertip in FingerTips){
            Debug.Log("Distance: " + (fingertip.position - averagePosition).magnitude + " " + fingertip.name);
            if((fingertip.position - averagePosition).magnitude < 0.5f){
                averagePositionCorrected += fingertip.position;
            }else{
                numFingerTipsToConsider--;
                fingertip.position = averagePositionCorrected / numFingerTipsToConsider;
            }
        }

        averagePositionCorrected /= numFingerTipsToConsider;

        return averagePositionCorrected;
    }

    private void InstantiateConnectors(){
        int numChains = FingerTips.Length;
        
        //for every child in every chain, instatiate the connector prefab with origin in parent and end of mesh in child
        for(int i = 0 ; i < numChains ; i++){
            FABRIKChain chain = GetEndChain("fingertip" + (i+1) + "_end_effector");
            FABRIKEffector [] effectors = chain.Effectors.ToArray();
            for(int j = 0 ; j < effectors.Length - 2 ; j++){
                FABRIKEffector parent = effectors[j];
                FABRIKEffector child = effectors[j+1];
                Vector3 origin = parent.transform.position;
                Vector3 end = child.transform.position;
                Vector3 direction = end - origin;
                Vector3 scale = new Vector3(1f, 1f, direction.magnitude * connectorScale);
                Vector3 position = origin + direction / 2;
                Quaternion rotation = Quaternion.LookRotation(direction);
                GameObject connector = Instantiate(ConnectorPrefab);
                connector.transform.position = position;
                connector.transform.rotation = rotation;
                connector.transform.localScale = scale;
                connector.transform.parent = parent.transform;
            }
        }

    }
}
