// Copyright 2014 Adrian Chlubek. This file is part of GTA Multiplayer IV project.
// Use of this source code is governed by a MIT license that can be
// found in the LICENSE file.
using System;
using System.Diagnostics;
using System.Windows.Forms;
using MIVSDK;

namespace MIVClientGUI
{
    public partial class GameLaunchedForm : Form
    {
        public GameLaunchedForm()
        {
            InitializeComponent();
        }

        private void GameLaunchedForm_Load(object sender, EventArgs e)
        {
        }

        private void GameLaunchedForm_Shown(object sender, EventArgs e)
        {
            FileSystemOverlay.prepareForMIV();
            Application.DoEvents();
            Process gameProcess = new Process();
            gameProcess.StartInfo = new ProcessStartInfo("LaunchGTAIV.exe");
            gameProcess.Start();
            gameProcess.WaitForExit();
            FileSystemOverlay.prepareForSP();
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
        }
    }
}