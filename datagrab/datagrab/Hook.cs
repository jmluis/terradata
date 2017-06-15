using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;
using TerraLimb;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Item = TerraLimb.Item;

namespace datagrab
{
    public class Hook : Main
    {
        protected override void Initialize()
        {
            Content.RootDirectory = Program.WORKING_FOLDER + @"\Content";
            Lang.InitializeLegacyLocalization();
            base.Initialize();
        }

        private KeyboardState oldKbState;

        protected override void Update(GameTime gameTime)
        {
            var kbState = Keyboard.GetState();
            // Press SHIFT + TAB to dump
            if (kbState.IsKeyUp(Keys.Tab) && kbState.PressingShift() && oldKbState.IsKeyDown(Keys.Tab))
            {
                DumpItems(0, ItemID.Count);
                DumpBuffs(0, BuffID.Count);
                DumpPrefixes(0, PrefixID.Count);
            }

            if (kbState.IsKeyUp(Keys.F1) && kbState.PressingShift() && oldKbState.IsKeyDown(Keys.F1))
            {
                // reading
                //String json = File.ReadAllText("items.json");
                //Dictionary<int, TerraLimb.Item> items = JsonConvert.DeserializeObject<Dictionary<int, TerraLimb.Item>>(json);
            }
            oldKbState = kbState;
            base.Update(gameTime);
        }

        private void DumpItems(int low, int high)
        {
            var items = new Dictionary<int, Item>();
            Item itm;
            for (var i = low; i < high; i++)
            {
                itm = GetItem(i);
                if (itm.ItemID != 0)
                    items.Add(i, itm);
            }
            File.WriteAllText(@"items.json",
                JsonConvert.SerializeObject(items, Formatting.None,
                    new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore}));
        }

        private void DumpBuffs(int low, int high)
        {
            var buffs = new Dictionary<int, Buff>();
            for (var i = low; i < high; i++)
            {
                var buff = GetBuff(i);
                if (buff.BuffID != 0)
                    buffs.Add(i, buff);
            }
            File.WriteAllText(@"buffs.json",
                JsonConvert.SerializeObject(buffs, Formatting.None,
                    new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore}));
        }

        private void DumpPrefixes(int low, int high)
        {
            var prefixes = new Dictionary<int, Prefix>();
            var prfx = new Prefix();
            for (var i = low; i < high; i++)
            {
                prfx.ID = (byte) i;
                prfx.Name = Language.GetTextValue(Lang.prefix[i].Value);
                prefixes.Add(i, prfx);
            }
            File.WriteAllText(@"prefixes.json",
                JsonConvert.SerializeObject(prefixes, Formatting.None,
                    new JsonSerializerSettings {DefaultValueHandling = DefaultValueHandling.Ignore}));
        }

        private Buff GetBuff(int type)
        {
            var buff = new Buff();
            buff.BuffID = type;
            buff.BuffName = Language.GetTextValue(Lang.GetBuffName(type));
            buff.BuffDescription = Language.GetTextValue(Lang.GetBuffDescription(type));
            return buff;
        }

        private Item GetItem(int type)
        {
            var item = new Item();
            var terraItem = new Terraria.Item();

            terraItem.SetDefaults(type);

            item.ItemID = terraItem.netID;
            item.MaxStack = terraItem.maxStack;
            item.ItemName = Lang.GetItemName(terraItem.netID).ToNetworkText().ToString();

            item.BackSlot = terraItem.backSlot;
            item.BalloonSlot = terraItem.balloonSlot;
            item.BodySlot = terraItem.bodySlot;
            item.FaceSlot = terraItem.faceSlot;
            item.FrontSlot = terraItem.frontSlot;
            item.HandOffSlot = terraItem.handOffSlot;
            item.HandOnSlot = terraItem.handOnSlot;
            item.HeadSlot = terraItem.headSlot;
            item.LegSlot = terraItem.legSlot;
            item.NeckSlot = terraItem.neckSlot;
            item.ShieldSlot = terraItem.shieldSlot;
            item.ShoeSlot = terraItem.shoeSlot;
            item.WaistSlot = terraItem.waistSlot;
            item.WingSlot = terraItem.wingSlot;

            item.Color = terraItem.color.PackedValue == 0
                ? null
                : new int[] {terraItem.color.A, terraItem.color.R, terraItem.color.G, terraItem.color.B};

            return item;
        }
    }
}
