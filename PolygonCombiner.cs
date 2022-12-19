using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonCombiner : MonoBehaviour
{

    static private int indexA;
    static private int indexB;

    static private List<List<Vector2>> outputPolygons = new List<List<Vector2>>();

    static private List<Vector2> basisVerts;
    static private List<Vector2> patternVerts;

    static private List<verts> basis = new List<verts>();
    static private List<verts> pattern = new List<verts>();

    static private float sum;


    static public List<List<Vector2>> cutpolygon(List<Vector2> basisInpt, List<Vector2> patternInpt)
    {
        outputPolygons.Clear();

        basisVerts = new List<Vector2>(basisInpt);
        patternVerts = new List<Vector2>(patternInpt);

        basis.Clear();
        pattern.Clear();

        createVertLists();
        subtract();

        return outputPolygons;
    }
    static public List<List<Vector2>> addpolygon(List<Vector2> basisInpt, List<Vector2> patternInpt)
    {
        outputPolygons.Clear();

        basisVerts = new List<Vector2>(basisInpt);
        patternVerts = new List<Vector2>(patternInpt);

        basis.Clear();
        pattern.Clear();

        createVertLists();
        add();

        return outputPolygons;
    }

    static void createVertLists()
    {
        initializeVertList();
        intersectPolygons();
        insertPoint(basis, pattern);
        insertPoint(pattern, basis);
        setVertsCross();
        setVertsOutside();
    }
    static void initializeVertList()
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
    static bool intersectPolygons()
    {
        for (int i = 0; i < basis.Count; i++)
        {
            for (int j = 0; j < pattern.Count; j++)
            {
                Vector2 intersection;
                bool inter = Utills.getIntersectionV2Cramer(Utills.getItem<verts>(basis, i).position, Utills.getItem<verts>(basis, i + 1).position, Utills.getItem<verts>(pattern, j).position, Utills.getItem<verts>(pattern, j + 1).position, out intersection);

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
    static void setVertsCross()
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
    static void insertPoint(List<verts> acual, List<verts> other)
    {

        List<verts> newVerts = new List<verts>();
        List<int> newVertsIdex = new List<int>();

        for (int i = 0; i < acual.Count; i++)
        {
            verts v1 = Utills.getItem<verts>(acual, i);
            verts v2 = Utills.getItem<verts>(acual, i + 1);

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
    static void setVertsOutside()
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
    static void subtract()
    {
        step1();

        /*
        foreach (Vector2 v2 in outputPolygons[0])
        {
            Debug.Log(v2);
        }
        */

    }
    static void step1()
    {
        if (firstUnusedOutsideVert() == true)
        {
            outputPolygons.Add(new List<Vector2>());
            step2();
        }

    }
    static void step2()
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
    static void step3()
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
    static void step4()
    {
        indexB = basis[indexA].cross;
        step5();
    }
    static void step5()
    {
        decreaseIndex(ref indexB, pattern);
        step6();
    }
    static void step6()
    {
        outputPolygons[outputPolygons.Count - 1].Add(pattern[indexB].position);
        step7();
    }
    static void step7()
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
    static void step8()
    {
        indexA = pattern[indexB].cross;
        increaseIndex(ref indexA, basis);

        step2();
    }
    static void step9()
    {
        increaseIndex(ref indexA, basis);
        step2();
    }

    static void add()
    {
        step1Add();
    }
    static void step1Add()
    {
        if (firstUnusedOutsideVert() == true)
        {
            outputPolygons.Add(new List<Vector2>());
            step2Add();
        }

    }
    static void step2Add()
    {
        outputPolygons[outputPolygons.Count - 1].Add(basis[indexA].position);
        basis[indexA].processed = true;

        if (outputPolygons[outputPolygons.Count - 1][0] == basis[indexA].position && outputPolygons[outputPolygons.Count - 1].Count > 1)
        {

            List<Vector2> last = outputPolygons[outputPolygons.Count - 1];
            int lastint = last.Count - 1;

            outputPolygons[outputPolygons.Count - 1].RemoveAt(lastint);
            step1Add();
        }

        else
        {
            step3Add();
        }

    }
    static void step3Add()
    {
        if (basis[indexA].cross == -1)
        {
            step9Add();
        }
        else
        {
            step4Add();
        }
    }
    static void step4Add()
    {
        indexB = basis[indexA].cross;
        step5Add();
    }
    static void step5Add()
    {
        increaseIndex(ref indexB, pattern);
        step6Add();
    }
    static void step6Add()
    {
        outputPolygons[outputPolygons.Count - 1].Add(pattern[indexB].position);
        step7Add();
    }
    static void step7Add()
    {
        if (pattern[indexB].cross == -1)
        {
            step5Add();
        }
        else
        {
            step8Add();
        }
    }
    static void step8Add()
    {
        indexA = pattern[indexB].cross;
        increaseIndex(ref indexA, basis);

        step2Add();
    }
    static void step9Add()
    {
        increaseIndex(ref indexA, basis);
        step2Add();
    }

    static void increaseIndex(ref int index, List<verts> list)
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
    static void decreaseIndex(ref int index, List<verts> list)
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
    static bool firstUnusedOutsideVert()
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

    static bool insidePolygon(Vector2 aPoint, List<verts> bVertList)
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
