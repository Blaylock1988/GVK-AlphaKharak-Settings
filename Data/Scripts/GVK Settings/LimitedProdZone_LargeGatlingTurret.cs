﻿using SpaceEngineers.Game.ModAPI;
using System;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
using VRage.Game.ModAPI;
using Sandbox.ModAPI;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using VRage.Utils;
using System.Collections.Generic;

namespace LimitedProdZone
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_LargeGatlingTurret), false)]
    public class LimitedProdZone_LargeGatlingTurret : MyGameLogicComponent
    {
        private IMyLargeGatlingTurret weapon;
        private IMyPlayer client;
        private bool isServer;
        private bool inZone;
        public static List<IMyBeacon> beaconList = new List<IMyBeacon>();
        private Vector3D limitedProdCenterCoord = new Vector3D(62495, 28019, 37195); //[Coordinates:{X:62495.55 Y:28019.04 Z:37195.71}]

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            weapon = (Entity as IMyLargeGatlingTurret);
            if (weapon != null)
            {
                NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                NeedsUpdate |= MyEntityUpdateEnum.EACH_10TH_FRAME;
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            isServer = MyAPIGateway.Multiplayer.IsServer;
            client = MyAPIGateway.Session.LocalHumanPlayer;

            if (isServer)
            {
                weapon.IsWorkingChanged += WorkingStateChange;
            }
        }

        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();

            try
            {
                if (isServer)
                {
                    if (!weapon.Enabled) return;

                    foreach (var beacon in beaconList)
                    {                        
                        if (beacon == null) continue;
                        if (!beacon.Enabled) continue;
                        if (Vector3D.Distance(weapon.GetPosition(), limitedProdCenterCoord) < 20000)
                        {
                            string strSubBlockType = weapon.BlockDefinition.SubtypeId.ToString();
                            bool isBasicLargeGatlingTurret = false;
                            isBasicLargeGatlingTurret = strSubBlockType.Contains("Basic");

                            if (isBasicLargeGatlingTurret == false)
                            {
								inZone = true;
								weapon.Enabled = false;
								return;
                            }
                        }
                    }

                    inZone = false;
                }
            }
            catch (Exception exc)
            {
                MyLog.Default.WriteLineAndConsole($"Failed looping through beacon list: {exc}");
            }
        }

        private void WorkingStateChange(IMyCubeBlock block)
        {
            if (!weapon.Enabled)
            {
                foreach (var beacon in beaconList)
                {
                    if (beacon == null) continue;
                    if (!beacon.Enabled) continue;
                    if (Vector3D.Distance(weapon.GetPosition(), limitedProdCenterCoord) < 20000)
                    {
                        string strSubBlockType = weapon.BlockDefinition.SubtypeId.ToString();
                        Boolean isBasicLargeGatlingTurret = false;
                        isBasicLargeGatlingTurret = strSubBlockType.Contains("Basic");

                        if (isBasicLargeGatlingTurret == false)
                        {
							weapon.Enabled = false;
                        }
                    }
                }               
            }
        }

        public override void Close()
        {
            if (Entity == null)
                return;
        }

        public override void OnRemovedFromScene()
        {

            base.OnRemovedFromScene();

            if (Entity == null || Entity.MarkedForClose)
            {
                return;
            }

            var Block = Entity as IMyLargeGatlingTurret;

            if (Block == null) return;

            try
            {
                if (isServer)
                {
                    weapon.IsWorkingChanged -= WorkingStateChange;
                }

            }
            catch (Exception exc)
            {

                MyLog.Default.WriteLineAndConsole($"Failed to deregister event: {exc}");
                return;
            }
            //Unregister any handlers here
        }
    }
}
