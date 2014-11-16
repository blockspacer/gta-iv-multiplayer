﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;

namespace gtaiv_multiplayer_client
{
    class PlayerPedController
    {
        Dictionary<byte, Ped> peds;

        public PlayerPedController()
        {
            peds = new Dictionary<byte, Ped>();
        }

        public Ped getById(byte id)
        {
            if (!peds.ContainsKey(id))
            {
                //if (peds.ContainsKey(id)) peds.Remove(id);
                peds.Add(id, World.CreatePed(new Vector3(0, 0, 0), Gender.Male));
            }
            return peds[id];
        }

        public void destroy(byte id)
        {
            if (peds == null)
            {
                peds = new Dictionary<byte, Ped>();
            }
            if (peds.ContainsKey(id))
            {
                peds[id].Delete();
                peds.Remove(id);
            }
        }
    }
}
