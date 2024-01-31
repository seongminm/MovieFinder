
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MovieFinder.Logics
{
    internal class Commons
    {
        // mahapps.doc을 참조하여
        //private async void OnButtonClick(object sender, RoutedEventArgs e)
        //{
        //    await this.ShowMessageAsync("This is the title", "Some message");
        //}
        // 를 통해 디자인된 message dialog를 사용할 수 있음
        // 

        public static async Task<MessageDialogResult> ShowMessageAsync(string title, string message)
        {

            return await ((MetroWindow)Application.Current.MainWindow).ShowMessageAsync(title, message);
            // Application.current.Mainwindow는 현재 실행 중인 응용 프로그램에 대한 Applicaiton 클래스의 현재 인스턴스를 나타냄
            // 하지만 ShowMessageAsync 메서드는 MetroWindow의 확장 메서드이므로 (MetroWindow)를 통한 명시적 업캐스팅
        }

        public static readonly string msSql_String = "Data Source=localhost;" +
                                                   "Initial Catalog=MyDatabase;" + 
                                                   "Persist Security Info=True;" +
                                                   "User Id=sa;" + 
                                                   "Password=1234;";


    }
}
