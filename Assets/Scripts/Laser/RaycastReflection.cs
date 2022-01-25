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
	float car1 = 0f;
	float car2 = 0f;
	float car3 = 0f;
	float car4 = 0f;
	float car5 = 0f;

	//시간 설정
	float timecount = 0;
	int oneSecond = 1;
	int realSecond = 0;

	//상태 설정
	bool one = false;
	bool two = false;
	bool three = false;
	bool four = false;
	bool five = false;

	private void Awake()
	{
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.startColor = Color.black;
	}

	private void Start()
	{
		countText1.text = "Car 1 :" + car1.ToString();
		countText2.text = "Car 2 :" + car2.ToString();
		countText3.text = "Car 3 :" + car3.ToString();
		countText4.text = "Car 4 :" + car4.ToString();
		countText5.text = "Car 5 :" + car5.ToString();
		timeText.text = "시간 :" + realSecond.ToString();

	}

	private void FixedUpdate()
	{
		// Beam Y로 Rotation하기

		if(oneSecond < timecount)
        {
			transform.Rotate(new Vector3(0, 1f, 0f));
			timecount = 0;
			realSecond++;
            if (one == true)
            {
                one = false;
                car1++;
            }
			if (two == true)
			{
				two = false;
				car2++;
			}
			if (three == true)
			{
				three = false;
				car3++;
			}
			if (four == true)
			{
				four = false;
				car4++;
			}
			if (five == true)
			{
				five = false;
				car5++;
			}
		}
        timecount += 1f * Time.deltaTime;
		

		ray = new Ray(transform.position, transform.forward);

		lineRenderer.positionCount = 1;
		lineRenderer.SetPosition(0, transform.position);
		float remainingLength = maxLength;

		countText1.text = "Car 1 :" + car1.ToString();
		countText2.text = "Car 2 :" + car2.ToString();
		countText3.text = "Car 3 :" + car3.ToString();
		countText4.text = "Car 4 :" + car4.ToString();
		countText5.text = "Car 5 :" + car5.ToString();
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
				if (hit.collider.tag == "car1")one = true;
				if (hit.collider.tag == "car2")two = true;				
				if (hit.collider.tag == "car3")three = true;			
				if (hit.collider.tag == "car4")four = true;				
				if (hit.collider.tag == "car5")five = true;
				

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
