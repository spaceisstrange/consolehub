﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consolehub.Util;
using Octokit;

namespace Consolehub.Commands
{
    class IssuesCommand : Command
    {
        public override string Name => "issues";

        /// <summary>
        /// Selected filter passed as an argument.
        /// </summary>
        private IssueFilter issuesFilter;

        /// <summary>
        /// Selected state of issues passed as an argument.
        /// </summary>
        private ItemStateFilter issuesState;

        /// <summary>
        /// Indicates the minimum date of the issues to show.
        /// </summary>
        private DateTimeOffset issuesSince = DateTimeOffset.Now.Subtract(TimeSpan.FromDays(365));

        public override Command CreateCommand(string[] args, string[] flags)
        {
            var command = new IssuesCommand();

            command.issuesFilter = IssueFilter.All;
            command.issuesState = ItemStateFilter.All;

            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "all":
                        command.issuesFilter = IssueFilter.All;
                        break;
                    case "assigned":
                        command.issuesFilter = IssueFilter.Assigned;
                        break;
                    case "created":
                        command.issuesFilter = IssueFilter.Created;
                        break;
                    case "mentioned":
                        command.issuesFilter = IssueFilter.Mentioned;
                        break;
                    case "subscribed":
                        command.issuesFilter = IssueFilter.Subscribed;
                        break;
                    default:
                        throw new ArgumentException("Unrecognized filter " + args[0]);
                }
            }

            if (args.Length > 1)
            {
                switch (args[1])
                {
                    case "open":
                        command.issuesState = ItemStateFilter.Open;
                        break;
                    case "closed":
                        command.issuesState = ItemStateFilter.Closed;
                        break;
                    default:
                        throw new ArgumentException("Unrecognized state " + args[1]);
                }
            }

            return command;
        }

        public override async Task Execute()
        {
            if (!checkUserIsLoggedIn())
            {
                return;
            }

            var options = new IssueRequest
            {
                Filter = issuesFilter,
                State = issuesState,
            };

            if (issuesSince != null)
            {
                options.Since = issuesSince;
            }

            Console.WriteLine("Getting your issues...");

            var issues = await GHClient.client.Issue.GetAllForCurrent(options);

            foreach (var issue in issues)
            {
                Ui.WriteCyan($"{issue.Repository.Name} ");
                Console.Write($"#{issue.Number} - ");
                Ui.WriteBlue(issue.Title);
                Console.WriteLine($", created by {issue.User.Name}");

                var date = issue.CreatedAt;
                Console.Write($"At {date.Day}/{date.Month}/{date.Year}, ");

                if (issue.State.StringValue == "open")
                {
                    Ui.WriteLineGreen($"open");
                }
                else
                {
                    Ui.WriteLineRed($"closed");
                }

                Console.WriteLine();
            }
        }

        public override void PrintHelp()
        {
            Ui.WriteLineBlue("issues [filter] [state] - Shows a list of all your current issues");
            Ui.WriteLineBlue("[filter]: all (default), assigned, created, mentioned and subscribed");
            Ui.WriteLineBlue("[state]: all (default), closed and open");
        }
    }
}
