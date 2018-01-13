using Android.App;
using Android.Content;
using Android.Widget;
using Android.OS;
using Object = Java.Lang.Object;

namespace CoinAppService
{
    [Activity(Label = "CoinAppService", MainLauncher = true)]
    public class MainActivity : Activity
    {
        class MainServiceConnection : Object, IServiceConnection
        {
            readonly MainActivity _activity;
            MainServiceBinder _binder;

            public MainServiceBinder Binder => _binder;

            public MainServiceConnection(MainActivity activity)
            {
                _activity = activity;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                if (service is MainServiceBinder demoServiceBinder)
                {
                    _activity._binder = demoServiceBinder;
                    _activity._isBound = true;

                    // keep instance for preservation across configuration changes
                    _binder = demoServiceBinder;
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                _activity._isBound = false;
            }
        }

        int _count = 1;
        bool _isBound;
        MainServiceBinder _binder;
        private MainServiceConnection _mainServiceConnection;
        private bool _isConfigurationChanged;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.myButton);

            button.Click += delegate { button.Text = $"{_count++} clicks!"; };

            var startButton = FindViewById<Button>(Resource.Id.startService);
            startButton.Click += (sender, args) =>
            {
                Toast.MakeText(Application.Context, "Trying to start service", ToastLength.Long).Show();
                StartService(new Intent(Application.Context, typeof(MainService)));
            };

            var stopButton = FindViewById<Button>(Resource.Id.stopService);
            stopButton.Click += (sender, args) =>
            {
                Toast.MakeText(this, "Trying to stop service", ToastLength.Long).Show();
                StopService(new Intent(Application.Context, typeof(MainService)));
            };

            var callService = FindViewById<Button>(Resource.Id.callService);

            callService.Click += delegate {
                if (_isBound)
                {
                    RunOnUiThread(() => {
                            var text = _binder.GetDemoService().GetText();
                            Toast.MakeText(Application.Context, text, ToastLength.Long).Show();
                        }
                    );
                }
            };

            var manualRun = FindViewById<Button>(Resource.Id.manualRun);

            manualRun.Click += delegate {
                if (_isBound)
                {
                    RunOnUiThread(() => {
                            _binder.GetDemoService().RunLogic();
                        }
                    );
                }
            };

            // restore from connection there was a configuration change, such as a device rotation
            _mainServiceConnection = LastNonConfigurationInstance as MainServiceConnection;

            if (_mainServiceConnection != null)
                _binder = _mainServiceConnection.Binder;
        }

        protected override void OnStart()
        {
            base.OnStart();

            var serviceIntent = new Intent(Application.Context, typeof(MainService));
            _mainServiceConnection = new MainServiceConnection(this);
            BindService(serviceIntent, _mainServiceConnection, Bind.AutoCreate);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (!_isConfigurationChanged)
            {
                if (_isBound)
                {
                    UnbindService(_mainServiceConnection);
                    _isBound = false;
                }
            }
        }

        public override Object OnRetainNonConfigurationInstance()
        {
            base.OnRetainNonConfigurationInstance();
            _isConfigurationChanged = true;
            return _mainServiceConnection;
        }
    }
}

