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
    [Service]
    [IntentFilter(new String[] { "com.xamarin.MainService" })]
    public class MainService : Service
    {
        static readonly string Tag = "X:" + typeof(MainService).Name;
        static readonly int TimerWait = 10 * 60 * 1000;
        Timer _timer;
        DateTime _startTime;
        bool _isStarted = false;
        private MainServiceBinder _binder;
        private int _obsCount = 0;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            Log.Debug(Tag, $"OnStartCommand called at {_startTime}, flags={flags}, startid={startId}");
            if (_isStarted)
            {
                TimeSpan runtime = DateTime.UtcNow.Subtract(_startTime);
                Log.Debug(Tag, $"This service was already started, it's been running for {runtime:c}.");
            }
            else
            {
                _startTime = DateTime.UtcNow;
                Log.Debug(Tag, $"Starting the service, at {_startTime}.");
                Toast.MakeText(this, "The demo service has started", ToastLength.Long).Show();

                _timer = new Timer(HandleTimerCallback, _startTime, 0, TimerWait);
                _isStarted = true;
            }
            return StartCommandResult.NotSticky;
        }

        public override IBinder OnBind(Intent intent)
        {
            _binder = new MainServiceBinder(this);
            return _binder;
            // This is a started service, not a bound service, so we just return null.
            //return null;
        }


        public override void OnDestroy()
        {
            _timer.Dispose();
            _timer = null;
            _isStarted = false;

            TimeSpan runtime = DateTime.UtcNow.Subtract(_startTime);
            Log.Debug(Tag, $"Simple Service destroyed at {DateTime.UtcNow} after running for {runtime:c}.");
            base.OnDestroy();
        }

        void HandleTimerCallback(object state)
        {
            TimeSpan runTime = DateTime.UtcNow.Subtract(_startTime);
            Log.Debug(Tag, $"This service has been running for {runTime:c} (since ${state}).");

            execLogic();
        }

        private void execLogic()
        {
            var absolutePath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string filename = Path.Combine(absolutePath, "change.csv");

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

        internal string GetText()
        {
            Log.Debug(Tag, $"This service will write under followin path {Environment.GetFolderPath(Environment.SpecialFolder.Personal)}");
            return $"We attent {_obsCount} observation";
        }

        public void RunLogic()
        {
            execLogic();
            Toast.MakeText(Application.Context, "Manually application Run.", ToastLength.Long).Show();
        }
    }

    public class MainServiceBinder : Binder
    {
        MainService service;

        public MainServiceBinder(MainService service)
        {
            this.service = service;
        }

        public MainService GetDemoService()
        {
            return service;
        }
    }

}