

/*

d8b   db  .d88b.  d888888b .d8888. d88888b d8888b.  .d88b.  db   d8b   db d88888b d8888b. 
888o  88 .8P  Y8.   `88'   88'  YP 88'     88  `8D .8P  Y8. 88   I8I   88 88'     88  `8D 
88V8o 88 88    88    88    `8bo.   88ooooo 88oodD' 88    88 88   I8I   88 88ooooo 88oobY' 
88 V8o88 88    88    88      `Y8b. 88~~~~~ 88~~~   88    88 Y8   I8I   88 88~~~~~ 88`8b   
88  V888 `8b  d8'   .88.   db   8D 88.     88      `8b  d8' `8b d8'8b d8' 88.     88 `88. 
VP   V8P  `Y88P'  Y888888P `8888Y' Y88888P 88       `Y88P'   `8b8' `8d8'  Y88888P 88   YD 

Created by Kiko. MIT License Copyright 2022
*/

#region Libraries
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
#endregion

namespace NoisePower.Views
{
    public partial class MainWindow : Window
    {
        /*
        __      __        _       _     _           
        \ \    / /       (_)     | |   | |          
         \ \  / /_ _ _ __ _  __ _| |__ | | ___  ___ 
          \ \/ / _` | '__| |/ _` | '_ \| |/ _ \/ __|
           \  / (_| | |  | | (_| | |_) | |  __/\__ \
            \/ \__,_|_|  |_|\__,_|_.__/|_|\___||___/
        */
        #region Variables
        private ToggleSwitch _toggleSwitch;
        private ListBox _listBox;
        private TextBlock _mainText;
        private List<string> _deviceIDs = new List<string>();
        MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
        WasapiCapture capture = new WasapiCapture();
        WasapiOut output = new WasapiOut();
        MMDevice outDevice;
        #endregion

        /*   
         _______        _     
        |__   __|      | |    
           | | ___  ___| |__  
           | |/ _ \/ __| '_ \ 
           | |  __/ (__| | | |
           |_|\___|\___|_| |_|
        */
        #region Tech
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            _toggleSwitch = this.FindControl<ToggleSwitch>("power");
            _listBox = this.FindControl<ListBox>("list");
            _mainText = this.FindControl<TextBlock>("mainText");
            _toggleSwitch.IsVisible = false;

            UpdateDevices(); //Обновляем все доступные устройства для ввода
        }
        #endregion
        /*
         ____        _   _                  ______               _       
        |  _ \      | | | |                |  ____|             | |      
        | |_) |_   _| |_| |_ ___  _ __  ___| |____   _____ _ __ | |_ ___ 
        |  _ <| | | | __| __/ _ \| '_ \/ __|  __\ \ / / _ \ '_ \| __/ __|
        | |_) | |_| | |_| || (_) | | | \__ \ |___\ V /  __/ | | | |_\__ \
        |____/ \__,_|\__|\__\___/|_| |_|___/______\_/ \___|_| |_|\__|___/
        */
        #region ButtonEvents
        public void OnExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public void OnHideClick(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        public void OnUpdateClick(object sender, RoutedEventArgs e)
        {
            UpdateDevices();
        }

        public void OnInstallClick(object sender, RoutedEventArgs e)
        {
            if (System.Environment.Is64BitOperatingSystem)
                ExecuteAsAdmin("VAC_SETUP\\setup64.exe");
            else
                ExecuteAsAdmin("VAC_SETUP\\setup.exe");
        }
        #endregion

        /*
        __      __     _____ ______                    
        \ \    / /\   / ____|  ____|                   
         \ \  / /  \ | |    | |__ _   _ _ __   ___ ___ 
          \ \/ / /\ \| |    |  __| | | | '_ \ / __/ __|
           \  / ____ \ |____| |  | |_| | | | | (__\__ \
            \/_/    \_\_____|_|   \__,_|_| |_|\___|___/
        */
        #region VACFuncs
        public void ExecuteAsAdmin(string fileName)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = fileName;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.Verb = "runas";
            proc.Start();
        }

        public string InstalledVACText
        {
            get
            {
                if (IsVACInstalled)
                    return "Выберите устройство ввода";

                return "Установите Virtual Audio Cable для работы программы";
            }
        } // Проверка установлен ли VAC

        public bool IsVACInstalled
        {
            get
            {
                enumerator = new MMDeviceEnumerator();
                foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
                {
                    if (wasapi.FriendlyName.Contains("Virtual Audio Cable"))
                    {
                        return true;
                    }
                }
                return false;
            }
        } // Проверка установлен ли VAC

        #endregion

