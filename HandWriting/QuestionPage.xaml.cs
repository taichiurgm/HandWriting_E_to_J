using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;
using Windows.UI.Input.Inking;
using Windows.UI;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Popups;


namespace HandWriting
{
    public static class Id
    {
        public static string id="";
        public static string Id_get_set
        {
            get { return id; }
            set { id = value; }
        }
    }

    public sealed partial class QuestionPage : Page
    {
        int count = 0;//配列にアクセスするためのカウンタ
        int q_num = 1;//問題番号


        List<string> lines;
        int total;

        InkManager inkManager = new InkManager();

        // 最大の線の太さ
        const int MAX_STROKE_WIDTH = 10;

        // ペンのID
        uint _PenID = 0;
        // 前回の位置
        Point _PrevPoint;
        // 開始時刻
        DateTime _PrevTime;

        StorageFolder folder = ApplicationData.Current.LocalFolder;
        StorageFolder datafolder;

        string LogList = "";
        string ResultList = "ファイル名,No,解答開始時間,正答,問題,解答,解答時間,確信度\n";
        string Result = "";
        string QFileName;

        bool press = false;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            StorageFile file = e.Parameter as StorageFile;
            QFileName = file.Name.Replace(".csv", "");
            base.OnNavigatedTo(e);
            Read(file);
        }

        public QuestionPage()
        {
            this.InitializeComponent();
            //ページが変わるとデータは破棄される？ユーザIDは保持したい
            Read_ID();
            if (Id.id=="")
            {
                UserID();
            }
            else
            {
                UserID(Id.id);
            }

            _PrevTime = DateTime.Now;


            InkCanvas.PointerPressed += InkCanvas_PointerPressed;
            InkCanvas.PointerMoved += InkCanvas_PointerMoved;
            InkCanvas.PointerReleased += InkCanvas_PointerReleased;
            InkCanvas.PointerExited += InkCanvas_PointerExited;
        }

