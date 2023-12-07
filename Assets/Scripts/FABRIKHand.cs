using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FABRIKHand: FABRIK
{
    public Transform [] FingerTips;
    //NOTE: constraint is applied with respect to the parent, if the root joint has constraints it doesn't do anything anyways!

    /*
    NOTES FOR PRESENTATION / SPACE FOR IMPROVEMENT:
    - no distinction between global and local constraints (meaning, constraints relative to one another), only local constraints relative to the parent
    - constraint is the whole amplitude of rotation -> need to divide in half to get per direction
    - rotations of root joints need to be updated manually as the mocap data does not include those
    */

    public override void OnFABRIK ()
    {
        for(int i = 0 ; i < FingerTips.Length ; i++){
            string endEffectorName = "fingertip" + (i+1) + "_end_effector";
            FABRIKChain chain = GetEndChain(endEffectorName);
            chain.Target = FingerTips[i].position;
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
}
