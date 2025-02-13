﻿using Microsoft.VisualBasic;
using OmsiHook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Telepathy;
using System.Runtime.CompilerServices;

namespace Multiplayer_Client
{
    internal class GameClient
    {
        public Tuple<int, long, long> LastPing;
        private OmsiHook.OmsiHook omsi;
        private Dictionary<int, OmsiRoadVehicleInst> Vehicles = new Dictionary<int, OmsiRoadVehicleInst>();
        public GameClient(int playerId) {
            omsi = new OmsiHook.OmsiHook();
            try
            {
                omsi.AttachToOMSI().Wait(10000);
                Console.WriteLine("Připojeno k OMSI");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba připojení k OMSI: {ex.Message}");
            }
            Vehicles[playerId] = omsi.GetRoadVehicleInst(playerId);
        }

        int i = 0;
        public void UpdateVehicles(OMSIMPMessages.Vehicle_Position_Update update)
        {
            if (Vehicles.TryGetValue(update.ID, out var vehicle))
            {
                if (update.ID == omsi.Globals.PlayerVehicleIndex) return;
                //if (i % 20 == 0)
                    vehicle.Position = update.position;
                vehicle.Rotation = update.rotation;
                Console.WriteLine($"[{update.ID}] Applying velocity: {update.velocity.x} {update.velocity.y} {update.velocity.z}");
                vehicle.Velocity = update.velocity;
                vehicle.MyKachelPnt = update.tile;
                vehicle.RelMatrix = update.relmatrix;
                vehicle.Acc_Local = update.acclocal;

                var posMat = Matrix4x4.CreateFromQuaternion(update.rotation);
                posMat.Translation = update.position;
                var absPosMat = Matrix4x4.Multiply(posMat, Matrix4x4.Identity/*update.relmatrix*/);
                Matrix4x4.Invert(absPosMat, out var absPosMatInv);

                vehicle.Pos_Mat = posMat;
                vehicle.AbsPosition = absPosMat;
                vehicle.AbsPosition_Inv = absPosMatInv;
                vehicle.Used_RelVec = ((Matrix4x4)update.relmatrix).Translation;
                //vehicle.AI_Blinker_L = 1;
                //vehicle.AI_Blinker_R = 1;
                //vehicle.AI_var = 1;


                //vehicle.AbsPosition = update.abs_position;
                //vehicle.AbsPosition_Inv = update.abs_position_inv;
                i++;
            }
            else
            {
                Console.WriteLine($"Client ID {update.ID} doesn't exist, creating");
                Vehicles[update.ID] = omsi.GetRoadVehicleInst(update.ID);
                UpdateVehicles(update);
            }
        }

        public void Tick(Telepathy.Client client, int playerId)
        {
            if (omsi.GetRoadVehicleInst(playerId).IsNull || !client.Connected)
                return;

            var vehicle = omsi.GetRoadVehicleInst(playerId);
            //Console.WriteLine($"\x1b[8;0HP:{vehicle.Position}/{vehicle.MyKachelPnt}\x1b[9;0HR:{vehicle.Rotation}\x1b[10;0HV:{vehicle.Velocity}\x1b[11;0HB:{vehicle.Acc_Local} / {((Vehicles.ContainsKey(0)) ? (Vehicles[0].Acc_Local.ToString()):"-")}");
            byte[] buff = new byte[Unsafe.SizeOf<OMSIMPMessages.Player_Position_Update>() + 4];
            int out_pos = 0;
            Console.WriteLine($"[{playerId}] Sending velocity: {vehicle.Velocity.x} {vehicle.Velocity.y} {vehicle.Velocity.z}");
            FastBinaryWriter.Write(buff, ref out_pos, OMSIMPMessages.Messages.UPDATE_PLAYER_POSITION);
            FastBinaryWriter.Write(buff, ref out_pos, new OMSIMPMessages.Player_Position_Update()
            {
                ID = playerId,
                position = vehicle.Position,
                tile = vehicle.MyKachelPnt,
                rotation = vehicle.Rotation,
                velocity = vehicle.Velocity,
                relmatrix = vehicle.RelMatrix,
                acclocal = vehicle.Acc_Local
                //vehicle = vehicle.RoadVehicle.MyPath

            }); ;
            client.Send(buff);
        }
    }
}
