using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;

namespace BackgroundsRuleGenerator
{
    public class Rule
    {
        public Backgrounds Module;
        public KMBombInfo BombInfo { get { return Module.BombInfo; } }
        public int ColBacking, ColButton, Counter = 0;
        internal static int[] coordX { get { return Generator.coordX; } }
        internal static int[] coordY { get { return Generator.coordY; } }
        internal static int[,] ManualTable;
        internal static List<Generator.Logger> Possibilities;
        internal MonoRandom rng;
        public List<bool> Rules()
        {
            var list = new List<bool>();
            //var log = new List<string>();
            rng = Module.RuleSeed.GetRNG();
            if (Generator._rng != null && Generator._rng.Seed != rng.Seed)
                Generator.pass = false;
            var one = rng.Seed == 1;
            ColBacking = UnityEngine.Random.Range(0, 8);
            ColButton = UnityEngine.Random.Range(0, 9);
            if (!Generator.pass) Generator.Rules(this);
            if (!one)
            {
                Counter = UnityEngine.Random.Range(0, 2);
                Module.DebugLog(0, "Chosen rules:");
            }
            Module.coordX = coordX;
            Module.coordY = coordY;
            Module.BGManualTable = ManualTable;
            if (!Generator.pass) Possibilities = Generator.Swaps(Generator.Possibilities(this));
            else
            {
                Generator.options1[0] = () => ColBacking;
                Generator.options1[1] = () => ColButton;
                Generator.options1[2] = () => Counter;
            }
            //Rules that check if the submit button contains a digit
            var Submit = Module.Submit.GetComponentInChildren<UnityEngine.TextMesh>();
            if ((Generator.RuleIndicies.Contains(2) || Generator.RuleIndicies.Contains(3)) && Counter == 1) Submit.text = Submit.text + " 0";
            foreach (int index in Generator.RuleIndicies)
            {
                list.Add(Possibilities[index].Func());
                //log.Add(Possibilities[index].Descrip);
                if (!one)
                    Module.DebugLog(0, Possibilities[index].Descrip);
            }
            //Module.DebugLog(string.Join("\n", log.Select(x => Generator.RuleIndicies[log.IndexOf(x)] + ": " + x + " - " + list[log.IndexOf(x)]).ToArray()));
            return list.Concat(new[] { true }).ToList();
        }
    }
    internal static class Generator
    {
        public class Logger
        {
            public Func<bool> Func { get; private set; }
            public string Descrip { get; private set; }
            public Logger(Func<bool> func, string s) { Func = func; Descrip = s; }
        }
        internal static bool pass = false;
        //Instance of RuleSeed
        internal static MonoRandom _rng;
        //The number of rules Backgrounds uses
        private const int count = 9;
        //Rules to be chosen, primary colors to be chosen
        public static int[] RuleIndicies = new int[count], additive,
            //Original manual values to be randomized
            //May look into doing the randomization from scratch in the future
            coordXOriginal = { 0, 3, 2, 3, 1, 5, 4, 1, 2, 4 },
            coordX,
            coordYOriginal = { 2, 1, 4, 3, 5, 4, 1, 2, 3, 0 },
            coordY,
            ManualTableOriginal = {
                3, 2, 9, 1, 7, 4,
                7, 9, 8, 8, 2, 3,
                5, 1, 7, 4, 4, 6,
                6, 4, 2, 6, 8, 5,
                5, 1, 5, 3, 9, 9,
                1, 2, 3, 6, 7, 8
            },
            ManualTable;
        private static bool check;
        private static int possibleCount;

        //Individual items that will be used for comparisons in Possibilities
        internal static List<Func<int>> options1 = new List<Func<int>>();
        internal static List<IEnumerable<int>> options2 = new List<IEnumerable<int>>();
        internal static List<Func<Enum>> options3 = new List<Func<Enum>>();
        internal static List<Func<bool>> possibilities = new List<Func<bool>>();

