﻿// 
// Radegast Metaverse Client
// Copyright (c) 2009-2012, Radegast Development Team
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 
//     * Redistributions of source code must retain the above copyright notice,
//       this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the application "Radegast", nor the names of its
//       contributors may be used to endorse or promote products derived from
//       this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
// DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
// SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
// CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
// OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
// $Id$
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace Radegast
{
    public class RLVManager : IDisposable
    {
        #region Events
        /// <summary>The event subscribers. null if no subcribers</summary>
        private EventHandler<RLVEventArgs> m_RLVRuleChanged;

        /// <summary>Raises the RLVRuleChanged event</summary>
        /// <param name="e">An RLVRuleChangedEventArgs object containing the
        /// data returned from the data server</param>
        protected virtual void OnRLVRuleChanged(RLVEventArgs e)
        {
            EventHandler<RLVEventArgs> handler = m_RLVRuleChanged;
            try
            {
                if (handler != null)
                    handler(this, e);
            }
            catch (Exception) { }

        }

        /// <summary>Thread sync lock object</summary>
        private readonly object m_RLVRuleChangedLock = new object();

        /// <summary>Triggered when an RLVRuleChangedUpdate packet is received,
        /// telling us what our avatar is currently wearing
        /// <see cref="RequestRLVRuleChanged"/> request.</summary>
        public event EventHandler<RLVEventArgs> RLVRuleChanged
        {
            add { lock (m_RLVRuleChangedLock) { m_RLVRuleChanged += value; } }
            remove { lock (m_RLVRuleChangedLock) { m_RLVRuleChanged -= value; } }
        }
        #endregion

        #region Helper classes, methods, structs and enums
        public struct RLVWearable
        {
            public string Name { get; set; }
            public WearableType Type { get; set; }
        }

        public struct RLVAttachment
        {
            public string Name { get; set; }
            public AttachmentPoint Point { get; set; }
        }

        public static readonly List<RLVWearable> RLVWearables;
        public static readonly List<RLVAttachment> RLVAttachments;
        
        static RLVManager()
        {
            RLVWearables = new List<RLVWearable>(16);
            RLVWearables.Add(new RLVWearable() { Name = "gloves", Type = WearableType.Gloves });
            RLVWearables.Add(new RLVWearable() { Name = "jacket", Type = WearableType.Jacket });
            RLVWearables.Add(new RLVWearable() { Name = "pants", Type = WearableType.Pants });
            RLVWearables.Add(new RLVWearable() { Name = "shirt", Type = WearableType.Shirt });
            RLVWearables.Add(new RLVWearable() { Name = "shoes", Type = WearableType.Shoes });
            RLVWearables.Add(new RLVWearable() { Name = "skirt", Type = WearableType.Skirt });
            RLVWearables.Add(new RLVWearable() { Name = "socks", Type = WearableType.Socks });
            RLVWearables.Add(new RLVWearable() { Name = "underpants", Type = WearableType.Underpants });
            RLVWearables.Add(new RLVWearable() { Name = "undershirt", Type = WearableType.Undershirt });
            RLVWearables.Add(new RLVWearable() { Name = "skin", Type = WearableType.Skin });
            RLVWearables.Add(new RLVWearable() { Name = "eyes", Type = WearableType.Eyes });
            RLVWearables.Add(new RLVWearable() { Name = "hair", Type = WearableType.Hair });
            RLVWearables.Add(new RLVWearable() { Name = "shape", Type = WearableType.Shape });
            RLVWearables.Add(new RLVWearable() { Name = "alpha", Type = WearableType.Alpha });
            RLVWearables.Add(new RLVWearable() { Name = "tattoo", Type = WearableType.Tattoo });

            RLVAttachments = new List<RLVAttachment>();
            RLVAttachments.Add(new RLVAttachment() { Name = "none", Point = AttachmentPoint.Default });
            RLVAttachments.Add(new RLVAttachment() { Name = "chest", Point = AttachmentPoint.Chest });
            RLVAttachments.Add(new RLVAttachment() { Name = "skull", Point = AttachmentPoint.Skull });
            RLVAttachments.Add(new RLVAttachment() { Name = "left shoulder", Point = AttachmentPoint.LeftShoulder });
            RLVAttachments.Add(new RLVAttachment() { Name = "right shoulder", Point = AttachmentPoint.RightShoulder });
            RLVAttachments.Add(new RLVAttachment() { Name = "left hand", Point = AttachmentPoint.LeftHand });
            RLVAttachments.Add(new RLVAttachment() { Name = "right hand", Point = AttachmentPoint.RightHand });
            RLVAttachments.Add(new RLVAttachment() { Name = "left foot", Point = AttachmentPoint.LeftFoot });
            RLVAttachments.Add(new RLVAttachment() { Name = "right foot", Point = AttachmentPoint.RightFoot });
            RLVAttachments.Add(new RLVAttachment() { Name = "spine", Point = AttachmentPoint.Spine });
            RLVAttachments.Add(new RLVAttachment() { Name = "pelvis", Point = AttachmentPoint.Pelvis });
            RLVAttachments.Add(new RLVAttachment() { Name = "mouth", Point = AttachmentPoint.Mouth });
            RLVAttachments.Add(new RLVAttachment() { Name = "chin", Point = AttachmentPoint.Chin });
            RLVAttachments.Add(new RLVAttachment() { Name = "left ear", Point = AttachmentPoint.LeftEar });
            RLVAttachments.Add(new RLVAttachment() { Name = "right ear", Point = AttachmentPoint.RightEar });
            RLVAttachments.Add(new RLVAttachment() { Name = "left eyeball", Point = AttachmentPoint.LeftEyeball });
            RLVAttachments.Add(new RLVAttachment() { Name = "right eyeball", Point = AttachmentPoint.RightEyeball });
            RLVAttachments.Add(new RLVAttachment() { Name = "nose", Point = AttachmentPoint.Nose });
            RLVAttachments.Add(new RLVAttachment() { Name = "r upper arm", Point = AttachmentPoint.RightUpperArm });
            RLVAttachments.Add(new RLVAttachment() { Name = "r forearm", Point = AttachmentPoint.RightForearm });
            RLVAttachments.Add(new RLVAttachment() { Name = "l upper arm", Point = AttachmentPoint.LeftUpperArm });
            RLVAttachments.Add(new RLVAttachment() { Name = "l forearm", Point = AttachmentPoint.LeftForearm });
            RLVAttachments.Add(new RLVAttachment() { Name = "right hip", Point = AttachmentPoint.RightHip });
            RLVAttachments.Add(new RLVAttachment() { Name = "r upper leg", Point = AttachmentPoint.RightUpperLeg });
            RLVAttachments.Add(new RLVAttachment() { Name = "r lower leg", Point = AttachmentPoint.RightLowerLeg });
            RLVAttachments.Add(new RLVAttachment() { Name = "left hip", Point = AttachmentPoint.LeftHip });
            RLVAttachments.Add(new RLVAttachment() { Name = "l upper leg", Point = AttachmentPoint.LeftUpperLeg });
            RLVAttachments.Add(new RLVAttachment() { Name = "l lower leg", Point = AttachmentPoint.LeftLowerLeg });
            RLVAttachments.Add(new RLVAttachment() { Name = "stomach", Point = AttachmentPoint.Stomach });
            RLVAttachments.Add(new RLVAttachment() { Name = "left pec", Point = AttachmentPoint.LeftPec });
            RLVAttachments.Add(new RLVAttachment() { Name = "right pec", Point = AttachmentPoint.RightPec });
            RLVAttachments.Add(new RLVAttachment() { Name = "center 2", Point = AttachmentPoint.HUDCenter2 });
            RLVAttachments.Add(new RLVAttachment() { Name = "top right", Point = AttachmentPoint.HUDTopRight });
            RLVAttachments.Add(new RLVAttachment() { Name = "top", Point = AttachmentPoint.HUDTop });
            RLVAttachments.Add(new RLVAttachment() { Name = "top left", Point = AttachmentPoint.HUDTopLeft });
            RLVAttachments.Add(new RLVAttachment() { Name = "center", Point = AttachmentPoint.HUDCenter });
            RLVAttachments.Add(new RLVAttachment() { Name = "bottom left", Point = AttachmentPoint.HUDBottomLeft });
            RLVAttachments.Add(new RLVAttachment() { Name = "bottom", Point = AttachmentPoint.HUDBottom });
            RLVAttachments.Add(new RLVAttachment() { Name = "bottom right", Point = AttachmentPoint.HUDBottomRight });
            RLVAttachments.Add(new RLVAttachment() { Name = "neck", Point = AttachmentPoint.Neck });
            RLVAttachments.Add(new RLVAttachment() { Name = "root", Point = AttachmentPoint.Root });
        }

        public static WearableType WearableFromString(string type)
        {
            var found = RLVWearables.FindAll(w => w.Name == type);
            if (found.Count == 1)
            {
                return found[0].Type;
            }
            else
            {
                return WearableType.Invalid;
            }
        }

        public static AttachmentPoint AttachmentPointFromString(string point)
        {
            var found = RLVAttachments.FindAll(a => a.Name == point);
            if (found.Count == 1)
            {
                return found[0].Point;
            }
            else
            {
                return AttachmentPoint.Default;
            }
        }

        #endregion Helper classes, structs and enums

        public bool Enabled
        {

            get
            {
                if (instance.GlobalSettings["rlv_enabled"].Type == OSDType.Unknown)
                {
                    instance.GlobalSettings["rlv_enabled"] = new OSDBoolean(false);
                }

                return instance.GlobalSettings["rlv_enabled"].AsBoolean();
            }

            set
            {
                if (Enabled != instance.GlobalSettings["rlv_enabled"].AsBoolean())
                {
                    instance.GlobalSettings["rlv_enabled"] = new OSDBoolean(value);
                    OnRLVRuleChanged(new RLVEventArgs(null));
                }

                if (value)
                    StartTimer();
                else
                    StopTimer();
            }
        }

        RadegastInstance instance;
        GridClient client { get { return instance.Client; } }
        Regex rlv_regex = new Regex(@"(?<behaviour>[^:=]+)(:(?<option>[^=]+))?=(?<param>\w+)", RegexOptions.Compiled);
        List<RLVRule> rules = new List<RLVRule>();
        System.Timers.Timer CleanupTimer;

        public RLVManager(RadegastInstance instance)
        {
            this.instance = instance;

            if (Enabled)
            {
                StartTimer();
            }
        }

        public void Dispose()
        {
            StopTimer();
        }

        void StartTimer()
        {
            StopTimer();
            CleanupTimer = new System.Timers.Timer()
            {
                Enabled = true,
                Interval = 120 * 1000 // two minutes
            };

            CleanupTimer.Elapsed += new System.Timers.ElapsedEventHandler(CleanupTimer_Elapsed);
        }

        void CleanupTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            List<UUID> objecs = new List<UUID>();
            lock (rules)
            {
                foreach (var rule in rules)
                {
                    if (!objecs.Contains(rule.Sender))
                        objecs.Add(rule.Sender);
                }
            }

            foreach (UUID obj in objecs)
            {
                if (client.Network.CurrentSim.ObjectsPrimitives.Find((Primitive p) => { return p.ID == obj; }) == null)
                {
                    Clear(obj);
                }
            }
        }

        void StopTimer()
        {
            if (CleanupTimer != null)
            {
                CleanupTimer.Enabled = false;
                CleanupTimer.Dispose();
                CleanupTimer = null;
            }
        }

        public bool TryProcessCMD(ChatEventArgs e)
        {
            if (!Enabled || !e.Message.StartsWith("@")) return false;

            if (e.Message == "@clear")
            {
                Clear(e.SourceID);
                return true;
            }

            foreach (var cmd in e.Message.Substring(1).Split(','))
            {
                var m = rlv_regex.Match(cmd);
                if (!m.Success) continue;

                var rule = new RLVRule();
                rule.Behaviour = m.Groups["behaviour"].ToString().ToLower(); ;
                rule.Option = m.Groups["option"].ToString().ToLower();
                rule.Param = m.Groups["param"].ToString().ToLower();
                rule.Sender = e.SourceID;
                rule.SenderName = e.FromName;

                Logger.DebugLog(rule.ToString());

                if (rule.Param == "rem") rule.Param = "y";
                if (rule.Param == "add") rule.Param = "n";

                if (rule.Param == "n")
                {
                    lock (rules)
                    {
                        var existing = rules.Find(r =>
                            r.Behaviour == rule.Behaviour &&
                            r.Sender == rule.Sender &&
                            r.Option == rule.Option);

                        if (existing != null)
                        {
                            rules.Remove(existing);
                        }
                        rules.Add(rule);
                        OnRLVRuleChanged(new RLVEventArgs(rule));
                    }
                    continue;
                }

                if (rule.Param == "y")
                {
                    lock (rules)
                    {
                        if (rule.Option == "")
                        {
                            rules.RemoveAll((RLVRule r) =>
                                {
                                    return r.Behaviour == rule.Behaviour && r.Sender == rule.Sender;
                                });
                        }
                        else
                        {
                            rules.RemoveAll((RLVRule r) =>
                            {
                                return r.Behaviour == rule.Behaviour && r.Sender == rule.Sender && r.Option == rule.Option;
                            });
                        }
                    }

                    OnRLVRuleChanged(new RLVEventArgs(rule));
                    continue;
                }


                switch (rule.Behaviour)
                {
                    case "version":
                        int chan = 0;
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            client.Self.Chat("RestrainedLife viewer v1.23 (" + Properties.Resources.RadegastTitle + "." + RadegastBuild.CurrentRev + ")", chan, ChatType.Normal);
                        }
                        break;

                    case "versionnew":
                        chan = 0;
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            client.Self.Chat("RestrainedLove viewer v1.23 (" + Properties.Resources.RadegastTitle + "." + RadegastBuild.CurrentRev + ")", chan, ChatType.Normal);
                        }
                        break;


                    case "versionnum":
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            client.Self.Chat("1230100", chan, ChatType.Normal);
                        }
                        break;

                    case "getgroup":
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            UUID gid = client.Self.ActiveGroup;
                            if (instance.Groups.ContainsKey(gid))
                            {
                                client.Self.Chat(instance.Groups[gid].Name, chan, ChatType.Normal);
                            }
                        }
                        break;

                    case "setgroup":
                        {
                            if (rule.Param == "force")
                            {
                                foreach (var g in instance.Groups.Values)
                                {
                                    if (g.Name.ToLower() == rule.Option)
                                    {
                                        client.Groups.ActivateGroup(g.ID);
                                    }
                                }
                            }
                        }
                        break;

                    case "getsitid":
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            Avatar me;
                            if (client.Network.CurrentSim.ObjectsAvatars.TryGetValue(client.Self.LocalID, out me))
                            {
                                if (me.ParentID != 0)
                                {
                                    Primitive seat;
                                    if (client.Network.CurrentSim.ObjectsPrimitives.TryGetValue(me.ParentID, out seat))
                                    {
                                        client.Self.Chat(seat.ID.ToString(), chan, ChatType.Normal);
                                        break;
                                    }
                                }
                            }
                            client.Self.Chat(UUID.Zero.ToString(), chan, ChatType.Normal);
                        }
                        break;

                    case "getstatusall":
                    case "getstatus":
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            string sep = "/";
                            string filter = "";

                            if (!string.IsNullOrEmpty(rule.Option))
                            {
                                var parts = rule.Option.Split(';');
                                if (parts.Length > 1 && parts[1].Length > 0)
                                {
                                    sep = parts[1].Substring(0, 1);
                                }
                                if (parts.Length > 0 && parts[0].Length > 0)
                                {
                                    filter = parts[0].ToLower();
                                }
                            }

                            lock (rules)
                            {
                                string res = "";
                                rules
                                    .FindAll(r => (rule.Behaviour == "getstatusall" || r.Sender == rule.Sender) && r.Behaviour.Contains(filter))
                                    .ForEach(objRule =>
                                {
                                    res += sep + objRule.Behaviour;
                                });
                                client.Self.Chat(res, chan, ChatType.Normal);
                            }
                        }
                        break;

                    case "sit":
                        UUID sitTarget = UUID.Zero;

                        if (rule.Param == "force" && UUID.TryParse(rule.Option, out sitTarget) && sitTarget != UUID.Zero)
                        {
                            instance.State.SetSitting(true, sitTarget);
                        }
                        break;

                    case "unsit":
                        if (rule.Param == "force")
                        {
                            instance.State.SetSitting(false, UUID.Zero);
                        }
                        break;

                    case "setrot":
                        double rot = 0.0;

                        if (rule.Param == "force" && double.TryParse(rule.Option, System.Globalization.NumberStyles.Float, Utils.EnUsCulture, out rot))
                        {
                            client.Self.Movement.UpdateFromHeading(Math.PI / 2d - rot, true);
                        }
                        break;

                    case "tpto":
                        var coord = rule.Option.Split('/');

                        try
                        {
                            float gx = float.Parse(coord[0], Utils.EnUsCulture);
                            float gy = float.Parse(coord[1], Utils.EnUsCulture);
                            float z = float.Parse(coord[2], Utils.EnUsCulture);
                            float x = 0, y = 0;

                            instance.TabConsole.DisplayNotificationInChat("Starting teleport...");
                            ulong h = Helpers.GlobalPosToRegionHandle(gx, gy, out x, out y);
                            client.Self.RequestTeleport(h, new Vector3(x, y, z));
                        }
                        catch (Exception) { }

                        break;

                    #region #RLV folder and outfit manipulation
                    case "getoutfit":
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            var wearables = client.Appearance.GetWearables();
                            string res = "";

                            // Do we have a specific wearable to check, ie @getoutfit:socks=99
                            if (!string.IsNullOrEmpty(rule.Option))
                            {
                                if (wearables.ContainsKey(WearableFromString(rule.Option)))
                                {
                                    res = "1";
                                }
                                else
                                {
                                    res = "0";
                                }
                            }
                            else
                            {
                                for (int i = 0; i < RLVWearables.Count; i++)
                                {
                                    if (wearables.ContainsKey(RLVWearables[i].Type))
                                    {
                                        res += "1";
                                    }
                                    else
                                    {
                                        res += "0";
                                    }

                                }
                            }
                            client.Self.Chat(res, chan, ChatType.Normal);
                        }
                        break;

                    case "getattach":
                        if (int.TryParse(rule.Param, out chan) && chan > 0)
                        {
                            string res = "";
                            var attachments = client.Network.CurrentSim.ObjectsPrimitives.FindAll(p => p.ParentID == client.Self.LocalID);
                            if (attachments.Count > 0)
                            {
                                var myPoints = new List<AttachmentPoint>(attachments.Count);
                                for (int i = 0; i < attachments.Count; i++)
                                {
                                    if (!myPoints.Contains(attachments[i].PrimData.AttachmentPoint))
                                    {
                                        myPoints.Add(attachments[i].PrimData.AttachmentPoint);
                                    }
                                }

                                // Do we want to check one single attachment
                                if (!string.IsNullOrEmpty(rule.Option))
                                {
                                    if (myPoints.Contains(AttachmentPointFromString(rule.Option)))
                                    {
                                        res = "1";
                                    }
                                    else
                                    {
                                        res = "0";
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < RLVAttachments.Count; i++)
                                    {
                                        if (myPoints.Contains(RLVAttachments[i].Point))
                                        {
                                            res += "1";
                                        }
                                        else
                                        {
                                            res += "0";
                                        }
                                    }
                                }


                            }
                            client.Self.Chat(res, chan, ChatType.Normal);
                        }
                        break;

                    #endregion #RLV folder and outfit manipulation

                }
            }


            return true;
        }

        public void Clear(UUID id)
        {
            lock (rules)
            {
                rules.RemoveAll((RLVRule r) => { return r.Sender == id; });
            }
        }

        public bool RestictionActive(string behaviour)
        {
            if (!Enabled) return false;

            if (rules.FindAll((RLVRule r) => { return r.Behaviour == behaviour && string.IsNullOrEmpty(r.Option); }).Count > 0)
            {
                return true;
            }

            return false;
        }

        public bool RestictionActive(string behaviour, string exception)
        {
            if (!Enabled) return false;
            var set = rules.FindAll((RLVRule r) => { return r.Behaviour == behaviour; });

            if (set.Count > 0 &&
                set.FindAll((RLVRule r) => { return r.Option == exception; }).Count == 0 &&
                set.FindAll((RLVRule r) => { return string.IsNullOrEmpty(r.Option); }).Count > 0
                )
            {
                return true;
            }

            return false;
        }

        public List<string> GetOptions(string behaviour)
        {
            List<string> ret = new List<string>();

            foreach (var rule in rules.FindAll((RLVRule r) => { return r.Behaviour == behaviour && !string.IsNullOrEmpty(r.Option); }))
            {
                if (!ret.Contains(rule.Option)) ret.Add(rule.Option);
            }

            return ret;
        }

        public bool AllowDetach(AttachmentInfo a)
        {
            if (!Enabled || a == null) return true;

            if (rules.FindAll((RLVRule r) => { return r.Behaviour == "detach" && r.Sender == a.Prim.ID; }).Count > 0)
            {
                return false;
            }

            return true;
        }

        public bool AutoAcceptTP(UUID agent)
        {
            if (!Enabled || agent == UUID.Zero) return false;

            if (rules.FindAll((RLVRule r) => { return r.Behaviour == "accepttp" && (r.Option == "" || r.Option == agent.ToString()); }).Count > 0)
            {
                return true;
            }

            return false;
        }
    }
}
