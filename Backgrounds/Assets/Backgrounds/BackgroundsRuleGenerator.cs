using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KModkit;

namespace RuleGenerator
{
    public static class BackgroundsRuleGenerator
    {
        //Instance of Module
        public static Backgrounds Module;
        //Instance of RuleSeed
        private static MonoRandom _rng;
        //The number of rules Backgrounds uses
        private const int count = 9;
        internal static int ColBacking, ColButton, Counter, listCount;
        //The original idea was to compare the last digit in submit to screen
        //But since the module always starts with digit 0, this was deemed unnecessary
        //They're only still here because of the hardcoded randomized values
        internal static UnityEngine.TextMesh SubmitButton, Screen;
        //Rules to be chosen, primary colors to be chosen
        private static int[] RuleIndicies = new int[count], additive,
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
        private static List<string> test = new List<string>();
        internal static KMBombInfo BombInfo
        {
            get
            {
                return Module.BombInfo;
            }
        }

        private static List<object> options1 = new List<object>();

        private static List<bool> Swaps(List<bool> list)
        {
            var values = new[] { 323, 1485, 393, 1413, 566, 1174, 940, 1306, 555 };
            var newValues = new[] { 0, 370, 1320, 1326, 8, 1039, 1400, 1360, 20 };
            for (int i = 0; i < values.Count(); i++)
            {
                var hold = list[values[i]];
                list[values[i]] = list[newValues[i]];
                list[newValues[i]] = hold;
            }
            return list;
        }

        private static void DetermineColorCombinations()
        {
            if (additive.Take(3).ToArray().SequenceEqual(new[] { 0, 2, 4 }))
            {
                //red
                options1.Add(new[] { -1, 2, 1, 4, 5 });
                //yellow
                options1.Add(new[] { -1, 0, 1, 4, 3 });
                //blue
                options1.Add(new[] { -1, 0, 5, 3, 4 });
            }
            else
            {
                //red
                options1.Add(new[] { -1, 3, 2, 4, 5 });
                //green
                options1.Add(new[] { -1, 0, 2, 4, 1 });
                //blue
                options1.Add(new[] { -1, 0, 5, 3, 1 });
            }
        }
        
        private static void AddToOptions()
        {
            options1.Add(ColBacking = UnityEngine.Random.Range(0, 8));
            options1.Add(ColButton = UnityEngine.Random.Range(0, 9));
            options1.Add(SubmitButton = Module.Submit.GetComponentInChildren<UnityEngine.TextMesh>());
            options1.Add(Screen = Module.CounterText);
            additive = Primaries().ToArray();
            options1.Add(additive.Take(3).ToList());
            options1.Add(additive.Skip(3).ToList());
            DetermineColorCombinations();
            foreach (Battery battery in Enum.GetValues(typeof(Battery)))
            {
                if (battery.GetHashCode() > 0) options1.Add(battery);
            }
            foreach(Port port in Enum.GetValues(typeof(Port)))
            {
                options1.Add(port);
            }
            var indicatorValues = Enum.GetValues(typeof(Indicator));
            foreach(Indicator indicator in indicatorValues)
            {
                if (indicator.GetHashCode() < indicatorValues.Length - 1) options1.Add(indicator);
            }

            var listSize = 1;
            while (listSize < 4)
            {
                var orderedArray = new [] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
                var intList = new List<List<int>>();
                foreach (int i in orderedArray)
                {
                    intList.Add(new List<int>());
                    intList.Last().Add(i);
                    if (listSize > 1)
                    {
                        var list = orderedArray.Skip(i + 1);
                        foreach (int j in list)
                        {
                            intList.Last().Add(j);
                            if (listSize > 2)
                            {
                                var list2 = orderedArray.Skip(j + 1);
                                foreach (int k in list2)
                                {
                                    intList.Last().Add(k);
                                    intList.Add(new List<int>());
                                    intList.Last().Add(i);
                                    intList.Last().Add(j);
                                }
                                intList.Remove(intList.Last());
                            }
                            intList.Add(new List<int>());
                            intList.Last().Add(i);
                        }
                        intList.Remove(intList.Last());
                    }
                }
                foreach (List<int> item in intList)
                {
                    if (!options1.Any(x => (x as List<int>) != null && (x as List<int>).SequenceEqual(item))) options1.Add(item);
                }
                listSize++;
            }
        }

