using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraManager : MonoBehaviour {

    GameObject selectionToolGO;

	// Use this for initialization
	void Start () {
        selectionToolGO = GameObject.Find("gameManager").gameObject;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Material mat;

    void OnPostRender()
    {
        if (selectionToolGO.GetComponent<selectionTool>().lassoTool == true)
        {
            ArrayList points = selectionToolGO.GetComponent<selectionTool>().ourPoints;

            mat.SetPass(0);

            GL.Begin(GL.LINES);

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 start;
                Vector3 end;

                if (i == 0)
                {
                    start = (Vector3)points[points.Count - 1]; //last point to...
                    end = (Vector3)points[i]; //first
                }
                else
                {
                    start = (Vector3)points[i - 1];
                    end = (Vector3)points[i];
                }

                GL.Vertex3(start.x, start.y, start.z);
                GL.Vertex3(end.x, end.y, end.z);
            }

            GL.End();
        }
    }
}
