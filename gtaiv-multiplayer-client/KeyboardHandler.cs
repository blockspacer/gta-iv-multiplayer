﻿using GTA;
using MIVSDK;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace MIVClient
{
    public class KeyboardHandler
    {
        public int cursorpos = 0;
        public bool inKeyboardTypingMode;

        private Client client;
        private float gamescale;
        private GTA.KeyboardLayoutUS keyboardUS;

        public KeyboardHandler(Client client)
        {
            this.client = client;
            keyboardUS = new KeyboardLayoutUS();
            inKeyboardTypingMode = false;
            client.KeyDown += new GTA.KeyEventHandler(this.eventOnKeyDown);
            client.KeyUp += new GTA.KeyEventHandler(this.eventOnKeyUp);
        }

        private void eventOnKeyDown(object sender, GTA.KeyEventArgs e)
        {
            if (lastKeyDown != null && lastKeyDown == (int)e.Key) return;
            if (!inKeyboardTypingMode && e.Key == System.Windows.Forms.Keys.T)
            {
                inKeyboardTypingMode = true;
                cursorpos = 0;
                client.getPlayer().CanControlCharacter = false;
            }
            else if (inKeyboardTypingMode)
            {
                if (e.Key == System.Windows.Forms.Keys.Enter)
                {
                    if (client.chatController.currentTypedText.Length > 0)
                    {
                        var bpf = new BinaryPacketFormatter(Commands.Chat_sendMessage);
                        bpf.add(client.chatController.currentTypedText);
                        client.serverConnection.write(bpf.getBytes());
                    }
                    client.chatController.currentTypedText = "";
                    cursorpos = 0;
                    inKeyboardTypingMode = false;
                    client.getPlayer().CanControlCharacter = true;
                }
                else if (e.Key == System.Windows.Forms.Keys.Escape)
                {
                    client.chatController.currentTypedText = "";
                    cursorpos = 0;
                    inKeyboardTypingMode = false;
                }
                else if (e.Key == System.Windows.Forms.Keys.Left)
                {
                    cursorpos = cursorpos > 0 ? cursorpos - 1 : cursorpos;
                }
                else if (e.Key == System.Windows.Forms.Keys.Right)
                {
                    cursorpos = cursorpos >= client.chatController.currentTypedText.Length ? client.chatController.currentTypedText.Length : cursorpos + 1;
                }
                else if (e.Key == System.Windows.Forms.Keys.Back)
                {
                    string leftcut = cursorpos > 0 ? client.chatController.currentTypedText.Substring(0, cursorpos - 1) : client.chatController.currentTypedText;
                    string rightcut = client.chatController.currentTypedText.Substring(cursorpos, client.chatController.currentTypedText.Length - cursorpos);
                    client.chatController.currentTypedText = leftcut + rightcut;
                    cursorpos = cursorpos > 0 ? cursorpos - 1 : cursorpos;
                }
                else
                {
                    string leftcut = client.chatController.currentTypedText.Substring(0, cursorpos);

                    string rightcut =
                        cursorpos >= client.chatController.currentTypedText.Length ?
                        "" :
                        client.chatController.currentTypedText.Substring(cursorpos, client.chatController.currentTypedText.Length - cursorpos);
                    string newstr = keyboardUS.ParseKey((int)e.Key, e.Shift, e.Control, e.Alt);
                    client.chatController.currentTypedText = leftcut + newstr + rightcut;
                    cursorpos += newstr.Length;
                }
                return;
            }

            foreach (int id in Enumerable.Range((int)System.Windows.Forms.Keys.D0, (int)System.Windows.Forms.Keys.D9))
            {
                if (e.Key == (System.Windows.Forms.Keys)id && e.Alt) client.saveBindPoint(id - (int)System.Windows.Forms.Keys.D0);
                if (e.Key == (System.Windows.Forms.Keys)id && e.Control) client.teleportToBindPoint(id - (int)System.Windows.Forms.Keys.D0);
            }

            if (e.Key == System.Windows.Forms.Keys.Add)
            {
                gamescale *= 1.3f;
                Game.TimeScale = gamescale;
            }
            if (e.Key == System.Windows.Forms.Keys.Subtract)
            {
                gamescale *= 0.7f;
                Game.TimeScale = gamescale;
            }
            if (e.Key == System.Windows.Forms.Keys.Multiply)
            {
                gamescale = 1.0f;
                Game.TimeScale = gamescale;
            }

            if (e.Key == System.Windows.Forms.Keys.Insert)
            {
                if (client.getPlayerPed().isInVehicle())
                {
                    client.getPlayerPed().CurrentVehicle.Velocity *= 2.0f;
                }
                else
                {
                    client.getPlayerPed().Velocity *= 2.0f;
                }
            }
            if (e.Key == System.Windows.Forms.Keys.G)
            {
                Vehicle veh = World.GetClosestVehicle(client.getPlayerPed().Position, 20.0f);
                if (veh != null && veh.Exists())
                {
                    VehicleSeat seat = veh.GetFreePassengerSeat();
                    client.getPlayerPed().Task.EnterVehicle(veh, seat);
                }
            }

            if (e.Key == System.Windows.Forms.Keys.L)
            {
                try
                {
                    if (client.client != null && client.client.Connected)
                    {
                        client.client.Close();
                    }
                    client.client = new TcpClient();
                    INIReader ini = new INIReader(System.IO.File.ReadAllLines("server.ini"));
                    IPAddress address = IPAddress.Parse(ini.getString("ip"));
                    client.nick = ini.getString("nick");
                    int port = ini.getInt("port");

                    client.client.Connect(address, port);

                    Client.currentData = UpdateDataStruct.Zero;

                    client.serverConnection = new ServerConnection(client);

                    World.CurrentDayTime = new TimeSpan(12, 00, 00);
                    World.PedDensity = 0;
                    World.CarDensity = 0;
                    client.currentState = ClientState.Connecting;
                }
                catch
                {
                    client.currentState = ClientState.Disconnected;
                    if (client.client != null && client.client.Connected)
                    {
                        client.client.Close();
                    }
                    throw;
                }
            }

            if (client.currentState == ClientState.Connected)
            {
                client.chatController.writeDebug("keydown");
                var bpf = new BinaryPacketFormatter(Commands.Keys_down);
                bpf.add((int)e.Key);
                client.serverConnection.write(bpf.getBytes());
            }
            lastKeyDown = (int)e.Key;
            lastKeyUp = 0;
        }

        int? lastKeyUp, lastKeyDown;
        private void eventOnKeyUp(object sender, GTA.KeyEventArgs e)
        {
            if (lastKeyUp != null && lastKeyUp == (int)e.Key) return;
            if (client.currentState == ClientState.Connected)
            {
                client.chatController.writeDebug("keyup");
                var bpf = new BinaryPacketFormatter(Commands.Keys_up);
                bpf.add((int)e.Key);
                client.serverConnection.write(bpf.getBytes());
            }
            lastKeyUp = (int)e.Key;
            lastKeyDown = 0;
        }
    }
}