        /*
         _______                _           ______               _       
        |__   __|              | |         |  ____|             | |      
           | | ___   __ _  __ _| | ___  ___| |____   _____ _ __ | |_ ___ 
           | |/ _ \ / _` |/ _` | |/ _ \/ __|  __\ \ / / _ \ '_ \| __/ __|
           | | (_) | (_| | (_| | |  __/\__ \ |___\ V /  __/ | | | |_\__ \
           |_|\___/ \__, |\__, |_|\___||___/______\_/ \___|_| |_|\__|___/
                     __/ | __/ |                                         
                    |___/ |___/              
        */
        #region TogglesEvents
        public void ShowToggle(object sender, SelectionChangedEventArgs e)
        {
            _toggleSwitch.IsVisible = true;
        }
        public void EnableRecordToggle(object sender, RoutedEventArgs e)
        {
            StartRecord();
        }
        public void DisableRecordToggle(object sender, RoutedEventArgs e)
        {
            StopRecord();
        }
        #endregion

        /*
         _____             _          ______                    
        |  __ \           (_)        |  ____|                   
        | |  | | _____   ___  ___ ___| |__ _   _ _ __   ___ ___ 
        | |  | |/ _ \ \ / / |/ __/ _ \  __| | | | '_ \ / __/ __|
        | |__| |  __/\ V /| | (_|  __/ |  | |_| | | | | (__\__ \
        |_____/ \___| \_/ |_|\___\___|_|   \__,_|_| |_|\___|___/ 
        */
        #region DeviceFuncs
        /// <summary>
        /// Обновляет список устройств
        /// </summary>
        private void UpdateDevices()
        {
            ObservableCollection<ListBoxItem> items = new ObservableCollection<ListBoxItem>();
            items.Clear();
            _listBox.Items = items;

            enumerator = new MMDeviceEnumerator();
            foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active))
            {
                items.Add(new ListBoxItem() { Content = $"{wasapi.FriendlyName}" });
                _deviceIDs.Add(wasapi.ID);
            }

            _listBox.Items = items;
        }

        /// <summary>
        /// Основной VAC динамик
        /// </summary>
        private MMDevice VACDevice()
        {
            if(outDevice != null)
            {
                return outDevice;
            }

            foreach (var wasapi in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (wasapi.FriendlyName.Contains("Virtual"))
                {
                    outDevice = wasapi;
                    return wasapi;
                }
            }
            return null;
        }

        private void StartRecord()
        {
            _listBox.IsVisible = false;
            _mainText.Text = $"Все работает. Звук передается в VAC.";
            capture = new WasapiCapture(enumerator.GetDevice(_deviceIDs[_listBox.SelectedIndex]));
            capture.StartRecording();
            Random random = new Random();

            byte[] waveFile = File.ReadAllBytes("SOUNDS\\TRANS.wav");

            var transStream = bufferToWave(waveFile, 0, 19200, new WaveFormat(48000, 16, 1));

            capture.DataAvailable += (s, a) => // Выполняется как дата от микрофона готова
            {
                MMDevice outDevice = VACDevice(); // Наш девайс

                    using (WaveStream ws = bufferToWave(a.Buffer, 0, a.BytesRecorded, capture.WaveFormat))
                    {
                        using (WasapiOut output = new WasapiOut(outDevice, AudioClientShareMode.Shared, false, 0))
                        {
                            //waveMixerStream.AddInputStream(ws);
                            ws.Position = 0;
                            transStream.Position = 0;
                            
                            var mixer = new MixingSampleProvider(new[] { ws.ToSampleProvider(), transStream.ToSampleProvider()});
                            Thread playTH = new Thread(new ParameterizedThreadStart(Play));
                            playTH.Start(mixer);
                        }
                    }
                
            };

        }

        public void Play(object provider) // Поток играет
        {
            using (output = new WasapiOut(outDevice, AudioClientShareMode.Shared, false, 211))
            {
                output.Volume = 1;
                output.Init(provider as MixingSampleProvider);
                output.Play(); // Играем
            }
        }

        
        private void StopRecord()
        {
            _listBox.IsVisible = true;
            _mainText.Text = "Выберите устройство ввода";
            capture.StopRecording();
            output.Stop();
        }
        #endregion

        /*
        __          __             ______                    
        \ \        / /            |  ____|                   
         \ \  /\  / /_ ___   _____| |__ _   _ _ __   ___ ___ 
          \ \/  \/ / _` \ \ / / _ \  __| | | | '_ \ / __/ __|
           \  /\  / (_| |\ V /  __/ |  | |_| | | | | (__\__ \
            \/  \/ \__,_| \_/ \___|_|   \__,_|_| |_|\___|___/
        */
        #region WaveFuncs
        WaveStream bufferToWave(byte[] bytes, int offset, int bytesCount, WaveFormat format)
        {
            MemoryStream ms = new MemoryStream();
            ms.Write(bytes, 0, bytesCount); // Запись полученной даты в стрим
            ms.Position = 0;
            WaveStream ws = new RawSourceWaveStream(ms, format);
            ws.Position = 0;
            return ws;
        }
        #endregion
    }
}
