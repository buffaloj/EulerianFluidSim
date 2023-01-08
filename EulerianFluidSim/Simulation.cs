namespace EulerianFluidSim
{
    public enum Fields { U_Field, V_Field, S_Field }
    public class Simulation
    {
        public int NumCellsX { get; private set; }
        public int NumCellsY { get; private set; }
        public float GridSpacing { get; private set; }

        public readonly float[] s;
        public readonly float[] p;
        public float[] m;

        private float[] u;
        private float[] v;
        
        private float[] newu;
        private float[] newv;

        private float[] newm;

        private float _density;
        
        private float _overRelaxation;
        private int _numIterations;
        private float _gravity = -9.8f;

        public float FlowRate { get; set; } = 1000.0f;

        public Simulation(int numCellsX, int numCellsY, float density, float gridSpacing, float overRelaxation)
        {
            _density = 1.0f;// density;
            GridSpacing = gridSpacing;
            _overRelaxation = 1.9f;//overRelaxation;
            _numIterations = 10;// 000;

            NumCellsX = numCellsX;
            NumCellsY = numCellsY;

            u = new float[NumCellsX*NumCellsY];
            v = new float[NumCellsX*NumCellsY];
            newu = new float[NumCellsX*NumCellsY];
            newv = new float[NumCellsX*NumCellsY];
            p = new float[NumCellsX*NumCellsY];
            s = new float[NumCellsX*NumCellsY];

            m = new float[NumCellsX*NumCellsY];
            newm = new float[NumCellsX*NumCellsY];

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
                        s[i + (j*NumCellsX)] = 0;
                }
            }
        }

        private void InitAsContainedBox()
        {
            for (int y = 0; y < NumCellsY; y++)
                for (int x = 0; x < NumCellsX; x++)
                    s[x + (y * NumCellsX)] = isEdge(x, y) ? 0.0f : 1.0f;
        }

        private bool isEdge(int x, int y)
        {
            return x == 0 || x == (NumCellsX - 1) || y == 0 || y == (NumCellsY - 1);
        }

        public void SetLeftToRightFlow(float flowRate)
        {
            for (int y = 1; y < NumCellsY-1; y++)
            {
                u[y * NumCellsX] = flowRate;
                u[1+ (y * NumCellsX)] = flowRate;
                u[NumCellsX - 1+ (y * NumCellsX)] = flowRate;
            }
        }

        private void SetLeftSmoke(float amount)
        {
            var begin = (int)(NumCellsY * 0.4f);
            var end = (int)(NumCellsY * 0.6f);

            for (int y = begin; y < end; y++)
            {
                m[y * NumCellsX] = amount;
                newm[y * NumCellsX] = amount;
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
            int index = 0;
            for (int j = 1; j < NumCellsY; j++)
            {
                for (int i = 1; i < NumCellsX; i++)
                {
                    index = i + (j * NumCellsX);
                    if (s[index] != 0.0f && s[i + ((j-1) * NumCellsX)] != 0.0f)
                        v[index] += _gravity * timeStep;
                }
            }

            SetLeftToRightFlow(FlowRate * timeStep);
        }

        public void IterativelySolveCompressibility(float timeStep)
        {
            int index = 0;
            for (int j = 0; j < NumCellsY; j++)
                for (int i = 0; i < NumCellsX; i++)
                    p[index++] = 0.0f;

            for (int i = 0; i < _numIterations; i++)
                SolveCompressibility(timeStep);
        }

        private void SolveCompressibility(float timeStep)
        {
            int j0 = 0, jp1 = 0, jm1 = 0;
            var some = _density * GridSpacing / timeStep;
            for (int j = 1; j < NumCellsY - 1; j++)
            {
                for (int i = 1; i < NumCellsX - 1; i++)
                {
                    j0 = j * NumCellsX;
                    jp1 = j0 + NumCellsX;
                    jm1 = j0 - NumCellsX;

                    var d = u[i + 1+ j0] - u[i+ j0] + v[i+ jp1] - v[i+ j0];
                    var st = s[i + 1+ j0] + s[i - 1+ j0] + s[i+ jp1] + s[i+ jm1];
                    if (st == 0.0f)
                        continue;

                    d *= _overRelaxation;
                    var invst = 1.0f / st;

                    u[i+ j0] += d * s[i - 1+ j0] * invst;
                    u[i+1+ j0] -= d * s[i + 1+ j0] * invst;
                    v[i+ j0] += d * s[i+ jm1] * invst;
                    v[i+ jp1] -= d * s[i+ jp1] * invst;

                    p[i+j0] -= (d / st) * some;
                }
            }
        }

        private void AdvectVelocities(float timeStep)
        {
             CopyArrays(u, newu);
             CopyArrays(v, newv);

            int j0 = 0, jp1 = 0, jm1 = 0;
            var h2 = GridSpacing / 2.0f;
           for (int j = 1; j < NumCellsY; j++)
           {
                for (int i = 1; i < NumCellsX; i++)
                {
                    j0 = j * NumCellsX;

                    if (s[i+ j0] == 0)
                        continue;

                    jp1 = j0 + NumCellsX;
                    jm1 = j0 - NumCellsX;

                    if (s[i-1+ j0] != 0 && j < NumCellsY-1)
                    {
                        var vprime = (v[i+ j0] + v[i+ jp1] + v[i + 1+ j0] + v[i + 1+ jp1]) * 0.25f;
                        var xpos = (i * GridSpacing) - (timeStep * u[i+j0]);
                        var ypos = (j * GridSpacing) + h2 - (timeStep * vprime);
                        newu[i+ j0] = sampleField(xpos, ypos, Fields.U_Field);//InterpolateU(xpos, ypos);
                    }

                    if (s[i+ jm1] != 0 && i < NumCellsX-1)
                    {
                        var uprime = (u[i+ j0] + u[i+ jp1] + u[i + 1+ j0] + u[i + 1+ jp1]) * 0.25f;
                        var xpos = (i * GridSpacing) + h2 - (timeStep * uprime);
                        var ypos = (j * GridSpacing) - (timeStep * v[i+ j0]);
                        newv[i+ j0] = sampleField(xpos, ypos, Fields.V_Field);//InterpolateV(xpos, ypos);
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

            int j0 = 0, jp1 = 0, jm1 = 0;
            var h2 = GridSpacing / 2.0f;

            for (int j = 0; j < NumCellsY-1; j++)
            {
                for (int i = 0; i < NumCellsX-1; i++)
                {
                    j0 = j * NumCellsX;

                    if (s[i+ j0] == 0)
                        continue;

                    jp1 = j0 + NumCellsX;
                    jm1 = j0 - NumCellsX;

                    var cu = (u[i+ j0] + u[i + 1+ j0]) * 0.5f;
                    var cv = (v[i+ j0] + v[i+ jp1]) * 0.5f;
                    var x = i * GridSpacing + h2 - timeStep * cu;
                    var y = j * GridSpacing + h2 - timeStep * cv;
                    newm[i+ j0] = sampleField(x, y, Fields.S_Field);// InterpolateM(x, y);
                }
            }

            CopyArrays(newm, m);

            //SwapArrays(ref m, ref newm);
        }

        private void CopyArrays(float[] from, float[] to)
        {
            var index = 0;
            for (int j = 0; j < NumCellsY - 1; j++)
                for (int i = 0; i < NumCellsX - 1; i++)
                {
                    index = i + j * NumCellsX;
                    to[index] = from[index];
                }
        }

        private void SwapArrays(ref float[] a, ref float[] b)
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
        //private void extrapolate()
        //{
        //    for (int i = 0; i < NumCellsX; i++)
        //    {
        //        u[i, 0] = u[i, 1];
        //        u[i, NumCellsY - 1] = u[i, NumCellsY - 2];
        //    }

        //    for (int j = 0; j < NumCellsY; j++)
        //    {
        //        v[0, j] = v[1, j];
        //        v[NumCellsX-1, j] = v[NumCellsX - 2, j];
        //    }
        //}

        public float sampleField(float x, float y, Fields field)
        {
            var n = NumCellsX;
            var h = GridSpacing;
            var h1 = 1.0f / h;
            var h2 = h / 2.0f;

            x = Math.Max(Math.Min(x, NumCellsX), h);
            y = Math.Max(Math.Min(y, NumCellsY), h);

            var dx = 0.0f;
            var dy = 0.0f;

            float[] f;

            switch (field)
            {
                case Fields.U_Field: { f = u; dy = h2;} break;
                case Fields.V_Field: { f = v; dx = h2;} break;
                case Fields.S_Field: { f = m; dx = h2; dy = h2;} break;
                default:
                    throw new Exception($"Unknown {nameof(Fields)} value");
            }

            var x0 = Math.Min(Math.Floor((x-dx)*h1), NumCellsX-1);
            var tx = ((x - dx) - x0 * h) * h1;
            var x1 = Math.Min(x0+1, NumCellsX-1);

            var y0 = Math.Min(Math.Floor((y - dy) * h1), NumCellsY - 1);
            var ty = ((y - dy) - y0 * h) * h1;
            var y1 = Math.Min(y0 + 1, NumCellsY - 1);

            var sx = 1.0f - tx;
            var sy = 1.0f - ty;

            var val = sx * sy * f[(int)x0 + ((int)y0*NumCellsX)] +
                      tx * sy * f[(int)x1 + ((int)y0 * NumCellsX)] +
                      tx * ty * f[(int)x1 + ((int)y1 * NumCellsX)] +
                      sx * ty * f[(int)x0 + ((int)y1 * NumCellsX)];

            return (float)val;
        }
    }
}