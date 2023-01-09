namespace EulerianFluidSim
{
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
        DrawLine drawLine;

        public byte[] bits { get; set; }

        public ColorSimRenderer(Simulation simulation, DrawLine liner)
        {
            sim = simulation;
            drawLine = liner;

            var len = sim.NumCellsX * sim.NumCellsY * 3;
            bits = new byte[len];
            for (int i = 0; i < len; i++)
                bits[i] = 255;
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

        private void GetMinMaxSpread(Simulation sim, float[] f, ref float min, ref float max, out float spread)
        {
            min = float.MaxValue;
            max = float.MinValue;

            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    var val = f[i+ (j* sim.NumCellsX)];
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
            int index = 0;
            int j0 = 0;
            int jmax = (sim.NumCellsY- 1)*sim.NumCellsX;
            GetMinMaxSpread(sim, sim.m, ref _minm, ref _maxm, out float spread);
            var invspread = (1.0f / spread)*255.0f;
            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    j0 = j * sim.NumCellsX;
                    var percent = (sim.m[i+j0] - _minm) * invspread;

                    index = (i + jmax - j0) * 3;
                    bits[index++] = (byte)(percent);
                    bits[index++] = (byte)(percent);
                    bits[index++] = (byte)(percent);
                }
        }

        public void RenderColoredSmoke()
        {
            GetMinMaxSpread(sim, sim.p, ref _minp, ref _maxp, out float spread);
            GetMinMaxSpread(sim, sim.m, ref _minm, ref _maxm, out float smokespread);

            int index = 0;
            int j0 = 0;
            int jmax = (sim.NumCellsY - 1) * sim.NumCellsX;
            var invspread = 1.0f / spread;
            var invsmokespread = (1.0f / smokespread) * 255.0f;
            float red; float green; float blue;
            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    j0 = j * sim.NumCellsX;

                    var percent = (sim.p[i+ j0] - _minp) * invspread;
                    ChoosePressureColor(percent, out red, out green, out blue);
                    percent = (sim.m[i+ j0] - _minm) * invsmokespread;

                    index = (i + jmax-j0) * 3;
                    bits[index++] = (byte)(percent * red);
                    bits[index++] = (byte)(percent * green);
                    bits[index++] = (byte)(percent * blue);
                }
        }

        private void RenderPressure()
        {
            GetMinMaxSpread(sim, sim.p, ref _minp, ref _maxp, out float spread);

            int index = 0;
            int j0 = 0;
            int jmax = (sim.NumCellsY - 1) * sim.NumCellsX;
            var invspread = (1.0f / spread);

            float red; float green; float blue;
            for (int j = 0; j < sim.NumCellsY; j++)
                for (int i = 0; i < sim.NumCellsX; i++)
                {
                    j0 = j * sim.NumCellsX;

                    var percent = (sim.p[i+ j0] - _minp) * invspread;
                    ChoosePressureColor(percent, out red, out green, out blue);

                    index = (i + jmax-j0) * 3;
                    bits[index++] = (byte)(red*255.0f);
                    bits[index++] = (byte)(green * 255.0f);
                    bits[index++] = (byte)(blue * 255.0f);
                }
        }

        private void ChoosePressureColor(float percent, out float red, out float green, out float blue)
        {
            var h = percent * 260.0f;
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

                    if (sim.s[(int)x+ ((int)y*sim.NumCellsX)] == 0)
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
