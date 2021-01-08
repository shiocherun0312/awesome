using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System.Linq;

public class handRSP : MonoBehaviour {
    // goal検知用フラグ
    public bool flag = false;
    public bool flag2 = false;
    public bool flag3 = false;
    public bool flag4 = false;
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
    Vector3 lastLeftHand_temp;
    Vector3 nowLeftHand_temp;
    Quaternion firstRotate;
    GameObject goal2;

    Mesh mesh;
    Vector3[] vertices;
    MeshFilter mf;

    Mesh mesh2;
    Vector3[] vertices2;
    MeshFilter mf2;

    GameObject thumb;
    Vector3 last_vt;

    

    // 回転用パラメータ
    private static float speed = 0.01f;
    private static float magnification = 1.0f;
    private static float v_th = 0.25f;
    private static float a = 0.3f;
    //Child2ColliderEnter sc2;
    int count = 0;
    // Use this for initialization
    void Start () {
        //controller = new Controller();
        //fingers = new Finger[5];
        //isGripFingers = new bool[5];
        //sc1 = FindObjectOfType<ChildColliderEnter>(); // インスタンス化
        //sc = GameObject.FindGameObjectWithTag("Player");
        //sc1 = sc.GetComponent<ChildColliderEnter>();　  

        controller = new Controller();
        rightFingers = new Finger[5];
        leftFingers = new Finger[5];
        rightIsGripFingers = new bool[5];
        leftIsGripFingers = new bool[5];
        rigidbody = GetComponent<Rigidbody>();
        firstRotate = this.transform.rotation;

        goal2 = GameObject.Find("goal");
        mf = GetComponent<MeshFilter>();
        mf2 = goal2.GetComponent<MeshFilter>();
        thumb = GameObject.FindGameObjectWithTag("Player");
        //Debug.Log(hand5.transform.position);
    }

