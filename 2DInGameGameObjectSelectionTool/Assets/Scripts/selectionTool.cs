using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class selectionTool : MonoBehaviour {

    //NOTE: This Selection Tool Selects work EXCLUSIVELY with:
    //1. LEFT CLICKS
    //2. LEFT CTRL KEY (as modifier)

    //I am using in Large Part Code From Here http://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/

    ArrayList selectedObjects; //this will store all of our selected object

	// Use this for initialization
	void Start () {
        selectedObjects = new ArrayList();
	}

    //used to create the Rectangle every Frame with onGUI
    //used to finish the Lasso Selection if it isnt done on its own
    Vector3 initialScreenMousePos;
    Vector3 initialWorldMousePos;
    Vector3 finalScreenMousePos;
    Vector3 finalWorldMousePos;

    //these indicate which tool is being used when you are clicking the button
    bool rectTool;
    bool lassoTool;

    // Update is called once per frame
    void Update () {

        Vector3 screenMousePos;
        Vector3 worldMousePos;

        if (Input.GetMouseButtonDown(0)) //QUICK ACTION
        {
            Debug.Log("Down");

            //get the Position Of Our Mouse
            screenMousePos = Input.mousePosition;
            screenMousePos.z = transform.position.z - Camera.main.transform.position.z;
            worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);

            //save our Initial Position in Case We Are Using the Rect or Lasso Tool
            initialScreenMousePos = screenMousePos;
            initialWorldMousePos = worldMousePos;

            //raycast to try to find a gameobject in front of us
            RaycastHit2D hit = Physics2D.Raycast(new Vector2(worldMousePos.x, worldMousePos.y), Vector3.forward);

            if (hit.collider != null) //we are over a gameobject...
            {
                //the gameObject we have selected
                GameObject newGO = hit.collider.gameObject;

                if (selectedObjects.Count != 0) //if our array list is NOT empty we might have to delete objects
                {
                    GameObject old = (GameObject)selectedObjects[0];

                    if (old.name != newGO.name) //if we have selected a different TYPE of game object then delete it
                    {
                        ///TODO MAYBE... ask user whether they (REQUIRES SIGNIFICANT REWRITING OF CODE)
                        //1. Want to Select THIS... and DE select OTHER
                        //2. Want to DeSelect THIS...

                        foreach (GameObject go in selectedObjects)
                        {
                            //unhighlight the game object
                            go.GetComponent<SpriteOutline>().DeActivateOutline();
                        }

                        selectedObjects.Clear();
                    }
                    //else we simply need to add the game object
                }
                else
                {
                    //else we are switching to the selection tool and must therefore shut off all other tools that cause Conflicts
                }

                //we haven't selected this game object before... then select
                if (!selectedObjects.Contains(newGO))
                {
                    Debug.Log("Object Being Selected");
                    selectedObjects.Add(newGO);
                    newGO.GetComponent<SpriteOutline>().ActivateOutline();
                }
                else
                {
                    //else... deselect
                    selectedObjects.Remove(newGO);
                    newGO.GetComponent<SpriteOutline>().DeActivateOutline();
                }

            }
            else
            {
                //we want to make a selection box or a Lasso... 

                if (Input.GetKey(KeyCode.LeftControl))//if we are pressing control then Lasso
                {
                    lassoTool = true;
                }
                else //simply make a selection box
                {
                    rectTool = true;
                }
            }
        }
        else //LONG ACTION
        {
            //get the Position Of Our Mouse
            screenMousePos = Input.mousePosition;
            screenMousePos.z = transform.position.z - Camera.main.transform.position.z;
            worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);

            finalScreenMousePos = screenMousePos;
            finalWorldMousePos = worldMousePos;

            if (Input.GetMouseButton(0)) //Update the Lasso and Rectangle Tool
            {
                Debug.Log("Pressed");

                if (lassoTool == true)
                {
                    Debug.Log("Lasso Tool");
                }
                //ELSE for the Rectangle Tool We dont need to keep track of anything except what we are already keeping track of
            }
            else if (Input.GetMouseButtonUp(0)) //OutPut Results of Lasso and Rectangle Tool
            {
                Debug.Log("UP");

                if (rectTool == true)
                {
                    Debug.Log("Rect Tool CLOSE");

                    //code
                    //NOTE: the rectangle is built from the top left corner (topLeftCorner Coords, size.x, size.y)
                    Vector3 init = initialScreenMousePos;
                    Vector3 final = finalScreenMousePos;

                    //set top Left Corner Vars
                    float smallX = Mathf.Min(init.x, final.x);
                    float largeX = Mathf.Max(init.x, final.x);

                    //set bottom right Corner Vars
                    float smallY = Mathf.Min(init.y, final.y);
                    float largeY = Mathf.Max(init.y, final.y);

                    float halfWidth = (largeX - smallX)/2;
                    float halfHeight = (largeY - smallY)/2;
                    Vector2 centerPoint = Camera.main.WorldToViewportPoint(new Vector3(halfWidth, halfHeight));

                    Collider2D[] inRect = Physics2D.OverlapBoxAll(new Vector2(centerPoint.x,centerPoint.y),new Vector2(halfWidth,halfHeight),0);

                    Debug.Log("----------Collisions:" + inRect.Length);

                    rectTool = false;
                }
                else if (lassoTool == true)
                {
                    Debug.Log("Lasso Tool CLOSE");

                    //code

                    lassoTool = false;
                }
            }
        } 
    }


    void OnGUI()
    {
        Debug.Log("rect: " + rectTool);
        if (rectTool == true)
        {
            //NOTE: the rectangle is built from the top left corner (topLeftCorner Coords, size.x, size.y)
            Vector3 init = initialScreenMousePos;
            Vector3 final = finalScreenMousePos;

            //set top Left Corner Vars
            float smallX = Mathf.Min(init.x, final.x);
            float largeX = Mathf.Max(init.x, final.x);

            //set bottom right Corner Vars
            float smallY = Mathf.Min(Screen.height - init.y, Screen.height - final.y); 
            float largeY = Mathf.Max(Screen.height - init.y, Screen.height - final.y);

            //x,y for top left corner, height, width
            DrawScreenRect(new Rect(smallX, smallY, largeX - smallX , largeY - smallY), new Color(0.8f, 0.8f, 0.95f, 0.25f));
            DrawScreenRectBorder(new Rect(smallX, smallY, largeX - smallX, largeY - smallY), 2, new Color(0.8f, 0.8f, 0.95f));
        }
        else if(lassoTool == true)
        {

        }
    }

    //---Drawing Tools

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;
    public static void DrawScreenRect(Rect rect, Color color)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(rect, GUIContent.none, _staticRectStyle);
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }
}
