﻿using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;


namespace StarterGrinder
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
    public class StarterGrinder : MySessionComponentBase
    {
        IMySlimBlock reuse_slim;
        IMyAngleGrinder reuse_grinder;
        IMyFaction reuse_faction;
        IMyFaction reuse_faction_grinder;
        string grinder_sub = "AngleGrinder";
        float multiplier = 1.2f;

        // New variables for no damage/grinding in desinated area
        private Vector3D NO_DAMAGE_AREA = new Vector3D(0, 0, 0);
        private float NO_DAMAGE_RADIUS = 20000f;

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            if (MyAPIGateway.Session.IsServer)
            {
                MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, grinder_handler);
            }
        }

        /*public override void LoadData()
        {
            if (!MyAPIGateway.Utilities.GetVariable<float>("basic_grinder_multiplier", out multiplier))
            {
                multiplier = 1.2f;
                MyAPIGateway.Utilities.SetVariable<float>("basic_grinder_multiplier", multiplier);
            }
        }*/

        private void grinder_handler(object target, ref MyDamageInformation info)
        {
            try
            {
                if (info.Type.ToString() == "Grind")
                {
                    reuse_slim = null;
                    reuse_slim = target as IMySlimBlock;
                    reuse_grinder = MyAPIGateway.Entities.GetEntityById(info.AttackerId) as IMyAngleGrinder;
                    if (reuse_grinder == null || reuse_slim == null) return;

                    // Checks if in the desinated zone and DOESN"T allow hacking other blocks
                    if (IsInZone(reuse_slim))
                    {
                        long owner = reuse_slim.CubeGrid.BigOwners.FirstOrDefault();
                        if (owner == 0) return;
                        if (owner == reuse_grinder.OwnerIdentityId) return;
                        //if (reuse_slim.OwnerId == 0 || reuse_slim.BuiltBy == 0) return;
                        //if (reuse_slim.OwnerId == reuse_grinder.OwnerIdentityId) return;
                        //if (reuse_slim.BuiltBy == reuse_grinder.OwnerIdentityId) return;

                        reuse_faction = null;
                        reuse_faction_grinder = null;

                        reuse_faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);
                        reuse_faction_grinder = MyAPIGateway.Session.Factions.TryGetPlayerFaction(reuse_grinder.OwnerIdentityId);

                        if (reuse_faction != null && reuse_faction_grinder != null)
                        {
                            if (reuse_faction == reuse_faction_grinder) return;
                            if (reuse_faction.IsEveryoneNpc() || reuse_faction.Tag.Length >= 4) return;
                        }

                        info.Amount = 0f;
                        return;
                    }

                    // This checks if using the basic grinder to NOT allow hacking of other blocks
                    if (reuse_grinder.DefinitionId.SubtypeName == grinder_sub)
                    {
                        var slim_owner_id = reuse_slim.OwnerId;
                        var slim_built_id = reuse_slim.BuiltBy;
                        var grinder_owner = reuse_grinder.OwnerIdentityId;

                        if (slim_owner_id == 0)
                        {
                            if (reuse_slim.FatBlock != null)
                            {
                                info.Amount *= multiplier;
                                return;
                            }

                            if (slim_built_id == 0)
                            {
                                info.Amount *= multiplier;
                                return;
                            }

                            if (grinder_owner == slim_built_id)
                            {
                                info.Amount *= multiplier;
                                return;
                            }

                            reuse_faction = null;
                            reuse_faction_grinder = null;

                            reuse_faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(slim_built_id);
                            reuse_faction_grinder = MyAPIGateway.Session.Factions.TryGetPlayerFaction(grinder_owner);

                            if (reuse_faction != null && reuse_faction_grinder != null && reuse_faction.FactionId == reuse_faction_grinder.FactionId)
                            {
                                info.Amount *= multiplier;
                                return;
                            }

                            info.Amount = 0f;
                        }
                        else
                        {
                            if (slim_owner_id == grinder_owner)
                            {
                                info.Amount *= multiplier;
                                return;
                            }

                            reuse_faction = null;
                            reuse_faction_grinder = null;

                            reuse_faction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(slim_owner_id);
                            reuse_faction_grinder = MyAPIGateway.Session.Factions.TryGetPlayerFaction(grinder_owner);

                            if (reuse_faction != null && reuse_faction_grinder != null && reuse_faction.FactionId == reuse_faction_grinder.FactionId)
                            {
                                info.Amount *= multiplier;
                                return;
                            }
                            info.Amount = 0f;
                        }
                    }

                }
                else
                {
                    reuse_slim = target as IMySlimBlock;
                    if (reuse_slim == null) return;
                    if (IsInZone(reuse_slim))
                        info.Amount = 0f;
                }
                
            }
            catch (Exception)
            {

            }
        }

        private bool IsInZone(IMySlimBlock block)
        {
            if (Vector3D.Distance(block.CubeGrid.GetPosition(), NO_DAMAGE_AREA) <= NO_DAMAGE_RADIUS) return true;
            return false;
        }
    }
}