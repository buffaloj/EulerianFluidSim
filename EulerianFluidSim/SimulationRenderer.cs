using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EulerianFluidSim
{
    public class SimulationRenderer
    {
        private readonly Simulation _sim;

        public bool ShowPressure { get; set; } = false;
        public bool ShowColoredSmoke { get; set; } = true;
        public bool ShowFlowLines { get; set; } = false;

        private float _minp = float.MaxValue;
        private float _maxp = float.MinValue;

        private float _minm = float.MaxValue;
        private float _maxm = float.MinValue;

        public SimulationRenderer(Simulation simulation)
        {
            _sim = simulation;
        }

        public void Render(Bitmap bitmap)
        {
            if (ShowPressure)
                RenderPressure(bitmap);
            else if (ShowColoredSmoke)
                RenderColoredSmoke(bitmap);
            else
                RenderSmoke(bitmap);

            if (ShowFlowLines)
                RenderFlowLines(bitmap);
        }

        public void RenderExamplePixels(Bitmap bitmap)
        {
            for (int x = 0; x < 100; x++)
                bitmap.SetPixel(10 + x, 10, Color.Red);
        }

        private void GetMinMaxSpread(float[,] f, ref float min, ref float max, out float spread)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int j = 0; j < _sim.NumCellsY; j++)
                for (int i = 0; i < _sim.NumCellsX; i++)
                {
                    var val = f[i, j];
                    if (min > val)
                        min = val;
                    if (max < val)
                        max = val;
                }

            spread = max - min;
            if (spread < 0.1f)
                spread = 0.1f;
        }

        public void RenderSmoke(Bitmap bitmap)
        {
            GetMinMaxSpread(_sim.m, ref _minm, ref _maxm, out float spread);
            for (int j = 0; j < _sim.NumCellsY; j++)
                for (int i = 0; i < _sim.NumCellsX; i++)
                {
                    var percent = (_sim.m[i, j] - _minm) / spread;
                    var gray = Math.Max(Math.Min((int)(percent * 255.0f), 255), 0);

                    bitmap.SetPixel(i, _sim.NumCellsY - 1 - j, Color.FromArgb(gray, gray, gray));
                }
        }

        public void RenderColoredSmoke(Bitmap bitmap)
        {
            GetMinMaxSpread(_sim.p, ref _minp, ref _maxp, out float spread);
            GetMinMaxSpread(_sim.m, ref _minm, ref _maxm, out float smokespread);

            for (int j = 0; j < _sim.NumCellsY; j++)
                for (int i = 0; i < _sim.NumCellsX; i++)
                {
                    var percent = (_sim.p[i, j] - _minp) / spread;
                    var color = LerpColor(1.0f - percent);

                    percent = (_sim.m[i, j] - _minm) / smokespread;

                    bitmap.SetPixel(i, _sim.NumCellsY - 1 - j, InterpolateColor(Color.Black, color, 1.0f - ((1.0f - percent) * (1.0f - percent))));
                }
        }

        public void RenderPressure(Bitmap bitmap)
        {
            GetMinMaxSpread(_sim.p, ref _minp, ref _maxp, out float spread);

            for (int j = 0; j < _sim.NumCellsY; j++)
                for (int i = 0; i < _sim.NumCellsX; i++)
                {
                    var percent = (_sim.p[i, j] - _minp) / spread;

                    var color = LerpColor(1.0f - percent);

                    bitmap.SetPixel(i, _sim.NumCellsY - 1 - j, color);
                }
        }

        private Color LerpColor(float percent)
        {
            var h = percent * 260.0f;
            var s = 1.0f;
            var l = 0.5f;

            var d = s * (1 - Math.Abs(2 * l - 1));
            var x = d * (1 - Math.Abs(h / 60 % 2 - 1));
            var m = l - (d / 2.0f);

            if (h < 60.0f)
                return Color.FromArgb(255, (int)(255.0f * (d + m)), (int)(255.0f * (x + m)), (int)(m * 255.0f));
            else if (h < 120.0f)
                return Color.FromArgb(255, (int)(255.0f * (x + m)), (int)(255.0f * (d + m)), (int)(m * 255.0f));
            else if (h < 180.0f)
                return Color.FromArgb(255, (int)(m * 255.0f), (int)(255.0f * (d + m)), (int)(255.0f * (x + m)));
            else if (h < 240.0f)
                return Color.FromArgb(255, (int)(m * 255.0f), (int)(255.0f * (x + m)), (int)(255.0f * (d + m)));
            else if (h < 300.0f)
                return Color.FromArgb(255, (int)(255.0f * (x + m)), (int)(m * 255.0f), (int)(255.0f * (d + m)));
            else// if (h < 360.0f)
                return Color.FromArgb(255, (int)(255.0f * (d + m)), (int)(m * 255.0f), (int)(255.0f * (x + m)));

        }

        private Color InterpolateColor(Color from, Color to, float percent)
        {
            var r = from.R + ((to.R - from.R) * percent);
            var g = from.G + ((to.G - from.G) * percent);
            var b = from.B + ((to.B - from.B) * percent);

            return Color.FromArgb(255, (int)r, (int)g, (int)b);
        }

        public void RenderFlowLines(Bitmap bitmap)
        {
            Pen blackPen = new Pen(Color.Red, 1);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                for (int i = 0; i <= 8; i++)
                {
                    //if (i == 5 || i == 6 || i == 7)
                    //  continue;

                    for (int j = 0; j < 5; j++)
                    {
                        var x = _sim.NumCellsX * (0.1f * i);
                        var y = _sim.NumCellsY * (0.2f * j);

                        if (x < 1)
                            x = 1;

                        if (_sim.s[(int)x, (int)y] == 0)
                            continue;
                        var diru = _sim.sampleField(x * _sim.GridSpacing, y * _sim.GridSpacing, Fields.U_Field);
                        var dirv = _sim.sampleField(x * _sim.GridSpacing, y * _sim.GridSpacing, Fields.V_Field);
                        var len = Math.Sqrt(diru * diru + dirv * dirv);
                        var x2 = x + (int)(((double)diru / len) * 10.0);
                        var y2 = y + (int)(((double)dirv / len) * 10.0);
                        graphics.DrawLine(blackPen, x, y, x2, y2);
                    }
                }
            }
        }
    }
}
