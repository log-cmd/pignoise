using Gma.System.MouseKeyHook;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace pignoise
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        IKeyboardMouseEvents _hook;

        ReactiveProperty<bool> State { get; }
        public ReadOnlyReactiveProperty<string> StateStr { get; }
        public ReactiveCommand StateChangeCommand { get; }

        SoundPlayer _up;
        SoundPlayer _down;

        void Subscribe()
        {
            _hook = Hook.GlobalEvents();

            _hook.MouseWheelExt += (o, e) =>
            {
                if (e.Delta > 0)
                {
                    _up.Play();
                }
                else
                {
                    _down.Play();
                }
            };
        }

        void UnSubscribe()
        {
            _hook.Dispose();
            _hook = null;
        }

        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = this;

            {
                var up = "up.wav";
                var down = "down.wav";

                // generate wave
                if (!File.Exists(up))
                {
                    var wav = new SignalGenerator() { Gain = 0.2, Frequency = 440, Type = SignalGeneratorType.Sin }.Take(TimeSpan.FromMilliseconds(50)).ToWaveProvider();
                    WaveFileWriter.CreateWaveFile(up, wav);
                }
                _up = new SoundPlayer(up);

                if (!File.Exists(down))
                {
                    var wav = new SignalGenerator() { Gain = 0.2, Frequency = 880, Type = SignalGeneratorType.Sin }.Take(TimeSpan.FromMilliseconds(50)).ToWaveProvider();
                    WaveFileWriter.CreateWaveFile(down, wav);
                }
                _down = new SoundPlayer(down);
            }

            State = new ReactiveProperty<bool>(true);

            State.Subscribe(b =>
            {
                if (b && _hook == null)
                {
                    Subscribe();
                }
                if (!b && _hook != null)
                {
                    UnSubscribe();
                }
            });

            StateStr = State.Select(b => b ? "Stop" : "Start").ToReadOnlyReactiveProperty();
            StateChangeCommand = new ReactiveCommand();
            StateChangeCommand.Subscribe(() =>
            {
                State.Value = !State.Value;
            });
        }
    }
}