    // Update is called once per frame
    void Update () {

        if(count < 300){
            //this.transform.position = this.transform.position;
             rigidbody.constraints = RigidbodyConstraints.FreezeAll;
             //Debug.Log("qwe");
        }else{
             rigidbody.constraints = RigidbodyConstraints.None;
             flag3 = false;
        }

        thumb = GameObject.FindGameObjectWithTag("Player");
        mesh = mf.mesh;
        vertices = mesh.vertices;

        mesh2 = mf2.mesh    ;
        vertices2 = mesh2.vertices;
        //foreach(Vector3[] v in vertices){
        //    vertices= transform.TransformVector(v);
        //}
        //Debug.Log(vertices.Length);
        for(int i=0; i < vertices.Length; i++){
            vertices[i] = this.transform.TransformPoint(vertices[i].x, vertices[i].y, vertices[i].z);
        }

        for(int i=0; i < vertices.Length; i++){
            vertices2[i] = goal2.transform.TransformPoint(vertices2[i].x, vertices2[i].y, vertices2[i].z);
        }

        
        //Debug.Log("1: " + vertices2[0].x + ", " + vertices2[0].y + ", " + vertices2[0].z);
        //Debug.Log("2: " + vertices2[1].x + ", " + vertices2[1].y + ", " + vertices2[1].z);
        //Debug.Log(vertices2[3]);

        //rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
        //rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;
        count++;
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

            //GameObject target = GameObject.Find ("goal");
            //if(this.transform.position == target.transform.position){
            //        rigidbody.constraints = RigidbodyConstraints.FreezePosition;
            //}

            rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            // 両手がグーの場合
            if(rightExtendedFingerCount == 0 && leftExtendedFingerCount == 0)
            {
                
                //Debug.Log("asd");
                // 時間計測開始
                startTimeFlag = true;

                

                //timeController();                

                if(!speedFlag)
                {      
                    lastLeftHand_temp = this.transform.TransformPoint(ToVector3(leftHand.PalmPosition));
                    lastLeftHand = this.transform.TransformPoint(lastLeftHand_temp);
                    last_vt = thumb.transform.position - this.transform.position;

                    if(ToVector3(leftHand.PalmVelocity).magnitude > speed){
                        speedFlag = true;
                    }

                    
                }

                if(speedFlag)
                {
                    flag3 = true;
                    //rigidbody.constraints = RigidbodyConstraints.None;
                    //Debug.Log("asd");
                    nowLeftHand_temp =this.transform.TransformPoint(ToVector3(leftHand.PalmPosition));
                    nowLeftHand = this.transform.TransformPoint(nowLeftHand_temp);

                    // 回転倍率
                    magnification = SpeedAdjustment(ToVector3(leftHand.PalmVelocity), v_th);

                    // 回転動作
                    Vector3 v_c = (nowLeftHand - this.transform.position).normalized;
                    Vector3 v_c2 = (lastLeftHand - this.transform.position).normalized;
                    Quaternion q_r = Quaternion.FromToRotation(v_c, v_c2);
                    //this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.transform.rotation*q_r, magnification);
                    Quaternion q_f = Quaternion.Slerp(this.transform.rotation, this.transform.rotation*q_r, magnification);
                    
                    // quaternion→回転変換行列
                    Matrix4x4 Ma = Matrix4x4.identity;
                    Ma[0]= q_f.x*q_f.x - q_f.y*q_f.y - q_f.z*q_f.z + q_f.w*q_f.w;
                    Ma[1]= 2*(q_f.x*q_f.y + q_f.z*q_f.w);
                    Ma[2]= 2*(q_f.x*q_f.z - q_f.y*q_f.w);
                    Ma[3] = 0;
                    Ma[4]= 2*(q_f.x*q_f.y - q_f.z*q_f.w);
                    Ma[5]= -q_f.x*q_f.x + q_f.y*q_f.y - q_f.z*q_f.z + q_f.w*q_f.w;
                    Ma[6]= 2*(q_f.y*q_f.z + q_f.x*q_f.w);
                    Ma[7] = 0;
                    Ma[8]= 2*(q_f.x*q_f.z + q_f.y*q_f.w);
                    Ma[9]= 2*(q_f.y*q_f.z - q_f.x*q_f.w);
                    Ma[10]= -q_f.x*q_f.x - q_f.y*q_f.y + q_f.z*q_f.z + q_f.w*q_f.w;
                    Ma[11] = 0;
                    Ma[12] = 0;
                    Ma[13] = 0;
                    Ma[14] = 0;
                    Ma[15] = 1;

                    // 仮想物体から親指の先へのベクトル
                    Vector3 now_vt = thumb.transform.position - this.transform.position;
                    Vector3 v_c3 = now_vt - last_vt;
                    last_vt = now_vt;

                    // 変位を表す変換行列
                    Matrix4x4 Mt = Matrix4x4.identity;
                    Mt[5] = v_c3.y;
                    Mt[10] = v_c3.y;

                    // 仮想物体の状態を表す変換行列？
                    Matrix4x4 Mv = Matrix4x4.identity;
                    Mv[0] = this.transform.position.x;
                    Mv[5] = this.transform.position.y;
                    Mv[10] = this.transform.position.z;

                    // 回転
                    Matrix4x4 Mo= Mv*Mt*Ma*Mt.inverse*Mv.inverse;

                    // 回転行列からquaternion 
                    float a1 = (float)System.Math.Sqrt(Mo[0]-Mo[5]-Mo[10]+1)/2.0f;
                    float a2 = (float)System.Math.Sqrt(-Mo[0]+Mo[5]-Mo[10]+1)/2.0f;
                    float a3 = (float)System.Math.Sqrt(-Mo[0]-Mo[5]+Mo[10]+1)/2.0f;
                    float a4 = (float)System.Math.Sqrt(Mo[0]+Mo[5]+Mo[10]+1)/2.0f;
                    Quaternion q_f2= new Quaternion(a1, a2, a3, a4);
                    this.transform.rotation =q_f2;

                    if(ToVector3(leftHand.PalmVelocity).magnitude <= speed){
                        speedFlag = false;
                    }
                }
            }
            else
            {
                lastLeftHand_temp = ToVector3(leftHand.PalmPosition);
                lastLeftHand = lastLeftHand_temp;
            }

            ClearJudge();
        }
    }

    Vector3 ToVector3(Vector v)
    {
        float a = 0.001f;
        v.x *= a;
        v.y *= a;
        v.z *= a; 
        return new Vector3(v.x, v.y, v.z);
    }

    float SpeedAdjustment(Vector3 hand, float v_th)
    {
        float mag=0.1f;                 // 回転倍率
        /*if(hand.magnitude >= v_th){
            mag = 1.0f;
        }else{
            //mag = 0.1f;
            mag = (hand.magnitude / v_th)*1.0f;
        }*/
        return mag;
    }


    void ClearJudge()
    {

        //Debug.Log(vertices2[0].x + ", " + vertices2[0].y + "," + vertices2[0].z);
        //Debug.Log(vertices2[1].x + ", " + vertices2[1].y + "," + vertices2[1].z);
        //Debug.Log(flag4);
        /*if(0.60 <= vertices[0].x && vertices[0].x <= 0.70){
            //Debug.Log("1");
            if(0.10 <= vertices[0].y && vertices[0].y <= 0.20){
                //Debug.Log("2");
                if(0.25 <= vertices[0].z && vertices[0].z <= 0.35){
                    flag4 = true;
                }
            }
        }*/

        bool flag41 = false;
        bool flag42 = false;

        if(0.60 <= vertices[0].x && vertices[0].x <= 0.70){
            //Debug.Log("1");
            if(0.10 <= vertices[0].y && vertices[0].y <= 0.20){
                //Debug.Log("2");
                if(0.25 <= vertices[0].z && vertices[0].z <= 0.35){
                    flag41 = true;
                }
            }
        }

        if(0.28 <= vertices[0].x && vertices[0].x <= 0.36){
            //Debug.Log("1");
            if(0.20 <= vertices[0].y && vertices[0].y <= 0.28){
                //Debug.Log("2");
                if(0.33 <= vertices[0].z && vertices[0].z <= 0.42){
                    flag42 = true;
                }
            }
        }

        if(flag41 && flag42){
            flag4 = true;
        }

        GameObject target = GameObject.Find ("goal");
        /*if(this.transform.rotation == target.transform.rotation && this.transform.position == target.transform.position){
            this.GetComponent<Renderer>().material.color = Color.yellow;
            /*flag = true;
            endTimeFlag = true;
            startTimeFlag = false;
            timeController();
            ChangeParameter_Ver9();
            Invoke("ChangeScene", 2.0f);
            enabled = false;
            Debug.Log("clear");
        }*/

        if(flag4){
            //Debug.Log("clear");
        }

    }
}

