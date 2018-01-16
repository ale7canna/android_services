using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Widget;
using BusinessLogic;
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

        Logic _bl = new Logic();
        private PowerManager.WakeLock _wakeLock;
        private int _wakeLockCount;
        private object _state;

        public override void OnCreate()
        {
            base.OnCreate();

            var powerManager = (PowerManager)this.GetSystemService("power");
            _wakeLock = powerManager.NewWakeLock(WakeLockFlags.Partial, "servicewakelock");
            _wakeLock.SetReferenceCounted(false);
        }

        public void SetWakeLock()
        {
            if (!_wakeLock.IsHeld) return;

            _wakeLock.Acquire();
            _wakeLockCount++;
            MyLog(Tag, "WakeLock Armed");
        }

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
            _state = state;
            try
            {
                SetWakeLock();
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
                    "\"Giorno e ora\";\"CambiaValute\";\"Xe Change\";\"Difference\";\"Diff Percentage\""
                };
                File.AppendAllLines(filename, lines);
            }

            var cambiaValuteValue = _bl.GetCambiaValuteValue();
            var xeValue = _bl.GetXeChangeValue();

            var difference = xeValue - cambiaValuteValue;
            var diffPercentage = (decimal)100 * difference / xeValue;

            var values = new[]
            {
                DateTime.Now.ToString("s", CultureInfo.InvariantCulture),
                cambiaValuteValue.ToString(CultureInfo.InvariantCulture),
                xeValue.ToString(CultureInfo.InvariantCulture),
                difference.ToString(CultureInfo.InvariantCulture),
                diffPercentage.ToString(CultureInfo.InvariantCulture)
            };
            var line = String.Join(";", values.Select(v => $"\"{v}\""));
            File.AppendAllLines(filename, new[]
            {
                $"{line}"
            });

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
            TimeSpan runTime = DateTime.UtcNow.Subtract(_startTime);

            MyLog(Tag, $"This service will write under following path {Environment.GetFolderPath(Environment.SpecialFolder.Personal)}");
            MyLog(Tag, $"This service has been running for {runTime:c} (since ${_state}).");
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