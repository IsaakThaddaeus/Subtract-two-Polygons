using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon
{

    private int indexA;
    private int indexB;

    private List<List<Vector2>> outputPolygons = new List<List<Vector2>>();

    private List<Vector2> basisVerts;
    private List<Vector2> patternVerts;

    private List<verts> basis = new List<verts>();
    private List<verts> pattern = new List<verts>();

    private float sum;


    public List<List<Vector2>> cutpolygon(List<Vector2> basisInpt, List<Vector2> patternInpt)
    {
        outputPolygons.Clear();

        basisVerts = null;
        patternVerts = null;

        basisVerts = basisInpt;
        patternVerts = patternInpt;

        basis.Clear();
        pattern.Clear();

        createVertLists();
        subtract();

        return outputPolygons;
    }


    void createVertLists()
    {
        initializeVertList();
        intersectPolygons();
        insertPoint(basis, pattern);
        insertPoint(pattern, basis);
        setVertsCross();
        setVertsOutside();

        
        /*
        Debug.Log(basis.Count);

        
        foreach (verts vert in basis)
        {
            Debug.Log("Basis: " + vert.position + " | " + vert.cross + " | " + vert.outside);
        }

        foreach (verts vert in pattern)
        {
            Debug.Log("Pattern: " + vert.position + " | " + vert.cross + " | " + vert.outside);
        }
        */

    }
    void initializeVertList()
    {

        foreach (Vector2 v2 in basisVerts)
        {
            verts vert = new verts();
            vert.position = v2;
            basis.Add(vert);
        }

        foreach (Vector2 v2 in patternVerts)
        {
            verts vert = new verts();
            vert.position = v2;
            pattern.Add(vert);
        }
    }
    bool intersectPolygons()
    {
        for (int i = 0; i < basis.Count; i++)
        {
            for (int j = 0; j < pattern.Count; j++)
            {
                Vector2 intersection;
                bool inter = getIntersectionV2Cramer(getVertInList(basis, i).position, getVertInList(basis, i + 1).position, getVertInList(pattern, j).position, getVertInList(pattern, j + 1).position, out intersection);

                if (inter == true)
                {
                    verts vertBase = new verts();
                    vertBase.position = intersection;
                    vertBase.intersection = true;
                    basis.Insert(i + 1, vertBase);

                    verts vertPattern = new verts();
                    vertPattern.position = intersection;
                    vertPattern.intersection = true;
                    pattern.Insert(j + 1, vertPattern);

                    return intersectPolygons();
                }

            }
        }

        return true;

    }
    void setVertsCross()
    {
        for (int i = 0; i < basis.Count; i++)
        {
            for (int j = 0; j < pattern.Count; j++)
            {
                if (basis[i].position == pattern[j].position)
                {
                    basis[i].cross = j;
                    pattern[j].cross = i;
                }
            }
        }
    }
    void insertPoint(List<verts> acual, List<verts> other)
    {

        List<verts> newVerts = new List<verts>();
        List<int> newVertsIdex = new List<int>();

        for (int i = 0; i < acual.Count; i++)
        {
            verts v1 = getVertInList(acual, i);
            verts v2 = getVertInList(acual, i + 1);

            if (v1.intersection == true && v2.intersection == true)
            {
                Vector2 mid = v1.position + (v2.position - v1.position) / 2;

                if (insidePolygon(mid, other) == false)
                {
                    verts middle = new verts();
                    middle.position = mid;

                    newVerts.Add(middle);
                    newVertsIdex.Add(i + 1);
                }
            }
        }

        for (int i = 0; i < newVerts.Count; i++)
        {
            if (newVertsIdex[i] >= basis.Count)
            {
                acual.Add(newVerts[i]);
            }
            else
            {
                acual.Insert(newVertsIdex[i], newVerts[i]);
            }
        }


    }
    void setVertsOutside()
    {
        foreach (verts vert in basis)
        {
            if (vert.intersection == false && insidePolygon(vert.position, pattern) == false)
            {
                vert.outside = true;
            }
        }

        foreach (verts vert in pattern)
        {
            if (vert.intersection == false && insidePolygon(vert.position, basis) == false)
            {
                vert.outside = true;
            }
        }
    }


    //subtract - based on this paper: https://www.pnnl.gov/main/publications/external/technical_reports/PNNL-SA-97135.pdf page 2.5
    void subtract()
    {
        step1();

        /*
        foreach (Vector2 v2 in outputPolygons[0])
        {
            Debug.Log(v2);
        }
        */

    }
    void step1()
    {
        if (firstUnusedOutsideVert() == true)
        {
            outputPolygons.Add(new List<Vector2>());
            step2();
        }

    }
    void step2()
    {
        outputPolygons[outputPolygons.Count - 1].Add(basis[indexA].position);
        basis[indexA].processed = true;

        if (outputPolygons[outputPolygons.Count - 1][0] == basis[indexA].position && outputPolygons[outputPolygons.Count - 1].Count > 1)
        {

            List<Vector2> last = outputPolygons[outputPolygons.Count - 1];
            int lastint = last.Count - 1;

            outputPolygons[outputPolygons.Count - 1].RemoveAt(lastint);
            step1();
        }

        else
        {
            step3();
        }

    }
    void step3()
    {
        if (basis[indexA].cross == -1)
        {
            step9();
        }
        else
        {
            step4();
        }
    }
    void step4()
    {
        indexB = basis[indexA].cross;
        step5();
    }
    void step5()
    {
        decreaseIndex(ref indexB, pattern);
        step6();
    }
    void step6()
    {
        outputPolygons[outputPolygons.Count - 1].Add(pattern[indexB].position);
        step7();
    }
    void step7()
    {
        if (pattern[indexB].cross == -1)
        {
            step5();
        }
        else
        {
            step8();
        }
    }
    void step8()
    {
        indexA = pattern[indexB].cross;
        increaseIndex(ref indexA, basis);

        step2();
    }
    void step9()
    {
        increaseIndex(ref indexA, basis);
        step2();
    }

    void increaseIndex(ref int index, List<verts> list)
    {
        if (index + 1 >= list.Count)
        {
            index = 0;
        }
        else
        {
            index++;
        }
    }
    void decreaseIndex(ref int index, List<verts> list)
    {
        if (index - 1 < 0)
        {
            index = list.Count - 1;
        }

        else
        {
            index--;
        }
    }
    bool firstUnusedOutsideVert()
    {

        for (int i = 0; i < basis.Count; i++)
        {
            if (basis[i].outside == true && basis[i].processed == false)
            {
                indexA = i;
                return true;
            }
        }

        indexA = -1;
        return false;
    }


    bool insidePolygon(Vector2 aPoint, List<verts> bVertList)
    {
        sum = 0;

        for (int i = 0; i < bVertList.Count; i++)
        {
            Vector2 xA;
            Vector2 xB;

            if (i + 1 != bVertList.Count)
            {
                xA = bVertList[i].position - aPoint;
                xB = bVertList[i + 1].position - aPoint;
            }
            else
            {
                xA = bVertList[i].position - aPoint;
                xB = bVertList[0].position - aPoint;
            }

            sum += Vector2.SignedAngle(xA, xB);
        }


        if (sum < -359 && sum > -361)
        {
            return true;
        }

        else
        {
            return false;
        }
    }
    bool getIntersectionV2Cramer(Vector2 aStart, Vector2 aEnd, Vector2 bStart, Vector2 bEnd, out Vector2 intersection)
    {
        intersection = new Vector2(0, 0);

        Vector2 ab = aEnd - aStart;
        Vector2 cd = bEnd - bStart;

        //no intersection because of colinearity
        if (ab.normalized == cd.normalized || ab.normalized == -cd.normalized || -ab.normalized == cd.normalized)
        {
            return false;
        }


        //Solving equation by Cramers Rule
        float a1 = ab.x;
        float b1 = -cd.x;
        float c1 = bStart.x - aStart.x;

        float a2 = ab.y;
        float b2 = -cd.y;
        float c2 = bStart.y - aStart.y;

        float D = (a1 * b2) - (b1 * a2);
        float Dx = (c1 * b2) - (b1 * c2);
        float Dy = (a1 * c2) - (c1 * a2);

        float x = Dx / D;
        float y = Dy / D;

        intersection = aStart + x * (ab);


        //is intersectionpoint on Vector?
        float siA = Vector2.Distance(aStart, intersection);
        float ieA = Vector2.Distance(intersection, aEnd);
        float seA = Vector2.Distance(aStart, aEnd);

        float siB = Vector2.Distance(bStart, intersection);
        float ieB = Vector2.Distance(intersection, bEnd);
        float seB = Vector2.Distance(bStart, bEnd);


        if (siA + ieA > seA + 0.01 || siB + ieB > seB + 0.01)
        {
            return false;
        }

        else if (Vector2.Distance(intersection, aStart) < 0.01 || Vector2.Distance(intersection, aEnd) < 0.01 || Vector2.Distance(intersection, bStart) < 0.01 || Vector2.Distance(intersection, bEnd) < 0.01)
        {
            return false;
        }

        return true;
    }
    verts getVertInList(List<verts> vertlist, int index)
    {
        if (index >= vertlist.Count)
        {
            return vertlist[0];
        }

        return vertlist[index];
    }


    class verts
    {
        public Vector2 position;
        public bool outside;
        public int cross;
        public bool processed;

        public bool intersection;

        public verts()
        {
            cross = -1;
        }
    }

}
