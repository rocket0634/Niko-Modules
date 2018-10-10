using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        internal static List<Expression<Func<bool>>> Possibilities;
        internal MonoRandom rng;
        public List<bool> Rules()
        {
            var list = new List<bool>();
            rng = Module.RuleSeed.GetRNG();
            ColBacking = UnityEngine.Random.Range(0, 8);
            ColButton = UnityEngine.Random.Range(0, 9);
            if (!Generator.pass) Generator.Rules(this);
            if (rng.Seed != 1) Counter = UnityEngine.Random.Range(0, 2);
            Module.coordX = coordX;
            Module.coordY = coordY;
            Module.BGManualTable = ManualTable;
            Possibilities = Generator.Swaps(Generator.Possibilities(this));
            //Rules that check if the submit button contains a digit
            var Submit = Module.Submit.GetComponentInChildren<UnityEngine.TextMesh>();
            if ((Generator.RuleIndicies.Contains(2) || Generator.RuleIndicies.Contains(3)) && Counter == 1) Submit.text = Submit.text + " 0";
            foreach (int index in Generator.RuleIndicies)
            {
                list.Add(Possibilities[index].Compile()());
            }
            return list.Concat(new[] { true }).ToList();
        }
    }
    internal static class Generator
    {
        internal static bool pass = false;
        //Instance of RuleSeed
        private static MonoRandom _rng;
        //The number of rules Backgrounds uses
        private const int count = 9;
        //Rules to be chosen, primary colors to be chosen
        public static int[] RuleIndicies = new int[count], additive,
            //Original manual values to be randomized
            //May look into doing the randomization from scratch in the future
            coordX = { 0, 3, 2, 3, 1, 5, 4, 1, 2, 4 },
            coordY = { 2, 1, 4, 3, 5, 4, 1, 2, 3, 0 },
            ManualTable = {
                3, 2, 9, 1, 7, 4,
                7, 9, 8, 8, 2, 3,
                5, 1, 7, 4, 4, 6,
                6, 4, 2, 6, 8, 5,
                5, 1, 5, 3, 9, 9,
                1, 2, 3, 6, 7, 8
            };
        private static bool check, check2;
        private static int possibleCount;

        //Individual items that will be used for comparisons in Possibilities
        internal static List<Func<int>> options1 = new List<Func<int>>();
        internal static List<IEnumerable<int>> options2 = new List<IEnumerable<int>>();
        internal static List<Func<Enum>> options3 = new List<Func<Enum>>();
        internal static List<Expression<Func<bool>>> possibilities = new List<Expression<Func<bool>>>();

        //Swap randomized values for seed 1 to output the original manual
        internal static List<Expression<Func<bool>>> Swaps(List<Expression<Func<bool>>> list)
        {
            var values = new[] { 323, 1485, 393, 1413, 566, 1174, 940, 1306, 555 };
            var newValues = new[] { 0, 463, 1320, 1326, 8, 22, 1400, 1360, 28 };
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
                options2.Add(new[] { -1, 0, 5, 3, 4 });
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
            //Battery types
            foreach (Battery battery in Enum.GetValues(typeof(Battery)))
            {
                if (battery.GetHashCode() > 0) options3.Add(() => battery);
            }
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
            //This shouldn't be reached? I saw it in Morse-A-Maze's code
            if (_rng != null && _rng.Seed == Rules.rng.Seed) return;
            _rng = Rules.rng;
            AddToOptions(Rules);
            //The Counter should not be changed for the original seed.
            if (_rng.Seed != 1)
            {
                _rng.ShuffleFisherYates(coordX);
                _rng.ShuffleFisherYates(coordY);
                _rng.ShuffleFisherYates(ManualTable);
            }
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
            return list1.Concat(list3).ToList();
        }

        public static List<Expression<Func<bool>>> Possibilities(Rule Rules)
        {
            options1[0] = () => Rules.ColBacking;
            options1[1] = () => Rules.ColButton;
            options1[2] = () => Rules.Counter;
            var list = new List<Expression<Func<bool>>>();
            list.Add(() => options1[0]() == options1[1]());
            list.Add(() => options1[0]() != options1[1]());
            list.Add(() => options1[2]() != 0);
            list.Add(() => options1[2]() == 0);
            for (int i = 0; i < options2.Count; i++)
            {
                var option = options2[i].ToList();
                if (option.First() == -1)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        list.Add(() => (options1[j]() == option[1] && options1[(j + 1) % 2]() == option[2]) || (options1[j]() == option[3] && options1[(j + 1) % 2]() == option[4]));
                    }
                    //These enumerables are special, don't do any more checks 
                    continue;
                }
                list.Add(() => option.Contains(options1[1]()) || option.Contains(options1[0]()));
                list.Add(() => option.Contains(options1[1]()) && !option.Contains(options1[0]()));
                list.Add(() => !option.Contains(options1[1]()) && option.Contains(options1[0]()));
                list.Add(() => (option.Contains(options1[1]()) && !option.Contains(options1[0]())) || (!option.Contains(options1[1]()) && option.Contains(options1[0]())));
                list.Add(() => option.Contains(options1[1]()) && option.Contains(options1[0]()));
                list.Add(() => !option.Contains(options1[1]()) && !option.Contains(options1[0]()));
                for (int j = 0; j < 2; j++)
                {
                    list.Add(() => option.Contains(options1[j]()));
                    list.Add(() => !option.Contains(options1[j]()));
                }
            }

            var ports = Rules.BombInfo.GetPorts();
            var batteries = Rules.BombInfo.GetBatteryCount();
            var holders = Rules.BombInfo.GetBatteryHolderCount();
            var plates = Rules.BombInfo.GetPortPlates();
            var indicators = Rules.BombInfo.GetIndicators();
            var onIndicators = Rules.BombInfo.GetOnIndicators();
            var offIndicators = Rules.BombInfo.GetOffIndicators();
            list.Add(() => ports.Count() < 1);
            list.Add(() => plates.Where(x => x.Length < 1).Count() > 0);
            list.Add(() => ports.Count() == 1);
            list.Add(() => ports.Count() > 2);
            list.Add(() => plates.Count() > 1);
            list.Add(() => batteries < 1);
            list.Add(() => batteries % 2 == 1);
            list.Add(() => batteries % 2 == 0);
            list.Add(() => holders < 1);
            list.Add(() => holders == 1);
            list.Add(() => holders > 1);
            list.Add(() => indicators.Count() < 1);
            list.Add(() => indicators.Count() == 1);
            list.Add(() => indicators.Count() > 1);
            list.Add(() => onIndicators.Count() < 1);
            list.Add(() => onIndicators.Count() == 1);
            list.Add(() => onIndicators.Count() > 1);
            list.Add(() => offIndicators.Count() < 1);
            list.Add(() => offIndicators.Count() == 1);
            list.Add(() => offIndicators.Count() > 1);

            foreach (Func<Enum> value in options3)
            {
                if (value() is Battery)
                {
                    batteries = Rules.BombInfo.GetBatteryCount((Battery)value());
                    holders = Rules.BombInfo.GetBatteryHolderCount((Battery)value());
                    //Always true atm
                    if (value().GetHashCode() != 0)
                    {
                        list.Add(() => batteries < 1);
                        list.Add(() => batteries == 1);
                        list.Add(() => batteries > 1);
                    }
                    list.Add(() => holders < 1);
                    list.Add(() => holders == 1);
                    list.Add(() => holders > 1);
                }
                if (value() is Port)
                {
                    var itemName = value().ToString();
                    list.Add(() => ports.Contains(itemName));
                    list.Add(() => ports.Where(x => x == itemName).Count() > 1);
                    list.Add(() => ports.Where(x => x == itemName).Count() == 1);
                    list.Add(() => ports.Where(x => x == itemName).Count() < 1);
                }
                if (value() is Indicator)
                {
                    var itemName = value().ToString();
                    list.Add(() => indicators.Contains(itemName));
                    list.Add(() => !indicators.Contains(itemName));
                    list.Add(() => onIndicators.Contains(itemName));
                    list.Add(() => !onIndicators.Contains(itemName));
                    list.Add(() => offIndicators.Contains(itemName));
                    list.Add(() => !offIndicators.Contains(itemName));
                    foreach (IndicatorColor color in Enum.GetValues(typeof(IndicatorColor)))
                    {
                        if (color.GetHashCode() < 2) continue;
                        var coloredIndicator = Rules.BombInfo.GetColoredIndicators(color);
                        var colorName = Enum.GetName(typeof(IndicatorColor), color);
                        //The colors only need to be checked once
                        //It's here because I only wanted to iterate the colors in one place
                        if (!check2)
                        {
                            list.Add(() => coloredIndicator.Count() > 0);
                        }
                        var coloredIndicators = Rules.BombInfo.GetColoredIndicators(colorName, itemName);
                        list.Add(() => coloredIndicators.Count() > 0);
                    }
                    check2 = true;
                }
            }
            possibleCount = list.Count;
            if (!pass)
            {
                for (int i = 0; i < count; i++)
                    RuleIndicies[i] = _rng.Next(0, possibleCount);
            }
            pass = true;
            return list;
        }
    }
}
