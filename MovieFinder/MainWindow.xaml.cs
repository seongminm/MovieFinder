using MahApps.Metro.Controls;
using MovieFinder.Logics;
using MovieFinder.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Windows;
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
            if(tmdb_apiKey == null)
            {
                await Commons.ShowMessageAsync("TMDB API", "인증키가 필요합니다");
                return;
            }
            string encoding_movieName = HttpUtility.UrlEncode(movieName, Encoding.UTF8);
            string openApiUrl = $@"https://api.themoviedb.org/3/search/movie?api_key={tmdb_apiKey}&language=ko-KR&page=1&include_adult=false&query={encoding_movieName}";
            string result = string.Empty;   // 결과값

            // api 실행할 객체
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;

            // API 요청
            try
            {
                req = WebRequest.Create(openApiUrl);    // URL을 넣어서 객체를 생성
                res = req.GetResponse();    // 요청한 결과를 응답에 할당
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();    // json 결과 텍스트로 저장

                Debug.WriteLine(result);
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
            this.DataContext = movieItems;
        }

        // 예고편 보기 클릭
        private async void BtnWatchTrailer_Click(object sender, RoutedEventArgs e)
        {
            if(GrdResult.SelectedItems.Count == 0)
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
    }
}