        //Swap randomized values for seed 1 to output the original manual
        internal static List<Logger> Swaps(List<Logger> list)
        {
            var values = new[] { 324, 1487, 393, 1415, 566, 1176, 941, 1308, 556 };
            var newValues = new[] { 0, 463, 1308, 1311, 8, 22, 1390, 1350, 28 };
            for (int i = 0; i < values.Count(); i++)
            {
                var hold = list[values[i]];
                list[values[i]] = list[newValues[i]];
                list[newValues[i]] = hold;
            }
            return list;
        }

        //"Backing/Button mixed with color makes Button/Backing's color" rules
        private static void DetermineColorCombinations()
        {
            //Primary Colors are Red, Yellow, and Blue
            if (additive.Take(3).ToArray().SequenceEqual(new[] { 0, 2, 4 }))
            {
                //red
                options2.Add(new[] { -1, 2, 1, 4, 5 });
                //yellow
                options2.Add(new[] { -1, 0, 1, 4, 3 });
                //blue
                options2.Add(new[] { -1, 0, 5, 2, 3 });
            }
            //Primary Colors are Red, Green, and Blue
            else
            {
                //red
                options2.Add(new[] { -1, 3, 2, 4, 5 });
                //green
                options2.Add(new[] { -1, 0, 2, 4, 1 });
                //blue
                options2.Add(new[] { -1, 0, 5, 3, 1 });
            }
        }
        
        //Add to list possible objects to be compared for the final rules
        private static void AddToOptions(Rule Rules)
        {
            options1.Add(() => Rules.ColBacking);
            options1.Add(() => Rules.ColButton);
            options1.Add(() => Rules.Counter);
            //Determine what the primary colors will be [Either outdated or additive]
            //Secondaries were also going to be used as primaries, but I forgot to include it
            additive = Primaries().ToArray();
            //Primary Colors
            options2.Add(additive.Take(3).ToList());
            //Secondary Colors
            options2.Add(additive.Skip(3).ToList());
            //Color mixes
            DetermineColorCombinations();
            //Port types
            foreach(Port port in Enum.GetValues(typeof(Port)))
            {
                options3.Add(() => port);
            }
            //Indicator types
            var indicatorValues = Enum.GetValues(typeof(Indicator));
            foreach(Indicator indicator in indicatorValues)
            {
                if (indicator.GetHashCode() < indicatorValues.Length - 1) options3.Add(() => indicator);
            }

            //All possible color combinations of sequence lengths 1, 2, and 3
            //Not including values already used in primary or secondary colors
            var listSize = 1;
            while (listSize < 4)
            {
                //Red, Orange/Cyan, Yellow, Green, Blue, Purple/Magenta, White, Gray, Black
                var orderedArray = new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                var intList = new List<List<int>>();
                foreach (int i in orderedArray)
                {
                    intList.Add(new List<int>());
                    //First digit in the sequence
                    intList.Last().Add(i);
                    if (listSize > 1)
                    {
                        var list = orderedArray.Skip(i + 1);
                        foreach (int j in list)
                        {
                            //Second digit in the sequence
                            intList.Last().Add(j);
                            if (listSize > 2)
                            {
                                var list2 = orderedArray.Skip(j + 1);
                                foreach (int k in list2)
                                {
                                    //Third digit in the sequence
                                    intList.Last().Add(k);
                                    //Prepare for next iteration
                                    intList.Add(new List<int>());
                                    //First digit in next sequence
                                    intList.Last().Add(i);
                                    //Second digit in next sequence
                                    intList.Last().Add(j);
                                }
                                //foreach adds two extra values that are not needed
                                //remove the sequence
                                intList.Remove(intList.Last());
                            }
                            //Prepare for next iteration
                            intList.Add(new List<int>());
                            //First digit in next sequence
                            intList.Last().Add(i);
                        }
                        //foreach adds an extra iteration that is not used
                        //remove the sequence
                        intList.Remove(intList.Last());
                    }
                }
                foreach (List<int> item in intList)
                {
                    if (!options2.Any(x => x.SequenceEqual(item))) options2.Add(item);
                }
                //The size of the sequence, max 3
                listSize++;
            }
        }

