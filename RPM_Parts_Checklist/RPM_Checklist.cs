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

        string selectedFile = "";

        public RPM_Checklist()
        {
            router = new("home");

            Checklist_HomeView homeView = new Checklist_HomeView(pageTitle, router);
            router.pages.Add(new Page("home", homeView.Display, homeView.ButtonProcessor));

            Checklist_HelpView helpView = new Checklist_HelpView(pageTitle);
            router.pages.Add(new Page("help", helpView.Display, helpView.ButtonProcessor));

            Checklist_FileListView fileListView = new Checklist_FileListView(pageTitle, (string fileName) => { selectedFile = fileName; }, router);
            router.pages.Add(new Page("files", fileListView.Display, fileListView.ButtonProcessor));

            Checklist_ChecklistView checklistView = new Checklist_ChecklistView(pageTitle);
            router.pages.Add(new Page("checklist", (int screenWidth, int screenHeight) => checklistView.Display(screenWidth, screenHeight, selectedFile), checklistView.ButtonProcessor));
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
                    return new List<string>() { (selected ? "[#20BF6BFF]" : "") + "Show checklists" };
                },
                (int button) =>
                {
                    if (button == 2) router.Navigate("files");
                }
            ));

            //Help
            dl.elements.Add(new DisplayListElement(
                (int index, bool selected, int screenWidth) =>
                {
                    return new List<string>() { (selected ? "[#20BF6BFF]" : "") + "Help" };
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

    class Checklist_FileListView
    {
        DisplayList dl = new();
        string pageTitle;
        Action<string> SetSelectedFile;
        Router router;

        public Checklist_FileListView(string pageTitle, Action<string> SetSelectedFile, Router router)
        {
            this.pageTitle = pageTitle;
            this.SetSelectedFile = SetSelectedFile;
            this.router = router;
        }

        public string Display(int screenWidth, int screenHeight)
        {
            dl.elements.Clear();
            List<string> files = new();
            string checklistDirectory = KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/Checklists/";
            
            if (Directory.Exists(checklistDirectory))
            {
                // Get all text files in the directory
                string[] txtFiles = Directory.GetFiles(checklistDirectory, "*.txt");

                // Display the list of text files
                foreach (string txtFile in txtFiles)
                {
                    files.Add(txtFile);
                }
            }

            foreach (string file in files)
            {
                dl.elements.Add(new DisplayListElement(
                    (int index, bool selected, int screenWidth) =>
                    {
                        return new List<string>() { (selected ? "[#20BF6BFF]" : "") + Path.GetFileNameWithoutExtension(file) };
                    },
                    (int button) =>
                    {
                        if (button == 2)
                        {
                            SetSelectedFile(file);
                            router.Navigate("checklist");
                        }
                    }
                ));
            }

            if (dl.index >= dl.elements.Count) dl.index = dl.elements.Count - 1;

            return Environment.NewLine + pageTitle + Environment.NewLine + Environment.NewLine + "Select checklist:" + Environment.NewLine + Environment.NewLine + dl.Display(screenWidth, screenHeight - 8);
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
        }

        void UpdateItem(int index, bool completed, string fileName)
        {
            if (index < items.Count)
            {
                items[index].completed = completed;
            }

            ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/save.data");
            string completedData = "";
            foreach (ChecklistItem item in items)
            {
                if (item.completed) completedData += "X";
                else completedData += "O";
            }
            if (node.HasValue("completedData_" + fileName))
            {
                node.SetValue("completedData_" + fileName, completedData);
            }
            else
            {
                node.AddValue("completedData_" + fileName, completedData);
            }
            node.Save(KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/save.data");
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

        public string Display(int screenWidth, int screenHeight, string filePath)
        {
            items = new();
            List<string> file = new();
            bool readSuccessful = false;
            if (filePath != "")
            {
                readSuccessful = ReadFile(filePath, out file);
            }

            if (readSuccessful)
            {
                foreach (string item in file) items.Add(new ChecklistItem(item));
            }

            try
            {
                ConfigNode node = ConfigNode.Load(KSPUtil.ApplicationRootPath + "GameData/RPM_Parts_Checklist/save.data");
                string loadedCompletedData = node.GetValue("completedData_" + Path.GetFileNameWithoutExtension(filePath));
                if (loadedCompletedData.Length == items.Count)
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (loadedCompletedData[i] == 'X')
                        {
                            items[i].completed = true;
                        }
                        else
                        {
                            items[i].completed = false;
                        }
                    }
                }
            }
            catch { }

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
                        if (button == 2) UpdateItem(itemIndex, true, Path.GetFileNameWithoutExtension(filePath));
                        else if (button == 3) UpdateItem(itemIndex, false, Path.GetFileNameWithoutExtension(filePath));
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
                 "Version: 1.1.0";
        }

        public void ButtonProcessor(int button)
        {
            
        }
    }
}
