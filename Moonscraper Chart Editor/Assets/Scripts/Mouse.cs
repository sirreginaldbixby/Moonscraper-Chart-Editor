﻿using UnityEngine;
using System.Collections;

public class Mouse : MonoBehaviour {
    [Header("Viewing modes")]
    public Camera camera2D;
    public Camera camera3D;

    bool dragging;

    ChartEditor editor;
    GameObject selectedGameObject;
	
    public static Vector2? world2DPosition = null;

    void Start()
    {
        editor = GameObject.FindGameObjectWithTag("Editor").GetComponent<ChartEditor>();
    }

    Vector2 initMouseDragPos = Vector2.zero;
	// Update is called once per frame
	void Update () {

        GameObject objectUnderMouse = GetSelectableObjectUnderMouse();
        Vector2 viewportPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            world2DPosition = null;
        else
        {
            Vector3 screenPos = Input.mousePosition;
            float maxY = Camera.main.WorldToScreenPoint(editor.mouseYMaxLimit.position).y;

            // Calculate world2DPosition
            if (Input.mousePosition.y > maxY)
                screenPos.y = maxY;

            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            int layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
            RaycastHit[] planeHit;
            planeHit = Physics.RaycastAll(ray, Mathf.Infinity, layerMask);

            if (planeHit.Length > 0)
                world2DPosition = planeHit[0].point;
            else
                world2DPosition = null;
        }

        // OnSelectableMouseDown
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && world2DPosition != null)
        {
            initMouseDragPos = (Vector2)world2DPosition;

            selectedGameObject = objectUnderMouse;

            if (selectedGameObject)
            {
                SelectableClick[] monos = selectedGameObject.GetComponents<SelectableClick>();
                foreach (SelectableClick mono in monos)
                {
                    mono.OnSelectableMouseDown();
                }
            }
        }
        // OnSelectableMouseUp
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)))
        {
            dragging = false;

            selectedGameObject = null;
        }

        if ((Input.GetMouseButton(0) || Input.GetMouseButton(1)) && world2DPosition != null && world2DPosition != initMouseDragPos)
        {
            dragging = true;
        }

        // OnSelectableMouseDrag
        if (dragging && selectedGameObject)
        {
            SelectableClick[] monos = selectedGameObject.GetComponents<SelectableClick>();
            foreach (SelectableClick mono in monos)
            {
                mono.OnSelectableMouseDrag();
            }
        }

        // OnSelectableMouseOver
        if (objectUnderMouse)
        {
            SelectableClick[] mouseOver = objectUnderMouse.GetComponents<SelectableClick>();
            foreach (SelectableClick mono in mouseOver)
            {
                mono.OnSelectableMouseOver();
            }
        }
    }

    public void SwitchCamera()
    {
        if (camera2D.gameObject.activeSelf)
        {
            camera2D.gameObject.SetActive(false);
            camera3D.gameObject.SetActive(true);
        }
        else
        {
            camera3D.gameObject.SetActive(false);
            camera2D.gameObject.SetActive(true);
        }
    }

    static RaycastHit2D lowestY(RaycastHit2D[] hits)
    {
        RaycastHit2D lowestHit = hits[0];

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider.gameObject.transform.position.y < lowestHit.collider.gameObject.transform.position.y)
                lowestHit = hit;
        }

        return lowestHit;
    }

    static RaycastHit lowestY(RaycastHit[] hits)
    {
        RaycastHit lowestHit = hits[0];

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.transform.position.y < lowestHit.collider.gameObject.transform.position.y)
                lowestHit = hit;
        }

        return lowestHit;
    }

    static GameObject[] raySortLowestY(GameObject[] hits)
    {
        int length = hits.Length;

        for (int i = 1; i < length; i++)
        {
            int j = i;

            while ((j > 0) && (hits[j].transform.position.y < hits[j - 1].transform.position.y))
            {
                int k = j - 1;
                GameObject temp = hits[k];
                hits[k] = hits[j];
                hits[j] = temp;

                j--;
            }
        }

        return hits;
    }
    /*
    public static GameObject GetSongObjectUnderMouse()
    {
        if (world2DPosition != null)
        {
            LayerMask mask = 1 << LayerMask.NameToLayer("SongObject");
            RaycastHit[] hits3d = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, mask); //Physics.RaycastAll((Vector2)world2DPosition, Vector2.zero, 0, mask);
            RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)world2DPosition, Vector2.zero, 0, mask);

            if (hits3d.Length > 0)
            {
                RaycastHit lowestYHit = lowestY(hits3d);

                if (lowestYHit.collider)
                    return lowestYHit.collider.gameObject;
            }
            else if (hits.Length > 0)
            {
                RaycastHit2D lowestYHit = lowestY(hits);

                if (lowestYHit.collider)
                    return lowestYHit.collider.gameObject;
            }
        }

        return null;
    }*/

    public static GameObject GetSelectableObjectUnderMouse()
    {
        if (world2DPosition != null)
        {
            LayerMask mask;

            if (Globals.viewMode == Globals.ViewMode.Chart)
                mask = 1 << LayerMask.NameToLayer("ChartObject");
            else
                mask = 1 << LayerMask.NameToLayer("SongObject");

            RaycastHit[] hits3d = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), Mathf.Infinity, mask); //Physics.RaycastAll((Vector2)world2DPosition, Vector2.zero, 0, mask);
            RaycastHit2D[] hits = Physics2D.RaycastAll((Vector2)world2DPosition, Vector2.zero, 0, mask);

            GameObject[] hitGameObjects;

            if (hits3d.Length > 0)
            {
                hitGameObjects = new GameObject[hits3d.Length];
                for (int i = 0; i < hits3d.Length; ++i)
                    hitGameObjects[i] = hits3d[i].collider.gameObject;

                GameObject[] sortedObjects = raySortLowestY(hitGameObjects);

                foreach(GameObject selectedObject in sortedObjects)
                {
                    if (selectedObject.GetComponent<SelectableClick>())
                        return selectedObject;
                }
            }
            else if (hits.Length > 0)
            {
                hitGameObjects = new GameObject[hits.Length];

                for (int i = 0; i < hits.Length; ++i)
                    hitGameObjects[i] = hits[i].collider.gameObject;

                GameObject[] sortedObjects = raySortLowestY(hitGameObjects);

                foreach (GameObject selectedObject in sortedObjects)
                {
                    if (selectedObject.GetComponent<SelectableClick>())
                        return selectedObject;
                }
            }
        }

        return null;
    }
}

public class Draggable : MonoBehaviour
{
    public virtual void OnRightMouseDrag() { }
}
