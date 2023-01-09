using EulerianFluidSim;

namespace SimRunnerApp.Winforms
{
    public partial class MainForm : Form
    {
        private Simulation _simulation;
        private ColorSimRenderer _simulationRenderer;

        public void StepSimulation(float elapsed)
        {
            var fps = 1.0f / elapsed;
            fpsLabel.Text = $"{fps}fps";

            _simulation?.StepSimulation(elapsed);
        }

        public void InvalidateSimView()
        {
            simView.Invalidate();
        }

        private void CreateSim()
        {
            var width = ClientRectangle.Width;
            var height = ClientRectangle.Height;
            width /= 5;
            height /= 5;
            _simulation = new Simulation(width, height, 1.0f, 1.0f, 1.9f);
            _simulationRenderer = simView.SetSimulation(_simulation);
            

            speedBar.Value = (int)_simulation.FlowRate;
            checkPressure.Checked = _simulationRenderer.ShowPressure;
            checkColorSmoke.Checked = _simulationRenderer.ShowColoredSmoke;
            checkFlowLines.Checked = simView.ShowFlowLines;
        }

        public MainForm()
        {
            InitializeComponent();

            CreateSim();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            //CreateSim();
            //Refresh();
        }

        private void speedBar_Scroll(object sender, EventArgs e)
        {
            _simulation.FlowRate = speedBar.Value;
        }

        private void checkPressure_CheckedChanged(object sender, EventArgs e)
        {
            _simulationRenderer.ShowPressure = checkPressure.Checked;
        }

        private void checkColorSmoke_CheckedChanged(object sender, EventArgs e)
        {
            _simulationRenderer.ShowColoredSmoke = checkColorSmoke.Checked;
        }

        private void checkFlowLines_CheckedChanged(object sender, EventArgs e)
        {
            simView.ShowFlowLines = checkFlowLines.Checked;
        }
    }
}