using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Leap;
using System.Linq;
using System.Text;
using System.IO;
/*
・座標変換
・強化学習のアルゴリズムを考える．
・試行回数に応じて変化量を少なくする．
*/

public class cubescript : MonoBehaviour
{
    // 試行回数
    public static int count = 0;
    // goal検知用フラグ
    public bool flag = false;
    public bool flag2 = false;
    public bool nongripflag= false;
    // 手の速度検知用フラグ    
    private bool speedFlag = false;   
    // 時間計測用フラグ
    public bool startTimeFlag = false;
    public bool endTimeFlag = false;         

    // leapMotionによる手の形を確認するための変数
    private Controller controller;
    private Hand rightHand = null;
    private Hand leftHand = null;
    private Finger[] rightFingers;
    private Finger[] leftFingers;
    private bool[] rightIsGripFingers;
    private bool[] leftIsGripFingers;

    Rigidbody rigidbody;
    Vector3 lastLeftHand;
    Vector3 nowLeftHand;
    Vector lastLeftHand_temp;
    Vector nowLeftHand_temp;
    Quaternion firstRotate;

    // 回転用パラメータ
    private static float speed = 0.01f;
    private static float magnification = 1.0f;
    private static float v_th = 0.25f;

    GameObject text;
    GameObject goal2;
    TimeManager timeScript;
    goalManager goalScript;

    private float elapsedTime = 0.0f;
    private static float time_ave = 0.0f;
    private static float time_max = 0.0f;
    private static float time_min = 0.0f;
    public static List<float> timeList = new List<float>();
    public static List<float> timeList10 = new List<float>();
    public static List<float> v_thList = new List<float>();
    public static float[] vth_array = {0.0f, 0.05f, 0.1f, 0.15f, 0.2f, 0.25f, 0.3f, 0.35f, 0.4f, 0.45f, 0.5f};
    
    /*強化学習*/
    // ＜現在＞状態：s (0 ~ 10)　行動：a (0 or 1)
    private static int s;             
    private static int a;
    // ＜未来＞状態：s_next (0 ~ 10)　行動：a_next (0 or 1)                 
    int s_next;
    int a_next;
    // Q値用配列
    public static float[,] Q = new float[11,2];
    float Q_next;
    float eps;

