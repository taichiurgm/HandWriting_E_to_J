using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using System.Collections.Generic;

namespace HandWriting
{
    public sealed partial class IndexPage : Page
    {
        StorageFolder folder = ApplicationData.Current.LocalFolder;
        StorageFolder levelfolder;

        public IndexPage()
        {
            this.InitializeComponent();
            SelectFolder();
        }

        private async void SelectFolder()
        {
            folder = await folder.CreateFolderAsync("Questions", CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFolder> folderList = await folder.GetFoldersAsync();
            foreach (StorageFolder i in folderList)
            {
                LevelBox.Items.Add(i.Name);
            }

        }

        private async void SelectQuestion(object sender, RoutedEventArgs e)
        {
            QuestionsBox.Items.Clear();
            levelfolder = await folder.CreateFolderAsync(LevelBox.SelectedItem.ToString(), CreationCollisionOption.OpenIfExists);
            IReadOnlyList<StorageFile> fileList = await levelfolder.GetFilesAsync();

            foreach (StorageFile i in fileList)
            {
                QuestionsBox.Items.Add(i.Name.Replace(".csv", ""));
            }
        }



        private async void Start(object sender, RoutedEventArgs e)
        {
            var file = await levelfolder.GetFileAsync(QuestionsBox.SelectedItem.ToString() + ".csv");
            this.Frame.Navigate(typeof(QuestionPage), file);
        }
    }
}
