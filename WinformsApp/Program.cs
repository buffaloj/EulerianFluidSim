using System.Diagnostics;

namespace EulerianFluidSim
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            using (MainForm f = new MainForm())
            {
                bool done = false;
                f.FormClosing += (sender, e) => { done = true; };

                Stopwatch sw = Stopwatch.StartNew();
                long start = 0;
                long end = sw.ElapsedMilliseconds;
                sw.Start();

                f.Show();
                while (!done) 
                {
                    start = end;
                    end = sw.ElapsedMilliseconds;

                    var elapsed = end - start;
                    float deltaTime = (float)elapsed / 1000.0f;

                    if (deltaTime > 0.000001f)
                    {
                        f.StepSimulation(deltaTime);
                        f.Refresh();
                    }
                    
                    Application.DoEvents(); // default message pump
                }
            }
        }
    }
}