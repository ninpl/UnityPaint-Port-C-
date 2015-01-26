using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Painter : MonoBehaviour 
{
    //Public
    public Texture2D sourceBaseTex;
    public Drawing.Samples AntiAlias = Drawing.Samples.Samples4;
    public Tool tool = Tool.Brush;
    public Texture[] toolimgs;
    public Texture2D colorCircle;
    public float lineWidth = 1;
    public float strokeWidth = 1;
    public Color col = Color.white;
    public Color col2 = Color.white;
    public GUISkin gskin;
    public LineTool lineTool = new LineTool();
    public BrushTool brush = new BrushTool();
    public EraserTool eraser = new EraserTool();
    public Stroke stroke = new Stroke();
    public int zoom = 1;
    Drawing.BezierPoint[] BezierPoints;
    
    
    //Private
    private int tool2 = 1;
    private Texture2D baseTex;
    private Vector2 dragStart;
    private Vector2 dragEnd;
    private Vector2 preDrag;

    void Start()
    {
        baseTex = (Texture2D)Instantiate(sourceBaseTex);
    }

    void OnGUI()
    {
        GUI.skin = gskin;

        GUILayout.BeginArea(new Rect(5, 5, 100 + baseTex.width * zoom, baseTex.height * zoom), "", "Box");
        GUILayout.BeginArea(new Rect(0, 0, 100, baseTex.height * zoom));
        tool2 = GUILayout.Toolbar(tool2, toolimgs, "Tool");

        tool = (Tool)System.Enum.Parse(typeof(Tool), tool2.ToString());


        GUILayout.Label("Drawing Options");
        GUILayout.Space(10);
        switch (tool)
        {
            case Tool.Line:
                GUILayout.Label("Size " + Mathf.Round(lineTool.width * 10) / 10);
                lineTool.width = GUILayout.HorizontalSlider(lineTool.width, 0, 40);
                col = GUIControls.RGBCircle(col, "", colorCircle);
                break;
            case Tool.Brush:
                GUILayout.Label("Size " + Mathf.Round(brush.width * 10) / 10);
                brush.width = GUILayout.HorizontalSlider(brush.width, 0, 40);
                GUILayout.Label("Hardness " + Mathf.Round(brush.hardness * 10) / 10);
                brush.hardness = GUILayout.HorizontalSlider(brush.hardness, 0.1f, 50);
                col = GUIControls.RGBCircle(col, "", colorCircle);
                break;
            case Tool.Eraser:
                GUILayout.Label("Size " + Mathf.Round(eraser.width * 10) / 10);
                eraser.width = GUILayout.HorizontalSlider(eraser.width, 0, 50);
                GUILayout.Label("Hardness " + Mathf.Round(eraser.hardness * 10) / 10);
                eraser.hardness = GUILayout.HorizontalSlider(eraser.hardness, 1, 50);
                break;
        }

        if (tool == Tool.Line)
        {
            stroke.enabled = GUILayout.Toggle(stroke.enabled, "Stroke");
            GUILayout.Label("Stroke Width " + Mathf.Round(stroke.width * 10) / 10);
            stroke.width = GUILayout.HorizontalSlider(stroke.width, 0, lineWidth);
            GUILayout.Label("Secondary Color");
            col2 = GUIControls.RGBCircle(col2, "", colorCircle);
        }

        GUILayout.EndArea();
        GUI.DrawTexture(new Rect(100, 0, baseTex.width * zoom, baseTex.height * zoom), baseTex);
        GUILayout.EndArea();
    }

    void Update()
    {
        Rect imgRect = new Rect(5 + 100, 5, baseTex.width * zoom, baseTex.height * zoom);
        Vector2 mouse = Input.mousePosition;
        mouse.y = Screen.height - mouse.y;

        if (Input.GetKeyDown("t"))
        {
            test();
        }
        if (Input.GetKeyDown("mouse 0"))
        {

            if (imgRect.Contains(mouse))
            {
                if (tool == Tool.Vector)
                {
                    var m2 = mouse - new Vector2(imgRect.x, imgRect.y);
                    m2.y = imgRect.height - m2.y;
                    var bz = new ArrayList(BezierPoints);
                    bz.Add(new Drawing.BezierPoint(m2, m2 - new Vector2(50, 10), m2 + new Vector2(50, 10)));
                    BezierPoints = (Drawing.BezierPoint[])bz.ToArray();
                    Drawing.DrawBezier(BezierPoints, lineTool.width, col, baseTex);
                }

                dragStart = mouse - new Vector2(imgRect.x, imgRect.y);
                dragStart.y = imgRect.height - dragStart.y;
                dragStart.x = Mathf.Round(dragStart.x / zoom);
                dragStart.y = Mathf.Round(dragStart.y / zoom);

                dragEnd = mouse - new Vector2(imgRect.x, imgRect.y);
                dragEnd.x = Mathf.Clamp(dragEnd.x, 0, imgRect.width);
                dragEnd.y = imgRect.height - Mathf.Clamp(dragEnd.y, 0, imgRect.height);
                dragEnd.x = Mathf.Round(dragEnd.x / zoom);
                dragEnd.y = Mathf.Round(dragEnd.y / zoom);
            }
            else
            {
                dragStart = Vector3.zero;
            }

        }
        if (Input.GetKey("mouse 0"))
        {
            if (dragStart == Vector2.zero)
            {
                return;
            }
            dragEnd = mouse - new Vector2(imgRect.x, imgRect.y);
            dragEnd.x = Mathf.Clamp(dragEnd.x, 0, imgRect.width);
            dragEnd.y = imgRect.height - Mathf.Clamp(dragEnd.y, 0, imgRect.height);
            dragEnd.x = Mathf.Round(dragEnd.x / zoom);
            dragEnd.y = Mathf.Round(dragEnd.y / zoom);

            if (tool == Tool.Brush)
            {
                Brush(dragEnd, preDrag);
            }
            if (tool == Tool.Eraser)
            {
                Eraser(dragEnd, preDrag);
            }

        }
        if (Input.GetKeyUp("mouse 0") && dragStart != Vector2.zero)
        {
            if (tool == Tool.Line)
            {
                dragEnd = mouse - new Vector2(imgRect.x, imgRect.y);
                dragEnd.x = Mathf.Clamp(dragEnd.x, 0, imgRect.width);
                dragEnd.y = imgRect.height - Mathf.Clamp(dragEnd.y, 0, imgRect.height);
                dragEnd.x = Mathf.Round(dragEnd.x / zoom);
                dragEnd.y = Mathf.Round(dragEnd.y / zoom);
                Debug.Log("Draw Line");
                Drawing.NumSamples = AntiAlias;
                if (stroke.enabled)
                {
                    baseTex = Drawing.DrawLine(dragStart, dragEnd, lineTool.width, col, baseTex, true, col2, stroke.width);
                }
                else
                {
                    baseTex = Drawing.DrawLine(dragStart, dragEnd, lineTool.width, col, baseTex);
                }
            }
            dragStart = Vector2.zero;
            dragEnd = Vector2.zero;
        }
        preDrag = dragEnd;
    }

    void Brush(Vector2 p1, Vector2 p2)
    {
        Drawing.NumSamples = AntiAlias;
        if (p2 == Vector2.zero)
        {
            p2 = p1;
        }
        Drawing.PaintLine(p1, p2, brush.width, col, brush.hardness, baseTex);
        baseTex.Apply();
    }

    void Eraser(Vector2 p1, Vector2 p2)
    {
        Drawing.NumSamples = AntiAlias;
        if (p2 == Vector2.zero)
        {
            p2 = p1;
        }
        Drawing.PaintLine(p1, p2, eraser.width, Color.white, eraser.hardness, baseTex);
        baseTex.Apply();
    }

    void test()
    {
        float startTime = Time.realtimeSinceStartup;
        var w = 100;
        var h = 100;
        var p1 = new Drawing.BezierPoint(new Vector2(10, 0), new Vector2(5, 20), new Vector2(20, 0));
        var p2 = new Drawing.BezierPoint(new Vector2(50, 10), new Vector2(40, 20), new Vector2(60, -10));
        var c = new Drawing.BezierCurve(p1.main, p1.control2, p2.control1, p2.main);
        p1.curve2 = c;
        p2.curve1 = c;
        Vector2 elapsedTime = new Vector2((Time.realtimeSinceStartup - startTime) * 10, 0);
        float startTime2 = Time.realtimeSinceStartup;
        for (var i = 0; i < w * h; i++)
        {
            Mathfx.IsNearBezier(new Vector2(Random.value * 80, Random.value * 30), p1, p2, 10);
        }

        Vector2 elapsedTime2 = new Vector2((Time.realtimeSinceStartup - startTime2) * 10, 0);
        Debug.Log("Drawing took " + elapsedTime.ToString() + "  " + elapsedTime2.ToString());

    }

    //Class
    public class LineTool
    {
        public float width = 1;
    }

    public class EraserTool
    {
        public float width = 10;
        public float hardness = 20;
    }

    public class BrushTool
    {
        public float width = 10;
        public float hardness = 2;
        public float spacing = 2;
    }

    public class Stroke
    {
        public bool enabled = false;
        public float width = 1;
    }

    //Enums
    public enum Tool
    {
        None,
        Line,
        Brush,
        Eraser,
        Vector
    }
}
