using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// http://sourceforge.net/p/urgnetwork/wiki/top_jp/
// https://www.hokuyo-aut.co.jp/02sensor/07scanner/download/pdf/URG_SCIP20.pdf
using System.Reflection.Emit;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using Uniduino.Examples;
public class URGSample : MonoBehaviour
{

    class DetectObject
    {
        public List<long> distList;
        public List<int> idList;

        public long startDist;

        public DetectObject()
        {
            distList = new List<long>();
            idList = new List<int>();
        }
    }

    [SerializeField]
    string ip_address = "192.168.0.10";
    [SerializeField]
    int port_number = 10940;

    List<DetectObject> detectObjects;
    List<int> detectIdList;

    private Vector3[] directions;
    private bool cached = false;

    public UrgDeviceEthernet urg;
    public float scale = 0.1f;
    public float limit = 100000.0f;//mm
    public int noiseLimit = 20;

    public Color distanceColor = Color.white;
    //	public Color detectColor = Color.white;
    public Color strengthColor = Color.white;

    public Color[] groupColors;

    List<long> distances;
    List<long> strengths;

    public Rect areaRect;

    public bool debugDraw = false;
    public bool ifDetectCollision = true;
    int drawCount;
    protected Rect _detectAreaRect;
    public Color detecColor = Color.red;

    //decide by led pin number
    [HideInInspector]public int detectRegionNumber = 20;
    public UniduinoTestPanel testPanel;
    //public int detect
    private int singleLenth;
    private bool[] DetecRegions = new bool[20];
    public bool[] getDetecRegions
    {
        get
        {
            return DetecRegions;
        }
    }

    public Rect detectAreaRect
    {
        get
        {
            return _detectAreaRect;
        }
    }
    public int threshold = 50;
    [Range(0,1)]public float percentageDynamicThreshold = 0.6f;
    private int[] regionPointCount;
    // Use this for initialization
    void Start()
    {
        distances = new List<long>();
        strengths = new List<long>();

        urg = this.gameObject.AddComponent<UrgDeviceEthernet>();
        urg.StartTCP(ip_address, port_number);

        if (ifDetectCollision)
        {
            if (testPanel == null) Debug.LogError("testPannel is null");
            detectRegionNumber = testPanel.LedPin.Length;
            DetecRegions = new bool[detectRegionNumber];
            regionPointCount = new int[detectRegionNumber];
            for (int i = 0; i < DetecRegions.Length; i++)
            {
                DetecRegions[i] = false;
                regionPointCount[i] = 0;
            }
        }

    }
    public Text text;

