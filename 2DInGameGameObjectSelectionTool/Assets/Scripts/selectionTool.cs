using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
 * Author: Bryan Cancel
 * Status: Working but NOT optimized
*/ 

public class selectionTool : MonoBehaviour {

    /*
     * For this to WORK at ALL -> attach this Spript to an Empty gameObject at root of gameobjects in Hierarchy Window
     */

    /*
    * NOTE: This Selection Tool works EXCLUSIVELY with:
    * 1. LEFT CLICKS
    * 2. LEFT CTRL KEY (as modifier)
    * 3. LEFT ALT KEY (as modifier)
    * 
    * IF on GameObject (will Select IF deselected AND Deselect IF selected)
    *   click -> to (select OR deselect) 1 object
    *   ctrl + click -> to (add OR remove) objects to your list of selections
    * ELSE (will allways select)
    *   alt + click + drag -> Rectangular Selection Tool
    *   ctrl + click + drag -> Lasso Selection Tool
    */

    /*
     * NOTE: To Maintain the SelectedObjects Array you Need These things Attached to your GameObjects
     * For (1) Selection (2) Rectangle Selection Tool -> your gameobjects NEED a Collider2D
     * For (3) Lasso Tool -> your gameobjects NEED a RigidBody2D (CANT HAVE TYPE Static)
    */

    /*
     * NOTE: To Keep the Graphical Effects you NEED to have...
     ***For 2D Sprite Outline on a GameObject http://nielson.io/2016/04/2d-sprite-outlines-in-unity/
     * (1) in Sprite Render Component
     *      (a) have sprite
     *      (b) have "SpriteOutline" Material
     * (2) "SpriteOutline" Script
     * (3) "SpriteOutline" Shader
     ***For Visual Dispaly of Lasso
     * (1) cameraManager Script Attched to the Main Camera
    */

    //NOTE: this implementation of the lasso tool has SIGNIFICANT limitations... but For my purposes (Selecting 100 objects or less) its works well
    
    //NOTE: you can chosse to REQUIRE all objects that are selected to be of the same type by modifying the canSelectDifTypes boolean

    ArrayList selectedObjects; //this will store all of our selected object

    //used to create the Rectangle every Frame with onGUI
    //used to finish the Lasso Selection
    Vector3 initialScreenMousePos;
    Vector3 initialWorldMousePos;
    Vector3 finalScreenMousePos;
    Vector3 finalWorldMousePos;

    //these indicate which tool is being used when you are clicking the button
    public bool rectTool;
    public bool lassoTool;
    public ArrayList ourPoints; //keep track of ourpoints when using the lasso tool

    //determines if you CAN or CAN't select object of different Types
    public bool canSelectDifTypes;

    //the two variables we use to not save points with the lasso tool every single frame (rather we save the point every couple of seconds)
    private float nextActionTime;
    public float period;

    // Use this for initialization
    void Start () {
        selectedObjects = new ArrayList();
        rectTool = false;
        lassoTool = false;
        ourPoints = new ArrayList();
        nextActionTime = 0.0f;
        period = 0.1f;

        canSelectDifTypes = false;
    }

    // Update is called once per frame
    void Update () {

        Vector3 screenMousePos;
        Vector3 worldMousePos;

        if (Input.GetMouseButtonDown(0)) //QUICK ACTION
        {
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

                            if (!canSelectDifTypes)
                            {
                                //NOTE: I defined type by TAG... feel free to define type by label or layer... etc...
                                //if we have selected a different TYPE of game object then delete all the previous objects in the list
                                if (old.tag != newGO.tag)
                                {
                                    //TODO LET THE USER KNOW WHY THE SELECTION WASNT MADE

                                    deselectAll();
                                }
                            }
                            //ELSE we can select different types...
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
                    deselectAll();
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
                if (lassoTool == true)
                {
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
                if (rectTool == true)
                {
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

                    //select all dogs in Rectangle Area
                    selectAll(inRect);

                    rectTool = false;
                }
                else if (lassoTool == true)
                {
                    //create polygonal collider
                    this.gameObject.AddComponent<PolygonCollider2D>();
                    Vector2[] v2Arr = objArrayToVector2Array(ourPoints.ToArray());
                    GetComponent<PolygonCollider2D>().SetPath(0, v2Arr);
                    GetComponent<PolygonCollider2D>().isTrigger = true;

                    polyColliderExist = true; //indicate this to our fixedUpdate

                    ourPoints.Clear();

                    lassoTool = false;
                }
            }
        } 
    }

