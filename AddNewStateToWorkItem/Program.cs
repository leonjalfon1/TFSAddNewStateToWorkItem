using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AddNewStateToWorkItem
{
    class Program
    {
        static void Main(string[] args)
        {
            // FOR DEBUG
            // args = new string[] { @"Domain\UserName", "Password", "http://TfsUrl:8080/tfs/DefaultCollection", "(*) or (tp1,tp2,tp3)", "WorkItemName (Bug)", "NewStateName (Open)", "Transitions (from:New(defaultreason,reason1,reason2);to:Active(defaultreason,reason4))", "StateTypeInProcessConfig (In Progress)" };
            
            if (args.Length != 9)
            {
                Console.WriteLine("Run the command with the following parameters: <user> <password> <collectionUrl> <teamProjects> <workItem> <newState> <transition(from:State(defaultreason,reason...);to:State(defaultreason,reason...))> <category> <stateType(Proposed,In Progress,Completed>");
            }
            else
            {
                InitTool();
                TfsTeamProjectCollection TfsCollection = ConnectToTFS(args[0], args[1], args[2]);
                List<string> teamProjectsList = GetTeamProjectsInCollection(TfsCollection, args[3]);
                string witadminPath = @"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\witadmin.exe";

                foreach (string tp in teamProjectsList)
                {
                    string filePath = Directory.GetCurrentDirectory() + @"\Files\" + args[4] + "(" + tp + ").xml";
                    if (ExportWorkItemsXML(TfsCollection.Uri.ToString(), tp, args[4], filePath, witadminPath))
                    {
                        if (!CheckIfStateAlreadyExist(filePath, args[5]))
                        {
                            AddNewStateToWorkItemXML(filePath, args[5]);
                            AddTransitionsToWorkItemXML(filePath, args[5], args[6]);
                            if (ImportWorkItemsXML(TfsCollection.Uri.ToString(), tp, args[4], filePath, witadminPath))
                            {
                                Console.WriteLine("--> " + args[4] + " XML for TP {" + tp + "} updated successfully");
                            }
                            else
                            {
                                Console.WriteLine("--> " + args[4] + " XML for TP {" + tp + "} not updated");
                            }

                            string processConfigFilePath = Directory.GetCurrentDirectory() + @"\Files\ProcessConfig(" + tp + ").xml";
                            if (ExportProcessConfigXML(TfsCollection.Uri.ToString(), tp, processConfigFilePath, witadminPath))
                            {
                                AddNewStateToProcessConfigInSpecifiedCategory(processConfigFilePath, args[5], args[7], args[8]);
                                if (ImportProcessConfigXML(TfsCollection.Uri.ToString(), tp, processConfigFilePath, witadminPath))
                                {
                                    Console.WriteLine("--> ProcessConfig XML for TP {" + tp + "} updated successfully");
                                }
                                else
                                {
                                    Console.WriteLine("--> ProcessConfig XML for TP {" + tp + "} not updated");
                                }
                            }
                        }
                    }

                    Console.WriteLine("");
                }
            }
        }

        public static void InitTool()
        {
            Console.Clear();
            string filesFolderPath = Directory.GetCurrentDirectory() + @"\Files";

            try
            {
                if (Directory.Exists(filesFolderPath))
                {
                    Directory.Delete(filesFolderPath, true);
                }

                Directory.CreateDirectory(filesFolderPath);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
            }
        }

        public static TfsTeamProjectCollection ConnectToTFS(string user, string password, string collectionUrl)
        {
            try
            {
                NetworkCredential credentials = new NetworkCredential(user, password);
                TfsTeamProjectCollection collection = new TfsTeamProjectCollection(new Uri(collectionUrl), credentials);
                collection.EnsureAuthenticated();
                Console.WriteLine("Connection established successfully...");
                return collection;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error, Connection Failed, Exception: " + e);
                return null;
            }
        }

        public static List<string> GetTeamProjectsInCollection(TfsTeamProjectCollection collection, string teamProjects)
        {
            if(teamProjects == "*")
            {
                var workItemStore = new WorkItemStore(collection);
                return (from Project tp in workItemStore.Projects select tp.Name).ToList();
            }
            else
            {
                return teamProjects.Split(',').ToList<string>();
            }
        }

        public static bool ExportWorkItemsXML(string collectionUrl, string teamProject, string workItem, string filePath, string witadminPath)
        {
            string command = "exportwitd";
            string arguments = command + " /collection:\"" + collectionUrl + "\" /p:\"" + teamProject + "\" /n:\"" + workItem + "\" /f:\"" + filePath + "\"";

            Console.WriteLine("Downloading {" + workItem + "} XML for TP {" + teamProject + "}...");
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\witadmin.exe", arguments);
            info.CreateNoWindow = true;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
            p.WaitForExit();

            if (File.Exists(filePath))
            {
                Console.WriteLine("Download Success!");
                return true;
            }
            else
            {
                Console.WriteLine("Download Failed!");
                return false;
            }
        }

        public static void AddNewStateToWorkItemXML(string filePath, string newState)
        {
            //Load File
            XmlDocument witd = new XmlDocument();
            witd.Load(filePath);

            //Retrieve <STATES> element
            XmlNode states = witd.DocumentElement.SelectSingleNode("WORKITEMTYPE/WORKFLOW/STATES");

            //Create the new state
            XmlElement stateToAdd = witd.CreateElement("STATE");
            XmlAttribute stateAttribute = witd.CreateAttribute("value");
            stateAttribute.Value = newState;
            stateToAdd.Attributes.Append(stateAttribute);

            //Add the new State
            states.AppendChild(stateToAdd);
            witd.Save(filePath);
        }

        public static bool CheckIfStateAlreadyExist(string filePath, string newState)
        {
            //Load File
            XmlDocument witd = new XmlDocument();
            witd.Load(filePath);

            //Retrieve <STATE> elements
            foreach (XmlNode state in witd.DocumentElement.SelectSingleNode("WORKITEMTYPE/WORKFLOW/STATES").ChildNodes)
            {
                if (state.Attributes["value"].Value.ToUpper() == newState.ToUpper())
                {
                    return true;
                }
            }

            //If not found
            return false;
        }

        public static void AddTransitionsToWorkItemXML(string filePath, string newState, string transitions)
        {
            //Load File
            XmlDocument witd = new XmlDocument();
            witd.Load(filePath);

            //Retrieve <TRANSITIONS> element
            XmlNode transitionsElement = witd.DocumentElement.SelectSingleNode("WORKITEMTYPE/WORKFLOW/TRANSITIONS");

            foreach (string transition in transitions.Split(';'))
            {
                string transitionDirection = transition.Split(':')[0];
                string toState = transition.Split(':')[1].Split('(')[0];
                string transitionReasons = transition.Split(':')[1].Split('(')[1].Split(')')[0];
                List<string> reasonsList = transition.Split(':')[1].Split('(')[1].Split(')')[0].Split(',').ToList<string>();

                //Create the new transition
                XmlElement transitionToAdd = witd.CreateElement("TRANSITION");

                //Create attribute "from"
                XmlAttribute transitionAttributeFrom = witd.CreateAttribute("from");
                if (transitionDirection.ToUpper() == "FROM")
                {
                    transitionAttributeFrom.Value = toState;
                }
                else
                {
                    transitionAttributeFrom.Value = newState;
                }
                transitionToAdd.Attributes.Append(transitionAttributeFrom);

                //Create attribute "to"
                XmlAttribute transitionAttributeTo = witd.CreateAttribute("to");
                if (transitionDirection.ToUpper() == "TO")
                {
                    transitionAttributeTo.Value = toState;
                }
                else
                {
                    transitionAttributeTo.Value = newState;
                }
                transitionToAdd.Attributes.Append(transitionAttributeTo);

                //Add the new Transition
                transitionsElement.AppendChild(transitionToAdd);

                //Create and add <REASONS> element
                XmlElement reasons = witd.CreateElement("REASONS");
                transitionToAdd.AppendChild(reasons);

                //Create and add default reason element
                XmlElement defaultReasonToAdd = witd.CreateElement("DEFAULTREASON");
                XmlAttribute defaultReasonAttribute = witd.CreateAttribute("value");
                defaultReasonAttribute.Value = reasonsList.First();
                defaultReasonToAdd.Attributes.Append(defaultReasonAttribute);
                reasons.AppendChild(defaultReasonToAdd);

                //Create and add another reasons
                reasonsList.RemoveAt(0);
                foreach (string reason in reasonsList)
                {
                    XmlElement reasonToAdd = witd.CreateElement("REASON");
                    XmlAttribute reasonAttribute = witd.CreateAttribute("value");
                    reasonAttribute.Value = reason;
                    reasonToAdd.Attributes.Append(reasonAttribute);
                    reasons.AppendChild(reasonToAdd);
                }

                //Save Changes
                witd.Save(filePath);
            }
        }

        public static bool ImportWorkItemsXML(string collectionUrl, string teamProject, string workItem, string filePath, string witadminPath)
        {
            string command = "importwitd";
            string arguments = command + " /collection:\"" + collectionUrl + "\" /p:\"" + teamProject + "\" /f:\"" + filePath + "\"";

            Console.WriteLine("Uploading {" + workItem + "} XML for TP {" + teamProject + "}...");
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\witadmin.exe", arguments);
            info.CreateNoWindow = true;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                Console.WriteLine("Upload Success!");
                return true;
            }
            else
            {
                Console.WriteLine("Upload Failed!");
                return false;
            }
        }

        public static bool ExportProcessConfigXML(string collectionUrl, string teamProject, string filePath, string witadminPath)
        {
            string command = "exportprocessconfig";
            string arguments = command + " /collection:\"" + collectionUrl + "\" /p:\"" + teamProject + "\" /f:\"" + filePath + "\"";

            Console.WriteLine("Downloading ProcessConfig XML for TP {" + teamProject + "}...");
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\witadmin.exe", arguments);
            info.CreateNoWindow = true;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
            p.WaitForExit();

            if (File.Exists(filePath))
            {
                Console.WriteLine("Download Success!");
                return true;
            }
            else
            {
                Console.WriteLine("Download Failed!");
                return false;
            }
        }

        public static bool AddNewStateToProcessConfigInSpecifiedCategory(string filePath, string newState, string category, string stateType)
        {
            //Load File
            XmlDocument processConfig = new XmlDocument();
            processConfig.Load(filePath);

            //Retrieve xml elements
            foreach (XmlNode node in processConfig.DocumentElement.ChildNodes)
            {
                if (node.Attributes["category"] != null && node.Attributes["category"].Value.ToUpper() == category.ToUpper())
                {
                    XmlNode states = node.SelectSingleNode("States");
                    bool isExist = false;
                    foreach (XmlNode state in states.ChildNodes)
                    {
                        if (state.Attributes["value"].Value.ToUpper() == newState.ToUpper())
                        {
                            isExist = true;
                        }
                    }
                    if (!isExist)
                    {
                        //Create the new transition
                        XmlElement stateToAdd = processConfig.CreateElement("State");

                        //Create attribute "type"
                        XmlAttribute attributeType = processConfig.CreateAttribute("type");
                        attributeType.Value = stateType;
                        stateToAdd.Attributes.Append(attributeType);

                        //Create attribute "value"
                        XmlAttribute attributeValue = processConfig.CreateAttribute("value");
                        attributeValue.Value = newState;
                        stateToAdd.Attributes.Append(attributeValue);

                        //Add the new state
                        states.AppendChild(stateToAdd);
                        processConfig.Save(filePath);
                    }
                    return true;
                }

                if (node.Name == "PortfolioBacklogs")
                {
                    foreach (XmlNode subNode in node.ChildNodes)
                    {
                        if (subNode.Attributes["category"] != null && subNode.Attributes["category"].Value.ToUpper() == category.ToUpper())
                        {
                            XmlNode states = subNode.SelectSingleNode("States");
                            bool isExist = false;
                            foreach (XmlNode state in states.ChildNodes)
                            {
                                if (state.Attributes["value"].Value.ToUpper() == newState.ToUpper())
                                {
                                    isExist = true;
                                }
                            }
                            if (!isExist)
                            {
                                //Create the new transition
                                XmlElement stateToAdd = processConfig.CreateElement("State");

                                //Create attribute "type"
                                XmlAttribute attributeType = processConfig.CreateAttribute("type");
                                attributeType.Value = stateType;
                                stateToAdd.Attributes.Append(attributeType);

                                //Create attribute "value"
                                XmlAttribute attributeValue = processConfig.CreateAttribute("value");
                                attributeValue.Value = newState;
                                stateToAdd.Attributes.Append(attributeValue);

                                //Add the new state
                                states.AppendChild(stateToAdd);
                                processConfig.Save(filePath);
                            }
                            return true;
                        }
                    }
                }
            }

            //If category not found
            return false;
        }

        public static bool ImportProcessConfigXML(string collectionUrl, string teamProject, string filePath, string witadminPath)
        {
            string command = "importprocessconfig";
            string arguments = command + " /collection:\"" + collectionUrl + "\" /p:\"" + teamProject + "\" /f:\"" + filePath + "\"";

            Console.WriteLine("Uploading ProcessConfig XML for TP {" + teamProject + "}...");
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(@"C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\witadmin.exe", arguments);
            info.CreateNoWindow = true;
            info.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(info);
            p.WaitForExit();

            if (p.ExitCode == 0)
            {
                Console.WriteLine("Upload Success!");
                return true;
            }
            else
            {
                Console.WriteLine("Upload Failed!");
                return false;
            }
        }
    }
}
