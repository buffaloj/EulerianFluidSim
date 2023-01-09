using EulerianFluidSim;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;

namespace SimRunnerApp.Maui
{
    public partial class MainPage : ContentPage
    {
        private SKBitmap _bitmap;
        private Simulation _simulation;
        private ColorSimRenderer _renderer;
        private SKPaint linePen = new SKPaint { Color = new SKColor(0xffff0000) };
        private SKCanvas _canvas;

        private bool _showFlowLines;

        public MainPage()
        {
            InitializeComponent();
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            CreateSim();

            Stopwatch sw = Stopwatch.StartNew();
            long start = 0;
            long end = sw.ElapsedMilliseconds;
            sw.Start();

            IDispatcherTimer timer = Dispatcher.CreateTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += (sender, e) =>
            {
                timer.Stop();

                if (!this.IsLoaded)
                    return;

                var elapsed = sw.ElapsedMilliseconds - start;
                float deltaTime = (float)elapsed / 1000.0f;

                if (deltaTime > 0.005f)
                {
                    start = end;
                    end = sw.ElapsedMilliseconds;

                    var fps = 1.0f / deltaTime;
                    fpsLabel.Text = $"{fps}fps";

                    _simulation?.StepSimulation(deltaTime);
                    canvasView.InvalidateSurface();
                };
                timer.Start();
            };
            timer.Start();
        }

        private void CreateSim()
        {
            var width = (int)canvasView.Width;
            var height = (int)canvasView.Height;
            //width /= 15;
            //height /= 15;
            width = 160;// 15;
            height = 90;// 15;
            _bitmap = new SKBitmap(width, height);
            _canvas = new SKCanvas(_bitmap);
            _simulation = new Simulation(width, height, 1.0f, 1.0f, 1.9f);
            _renderer = new ColorSimRenderer(_simulation);

            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    _bitmap.SetPixel(x, y, new SKColor(0xff00ff00));

            speedSlider.Value = (int)_simulation.FlowRate;
            pressureCheck.IsChecked = _renderer.ShowPressure;
            colorSmokeCheck.IsChecked = _renderer.ShowColoredSmoke;
            showFlowLinesCheck.IsChecked = _showFlowLines;
        }

        private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface vSurface = e.Surface;
            SKCanvas canvas = vSurface.Canvas;

            if (_simulation == null)
            {
                canvas.Clear();
                return;
            }

            _renderer.Render();

            var mainDisplayInfo = DeviceDisplay.MainDisplayInfo;
            var width = canvasView.Width * mainDisplayInfo.Density;
            var height = canvasView.Height * mainDisplayInfo.Density;

            // render the bitmap to the canvas
            //using SKCanvas sKCanvas = new SKCanvas(_bitmap);
            byte red = 0, green = 0, blue = 0;
            int index = 0;
            for (int j = 0; j < _simulation.NumCellsY; j++) 
            { 
                for (int i = 0; i < _simulation.NumCellsX; i++) 
                {
                    blue = _renderer.bits[index++];
                    green = _renderer.bits[index++];
                    red = _renderer.bits[index++];
                    _canvas.DrawPoint(i, j, new SKColor(red, green, blue));
                }
            }

            if (_showFlowLines)
                _renderer.RenderFlowLines((x1,y1,x2,y2) => _canvas.DrawLine(new SKPoint(x1, y1), new SKPoint(x2, y2), linePen));

            var paint2 = new SKPaint { FilterQuality = SKFilterQuality.High };
            canvas.DrawBitmap(_bitmap, new SKRect(0.0f, 0.0f, (float)width, (float)height), paint2);
        }

        private void pressureCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _renderer.ShowPressure = pressureCheck.IsChecked;
            canvasView.InvalidateSurface();
        }

        private void colorSmokeCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _renderer.ShowColoredSmoke = colorSmokeCheck.IsChecked;
            canvasView.InvalidateSurface();
        }

        private void showFlowLinesCheck_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            _showFlowLines = showFlowLinesCheck.IsChecked;
            canvasView.InvalidateSurface();
        }

        private void speedSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            _simulation.FlowRate = (float)speedSlider.Value;
            canvasView.InvalidateSurface();
        }

        private void ContentPage_Unfocused(object sender, FocusEventArgs e)
        {

        }
    }
}