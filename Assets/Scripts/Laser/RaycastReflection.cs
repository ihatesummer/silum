using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(LineRenderer))]
public class RaycastReflection : MonoBehaviour
{
	public int reflections;
	public float maxLength;

	private LineRenderer lineRenderer;
	private Ray ray;
	private RaycastHit hit;
	private Vector3 direction;

	// UI용 텍스트
	public Text countText1;
	public Text countText2;
	public Text countText3; 
	public Text countText4;
	public Text countText5;
	public Text timeText;

    // UI 설정
    // 차를 여러개 만들려면 계속 스크립트내에서 증식해야함 For문형태 불가

    //시간 설정
    float timecount = 0;
	public float oneSecond = 0.25f;
	int realSecond = 0;
    
    //자동차 갯수
    int carNumber = 5;

    float[] carList = new float[100]; // Max 100
    bool[] carStatus = new bool[100]; // Max 100
    
    void Awake()
	{
        
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.startColor = Color.black;
	}

	void Start()
	{
        for (int i = 0; i < carNumber; i++) carList[i] = 0f; // 초기화
        for (int i = 0; i < carNumber; i++) carStatus[i] = false; // 초기화

        //데이터 넣는 곳
        //UI 상에서는 carList = 5 (Default carList lenght)이므로, UI상 추가를 원하시면 Ctrl + C, V를 통해 배열값을 수정해야합니다.
        countText1.text = "Car 1 :" + carList[0].ToString();
		countText2.text = "Car 2 :" + carList[1].ToString();
		countText3.text = "Car 3 :" + carList[2].ToString();
		countText4.text = "Car 4 :" + carList[3].ToString();
		countText5.text = "Car 5 :" + carList[4].ToString();
		timeText.text = "시간 :" + realSecond.ToString();

	}

	void Update()
	{
        // 시간 컨트롤은 oneSecond 변수 하나만 컨트롤
		if(oneSecond < timecount)
        {
            // Beam Y로 Rotation하기
            transform.Rotate(new Vector3(0, 1f, 0f));
			timecount = 0;
			realSecond++;
            for (int i = 0; i < carNumber; i++) {
                if (carStatus[i] == true) {
                    carStatus[i] = false;
                    carList[i]++;
                }
            }
		}
        // Time.deltaTime이 아닌 oneSecond 변수만 컨트롤
        timecount += 1f * Time.deltaTime;
		

		ray = new Ray(transform.position, transform.forward);

		lineRenderer.positionCount = 1;
		lineRenderer.SetPosition(0, transform.position);
		float remainingLength = maxLength;

		countText1.text = "Car 1 :" + carList[0].ToString();
		countText2.text = "Car 2 :" + carList[1].ToString();
		countText3.text = "Car 3 :" + carList[2].ToString();
		countText4.text = "Car 4 :" + carList[3].ToString();
		countText5.text = "Car 5 :" + carList[4].ToString();
		timeText.text = "현재 각도 :" + realSecond.ToString();

		//반사 정하기, reflectoin은 inspector에서 정하기
		for (int i = 0; i < reflections; i++)
		{
			if (Physics.Raycast(ray.origin, ray.direction, out hit, remainingLength))
			{
				lineRenderer.positionCount += 1;
				lineRenderer.SetPosition(lineRenderer.positionCount - 1, hit.point);
				remainingLength -= Vector3.Distance(ray.origin, hit.point);
				ray = new Ray(hit.point, Vector3.Reflect(ray.direction, hit.normal));

                // Ray가 각각의 Car tag와 충돌했을 경우 더합니다.
                // this.tag -> 현재 스크립트에 들어간 tag 이름 (자기자신 tag와 똑같은 tag와 충돌했을 경우 Count 실행 X)
                // tag같은 경우 미리 설정을 해야함
                // Default carList Length = 5;
                // Tag의 경우 c#구조 수정이 아닌 Unity Framework에서 수정이므로 반복문 사용이 불가능함 (If Tag 자동 생성 기능이 있으면 가능하나 기능 X = 불가능)
                if (hit.collider.tag == "car1" && this.tag != "car1") carStatus[0] = true;
                if (hit.collider.tag == "car2" && this.tag != "car2") carStatus[1] = true;				
                if (hit.collider.tag == "car3" && this.tag != "car3") carStatus[2] = true;			
                if (hit.collider.tag == "car4" && this.tag != "car4") carStatus[3] = true;				
                if (hit.collider.tag == "car5" && this.tag != "car5") carStatus[4] = true;


                if (carNumber >= 6) if (hit.collider.tag == "car6" && this.tag != "car6") carStatus[5] = true;
                if (carNumber >= 7) if (hit.collider.tag == "car7" && this.tag != "car7") carStatus[6] = true;
                if (carNumber >= 8) if (hit.collider.tag == "car8" && this.tag != "car8") carStatus[7] = true;
                if (carNumber >= 9) if (hit.collider.tag == "car9" && this.tag != "car9") carStatus[8] = true;
                if (carNumber >= 10) if (hit.collider.tag == "car10" && this.tag != "car10") carStatus[9] = true;

                if (carNumber >= 11) if (hit.collider.tag == "car11" && this.tag != "car11") carStatus[10] = true;
                if (carNumber >= 12) if (hit.collider.tag == "car12" && this.tag != "car12") carStatus[11] = true;
                if (carNumber >= 13) if (hit.collider.tag == "car13" && this.tag != "car13") carStatus[12] = true;
                if (carNumber >= 14) if (hit.collider.tag == "car14" && this.tag != "car14") carStatus[13] = true;
                if (carNumber >= 15) if (hit.collider.tag == "car15" && this.tag != "car15") carStatus[14] = true;

                if (carNumber >= 16) if (hit.collider.tag == "car16" && this.tag != "car16") carStatus[15] = true;
                if (carNumber >= 17) if (hit.collider.tag == "car17" && this.tag != "car17") carStatus[16] = true;
                if (carNumber >= 18) if (hit.collider.tag == "car18" && this.tag != "car18") carStatus[17] = true;
                if (carNumber >= 19) if (hit.collider.tag == "car19" && this.tag != "car19") carStatus[18] = true;
                if (carNumber >= 20) if (hit.collider.tag == "car20" && this.tag != "car20") carStatus[19] = true;


                //자기 자신도 Collider할수 있도록
                /*
                if (hit.collider.tag == "car1") carStatus[0] = true;
                if (hit.collider.tag == "car2") carStatus[1] = true;
                if (hit.collider.tag == "car3") carStatus[2] = true;
                if (hit.collider.tag == "car4") carStatus[3] = true;
                if (hit.collider.tag == "car5") carStatus[4] = true;
                */
                if (hit.collider.tag != "Mirror")break;
			}
			else
			{
				lineRenderer.positionCount += 1;
				lineRenderer.SetPosition(lineRenderer.positionCount - 1, ray.origin + ray.direction * remainingLength);
			}
		}
	}
}
