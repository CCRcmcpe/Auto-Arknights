using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using REVUnit.AutoArknights.CLI.Properties;
using REVUnit.AutoArknights.Core;
using REVUnit.AutoArknights.Core.Tasks;

namespace REVUnit.AutoArknights.CLI
{
    public class Plan
    {
        public Plan(IArkTask[] tasks) => Tasks = tasks;
        public IArkTask[] Tasks { get; }

        public static Plan Parse(string value, ISettings parseSettings)
        {
            IArkTask[] tasks = new TasksParser(parseSettings).Parse(value);
            return new Plan(tasks);
        }

        private class TasksParser
        {
            private readonly Regex _regex =
                new(@"(?<mode>\d)(?<times>\d+)?(?<post_actions>\w+)?", RegexOptions.Compiled);

            public TasksParser(ISettings parseSettings) => ParseSettings = parseSettings;

            public ISettings ParseSettings { get; }

            public IArkTask[] Parse(string value)
            {
                Match match = _regex.Match(value);
                if (!match.Success)
                {
                    throw new FormatException(Resources.Plan_Exception_Parsing);
                }

                var tasks = new List<IArkTask>();

                LevelFarming mainTask = ParseMainTask(match.Groups["mode"], match.Groups["times"]);
                IEnumerable<PostAction> postActions = ParsePostActions(match.Groups["post_actions"]);

                tasks.Add(mainTask);
                tasks.AddRange(postActions);

                return tasks.ToArray();
            }

            private static LevelFarming ParseMainTask(Group modeGroup, Group timesGroup)
            {
                var mode = (LevelFarming.Mode) int.Parse(modeGroup.Value);
                if (!Enum.IsDefined(mode)) throw new ArgumentException(Resources.Plan_Exception_InvalidMode);

                if (mode != LevelFarming.Mode.SpecifiedTimes && mode != LevelFarming.Mode.SpecTimesWithWait)
                {
                    return new LevelFarming(mode, -1);
                }

                if (!timesGroup.Success)
                {
                    throw new FormatException(Resources.Plan_Exception_InvalidTimes);
                }

                int repeatTimes = int.Parse(timesGroup.Value);
                return new LevelFarming(mode, repeatTimes);
            }

            private IEnumerable<PostAction> ParsePostActions(Group postActionsGroup)
            {
                return postActionsGroup.Value.Select(c => PostAction.Parse(c, ParseSettings));
            }
        }
    }
}