        private void BackToHome(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(IndexPage));
        }

        private async void Read(StorageFile file)
        {
            string raw = await FileIO.ReadTextAsync(file);

            string[] separetor = new string[] { "\r\n" };
            lines = raw.Split(separetor, StringSplitOptions.None).ToList();

            lines.RemoveAt(0);
            lines.RemoveAt(lines.Count() - 1);

            lines = lines.OrderBy(i => Guid.NewGuid()).ToList();

            total = lines.Count();

            for (int i = 0; i < total; i++)
            {
                Question.Text += lines[i];
            }

            Show(lines[count]);

        }

        private void Show(string line)
        {
            string[] terms = line.Split(',');
            Result += QFileName + "," + terms[0] + "," + _PrevTime.ToString("yyyy-MM-dd-HH-mm-ss") + "," + terms[2] + "," + terms[3];
            Question.Text = "Q " + q_num + ". \n\n" + /*terms[1]*/  "次の英単語を日本語に訳しなさい。" + "\n\n" + terms[3];
        }

        // ２点間の距離
        public double GetPointDistance(Point po1, Point po2)
        {
            return Math.Sqrt(Math.Pow(po2.X - po1.X, 2) + Math.Pow(po2.Y - po1.Y, 2));
        }

        void InkCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            string log = "Pressed";
            OutputPointerData(e, log);

            press = true;

            PointerPoint po = e.GetCurrentPoint(InkCanvas);
            if (po.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                _PenID = po.PointerId;
                _PrevPoint = po.Position;

                if (po.Properties.IsEraser)
                {
                    // 消しゴムモード
                    inkManager.Mode = InkManipulationMode.Erasing;
                }
                else
                {
                    // ペンモード
                    inkManager.Mode = InkManipulationMode.Inking;
                }
                try
                {
                    inkManager.ProcessPointerDown(po);
                }
                catch
                {
                    return;
                }
            }

            e.Handled = true;
        }

        void InkCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            string log = "Moved";
            OutputPointerData(e, log);

            if (e.Pointer.PointerId == _PenID)
            {

                PointerPoint po = e.GetCurrentPoint(InkCanvas);
                Point currentPoint = po.Position;

                if (GetPointDistance(_PrevPoint, currentPoint) > 0 && press == true && 5 < currentPoint.X && currentPoint.X < InkCanvas.Width - 5 && 5 < currentPoint.Y && currentPoint.Y < InkCanvas.Height - 5)
                {
                    // 線の作成
                    Line l = new Line();
                    l.X1 = _PrevPoint.X;
                    l.Y1 = _PrevPoint.Y;
                    l.X2 = currentPoint.X;
                    l.Y2 = currentPoint.Y;

                    // 線を円形にする
                    l.StrokeStartLineCap = PenLineCap.Round;
                    l.StrokeEndLineCap = PenLineCap.Round;

                    // 線の太さ　
                    // 筆圧によって太さを変える
                    l.StrokeThickness = MAX_STROKE_WIDTH * po.Properties.Pressure;

                    // 線の色
                    if (po.Properties.IsEraser)
                    {
                        l.Stroke = new SolidColorBrush(Colors.LightCyan);
                    }
                    else
                    {
                        l.Stroke = new SolidColorBrush(Colors.Black);
                    }
                    // 線を追加
                    InkCanvas.Children.Add(l);

                    // 現在の位置を保存する
                    _PrevPoint = currentPoint;
                    try
                    {
                        inkManager.ProcessPointerUpdate(po);
                    }
                    catch
                    {
                        return;
                    }
                }
            }

            e.Handled = true;
        }

        void InkCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            string log = "Released";
            OutputPointerData(e, log);

            press = false;

            if (e.Pointer.PointerId == _PenID)
            {
                _PenID = 0;
                try
                {
                    inkManager.ProcessPointerUp(e.GetCurrentPoint(InkCanvas));
                }
                catch
                {
                    return;
                }
            }

            e.Handled = true;
        }

        void InkCanvas_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            // string log = "Exited";
            // OutputPointerData(e, log);
        }

        // PointerRoutedEventArgsの情報を出力する
        private void OutputPointerData(PointerRoutedEventArgs e, string log)
        {
            // InkCanvasに対する情報を取得する
            PointerPoint po = e.GetCurrentPoint(InkCanvas);

            // 時刻の取得
            DateTime dt = DateTime.Now;
            long ts = dt.Ticks - _PrevTime.Ticks;

            log += "," + ts.ToString() + "," + (int)po.Position.X + "," + (int)po.Position.Y;


            // 各デバイスに合わせた情報を表示する
            if (po.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                // マウス
                log += ",Mouse,,";
            }
            else if (po.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
            {
                // ペン
                log += ",Pen";
                // 筆圧
                log += "," + po.Properties.Pressure;
                // 消しゴムかどうか
                log += "," + po.Properties.IsEraser;
            }
            else if (po.PointerDevice.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            {
                // タッチ
                log += ",Touch";
                // 筆圧
                log += "," + po.Properties.Pressure;
                // 消しゴムかどうか
                log += "," + po.Properties.IsEraser;
            }
            log += "\n";
            LogList += log;
        }

        private async void WriteToFile()
        {
            StorageFile logfile = await datafolder.CreateFileAsync("log-" + _PrevTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv", CreationCollisionOption.ReplaceExisting);
            try
            {
                await FileIO.WriteTextAsync(logfile, LogList);
            }
            catch
            {
                return;
            }
            StorageFile resultfile = await datafolder.CreateFileAsync("result-" + _PrevTime.ToString("yyyy-MM-dd-HH-mm-ss") + ".csv", CreationCollisionOption.ReplaceExisting);
            try
            {
                await FileIO.WriteTextAsync(resultfile, Result);
            }
            catch
            {
                return;
            }
        }

        private async void Uncofident(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            long ts = dt.Ticks - _PrevTime.Ticks;
            Recognize();
            await Task.Delay(1000);
            Result += "," + ts.ToString();

            Result += ",0";

            WriteToFile();
            await Task.Delay(1000);
            Reset();
        }

        private async void Confident(object sender, RoutedEventArgs e)
        {
            DateTime dt = DateTime.Now;
            long ts = dt.Ticks - _PrevTime.Ticks;
            Recognize();
            await Task.Delay(1000);

            Result += "," + ts.ToString();

            Result += ",1";

            WriteToFile();
            await Task.Delay(1000);
            Reset();
        }

        // 文字認識
        private async void Recognize()
        {
            string text = "";

            if (inkManager.GetStrokes().Count() != 0)
            {
                var recognizer = inkManager.GetRecognizers().FirstOrDefault(r => r.Name.Contains("日本語"));
                // 文字認識エンジンの設定
                inkManager.SetDefaultRecognizer(recognizer);

                // 文字認識
                IReadOnlyList<InkRecognitionResult> results = await inkManager.RecognizeAsync(InkRecognitionTarget.All);
                text = results.Select(x => x.GetTextCandidates().First()).Aggregate((x, y) => x + y);
            }
            Result += "," + text;
        }

        private void Reset()
        {
            count++;
            q_num++;
            ResultList += Result + "\n";

            if (count < total)
            {
                //初期化
                _PrevTime = DateTime.Now;
                LogList = "";
                Result = "";
                inkManager = new InkManager();
                InkCanvas.Children.Clear();
                Show(lines[count]);

            }
            else
            {
                this.Frame.Navigate(typeof(ResultPage), ResultList);
            }
        }

        //引数なし
        private async void UserID()
        {
            var Dialog = new UserID();
            var res = await Dialog.ShowAsync();

            //userIDを途中で変更する機能も付ける(これから)
            if (res == ContentDialogResult.Primary)
            {
                string userID = Dialog.GetUserID();


                if (userID == "")
                {

                    Id.id = "guest";

                }
                else
                {
                    Id.id = userID;
                }
  
                var tmp = await folder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
                datafolder = await tmp.CreateFolderAsync(Id.id, CreationCollisionOption.OpenIfExists);
            }
        }

        //引数あり
        private async void UserID(string id)
        {
            {
                string userID = id;

                var tmp = await folder.CreateFolderAsync("Data", CreationCollisionOption.OpenIfExists);
                datafolder = await tmp.CreateFolderAsync(userID, CreationCollisionOption.OpenIfExists);
            }
        }



        private async void Write_ID(String id)
        {
            // LocalFolderへの書き込み
            var storage = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await storage.CreateFileAsync("Id_data.txt", Windows.Storage.CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(file, id);
        }


        private async void Read_ID()
        {
            // LocalFolderからの読み込み
            try
            {
                var storage = Windows.Storage.ApplicationData.Current.LocalFolder;
                var file = await storage.GetFileAsync("ID_data.txt");
                var text = await Windows.Storage.FileIO.ReadTextAsync(file);
                Id.id = text;

            }
            catch (Exception)
            {
            // ファイルが存在しない場合の処理
            }
        }


        private void Clear(object sender, RoutedEventArgs e)
        {
            inkManager = new InkManager();
            InkCanvas.Children.Clear();
        }

        private void Quit(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ResultPage), ResultList);
        }



    }
}

