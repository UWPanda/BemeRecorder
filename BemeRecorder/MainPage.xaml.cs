using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BemeRecorder
{
    public sealed partial class MainPage : Page
    {
        private ProximitySensor sensor;
        private DeviceWatcher watcher;
        private bool isRecording;
        private bool isInitialized;
        private MediaCapture mediaCapture;
        private static Timer timer;

        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            watcher = DeviceInformation.CreateWatcher(ProximitySensor.GetDeviceSelector());
            watcher.Added += OnProximitySensorAdded;
            watcher.Start();
            timer = new Timer(TimerCallBack, null, Timeout.Infinite, 4000);
            await InitializeCameraAsync();
        }

        private async Task InitializeCameraAsync()
        {
            if (mediaCapture == null)
            {
                // get camera device (back camera preferred)
                var cameraDevice = await FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel.Back);

                if (cameraDevice == null)
                {
                    Debug.WriteLine("no camera device found");
                    return;
                }

                // Create MediaCapture and its settings
                mediaCapture = new MediaCapture();

                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                // Initialize MediaCapture
                try
                {
                    await mediaCapture.InitializeAsync(settings);
                    isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("access to the camera denied");
                }
            }
        }

        private static async Task<DeviceInformation> FindCameraDeviceByPanelAsync(Windows.Devices.Enumeration.Panel desiredPanel)
        {
            // get all camera devices
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // get camera by panel
            DeviceInformation desiredDevice = allVideoDevices.FirstOrDefault(x => x.EnclosureLocation != null && x.EnclosureLocation.Panel == desiredPanel);

            // if there is no camera on the desired panel - return default camera
            return desiredDevice ?? allVideoDevices.FirstOrDefault();
        }

        private void OnProximitySensorAdded(DeviceWatcher sender, DeviceInformation device)
        {
            if (null == sensor)
            {
                ProximitySensor foundSensor = ProximitySensor.FromId(device.Id);
                if (null != foundSensor)
                {
                    sensor = foundSensor;
                    sensor.ReadingChanged += Sensor_ReadingChanged;
                }
                else
                {
                    Debug.WriteLine("device has no proximity sensor");
                }
            }
        }
        private async void Sensor_ReadingChanged(ProximitySensor sender, ProximitySensorReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                ProximitySensorReading reading = e.Reading;
                if (null != reading)
                {
                    if (reading.IsDetected)
                    {
                        //hide status bar
                        if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                        {
                            var statusBar = StatusBar.GetForCurrentView();
                            await statusBar.HideAsync();
                        }
                        //enable timer
                        timer.Change(4000, 4000);
                        Overlay_RecordingTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                        Overlay_RecordingTextBlock.Text = "Recording...";
                        Overlay_RecordingTextBlock.Visibility = Visibility.Visible;
                        Overlay_Grid.Visibility = Visibility.Visible;
                        await Task.Delay(1000);
                        Overlay_RecordingTextBlock.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        if (isRecording)
                        {
                            //disable timer
                            timer.Change(Timeout.Infinite, 4000);
                            Overlay_RecordingTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                            Overlay_RecordingTextBlock.Text = "Done...";
                            Overlay_RecordingTextBlock.Visibility = Visibility.Visible;
                            await Task.Delay(1000);
                            Overlay_Grid.Visibility = Visibility.Collapsed;
                            await StopRecordingAsync();
                            isRecording = false;
                        }
                        else
                        {
                            timer.Change(Timeout.Infinite, 4000);
                            Overlay_RecordingTextBlock.Foreground = new SolidColorBrush(Color.FromArgb(100, 255, 255, 255));
                            Overlay_RecordingTextBlock.Text = "Canceled...";
                            Overlay_RecordingTextBlock.Visibility = Visibility.Visible;
                            await Task.Delay(1000);
                            Overlay_Grid.Visibility = Visibility.Collapsed;
                            isRecording = false;
                        }
                    }
                }
            });
        }

        private async Task StartRecordingAsync()
        {
            try
            {
                //create video file in picture library
                var videoFile = await KnownFolders.PicturesLibrary.CreateFileAsync("Beme.mp4", CreationCollisionOption.GenerateUniqueName);

                var encodingProfile = MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto);
                await mediaCapture.StartRecordToStorageFileAsync(encodingProfile, videoFile);
                isRecording = true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async Task StopRecordingAsync()
        {
            isRecording = false;
            await mediaCapture.StopRecordAsync();
        }
        private async void TimerCallBack(object state)
        {
            if (!isRecording)
            {
                await StartRecordingAsync();
                isRecording = true;
            }
        }
    }
}