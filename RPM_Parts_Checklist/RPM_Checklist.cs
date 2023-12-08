using UnityEngine;

namespace RPM_Parts_Checklist
{
    class ChecklistItem
    {
        public bool completed = false;
        public string text;

        public ChecklistItem(string text)
        {
            this.text = text;
        }
    }
    public class RPM_Checklist : InternalModule
    {
        [KSPField]
        public string pageTitle = "============== Checklist ===============";
        [KSPField]
        public int buttonUp = 0;
        [KSPField]
        public int buttonDown = 1;
        [KSPField]
        public int buttonEnter = 2;
        [KSPField]
        public int buttonEsc = 3;
        [KSPField]
        public int buttonHome = 4;
        [KSPField]
        public int buttonRight = 5;
        [KSPField]
        public int buttonLeft = 6;
        [KSPField]
        public int buttonNext = 7;
        [KSPField]
        public int buttonPrev = 8;

        Router router;
        public RPM_Checklist()
        {
            router = new("home");

            Checklist_HomeView homeView = new Checklist_HomeView(pageTitle, router);
            router.pages.Add(new Page("home", homeView.Display, homeView.ButtonProcessor));

            Checklist_HelpView helpView = new Checklist_HelpView(pageTitle);
            router.pages.Add(new Page("help", helpView.Display, helpView.ButtonProcessor));

            Checklist_ChecklistView checklistView = new Checklist_ChecklistView(pageTitle);
            router.pages.Add(new Page("checklist", checklistView.Display, checklistView.ButtonProcessor));
        }

        public string Display(int screenWidth, int screenHeight)
        {
            return router.Display(screenWidth, screenHeight);
        }

        public void ButtonProcessor(int button)
        {
            router.ButtonProcessor(button);
        }
    }

    class Checklist_HomeView
    {
        DisplayList dl;
        string pageTitle;

        public Checklist_HomeView(string pageTitle, Router router)
        {
            this.pageTitle = pageTitle;
            dl = new();

            //Part list
            dl.elements.Add(new DisplayListElement(
                (int index, bool selected, int screenWidth) =>
                {
                    List<string> output = new();
                    output.Add((selected ? "[#20BF6BFF]" : "") + "Show checklist");
                    return output;
                },
                (int button) =>
                {
                    if (button == 2) router.Navigate("checklist");
                }
            ));

            //Help
            dl.elements.Add(new DisplayListElement(
                (int index, bool selected, int screenWidth) =>
                {
                    List<string> output = new();
                    output.Add((selected ? "[#20BF6BFF]" : "") + "Help");
                    return output;
                },
                (int button) =>
                {
                    if (button == 2) router.Navigate("help");
                }
            ));
        }

        public string Display(int screenWidth, int screenHeight)
        {
            return Environment.NewLine + pageTitle + Environment.NewLine + Environment.NewLine + dl.Display(screenWidth, screenHeight - 5);
        }

        public void ButtonProcessor(int button)
        {
            dl.ButtonProcessor(button);
        }
    }

    class Checklist_ChecklistView
    {
        DisplayList dl = new();
        string pageTitle;
        List<ChecklistItem> items = new();

        public Checklist_ChecklistView(string pageTitle)
        {
            this.pageTitle = pageTitle;
            List<string> file = new();
            bool readSuccessful = ReadFile(KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/checklist.txt", out file);

            if (readSuccessful)
            {
                foreach (string item in file) items.Add(new ChecklistItem(item));

                try
                {
                    ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/save.txt");
                    string loadedCompletedData = node.GetValue("completedData");
                    if (loadedCompletedData.Length == items.Count)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            if (loadedCompletedData[i] == 'X')
                            {
                                items[i].completed = true;
                            }
                        }
                    }
                }
                catch {}
            }
        }

        void UpdateItem(int index, bool completed)
        {
            if (index < items.Count)
            {
                items[index].completed = completed;
            }

            ConfigNode node = new();
            string completedData = "";
            foreach (ChecklistItem item in items)
            {
                if (item.completed) completedData += "X";
                else completedData += "O";
            }
            node.AddValue("completedData", completedData);
            node.Save(KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/save.txt");
        }
        static List<string> SplitStringIntoChunks(string input, int chunkSize)
        {
            List<string> chunks = new();

            for (int i = 0; i < input.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, input.Length - i);
                chunks.Add(input.Substring(i, length));
            }

            return chunks;
        }

        static bool ReadFile(string filePath, out List<string> lines)
        {
            lines = new List<string>();

            try
            {
                // Open the file using a StreamReader
                using (StreamReader reader = new StreamReader(filePath))
                {
                    // Read the file line by line
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        lines.Add(line);
                    }
                }

                // Reading was successful
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Display(int screenWidth, int screenHeight)
        {
            dl.elements.Clear();

            int counter = 0;
            foreach (ChecklistItem item in items)
            {
                int itemIndex = counter;
                dl.elements.Add(new DisplayListElement(
                    (int index, bool selected, int screenWidth) =>
                    {
                        List<string> output = new();
                        List<string> lines = SplitStringIntoChunks("___ " + item.text, screenWidth);
                        if (item.completed)
                        {
                            if (selected)
                            {
                                //Selected, complete
                                output.Add("[#20BF6BFF][[X]" + lines[0].Substring(3));
                                for (int j = 1; j < lines.Count; j++) output.Add("[#20BF6BFF]" + lines[j]);
                            }
                            else
                            {
                                //Unselected, complete
                                output.Add("[[[#20BF6BFF]X[#FFFFFFFF]]" + lines[0].Substring(3));
                                for (int j = 1; j < lines.Count; j++) output.Add(lines[j]);
                            }
                        }
                        else
                        {
                            if (selected)
                            {
                                //Selected, incomplete
                                output.Add("[#20BF6BFF][[ ]" + lines[0].Substring(3));
                                for (int j = 1; j < lines.Count; j++) output.Add("[#20BF6BFF]" + lines[j]);
                            }
                            else
                            {
                                //Unselected, incomplete
                                output.Add("[[ ]" + lines[0].Substring(3));
                                for (int j = 1; j < lines.Count; j++) output.Add(lines[j]);
                            }
                        }
                        return output;
                    },
                    (int button) =>
                    {
                        if (button == 2) UpdateItem(itemIndex, true);
                        else if (button == 3) UpdateItem(itemIndex, false);
                    }
                ));

                counter++;
            }

            if (dl.index >= dl.elements.Count) dl.index = dl.elements.Count - 1;

            return Environment.NewLine + pageTitle + Environment.NewLine + Environment.NewLine + dl.Display(screenWidth, screenHeight - 5);
        }

        public void ButtonProcessor(int button)
        {
            dl.ButtonProcessor(button);
        }
    }

    class Checklist_HelpView
    {
        string pageTitle;

        public Checklist_HelpView(string pageTitle)
        {
            this.pageTitle = pageTitle;
        }

        public string Display(int screenWidth, int screenHeight)
        {
            return Environment.NewLine +
                 pageTitle + Environment.NewLine +
                 "                               Cursor up" + Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 "                             Cursor down" + Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 "                          Select / Check" + Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 "                                 Uncheck" + Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 Environment.NewLine +
                 "                                  Return" + Environment.NewLine +
                 "Version: 1.0.0";
        }

        public void ButtonProcessor(int button)
        {
            
        }
    }
}
