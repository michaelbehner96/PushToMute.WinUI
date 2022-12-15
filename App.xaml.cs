using KeyShark;
using KeyShark.Native;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using PushToMute.Core.Services;
using PushToMute.Core.ViewModels;
using PushToMute.WinUI.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using WinUIEx;

namespace PushToMute.WinUI
{
    public partial class App : Application
    {
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services { get; private set; }
        public Window MainWindow { get; private set; }
        public Frame RootFrame => MainWindow?.Content as Frame;

        public App()
        {
            Services = ConfigureServices();
            LoadSettings();
            this.InitializeComponent();
        }

        public IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<SystemVolumeConfigurator>();

            services.AddTransient<IKeyStateTracker, SimpleKeyStateTracker>();
            services.AddSingleton<IKeyboardListener, SimpleKeyboardListener>();
            services.AddSingleton<IKeyRecorder>((sp) =>
            {
                var auxKeys = new VKey[] { VKey.LSHIFT, VKey.LMENU, VKey.LCONTROL, VKey.RSHIFT, VKey.RMENU, VKey.RCONTROL, VKey.ESCAPE };
                return new SimpleKeyRecorder(
                    sp.GetRequiredService<IKeyboardListener>(),
                    sp.GetRequiredService<IKeyStateTracker>(),
                    auxKeys);
            });

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainPage>();

            return services.BuildServiceProvider();
        }

        public void LoadSettings()
        {
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            var localSettings = ApplicationData.Current.LocalSettings;

            var pttKeysSetting = localSettings.GetJsonSerializedObject<VKey[]>("ptt_keys");
            var volumeReductionSetting = localSettings.GetJsonSerializedObject<int?>("vol_red");

            mainViewModel.SetPushToTalkKeys(pttKeysSetting == null ? new VKey[] { VKey.KEY_M } : pttKeysSetting);
            mainViewModel.VolumeReductionPercentage = volumeReductionSetting == null ? 30 : volumeReductionSetting.Value;
        }

        public void SaveSettings()
        {
            var mainViewModel = Services.GetRequiredService<MainViewModel>();
            var localSettings = ApplicationData.Current.LocalSettings;

            localSettings.SetJsonSerializedObject("ptt_keys", mainViewModel.PttKeybind?.KeyCodes);
            localSettings.SetJsonSerializedObject("vol_red", mainViewModel.VolumeReductionPercentage);
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var frame = new Frame();
            MainWindow = new Window();
            MainWindow.Content = frame;
            MainWindow.Closed += MainWindow_Closed;
            MainWindow.SetWindowSize(300, 175);
            MainWindow.CenterOnScreen();
            MainWindow.SetIsResizable(false);
            MainWindow.SetIsMaximizable(false);
            MainWindow.ExtendsContentIntoTitleBar = true;
            MainWindow.Activate();
            RootFrame.Navigate(typeof(MainPage));
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            SaveSettings();
        }
    }
}
