﻿using BatenKaitosHardMode.Properties;
using System;
using System.Threading;
using System.Windows.Forms;

namespace BatenKaitosHardMode
{
    public partial class Form1 : Form
    {
        private MemoryManipulator manager;
        private bool hpChanged;

        public Form1()
        {
            InitializeComponent();
            manager = new MemoryManipulator();

            // Restore settings
            partyHPText.Text = Settings.Default.PartyHPModifier.ToString();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            setupTimer.Start();
        }

        private void AdjustPartyMapHP()
        {
            // Map
            if (manager.GetPartyMapHP(PartyMembers.Kalas) > 40)
            {
                manager.SetPartyMapHP(PartyMembers.Kalas, 30);
                Log("Set Kalas HP to 60");
            }
        }

        private void AdjustPartyBattleHP()
        {
            // Get modifier
            double partyHPModifier = Convert.ToDouble(partyHPText.Text) / 100;

            // Adjust all character's HP
            int partySize = manager.GetBattlerCount();
            for (int i = 0; i < partySize; ++i)
            {
                // Calculate MAX HP
                int oldMaxHP = (int) manager.GetPartyBattleMaxHP(i);
                int modifiedMaxHP = (int)Math.Ceiling(manager.GetPartyBattleMaxHP(i) * partyHPModifier);

                // Check if current HP go over MAX
                long currentHP = manager.GetPartyBattleHP(i);
                if (currentHP > modifiedMaxHP)
                {
                    // Cap at MAX HP
                    manager.SetPartyBattleHP(i, modifiedMaxHP);
                }

                // Write MAX HP
                manager.SetPartyBattleMaxHP(i, modifiedMaxHP);
                Log("Set Char " + i + " HP from " + oldMaxHP + " to " + modifiedMaxHP.ToString());
            }
        }

        private void mainTimer_Tick(object sender, EventArgs e)
        {
            //AdjustPartyMapHP();

            manager.ReadBattleFlags();
            if (manager.IsInBattle() && manager.AreCharsBattleReady())
            {
                if (!hpChanged)
                {
                    lblBattle.Text = "Battle ID: " + manager.GetEnemyID() + ", Battlers: " + manager.GetBattlerCount();
                    AdjustPartyBattleHP();
                    hpChanged = true;
                }
            } 
            else
            {
                lblBattle.Text = "Waiting for battle...";
                hpChanged = false;
            }
        }

        private void Log(string message)
        {
            logBox.Items.Add($"[{DateTime.Now.ToShortTimeString()}] {message}");
        }

        private void setupTimer_Tick(object sender, EventArgs e)
        {
            // Setup loop. Runs as long as the game is not running
            if (manager.IsDolphinRunning())
            {
                if (manager.IsBKNARunning())
                {
                    lblStatus.Text = "Baten Kaitos running";
                    battlePanel.Visible = true;
                    mainTimer.Start();
                    setupTimer.Stop();
                }
                else
                {
                    lblStatus.Text = "Baten Kaitos (NOA) is not running";
                }

            }
            else
            {
                lblStatus.Text = "Dolphin 5.0 not running";
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Store settings
            Settings.Default.PartyHPModifier = Convert.ToInt32(partyHPText.Text);
            Settings.Default.Save();
        }
    }
}
