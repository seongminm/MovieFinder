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
        bool isFavorite = false; // 즐겨찾기 모드
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
            isFavorite = false;
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
        private async void BtnStateView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                /* using문 사용 이유
                 * using문은 블록을 나갈 때 자동으로 Dispose 메서드를 호출하여 자원을 해제
                 * 명시적인 자원 관리는(파일 핸들, 네트워크 연결, 데이터베이스 연결 등)에서 중요
                 * 특히 데이터베이스 연결과 같은 리소스는 제한된 수의 연결을 가지고 있을 수 있으며, 
                 * 이러한 연결한 오랫동안 열어두는 것은 성능 문제를 일으킬 수 있음
                 */
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
                    isFavorite = true;
                    TxtMovieName.Text = "";
                }
            }
            catch (Exception ex)
            {   
                await Commons.ShowMessageAsync("오류", $"DB조회 오류 {ex.Message}");
            }
        }

        // 즐겨찾기 추가
        private async void BtnAddFavorite_Click(object sender, RoutedEventArgs e)
        {
            if(GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기에 추가할 영화를 선택하세요(복수선택 가능).");
                return;
            }
            if(isFavorite)
            {
                // 즐겨찾기 모드일 경우
                await Commons.ShowMessageAsync("오류", "이미 즐겨찾기한 영화입니다.");
                return;
            }

            try
            {
                using(SqlConnection conn = new SqlConnection(Commons.msSql_String))
                {
                    if (conn.State == ConnectionState.Closed) conn.Open();

                    var query = @"INSERT INTO [dbo].[FavoriteMovieItem]
                                               ([Id]
                                               ,[Title]
                                               ,[Original_Title]
                                               ,[Release_Date]
                                               ,[Original_Language]
                                               ,[Adult]
                                               ,[Popularity]
                                               ,[Vote_Average]
                                               ,[Poster_Path]
                                               ,[Overview]
                                               ,[Reg_Date])
                                         VALUES
                                               (@Id
                                               ,@Title
                                               ,@Original_Title
                                               ,@Release_Date
                                               ,@Original_Language
                                               ,@Adult
                                               ,@Popularity
                                               ,@Vote_Average
                                               ,@Poster_Path
                                               ,@Overview
                                               ,@Reg_Date)";
                    int count = 0;
                    foreach (MovieItem item in GrdResult.SelectedItems)  // openAPI로 조회된 결과 -> MovieItem
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);

                        cmd.Parameters.AddWithValue("@Id", item.Id);
                        cmd.Parameters.AddWithValue("@Title", item.Title);
                        cmd.Parameters.AddWithValue("@Original_Title", item.Original_Title);
                        cmd.Parameters.AddWithValue("@Release_Date", item.Release_Date);
                        cmd.Parameters.AddWithValue("@Original_Language", item.Original_Language);
                        cmd.Parameters.AddWithValue("@Adult", item.Adult);
                        cmd.Parameters.AddWithValue("@Popularity", item.Popularity);
                        cmd.Parameters.AddWithValue("@Vote_Average", item.Vote_Average);
                        cmd.Parameters.AddWithValue("@Poster_Path", item.Poster_Path);
                        cmd.Parameters.AddWithValue("@Overview", item.Overview);
                        cmd.Parameters.AddWithValue("@Reg_Date", DateTime.Now);

                        cmd.ExecuteNonQuery();
                        count++;
                    }

                    await Commons.ShowMessageAsync("즐겨찾기", "즐겨찾기 저장 완료");
                    StsResult.Content = $"즐겨찾기 {count} 건 저장 완료";
                }

            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB 저장 오류 : {ex.Message}");
            }
        }

        // 즐겨찾기 제거
        private async void BtnDeleteFavorite_Click(object sender, RoutedEventArgs e)
        {
            if (isFavorite == false)
            {
                await Commons.ShowMessageAsync("오류", "즐겨찾기만 삭제할 수 있습니다.");
                return;
            }

            if (GrdResult.SelectedItems.Count == 0)
            {
                await Commons.ShowMessageAsync("오류", "삭재할 영화를 선택하시오.");
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(Commons.msSql_String))
                {
                    if (conn.State == ConnectionState.Closed) { conn.Open(); }

                    var query = @"DELETE FROM FavoriteMovieItem WHERE Id=@Id";
                    var count = 0;

                    foreach (FavoriteMovieItem item in GrdResult.SelectedItems)
                    {
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Id", item.Id);

                        cmd.ExecuteNonQuery();
                        count++;
                    }

                     await Commons.ShowMessageAsync("삭제", $"{count}건 즐겨찾기 삭제");
                     StsResult.Content = $"즐겨찾기 {count} 건 삭제 완료";

                    
                }
            }
            catch (Exception ex)
            {
                await Commons.ShowMessageAsync("오류", $"DB삭제 오류 {ex.Message}");
            }

            BtnStateView_Click(sender, e);    // 삭제 후 즐겨찾기 보기 이벤트 핸들러 실행
        }
    }
}
