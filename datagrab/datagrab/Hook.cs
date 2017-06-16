using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
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
        readonly List<string> _toDownload = new List<string>
        {
            "Armor",
            "Background",
            "BackPack",
            "Buff",
            "Female",
            "Inventory",
            "Item",
            "ItemFlame",
            "Player",
            "Skin"
        };
        private bool _written;
        private string _dumpPath;
        protected override void Initialize()
        {
            _dumpPath = Path.Combine(Environment.CurrentDirectory, "Data");
            Content.RootDirectory = Program.CONTENT_PATH;
            Lang.InitializeLegacyLocalization();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            Console.WriteLine("Setting Up the Data Directory");
            if (!Directory.Exists(_dumpPath))
                Directory.CreateDirectory(_dumpPath);
            WritePackage();
            DumpTextures();
            base.LoadContent();
        }

        void WritePackage()
        {
            Console.Write($"Writing Package...project {Program.PROJECT_NUMBER} Rev. {Program.REVISION_NUMBER}...");
            var package = new TerraLimb.Package
            {
                Version = Program.VERSION_NUMBER,
                TerrariaVersion = Main.curRelease,
                TerrariaVersionString = Main.versionNumber,
                ProjectNo = Program.PROJECT_NUMBER,
                RevNo = Program.REVISION_NUMBER
            };

            File.WriteAllText(_dumpPath + @"\package.json", JsonConvert.SerializeObject(package, Formatting.None));
            Console.WriteLine("done!");
        }

        void DumpTextures()
        {
            Console.WriteLine("Loading Textures from {0}", Program.CONTENT_PATH + "\\Images");
            var watch = new Stopwatch();
            watch.Start();
            foreach (var xnb in FilterFiles(Program.CONTENT_PATH + "\\Images", _toDownload.ToArray()))
            {
                var fileinfo = new FileInfo(xnb);
                var name = fileinfo.Name.Replace(".xnb", "");

                if (!name.Contains("_") || !_toDownload.Contains(name.Split('_')[0])) continue;
                var texture = base.Content.Load<Texture2D>("Images\\" + name);
                CreateDirectories(_dumpPath, name);
                texture.SaveAsPng(File.Create(Path.Combine(_dumpPath, name.Replace("_", "/") + ".png")),
                    texture.Width, texture.Height);
                texture.Dispose();
            }
            watch.Stop();
            Console.WriteLine("Completed in {0} Seconds", watch.Elapsed.TotalSeconds);
        }

        public IEnumerable<string> FilterFiles(string path, params string[] exts)
        {
            var files = Directory.EnumerateFiles(path, "*.xnb", SearchOption.AllDirectories);
            return
                    files.Where(file => exts.Any(file.Contains));
        }


        public void CreateDirectories(string dataPath, string name)
        {
            var names = name.Split('_');
            for (var i = 0; i < names.Length - 1; i++)
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
                DumpItems(ItemID.Count, _dumpPath);
                Console.Write("buffs...");
                DumpBuffs(BuffID.Count, _dumpPath);
                Console.Write("prefixes...");
                DumpPrefixes(PrefixID.Count, _dumpPath);
                Console.WriteLine("Done!");
                _written = true;
                watch.Stop();
                Console.WriteLine("Completed in {0} Seconds", watch.Elapsed.TotalSeconds);
            }
            else
            {
                Console.WriteLine("Press any key to exit");
                Process.Start(_dumpPath);
                Environment.Exit(0);
            }
            base.Update(gameTime);
        }

        public void DumpItems(int count, string path)
        {
            var items = new Dictionary<int, Item>();
            var itm = new Item
            {
                ItemID = 0,
                ItemName = string.Empty,
                Nick = string.Empty
            };

            items.Add(0, itm);
            for (var i = 0; i < count; i++)
            {
                itm = GetItem(i);
                if (itm.ItemID != 0)
                    items.Add(i, itm);
            }
            File.WriteAllText(path + @"\items.json",
                        JsonConvert.SerializeObject(items, Formatting.Indented,
                            new JsonSerializerSettings
                            {
                                DefaultValueHandling = DefaultValueHandling.Ignore
                            }));
        }

        public void DumpBuffs(int count, string path)
        {
            var buffs = new Dictionary<int, Buff>();
            var buff = new Buff
            {
                BuffID = 0,
                BuffName = "",
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
                Name = ""
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
            terraItem.netDefaults(type);
            return new Item
            {
                ItemID = terraItem.netID,
                Nick = string.Empty,
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