    // Start is called before the first frame update
    void Start()
    {

       //MeshFilter mf = GetComponent<MeshFilter>();
       //Vector3[] test = mf.mesh.vertices;
       
       /*foreach (Vector3 item in test) {
           Debug.Log(item);
       }*/
        controller = new Controller();
        rightFingers = new Finger[5];
        leftFingers = new Finger[5];
        rightIsGripFingers = new bool[5];
        leftIsGripFingers = new bool[5];
        rigidbody = GetComponent<Rigidbody>();
        firstRotate = this.transform.rotation;

        text = GameObject.Find("Text");
        goal2 = GameObject.Find("Sphere");
        timeScript = text.GetComponent<TimeManager>();
        goalScript = goal2.GetComponent<goalManager>();

        if(count == 0){
            Debug.Log("計測開始！");
        }

        // 試行回数が50回になったらcsvファイルに出力
        if(count == 51){
            StreamWriter sw = new StreamWriter(@"v_th30Change10_ver18_0.3.csv",false, Encoding.GetEncoding("Shift_JIS"));
            string[] s1 = { "試行回数", "速度の閾値", "操作時間"};
            string s2 = string.Join(",", s1);
            sw.WriteLine(s2);

            for(int i=0; i<50; i++){
                string[] str = {""+(i+1), ""+v_thList[i]*100, ""+timeList[i]};
                string str2 = string.Join(",", str);
                sw.WriteLine(str2);
            }

            sw.Close();
            Debug.Log("計測完了！");
        }

        /*強化学習*/   
        // 変数の初期化
        if(count==0){
            // 行動aの初期化
            if(eps >= 0.5) a = 0;
            else a = 1;

            // Q値の初期化
            for(int s=0; s<vth_array.Length; s++){
                for (int a=0; a<2; a++){
                    Q[s,a] = 0.0f;
                }
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        
        // 手の挙動の情報
        Frame frame = controller.Frame();

        // 両手が画面上で認識されたら
        if(frame.Hands.Count == 2)
        {
            // 右手と左手を取得する
            foreach (Hand hand in frame.Hands)
            {
                if (hand.IsRight)    rightHand = hand;
                if (hand.IsLeft)     leftHand = hand;
            }

            // 手のデータから指のデータを取得
            rightFingers = rightHand.Fingers.ToArray();
            leftFingers = leftHand.Fingers.ToArray();

            // 全ての指の曲がり方のbool値を取得
            rightIsGripFingers = Array.ConvertAll(rightFingers, new Converter<Finger, bool>(i => i.IsExtended));
            leftIsGripFingers  = Array.ConvertAll(leftFingers, new Converter<Finger, bool>(i => i.IsExtended));

            // 伸びている指の本数を取得
            int rightExtendedFingerCount = rightIsGripFingers.Count(n => n == true);
            int leftExtendedFingerCount = leftIsGripFingers.Count(n => n == true);

            if(rightExtendedFingerCount == 5 && leftExtendedFingerCount == 5){
                nongripflag = true;
                Debug.Log("aaa");
            }else{
                nongripflag = false;
            }

            // 両手がグーの場合
            if(rightExtendedFingerCount == 0 && leftExtendedFingerCount == 0)
            {
                // 時間計測開始
                startTimeFlag = true;

                timeController();                

                if(!speedFlag)
                {      
                    lastLeftHand_temp = leftHand.PalmPosition;
                    lastLeftHand = ToVector3(lastLeftHand_temp);

                    if(ToVector3(leftHand.PalmVelocity).magnitude > speed){
                        speedFlag = true;
                    }
                }

                if(speedFlag)
                {
                    nowLeftHand_temp = leftHand.PalmPosition;
                    nowLeftHand = ToVector3(nowLeftHand_temp);

                    // 回転倍率
                    magnification = SpeedAdjustment(ToVector3(leftHand.PalmVelocity), v_th);

                    // 回転動作
                    Vector3 v_c = (nowLeftHand - this.transform.position).normalized;
                    Vector3 v_c2 = (lastLeftHand - this.transform.position).normalized;
                    Quaternion q_r = Quaternion.FromToRotation(v_c2, v_c);
                    this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.transform.rotation*q_r, magnification);

                    if(ToVector3(leftHand.PalmVelocity).magnitude <= speed){
                        speedFlag = false;
                    }
                }

                
                //Debug.Log(v_th);

            }
            else
            {
                lastLeftHand_temp = leftHand.PalmPosition;
                lastLeftHand = ToVector3(lastLeftHand_temp);
            }
            ClearJudge();
        }
    }

    void ClearJudge()
    {
        GameObject target = GameObject.Find ("goal");
        /*if(this.transform.rotation == target.transform.rotation){
            this.GetComponent<Renderer>().material.color = Color.yellow;
            flag = true;
            endTimeFlag = true;
            startTimeFlag = false;
            timeController();
            ChangeParameter_Ver9();
            Invoke("ChangeScene", 2.0f);
            enabled = false;
        }*/
        Debug.Log(goalScript.goalFlag2);
        if(goalScript.goalFlag2 && nongripflag){
            
            this.GetComponent<Renderer>().material.color = Color.yellow;
            flag = true;
            endTimeFlag = true;
            startTimeFlag = false;
            timeController();
            //ChangeParameter();
            Q_learning();
            Invoke("ChangeScene", 2.0f);
            enabled = false;
            goalScript.goalFlag2 = false;
        }
    }

    float SpeedAdjustment(Vector3 hand, float v_th)
    {
        float mag;                 // 回転倍率
        if(hand.magnitude >= v_th){
            mag = 1.0f;
        }else{
            mag = (hand.magnitude / v_th)*1.0f;
        }
        return mag;
    }

    // LeapのVectorからUnityのVector3に変換(単位もmmからmに変換)
    Vector3 ToVector3(Vector v)
    {
        float a = 0.001f;
        v.x *= a;
        v.y *= a;
        v.z *= a; 
        return new Vector3(v.x, v.y, v.z);
    }

    // Scene初期化
    void ChangeScene()
    {
        SceneManager.LoadScene("dsa");
    }

    // 時間管理
    void timeController()
    {
        if(startTimeFlag){
            elapsedTime += Time.deltaTime;
        }
        if(endTimeFlag){
            count++;
            timeList.Add(elapsedTime);
            
            timeList10.Add(elapsedTime);
            time_ave = timeList10.Average();
            time_max = timeList10.Max();
            time_min = timeList10.Min();
            v_thList.Add(v_th);

            endTimeFlag = false;
            Debug.Log("試行回数："+count+"　速度の閾値："+v_th+ "　所要時間："+elapsedTime);
        }
    }

    // パラメータ変更
    void ChangeParameter()
    {
        // epsilonの生成
        float epsilon = UnityEngine.Random.value;
        // epsilonの閾値(0.3から試行回数ごとに減少)
        float epsilon_th = -0.01f*count+0.3f;

        /* ε-greedyポリシー */
        // 探索
        if(epsilon <= epsilon_th){
            v_th = vth_array[UnityEngine.Random.Range(0, vth_array.Length)];
            flag2 = true;
            Debug.Log("探索！！！");
        // 活用
        }else{
            Q_learning();
        }
    }

    
    // 強化学習(Q学習)
    void Q_learning()
    {
        float alpha = 0.5f;     // 学習率
        float gamma = 0.3f;     // 割引率
        float r;                // 報酬
        eps = UnityEngine.Random.value; 

        s = Array.IndexOf(vth_array, v_th);     // 改良可能

        // 未来の状態s_nextの決定
        if(a == 0){
            if(s == vth_array.Length-1) s_next = vth_array.Length-1;
            else s_next = s+1;
        }else{
            if(s == 0) s_next = 0;
            else s_next = s-1;
        }

        // 未来の行動a_nextの決定
        /*if(s_next == 0){
            a_next = 0;
        }else if(s_next == vth_array.Length-1){
            a_next = 1;
        }else if(Q[s_next,0] > Q[s_next,1] || Q[a_next,0] == Q[a_next,1] && eps >= 0.5){
            a_next = 0;
            Q_next = Q[s_next, a_next];        
        }else{
            a_next = 1;
            Q_next = Q[s_next, a_next];
        }*/

        // 未来の行動a_nextの決定
        if(s_next == 0){
            a_next = 0;
        }else if(s_next == vth_array.Length-1){
            a_next = 1;
        }else{
            double x = Math.Exp(Q[s_next, 0])/(Math.Exp(Q[s_next, 0]) + Math.Exp(Q[s_next, 1]));
            double y = Math.Exp(Q[s_next, 1])/(Math.Exp(Q[s_next, 0]) + Math.Exp(Q[s_next, 1]));
            if(x <= y){
                if(eps >= x){
                    a_next = 0;
                    Q_next = Q[s_next, a_next]; 
                }else{
                    a_next = 1;
                    Q_next = Q[s_next, a_next]; 
                }
            }else{
                if(eps <= y){
                    a_next = 1;
                    Q_next = Q[s_next, a_next]; 
                }else{
                    a_next = 0;
                    Q_next = Q[s_next, a_next]; 
                }
            }
        }

        // 報酬
    　　if(timeList[count-1] <= time_min){
            r = 1.0f;
        }else if(time_min <= timeList[count-1] && timeList[count-1] <= time_ave){
            r = 0.1f;
        }else if(time_ave <= timeList[count-1] && timeList[count-1] <= time_max){
            r = -0.1f;
        }else{
            r = -1.0f;
        }

        // Q値更新
        Q[s,a] = (1-alpha)*Q[s,a] + alpha*(r+gamma*Q_next);

        // 変数の更新
        a = a_next;
        v_th = vth_array[s_next];      // 改良可能
        
    }
}
