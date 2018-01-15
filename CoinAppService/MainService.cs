using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using Environment = System.Environment;

namespace CoinAppService
{
    [Service()]
    [IntentFilter(new [] { "com.xamarin.MainService" })]
    public class MainService : Service
    {
        static readonly string Tag = "X:" + typeof(MainService).Name;
        static readonly int TimerWait = 5 * 60 * 1000;
        Timer _timer;
        DateTime _startTime;
        bool _isStarted;
        private MainServiceBinder _binder;
        private int _obsCount;
        private readonly string _absolutePath = $"{Android.OS.Environment.ExternalStorageDirectory.AbsolutePath}{Path.DirectorySeparatorChar}com.ale7canna.appcoinservice{Path.DirectorySeparatorChar}";

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            CheckDirectoryExist();

            MyLog(Tag, $"OnStartCommand called at {_startTime}, flags={flags}, startid={startId}");
            if (_isStarted)
            {
                TimeSpan runtime = DateTime.UtcNow.Subtract(_startTime);
                MyLog(Tag, $"This service was already started, it's been running for {runtime:c}.");
            }
            else
            {
                _startTime = DateTime.UtcNow;
                MyLog(Tag, $"Starting the service, at {_startTime}.");
                Toast.MakeText(this, "The demo service has started", ToastLength.Long).Show();

                _timer = new Timer(HandleTimerCallback, _startTime, 0, TimerWait);
                _isStarted = true;
            }
            return StartCommandResult.Sticky;
        }

        private void CheckDirectoryExist()
        {
            if (Directory.Exists(_absolutePath))
                return;

            Directory.CreateDirectory(_absolutePath);
            MyLog(Tag, $"Folder {_absolutePath} was created successfully.");
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new MainServiceBinder(this);
            return _binder;
        }


        public override void OnDestroy()
        {
            _timer.Dispose();
            _timer = null;
            _isStarted = false;

            TimeSpan runtime = DateTime.UtcNow.Subtract(_startTime);
            MyLog(Tag, $"Simple Service destroyed at {DateTime.UtcNow} after running for {runtime:c}.");
            base.OnDestroy();
        }

        void HandleTimerCallback(object state)
        {
            TimeSpan runTime = DateTime.UtcNow.Subtract(_startTime);
            MyLog(Tag, $"This service has been running for {runTime:c} (since ${state}).");

            try
            {
                ExecLogic();
            }
            catch (Exception e)
            {
                MyLog(e);
            }
        }

        private void ExecLogic()
        {
            var filename = Path.Combine(_absolutePath, "change.csv");

            if (!File.Exists(filename))
            {
                IEnumerable<string> lines = new List<string>()
                {
                    "CAMBIO CHF/EUR",
                    "\"Giorno e ora\";\"cambio\""
                };
                File.AppendAllLines(filename, lines);
            }

            var pageDwnldr = new PageDownloader();

            var page = pageDwnldr.DownloadPage();
            var result = page.GetChangeValue();

            File.AppendAllLines(filename, new[] { $"\"{DateTime.Now.ToString("s", CultureInfo.InvariantCulture)}\";\"{result.ToString(CultureInfo.InvariantCulture)}\"" });

            _obsCount++;
        }

        private void MyLog(Exception exc)
        {
            while (exc != null)
            {
                MyLog(Tag, exc.Message);
                exc = exc.InnerException;
            }
        }

        private void MyLog(string tag, string content)
        {
            Log.Debug(tag, content);
            LogOnFile(content);
        }

        private void LogOnFile(string content)
        {
            var filename = Path.Combine(_absolutePath, "log.txt");
            File.AppendAllLines(filename, new [] { $"{DateTime.Now:s}__{content}"});
        }

        internal string GetText()
        {
            MyLog(Tag, $"This service will write under following path {Environment.GetFolderPath(Environment.SpecialFolder.Personal)}");
            return $"We attent {_obsCount} observation";
        }

        public void RunLogic()
        {
            ExecLogic();
            Toast.MakeText(Application.Context, "Manually application Run.", ToastLength.Long).Show();
        }
    }

    public class MainServiceBinder : Binder
    {
        readonly MainService _service;

        public MainServiceBinder(MainService service)
        {
            _service = service;
        }

        public MainService GetDemoService()
        {
            return _service;
        }
    }

}