    // Update is called once per frame
    void Update()
    {

        if (gd_loop)
        {
            urg.Write(SCIP_library.SCIP_Writer.GD(0, 1080));
        }

        // center offset rect
        Rect detectAreaRect = areaRect;
        detectAreaRect.x *= scale;
        var x = detectAreaRect.x;
        detectAreaRect.y *= scale;
        detectAreaRect.width *= scale;
        detectAreaRect.height *= scale;

        detectAreaRect.x = -detectAreaRect.width / 2 + x;
        //
        singleLenth = (int)Mathf.Ceil(detectAreaRect.width / detectRegionNumber);

        if (ifDetectCollision)
        {
            for (int i = 0; i < DetecRegions.Length; i++)
            {
                regionPointCount[i] = 0;
                DetecRegions[i] = false;
            }
        }

        float d = Mathf.PI * 2 / 1440;
        float offset = d * 540;

        // cache directions
        if (urg.distances.Count > 0)
        {
            if (!cached)
            {
                directions = new Vector3[urg.distances.Count];
                Debug.Log("urg.distances.Count" + urg.distances.Count);
                for (int i = 0; i < directions.Length; i++)
                {
                    float a = d * i + offset;
                    directions[i] = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
                }
                cached = true;
            }
        }

        // strengths
        try
        {
            if (urg.strengths.Count > 0)
            {
                strengths.Clear();
                strengths.AddRange(urg.strengths);
            }
        }
        catch
        {
        }
        // distances
        try
        {
            if (urg.distances.Count > 0)
            {
                distances.Clear();
                distances.AddRange(urg.distances);
            }
        }
        catch
        {
        }
        //		List<long> distances = urg.distances;
        List<Vector3> endPoints = new List<Vector3>();

        if (debugDraw)
        {
            // strengths
            for (int i = 0; i < strengths.Count; i++)
            {
                //float a = d * i + offset;
                //Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
                Vector3 dir = directions[i];
                long dist = strengths[i];
                Debug.DrawRay(Vector3.zero, Mathf.Abs(dist) * dir * scale, strengthColor);
            }

            // distances
            //			float colorD = 1.0f / 1440;
            for (int i = 0; i < distances.Count; i++)
            {
                Vector3 dir = directions[i];
                long dist = distances[i];
                Vector3 endPoint = dist * dir * scale;
                Debug.DrawRay(Vector3.zero, endPoint, distanceColor);
                endPoints.Add(endPoint);
            }
        }

        //-----------------
        //  group
        detectObjects = new List<DetectObject>();
#region not use
        //
        //------
        //		bool endGroup = true;
        //		for(int i = 0; i < distances.Count; i++){
        //			int id = i;
        //			long dist = distances[id];
        //
        //			float a = d * i + offset;
        //			Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
        //
        //			if(dist < limit && dir.y > 0){
        //				DetectObject detect;
        //				if(endGroup){
        //					detect = new DetectObject();
        //					detect.idList.Add(id);
        //					detect.distList.Add(dist);
        //
        //					detect.startDist = dist;
        //					detectObjects.Add(detect);
        //					
        //					endGroup = false;
        //				}else{
        //					detect = detectObjects[detectObjects.Count-1];
        //					detect.idList.Add(id);
        //					detect.distList.Add(dist);
        //
        //					if(dist > detect.startDist){
        //						endGroup = true;
        //					}
        //				}
        //			}else{
        //				endGroup = true;
        //			}
        //		}

        //------
        //		bool endGroup = true;
        //		for(int i = 1; i < distances.Count-1; i++){
        //			long dist = distances[i];
        //			float delta = Mathf.Abs((float)(distances[i] - distances[i-1]));
        //			float delta1 = Mathf.Abs((float)(distances[i+1] - distances[i]));
        //			
        //			float a = d * i + offset;
        //			Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
        //			
        //			if(dir.y > 0){
        //				DetectObject detect;
        //				if(endGroup){
        //					if(dist < limit && delta > 50){
        //						detect = new DetectObject();
        //						detect.idList.Add(i);
        //						detect.distList.Add(dist);
        //						
        //						detect.startDist = dist;
        //						detectObjects.Add(detect);
        //						
        //						endGroup = false;
        //					}
        //				}else{
        //					if(delta < 50){
        //						detect = detectObjects[detectObjects.Count-1];
        //						detect.idList.Add(i);
        //						detect.distList.Add(dist);
        //					}else{
        //						endGroup = true;
        //					}
        //				}
        //			}
        //		}
        /*

        //------
        bool endGroup = true;
        float deltaLimit = 100000.0f; // 認識の閾値　連続したもののみを取得するため (mm)
        for (int i = 1; i < distances.Count - 1; i++)
        {
            //float a = d * i + offset;
            //Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
            Vector3 dir = directions[i];
            long dist = distances[i];

            float delta = Mathf.Abs((float)(distances[i] - distances[i - 1]));
            float delta1 = Mathf.Abs((float)(distances[i + 1] - distances[i]));

            if (dir.y > 0)
            {
                DetectObject detect;
                if (endGroup)
                {
                    Vector3 pt = dist * dir * scale;
                    if (dist < limit && (delta < deltaLimit && delta1 < deltaLimit))
                    {
                        //					bool isArea = detectAreaRect.Contains(pt);
                        //					if(isArea && (delta < deltaLimit && delta1 < deltaLimit)){
                        detect = new DetectObject();
                        detect.idList.Add(i);
                        detect.distList.Add(dist);

                        detect.startDist = dist;
                        detectObjects.Add(detect);

                        endGroup = false;
                    }
                }
                else
                {
                    if (delta1 >= deltaLimit || delta >= deltaLimit)
                    {
                        endGroup = true;
                    }
                    else
                    {
                        detect = detectObjects[detectObjects.Count - 1];
                        detect.idList.Add(i);
                        detect.distList.Add(dist);
                    }
                }
            }
        }
        */
        /*
        #region draw
        //-----------------
        // draw 
        drawCount = 0;
        for (int i = 0; i < detectObjects.Count; i++)
        {
            DetectObject detect = detectObjects[i];

            // noise
            if (detect.idList.Count < noiseLimit)
            {
                continue;
            }

            int offsetCount = detect.idList.Count / 3;
            int avgId = 0;
            for (int n = 0; n < detect.idList.Count; n++)
            {
                avgId += detect.idList[n];
            }
            avgId = avgId / (detect.idList.Count);

            long avgDist = 0;
            for (int n = offsetCount; n < detect.distList.Count - offsetCount; n++)
            {
                avgDist += detect.distList[n];
            }
            avgDist = avgDist / (detect.distList.Count - offsetCount * 2);

            //float a = d * avgId + offset;
            //Vector3 dir = new Vector3(-Mathf.Cos(a), -Mathf.Sin(a), 0);
            Vector3 dir = directions[avgId];
            long dist = avgDist;


            //float a0 = d * detect.idList[offsetCount] + offset;
            //Vector3 dir0 = new Vector3(-Mathf.Cos(a0), -Mathf.Sin(a0), 0);
            int id0 = detect.idList[offsetCount];
            Vector3 dir0 = directions[id0];
            long dist0 = detect.distList[offsetCount];

            //float a1 = d * detect.idList[detect.idList.Count-1 - offsetCount] + offset;
            //Vector3 dir1 = new Vector3(-Mathf.Cos(a1), -Mathf.Sin(a1), 0);
            int id1 = detect.idList[detect.idList.Count - 1 - offsetCount];
            Vector3 dir1 = directions[id1];
            long dist1 = detect.distList[detect.distList.Count - 1 - offsetCount];

            Color gColor;
            if (drawCount < groupColors.Length)
            {
                gColor = groupColors[drawCount];
            }
            else
            {
                gColor = Color.green;
            }
            for (int j = offsetCount; j < detect.idList.Count - offsetCount; j++)
            {
                //float _a = d * detect.idList[j] + offset;
                //Vector3 _dir = new Vector3(-Mathf.Cos(_a), -Mathf.Sin(_a), 0);
                int _id = detect.idList[j];
                Vector3 _dir = directions[_id];
                long _dist = detect.distList[j];
                Debug.DrawRay(Vector3.zero, _dist * _dir * scale, gColor);
            }

            Debug.DrawLine(dist0 * dir0 * scale, dist1 * dir1 * scale, gColor);
            Debug.DrawRay(Vector3.zero, dist * dir * scale, Color.green);

            drawCount++;
        }

*/
        #endregion
        if (ifDetectCollision)
        {
            text.text = "";
            int lastVal;
            int maxVal = 0;
            int minVal = threshold;

            for (int i = 0; i < endPoints.Count; i++)
            {
                if (detectAreaRect.Contains(endPoints[i]))
                {
                    int num = SingleDtectRect(endPoints[i].x, detectAreaRect.xMin);
                    regionPointCount[num - 1] += 1;
                    Debug.DrawRay(Vector3.zero,endPoints[i], Color.yellow);
                }
            }
            text.text += "DetecRegions.Length : " + DetecRegions.Length +"\n";
            text.text += "detectAreaRect.xMin : " + detectAreaRect.xMin + "\n";
            for (int i = 0; i < DetecRegions.Length;i++)
            {
                if(regionPointCount[i] > threshold)
                {
                    lastVal = regionPointCount[i];
                    if(lastVal > maxVal)
                    {
                        maxVal = lastVal;
                    }
                    regionPointCount[i] -= minVal;
                    //DetecRegions[i] = true;
                }
                else{
                    regionPointCount[i] = 0;
                    //DetecRegions[i] = false;
                }

            }
            threshold = (int)Mathf.Floor((maxVal - minVal) * percentageDynamicThreshold);
            for (int i = 0; i < DetecRegions.Length; i++)
            {
                if (regionPointCount[i] > threshold)
                { 
                    DetecRegions[i] = true;
                }
                else{
                    DetecRegions[i] = false;
                }
                text.text += "Region " + (i) + " : " + DetecRegions[i] + "\n";
            }
        }
        DrawRect(detectAreaRect, Color.green);
        endPoints.Clear();
    }


