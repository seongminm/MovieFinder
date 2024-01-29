using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MahApps.Metro.Controls;
using MovieFinder.Logics;
using MovieFinder.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MovieFinder
{
    /// <summary>
    /// TrailerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TrailerWindow : MetroWindow
    {
        List<YoutubeItem> youtubeItems = null;  //검색결과 담을 리스트
        public TrailerWindow()
        {
            InitializeComponent();
        }

        // 부모에서 데이터를 가져올려면 재정의 생성자 필요
        public TrailerWindow(string movieName) : this()
        {
            LblMovieName.Content = $"{movieName} 예고편";
        }

        public TrailerWindow(MovieItem movie) : this()
        {
            LblMovieName.Content = $"{movie.Title} 예고편";
        }

        // 윈도우 로드 시
        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            youtubeItems = new List<YoutubeItem>(); // 초기화
            SearchYoutubeApi();
        }

        private async void SearchYoutubeApi()
        {
            await LoadDataCollection();
            LsvResult.ItemsSource = youtubeItems;   // direct binding
        }

        private async Task LoadDataCollection()
        {
            // https://developers.google.com/youtube/v3/code_samples/dotnet?hl=ko#search_by_keyword 참조

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

        // 목록 더블 클릭 시 
        private void LsvResult_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LsvResult.SelectedItem is YoutubeItem)
            {
                var video = LsvResult.SelectedItem as YoutubeItem;
                BrsYoutube.Address = video.URL;
            }
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            BrsYoutube.Address = string.Empty;  // 웹브라우저 주소 클리어
            BrsYoutube.Dispose();   // 리소스 해제
        }


    }
}
