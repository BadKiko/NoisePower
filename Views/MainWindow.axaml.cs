

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
        private TextBlock _mainText, _optionsText;
        private SplitView _splitView;
        private Button _options;
        private List<string> _deviceIDs = new List<string>();

        private List<CheckBox> _checkBoxes = new List<CheckBox>();

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
            _splitView = this.FindControl<SplitView>("mainSplit");
            _options = this.FindControl<Button>("mainOptions");
            _optionsText = this.FindControl<TextBlock>("optionsText");
            _checkBoxes.Add(this.FindControl<CheckBox>("check0"));
            _checkBoxes.Add(this.FindControl<CheckBox>("check1"));
            _checkBoxes.Add(this.FindControl<CheckBox>("check2"));
            _checkBoxes.Add(this.FindControl<CheckBox>("check3"));


            CheckBox btn = new CheckBox()
            {
                Content="123"
            };
            DataContext = btn;

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

        public void OnOptionsClick(object sender, RoutedEventArgs e)
        {
            _options.IsVisible = false;
            _splitView.IsPaneOpen = true;
            _optionsText.IsVisible = true;
            foreach (CheckBox check in _checkBoxes)
            {
                check.IsVisible = true;
            }
        }

        public void OnOptionsHide(object sender, SplitViewPaneClosingEventArgs e)
        {
            _optionsText.IsVisible = false;
            _options.IsVisible = true;
            foreach (CheckBox check in _checkBoxes)
            {
                check.IsVisible = false;
            }
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
            _splitView.CompactPaneLength = 68;

        }
        public void EnableRecordToggle(object sender, RoutedEventArgs e)
        {
            StartRecord();
            _splitView.CompactPaneLength = 0;
        }
        public void DisableRecordToggle(object sender, RoutedEventArgs e)
        {
            StopRecord();
            _splitView.CompactPaneLength = 68;
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
            if (outDevice != null)
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
            #region Variables
            _listBox.IsVisible = false;
            _mainText.Text = $"Все работает. Звук передается в VAC.";
            capture = new WasapiCapture(enumerator.GetDevice(_deviceIDs[_listBox.SelectedIndex]));

            bool transformatorSound = _checkBoxes[3].IsChecked.Value;
            bool tearAudio = _checkBoxes[0].IsChecked.Value;
            bool popcornAudio = _checkBoxes[1].IsChecked.Value;
            bool nextAndBeforeEffectCheckbox = _checkBoxes[2].IsChecked.Value;
            bool nextAndBeforeEffect = true;

            capture.StartRecording();
            Random random = new Random();


            byte[] waveFile = File.ReadAllBytes("SOUNDS\\TRANS.wav");

            var transStream = bufferToWave(waveFile, 0, 19200, new WaveFormat(48000, 16, 1));
            #endregion



            #region Capture
            capture.DataAvailable += (s, a) => // Выполняется как дата от микрофона готова
            {
                MMDevice outDevice = VACDevice(); // Наш девайс

                if (tearAudio)
                    Thread.Sleep(random.Next(0, 100));

                if(popcornAudio)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        a.Buffer[i] = Convert.ToByte(random.Next(0, 254));
                    }
                }

                using (WaveStream ws = bufferToWave(a.Buffer, 0, a.BytesRecorded, capture.WaveFormat))
                {
                    using (WasapiOut output = new WasapiOut(outDevice, AudioClientShareMode.Shared, false, 0))
                    {
                        if(nextAndBeforeEffectCheckbox) // Если функция эффекта вперед назад нужна
                            nextAndBeforeEffect = !nextAndBeforeEffect;

                        if (nextAndBeforeEffect)
                        {
                            ws.Position = 0;
                            transStream.Position = 0;
                            var mixer = new MixingSampleProvider(new[] { ws.ToSampleProvider() });
                            if (transformatorSound)
                                mixer.AddMixerInput(transStream);
                            Thread playTH = new Thread(new ParameterizedThreadStart(Play));
                            playTH.Start(mixer);
                        }
                        else
                        {
                            using (WaveStream nws = bufferToWave(reverseSample(a.Buffer, a.BytesRecorded, capture.WaveFormat.BitsPerSample), 0, a.BytesRecorded, capture.WaveFormat))
                            {
                                nws.Position = 0;
                                transStream.Position = 0;
                                var mixer = new MixingSampleProvider(new[] { nws.ToSampleProvider() });
                                if (transformatorSound)
                                    mixer.AddMixerInput(transStream);
                                Thread playTH = new Thread(new ParameterizedThreadStart(Play));
                                playTH.Start(mixer);
                            }
                        }
                    }
                }

            };
            #endregion
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

        public byte[] reverseSample(byte[] sampleToReverse, int SourceLengthBytes, int bytesPerSample)
        {
            // Length of the buffer
            int numOfBytes;

            // The byte array to store the reversed sample
            byte[] reversedSample;

            numOfBytes = SourceLengthBytes;

            // Set the byte array to the length of the source sample
            reversedSample = new byte[SourceLengthBytes];

            // The alternatve location; starts at the end and works to the begining
            int b = 0;

            //Prime the loop by 'reducing' the numOfBytes by the first increment for the first sample
            numOfBytes = numOfBytes - bytesPerSample;

            // Used for the imbeded loop to move the complete sample
            int q = 0;

            // Moves through the stream based on each sample
            for (int i = 0; i < numOfBytes - bytesPerSample; i = i + bytesPerSample)
            {
                // Effectively a mirroing process; b will equal i (or be out by one if its an equal buffer)
                // when the middle of the buffer is reached.
                b = numOfBytes - bytesPerSample - i;

                // Copies the 'sample' in whole to the opposite end of the reversedSample
                for (q = 0; q <= bytesPerSample; q++)
                {
                    reversedSample[b + q] = sampleToReverse[i + q];
                }
            }

            // Sends back the reversed stream
            return reversedSample;
        }
        #endregion
    }
}
