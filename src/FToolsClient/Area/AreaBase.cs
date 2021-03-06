﻿using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FToolsClient.Area
{
    abstract class AreaBase
    {
        public string Identifier { get; set; }
        public AreaType Type { get; set; }
        public dynamic OnEnter { get; set; }
        public dynamic OnExit { get; set; }
        public dynamic Params { get; set; }
        public dynamic Debug { get; set; }

        protected bool PlayerInside = false;

        protected void TriggerEnter()
        {
            Vector3 PlayerPos = Game.PlayerPed.Position;
            BaseScript.TriggerEvent("FTools:OnEnterArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });
            BaseScript.TriggerServerEvent("FTools:OnEnterArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });

            if (this.OnEnter == null)
                return;

            if (this.OnEnter.GetType() == typeof(System.String))
            {
                BaseScript.TriggerEvent((String)this.OnEnter, this.Params);
            }
            else if (this.OnEnter.GetType() == typeof(CitizenFX.Core.CallbackDelegate))
            {   
                if (this.Params != null)
                {
                    ((CallbackDelegate)this.OnEnter).Invoke(this.Params);
                }
                else
                {
                    ((CallbackDelegate)this.OnEnter)();
                }
            }
        }

        protected void TriggerExit()
        {
            Vector3 PlayerPos = Game.PlayerPed.Position;
            BaseScript.TriggerEvent("FTools:OnExitArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });
            BaseScript.TriggerServerEvent("FTools:OnExitArea", Identifier, new { X = PlayerPos.X, Y = PlayerPos.Y, Z = PlayerPos.Z });

            if (this.OnExit == null)
                return;

            if (this.OnExit.GetType() == typeof(System.String))
            {
                BaseScript.TriggerEvent((String)this.OnExit, this.Params);
            }
            else if (this.OnExit.GetType() == typeof(CitizenFX.Core.CallbackDelegate))
            {
                if (this.Params != null)
                {
                    ((CallbackDelegate)this.OnExit).Invoke(this.Params);
                }
                else
                {
                    ((CallbackDelegate)this.OnExit)();
                }
            }
        }

        abstract public void Check();
        abstract public void Draw();
    }
}
