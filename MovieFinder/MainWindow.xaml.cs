using MahApps.Metro.Controls;
using MovieFinder.Logics;
using MovieFinder.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MovieFinder
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // 메인윈도우 로드 시 호출
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            TxtMovieName.Focus();
        }

        // 텍스트박스에서 키 입력 시 엔터 누르면 검색 시작
        private void TxtMovieName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnSearchMovie_Click(sender, e); // BtnSearchMovie_Click를 거치는 이유는 예외 처리를 재사용하기 위함
            }
        }

        // 검색 버튼 클릭 시 검색 메서드 호출
        private async void BtnSearchMovie_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TxtMovieName.Text))
            {
                await Commons.ShowMessageAsync("검색", "검색할 영화명을 입력하시오."); // 정적 메서드를 활용
                return;
            }
            try
            {
                SearchMovie(TxtMovieName.Text);
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"※오류 발생 : {ex.Message}");
            }

        }

        // 그리드의 셀을 선택하거나 변경할 경우
        private async void GrdResult_SelectedCellsChanged(object sender, System.Windows.Controls.SelectedCellsChangedEventArgs e)
        {
            try
            {
                string posterPath = string.Empty;

                if (GrdResult.SelectedItem is MovieItem)
                {
                    var movie = GrdResult.SelectedItem as MovieItem;
                    posterPath = movie.Poster_Path;
                } else if(GrdResult.SelectedItem is FavoriteMovieItem)
                {
                    var movie = GrdResult.SelectedItem as FavoriteMovieItem;
                    posterPath = movie.Poster_Path;
                }

                if (string.IsNullOrEmpty(posterPath)) // 포스터 이미지가 없으면 No_Picture
                {
                    ImgPoster.Source = new BitmapImage(new Uri("Resources/No_Picture.png", UriKind.RelativeOrAbsolute));
                }
                else    // 있으면
                {
                    var base_url = $"https://image.tmdb.org/t/p/w300_and_h450_bestv2";
                    ImgPoster.Source = new BitmapImage(new Uri($"{base_url}{posterPath}", UriKind.RelativeOrAbsolute));
                }
            }
            catch
            {
                await Commons.ShowMessageAsync("오류", $"이미지로드 오류 발생");
            }
        }

        // TMDB API를 통한 영화검색
        private async void SearchMovie(string movieName)
        {
            string tmdb_apiKey = Environment.GetEnvironmentVariable("TMDB_API");
            if (tmdb_apiKey == null)
            {
                await Commons.ShowMessageAsync("TMDB API", "인증키가 필요합니다");
                return;
            }
            /* 인코딩 : 정보나 데이터를 특정 형식으로 변환하는 과정
             * 대부분의 웹 브라우저나 HTTP클라이언트에서는 자동으로 인코딩을 처리하지만
             * 명시적인 인코딩 과정을 통해 모든 상황에서 안전한 전송을 보장하기 위한 좋은 습관
             */
            string encoding_movieName = HttpUtility.UrlEncode(movieName, Encoding.UTF8);


            string openApiUrl = $@"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}&language=ko-KR&page=1&include_adult=false&query={encoding_movieName}";

            string result = string.Empty;   // 결과값

            // api 실행할 객체
            WebRequest req = null; // 웹 리소스에 대한 요청을 만들기 위한 추상클래스
            WebResponse res = null; // 웹 리소스에 대한 요청을 나타내는 추상클래스
            StreamReader reader = null; //텍스트 데이터를 읽기 위한 기능을 제공

            // API 요청
            try
            {
                req = WebRequest.Create(openApiUrl);    // URL을 넣어서 객체를 생성
                res = req.GetResponse();    // 요청한 결과를 응답에 할당
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();    // json 결과 텍스트로 저장

                //Debug.WriteLine(result); 디버깅을 위한 코드
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                reader.Close();
                res.Close();
            }

            // result를 json으로 변경
            var jsonResult = JObject.Parse(result); // string -> json

            var total = Convert.ToInt32(jsonResult["total_results"]);   // 전체 검색결과 수

            //await Commons.ShowMessageAsync("검색결과", total.ToString());
            var items = jsonResult["results"];
            // items를 데이터그리드에 표시
            var json_array = items as JArray;

            var movieItems = new List<MovieItem>(); // json에서 넘어온 배열을 담을 장소
            foreach (var val in json_array)
            {
                var MovieItem = new MovieItem()
                {
                    Adult = Convert.ToBoolean(val["adult"]),
                    Id = Convert.ToInt32(val["id"]),
                    Original_Language = Convert.ToString(val["original_languagte"]),
                    Original_Title = Convert.ToString(val["original_title"]),
                    Overview = Convert.ToString(val["overview"]),
                    Popularity = Convert.ToDouble(val["popularity"]),
                    Poster_Path = Convert.ToString(val["poster_path"]),
                    Title = Convert.ToString(val["title"]),
                    Release_Date = Convert.ToString(val["release_date"]),
                    Vote_Average = Convert.ToDouble(val["vote_average"]),
                };
                movieItems.Add(MovieItem);
            }


            StsResult.Content = $"OpenAPI 조회 {movieItems.Count} 건 조회 완료";
            //GrdResult.DataContext = movieItems;
            this.DataContext = movieItems;
        }

        // 예고편 보기 클릭
        private async void BtnWatchTrailer_Click(object sender, RoutedEventArgs e)
        {
            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("유튜브", "영화를 선택하세요");
                return;
            }
            if (GrdResult.SelectedItems.Count > 1)
            {
                await Commons.ShowMessageAsync("유튜브", "영화를 하나만 선택하세요");
                return;
            }
            if (Environment.GetEnvironmentVariable("YOUTUBE_API") == null)
            {
                await Commons.ShowMessageAsync("유튜브 API", "인증키가 필요합니다.");
                return;
            }
            string movieName = string.Empty;
            var movie = GrdResult.SelectedItem as MovieItem;

            movieName = (GrdResult.SelectedItem as MovieItem).Title;
            // await Commons.ShowMessageAsync("유튜브", $"예고편 볼 영화 {movieName}");
            var trailerWindow = new TrailerWindow(movieName);

            trailerWindow.Owner = this; // TrailerWindow의 부모는 MainWindow
            trailerWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner; // 부모창의 정중앙에 위치
            // trailerWindow.Show(); // 모달리스로 창을 열면 부모창을 손댈 수 있기 때문에
            trailerWindow.ShowDialog(); // 모달창
        }

        // 즐겨찾기 보기
        private void BtnStateView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(Commons.msSql_String))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = @"SELECT Id
                                      , Title
                                      , Original_Title
                                      , Release_Date
                                      , Original_Language
                                      , Adult
                                      , Popularity
                                      , Vote_Average
                                      , Poster_Path
                                      , Overview
                                      , Reg_Date
                                  FROM FavoriteMovieItem
                                 ORDER BY Id ASC";
                    var cmd = new SqlCommand(query, conn); // SqlCommand, SQL의 명령문을 나타냄
                    var adapter = new SqlDataAdapter(cmd); // 데이터베이스와 데이터를 주고받기 위한 중간 계층 역할
                    var dataSet = new DataSet(); // 메모리상의 간이 데이터베이스와 같은 개념
                                                 // dataSet.Tables[0]과 같이 인덱스를 사용할 수 있으며 
                                                 // dataSet.Tables["Tab1"] 과 같이 엑세스할 수 있음
                    adapter.Fill(dataSet, "TableName");


                    var favoriteMovieItem = new List<FavoriteMovieItem>();
                    // var dset 일 경우 오류 발생
                    foreach (DataRow dset in dataSet.Tables["TableName"].Rows)
                    {
                        var FavoriteMovieItem = new FavoriteMovieItem
                        {
                            Id = Convert.ToInt32(dset["id"]),
                            Title = Convert.ToString(dset["title"]),
                            Original_Title = Convert.ToString(dset["Original_Title"]),
                            Release_Date = Convert.ToString(dset["Release_Date"]),
                            Original_Language = Convert.ToString(dset["Original_Language"]),
                            Adult = Convert.ToBoolean(dset["Adult"]),
                            Popularity = Convert.ToDouble(dset["Popularity"]),
                            Vote_Average = Convert.ToDouble(dset["Vote_Average"]),
                            Poster_Path = Convert.ToString(dset["Poster_Path"]),
                            Overview = Convert.ToString(dset["Overview"]),
                            Reg_Date = Convert.ToDateTime(dset["Reg_Date"])
                        };
                        favoriteMovieItem.Add(FavoriteMovieItem);
                    }
                    this.DataContext = favoriteMovieItem;
                    StsResult.Content = $"즐겨찾기 {favoriteMovieItem.Count} 건 조회 완료";
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}
