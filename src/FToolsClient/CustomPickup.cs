﻿using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FToolsShared;

namespace FToolsClient
{
    class CustomPickup
    {
        public Vector3 Pos { get; set; }
        public Model Model { get; set; }
        public bool Dynamic { get; set; }
        public bool OnGround { get; set; }
        public bool DeleteOnAction { get; set; }
        public EventAction EventAction { get; set; }
        
        public int Handle
        {
            get
            {
                return PickupProp.Handle;
            }
        }

        public int NetHandle { get; set; }

        public bool Exist
        {
            get
            {
                return PickupProp != null && PickupProp.Exists();
            }
        }

        public bool Created { get; set; } = false;
        public bool Deleted { get; set; } = false;

        private Prop PickupProp = null;

        public static CustomPickup FromInfo(CustomPickupInfo info)
        {
            int handle = API.NetworkGetEntityFromNetworkId(info.NetHandle);
            Prop prop = new Prop(handle);
            if (prop == null)
            {
                return null;
            }

            CustomPickup pickup = new CustomPickup
            {
                PickupProp = prop,
                NetHandle = API.ObjToNet(prop.Handle),
                Pos = prop.Position,
                Model = prop.Model,
                Dynamic = info.Dynamic,
                OnGround = info.OnGround,
                DeleteOnAction = info.DeleteOnAction,
                EventAction = new EventAction
                {
                    Type = (EventActionType)info.EventActionType,
                    Control = (Control)info.Control,
                    HelpText = info.HelpText,
                    Callback = info.CallBack,
                    Params = info.Parameters
                },
                Created = info.Created
            };

            return pickup;
        }

        public async void Create()
        {
            try
            {
                if (!API.HasModelLoaded((uint)Model.Hash))
                {
                    await Model.Request(5000);
                }

                PickupProp = await World.CreateProp(Model, Pos, Dynamic, OnGround);
                await BaseScript.Delay(100);
                API.SetEntityAsMissionEntity(PickupProp.Handle, false, false);

                NetHandle = API.ObjToNet(PickupProp.Handle);

                API.SetNetworkIdCanMigrate(NetHandle, true);
                API.SetNetworkIdExistsOnAllMachines(NetHandle, false);

                int maxPlayers = Int32.Parse(API.GetResourceMetadata(API.GetCurrentResourceName(), "max_clients", 0));
               
                for (var i = 0; i < maxPlayers; i++)
                {
                    if (API.NetworkIsPlayerActive(i))
                    {
                        API.SetNetworkIdSyncToPlayer(NetHandle, i, false);
                    }
                }

                Created = true;

                BaseScript.TriggerServerEvent("FTools:PickupCreated", this.GetInfo());                
            }
            catch(Exception Ex)
            {
                Debug.WriteLine("Error: " + Ex.Message);
            }
        }

        public void Delete()
        {
            Deleted = true;
            BaseScript.TriggerServerEvent("FTools:PickupDeleted", NetHandle);
        }

        public void Load(int handle)
        {
            PickupProp = new Prop(handle);
            Pos = PickupProp.Position;
            Model = PickupProp.Model;
            Dynamic = !PickupProp.IsPositionFrozen;
            OnGround = Dynamic;
        }

        public void Check()
        {
            if (EventAction != null && PickupProp != null && Math.Sqrt(Game.PlayerPed.Position.DistanceToSquared(PickupProp.Position)) < 1.5)
            {
                EventAction.Draw();
                if (EventAction.CheckPickupControl(NetHandle) && DeleteOnAction)
                {
                    Delete();
                }
            }
        }

        public CustomPickupInfo GetInfo()
        {
            CustomPickupInfo info = new CustomPickupInfo
            {
                NetHandle = NetHandle,
                Dynamic = Dynamic,
                OnGround = OnGround,
                DeleteOnAction = DeleteOnAction,
                EventActionType = EventAction != null ? (int)EventAction.Type : 0,
                Control = EventAction != null ? (int)EventAction.Control : 0,
                HelpText = EventAction?.HelpText,
                CallBack = EventAction?.Callback,
                Parameters = EventAction?.Params,
                Created = Created
            };

            return info;
        }
    }
}
