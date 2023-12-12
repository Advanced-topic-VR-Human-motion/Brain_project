using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Transform))]
public class FABRIK : MonoBehaviour
{
    private GameObject rootObject;
    private FABRIKChain rootChain;

    private List<FABRIKChain> chains = new List<FABRIKChain>();
    private Dictionary<string, FABRIKChain> endChains = new Dictionary<string, FABRIKChain>();

    public void CreateSystem()
    {
        CreateSystem(transform);

        AssetDatabase.SaveAssets();
    }

    protected void CreateSystem(Transform transform)
    {
        if (transform.gameObject.GetComponent<FABRIKEffector>() == null)
        {
            FABRIKEffector effector = transform.gameObject.AddComponent<FABRIKEffector>();

            if(transform.parent != null)
            {
                effector.forwardAxisConstraint = transform.localPosition;
                effector.upAxisConstraint = Vector3.up;
            }

            Debug.Log(transform.gameObject.name + ": FABRIKEffector added.");
        }
        else
        {
            Debug.Log(transform.gameObject.name + ": FABRIKEffector already exists!");
        }
        
        if (transform.childCount == 0 && !transform.gameObject.name.Contains("_end_effector"))
        {
            GameObject gameObject = new GameObject(transform.gameObject.name + "_end_effector");

            gameObject.transform.parent = transform;

            MeshFilter meshFilter = transform.gameObject.GetComponent<MeshFilter>();

            if (meshFilter != null)
            {
                Bounds bounds = meshFilter.mesh.bounds;

                gameObject.transform.localPosition = bounds.center + bounds.extents;
            }
            else
            {
                gameObject.transform.localPosition = Vector3.forward;
            }

            Debug.Log(transform.gameObject.name + ": end effector added as " + gameObject.name);
        }

        foreach (Transform child in transform)
        {
            CreateSystem(child);
        }
    }

    public void Awake()
    {
        // Load our IK system from the root transform
        //call loadsystem with this as a parent
        rootChain = LoadSystem(transform);

        // Inversely sort by layer, greater-first
        chains.Sort(delegate (FABRIKChain x, FABRIKChain y) { return y.Layer.CompareTo(x.Layer); });

        foreach(FABRIKChain chain in chains)
        {
            chain.CalculateSummedWeight();
        }
    }

    private FABRIKChain LoadSystem(Transform transform, FABRIKChain parent = null, int layer = 0)
    {
        List<FABRIKEffector> effectors = new List<FABRIKEffector>();

        // Use parent chain's end effector as our sub-base effector, e.g:
        //                 [D]---[E]
        //        1       /    2        1 = [A, B, C]
        // [A]---[B]---[C]              2 = [C, D, E]
        //                \    3        3 = [C, F, G]
        //                 [F]---[G]
        if (parent != null)
        {
            effectors.Add(parent.EndEffector);
        }

        // childCount > 1 is a new sub-base
        // childCount = 0 is an end chain (added to our list below)
        // childCount = 1 is continuation of chain
        while (transform != null)
        {
            FABRIKEffector effector = transform.gameObject.GetComponent<FABRIKEffector>();

            if (effector == null)
            {
                break;
            }

            effectors.Add(effector);

            
            if (transform.childCount != 1)
            {
                break;
            }
            
            transform = transform.GetChild(0);
        }

        FABRIKChain chain = new FABRIKChain(parent, effectors, layer);

        chains.Add(chain);

        // Add to our end chain list if it is an end chain
        if (chain.IsEndChain)
        {
            endChains.Add(transform.gameObject.name, chain);
        }
        // Else iterate over each of the end effector's children to create a new chain in the layer above
        else foreach (Transform child in transform)
        {
            LoadSystem(child, chain, layer + 1);
        }

        return chain;
    }

    public void OnDestroy()
    {
        Destroy(rootObject);
    }

    public void Update()
    {
        OnFABRIK();
        Solve();

    }

    public virtual void OnFABRIK()
    {
        
    }

    public void Solve()
    {
        // We must iterate by layer in the first stage, working from target(s) to root
        foreach (FABRIKChain chain in chains)
        {
            chain.Backward();
        }

        // Provided our hierarchy, the second stage doesn't directly require an iterator
        rootChain.ForwardMulti();
    }

    public FABRIKChain GetEndChain(string name)
    {
        return endChains[name];
    }
}
