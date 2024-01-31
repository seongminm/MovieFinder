# MovieFinder
 - MahApps 오픈 소스 UI 라이브러리 적용 
 - TMDB API를 사용하여 영화의 개봉일, 인기도, 평점 포스터를 불러올 수 있도록 구현
 - Youtube API를 사용하여 해당 영화의 티저 재생 구현
 - DB와 연동하여 즐겨찾기 CRUD 구현
---

## 실행화면

![TMDB_API](https://github.com/seongminm/MovieFinder/assets/131761210/5b1a606d-1663-41bd-8a0a-792ab825129a)
</br></br>
---
# 주요 소스 코드

## TMDB API 접근
<details>
<summary>MainWindow.xaml.cs</summary>

```C#

        // TMDB API를 통한 영화검색
        private async void SearchMovie(string movieName)
        {
            // 환경 변수에서 인증키 읽어오기
            string tmdb_apiKey = Environment.GetEnvironmentVariable("TMDB_API");
            if(tmdb_apiKey == null)
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
```
</details>

</br>

<details>
<summary> API 호출 결과</summary>

```json
﻿{
  "page": 1,
  "results": [
    {
      "adult": false,
      "backdrop_path": null,
      "genre_ids": [ 10749 ],
      "id": 695906,
      "original_language": "ko",
      "original_title": "서울",
      "overview": "사랑에 빠지게 만드는 마법 같은 도시 | 낯선 그녀와의 짜릿한 로맨스  윤 감독은 서울을 배경으로 하는 청춘 로맨스 영화를 기획 중이다. 서울의 숨겨진 명소를 헌팅하고, 이어 연출부 스텝 구성과 배우 캐스팅까지 순조롭게 마친다. 그러던 어느 날, 촬영을 일주일 남겨놓고 남자배우인 성진은 촬영에서 하차하게 되고, 성진의 대타로 연출부 스텝이었던 채만이 출연을 결정하게 된다.  영화 속 채만과 지혜. 채만은 우연히 길에서 마주친 여행객 지혜를 보고 한 눈에 반하게 된다. 어린 시절해외로 입양되었다는 지혜 역시 다정하고 친절하게 길을 설명해주며 자신과 동행해주는 채만에게 점차 마음을 열게 된다. 한편 지혜는 짧은 여행을 마치고 출국을 앞둬 채만과 아쉬운 작별을 해야 하는데…",
      "popularity": 0.951,
      "poster_path": "/oNyte3Od2jqG1mj9MwIgwOuJH3b.jpg",
      "release_date": "2010-04-22",
      "title": "서울",
      "video": false,
      "vote_average": 5.0,
      "vote_count": 1
    },
    {
      "adult": false,
      "backdrop_path": "/7MkXMcLdlSzfhUsbJnb8ADimE6P.jpg",
      "genre_ids": [ 53, 28 ],
      "id": 277496,
      "original_language": "ja",
      "original_title": "ソウル",
      "overview": "범인을 인도하러 서울에 온 일본 형사 하야세 유타로(나가세 토모야)는 현금수송차를 강탈하는 사건을 목격하고 범인들을 뒤쫓는다. 그 와중에 경찰과 범인이 각각 1명씩 사망하고, 1명이 도주한다. 서울 경시청으로 호송당한 하야세 유타로는 형사부장 김윤철(최민수)에게 느닷없이 한 방을 맞고, 이후 \"여긴 한국이야!\" \"시간 엄수!\" \"명령에 따라!\" 등등의 말투에 시달리며 사건 조사에 참여한다. '민족의 새벽'이라는 테러 단체가 8개국 정상회담을 이용한 은행강탈사건을 벌이자 유타로는 김윤철 부장과 사건을 체류기간인 72시간 안에 해결하기 위해 협력한다.",
      "popularity": 1.96,
      "poster_path": "/8bONU4vYjDkO6VaSRdR49zNGIBm.jpg",
      "release_date": "2002-02-09",
      "title": "서울",
      "video": false,
      "vote_average": 5.0,
      "vote_count": 1
    }
  ],
  "total_pages": 3,
  "total_results": 46
}
```
</details>
</br>

---

## Youtube API 접근 
- https://developers.google.com/youtube/v3/code_samples/dotnet?hl=ko#search_by_keyword 참조 </br></br>

![Youtube_API](https://github.com/seongminm/MovieFinder/assets/131761210/7e3f0620-6985-4e16-9e68-f237cef613d3)

<details>
<summary>TrailerWindow.xaml.cs</summary>
 
```C#

        private async Task LoadDataCollection()
        {

            var youtubeService = new YouTubeService(
                new BaseClientService.Initializer()
                {
                    ApiKey = Environment.GetEnvironmentVariable("YOUTUBE_API"),
                    ApplicationName = this.GetType().ToString()
                }
                );

            var req = youtubeService.Search.List("snippet");
            req.Q = LblMovieName.Content.ToString();
            req.MaxResults = 10;

            var res = await req.ExecuteAsync(); // 검색결과를 받아옴

            //Debug.WriteLine("유튜브 검색결과--------------");

            foreach (var item in res.Items)
            {
                //Debug.WriteLine(item.Snippet.Title);
                if (item.Id.Kind.Equals("youtube#video"))   // youtube#video만 동영상 플레이 가능
                {
                    YoutubeItem youtube = new YoutubeItem()
                    {
                        // HTML 엔티티 표현을 제거하기 위한 디코딩
                        Title = WebUtility.HtmlDecode(item.Snippet.Title),
                        //Title = item.Snippet.Title,
                        ChannelTitle = item.Snippet.ChannelTitle,
                        URL = $"https://www.youtube.com/watch?v={item.Id.VideoId}", // 유튜브 플레이 링크
                        //Author = item.Snippet.ChannelTitle
                    };
                 
                    youtube.Thumbnail = new BitmapImage(new Uri(item.Snippet.Thumbnails.Default__.Url, UriKind.RelativeOrAbsolute));
                    youtubeItems.Add(youtube);

                }
            }
        }
```
</details>
</br>

<details>
<summary> CefSharp을 통해 크롬 브라우저 사용 TrailerWindow.xaml</summary>
 
```XAML

<Grid Grid.Row="0" Grid.Column="1" Grid.RowSpan="2" Margin="10" Background="Gainsboro">
            <cefSharp:ChromiumWebBrowser x:Name="BrsYoutube" Address=""/>
```

```C#
// TrailerWindow.xaml.cs

        private void LsvResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LsvResult.SelectedItem is YoutubeItem)
            {
                var video = LsvResult.SelectedItem as YoutubeItem;
                BrsYoutube.Address = video.URL;
            }
        }
```
</details>
</br>

---

## 즐겨찾기 CRUD 
![즐겨찾기추가](https://github.com/seongminm/MovieFinder/assets/131761210/37eeebf9-8b69-4dd8-b6b5-c3d1b26241d0)

<details>
<summary>즐겨찾기 추가</summary>
 
```C#

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

```
</details>
</br>


![즐겨찾기보기](https://github.com/seongminm/MovieFinder/assets/131761210/596460f1-e545-4788-8ea5-f7db81f32ae3)

<details>
<summary>즐겨찾기 보기</summary>
 
```C#

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

```
</details>
</br>

![즐겨찾기삭제](https://github.com/seongminm/MovieFinder/assets/131761210/c97cc646-c8fb-4acd-aac8-274f58615178)

<details>
<summary>즐겨찾기 삭제</summary> 
 
```C#

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
    
```

</details>
</br>



---

# 개발환경
:heavy_check_mark: WPF [.NET Framework 4.8](https://dotnet.microsoft.com/ko-kr/download/dotnet-framework/net48)

:heavy_check_mark: Visual Studio 2019

<img src="https://github.com/37inm/GrblController/assets/131761210/673f9ef5-07f9-48ee-aaf2-7e659e2c8af7" width="400"/>
