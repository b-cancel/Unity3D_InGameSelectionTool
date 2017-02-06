using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class selectionTool : MonoBehaviour {

    //NOTE: This Selection Tool Selects work EXCLUSIVELY with:
    //1. LEFT CLICKS
    //2. LEFT CTRL KEY (as modifier)

    ArrayList selectedObjects; //this will store all of our selected object

    //used to create the Rectangle every Frame with onGUI
    //used to finish the Lasso Selection if it isnt done on its own
    Vector3 initialScreenMousePos;
    Vector3 initialWorldMousePos;
    Vector3 finalScreenMousePos;
    Vector3 finalWorldMousePos;

    //these indicate which tool is being used when you are clicking the button
    public bool rectTool;
    public bool lassoTool;
    public ArrayList ourPoints;

    // Use this for initialization
    void Start () {
        selectedObjects = new ArrayList();
        rectTool = false;
        lassoTool = false;
        ourPoints = new ArrayList();
    }

    //the two variables we use to not save points with the lasso tool every single frame (rather we save the point every couple of seconds)
    private float nextActionTime = 0.0f;
    public float period = 0.1f;

    // Update is called once per frame
    void Update () {

        //--- Taking Care of what happens when we click on our mouse

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

                Debug.Log("Over a GO");

                //the gameObject we have selected
                GameObject newGO = hit.collider.gameObject;

                if (!selectedObjects.Contains(newGO))//we want to add the object to our selection group
                {
                    if (selectedObjects.Count != 0) //if our array list is NOT empty we might have to delete objects
                    {
                        if (!Input.GetKey(KeyCode.LeftControl)) //we dont want to multiple select
                        {
                            deselectAll();
                        }
                        else //we want to MULTIPLE select
                        {
                            //NOTE: we want to maintain a selectedList of objects of the same type
                            GameObject old = (GameObject)selectedObjects[0];

                            //NOTE: I defined type by name(works well with programatically cloned objects)... feel free to define type by label or layer... etc...
                            //if we have selected a different TYPE of game object then delete all the previous objects in the list
                            if (old.tag != newGO.tag)
                            {
                                ///TODO MAYBE... ask user whether they (REQUIRES SIGNIFICANT REWRITING OF CODE)
                                //1. Want to Select THIS... and DE select OTHERS
                                //2. or Keep OTHERS
                                deselectAll();
                            }
                        }
                    }
                    //ELSE this is our first selection
                }
                //ELSE we simply deselect it...

                selectOrDeselectDepends(newGO);
            }
            else
            {
                //we want to make a selection box or a Lasso... this is taken care off by tracking the pointer below

                if (Input.GetKey(KeyCode.LeftControl))//if we are pressing control then Lasso
                {
                    lassoTool = true;
                    ourPoints.Add(initialWorldMousePos);
                }
                else if(Input.GetKey(KeyCode.LeftAlt))//simply make a selection box
                {
                    rectTool = true;
                }
                else
                {
                    Debug.Log(selectedObjects.Count);
                    deselectAll();
                    Debug.Log(selectedObjects.Count);
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

                    //add this particular point every couple nextUpdate time
                    if (Time.time > nextActionTime)
                    {
                        nextActionTime = Time.time + period;
                        ourPoints.Add(finalWorldMousePos);
                    }
                }
                //ELSE for the Rectangle Tool We dont need to keep track of anything except what we are already keeping track of
            }
            else if (Input.GetMouseButtonUp(0)) //OutPut Results of Lasso and Rectangle Tool
            {
                Debug.Log("UP");

                if (rectTool == true)
                {
                    Debug.Log("Rect Tool CLOSE");

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

                    //Using OverlapArea
                    Collider2D[] inRect = Physics2D.OverlapAreaAll(initialWorldMousePos, finalWorldMousePos);
                    Debug.Log("----------Collisions:" + inRect.Length);

                    //select all dogs in Rectangle Area
                    selectAll(inRect);

                    rectTool = false;
                }
                else if (lassoTool == true)
                {
                    Debug.Log("Lasso Tool CLOSE");

                    //create polygonal collider
                    this.gameObject.AddComponent<PolygonCollider2D>();
                    Vector2[] v2Arr = Array.ConvertAll(ourPoints.ToArray(), item => (Vector2)item); //TODO this casting isnt working... -_-
                    GetComponent<PolygonCollider2D>().SetPath(0, v2Arr);
                    GetComponent<PolygonCollider2D>().isTrigger = true;

                    runCounter = 1; //indicate this to our fixedUpdate

                    ourPoints.Clear();

                    lassoTool = false;
                }
            }
        } 
    }

    //---helper Functions for Update Function

    //this function SELECTS an object IF it hasn't been selected
    //and DE-SELECTS an object IF its has been selected
    void selectOrDeselectDepends(GameObject GO)
    {
        if (selectedObjects.Contains(GO)) //if selected -> DE-SELECT
        {
            selectedObjects.Remove(GO.gameObject);
            GO.gameObject.GetComponent<SpriteOutline>().DeActivateOutline();
        }
        else //if NOT selected -> SELECT
        {
            selectedObjects.Add(GO.gameObject);
            GO.gameObject.GetComponent<SpriteOutline>().ActivateOutline();
        }
    }

    void deselectAll()
    {
        for(int i=0; i<selectedObjects.Count; i++)
            ((GameObject)selectedObjects[i]).GetComponent<SpriteOutline>().DeActivateOutline();
        selectedObjects.Clear();
    }

    void selectAll(ArrayList colliders)
    {
        selectAll(Array.ConvertAll(colliders.ToArray(), item => (Collider2D)item)); //TODO this casting isnt working... -_-
    }

    void selectAll(Collider2D[] colliders)
    {
        foreach (Collider2D col in colliders)
        {
            (col.gameObject).GetComponent<SpriteOutline>().ActivateOutline();
            selectedObjects.Add(col.gameObject);
        }
    }

    //---These are needed for our polygon collider to do its job after the Lasso Tool Has been Let Go

    /*
     * 1. Update Function create the 2dpolygoncollider/trigger on the indicated points
     * 2. fixed update runs
     * 3. onTriggerEnter runs (hopefully as many times as the collisions in it)
     * 4. fixed Update runs.... handle all the collisions found and select them... now the collider isnt needed so its deleted
     */
    int runCounter = 0; //if 0 nothing happens... if 1 we have created the collider but Not yet saved collisions... if 2 we must select collisions and delete collider
    ArrayList collider2DArr; //keeps track of all the collisions found in or touching our polygon collider

    void FixedUpdate()
    {
        if (runCounter != 0)
        {
            if (runCounter == 2)
            {
                //select Collisions in Lasso
                selectAll(collider2DArr);

                //delete the polygon Collider
                Destroy(GetComponent<PolygonCollider2D>());

                runCounter = 0; //we are back at our regular state
            }
            //ELSE we need to wait for onTriggerEnter to run and add our Collisions to the Collider2D list
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Thing in Lasso");
        collider2DArr.Add(other);
    }

    //---OnGUI is used to draw the visible Rectangle When using the Rect Selection Tool

    void OnGUI()
    {
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
        //NOTE: the Lines for the Lasso Tool are Taken care of in the cameraManager Script
    }

    //---Drawing Tools

    //Taken From http://hyunkell.com/blog/rts-style-unit-selection-in-unity-5/
    //and https://forum.unity3d.com/threads/draw-a-simple-rectangle-filled-with-a-color.116348/

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