        public static void Rules(Rule Rules)
        {
            if (_rng != null && _rng.Seed == Rules.rng.Seed) return;
            _rng = Rules.rng;
            if (!pass)
                ClearAll();
            if (_rng.Seed != 1)
            {
                _rng.ShuffleFisherYates(coordX);
                _rng.ShuffleFisherYates(coordY);
                _rng.ShuffleFisherYates(ManualTable);
            }
            AddToOptions(Rules);
            //ManualTable is a double array, which ShuffleFisherYates does not work on
            //As such, the ManualTable must be recreated using for statements
            Rule.ManualTable = new int[6, 6];
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Rule.ManualTable[i, j] = ManualTable[6 * i + j];
                }
            }
        }

        static void ClearAll()
        {
            coordX = coordXOriginal.ToArray();
            coordY = coordYOriginal.ToArray();
            ManualTable = ManualTableOriginal.ToArray();
            options1.Clear();
            options2.Clear();
            options3.Clear();
            possibilities.Clear();
            check = false;
        }

        //Determine which primary system is being used
        //Outdated or Additive
        //New system replaces orange with cyan and purple with magenta
        //Primaries will return both primary and secondary colors.
        public static List<int> Primaries()
        {
            var i = _rng.Next(0, 2);
            var list1 = new List<int> { 0, 2, 4 };
            var list2 = new List<int> { 0, 3, 4 };
            var list3 = new List<int> { 1, 3, 5 };
            var list4 = new List<int> { 1, 2, 5 };
            if (i == 1)
            {
                Backgrounds.color[1] = UnityEngine.Color.cyan;
                Backgrounds.colorList[1] = "cyan";
                Backgrounds.color[5] = UnityEngine.Color.magenta;
                Backgrounds.colorList[5] = "magenta";
                return list2.Concat(list4).ToList();
            }
            else
            {
                Backgrounds.color[1] = Backgrounds.orange;
                Backgrounds.colorList[1] = "orange";
                Backgrounds.color[5] = Backgrounds.purple;
                Backgrounds.colorList[5] = "purple";
            }
            return list1.Concat(list3).ToList();
        }

        public static List<Logger> Possibilities(Rule Rules)
        {
            options1[0] = () => Rules.ColBacking;
            options1[1] = () => Rules.ColButton;
            options1[2] = () => Rules.Counter;
            var list = new List<Logger>();
            var mix = 0;
            //var listValues = new List<string>();
            list.Add(new Logger(() => options1[0]() == options1[1](), "The color of the backing matches the color of the button"));
            list.Add(new Logger(() => options1[0]() != options1[1](), "The color of the backing does not match the color of the button"));
            list.Add(new Logger(() => options1[2]() != 0, "The Submit button contains a digit"));
            list.Add(new Logger(() => options1[2]() == 0, "The Submit button does not contain a digit"));
            for (int i = 0; i < options2.Count; i++)
            {
                var option = options2[i].ToList();
                if (option.First() == -1)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        var k = j;
                        list.Add(new Logger(() => (options1[k]() == option[1] && options1[(k + 1) % 2]() == option[2]) || (options1[k]() == option[3] && options1[(k + 1) % 2]() == option[4]), string.Format("The color of the {0} mixed with the color {1} make the {2}'s color", j == 0 ? "backing" : "button", Backgrounds.colorList[additive.Take(3).ToArray()[mix]], j == 0 ? "button" : "backing")));
                    }
                    mix++;
                    //These enumerables are special, don't do any more checks 
                    continue;
                }
                list.Add(new Logger(() => option.Contains(options1[1]()) || option.Contains(options1[0]()), string.Format("if either the button or backing {0} {1}", option.Count == 1 ? "is the color" : "is either", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2 ) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                list.Add(new Logger(() => option.Contains(options1[1]()) && !option.Contains(options1[0]()), string.Format("if the button and not the backing {0} {1}", option.Count == 1 ? "is the color" : "is either", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                list.Add(new Logger(() => !option.Contains(options1[1]()) && option.Contains(options1[0]()), string.Format("if the backing and not the button {0} {1}", option.Count == 1 ? "is the color" : "is either", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                list.Add(new Logger(() => (option.Contains(options1[1]()) && !option.Contains(options1[0]())) || (!option.Contains(options1[1]()) && option.Contains(options1[0]())), string.Format("if only either the button or the backing {0} {1}", option.Count == 1 ? "is the color" : "are the colors", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                list.Add(new Logger(() => option.Contains(options1[1]()) && option.Contains(options1[0]()), string.Format("if both the backing and the button {0} {1}", option.Count == 1 ? "are the color" : "are either", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                list.Add(new Logger(() => !option.Contains(options1[1]()) && !option.Contains(options1[0]()), string.Format("if neither the backing nor the button {0} {1}", option.Count == 1 ? "are the color" : "are either", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                for (int j = 0; j < 2; j++)
                {
                    //Delegates remember the last iterated value, so declare a new variable in the iteration
                    var k = j;
                    list.Add(new Logger(() => option.Contains(options1[k]()), string.Format("if the {0} {1} {2}", k == 0 ? "backing" : "button", option.Count == 1 ? "is the color" : "is either", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                    list.Add(new Logger(() => !option.Contains(options1[k]()), string.Format("if the {0} {1} {2}", k == 0 ? "backing" : "button", option.Count == 1 ? "is not the color" : "is neither", string.Join("", option.Select(x => ((x.Equals(option.Last()) && option.Count > 2) ? ", or " : (x.Equals(option.Last()) && option.Count == 2) ? " or " : !x.Equals(option.First()) && option.Count > 2 ? ", " : "") + Backgrounds.colorList[x]).ToArray()))));
                }
            }

            var ports = Rules.BombInfo.GetPorts();
            var batteries = Rules.BombInfo.GetBatteryCount();
            var dV9Batteries = Rules.BombInfo.GetBatteryCount(1);
            var AABatteries = Rules.BombInfo.GetBatteryAACount();
            var holders = Rules.BombInfo.GetBatteryHolderCount();
            var empty = Rules.BombInfo.GetBatteryHolderCount(0);
            var dV9Holder = Rules.BombInfo.GetBatteryHolderCount(1);
            var AAHolder = Rules.BombInfo.GetBatteryHolderCount(2);
            var AAx3Holder = Rules.BombInfo.GetBatteryHolderCount(3);
            var AAx4Holder = Rules.BombInfo.GetBatteryHolderCount(4);
            var plates = Rules.BombInfo.GetPortPlates();
            var indicators = Rules.BombInfo.GetIndicators();
            var onIndicators = Rules.BombInfo.GetOnIndicators();
            var offIndicators = Rules.BombInfo.GetOffIndicators();
            list.Add(new Logger(() => ports.Count() < 1, "There are no ports on the bomb"));
            list.Add(new Logger(() => plates.Where(x => x.Length < 1).Count() > 0, "There is an empty port plate"));
            list.Add(new Logger(() => ports.Count() == 1, "There is exactly one port on the bomb"));
            list.Add(new Logger(() => ports.Count() > 2, "There are more than two ports on the bomb"));
            list.Add(new Logger(() => plates.Count() > 1, "There is more than one port plate on the bomb"));
            list.Add(new Logger(() => batteries < 1, "There are no batteries on the bomb"));
            list.Add(new Logger(() => batteries % 2 == 1, "There are an odd number of batteries on the bomb"));
            list.Add(new Logger(() => batteries % 2 == 0, "There are an even number of batteries on the bomb"));
            list.Add(new Logger(() => dV9Batteries < 1, "There are no D batteries present on the bomb"));
            list.Add(new Logger(() => dV9Batteries > 0, "There is one D battery present on the bomb"));
            list.Add(new Logger(() => dV9Batteries > 1, "There is more than one D battery present on the bomb"));
            list.Add(new Logger(() => AABatteries < 1, "There are no AA batteries present on the bomb"));
            list.Add(new Logger(() => AABatteries > 1, "There are two AA batteries present on the bomb"));
            list.Add(new Logger(() => AABatteries == 3, "There are exactly three AA batteries present on the bomb"));
            list.Add(new Logger(() => AABatteries > 3, "There are four AA batteries present on the bomb"));
            list.Add(new Logger(() => AABatteries > 4, "There are more than four AA batteries present on the bomb"));
            list.Add(new Logger(() => holders < 1, "There are no battery holders on the bomb"));
            list.Add(new Logger(() => holders == 1, "There is exactly one battery holder on the bomb"));
            list.Add(new Logger(() => holders > 1, "There is more than one battery holder on the bomb"));
            list.Add(new Logger(() => empty > 0, "There is an empty battery holder on the bomb"));
            list.Add(new Logger(() => AAHolder > 0, "There is a battery holder with exactly two batteries on the bomb"));
            list.Add(new Logger(() => AAx3Holder > 0, "There is a battery holder with exactly three batteries on the bomb"));
            list.Add(new Logger(() => dV9Holder + AAx3Holder > 1, "There are multiple battery holders with an odd number of batteries on the bomb"));
            list.Add(new Logger(() => AAx4Holder > 0, "There is a battery holder with exactly four batteries on the bomb"));
            list.Add(new Logger(() => empty + AAHolder + AAx4Holder > 1, "There are multiple battery holders with an even number of batteries on the bomb"));
            list.Add(new Logger(() => indicators.Count() < 1, "There are no indicators on the bomb"));
            list.Add(new Logger(() => indicators.Count() == 1, "There is exactly one indicator on the bomb"));
            list.Add(new Logger(() => indicators.Count() > 1, "There is more than one indicator on the bomb"));
            list.Add(new Logger(() => onIndicators.Count() < 1, "There are no lit indicators on the bomb"));
            list.Add(new Logger(() => onIndicators.Count() == 1, "There is exactly one lit indicator on the bomb"));
            list.Add(new Logger(() => onIndicators.Count() > 1, "There is more than one lit indicator on the bomb"));
            list.Add(new Logger(() => offIndicators.Count() < 1, "There are no unlit indicators on the bomb"));
            list.Add(new Logger(() => offIndicators.Count() == 1, "There is exactly one unlit indicator on the bomb"));
            list.Add(new Logger(() => offIndicators.Count() > 1, "There is more than one unlit indicator on the bomb"));

            foreach (Func<Enum> value in options3)
            {
                if (value() is Port)
                {
                    var itemName = value().ToString();
                    list.Add(new Logger(() => ports.Contains(itemName), "There is a " + itemName + " port on the bomb"));
                    list.Add(new Logger(() => ports.Where(x => x == itemName).Count() > 1, "There is more than 1 " + itemName + " port on the bomb"));
                    list.Add(new Logger(() => ports.Where(x => x == itemName).Count() == 1, "There is exactly 1 " + itemName + " port on the bomb"));
                    list.Add(new Logger(() => ports.Where(x => x == itemName).Count() < 1, "There are no " + itemName + " ports on the bomb"));
                }
                if (value() is Indicator)
                {
                    var itemName = value().ToString();
                    list.Add(new Logger(() => indicators.Contains(itemName), "There is a " + itemName + " indicator present on the bomb"));
                    list.Add(new Logger(() => !indicators.Contains(itemName), "There is not a " + itemName + " indicator present on the bomb"));
                    list.Add(new Logger(() => onIndicators.Contains(itemName), "There is a lit " + itemName + " indicator present on the bomb"));
                    list.Add(new Logger(() => !onIndicators.Contains(itemName), "There is not a lit " + itemName + " indicator present on the bomb"));
                    list.Add(new Logger(() => offIndicators.Contains(itemName), "There is an unlit " + itemName + " indicator present on the bomb"));
                    list.Add(new Logger(() => !offIndicators.Contains(itemName), "There is not an unlit " + itemName + " indicator present on the bomb"));

                    foreach (IndicatorColor color in Enum.GetValues(typeof(IndicatorColor)))
                    {
                        if (color.GetHashCode() < 2) continue;
                        var coloredIndicator = Rules.BombInfo.GetColoredIndicators(color);
                        var colorName = Enum.GetName(typeof(IndicatorColor), color);
                        //The colors only need to be checked once
                        //It's here because I only wanted to iterate the colors in one place
                        if (!check)
                        {
                            list.Add(new Logger(() => coloredIndicator.Count() > 0, "There is a " + color.ToString() + " indicator present"));
                        }
                        var coloredIndicators = Rules.BombInfo.GetColoredIndicators(colorName, itemName);
                        list.Add(new Logger(() => coloredIndicators.Count() > 0, string.Format("There is a {0} {1} indicator present", color.ToString(), itemName)));
                    }
                    check = true;
                }
            }
            list.Add(new Logger(() => !Rules.BombInfo.IsTwoFactorPresent(), "There is not a Two Factor widget present"));
            list.Add(new Logger(() => Rules.BombInfo.GetTwoFactorCounts() == 1, "There is exactly one Two Factor widget present"));
            list.Add(new Logger(() => Rules.BombInfo.GetTwoFactorCounts() > 1, "There is more than one Two Factor widget present"));
            list.Add(new Logger(() => !Rules.BombInfo.IsManufacturePresent(), "There is not a Date of Manufacture widget present"));
            list.Add(new Logger(() => Rules.BombInfo.IsManufacturePresent(), "There is a Date of Manufacture widget present"));
            list.Add(new Logger(() => !Rules.BombInfo.IsDayOfWeekPresent(), "There is not a Day of Week widget present"));
            list.Add(new Logger(() => Rules.BombInfo.IsDayOfWeekPresent(), "There is a Day of Week widget present"));
            list.Add(new Logger(() => Rules.BombInfo.IsManufacturePresent() && Rules.BombInfo.IsDayOfWeekPresent(), "Thereis both a Date of Manufacture and Day of Week widget present"));
            list.Add(new Logger(() => !Rules.BombInfo.IsRandomTimePresent(), "There is not a Randomized Time widget present"));
            list.Add(new Logger(() => Rules.BombInfo.GetRandomTimeCount() < 2, "There are less than two Randomized Time widgets present"));
            list.Add(new Logger(() => Rules.BombInfo.GetRandomTimeCount() == 2, "There are exactly two Randomized Time widgets present"));
            list.Add(new Logger(() => Rules.BombInfo.GetRandomTimeCount() > 2, "There are more than two Randomized Time widgets present"));
            possibleCount = list.Count;
            if (!pass)
            {
                for (int i = 0; i < count; i++)
                {
                    var value = _rng.Next(0, possibleCount);
                    while (RuleIndicies.Contains(value))
                        value = _rng.Next(0, possibleCount);
                    RuleIndicies[i] = value;
                }
            }
            pass = true;
            /*var hold1 = new List<string>();
            var k = 0;
            for (int i = 0; i < options1.Count; i++)
            {
                hold1.Add(k + ": " + options1[i]());
                k++;
            }
            foreach (IEnumerable<int> vs in options2)
            {
                hold1.Add(k + ": [" + string.Join(", ", vs.Select(x => x.ToString()).ToArray()) + "]");
                k++;
            }
            for (int i = 0; i < options3.Count; i++)
            {
                hold1.Add(k + ": " + options3[i]().ToString());
                k++;
            }
            Rules.Module.DebugLog(0, string.Join("\n", hold1.ToArray()));*/
            /*var hold = string.Join("\n", list.Select(x => list.IndexOf(x) + ": " + x.Descrip).ToArray());
            //var hold = string.Join("\n", list.Select(x => x.Descrip).ToArray());
            Rules.Module.DebugLog(0, hold.Substring(0, 15900));
            Rules.Module.DebugLog(0, hold.Substring(15900, 15900));
            Rules.Module.DebugLog(0, hold.Substring(31800, 15900));
            Rules.Module.DebugLog(0, hold.Substring(47700, 15900));
            Rules.Module.DebugLog(0, hold.Substring(63600, 15900));
            Rules.Module.DebugLog(0, hold.Substring(79500));*/
            return list;
        }
    }
}
