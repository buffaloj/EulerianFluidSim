namespace EulerianFluidSim
{
    public delegate void SetPixelColor(int x, int y, float red, float green, float blue);
    public delegate void DrawLine(float x1, float y1, float x2, float y2);

    public class ColorSimRenderer
    {
        public bool ShowPressure { get; set; } = false;
        public bool ShowColoredSmoke { get; set; } = true;
        public bool ShowFlowLines { get; set; } = false;

        private float _minp = float.MaxValue;
        private float _maxp = float.MinValue;

        private float _minm = float.MaxValue;
        private float _maxm = float.MinValue;

        private readonly Simulation sim;
        private readonly SetPixelColor setPixelColor;
        DrawLine drawLine;

        public ColorSimRenderer(Simulation simulation, SetPixelColor setter, DrawLine liner)
        {
            sim = simulation;
            setPixelColor = setter;
            drawLine = liner;
        }

        public void Render()
        {
            if (ShowPressure)
                RenderPressure();
            else if (ShowColoredSmoke)
                RenderColoredSmoke();
            else
                RenderSmoke();

            if (ShowFlowLines)
                RenderFlowLines();
        }

        private void GetMinMaxSpread(Simulation sim, float[,] f, ref float min, ref float max, out float spread)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
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

        private void RenderSmoke()
        {
            GetMinMaxSpread(sim, sim.m, ref _minm, ref _maxm, out float spread);
            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    var percent = (sim.m[i, j] - _minm) / spread;
                    setPixelColor(i, sim.NumCellsY - 1 - j, percent, percent, percent);
                }
        }

        public void RenderColoredSmoke()
        {
            GetMinMaxSpread(sim, sim.p, ref _minp, ref _maxp, out float spread);
            GetMinMaxSpread(sim, sim.m, ref _minm, ref _maxm, out float smokespread);

            float red; float green; float blue;
            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    var percent = (sim.p[i, j] - _minp) / spread;
                    ChoosePressureColor(percent, out red, out green, out blue);

                    percent = (sim.m[i, j] - _minm) / smokespread;

                    setPixelColor(i, sim.NumCellsY - 1 - j, red * percent, green * percent, blue * percent);
                }
        }

        private void RenderPressure()
        {
            GetMinMaxSpread(sim, sim.p, ref _minp, ref _maxp, out float spread);

            float red; float green; float blue;
            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    var percent = (sim.p[i, j] - _minp) / spread;
                    ChoosePressureColor(percent, out red, out green, out blue);

                    setPixelColor(i, sim.NumCellsY - 1 - j, red, green, blue);
                }
        }

        private void ChoosePressureColor(float percent, out float red, out float green, out float blue)
        {
            var h = (1.0f - percent) * 260.0f;
            var s = 1.0f;
            var l = 0.5f;

            var d = s * (1 - Math.Abs(2 * l - 1));
            var x = d * (1 - Math.Abs(h / 60 % 2 - 1));
            var m = l - (d / 2.0f);

            if (h < 60.0f)
            {
                red = d + m;
                green = x + m;
                blue = m;
            }
            else if (h < 120.0f)
            {
                red = x + m;
                green = d + m;
                blue = m;
            }
            else if (h < 180.0f)
            {
                red = m;
                green = d + m;
                blue = x+m;
            }
            else if (h < 240.0f)
            {
                red = m;
                green = x + m;
                blue = d + m;
            }
            else if (h < 300.0f)
            {
                red = x+m;
                green = m;
                blue = d + m;
            }
            else
            {
                red = d + m;
                green = m;
                blue = x + m;
            }
        }

        private void RenderFlowLines()
        {
            for (int i = 0; i <= 8; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    var x = sim.NumCellsX * (0.1f * i);
                    var y = sim.NumCellsY * (0.2f * j);

                    if (x < 1)
                        x = 1;

                    if (sim.s[(int)x, (int)y] == 0)
                        continue;
                    var diru = sim.sampleField(x * sim.GridSpacing, y * sim.GridSpacing, Fields.U_Field);
                    var dirv = sim.sampleField(x * sim.GridSpacing, y * sim.GridSpacing, Fields.V_Field);
                    var len = Math.Sqrt(diru * diru + dirv * dirv);
                    var x2 = x + (int)(((double)diru / len) * 10.0);
                    var y2 = y + (int)(((double)dirv / len) * 10.0);
                    drawLine(x, y, x2, y2);
                }
            }
        }
    }
}
