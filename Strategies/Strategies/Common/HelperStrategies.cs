using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StockSharp.Algo.Strategies;
using Strategies.Settings;

namespace Strategies.Common
{
    public static class HelperStrategies
    {
        public static List<Type> GetStrategies()
        {
            var baseType = typeof(Strategy);
            //var baseTypeStrategy = typeof(Strategy);
            Assembly assembly = Assembly.LoadFrom(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
            return assembly.GetTypes().Where(type => type.IsSubclassOf(baseType)).ToList();
        }

        public static bool StrategyHasParams(string strategyName)
        {
            Type type = RobotsNameSpace(strategyName);
            if (typeof(IOptionalSettings).IsAssignableFrom(type))
                return true;
            return false;
        }

        public static Type GetRegistredStrategies(string strategyName)
        {
            var baseType = typeof(Strategy);
            Assembly assembly = Assembly.LoadFrom(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
            return assembly.GetTypes().FirstOrDefault(type => type.IsSubclassOf(baseType) && type.Name == strategyName);
        }

        public static Type GetStrategySettingsType(string strategyName)
        {
            Assembly assembly = Assembly.LoadFrom(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
            return assembly.GetTypes().FirstOrDefault(type => !type.Name.Contains("Default") && type.Name.StartsWith(strategyName) && type.Name.Contains("Settings"));
        }

        private static Type RobotsNameSpace(string strategyName)
        {
            Assembly assembly = Assembly.LoadFrom(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
            return assembly.GetTypes().FirstOrDefault(type => type.Name == strategyName);
        }

        public static string GetStrategyFriendlyName(string strategyName, SerializableDictionary<string, object> settingsStorage)
        {
            Type type = RobotsNameSpace(strategyName);
            Strategy instance;
            if (settingsStorage != null)
            {
                instance = (Strategy)Activator.CreateInstance(type, settingsStorage, "single",new List<long> {0});
            }
            else
            {
                instance = (Strategy)Activator.CreateInstance(type, "single");
            }
            return instance.Name;
        }
        public static Type GetRegistredStrategiesTest(string strategyName)
        {
            var baseType = typeof(Strategy);
            Assembly assembly = Assembly.LoadFrom(Assembly.GetExecutingAssembly().GetName().Name + ".dll");
            return assembly.GetTypes().FirstOrDefault(type => type.IsSubclassOf(baseType) && type.Name.StartsWith(strategyName));
        }

    }
}
