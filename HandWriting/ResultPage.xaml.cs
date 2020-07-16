using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace HandWriting
{
    public sealed partial class ResultPage : Page
    {
        string[] questions;
        string[] _PrevTime;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            string ResultList = e.Parameter as string;

            Show(ResultList);

        }

        public ResultPage()
        {
            this.InitializeComponent();
        }

        public void Show(string ResultList)
        {
            questions = ResultList.Split('\n');

            int count = 0;
            string time = "";

            foreach (string q in questions)
            {
                if (q != "")
                {
                    string[] item = q.Split(',');
                    time += item[2] + ",";
                    if (item[1] == "No")
                    {
                        Number.Items.Add(item[1]);
                    }
                    else
                    {
                        count++;
                        Number.Items.Add(count.ToString());
                    }
                    Question.Items.Add(item[4]);
                    Input.Items.Add(item[5]);
                    Answer.Items.Add(item[3]);

                    if (count == 0)//item[7] == "確信度")
                    {
                        Confidence.Items.Add("確信度");
                    }
                    else if (item[7] == "1")
                    {
                        Confidence.Items.Add("確信あり");
                    }
                    else if (item[7] == "0")
                    {
                        Confidence.Items.Add("確信なし");
                    }
                }
            }
            _PrevTime = time.Split(',');
        }

        private void BackToHome(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(IndexPage));
        }

        private async void Feedback(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            string tmp = Number.SelectedItem.ToString();
            int i = int.Parse(tmp);
            folder = await folder.CreateFolderAsync("Result", CreationCollisionOption.OpenIfExists);
            StorageFile feedbackfile = await folder.CreateFileAsync("result-" + _PrevTime[i] + ".csv", CreationCollisionOption.ReplaceExisting);
            try
            {
                string[] item = questions[i].Split(',');
                string result = "";
                for (i = 0; i < 8; i++)
                {
                    result += item[i] + ",";
                }
                if (item[7] == "1")
                {
                    item[7] = "0";
                }
                else if (item[7] == "0")
                {
                    item[7] = "1";
                }
                result += item[6];

                await FileIO.WriteTextAsync(feedbackfile, result);
            }
            catch
            {
                return;
            }

        }

    }
}