        public static List<bool> Rules(MonoRandom rng)
        {
            if (_rng != null && _rng.Seed == rng.Seed) return null;
            _rng = rng;
            var list = new List<bool> { true };
            Counter = 0;
            if (_rng.Seed != 1)
            {
                Counter = UnityEngine.Random.Range(0, 2);
                _rng.ShuffleFisherYates(coordX);
                _rng.ShuffleFisherYates(coordY);
                _rng.ShuffleFisherYates(ManualTable);
            }
            Module.coordX = coordX;
            Module.coordY = coordY;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Module.BGManualTable[i, j] = ManualTable[6 * i + j];
                }
            }
            return Possibilities().Concat(list).ToList();
        }

        public static List<int> Primaries()
        {
            var i = _rng.Next(0, 2);
            var list1 = new List<int> { 0, 2, 4 };
            var list2 = new List<int> { 0, 3, 4 };
            var list3 = new List<int> { 1, 3, 5 };
            var list4 = new List<int> { 1, 2, 5 };
            if (i == 1)
            {
                Module.color[1] = UnityEngine.Color.cyan;
                Module.colorList[1] = "cyan";
                Module.color[5] = UnityEngine.Color.magenta;
                Module.colorList[5] = "magenta";
                return list2.Concat(list4).ToList();
            }
            return list1.Concat(list3).ToList();
        }

        public static List<bool> DetermineIntMatches(int item, int index)
        {
            var list = new List<bool>();
            for (int i = 0; i < options1.Count; i++)
            {
                if (index == i) continue;
                if (options1[i] is int && index == 0)
                {
                    list.Add(item == (int)options1[i]);
                    test.Add(listCount + ": " + item + " equals " + (int)options1[i]);
                    list.Add(item != (int)options1[i]);
                    listCount++;
                    test.Add(listCount + ": " + item + " does not equal " + (int)options1[i]);
                    listCount++;
                }
                if (options1[i] is IEnumerable<int>)
                {
                    var option = (options1[i] as IEnumerable<int>).ToList();
                    if (option.First() == -1)
                    {
                        var item2 = index == 0 ? (int)options1[0] : (int)options1[1];
                        list.Add((item == option[1] && item2 == option[2]) || (item == option[3] && item2 == option[4]));
                        test.Add(string.Format("{6}: {0} equals {1} or {2} while {3} equals {4} or {5}", item, option[1], option[3], item2, option[2], option[4], listCount));
                        listCount++;
                        continue;
                    }
                    list.Add(option.Contains(item));
                    test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains " + item);
                    listCount++;
                    list.Add(!option.Contains(item));
                    test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] does not contain " + item);
                    listCount++;
                    if (index == 0)
                    {
                        list.Add(option.Contains((int)options1[1]) || option.Contains(item));
                        test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains either " + (int)options1[1] + " or " + item);
                        listCount++;
                        list.Add(option.Contains((int)options1[1]) && !option.Contains(item));
                        test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains " + (int)options1[1] + " but not " + item);
                        listCount++;
                        list.Add(!option.Contains((int)options1[1]) && option.Contains(item));
                        test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains " + item + " but not " + (int)options1[1]);
                        listCount++;
                        list.Add((option.Contains((int)options1[1]) && !option.Contains(item)) || (!option.Contains((int)options1[1]) && option.Contains(item)));
                        test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains either " + (int)options1[1] + " and not " + item + " or vice versa");
                        listCount++;
                        list.Add(option.Contains((int)options1[1]) && option.Contains(item));
                        test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains both " + (int)options1[1] + " and " + item);
                        listCount++;
                        list.Add(!option.Contains((int)options1[1]) && !option.Contains(item));
                        test.Add(listCount + "-" + i + ": [" + string.Join(", ", option.Select(x => x.ToString()).ToArray()) + "] contains neither " + (int)options1[1] + " nor " + item);
                        listCount++;
                    }
                }
            }
            return list;
        }

        public static List<bool> DetermineWidgets(object item, IEnumerable<string> ports, IEnumerable<string> indicators, IEnumerable<string> onIndicators, IEnumerable<string> offIndicators)
        {
            var list = new List<bool>();
            if (!check)
            {
                var batteries = BombInfo.GetBatteryCount();
                var holders = BombInfo.GetBatteryHolderCount();
                var plates = BombInfo.GetPortPlates();
                list.Add(ports.Count() < 1);
                test.Add(listCount + ": There's more than one port");
                listCount++;
                list.Add(plates.Where(x => x.Length < 1).Count() > 0);
                test.Add(listCount + ": There's an empty port plate");
                listCount++;
                list.Add(ports.Count() == 1);
                test.Add(listCount + ": There is exactly one port");
                listCount++;
                list.Add(ports.Count() > 2);
                test.Add(listCount + ": There is more than one port");
                listCount++;
                list.Add(plates.Count() > 1);
                test.Add(listCount + ": There are more than two port plates");
                listCount++;
                list.Add(batteries < 1);
                test.Add(listCount + ": There are no batteries");
                listCount++;
                list.Add(batteries % 2 == 1);
                test.Add(listCount + ": There is more than one battery");
                listCount++;
                list.Add(batteries % 2 == 0);
                test.Add(listCount + ": There are more than two batteries");
                listCount++;
                list.Add(holders < 1);
                test.Add(listCount + ": There are no battery holders (or batteries)");
                listCount++;
                list.Add(holders == 1);
                test.Add(listCount + ": There is one battery holder");
                listCount++;
                list.Add(holders > 1);
                test.Add(listCount + ": There is more than one battery holder");
                listCount++;
                list.Add(indicators.Count() < 1);
                test.Add(listCount + ": There are no indicators");
                listCount++;
                list.Add(indicators.Count() == 1);
                test.Add(listCount + ": There is exactly one indicator");
                listCount++;
                list.Add(indicators.Count() > 1);
                test.Add(listCount + ": There is more than one indicator");
                listCount++;
                list.Add(onIndicators.Count() < 1);
                test.Add(listCount + ": There are no lit indicators");
                listCount++;
                list.Add(onIndicators.Count() == 1);
                test.Add(listCount + ": There is exactly one lit indicator");
                listCount++;
                list.Add(onIndicators.Count() > 1);
                test.Add(listCount + ": There is more than one lit indicator");
                listCount++;
                list.Add(offIndicators.Count() < 1);
                test.Add(listCount + ": There are no unlit indicators");
                listCount++;
                list.Add(offIndicators.Count() == 1);
                test.Add(listCount + ": There is exactly one unlit indicator");
                listCount++;
                list.Add(offIndicators.Count() > 1);
                test.Add(listCount + ": There is more than one unlit indicator");
                listCount++;
                check = true;
            }
            if (item is Port)
            {
                var itemName = ((Port)item).ToString();
                list.Add(ports.Contains(itemName));
                test.Add(listCount + ": The bomb contains a " + itemName + " port");
                listCount++;
                list.Add(ports.Where(x => x == itemName).Count() > 1);
                test.Add(listCount + ": There are more than 1 " + itemName + " port");
                listCount++;
                list.Add(ports.Where(x => x == itemName).Count() == 1);
                test.Add(listCount + ": There is exactly 1 " + itemName + " port");
                listCount++;
                list.Add(ports.Where(x => x == itemName).Count() < 1);
                test.Add(listCount + ": There are no " + itemName + " ports on the bomb");
                listCount++;
            }
            if (item is Battery)
            {
                var batteries = BombInfo.GetBatteryCount((Battery)item);
                var holders = BombInfo.GetBatteryHolderCount((Battery)item);
                var batteryName = Enum.GetName(typeof(Battery), item);
                //Always true atm
                if (item.GetHashCode() != 0)
                {
                    list.Add(batteries < 1);
                    test.Add(listCount + ": There are no " + batteryName + " batteries on the bomb");
                    listCount++;
                    list.Add(batteries == 1);
                    test.Add(listCount + ": There is exactly 1 " + batteryName + " battery on the bomb");
                    listCount++;
                    list.Add(batteries > 1);
                    test.Add(listCount + ": There is more than 1 " + batteryName + " battery on the bomb");
                    listCount++;
                }
                list.Add(holders < 1);
                test.Add(listCount + ": There are no battery holders of type " + batteryName + " on the bomb");
                listCount++;
                list.Add(holders == 1);
                test.Add(listCount + ": There is exactly one battery holder of type " + batteryName + " on the bomb");
                listCount++;
                list.Add(holders > 1);
                test.Add(listCount + ": There is more than one battery holder of type " + batteryName + " on the bomb");
                listCount++;
            }
            if (item is Indicator)
            {
                var itemName = ((Indicator)item).ToString();
                list.Add(indicators.Contains(itemName));
                test.Add(listCount + ": There is an " + itemName + " indicator present");
                listCount++;
                list.Add(!indicators.Contains(itemName));
                test.Add(listCount + ": There is not an " + itemName + " indicator present");
                listCount++;
                list.Add(onIndicators.Contains(itemName));
                test.Add(listCount + ": There is a lit " + itemName + " present");
                listCount++;
                list.Add(!onIndicators.Contains(itemName));
                test.Add(listCount + ": There is not a lit " + itemName + " present");
                listCount++;
                list.Add(offIndicators.Contains(itemName));
                test.Add(listCount + ": There is an unlit " + itemName + " present");
                listCount++;
                list.Add(!offIndicators.Contains(itemName));
                test.Add(listCount + ": There is not an unlit " + itemName + " present");
                listCount++;
                foreach (IndicatorColor color in Enum.GetValues(typeof(IndicatorColor)))
                {
                    if (color.GetHashCode() < 2) continue;
                    var coloredIndicator = BombInfo.GetColoredIndicators(color);
                    var colorName = Enum.GetName(typeof(IndicatorColor), color);
                    if (!check2)
                    {
                        list.Add(coloredIndicator.Count() > 0);
                        test.Add(listCount + ": There is an indicator of color " + colorName + " present");
                        listCount++;
                    }
                    var coloredIndicators = BombInfo.GetColoredIndicators(colorName, itemName);
                    list.Add(coloredIndicators.Count() > 0);
                    test.Add(listCount + ": There is a " + colorName + " " + itemName + " indicator present");
                    listCount++;
                }
                check2 = true;
            }
            return list;
        }

        public static List<bool> Possibilities()
        {
            var list = new List<bool>();
            AddToOptions();
            var ports = BombInfo.GetPorts();
            var indicators = BombInfo.GetIndicators();
            var onIndicators = BombInfo.GetOnIndicators();
            var offIndicators = BombInfo.GetOffIndicators();

            for (int i = 0; i < options1.Count; i++)
            {
                object item = options1[i];
                var type = new[] { item is int, item is UnityEngine.TextMesh,
                item is Port || item is Indicator || item is Battery};
                switch (Array.IndexOf(type, true))
                {
                    case 0:
                        list = list.Concat(DetermineIntMatches((int)item, i)).ToList();
                        break;
                    case 1:
                        list.Add(Counter != i % 2);
                        test.Add("Counter is " + Counter);
                        listCount++;
                        break;
                    case 2:
                        list = list.Concat(DetermineWidgets(item, ports, indicators, onIndicators, offIndicators)).ToList();
                        break;
                }
            }
            int y = 0;
            Module.DebugLog(string.Join("\n", list.Take(1494).Select(x => y++.ToString() + ": " + x.ToString()).ToArray()));
            Module.DebugLog(string.Join("\n", list.Skip(1494).Select(x => y++.ToString() + ": " + x.ToString()).ToArray()));
            Module.DebugLog(string.Join("\n", test.Take(390).ToArray()));
            Module.DebugLog(string.Join("\n", test.Skip(390).Take(350).ToArray()));
            Module.DebugLog(string.Join("\n", test.Skip(740).Take(350).ToArray()));
            Module.DebugLog(string.Join("\n", test.Skip(1090).Take(400).ToArray()));
            Module.DebugLog(string.Join("\n", test.Skip(1490).ToArray()));
            var newList = new List<bool>();
            list = Swaps(list);
            for (int i = 0; i < count; i++)
            {
                RuleIndicies[i] = _rng.Next(0, list.Count);
                newList.Add(list[RuleIndicies[i]]);
            }
            if ((RuleIndicies.Contains(1298) || RuleIndicies.Contains(1299)) && Counter == 1) SubmitButton.text = SubmitButton.text + " 0";
            Module.DebugLog(string.Join(", ", RuleIndicies.Select(x => x.ToString()).ToArray()));
            return newList;
        }
    }
}
