using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Graph
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent(); // 동기화
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 데이터 값 불러오는 방법
            /* 1. C# 특성상 전역공간 개념이 없기 때문에 class 안의 변수로 접근하려면 그 클래스의 인스턴스로 접근
             */

            // 더미 데이터 모음집, 데이터 받으면 수정할 목록 
            float[] errorRate = new[] { 100f, 150f, 130f, 240f, 500f, 600f, 125f, 364f, 425f, 632f, 124f}; // 더미 Error Rate, 데이터 받은 후에 추가 예정
            float[] timeRate = new[] { 0f, 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f, 9f, 10f }; // 더미 시간 데이터, 데이터 받은 후에 추가 예정

            // 차트 Length 
            int errorRateLength; // ErrorRateLength 길이
            errorRateLength = 10; // 10이라 가정 (임의값 넣어주면 될듯)

            // 그래프 값 넣기
            for (int i = 0; i <= errorRateLength; i++)chart1.Series[0].Points.AddXY(timeRate[i], errorRate[i]);
           
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            // 그래프 꾸미기
            chart1.Series[0].Name = "ErrorRate"; // 오른쪽 파란색 라벨값 ErrorRate로

            // 차트 종류 설정
            chart1.Series[0].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line; // 차트 Type Line으로

            // 차트 선 굵기 설정
            chart1.Series[0].BorderWidth = 3;

            // 차트 색상 설정
            chart1.Series[0].Color = Color.Blue;

            // 차트 타이틀
            chart1.Titles[0].Text = "타이틀 적는 곳";
        }

        private void Chart1_Click(object sender, EventArgs e)
        {

        }

     
    }
}
