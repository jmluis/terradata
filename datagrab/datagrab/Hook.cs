using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
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
        private bool _written;
        protected override void Initialize()
        {
            Content.RootDirectory = Program.CONTENT_PATH;
            Lang.InitializeLegacyLocalization();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Console.WriteLine("Setting Up the Data Directory");
            var dumpPath = Path.Combine(Environment.CurrentDirectory, "Data");

            if (!Directory.Exists(dumpPath))
                Directory.CreateDirectory(dumpPath);

            if (!Directory.Exists(Path.Combine(dumpPath, "Misc")))
                Directory.CreateDirectory(Path.Combine(dumpPath, "Misc"));

            Console.WriteLine("Loading Textures from {0}", Program.CONTENT_PATH);
            Console.WriteLine();
            var dir = new DirectoryInfo(Program.CONTENT_PATH);
            var watch = new Stopwatch();
            watch.Start();
            foreach (FileInfo xnb in dir.GetFiles("*.xnb"))
            {
                Texture2D texture;
                var name = xnb.Name.Replace(".xnb", "");
                try
                {
                    texture = base.Content.Load<Texture2D>(name);
                }
                catch (ContentLoadException)
                {
                    Console.WriteLine("Failed  to load " + name);
                    continue;
                }

                if (name.Contains("_"))
                {
                    CreateDirectories(dumpPath, name);
                    texture.SaveAsPng(File.Create(Path.Combine(dumpPath, name.Replace("_", "/") + ".png")), texture.Width, texture.Height);
                }
                else
                    texture.SaveAsPng(File.Create(Path.Combine(dumpPath, "Misc", name + ".png")), texture.Width, texture.Height);
            }
            watch.Stop();
            Console.WriteLine("Completed in {0} Seconds", watch.Elapsed.TotalSeconds);
            base.LoadContent();
        }


        public void CreateDirectories(string dataPath, string name)
        {
            var names = name.Split('_');
            for (int i = 0; i < names.Length - 1; i++)
            {
                dataPath = Path.Combine(dataPath, names[i]);
                if (!Directory.Exists(dataPath))
                    Directory.CreateDirectory(dataPath);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            var watch = new Stopwatch();
            if (!_written)
            {
                watch.Start();
                Console.Write("Writing json files...items...");
                DumpItems(ItemID.Count, Path.Combine(Environment.CurrentDirectory, "Data"));
                Console.Write("buffs...");
                DumpBuffs(BuffID.Count, Path.Combine(Environment.CurrentDirectory, "Data"));
                Console.Write("prefixes...");
                DumpPrefixes(PrefixID.Count, Path.Combine(Environment.CurrentDirectory, "Data"));
                Console.WriteLine("Done!");
                _written = true;
                watch.Stop();
                Console.WriteLine("Completed in {0} Seconds", watch.Elapsed.TotalSeconds);
            }
            else
            {
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();
                Environment.Exit(0);
            }
            // reading
            //String json = File.ReadAllText("items.json");
            //Dictionary<int, TerraLimb.Item> items = JsonConvert.DeserializeObject<Dictionary<int, TerraLimb.Item>>(json);
            base.Update(gameTime);
        }

        public void DumpItems(int count, string path)
        {
            var items = new Dictionary<int, Item>();
            var itm = new Item
            {
                ItemID = 0,
                ItemName = "(none)"
            };

            items.Add(0, itm);
            for (var i = 0; i < count; i++)
            {
                itm = GetItem(i);
                if (itm.ItemID != 0)
                    items.Add(i, itm);
            }
            File.WriteAllText(path + @"\items.json",
                JsonConvert.SerializeObject(items, Formatting.None,
                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));
        }

        public void DumpBuffs(int count, string path)
        {
            var buffs = new Dictionary<int, Buff>();
            var buff = new Buff
            {
                BuffID = 0,
                BuffName = "(none)",
                BuffDescription = ""
            };
            buffs.Add(0, buff);
            for (var i = 0; i < count; i++)
            {
                buff = GetBuff(i);
                if (buff.BuffID != 0)
                    buffs.Add(i, buff);
            }
            File.WriteAllText(path + @"\buffs.json",
                JsonConvert.SerializeObject(buffs, Formatting.None,
                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));
        }

        public void DumpPrefixes(int count, string path)
        {
            var prefixes = new Dictionary<int, Prefix>();
            var prfx = new Prefix
            {
                ID = 0,
                Name = "(none)"
            };
            prefixes.Add(0, prfx);
            for (var i = 0; i < count; i++)
            {
                prfx.ID = (byte)i;
                prfx.Name = Language.GetTextValue(Lang.prefix[i].Value);
                if (prfx.ID != 0)
                    prefixes.Add(i, prfx);
            }
            File.WriteAllText(path + @"\prefixes.json",
                JsonConvert.SerializeObject(prefixes, Formatting.None,
                    new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore }));
        }

        private static Buff GetBuff(int type)
        {
            var buff = new Buff
            {
                BuffID = type,
                BuffName = Language.GetTextValue(Lang.GetBuffName(type)),
                BuffDescription = Language.GetTextValue(Lang.GetBuffDescription(type))
            };
            return buff;
        }

        private static Item GetItem(int type)
        {
            var terraItem = new Terraria.Item();
            terraItem.SetDefaults(type);
            return new Item
            {
                ItemID = terraItem.netID,
                MaxStack = terraItem.maxStack,
                ItemName = Lang.GetItemName(terraItem.netID).ToNetworkText().ToString(),
                BackSlot = terraItem.backSlot,
                BalloonSlot = terraItem.balloonSlot,
                BodySlot = terraItem.bodySlot,
                FaceSlot = terraItem.faceSlot,
                FrontSlot = terraItem.frontSlot,
                HandOffSlot = terraItem.handOffSlot,
                HandOnSlot = terraItem.handOnSlot,
                HeadSlot = terraItem.headSlot,
                LegSlot = terraItem.legSlot,
                NeckSlot = terraItem.neckSlot,
                ShieldSlot = terraItem.shieldSlot,
                ShoeSlot = terraItem.shoeSlot,
                WaistSlot = terraItem.waistSlot,
                WingSlot = terraItem.wingSlot,
                Color = terraItem.color.PackedValue == 0
                    ? null
                    : new int[] { terraItem.color.A, terraItem.color.R, terraItem.color.G, terraItem.color.B }
            };
        }
    }
}
