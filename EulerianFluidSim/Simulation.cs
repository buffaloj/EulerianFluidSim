namespace EulerianFluidSim
{
    public enum Fields { U_Field, V_Field, S_Field }
    public class Simulation
    {
        private readonly int _numCellsX;
        private readonly int _numCellsY;
        private float[,] u;
        private float[,] v;
        private float[,] newu;
        private float[,] newv;
        private readonly float[,] p;
        private readonly float[,] s;
        private float[,] m;
        private float[,] newm;
        private float _density;
        private float _gridSpacing;
        private float _overRelaxation;
        private int _numIterations;
        private float _gravity = -9.8f;

        private float _minp = float.MaxValue;
        private float _maxp = float.MinValue;

        private float _minm = float.MaxValue;
        private float _maxm = float.MinValue;

        public float FlowRate { get; set; } = 1000.0f;
        public bool ShowPressure { get; set; } = false;
        public bool ShowColoredSmoke { get; set; } = true;
        public bool ShowFlowLines { get; set; } = false;

        public Simulation(int numCellsX, int numCellsY, float density, float gridSpacing, float overRelaxation)
        {
            _density = 1.0f;// density;
            _gridSpacing = gridSpacing;
            _overRelaxation = 1.9f;//overRelaxation;
            _numIterations = 10;// 000;

            _numCellsX = numCellsX;
            _numCellsY = numCellsY;

            u = new float[_numCellsX, _numCellsY];
            v = new float[_numCellsX, _numCellsY];
            newu = new float[_numCellsX, _numCellsY];
            newv = new float[_numCellsX, _numCellsY];
            p = new float[_numCellsX, _numCellsY];
            s = new float[_numCellsX, _numCellsY];

            m = new float[_numCellsX, _numCellsY];
            newm = new float[_numCellsX, _numCellsY];

            InitAsContainedBox();

            var size = numCellsX;
            if (size > numCellsY)
                size = numCellsY;
            BlockCircle(numCellsX / 4, numCellsY / 2, (float)size * 0.2f);

            SetLeftSmoke(0.1f);
        }

        public void BlockCircle(int x, int y, float radius)
        {
            for (int j = y - (int)radius; j < y + radius; j++) 
            { 
                for (int i = x - (int)radius; i < x + radius; i++)
                {
                    var dst = Math.Sqrt((i-x)*(i-x) + (j-y)*(j-y));
                    if (dst < radius)
                        s[i,j] = 0;
                }
            }
        }

        private void InitAsContainedBox()
        {
            for (int y = 0; y < _numCellsY; y++)
                for (int x = 0; x < _numCellsX; x++)
                    s[x, y] = isEdge(x, y) ? 0.0f : 1.0f;
        }

        private bool isEdge(int x, int y)
        {
            return x == 0 || x == (_numCellsX - 1) || y == 0 || y == (_numCellsY - 1);
        }

        public void SetLeftToRightFlow(float flowRate)
        {
            for (int y = 1; y < _numCellsY-1; y++)
            {
                u[0, y] = flowRate;
                u[1, y] = flowRate;
                u[_numCellsX - 1, y] = flowRate;
            }
        }

        private void SetLeftSmoke(float amount)
        {
            var begin = (int)(_numCellsY * 0.4f);
            var end = (int)(_numCellsY * 0.6f);

            for (int y = begin; y < end; y++)
            {
                m[0, y] = amount;
                newm[0, y] = amount;
            }
        }

        public void StepSimulation(float timeStep)
        {
            if (timeStep > 0.033f)
                timeStep = 0.033f;

            UpdateVelocities(timeStep);
            IterativelySolveCompressibility(timeStep);
            //extrapolate();    // video author did this...not fully getting this
            AdvectVelocities(timeStep);
            AdvectSmoke(timeStep);
        }

        private void UpdateVelocities(float timeStep)
        {
            for (int j = 1; j < _numCellsY; j++)
            {
                for (int i = 1; i < _numCellsX; i++)
                {
                    if (s[i, j] != 0.0f && s[i, j - 1] != 0.0f)
                        v[i, j] += _gravity * timeStep;
                }
            }

            SetLeftToRightFlow(FlowRate * timeStep);
        }

        public void IterativelySolveCompressibility(float timeStep)
        {
            for (int j = 0; j < _numCellsY; j++)
                for (int i = 0; i < _numCellsX; i++)
                    p[i, j] = 0.0f;

            for (int i = 0; i < _numIterations; i++)
                SolveCompressibility(timeStep);
        }

        private void SolveCompressibility(float timeStep)
        {
            for (int j = 1; j < _numCellsY - 1; j++)
            {
                for (int i = 1; i < _numCellsX - 1; i++)
                {
                    var d = u[i + 1, j] - u[i, j] + v[i, j + 1] - v[i, j];
                    var st = s[i + 1, j] + s[i - 1, j] + s[i, j + 1] + s[i, j - 1];
                    if (st == 0.0f)
                        continue;

                    d *= _overRelaxation;

                    u[i, j] += d * s[i - 1, j] / st;
                    u[i+1, j] -= d * s[i + 1, j] / st;
                    v[i, j] += d * s[i, j - 1] / st;
                    v[i, j + 1] -= d * s[i, j + 1] / st;

                    p[i, j] -= (d / st) * (_density * _gridSpacing / timeStep);
                }
            }
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

            for (int j = 0; j < _numCellsY; j++)
                for (int i = 0; i < _numCellsX; i++)
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
            GetMinMaxSpread(m, ref _minm, ref _maxm, out float spread);
            for (int j = 0; j < _numCellsY; j++)
                for (int i = 0; i < _numCellsX; i++)
                {
                    var percent = (m[i, j] - _minm) / spread;
                    var gray = Math.Max(Math.Min((int)(percent * 255.0f), 255), 0);

                    bitmap.SetPixel(i, _numCellsY - 1 - j, Color.FromArgb(gray, gray, gray));
                }
        }

        public void RenderColoredSmoke(Bitmap bitmap)
        {
            GetMinMaxSpread(p, ref _minp, ref _maxp, out float spread);
            GetMinMaxSpread(m, ref _minm, ref _maxm, out float smokespread);

            for (int j = 0; j < _numCellsY; j++)
                for (int i = 0; i < _numCellsX; i++)
                {
                    var percent = (p[i, j] - _minp) / spread;
                    var color = LerpColor(1.0f - percent);

                    percent = (m[i, j] - _minm) / smokespread;

                    bitmap.SetPixel(i, _numCellsY - 1 - j, InterpolateColor(Color.Black, color, 1.0f - ((1.0f-percent)* (1.0f - percent))));
                }
        }

        public void RenderPressure(Bitmap bitmap)
        {
            GetMinMaxSpread(p, ref _minp, ref _maxp, out float spread);

            for (int j = 0; j < _numCellsY; j++)
                for (int i = 0; i < _numCellsX; i++)
                {
                    var percent = (p[i, j] - _minp) / spread;

                    var color = LerpColor(1.0f - percent);

                    bitmap.SetPixel(i, _numCellsY - 1 - j, color);
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
                        var x = _numCellsX * (0.1f * i);
                        var y = _numCellsY * (0.2f * j);

                        if (x < 1)
                            x = 1;

                        if (s[(int)x, (int)y] == 0)
                            continue;
                        var diru = sampleField(x * _gridSpacing, y * _gridSpacing, Fields.U_Field);
                        var dirv = sampleField(x * _gridSpacing, y * _gridSpacing, Fields.V_Field);
                        var len = Math.Sqrt(diru * diru + dirv * dirv);
                        var x2 = x + (int)(((double)diru / len) * 10.0);
                        var y2 = y + (int)(((double)dirv / len) * 10.0);
                        graphics.DrawLine(blackPen, x, y, x2, y2);
                    }
                }
            }
        }

        private void AdvectVelocities(float timeStep)
        {
            CopyArrays(u, newu);
            CopyArrays(v, newv);

           var h2 = _gridSpacing / 2.0f;
           for (int j = 1; j < _numCellsY; j++)
           {
                for (int i = 1; i < _numCellsX; i++)
                {
                    if (s[i, j] == 0)
                        continue;

                    if (s[i-1, j] != 0 && j < _numCellsY-1)
                    {
                        var vprime = (v[i, j] + v[i, j + 1] + v[i + 1, j] + v[i + 1, j + 1]) / 4.0f;
                        var xpos = (i * _gridSpacing) - (timeStep * u[i, j]);
                        var ypos = (j * _gridSpacing) + h2 - (timeStep * vprime);
                        newu[i, j] = sampleField(xpos, ypos, Fields.U_Field);//InterpolateU(xpos, ypos);
                    }

                    if (s[i, j-1] != 0 && i < _numCellsX-1)
                    {
                        var uprime = (u[i, j] + u[i, j + 1] + u[i + 1, j] + u[i + 1, j + 1]) / 4.0f;
                        var xpos = (i * _gridSpacing) + h2 - (timeStep * uprime);
                        var ypos = (j * _gridSpacing) - (timeStep * v[i, j]);
                        newv[i, j] = sampleField(xpos, ypos, Fields.V_Field);//InterpolateV(xpos, ypos);
                    }
                }
            }

            CopyArrays(newu, u);
            CopyArrays(newv, v);
            //SwapArrays(ref u, ref newu);
            //SwapArrays(ref v, ref newv);
        }

        private void AdvectSmoke(float timeStep)
        {
            CopyArrays(m, newm);

            var h2 = _gridSpacing / 2.0f;

            for (int j = 0; j < _numCellsY-1; j++)
            {
                for (int i = 0; i < _numCellsX-1; i++)
                {
                    if (s[i, j] == 0)
                        continue;

                    var cu = (u[i, j] + u[i + 1, j]) * 0.5f;
                    var cv = (v[i, j] + v[i, j + 1]) * 0.5f;
                    var x = i * _gridSpacing + h2 - timeStep * cu;
                    var y = j * _gridSpacing + h2 - timeStep * cv;
                    newm[i, j] = sampleField(x, y, Fields.S_Field);// InterpolateM(x, y);
                }
            }

            CopyArrays(newm, m);

            //SwapArrays(ref m, ref newm);
        }

        private void CopyArrays(float[,] from, float[,] to)
        {
            

            for (int j = 0; j < _numCellsY - 1; j++)
                for (int i = 0; i < _numCellsX - 1; i++)
                    to[i,j] = from[i, j];
        }

        private void SwapArrays(ref float[,] a, ref float[,] b)
        {
            var tmp = a;
            a = b;
            b = tmp;
        }

        /*
        private float InterpolateU(float posx, float posy)
        {
            float h2 = _gridSpacing / 2.0f;

            int i = (int)posx;

            float xoffset = posx - (float)Math.Floor(posx);

            int j = (int)posy;
            float yoffset = posy - (float)Math.Floor(posy);
            if (yoffset > h2)
                j++;

            if (i > _numCellsX - 2)
                i = _numCellsX - 2;

            if (i < 0)
                i = 0;

            if (j > _numCellsY - 2)
                j = _numCellsY - 2;

            if (j < 0)
                j = 0;

            var w00 = 1.0f - (xoffset / _gridSpacing);
            var w01 = xoffset / 2.0f;
            var w10 = 1.0f - (yoffset / _gridSpacing);
            var w11 = yoffset / 2.0f;

            return w00*w10*u[i,j] + w01*w10*u[i+1,j] + w00*w11*u[i,j+1] + w01*w11*u[i+1,j+1];
        }

        private float InterpolateV(float posx, float posy)
        {
            float h2 = _gridSpacing / 2.0f;

            int i = (int)posx;

            float xoffset = posx - (float)Math.Floor(posx);
            if (xoffset > h2)
                i++;

            int j = (int)posy;
            float yoffset = posy - (float)Math.Floor(posy);

            if (i > _numCellsX - 2)
                i = _numCellsX - 2;

            if (i < 0)
                i = 0;

            if (j > _numCellsY - 2)
                j = _numCellsY - 2;

            if (j < 0)
                j = 0;

            var w00 = 1.0f - (xoffset / _gridSpacing);
            var w01 = xoffset / 2.0f;
            var w10 = 1.0f - (yoffset / _gridSpacing);
            var w11 = yoffset / 2.0f;

            return w00 * w10 * v[i, j] + w01 * w10 * v[i + 1, j] + w00 * w11 * v[i, j + 1] + w01 * w11 * v[i + 1, j + 1];
        }

        private float InterpolateM(float posx, float posy)
        {
            float h2 = _gridSpacing / 2.0f;

            int i = (int)posx;
            float xoffset = posx - (float)Math.Floor(posx);
            if (xoffset > h2)
                i++;

            int j = (int)posy;
            float yoffset = posy - (float)Math.Floor(posy);
            if (yoffset > h2)
                j++;

            if (i > _numCellsX - 2)
                i = _numCellsX - 2;

            if (i < 0)
                i = 0;

            if (j > _numCellsY - 2)
                j = _numCellsY - 2;

            if (j < 0)
                j = 0;

            var w00 = 1.0f - (xoffset / _gridSpacing);
            var w01 = xoffset / 2.0f;
            var w10 = 1.0f - (yoffset / _gridSpacing);
            var w11 = yoffset / 2.0f;

            return w00 * w10 * m[i, j] + w01 * w10 * m[i + 1, j] + w00 * w11 * m[i, j + 1] + w01 * w11 * m[i + 1, j + 1];
        }
        */
        private void extrapolate()
        {
            for (int i = 0; i < _numCellsX; i++)
            {
                u[i, 0] = u[i, 1];
                u[i, _numCellsY - 1] = u[i, _numCellsY - 2];
            }

            for (int j = 0; j < _numCellsY; j++)
            {
                v[0, j] = v[1, j];
                v[_numCellsX-1, j] = v[_numCellsX - 2, j];
            }
        }

        private float sampleField(float x, float y, Fields field)
        {
            var n = _numCellsX;
            var h = _gridSpacing;
            var h1 = 1.0f / h;
            var h2 = h / 2.0f;

            x = Math.Max(Math.Min(x, _numCellsX), h);
            y = Math.Max(Math.Min(y, _numCellsY), h);

            var dx = 0.0f;
            var dy = 0.0f;

            float[,] f;

            switch (field)
            {
                case Fields.U_Field: { f = u; dy = h2;} break;
                case Fields.V_Field: { f = v; dx = h2;} break;
                case Fields.S_Field: { f = m; dx = h2; dy = h2;} break;
                default:
                    throw new Exception($"Unknown {nameof(Fields)} value");
            }

            var x0 = Math.Min(Math.Floor((x-dx)*h1), _numCellsX-1);
            var tx = ((x - dx) - x0 * h) * h1;
            var x1 = Math.Min(x0+1, _numCellsX-1);

            var y0 = Math.Min(Math.Floor((y - dy) * h1), _numCellsY - 1);
            var ty = ((y - dy) - y0 * h) * h1;
            var y1 = Math.Min(y0 + 1, _numCellsY - 1);

            var sx = 1.0f - tx;
            var sy = 1.0f - ty;

            var val = sx * sy * f[(int)x0, (int)y0] +
                      tx * sy * f[(int)x1, (int)y0] +
                      tx * ty * f[(int)x1, (int)y1] +
                      sx * ty * f[(int)x0, (int)y1];

            return (float)val;
        }
    }
}
