using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goalManager : MonoBehaviour
{
    int goalcount = 0;
    public bool goalFlag2 = false;
    bool flag = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(flag){
            goalcount++;
            if(goalcount >= 40){
                goalFlag2 = true;
            }
        }
        
    }

    void OnTriggerEnter(Collider other){
        flag = true;
    }

    void OnTriggerExit(Collider other){
        flag = false;
        goalcount = 0;
    }


    /*void ChangeParameter_Ver1()
    {
        if(count >= 2){
            if(timeList[count-2] > timeList[count-1]){
                v_th += 0.05f; 
            }else{
                v_th -= 0.05f;
            }
        }
    }

    void ChangeParameter_Ver2()
    {  
        if(count >= 2){
            if(timeList[count-2] > timeList[count-1]){
                v_th -= a; 
            }else{
                v_th += a;
            }

            if(a >= 0) a -= 0.001f; 
        }
    }

    void ChangeParameter_Ver3()
    {  
        if(count >= 2){
            if(time_ave > timeList[count-1]){
                reward++;
                v_th -= a*(Mathf.Abs(time_ave - timeList[count-1]) / (time_max - time_min));
            }else{
                reward--;
                v_th += a*(Mathf.Abs(time_ave - timeList[count-1]) / (time_max - time_min));
            }
        }

        if(v_th >= 0.5f){
            v_th = 0.0f;
        }
    }

    void ChangeParameter_Ver4()
    {  
        // 初期値決定区間
        if(count < 2){
            v_th = 0.1f;
        }else if(count < 4){
            v_th = 0.2f;
        }else if(count < 6){
            v_th = 0.3f;
        }else if(count < 8){
            v_th = 0.4f;
        }else if(count < 10){
            v_th = 0.5f;
        }

        if(count == 10){
            float[] v_thTemp= new float[5];
            for(int i = 0; i<5; i++){
                v_thTemp[i] = (timeList[2*i]+timeList[2*i+1])*0.5f;
            }
            int number = Array.IndexOf(v_thTemp, v_thTemp.Min());
            v_th = 0.1f*(number+1);
        }

        if(count >= 11){
            ChangeParameter_Ver1();
        }
    }

    void ChangeParameter_Ver5()
    {
        if(count >= 2){
            float diff = timeList[count-2] - timeList[count-1];
            if(diff >= 2.0f){
                v_th += 0.1f;
            }else if(diff >= 1.0f){
                v_th += 0.1f; 
            }else if(diff >= 0.0f){
                v_th += 0.05f;
            }else if(diff <= -2.0f){
                v_th -= 0.1f;
            }else if(diff <= -1.0f){
                v_th -= 0.1f;
            }else{
                v_th -= 0.05f;
            }
        }
    }

    void ChangeParameter_Ver6()
    {
        if(count > 2 && count%3 == 0){
            float t_first = timeList[count-3];
            float t_middle = timeList[count-2];
            float t_last = timeList[count-1];

            // 単調増加
            if(t_first <= t_middle && t_middle <= t_last){
                v_th = v_thList[count-3];
            }
            // 単調減少
            else if(t_first >= t_middle && t_middle >= t_last){
                v_th = v_thList[count-1];
            }
            // 極大値
            else if(t_first <= t_middle && t_middle >= t_last){
                if(t_first >= t_last){
                    v_th = v_thList[count-1];
                }else{
                    v_th = v_thList[count-3];
                }
            }
            // 極小値
            else if(t_first >= t_middle && t_middle <= t_last){
                v_th = v_thList[count-2];
            }
        }
    }

    void ChangeParameter_Ver7()
    {
        if(count >= 2){
            float diff = time_ave - timeList[count-1];
            if(diff >= 2.0f){
                v_th -= 0.1f;
            }else if(diff >= 1.0f){
                v_th -= 0.05f; 
            }else if(diff >= 0.0f){
                v_th -= 0.025f;
            }else if(diff <= -2.0f){
                v_th += 0.1f;
            }else if(diff <= -1.0f){
                v_th += 0.05f;
            }else{
                v_th += 0.025f;
            }
        }
    }

    void ChangeParameter_Ver8()
    {
        UnityEngine.Random rnd = new UnityEngine.Random();
        float eps = UnityEngine.Random.value;
        int b = UnityEngine.Random.Range(0, vth_array.Length);

        //Debug.Log(eps);

        if(eps <= 1.0){
            var a = vth_array[b];
            v_th = a;
        }else{
            var minValue = myTable.Values.Min();
            var key = myTable.First(x => x.Value == minValue).Key;
            v_th = key;
        }
        
    }

    void ChangeParameter_Ver9()
    {
        float d;
        UnityEngine.Random rnd = new UnityEngine.Random();
        float eps = UnityEngine.Random.value;
        int b = UnityEngine.Random.Range(0, vth_array.Length);

        //Debug.Log(eps);

        d = -0.006f*count+0.3f;
        if(eps <= d){
            Debug.Log("aaa"+count);
            var a = vth_array[b];
            v_th = a;
            flag2 = true;
        }else{
            ChangeParameter_Ver10();
        }

    }

    void ChangeParameter_Ver10(){
        UnityEngine.Random rnd = new UnityEngine.Random();
        float eps2 = UnityEngine.Random.value;
        int a = 0;
        int a_next =0;
        float v_th2= 0.0f;

        if(v_th == 0.0f){
            a = 0;
        }else if(v_th == 0.05f){
            a = 1;
        }else if(v_th == 0.1f){
            a = 2;
        }else if(v_th == 0.15f){
            a = 3;
        }else if(v_th == 0.2f){
            a = 4;
        }else if(v_th == 0.25f){
            a = 5;
        }else if(v_th == 0.3f){
            a = 6;
        }else if(v_th == 0.35f){
            a = 7;
        }else if(v_th == 0.4f){
            a = 8;
        }else if(v_th == 0.45f){
            a = 9;
        }else if(v_th == 0.5f){
            a = 10;
        }

        float b = 0.5f;
        float c = 0.3f;
        float d = 0.0f;
        float r = 0.0f;

        if(v == 0){
            if(v_th == 0.5f){
                v_th2 = 0.5f;
            }else{
                v_th2 = vth_array[a+1];//v_th + 0.05f;
                // 100倍して丸める
                if(v_th2 > 0.5f){
                    v_th2 =0.5f;
                }
            } 
        }else{
            if(v_th == 0.0f){
                v_th2 = 0.0f;
            }else{
                v_th2 = vth_array[a-1];//v_th - 0.05f;
            }
        }

        if(v_th2 == 0.0f){
            a_next = 0;
        }else if(v_th2 == 0.05f){
            a_next = 1;
        }else if(v_th2 == 0.1f){
            a_next = 2;
        }else if(v_th2 == 0.15f){
            a_next = 3;
        }else if(v_th2 == 0.2f){
            a_next = 4;
        }else if(v_th2 == 0.25f){
            a_next = 5;
        }else if(v_th2 == 0.3f){
            a_next = 6;
        }else if(v_th2 == 0.35f){
            a_next = 7;
        }else if(v_th2 == 0.4f){
            a_next = 8;
        }else if(v_th2 == 0.45f){
            a_next = 9;
        }else if(v_th2 == 0.5f){
            a_next = 10;
        }

        if(Q[a_next,0] > Q[a_next,1]){
            d = Q[a_next,0];
            v_next = 0;
        }else if(Q[a_next,0] == Q[a_next,1]){
            if(eps2>=0.5){
                d = Q[a_next,0];
                v_next = 0;
            }else{
                d = Q[a_next,1];
                v_next = 1;
            }
        }else{
            d = Q[a_next,1];
            v_next = 1;
        }

        if(timeList[count-1] <= time_min){
            r = 1;
        }else if(timeList[count-1] <= time_ave){
            r = 0.1f;
        }else if(timeList[count-1] <= time_max){
            r = -0.1f;
        }else{
            r = -1;
        }


        if(count >= 1){
            Q[a,v] = (1-b)*Q[a,v] + b*(r+c*d);
            //Debug.Log(Q[a,v]);
        }

        v = v_next;
        v_th = v_th2;
    }*/
}