    //---custom casting Functions

    Vector2[] objArrayToVector2Array(System.Object[] arr) //the array MUST be a bunch of Vector 2 objects
    {
        Vector2[] newArr = new Vector2[arr.Length];

        for (int i = 0; i < arr.Length; i++)
        {
            Vector3 temp = (Vector3)arr[i];
            newArr[i] = new Vector2(temp.x, temp.y);
        }

        return newArr;
    }

    Collider2D[] objArrayToCollider2DArray(System.Object[] arr)
    {
        Collider2D[] newArr = new Collider2D[arr.Length];

        for (int i = 0; i < arr.Length; i++)
            newArr[i] = (Collider2D)arr[i];

        return newArr;
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

    void selectAll(Collider2D[] colliders) //Rectangular and Lasso Tool helper
    {
        if (colliders.Length != 0)
        {
            bool addingSameType = true;

            if (!canSelectDifTypes)
            {
                //if (we already have previously selected objects) we MUST make sure the objects we are adding...
                //are of the same type as the objects already selected FIRST
                if (selectedObjects.Count != 0)
                {
                    foreach (Collider2D col in colliders)
                    {
                        if (((GameObject)selectedObjects[0]).tag != col.gameObject.tag)
                        {
                            addingSameType = false;
                            break;
                        }
                    }

                    //if adding sameType is FALSE here we know that selectedObject AND colliders are all NOT of the same type
                    if (!addingSameType)
                        deselectAll();

                    //BUT all the colliders might still be of the same type
                }

                //make sure all the colliders in our current selection are of the same type
                GameObject firstObj = colliders[0].gameObject;
                foreach (Collider2D col in colliders)
                {
                    if (firstObj.tag != col.gameObject.tag)
                    {
                        addingSameType = false;
                        break;
                    }
                }
            }
            //ELSE we can select different types so just select the object

            //now we know what we need to know to select or not select the objects
            if (addingSameType)
            {
                foreach (Collider2D col in colliders)
                {
                    GameObject newGameObj = col.gameObject;
                    if (selectedObjects.Contains(newGameObj) == false)
                    {
                        newGameObj.GetComponent<SpriteOutline>().ActivateOutline();
                        selectedObjects.Add(newGameObj);
                    }
                    //ELSE we already selected this object
                }
            }
            else
            {
                //TODO LET THE USER KNOW WHY THE SELECTION WASNT MADE
            }

            collider2DArr.Clear();
        }
    }

    //---These are needed for our polygon collider to do its job after the Lasso Tool Has been Let Go

    /*
     * Strange but Functional "Hack" for Lasso Tool
     * 1. Update Function creates the 2dpolygoncollider/trigger on the indicated points
     * 
     * 2. fixed update detects that a polycollider exist
     * 3. it changes polycollider to false
     * 4. IF a collision is detected ontrigger enter will run, adding that gameobject to a list
     * 5. it then changes the polycollider to true
     * 
     * REPEAT 2->5 for as many objects as needed
     * 
     * 6. Finally when fixedupdate detects that polycollider doesn't exist
     * 7. it selects all objects within the area
     * 8. it deletes the polygon collider
     * 
     * 9. (REPEAT UNTIL LASSO USED AGAIN) the fixedupdate will detect that a polycollider DOESN'T exist, then realize it has no polycollider to delete and continue
     */

    //NOTE: because of the step taken above... if you are select many objects there might be a significant lag time...
    //EX: if fixedupdate runs 60 times per second, and you have found 600 objects inside of your lasso...
    //it will take 600 runs of fixed update to register all the updates or 600/60 = 10 seconds

    bool polyColliderExist = false; //this variable is used in a very strange way described above
    ArrayList collider2DArr = new ArrayList(); //keeps track of all the collisions found in or touching our polygon collider

    void FixedUpdate()
    {
        if (polyColliderExist)
        {
            polyColliderExist = false;
        }
        else
        {
            //our polygon collider should be deleted because onTriggerEnter Wasn't called last time since If it had been we would have entered the if statement

            if (GetComponent<PolygonCollider2D>() != null)
            {
                //select Collisions in Lasso
                Collider2D[] temp = objArrayToCollider2DArray(collider2DArr.ToArray());
                selectAll(temp);

                //delete the polygon Collider
                Destroy(GetComponent<PolygonCollider2D>());
            }
            //ELSE its just a regular iteration of FixedUpdate
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        collider2DArr.Add(other);
        polyColliderExist = true;
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