    int SingleDtectRect(float pointX, float rectX)
    {
        return (int)Mathf.Ceil((pointX - rectX) / singleLenth);
    }

    void DrawRect(Rect rect, Color color)
    {
        Vector3 p0 = new Vector3(rect.x, rect.y, 0);
        Vector3 p1 = new Vector3(rect.x + rect.width, rect.y, 0);
        Vector3 p2 = new Vector3(rect.x + rect.width, rect.y + rect.height, 0);
        Vector3 p3 = new Vector3(rect.x, rect.y + rect.height, 0);

        Debug.DrawLine(p0, p1, color);
        Debug.DrawLine(p1, p2, color);
        Debug.DrawLine(p2, p3, color);
        Debug.DrawLine(p3, p0, color);
    }

    private bool gd_loop = false;

    // PP
    //	MODL ... 傳感器型號信息
    //	DMIN ... 最小計測可能距離 (mm)
    //	DMAX ... 最大計測可能距離 (mm)
    //	ARES ... 角度分解能(360度の分割数)
    //	AMIN ... 最小計測可能方向値
    //	AMAX ... 最大計測可能方向値
    //	AFRT ... 正面方向値
    //	SCAN ... 標準操作角速度

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(760, 100, 300, 300));
        {

            //// https://sourceforge.net/p/urgnetwork/wiki/scip_jp/
            if (GUILayout.Button("VV: (獲取版本信息)"))
            {
                urg.Write(SCIP_library.SCIP_Writer.VV());
            }
            if (GUILayout.Button("PP: (獲取參數信息)"))
            {
                urg.Write(SCIP_library.SCIP_Writer.PP());
            }
            if (GUILayout.Button("MD: 測量和傳輸請求"))
            {
                urg.Write(SCIP_library.SCIP_Writer.MD(0, 1080, 1, 0, 0));
            }
            if (GUILayout.Button("ME: 測量和距離數據·接收強度值傳輸請求"))
            {
                urg.Write(SCIP_library.SCIP_Writer.ME(0, 1080, 1, 1, 0));
            }
            if (GUILayout.Button("BM: 激光發射"))
            {
                urg.Write(SCIP_library.SCIP_Writer.BM());
            }
            if (GUILayout.Button("GD: 測量距離數據傳輸請求"))
            {
                urg.Write(SCIP_library.SCIP_Writer.GD(0, 1080));
            }
            if (GUILayout.Button("GD_loop"))
            {
                gd_loop = !gd_loop;
            }
            if (GUILayout.Button("QUIT"))
            {
                urg.Write(SCIP_library.SCIP_Writer.QT());
            }

            GUILayout.Label("distances.Count: " + distances.Count + " / strengths.Count: " + strengths.Count);
            GUILayout.Label("drawCount: " + drawCount + " / detectObjects: " + detectObjects.Count);

        }
        GUILayout.EndArea();

    }
}
