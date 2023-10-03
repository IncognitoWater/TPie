﻿using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;
using System.Numerics;
using System.Reflection;
using TPie.Helpers;
using LuminaAction = Lumina.Excel.GeneratedSheets.Action;

namespace TPie.Models.Elements
{
    public class ActionElement : RingElement
    {
        private uint _actionId;

        [JsonProperty]
        public uint ActionID
        {
            get => _actionId;
            set
            {
                _actionId = value;

                if (value > 0)
                {
                    ExcelSheet<LuminaAction>? sheet = Plugin.DataManager.GetExcelSheet<LuminaAction>();
                    Data = sheet?.GetRow(value);

                    IconID = Data?.Icon ?? 0;
                }
                else
                {
                    Data = null;
                    IconID = 0;
                }
            }
        }

        [JsonIgnore] public LuminaAction? Data { get; private set; }

        public ActionElement(uint actionId)
        {
            ActionID = actionId;
        }

        public ActionElement() : this(0) { }

        public override void ExecuteAction()
        {
            if (Data != null)
            {
                ChatHelper.SendChatMessage($"/ac \"{Data.Name}\"");
            }
        }

        public override bool IsValid()
        {
            if (_actionId == 0) return false;

            uint jobId = Plugin.ClientState.LocalPlayer?.ClassJob.Id ?? 0;
            if (jobId <= 0) return false;

            ClassJobCategory? classJobCategory = Data?.ClassJobCategory.Value;
            if (classJobCategory == null) return false;

            PropertyInfo? property = classJobCategory.GetType().GetProperty(JobsHelper.JobNames[jobId]);
            bool? value = (bool?)property?.GetValue(classJobCategory);
            if (value.HasValue)
            {
                return value.Value;
            }

            return classJobCategory.Name == "All Classes" || classJobCategory.Name.ToString().Contains(JobsHelper.JobNames[jobId]);
        }

        public override string InvalidReason()
        {
            return "This action won't show on the current Job.";
        }

        public override string Description()
        {
            return Data?.Name ?? "";
        }

        public override void Draw(Vector2 position, Vector2 size, float scale, bool selected, uint color, float alpha, bool tooltip, ImDrawListPtr drawList)
        {
            base.Draw(position, size, scale, selected, color, alpha, tooltip, drawList);

            size = size * scale;

            if (Plugin.Settings.ShowCooldowns)
            {
                DrawHelper.DrawCooldown(ActionType.Action, ActionID, position, size, scale, drawList);

                if (CooldownHelper.GetMaxCharges(ActionID) > 1)
                {
                    int charges = CooldownHelper.GetCharges(ActionID);
                    string text = $"{charges}";
                    Vector2 textPos = position + (size / 2f) - new Vector2(4 * scale, 6 * scale);

                    uint redColor = 0xFF0149C6;
                    uint fillColor = charges == 0 ? redColor : 0xFFFFFFFF;
                    uint outlineColor = charges == 0 ? 0xFFFFFFFF : redColor;
                    DrawHelper.DrawOutlinedText(text, textPos, true, scale, fillColor, outlineColor, drawList);
                }
            }
        }
    }
}
