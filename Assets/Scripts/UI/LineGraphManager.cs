using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LineGraphManager : MonoBehaviour {

    // WPF로 파일 만들다가 Unity에서 WPF를 지원하는 걸 끊어버려서 다시 Unity로...
    // 만약 Unity 내부에서 데이터를 쌓고 외부 (Matlab or 다른 툴)로 그래프를 그릴 수 있다면 상관 없을 듯 합니다.

	public GameObject linerenderer; // 선긋기 
	public GameObject pointer; // 데이터 포인트

	public GameObject pointerRed; // 포인터색깔
	public GameObject pointerBlue; // 포인터색깔

    public GameObject HolderPrefb; // Prefab

	public GameObject holder;
	public GameObject xLineNumber; // xLine Num

	public Material bluemat;
	public Material greenmat;

	public Text topValue;

	public List<GraphData> graphDataPlayer1 = new List<GraphData>(); // 데이터
	public List<GraphData> graphDataPlayer2 = new List<GraphData>(); // 데이터

	private GraphData gd;
	private float highestValue = 56; // 최대 에러 크기

	public Transform origin;

	//public TextMesh player1name;
	//public TextMesh player2name;

	private float lrWidth = 0.1f;
	private int dataGap = 0;


	void Start(){

		// 처음에는 랜덤 변수로 설정 
        // ErrorData받아오면 여기로 수정함
		int index = 120;
		for(int i = 0; i < index; i++){
			GraphData gd = new GraphData();
			gd.marbles = Random.Range(10,47);
			graphDataPlayer1.Add(gd);
			GraphData gd2 = new GraphData();
			gd2.marbles = Random.Range(10,47);
			graphDataPlayer2.Add(gd2);
		}

		// 그래프 그리기
		ShowGraph();
	}
	
	public void ShowData(GraphData[] gdlist,int playerNum,float gap) {

		// Adjusting value to fit in graph
		for(int i = 0; i < gdlist.Length; i++)
		{
			// Y값 Grid 값으로 구간 7로 설정하기 (임시)	
			gdlist[i].marbles = (gdlist[i].marbles/highestValue)*7;
		}
		if(playerNum == 1) 
			StartCoroutine(BarGraphBlue(gdlist,gap));
		else if(playerNum == 2) 
			StartCoroutine(BarGraphGreen(gdlist,gap));
	}
    

    /*
	public void AddPlayer1Data(int numOfStones){
		GraphData gd = new GraphData();
		gd.marbles = numOfStones;
		graphDataPlayer1.Add(gd);
	}
	public void AddPlayer2Data(int numOfStones){
		GraphData gd = new GraphData();
		gd.marbles = numOfStones;
		graphDataPlayer2.Add(gd);
	}

    */
    

    // 그래프 그리기 Start()에서 바로 시작
	public void ShowGraph(){

		ClearGraph(); // 그래프 초기화

		if(graphDataPlayer1.Count >= 1 && graphDataPlayer2.Count >= 1){
			holder = Instantiate(HolderPrefb,Vector3.zero,Quaternion.identity) as GameObject;
			holder.name = "h2";

			GraphData[] gd1 = new GraphData[graphDataPlayer1.Count]; // 데이터 넣기
			GraphData[] gd2 = new GraphData[graphDataPlayer2.Count]; // 데이터 넣기
			for(int i = 0; i < graphDataPlayer1.Count; i++){ // 카운트
				GraphData gd = new GraphData();
				gd.marbles = graphDataPlayer1[i].marbles;
				gd1[i] = gd;
			}
			for(int i = 0; i < graphDataPlayer2.Count; i++){ // 카운트
				GraphData gd = new GraphData();
				gd.marbles = graphDataPlayer2[i].marbles;
				gd2[i] = gd;
			}

			dataGap = GetDataGap(graphDataPlayer2.Count);


			int dataCount = 0;
			int gapLength = 1;
			float gap = 1.0f;
			bool flag = false;

			while(dataCount < graphDataPlayer2.Count) // 데이터 카운트가 높으면 
			{
				if(dataGap > 1){

					if((dataCount+dataGap) == graphDataPlayer2.Count){

						dataCount+=dataGap-1;
						flag = true;
					}
					else if((dataCount+dataGap) > graphDataPlayer2.Count && !flag){

						dataCount =	graphDataPlayer2.Count-1;
						flag = true;
					}
					else{
						dataCount+=dataGap;
						if(dataCount == (graphDataPlayer2.Count-1))
							flag = true;
					}
				}
				else
					dataCount+=dataGap;

				gapLength++;
			}

			if(graphDataPlayer2.Count > 13)
			{
				if(graphDataPlayer2.Count < 40)
					gap = 13.0f/graphDataPlayer2.Count;
				else if(graphDataPlayer2.Count >= 40){
					gap = 13.0f/gapLength;
				}
			}

			ShowData(gd1,1,gap);
			//ShowData(gd2,2,gap);
		}
	}

	public void ClearGraph(){ // 그래프 치우기
		if(holder)
			Destroy(holder);
	}

	int GetDataGap(int dataCount){ // 데이터 띄어놓기
		int value = 1;
		int num = 0;
		while((dataCount-(40+num)) >= 0){
			value+= 1;
			num+= 20;
		}
		
		return value;
	}


	IEnumerator BarGraphBlue(GraphData[] gd,float gap)
	{
		float xIncrement = gap;
		int dataCount = 0;
		bool flag = false;
		Vector3 startpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));//origin.position;//

		while(dataCount < gd.Length)
		{
			
			Vector3 endpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));
			startpoint = new Vector3(startpoint.x,startpoint.y,origin.position.z);
			// pointer is an empty gameObject, i made a prefab of it and attach it in the inspector
			GameObject p = Instantiate(pointer, new Vector3(startpoint.x, startpoint.y, origin.position.z),Quaternion.identity) as GameObject;
			p.transform.parent = holder.transform;


			GameObject lineNumber = Instantiate(xLineNumber, new Vector3(origin.position.x+xIncrement, origin.position.y-0.18f , origin.position.z),Quaternion.identity) as GameObject;
			lineNumber.transform.parent = holder.transform;
			lineNumber.GetComponent<TextMesh>().text = (dataCount+1).ToString();


			// linerenderer is an empty gameObject with Line Renderer Component Attach to it, 
			// i made a prefab of it and attach it in the inspector
			GameObject lineObj = Instantiate(linerenderer,startpoint,Quaternion.identity) as GameObject;
			lineObj.transform.parent = holder.transform;
			lineObj.name = dataCount.ToString();
			
			LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
			
			lineRenderer.material = bluemat;
			lineRenderer.SetWidth(lrWidth, lrWidth);
			lineRenderer.SetVertexCount(2);

			while(Vector3.Distance(p.transform.position,endpoint) > 0.2f)
			{
				float step = 5 * Time.deltaTime;
				p.transform.position = Vector3.MoveTowards(p.transform.position, endpoint, step);
				lineRenderer.SetPosition(0, startpoint);
				lineRenderer.SetPosition(1, p.transform.position);
				
				yield return null;
			}
			
			lineRenderer.SetPosition(0, startpoint);
			lineRenderer.SetPosition(1, endpoint);
			
			
			p.transform.position = endpoint;
			GameObject pointered = Instantiate(pointerRed,endpoint,pointerRed.transform.rotation) as GameObject ;
			pointered.transform.parent = holder.transform;
			startpoint = endpoint;

			if(dataGap > 1){
				if((dataCount+dataGap) == gd.Length){
					dataCount+=dataGap-1;
					flag = true;
				}
				else if((dataCount+dataGap) > gd.Length && !flag){
					dataCount =	gd.Length-1;
					flag = true;
				}
				else{
					dataCount+=dataGap;
					if(dataCount == (gd.Length-1))
						flag = true;
				}
			}
			else
				dataCount+=dataGap;

			xIncrement+= gap;
			
			yield return null;
			
		}
	}

	IEnumerator BarGraphGreen(GraphData[] gd, float gap)
	{
		float xIncrement = gap;
		int dataCount = 0;
		bool flag = false;

		Vector3 startpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));
		while(dataCount < gd.Length)
		{
			
			Vector3 endpoint = new Vector3((origin.position.x+xIncrement),(origin.position.y+gd[dataCount].marbles),(origin.position.z));
			startpoint = new Vector3(startpoint.x,startpoint.y,origin.position.z);
			// pointer is an empty gameObject, i made a prefab of it and attach it in the inspector
			GameObject p = Instantiate(pointer, new Vector3(startpoint.x, startpoint.y, origin.position.z),Quaternion.identity) as GameObject;
			p.transform.parent = holder.transform;
			
			// linerenderer is an empty gameObject with Line Renderer Component Attach to it, 
			// i made a prefab of it and attach it in the inspector
			GameObject lineObj = Instantiate(linerenderer,startpoint,Quaternion.identity) as GameObject;
			lineObj.transform.parent = holder.transform;
			lineObj.name = dataCount.ToString();
			
			LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
			
			lineRenderer.material = greenmat;
			lineRenderer.SetWidth(lrWidth, lrWidth);
			lineRenderer.SetVertexCount(2);

			while(Vector3.Distance(p.transform.position,endpoint) > 0.2f)
			{
				float step = 5 * Time.deltaTime;
				p.transform.position = Vector3.MoveTowards(p.transform.position, endpoint, step);
				lineRenderer.SetPosition(0, startpoint);
				lineRenderer.SetPosition(1, p.transform.position);
				
				yield return null;
			}
			
			lineRenderer.SetPosition(0, startpoint);
			lineRenderer.SetPosition(1, endpoint);
			
			
			p.transform.position = endpoint;
			GameObject pointerblue = Instantiate(pointerBlue,endpoint,pointerBlue.transform.rotation) as GameObject; 
			pointerblue.transform.parent = holder.transform;
			startpoint = endpoint;

			if(dataGap > 1){
				if((dataCount+dataGap) == gd.Length){
					dataCount+=dataGap-1;
					flag = true;
				}
				else if((dataCount+dataGap) > gd.Length && !flag){
					dataCount =	gd.Length-1;
					flag = true;
				}
				else{
					dataCount+=dataGap;
					if(dataCount == (gd.Length-1))
						flag = true;
				}
			}
			else
				dataCount+=dataGap;

			xIncrement+= gap;
			
			yield return null;
			
		}
	}



	public class GraphData
	{
		public float marbles;
	}
}
