﻿using System;
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
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_Refinery), false)]
    public class LimitedProdZone_Refinery : MyGameLogicComponent
    {
        private IMyRefinery refinery;
        private bool isServer;
        public static List<IMyBeacon> beaconList = new List<IMyBeacon>();
        private Vector3D limitedProdCenterCoord = new Vector3D(62495, 28019, 37195); //[Coordinates:{X:62495.55 Y:28019.04 Z:37195.71}]

        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            refinery = (Entity as IMyRefinery);
            if (refinery != null)
            {
                NeedsUpdate |= MyEntityUpdateEnum.BEFORE_NEXT_FRAME;
                NeedsUpdate |= MyEntityUpdateEnum.EACH_100TH_FRAME;
            }
        }

        public override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();

            isServer = MyAPIGateway.Multiplayer.IsServer;

            if (isServer)
            {
                refinery.IsWorkingChanged += WorkingStateChange;
            }
        }

        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();

            try
            {
                if (isServer)
                {
                    if (!refinery.Enabled) return;

                    foreach (var beacon in beaconList)
                    {                        
						if (beacon == null || !beacon.Enabled) continue;
						if (Vector3D.DistanceSquared(refinery.GetPosition(), limitedProdCenterCoord) < 1225000000) // use squared of 35,000m for better performance
                        {
                            string strSubBlockType = refinery.BlockDefinition.SubtypeId.ToString();
                            bool isBasicRefinery = false;
                            isBasicRefinery = (strSubBlockType.Contains("Blast Furnace") || strSubBlockType.Contains("LargeRefinery_NPC_CU"));
                            if (isBasicRefinery == false)
                            {
								refinery.Enabled = false;
								return;
                            }
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                MyLog.Default.WriteLineAndConsole($"Failed looping through beacon list: {exc}");
            }
        }

        private void WorkingStateChange(IMyCubeBlock block)
        {
            if (!refinery.Enabled)
            {
                foreach (var beacon in beaconList)
                {
					if (beacon == null || !beacon.Enabled) continue;
					if (Vector3D.DistanceSquared(refinery.GetPosition(), limitedProdCenterCoord) < 1225000000) // use squared of 35,000m for better performance
                    {
                        string strSubBlockType = refinery.BlockDefinition.SubtypeId.ToString();
                        Boolean isBasicRefinery = false;
						isBasicRefinery = (strSubBlockType.Contains("Blast Furnace") || strSubBlockType.Contains("LargeRefinery_NPC_CU"));
                        if (isBasicRefinery == false)
                        {
							refinery.Enabled = false;
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

            var Block = Entity as IMyRefinery;

            if (Block == null) return;

            try
            {
                if (isServer)
                {
                    refinery.IsWorkingChanged -= WorkingStateChange;
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
